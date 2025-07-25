using EgsLib.Blueprints;
using EgsLib;

namespace Converter.Core
{
    /// <summary>
    /// Configuration for the mesh to blueprint conversion process
    /// </summary>
    public class ConversionConfiguration
    {
        /// <summary>
        /// Path to the template blueprint file (.epb)
        /// </summary>
        public string TemplateBlueprintPath { get; set; } = string.Empty;

        /// <summary>
        /// Path to the input mesh file (STL, etc.)
        /// </summary>
        public string InputMeshPath { get; set; } = string.Empty;

        /// <summary>
        /// Path for the output blueprint file
        /// </summary>
        public string OutputBlueprintPath { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the generated blueprint
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Block ID to use for filling the hull
        /// </summary>
        public int BlockId { get; set; } = 1; // Default to some basic block

        /// <summary>
        /// Block name for the block map (e.g., "HullFullLarge", "HullFullSmall")
        /// </summary>
        public string BlockName { get; set; } = string.Empty;

        /// <summary>
        /// Resolution for voxelization (higher = more detail, larger blueprint)
        /// </summary>
        public float Resolution { get; set; } = 1.0f;

        /// <summary>
        /// Radius for morphological erosion (makes hull thinner)
        /// </summary>
        public int ErosionRadius { get; set; } = 0;

        /// <summary>
        /// Radius for morphological dilation (makes hull thicker)  
        /// </summary>
        public int DilationRadius { get; set; } = 0;

        /// <summary>
        /// Whether to create a hollow hull (remove interior blocks)
        /// </summary>
        public bool CreateHollowHull { get; set; } = true;

        /// <summary>
        /// Radius for hollowing operation
        /// </summary>
        public int HollowRadius { get; set; } = 1;

        /// <summary>
        /// Maximum size for any dimension of the output blueprint
        /// </summary>
        public int MaxSize { get; set; } = 500;

        /// <summary>
        /// Mirror plane for symmetrical voxelization (None, XY, XZ, YZ)
        /// </summary>
        public MirrorPlane MirrorPlane { get; set; } = MirrorPlane.None;

        /// <summary>
        /// Whether to use mirror-based voxelization (voxelize half and mirror to other half)
        /// </summary>
        public bool UseMirrorVoxelization { get; set; } = false;

        /// <summary>
        /// Pre-transformed triangles to use instead of loading from file (for preview rotations)
        /// </summary>
        public List<Triangle>? TransformedTriangles { get; set; } = null;
    }

    /// <summary>
    /// Predefined mirror planes for symmetrical voxelization
    /// </summary>
    public enum MirrorPlane
    {
        None,  // No mirroring
        XY,    // Mirror across XY plane (Z=0)
        XZ,    // Mirror across XZ plane (Y=0) 
        YZ     // Mirror across YZ plane (X=0)
    }

    /// <summary>
    /// Interface for mesh file converters
    /// </summary>
    public interface IMeshConverter
    {
        /// <summary>
        /// Supported file extensions (e.g., ".stl", ".obj")
        /// </summary>
        string[] SupportedExtensions { get; }

        /// <summary>
        /// Load triangles from the mesh file
        /// </summary>
        /// <param name="filePath">Path to the mesh file</param>
        /// <returns>List of triangles, each containing 3 vertices as Vector3</returns>
        List<Triangle> LoadTriangles(string filePath);
    }

    /// <summary>
    /// Represents a triangle with three vertices
    /// </summary>
    public struct Triangle
    {
        public Vector3<float> V1 { get; set; }
        public Vector3<float> V2 { get; set; }
        public Vector3<float> V3 { get; set; }

        public Triangle(Vector3<float> v1, Vector3<float> v2, Vector3<float> v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
    }

    /// <summary>
    /// Result of the voxelization process
    /// </summary>
    public class VoxelizationResult
    {
        /// <summary>
        /// Set of voxel positions to be filled with blocks
        /// </summary>
        public HashSet<Vector3<int>> Voxels { get; set; } = new();

        /// <summary>
        /// Bounding box of the voxelized result
        /// </summary>
        public BoundingBox Bounds { get; set; }

        /// <summary>
        /// Statistics about the voxelization process
        /// </summary>
        public VoxelizationStats Stats { get; set; } = new();
    }

    /// <summary>
    /// Bounding box for 3D space
    /// </summary>
    public struct BoundingBox
    {
        public Vector3<int> Min { get; set; }
        public Vector3<int> Max { get; set; }

        public Vector3<int> Size => new(Max.X - Min.X + 1, Max.Y - Min.Y + 1, Max.Z - Min.Z + 1);
    }

    /// <summary>
    /// Statistics from the voxelization process
    /// </summary>
    public class VoxelizationStats
    {
        public int InputTriangles { get; set; }
        public int GeneratedVoxels { get; set; }
        public int FinalVoxels { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public Vector3<int> FinalSize { get; set; }
    }

    /// <summary>
    /// Main converter class that orchestrates the mesh to blueprint conversion
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Convert a mesh file to a blueprint using the specified configuration
        /// </summary>
        /// <param name="config">Conversion configuration</param>
        /// <returns>Path to the generated blueprint file</returns>
        Task<string> ConvertAsync(ConversionConfiguration config, IProgress<string>? progress = null);
    }

    /// <summary>
    /// Registry for mesh converters
    /// </summary>
    public static class ConverterRegistry
    {
        private static readonly Dictionary<string, IMeshConverter> _converters = new();

        /// <summary>
        /// Register a mesh converter for specific file extensions
        /// </summary>
        public static void RegisterConverter(IMeshConverter converter)
        {
            foreach (var extension in converter.SupportedExtensions)
            {
                _converters[extension.ToLowerInvariant()] = converter;
            }
        }

        /// <summary>
        /// Get a converter for the specified file extension
        /// </summary>
        public static IMeshConverter? GetConverter(string fileExtension)
        {
            return _converters.TryGetValue(fileExtension.ToLowerInvariant(), out var converter) 
                ? converter 
                : null;
        }

        /// <summary>
        /// Get all supported file extensions
        /// </summary>
        public static string[] GetSupportedExtensions()
        {
            return _converters.Keys.ToArray();
        }
    }
}
