using Converter.Core;
using EgsLib;
using EgsLib.Blueprints;
using System.IO;

namespace Converter.GUI
{
    public partial class Form1 : Form
    {
        private readonly MeshToBlueprintConverter _converter;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isConverting = false;
        private bool _isValidTemplate = false;
        
        // Mirror plane settings from preview window
        private MirrorPlane _selectedMirrorPlane = MirrorPlane.None;
        private bool _useMirrorVoxelization = false;
        private List<Converter.Core.Triangle>? _transformedTriangles = null;

        // Block type data structures for different blueprint types
        private readonly Dictionary<string, int> _smallVesselBlocks = new()
        {
            // Small Vessel / Hover Vessel blocks
            { "Steel Block (Small)", 381 }
        };

        private readonly Dictionary<string, int> _largeVesselBlocks = new()
        {
            // Capital Vessel / Base blocks
            { "Steel Block (Large)", 403 }
        };

        // Block name mappings for the block map (what Empyrion calls these blocks internally)
        private readonly Dictionary<int, string> _blockIdToName = new()
        {
            // Small vessel blocks use HullFullSmall
            { 381, "HullFullSmall" },
            
            // Large vessel blocks use HullFullLarge  
            { 403, "HullFullLarge" }
        };

        private Dictionary<string, int> _currentBlockTypes = new();

        public string GetBlockNameForId(int blockId)
        {
            return _blockIdToName.TryGetValue(blockId, out var name) ? name : "UnknownBlock";
        }

        public Form1()
        {
            InitializeComponent();
            _converter = new MeshToBlueprintConverter();
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Initially populate with large vessel blocks as default
            _currentBlockTypes = new Dictionary<string, int>(_largeVesselBlocks);
            UpdateBlockTypeDropdown();

            // Initialize blueprint info display
            labelBlueprintTypeValue.Text = "Not selected";
            labelBlueprintNameValue.Text = "Not selected";
            labelBlueprintWarning.Visible = false;

            // Set up file dialog filters with supported formats
            var supportedFormats = ConverterRegistry.GetSupportedExtensions();
            if (supportedFormats.Length > 0)
            {
                var filterParts = new List<string>();
                var allExtensions = new List<string>();

                foreach (var ext in supportedFormats)
                {
                    var extUpper = ext.TrimStart('.').ToUpperInvariant();
                    filterParts.Add($"{extUpper} Files (*{ext})|*{ext}");
                    allExtensions.Add($"*{ext}");
                }

                // Add "All Supported" filter
                var allSupportedFilter = $"All Supported Files ({string.Join(";", allExtensions)})|{string.Join(";", allExtensions)}";
                var fullFilter = allSupportedFilter + "|" + string.Join("|", filterParts) + "|All Files (*.*)|*.*";

                openFileDialogMesh.Filter = fullFilter;
            }

            // Enable/disable hollow radius based on checkbox
            UpdateHollowControls();

            // Initial button state
            UpdateConvertButtonState();
        }

        private void UpdateBlockTypeDropdown()
        {
            var selectedValue = comboBoxBlockType.SelectedItem?.ToString();

            comboBoxBlockType.DataSource = _currentBlockTypes.Keys.ToList();

            // Try to preserve selection if the block exists in the new list
            if (!string.IsNullOrEmpty(selectedValue) && _currentBlockTypes.ContainsKey(selectedValue))
            {
                comboBoxBlockType.SelectedItem = selectedValue;
            }
            else if (comboBoxBlockType.Items.Count > 0)
            {
                comboBoxBlockType.SelectedIndex = 0;
            }
        }

        #region File Browse Events

        private void buttonBrowseTemplate_Click(object sender, EventArgs e)
        {
            if (openFileDialogTemplate.ShowDialog() == DialogResult.OK)
            {
                textBoxTemplatePath.Text = openFileDialogTemplate.FileName;

                // Auto-generate output directory if not set
                if (string.IsNullOrWhiteSpace(textBoxOutputPath.Text))
                {
                    var templateDir = Path.GetDirectoryName(openFileDialogTemplate.FileName);
                    textBoxOutputPath.Text = templateDir ?? "";
                }

                // Load blueprint and update block types based on blueprint type
                UpdateBlockTypesFromTemplate(openFileDialogTemplate.FileName);
            }
        }

        private void UpdateBlockTypesFromTemplate(string templatePath)
        {
            try
            {
                var blueprint = new Blueprint(templatePath);
                var blueprintType = blueprint.Header.BlueprintType;
                var displayName = blueprint.Header.DisplayName ?? "Unnamed Blueprint";

                // Update blueprint information display
                labelBlueprintTypeValue.Text = blueprintType.ToString();
                labelBlueprintNameValue.Text = displayName;

                // Validate template - should only contain core blocks
                _isValidTemplate = ValidateTemplate(blueprint);
                labelBlueprintWarning.Visible = !_isValidTemplate;

                // Update block types based on blueprint type
                switch (blueprintType)
                {
                    case BlueprintType.SmallVessel:
                    case BlueprintType.HoverVessel:
                        _currentBlockTypes = new Dictionary<string, int>(_smallVesselBlocks);
                        UpdateStatus($"Loaded {blueprintType} template - using small vessel blocks");
                        break;

                    case BlueprintType.Base:
                    case BlueprintType.CapitalVessel:
                        _currentBlockTypes = new Dictionary<string, int>(_largeVesselBlocks);
                        UpdateStatus($"Loaded {blueprintType} template - using large vessel blocks");
                        break;

                    default:
                        // Default to large vessel blocks for unknown types
                        _currentBlockTypes = new Dictionary<string, int>(_largeVesselBlocks);
                        UpdateStatus($"Loaded {blueprintType} template - using default large vessel blocks");
                        break;
                }

                UpdateBlockTypeDropdown();
                UpdateConvertButtonState();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Warning: Could not determine blueprint type ({ex.Message}) - using default blocks");

                // Reset blueprint info display
                labelBlueprintTypeValue.Text = "Error loading";
                labelBlueprintNameValue.Text = "Error loading";
                labelBlueprintWarning.Visible = true;
                _isValidTemplate = false;

                _currentBlockTypes = new Dictionary<string, int>(_largeVesselBlocks);
                UpdateBlockTypeDropdown();
                UpdateConvertButtonState();
            }
        }

        private bool ValidateTemplate(Blueprint blueprint)
        {
            try
            {
                // Iterate through all positions in the blueprint to check blocks
                int totalBlocks = 0;
                int nonCoreBlocks = 0;
                var coreBlockId = GetCoreBlockId(blueprint.Header.BlueprintType);

                for (int x = 0; x < blueprint.BlockData.Size.X; x++)
                {
                    for (int y = 0; y < blueprint.BlockData.Size.Y; y++)
                    {
                        for (int z = 0; z < blueprint.BlockData.Size.Z; z++)
                        {
                            var position = new Vector3<int>(x, y, z);
                            var block = blueprint.BlockData.GetBlock(position);

                            // Skip empty blocks (BlockId == 0)
                            if (block.BlockId != 0)
                            {
                                totalBlocks++;

                                // Check if this is not a core block
                                if (block.BlockId != coreBlockId)
                                {
                                    UpdateStatus($"Found non-core block with id: {block.BlockId}");
                                    nonCoreBlocks++;
                                }
                            }
                        }
                    }
                }

                if (nonCoreBlocks > 0)
                {
                    UpdateStatus($"Template validation failed: Found {nonCoreBlocks} non-core blocks (total: {totalBlocks})");
                    return false;
                }

                if (totalBlocks == 0)
                {
                    UpdateStatus("Template validation failed: No blocks found in template");
                    return false;
                }

                UpdateStatus($"Template validation passed: Contains only core blocks ({totalBlocks} total)");
                return true;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Template validation error: {ex.Message}");
                return false;
            }
        }

        private int GetCoreBlockId(BlueprintType blueprintType)
        {
            return blueprintType switch
            {
                BlueprintType.Base => 558,
                BlueprintType.CapitalVessel => 558,
                BlueprintType.SmallVessel => 558,
                BlueprintType.HoverVessel => 558,
                _ => 558
            };
        }

        private void UpdateConvertButtonState()
        {
            buttonConvert.Enabled = _isValidTemplate && !_isConverting;
        }

        private async Task VerifyGeneratedBlueprint(string blueprintPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    var verifyBlueprint = new Blueprint(blueprintPath);
                    
                    // Check basic blueprint info
                    UpdateStatus($"Verification - Blueprint Type: {verifyBlueprint.Header.BlueprintType}");
                    UpdateStatus($"Verification - Blueprint Name: {verifyBlueprint.Header.DisplayName ?? "Unnamed"}");
                    UpdateStatus($"Verification - Blueprint Size: {verifyBlueprint.BlockData.Size.X}x{verifyBlueprint.BlockData.Size.Y}x{verifyBlueprint.BlockData.Size.Z}");
                    
                    // Count blocks by type
                    var blockCounts = new Dictionary<int, int>();
                    int totalBlocks = 0;
                    
                    for (int x = 0; x < verifyBlueprint.BlockData.Size.X; x++)
                    {
                        for (int y = 0; y < verifyBlueprint.BlockData.Size.Y; y++)
                        {
                            for (int z = 0; z < verifyBlueprint.BlockData.Size.Z; z++)
                            {
                                var position = new Vector3<int>(x, y, z);
                                var block = verifyBlueprint.BlockData.GetBlock(position);
                                
                                if (block.BlockId != 0)
                                {
                                    totalBlocks++;
                                    if (blockCounts.ContainsKey(block.BlockId))
                                        blockCounts[block.BlockId]++;
                                    else
                                        blockCounts[block.BlockId] = 1;
                                }
                            }
                        }
                    }
                    
                    UpdateStatus($"Verification - Total blocks found: {totalBlocks}");
                    
                    if (totalBlocks == 0)
                    {
                        UpdateStatus("⚠️ WARNING: No blocks found in generated blueprint!");
                    }
                    else
                    {
                        UpdateStatus("Verification - Block breakdown:");
                        foreach (var kvp in blockCounts.OrderBy(x => x.Key))
                        {
                            UpdateStatus($"  Block ID {kvp.Key}: {kvp.Value} blocks");
                        }
                        
                        // Check for core block
                        var expectedCoreId = GetCoreBlockId(verifyBlueprint.Header.BlueprintType);
                        if (blockCounts.ContainsKey(expectedCoreId))
                        {
                            UpdateStatus($"✓ Core block found (ID {expectedCoreId}): {blockCounts[expectedCoreId]} blocks");
                        }
                        else
                        {
                            UpdateStatus($"⚠️ WARNING: Expected core block (ID {expectedCoreId}) not found!");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                UpdateStatus($"Verification failed: {ex.Message}");
            }
        }

        private void buttonBrowseMesh_Click(object sender, EventArgs e)
        {
            if (openFileDialogMesh.ShowDialog() == DialogResult.OK)
            {
                textBoxMeshPath.Text = openFileDialogMesh.FileName;

                // Auto-generate display name if not set
                if (string.IsNullOrWhiteSpace(textBoxDisplayName.Text))
                {
                    var meshName = Path.GetFileNameWithoutExtension(openFileDialogMesh.FileName);
                    textBoxDisplayName.Text = meshName;
                }
            }
        }

        private void buttonPreviewMesh_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxMeshPath.Text))
            {
                MessageBox.Show("Please select a mesh file first.", "No Mesh File", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(textBoxMeshPath.Text))
            {
                MessageBox.Show("Selected mesh file does not exist.", "File Not Found", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var previewWindow = new PreviewWindow();
                previewWindow.LoadMesh(textBoxMeshPath.Text);
                
                if (previewWindow.ShowDialog() == true)
                {
                    // User selected a mirror plane and clicked Apply
                    _selectedMirrorPlane = previewWindow.SelectedMirrorPlane;
                    _useMirrorVoxelization = previewWindow.UseMirrorVoxelization;
                    _transformedTriangles = previewWindow.GetTransformedTriangles();
                    
                    var rotationInfo = "";
                    if (previewWindow.RotationX != 0 || previewWindow.RotationY != 0 || previewWindow.RotationZ != 0)
                    {
                        rotationInfo = $" with rotation X:{previewWindow.RotationX}° Y:{previewWindow.RotationY}° Z:{previewWindow.RotationZ}°";
                    }
                    
                    UpdateStatus($"Preview applied - Mirror plane: {_selectedMirrorPlane}{rotationInfo}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening preview: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonBrowseOutput_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select output directory for the converted blueprint",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxOutputPath.Text = folderDialog.SelectedPath;
            }
        }

        #endregion

        #region Parameter Events

        private void checkBoxHollow_CheckedChanged(object sender, EventArgs e)
        {
            UpdateHollowControls();
        }

        private void UpdateHollowControls()
        {
            labelHollowRadius.Enabled = checkBoxHollow.Checked;
            numericUpDownHollowRadius.Enabled = checkBoxHollow.Checked;
        }

        #endregion

        #region Conversion Events

        private async void buttonConvert_Click(object sender, EventArgs e)
        {
            if (_isConverting)
                return;

            try
            {
                // Validate inputs
                if (!ValidateInputs())
                    return;

                // Start conversion
                await StartConversionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Conversion failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Conversion failed: {ex.Message}");
            }
            finally
            {
                SetConvertingState(false);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (_isConverting && _cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                UpdateStatus("Conversion cancelled by user");
            }
        }

        #endregion

        #region Conversion Logic

        private bool ValidateInputs()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(textBoxTemplatePath.Text))
                errors.Add("Please select a template blueprint file");
            else if (!File.Exists(textBoxTemplatePath.Text))
                errors.Add("Template blueprint file does not exist");

            if (string.IsNullOrWhiteSpace(textBoxMeshPath.Text))
                errors.Add("Please select a mesh file");
            else if (!File.Exists(textBoxMeshPath.Text))
                errors.Add("Mesh file does not exist");

            if (string.IsNullOrWhiteSpace(textBoxOutputPath.Text))
                errors.Add("Please specify an output directory");
            else if (!Directory.Exists(textBoxOutputPath.Text))
                errors.Add("Output directory does not exist");

            if (errors.Any())
            {
                MessageBox.Show("Please fix the following errors:\n\n" + string.Join("\n", errors),
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private async Task StartConversionAsync()
        {
            SetConvertingState(true);
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                // Create configuration from UI
                var config = CreateConfigurationFromUI();

                // Create progress reporter
                var progress = new Progress<string>(UpdateStatus);

                // Start conversion
                UpdateStatus("Starting conversion...");
                var outputPath = await _converter.ConvertAsync(config, progress);

                // Verify the generated blueprint
                UpdateStatus("Verifying generated blueprint...");
                await VerifyGeneratedBlueprint(outputPath);

                // Success
                UpdateStatus($"Conversion completed successfully! Output saved to: {outputPath}");
                MessageBox.Show($"Conversion completed successfully!\n\nOutput saved to:\n{outputPath}",
                    "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                UpdateStatus("Conversion was cancelled");
            }
            catch (NotImplementedException ex)
            {
                UpdateStatus($"Feature not yet implemented: {ex.Message}");
                MessageBox.Show($"Feature not yet implemented:\n\n{ex.Message}\n\nThis will be completed in a future update.",
                    "Feature Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Conversion failed: {ex.Message}");
                throw; // Re-throw to be handled by caller
            }
        }

        private ConversionConfiguration CreateConfigurationFromUI()
        {
            // Get selected block ID and name
            var selectedBlockType = comboBoxBlockType.SelectedItem?.ToString() ?? _currentBlockTypes.Keys.First();
            var blockId = _currentBlockTypes[selectedBlockType];
            var blockName = GetBlockNameForId(blockId);

            // Generate output filename from display name
            var displayName = textBoxDisplayName.Text.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = "ConvertedMesh";
            }

            // Clean the display name for use as filename
            var cleanFileName = string.Join("_", displayName.Split(Path.GetInvalidFileNameChars()));
            var outputFilePath = Path.Combine(textBoxOutputPath.Text.Trim(), $"{cleanFileName}.epb");

            return new ConversionConfiguration
            {
                TemplateBlueprintPath = textBoxTemplatePath.Text.Trim(),
                InputMeshPath = textBoxMeshPath.Text.Trim(),
                OutputBlueprintPath = outputFilePath,
                DisplayName = displayName,
                BlockId = blockId,
                BlockName = blockName,
                Resolution = (float)numericUpDownResolution.Value,
                ErosionRadius = (int)numericUpDownErosionRadius.Value,
                DilationRadius = (int)numericUpDownDilationRadius.Value,
                CreateHollowHull = checkBoxHollow.Checked,
                HollowRadius = (int)numericUpDownHollowRadius.Value,
                MaxSize = (int)numericUpDownMaxSize.Value,
                MirrorPlane = _selectedMirrorPlane,
                UseMirrorVoxelization = _useMirrorVoxelization,
                TransformedTriangles = _transformedTriangles
            };
        }

        private void SetConvertingState(bool isConverting)
        {
            _isConverting = isConverting;

            // Disable/enable controls
            groupBoxFiles.Enabled = !isConverting;
            groupBoxBlueprintInfo.Enabled = !isConverting;
            groupBoxParameters.Enabled = !isConverting;
            buttonConvert.Enabled = !isConverting && _isValidTemplate;
            buttonCancel.Enabled = isConverting;

            // Show/hide progress bar
            progressBar.Visible = isConverting;
            if (isConverting)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
            }

            // Update cursor
            Cursor = isConverting ? Cursors.WaitCursor : Cursors.Default;
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), message);
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var statusMessage = $"[{timestamp}] {message}";

            textBoxStatus.AppendText(statusMessage + Environment.NewLine);
            textBoxStatus.SelectionStart = textBoxStatus.Text.Length;
            textBoxStatus.ScrollToCaret();

            Application.DoEvents(); // Allow UI to update
        }

        #endregion

        #region Form Events

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isConverting)
            {
                var result = MessageBox.Show("A conversion is currently in progress. Are you sure you want to exit?",
                    "Conversion In Progress", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _cancellationTokenSource?.Cancel();
            }

            base.OnFormClosing(e);
        }



        #endregion
    }
}
