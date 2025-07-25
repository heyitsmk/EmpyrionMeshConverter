# Empyrion Mesh Converter

A tool for converting 3D mesh files into Empyrion Galactic Survival blueprint files (.epb).

## 🚀 Features

- **3D Mesh Import**: Import STL files (both ASCII and binary formats)
- **Intelligent Voxelization**: Convert smooth 3D meshes into block-based structures
- **Configurable Quality**: Adjust resolution for detail vs. size trade-offs
- **Morphological Operations**: Fine-tune your hull with erosion and dilation
- **Hollow Hull Creation**: Generate hollow structures
- **Mirror Symmetry**: Create perfectly symmetrical builds using mirror planes
- **Progress Tracking**: Real-time feedback during conversion process
- **Windows GUI**: User-friendly interface with preview capabilities

## 📋 Requirements

- **Operating System**: Windows 10/11
- **.NET Runtime**: .NET 8.0 or later
- **Game**: Empyrion Galactic Survival (for using generated blueprints)
- **Dependencies**: EgsLib (heyitsmk fork)

## 🛠️ Installation

### Option 1: Download Release (Recommended)
1. Go to the [Releases](../../releases) page
2. Download the latest version
3. Extract the ZIP file to your desired location
4. Run `Converter.GUI.exe`

### Option 2: Build from Source
1. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/EmpyrionMeshConverter.git
   ```
2. Ensure you have .NET 8.0 SDK installed
3. Open `EmpyrionMeshConverter.sln` in Visual Studio 2022
4. Fix the project reference for the EgsLib fork
5. Build the solution (Build → Build Solution)
6. Run the GUI project

## 🎯 Usage

### Basic Workflow

1. **Prepare Your Files**:
   - Get a 3D mesh file in STL format
   - Create a blueprint in EGS for your desired vessel type. The blueprint should only be a single core block.
   - Blueprints are usually saved at: C:\Program Files (x86)\Steam\steamapps\common\Empyrion - Galactic Survival\Saves\Blueprints\STEAMID

2. **Launch the Application**:
   - Run `Converter.GUI.exe`
   - The GUI will open with conversion options

3. **Configure Conversion**:
   - **Input Mesh**: Select your STL file
   - **Template Blueprint**: Choose your .epb template file
   - **Output Path**: Specify where to save the generated blueprint.
   - **Display Name**: Set the name for your blueprint

4. **Adjust Settings**:
   - **Rotation/Symmetry**: Click on preview mesh to configure rotation and symmetry options.
   - **Resolution**: (I don't think this does anything leave it at 1.0)
   - **Block Type**: Choose the hull block to use. Right now you only have one choice.
   - **Max Size**: Set the target size for the largest dimension of the output bp
   - **Hollow Options**: Create interior space if desired
   - **Morphological Options**: Modify the mesh to improve voxelization results in some cases.

5. **Process**:
   - Click "Convert" to start the voxelization
   - Monitor progress in real-time

6. **Test**:
   - Load the blueprint in EGS to test it out!

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| **Resolution** | Voxel density (blocks per unit) | 1.0 |
| **Block ID** | Empyrion block type ID | 1 |
| **Block Name** | Block name for blueprint | "" |
| **Max Size** | Maximum dimension target| 500 |
| **Create Hollow Hull** | Remove interior blocks | true |
| **Hollow Radius** | Thickness of hull walls | 1 |
| **Erosion Radius** | Shrink the mesh | 0 |
| **Dilation Radius** | Expand the mesh | 0 |
| **Rotation** | Rotate the source mesh | None |
| **Mirror Plane** | Symmetry axis (XY/XZ/YZ) | None |

## 🔧 Technical Details

### Architecture

The project consists of two main components:

- **Converter.Core**: Core conversion library containing:
  - `MeshToBlueprintConverter`: Main conversion orchestrator
  - `VoxelizationEngine`: Converts triangular meshes to voxel grids
  - `StlConverter`: Handles STL file parsing
  - `ConversionConfiguration`: Settings and parameters

- **Converter.GUI**: Windows application providing:
  - User-friendly interface for configuration
  - Real-time preview capabilities
  - Progress tracking and error handling

### Supported File Formats

| Format | Input | Output | Notes |
|--------|-------|---------|-------|
| STL | ✅ | ❌ | ASCII and Binary variants |
| EPB | ❌ | ✅ | Empyrion blueprint format |

### Dependencies

- **EgsLib**: Empyrion blueprint manipulation library
- **.NET 8.0**: Modern .NET runtime
- **Windows Forms**: GUI framework
- **WPF**: Enhanced UI components

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Report Issues**: Found a bug? [Open an issue](../../issues)
2. **Feature Requests**: Have an idea? Let us know!
3. **Code Contributions**:
   - Fork the repository
   - Create a feature branch
   - Make your changes
   - Submit a pull request

### Development Setup

1. Install Visual Studio 2022 with .NET 8.0 support
2. Clone the repository with submodules
3. Open `EmpyrionMeshConverter.sln`
4. Build and run the solution

## 📝 File Format Support

### Current Support
- **STL Files**: Both ASCII and binary formats
- **Template Blueprints**: Any valid Empyrion .epb file

### Planned Support
- OBJ files
- PLY files
- Direct mesh import from modeling software

## ⚠️ Known Limitations

- Currently only supports STL input format
- Windows-only GUI application
- Requires template blueprint for proper game integration
- Large meshes may require significant processing time

## 🐛 Troubleshooting

### Common Issues

**"Template blueprint not found"**
- Ensure the template file path is correct
- Verify the .epb file is valid and accessible

**"Unsupported file format"**
- Currently only STL files are supported
- Check file extension is .stl

**"Output blueprint too large"**
- Reduce resolution setting
- Decrease max size limit
- Consider simplifying the input mesh

**Application won't start**
- Ensure .NET 8.0 runtime is installed
- Check Windows compatibility
- Verify all files were extracted properly

## 🙏 Acknowledgments

- **Empyrion Galactic Survival** 
- **EgsLib** 
- **Anvil Community**

## 📬 Support

- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Wiki**: [Project Wiki](../../wiki)

---

**Happy Building!** 🚀 Transform your 3D creations into Empyrion masterpieces! 