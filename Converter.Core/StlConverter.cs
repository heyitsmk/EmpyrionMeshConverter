using EgsLib;
using System.Text;

namespace Converter.Core
{
    /// <summary>
    /// Converter for STL (Stereolithography) files
    /// </summary>
    public class StlConverter : IMeshConverter
    {
        public string[] SupportedExtensions => new[] { ".stl" };

        public List<Triangle> LoadTriangles(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"STL file not found: {filePath}");

            // Determine if the file is binary or ASCII STL
            if (IsBinaryStl(filePath))
            {
                return LoadBinaryStl(filePath);
            }
            else
            {
                return LoadAsciiStl(filePath);
            }
        }

        private bool IsBinaryStl(string filePath)
        {
            // Read first 80 bytes (header) and check for "solid" keyword
            // Binary STL files may start with "solid" too, so we need additional checks
            using var reader = new BinaryReader(File.OpenRead(filePath));
            
            if (reader.BaseStream.Length < 84) // Minimum size for binary STL header
                return false;

            var header = reader.ReadBytes(80);
            var triangleCount = reader.ReadUInt32();

            // Calculate expected file size for binary STL
            var expectedSize = 80 + 4 + (triangleCount * 50); // header + count + (50 bytes per triangle)
            
            // If file size matches binary STL format exactly, it's binary
            if (reader.BaseStream.Length == expectedSize)
                return true;

            // Check if header contains non-printable characters (more likely binary)
            var headerText = Encoding.ASCII.GetString(header);
            return headerText.Any(c => c < 32 && c != 9 && c != 10 && c != 13); // Allow tab, LF, CR
        }

        private List<Triangle> LoadBinaryStl(string filePath)
        {
            var triangles = new List<Triangle>();

            using var reader = new BinaryReader(File.OpenRead(filePath));
            
            // Skip 80-byte header
            reader.ReadBytes(80);
            
            // Read triangle count
            var triangleCount = reader.ReadUInt32();
            
            for (uint i = 0; i < triangleCount; i++)
            {
                // Skip normal vector (3 floats)
                reader.ReadSingle(); // Normal X
                reader.ReadSingle(); // Normal Y  
                reader.ReadSingle(); // Normal Z
                
                // Read vertices
                var v1 = new Vector3<float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                var v2 = new Vector3<float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                var v3 = new Vector3<float>(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                
                triangles.Add(new Triangle(v1, v2, v3));
                
                // Skip 2-byte attribute count
                reader.ReadUInt16();
            }

            return triangles;
        }

        private List<Triangle> LoadAsciiStl(string filePath)
        {
            var triangles = new List<Triangle>();
            var lines = File.ReadAllLines(filePath);
            
            var vertexCount = 0;
            var vertices = new Vector3<float>[3];

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim().ToLowerInvariant();
                
                if (trimmedLine.StartsWith("facet"))
                {
                    // Start of a new triangle
                    vertexCount = 0;
                }
                else if (trimmedLine.StartsWith("vertex"))
                {
                    // Parse vertex coordinates
                    var parts = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4 && vertexCount < 3)
                    {
                        if (float.TryParse(parts[1], out var x) &&
                            float.TryParse(parts[2], out var y) &&
                            float.TryParse(parts[3], out var z))
                        {
                            vertices[vertexCount] = new Vector3<float>(x, y, z);
                            vertexCount++;
                        }
                    }
                }
                else if (trimmedLine.StartsWith("endfacet"))
                {
                    // End of triangle - create triangle if we have 3 vertices
                    if (vertexCount == 3)
                    {
                        triangles.Add(new Triangle(vertices[0], vertices[1], vertices[2]));
                    }
                }
            }

            return triangles;
        }
    }
} 