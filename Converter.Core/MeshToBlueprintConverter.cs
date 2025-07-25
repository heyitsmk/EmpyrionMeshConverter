using EgsLib;
using EgsLib.Blueprints;

namespace Converter.Core
{
    /// <summary>
    /// Main implementation of the mesh to blueprint converter
    /// </summary>
    public class MeshToBlueprintConverter : IConverter
    {
        private readonly VoxelizationEngine _voxelizationEngine;

        public MeshToBlueprintConverter()
        {
            _voxelizationEngine = new VoxelizationEngine();

            // Register the STL converter
            ConverterRegistry.RegisterConverter(new StlConverter());
        }

        public async Task<string> ConvertAsync(ConversionConfiguration config, IProgress<string>? progress = null)
        {
            progress?.Report("Starting mesh to blueprint conversion...");

            // Validate configuration
            ValidateConfiguration(config);

            // Step 1: Load the template blueprint
            progress?.Report("Loading template blueprint...");
            var templateBlueprint = LoadTemplateBlueprint(config.TemplateBlueprintPath);

            // Step 2: Load mesh triangles (or use pre-transformed ones)
            List<Triangle> triangles;
            if (config.TransformedTriangles != null && config.TransformedTriangles.Any())
            {
                progress?.Report("Using pre-transformed mesh data...");
                triangles = config.TransformedTriangles;
            }
            else
            {
                progress?.Report("Loading mesh file...");
                triangles = LoadMeshTriangles(config.InputMeshPath);
            }

            // Step 3: Voxelize the mesh
            progress?.Report("Starting voxelization process...");
            var voxelResult = await Task.Run(() =>
                _voxelizationEngine.Voxelize(triangles, config, progress));

            // Step 4: Create new blueprint with voxelized blocks
            progress?.Report("Creating blueprint from voxels...");
            var outputBlueprint = CreateBlueprintFromVoxels(templateBlueprint, voxelResult, config, progress);

            // Step 5: Save the output blueprint
            progress?.Report("Saving output blueprint...");
            SaveBlueprint(outputBlueprint, config.OutputBlueprintPath);

            progress?.Report($"Conversion complete! Generated {voxelResult.Stats.FinalVoxels} blocks.");
            return config.OutputBlueprintPath;
        }

        private void ValidateConfiguration(ConversionConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config.TemplateBlueprintPath))
                throw new ArgumentException("Template blueprint path is required", nameof(config));

            if (!File.Exists(config.TemplateBlueprintPath))
                throw new FileNotFoundException($"Template blueprint not found: {config.TemplateBlueprintPath}");

            if (string.IsNullOrWhiteSpace(config.InputMeshPath))
                throw new ArgumentException("Input mesh path is required", nameof(config));

            if (!File.Exists(config.InputMeshPath))
                throw new FileNotFoundException($"Input mesh file not found: {config.InputMeshPath}");

            if (string.IsNullOrWhiteSpace(config.OutputBlueprintPath))
                throw new ArgumentException("Output blueprint path is required", nameof(config));

            var outputDir = Path.GetDirectoryName(config.OutputBlueprintPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            if (config.Resolution <= 0)
                throw new ArgumentException("Resolution must be greater than 0", nameof(config));

            if (config.MaxSize <= 0)
                throw new ArgumentException("Max size must be greater than 0", nameof(config));
        }

        private Blueprint LoadTemplateBlueprint(string templatePath)
        {
            try
            {
                return new Blueprint(templatePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load template blueprint: {ex.Message}", ex);
            }
        }

        private List<Triangle> LoadMeshTriangles(string meshPath)
        {
            var extension = Path.GetExtension(meshPath);
            var converter = ConverterRegistry.GetConverter(extension);

            if (converter == null)
            {
                var supportedExtensions = string.Join(", ", ConverterRegistry.GetSupportedExtensions());
                throw new NotSupportedException($"Unsupported file format: {extension}. Supported formats: {supportedExtensions}");
            }

            try
            {
                return converter.LoadTriangles(meshPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load mesh file: {ex.Message}", ex);
            }
        }

        private Blueprint CreateBlueprintFromVoxels(Blueprint blueprint, VoxelizationResult voxelResult, ConversionConfiguration config, IProgress<string>? progress = null)
        {
            // Calculate the required blueprint size to accommodate centered mesh
            var meshSize = voxelResult.Bounds.Size;
            var blueprintSize = new Vector3<int>(
                meshSize.X + 2, // Add 1 block padding on each side (2 total per dimension)
                meshSize.Y + 2,
                meshSize.Z + 2
            );

            blueprint.SetDisplayName(config.DisplayName);

            // Calculate the padding offset (1 block padding on each side)
            var paddingOffset = new Vector3<int>(1, 1, 1);

            // Calculate the center of the blueprint for core placement
            var blueprintCenter = new Vector3<int>(
                blueprintSize.X / 2,
                blueprintSize.Y / 2,
                blueprintSize.Z / 2
            );

            blueprint.RemoveBlock(new Vector3<int>(0, 0, 0));

            // Ensure the hull block type is registered in the block map
            EnsureBlockInMap(blueprint, config.BlockId, config.BlockName);

            // Add all voxels as blocks, offset by padding
            // Voxels are already translated to start at (0,0,0), so we just add the padding offset
            foreach (var voxel in voxelResult.Voxels)
            {
                var offsetPosition = new Vector3<int>(
                    voxel.X + paddingOffset.X,
                    voxel.Y + paddingOffset.Y,
                    voxel.Z + paddingOffset.Z
                );

                // Ensure the position is within blueprint bounds
                if (IsWithinBounds(offsetPosition, blueprintSize))
                {
                    blueprint.AddBlock(offsetPosition, config.BlockId);
                }
                else
                {
                    progress?.Report("Tried to place block outside blueprint bounds");
                }
            }

            // Add core block at the center of the blueprint
            var coreBlockId = GetCoreBlockId(blueprint.Header.BlueprintType);
            EnsureBlockInMap(blueprint, coreBlockId, "Core");
            blueprint.AddBlock(blueprintCenter, coreBlockId);

            return blueprint;
        }

        private bool IsWithinBounds(Vector3<int> position, Vector3<int> size)
        {
            return position.X >= 0 && position.X < size.X &&
                   position.Y >= 0 && position.Y < size.Y &&
                   position.Z >= 0 && position.Z < size.Z;
        }

        private Blueprint CloneTemplateBlueprint(Blueprint template, Vector3<int> newSize)
        {
            // For now, we'll create a new blueprint by copying the template file to a temp location
            // and then loading it. This preserves all the template's properties and structure.

            var tempPath = Path.GetTempFileName() + ".epb";
            File.Copy(template.FilePath, tempPath, true);

            var newBlueprint = new Blueprint(tempPath);

            // Clear existing blocks (we want to start with just the core)
            ClearAllBlocks(newBlueprint);

            // Update the blueprint size to match our voxel data
            if (newBlueprint.Header.Size.HasValue)
            {
                newBlueprint.Header.Size = newSize;
            }

            // Clean up temp file
            File.Delete(tempPath);

            return newBlueprint;
        }

        private void ClearAllBlocks(Blueprint blueprint)
        {
            // Remove ALL blocks including core - we'll add the core back at the center later
            var blocksToRemove = new List<Vector3<int>>();

            for (int x = 0; x < blueprint.BlockData.Size.X; x++)
            {
                for (int y = 0; y < blueprint.BlockData.Size.Y; y++)
                {
                    for (int z = 0; z < blueprint.BlockData.Size.Z; z++)
                    {
                        var position = new Vector3<int>(x, y, z);
                        var block = blueprint.BlockData.GetBlock(position);

                        // Remove all blocks (including core)
                        if (block.BlockId != 0)
                        {
                            blocksToRemove.Add(position);
                        }
                    }
                }
            }

            foreach (var position in blocksToRemove)
            {
                blueprint.RemoveBlock(position);
            }
        }

        private void EnsureBlockInMap(Blueprint blueprint, int blockId, string blockName)
        {
            // Check if the block is already in the block map
            if (!blueprint.Header.BlockMap.ContainsKey(blockName))
            {
                // Add the block to the block map
                blueprint.Header.AddToBlockMap(blockName, blockId);
            }
        }

        private int GetCoreBlockId(BlueprintType blueprintType)
        {
            // Return the universal core block ID for Empyrion
            // All blueprint types use the same core block ID (558)
            return 558;
        }

        private void SetBlueprintDisplayName(Blueprint blueprint, string displayName)
        {
            // Use the proper SetDisplayName method from Blueprint class
            blueprint.SetDisplayName(displayName);
        }

        private void SaveBlueprint(Blueprint blueprint, string outputPath)
        {
            try
            {
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Use the proper SaveTo method from Blueprint class
                blueprint.SaveTo(outputPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save blueprint: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Exception thrown when blueprint operations encounter errors
    /// </summary>
    public class BlueprintException : Exception
    {
        public BlueprintException(string message) : base(message) { }
        public BlueprintException(string message, Exception innerException) : base(message, innerException) { }
    }
}