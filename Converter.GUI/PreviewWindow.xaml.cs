using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Converter.Core;

namespace Converter.GUI
{
    public partial class PreviewWindow : Window
    {
        private List<Converter.Core.Triangle> _originalTriangles; // Original unrotated mesh
        private List<Converter.Core.Triangle> _meshTriangles;     // Currently rotated mesh
        private ModelVisual3D? _meshVisual;
        private ModelVisual3D? _mirrorPlaneVisual;
        private ModelVisual3D? _coordinateAxesVisual;
        
        public MirrorPlane SelectedMirrorPlane { get; private set; } = MirrorPlane.None;
        public bool UseMirrorVoxelization { get; private set; } = false;
        
        // Rotation tracking
        private float _rotationX = 0; // Total rotation around X axis in degrees
        private float _rotationY = 0; // Total rotation around Y axis in degrees  
        private float _rotationZ = 0; // Total rotation around Z axis in degrees
        
        public float RotationX => _rotationX;
        public float RotationY => _rotationY;
        public float RotationZ => _rotationZ;

        public PreviewWindow()
        {
            InitializeComponent();
            _originalTriangles = new List<Converter.Core.Triangle>();
            _meshTriangles = new List<Converter.Core.Triangle>();
        }

        public void LoadMesh(string stlPath)
        {
            try
            {
                // Load STL triangles using existing converter
                var converter = new StlConverter();
                _originalTriangles = converter.LoadTriangles(stlPath);
                _meshTriangles = new List<Converter.Core.Triangle>(_originalTriangles);
                
                // Reset rotations
                _rotationX = _rotationY = _rotationZ = 0;
                
                // Display the mesh
                DisplayMesh();
                
                // Adjust camera to fit mesh
                AdjustCameraToFitMesh();
                
                Title = $"STL Preview - {System.IO.Path.GetFileName(stlPath)}";
                UpdateTitleWithRotation(); // Initialize title properly
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load STL file: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayMesh()
        {
            // Remove existing mesh
            if (_meshVisual != null)
            {
                Viewport.Children.Remove(_meshVisual);
            }

            if (!_meshTriangles.Any())
                return;

            // Create mesh geometry from triangles
            var meshGeometry = new MeshGeometry3D();
            var positions = new Point3DCollection();
            var normals = new Vector3DCollection();
            var triangleIndices = new Int32Collection();

            int vertexIndex = 0;
            foreach (var triangle in _meshTriangles)
            {
                // Add vertices
                positions.Add(new Point3D(triangle.V1.X, triangle.V1.Y, triangle.V1.Z));
                positions.Add(new Point3D(triangle.V2.X, triangle.V2.Y, triangle.V2.Z));
                positions.Add(new Point3D(triangle.V3.X, triangle.V3.Y, triangle.V3.Z));
                
                // Calculate normal for this triangle
                var v1 = new Vector3D(triangle.V2.X - triangle.V1.X, triangle.V2.Y - triangle.V1.Y, triangle.V2.Z - triangle.V1.Z);
                var v2 = new Vector3D(triangle.V3.X - triangle.V1.X, triangle.V3.Y - triangle.V1.Y, triangle.V3.Z - triangle.V1.Z);
                var normal = Vector3D.CrossProduct(v1, v2);
                normal.Normalize();
                
                // Add the same normal for all three vertices of this triangle
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);
                
                // Add triangle indices
                triangleIndices.Add(vertexIndex);
                triangleIndices.Add(vertexIndex + 1);
                triangleIndices.Add(vertexIndex + 2);
                
                vertexIndex += 3;
            }

            meshGeometry.Positions = positions;
            meshGeometry.Normals = normals;
            meshGeometry.TriangleIndices = triangleIndices;

            // Create better material with more contrast
            var materialGroup = new MaterialGroup();
            materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 200)))); // Nice blue
            materialGroup.Children.Add(new SpecularMaterial(System.Windows.Media.Brushes.White, 20)); // Add some shine

            // Create geometry model
            var geometryModel = new GeometryModel3D(meshGeometry, materialGroup);
            geometryModel.BackMaterial = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180))); // Light gray back

            // Create visual
            _meshVisual = new ModelVisual3D
            {
                Content = geometryModel
            };

            Viewport.Children.Add(_meshVisual);
            
            // Update coordinate axes to show current orientation
            DisplayCoordinateAxes();
        }

        private void AdjustCameraToFitMesh()
        {
            if (!_meshTriangles.Any())
                return;

            var bounds = CalculateMeshBounds();
            var center = new Point3D(bounds.CenterX, bounds.CenterY, bounds.CenterZ);
            var size = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ);
            
            // Position camera to view the entire mesh with better angle
            var distance = size * 2.5;
            Camera.Position = new Point3D(center.X + distance * 0.7, center.Y + distance * 0.7, center.Z + distance * 0.7);
            Camera.LookDirection = new Vector3D(-0.7, -0.7, -0.7);
        }

        private void OnRotateClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string tag)
            {
                var parts = tag.Split(',');
                if (parts.Length == 2 && float.TryParse(parts[1], out var angle))
                {
                    var axis = parts[0];
                    
                    // Debug: Flash the button to show it was clicked
                    var originalBackground = button.Background;
                    button.Background = System.Windows.Media.Brushes.LightBlue;
                    
                    RotateMesh(axis, angle);
                    
                    // Restore button color after a short delay
                    System.Threading.Tasks.Task.Delay(200).ContinueWith(_ => 
                    {
                        Dispatcher.Invoke(() => button.Background = originalBackground);
                    });
                }
            }
        }

        private void OnResetRotationClick(object sender, RoutedEventArgs e)
        {
            // Reset to original mesh
            _meshTriangles = new List<Converter.Core.Triangle>(_originalTriangles);
            _rotationX = _rotationY = _rotationZ = 0;
            
            DisplayMesh();
            DisplayMirrorPlane(); // Refresh mirror plane too
            DisplayCoordinateAxes(); // Refresh coordinate axes too
            UpdateTitleWithRotation(); // Update title to remove rotation info
        }

        private void RotateMesh(string axis, float angleDegrees)
        {
            if (!_meshTriangles.Any()) return;
            
            // Update rotation tracking
            switch (axis)
            {
                case "X": _rotationX += angleDegrees; break;
                case "Y": _rotationY += angleDegrees; break;
                case "Z": _rotationZ += angleDegrees; break;
            }

            // Apply rotation to mesh triangles
            var angleRadians = angleDegrees * Math.PI / 180.0;
            var rotatedTriangles = new List<Converter.Core.Triangle>();

            foreach (var triangle in _meshTriangles)
            {
                var v1 = RotatePoint(triangle.V1, axis, angleRadians);
                var v2 = RotatePoint(triangle.V2, axis, angleRadians);
                var v3 = RotatePoint(triangle.V3, axis, angleRadians);
                
                rotatedTriangles.Add(new Converter.Core.Triangle(v1, v2, v3));
            }

            _meshTriangles = rotatedTriangles;
            
            // Force refresh of the display
            DisplayMesh();
            DisplayMirrorPlane(); // Refresh mirror plane with new bounds
            DisplayCoordinateAxes(); // Refresh coordinate axes with new orientation
            
            // Update window title to show current rotation
            UpdateTitleWithRotation();
            
            // Force WPF to update the viewport
            Viewport.InvalidateVisual();
            this.UpdateLayout();
        }
        
        private void UpdateTitleWithRotation()
        {
            var rotationText = "";
            if (_rotationX != 0 || _rotationY != 0 || _rotationZ != 0)
            {
                rotationText = $" (X:{_rotationX}° Y:{_rotationY}° Z:{_rotationZ}°)";
            }
            
            var fileName = !string.IsNullOrEmpty(Title) && Title.Contains(" - ") 
                ? Title.Split(" - ")[1].Split(" (")[0] 
                : "Unknown";
            Title = $"STL Preview - {fileName}{rotationText}";
        }

        private EgsLib.Vector3<float> RotatePoint(EgsLib.Vector3<float> point, string axis, double angleRadians)
        {
            var cos = (float)Math.Cos(angleRadians);
            var sin = (float)Math.Sin(angleRadians);

            return axis switch
            {
                "X" => new EgsLib.Vector3<float>(point.X, point.Y * cos - point.Z * sin, point.Y * sin + point.Z * cos),
                "Y" => new EgsLib.Vector3<float>(point.X * cos + point.Z * sin, point.Y, -point.X * sin + point.Z * cos),
                "Z" => new EgsLib.Vector3<float>(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos, point.Z),
                _ => point
            };
        }



        private void OnMirrorPlaneChanged(object sender, RoutedEventArgs e)
        {
            // Determine selected mirror plane
            if (radioNone.IsChecked == true)
                SelectedMirrorPlane = MirrorPlane.None;
            else if (radioXY.IsChecked == true)
                SelectedMirrorPlane = MirrorPlane.XY;
            else if (radioXZ.IsChecked == true)
                SelectedMirrorPlane = MirrorPlane.XZ;
            else if (radioYZ.IsChecked == true)
                SelectedMirrorPlane = MirrorPlane.YZ;

            // Update mirror plane visualization
            DisplayMirrorPlane();
        }

        private void DisplayMirrorPlane()
        {
            // Remove existing mirror plane
            if (_mirrorPlaneVisual != null)
            {
                Viewport.Children.Remove(_mirrorPlaneVisual);
                _mirrorPlaneVisual = null;
            }

            if (SelectedMirrorPlane == MirrorPlane.None || !_meshTriangles.Any())
                return;

            // Calculate mesh bounds for plane sizing
            var bounds = CalculateMeshBounds();
            var size = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ) * 1.5; // Increased size

            // Create plane geometry
            var planeGeometry = new MeshGeometry3D();
            var positions = new Point3DCollection();
            var triangleIndices = new Int32Collection();

            // Create plane based on selected mirror plane - position relative to mesh center
            switch (SelectedMirrorPlane)
            {
                case MirrorPlane.XY: // Z = mesh center
                    positions.Add(new Point3D(bounds.CenterX - size/2, bounds.CenterY - size/2, bounds.CenterZ));
                    positions.Add(new Point3D(bounds.CenterX + size/2, bounds.CenterY - size/2, bounds.CenterZ));
                    positions.Add(new Point3D(bounds.CenterX + size/2, bounds.CenterY + size/2, bounds.CenterZ));
                    positions.Add(new Point3D(bounds.CenterX - size/2, bounds.CenterY + size/2, bounds.CenterZ));
                    break;
                
                case MirrorPlane.XZ: // Y = mesh center
                    positions.Add(new Point3D(bounds.CenterX - size/2, bounds.CenterY, bounds.CenterZ - size/2));
                    positions.Add(new Point3D(bounds.CenterX + size/2, bounds.CenterY, bounds.CenterZ - size/2));
                    positions.Add(new Point3D(bounds.CenterX + size/2, bounds.CenterY, bounds.CenterZ + size/2));
                    positions.Add(new Point3D(bounds.CenterX - size/2, bounds.CenterY, bounds.CenterZ + size/2));
                    break;
                
                case MirrorPlane.YZ: // X = mesh center
                    positions.Add(new Point3D(bounds.CenterX, bounds.CenterY - size/2, bounds.CenterZ - size/2));
                    positions.Add(new Point3D(bounds.CenterX, bounds.CenterY + size/2, bounds.CenterZ - size/2));
                    positions.Add(new Point3D(bounds.CenterX, bounds.CenterY + size/2, bounds.CenterZ + size/2));
                    positions.Add(new Point3D(bounds.CenterX, bounds.CenterY - size/2, bounds.CenterZ + size/2));
                    break;
                
                default:
                    return;
            }

            // Add triangle indices for a quad (two triangles)
            triangleIndices.Add(0); triangleIndices.Add(1); triangleIndices.Add(2);
            triangleIndices.Add(0); triangleIndices.Add(2); triangleIndices.Add(3);

            planeGeometry.Positions = positions;
            planeGeometry.TriangleIndices = triangleIndices;

            // Create more visible semi-transparent material
            var planeMaterial = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 255, 200, 0))); // More opaque, orange-yellow
            var planeModel = new GeometryModel3D(planeGeometry, planeMaterial);

            _mirrorPlaneVisual = new ModelVisual3D
            {
                Content = planeModel
            };

            Viewport.Children.Add(_mirrorPlaneVisual);
        }

        private void DisplayCoordinateAxes()
        {
            // Remove existing coordinate axes
            if (_coordinateAxesVisual != null)
            {
                Viewport.Children.Remove(_coordinateAxesVisual);
                _coordinateAxesVisual = null;
            }

            if (!_meshTriangles.Any())
                return;

            // Calculate mesh bounds for positioning
            var bounds = CalculateMeshBounds();
            var meshSize = Math.Max(Math.Max(bounds.SizeX, bounds.SizeY), bounds.SizeZ);
            var axisLength = meshSize * 0.3f; // Axes are 30% of mesh size
            var arrowHeadSize = axisLength * 0.15f;

            // Position axes at bottom-left corner of mesh bounds
            var axisOrigin = new Point3D(
                bounds.MinX - meshSize * 0.2,
                bounds.MinY - meshSize * 0.2,
                bounds.MinZ - meshSize * 0.2
            );

            var axesGroup = new Model3DGroup();

            // Fixed world coordinate directions (do NOT rotate with mesh)
            var xDirection = new Vector3D(axisLength, 0, 0);  // Always points right
            var yDirection = new Vector3D(0, axisLength, 0);  // Always points up
            var zDirection = new Vector3D(0, 0, axisLength);  // Always points forward

            // Create X-axis (Red) - pointing right (world coordinate)
            var xAxis = CreateArrow(axisOrigin, xDirection, arrowHeadSize, 
                System.Windows.Media.Colors.Red, "X");
            axesGroup.Children.Add(xAxis);

            // Create Y-axis (Green) - pointing up (world coordinate)  
            var yAxis = CreateArrow(axisOrigin, yDirection, arrowHeadSize,
                System.Windows.Media.Colors.LimeGreen, "Y");
            axesGroup.Children.Add(yAxis);

            // Create Z-axis (Blue) - pointing forward (world coordinate - this is our "forward" indicator)
            var zAxis = CreateArrow(axisOrigin, zDirection, arrowHeadSize,
                System.Windows.Media.Colors.Blue, "Z");
            axesGroup.Children.Add(zAxis);

            // Create the visual
            _coordinateAxesVisual = new ModelVisual3D
            {
                Content = axesGroup
            };

            Viewport.Children.Add(_coordinateAxesVisual);
        }

        private Model3D CreateArrow(Point3D start, Vector3D direction, double arrowHeadSize, 
            System.Windows.Media.Color color, string label)
        {
            var group = new Model3DGroup();
            var material = new DiffuseMaterial(new SolidColorBrush(color));

            // Create the shaft (cylinder)
            var shaftGeometry = CreateCylinder(start, direction, arrowHeadSize * 0.3);
            var shaftModel = new GeometryModel3D(shaftGeometry, material);
            group.Children.Add(shaftModel);

            // Create the arrow head (cone)
            var headStart = new Point3D(
                start.X + direction.X * 0.85, // Arrow head starts at 85% of shaft length
                start.Y + direction.Y * 0.85,
                start.Z + direction.Z * 0.85
            );
            var headDirection = new Vector3D(
                direction.X * 0.15, // Arrow head is 15% of total length
                direction.Y * 0.15,
                direction.Z * 0.15
            );

            var headGeometry = CreateCone(headStart, headDirection, arrowHeadSize);
            var headModel = new GeometryModel3D(headGeometry, material);
            group.Children.Add(headModel);

            return group;
        }

        private MeshGeometry3D CreateCylinder(Point3D start, Vector3D direction, double radius)
        {
            var geometry = new MeshGeometry3D();
            var positions = new Point3DCollection();
            var triangleIndices = new Int32Collection();

            // Simple cylinder with 8 sides
            const int sides = 8;
            var end = new Point3D(start.X + direction.X, start.Y + direction.Y, start.Z + direction.Z);

            // Create perpendicular vectors
            var up = Math.Abs(direction.Z) < 0.9 ? new Vector3D(0, 0, 1) : new Vector3D(1, 0, 0);
            var right = Vector3D.CrossProduct(direction, up);
            right.Normalize();
            up = Vector3D.CrossProduct(right, direction);
            up.Normalize();

            // Create vertices
            for (int i = 0; i < sides; i++)
            {
                var angle = 2 * Math.PI * i / sides;
                var x = Math.Cos(angle) * radius;
                var y = Math.Sin(angle) * radius;

                var offset = right * x + up * y;

                // Start circle
                positions.Add(new Point3D(start.X + offset.X, start.Y + offset.Y, start.Z + offset.Z));
                // End circle  
                positions.Add(new Point3D(end.X + offset.X, end.Y + offset.Y, end.Z + offset.Z));
            }

            // Create triangles for cylinder sides
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                int i0 = i * 2, i1 = i * 2 + 1;
                int n0 = next * 2, n1 = next * 2 + 1;

                // Two triangles per side
                triangleIndices.Add(i0); triangleIndices.Add(n0); triangleIndices.Add(i1);
                triangleIndices.Add(i1); triangleIndices.Add(n0); triangleIndices.Add(n1);
            }

            geometry.Positions = positions;
            geometry.TriangleIndices = triangleIndices;
            return geometry;
        }

        private MeshGeometry3D CreateCone(Point3D start, Vector3D direction, double radius)
        {
            var geometry = new MeshGeometry3D();
            var positions = new Point3DCollection();
            var triangleIndices = new Int32Collection();

            const int sides = 8;
            var tip = new Point3D(start.X + direction.X, start.Y + direction.Y, start.Z + direction.Z);

            // Create perpendicular vectors
            var up = Math.Abs(direction.Z) < 0.9 ? new Vector3D(0, 0, 1) : new Vector3D(1, 0, 0);
            var right = Vector3D.CrossProduct(direction, up);
            right.Normalize();
            up = Vector3D.CrossProduct(right, direction);
            up.Normalize();

            // Add tip vertex
            positions.Add(tip);

            // Create base circle
            for (int i = 0; i < sides; i++)
            {
                var angle = 2 * Math.PI * i / sides;
                var x = Math.Cos(angle) * radius;
                var y = Math.Sin(angle) * radius;

                var offset = right * x + up * y;
                positions.Add(new Point3D(start.X + offset.X, start.Y + offset.Y, start.Z + offset.Z));
            }

            // Create triangles from tip to base
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;
                triangleIndices.Add(0); // tip
                triangleIndices.Add(i + 1);
                triangleIndices.Add(next + 1);
            }

            geometry.Positions = positions;
            geometry.TriangleIndices = triangleIndices;
            return geometry;
        }

        private MeshBounds CalculateMeshBounds()
        {
            if (!_meshTriangles.Any())
                return new MeshBounds();

            var allVertices = _meshTriangles.SelectMany(t => new[] { t.V1, t.V2, t.V3 }).ToList();
            
            return new MeshBounds
            {
                MinX = allVertices.Min(v => v.X),
                MaxX = allVertices.Max(v => v.X),
                MinY = allVertices.Min(v => v.Y),
                MaxY = allVertices.Max(v => v.Y),
                MinZ = allVertices.Min(v => v.Z),
                MaxZ = allVertices.Max(v => v.Z)
            };
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            UseMirrorVoxelization = SelectedMirrorPlane != MirrorPlane.None;
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public List<Converter.Core.Triangle> GetTransformedTriangles()
        {
            return new List<Converter.Core.Triangle>(_meshTriangles);
        }

        private struct MeshBounds
        {
            public float MinX, MaxX, MinY, MaxY, MinZ, MaxZ;
            public float SizeX => MaxX - MinX;
            public float SizeY => MaxY - MinY; 
            public float SizeZ => MaxZ - MinZ;
            public float CenterX => (MinX + MaxX) / 2;
            public float CenterY => (MinY + MaxY) / 2;
            public float CenterZ => (MinZ + MaxZ) / 2;
        }
    }
} 