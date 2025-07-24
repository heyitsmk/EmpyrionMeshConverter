using EgsLib;
using System.Diagnostics;

namespace Converter.Core
{
    /// <summary>
    /// Engine for converting triangles to voxels with morphological operations
    /// </summary>
    public class VoxelizationEngine
    {
        /// <summary>
        /// Convert triangles to voxels with the specified parameters
        /// </summary>
        public VoxelizationResult Voxelize(List<Triangle> triangles, ConversionConfiguration config, IProgress<string>? progress = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var stats = new VoxelizationStats
            {
                InputTriangles = triangles.Count
            };

            progress?.Report($"Processing {triangles.Count} triangles...");

            // Step 1: Center triangles around origin for proper blueprint spawning
            var centeredTriangles = CenterTrianglesAroundOrigin(triangles, progress);

            // Step 2: Scale triangles to target size
            var scaledTriangles = ScaleTrianglesForMaxSize(centeredTriangles, config.MaxSize, progress);

            // Step 3: Generate points from scaled triangles at specified resolution
            var points = GeneratePointsFromTriangles(scaledTriangles, config.Resolution, progress);
            stats.GeneratedVoxels = points.Count;
            
            progress?.Report($"Generated {points.Count} voxels from triangulation");

            // Step 4: Apply morphological operations
            if (config.ErosionRadius > 0)
            {
                progress?.Report($"Applying erosion (radius: {config.ErosionRadius})...");
                points = ApplyErosion(points, config.ErosionRadius);
            }

            if (config.DilationRadius > 0)
            {
                progress?.Report($"Applying dilation (radius: {config.DilationRadius})...");
                points = ApplyDilation(points, config.DilationRadius);
            }

            // Step 5: Apply hollowing if requested
            if (config.CreateHollowHull)
            {
                progress?.Report($"Creating hollow hull (radius: {config.HollowRadius})...");
                points = ApplyHollowing(points, config.HollowRadius);
            }

            // Step 6: Translate to origin
            var bounds = CalculateBounds(points);
            var scaledResult = TranslateToOrigin(points, bounds);
            
            stats.FinalVoxels = scaledResult.voxels.Count;
            stats.ProcessingTime = stopwatch.Elapsed;
            stats.FinalSize = scaledResult.bounds.Size;

            progress?.Report($"Voxelization complete: {stats.FinalVoxels} blocks in {stats.ProcessingTime.TotalSeconds:F2}s");

            return new VoxelizationResult
            {
                Voxels = scaledResult.voxels,
                Bounds = scaledResult.bounds,
                Stats = stats
            };
        }

        private HashSet<Vector3<int>> GeneratePointsFromTriangles(List<Triangle> triangles, float resolution, IProgress<string>? progress)
        {
            var points = new HashSet<Vector3<int>>();
            var processedTriangles = 0;

            foreach (var triangle in triangles)
            {
                // Subdivide triangle based on resolution and add points
                var trianglePoints = SubdivideTriangle(triangle, resolution);
                foreach (var point in trianglePoints)
                {
                    points.Add(point);
                }

                processedTriangles++;
                if (processedTriangles % 1000 == 0)
                {
                    progress?.Report($"Processed {processedTriangles}/{triangles.Count} triangles");
                }
            }

            return points;
        }

        private HashSet<Vector3<int>> SubdivideTriangle(Triangle triangle, float resolution)
        {
            var points = new HashSet<Vector3<int>>();

            // Convert triangle vertices to scaled integer coordinates
            var v1 = ScaleAndRoundPoint(triangle.V1, resolution);
            var v2 = ScaleAndRoundPoint(triangle.V2, resolution);  
            var v3 = ScaleAndRoundPoint(triangle.V3, resolution);

            // Add the vertices themselves
            points.Add(v1);
            points.Add(v2);
            points.Add(v3);

            // Fill triangle using rasterization approach
            RasterizeTriangle(v1, v2, v3, points);

            return points;
        }

        private void RasterizeTriangle(Vector3<int> v1, Vector3<int> v2, Vector3<int> v3, HashSet<Vector3<int>> points)
        {
            // Find bounding box of triangle
            var minX = Math.Min(Math.Min(v1.X, v2.X), v3.X);
            var maxX = Math.Max(Math.Max(v1.X, v2.X), v3.X);
            var minY = Math.Min(Math.Min(v1.Y, v2.Y), v3.Y);
            var maxY = Math.Max(Math.Max(v1.Y, v2.Y), v3.Y);
            var minZ = Math.Min(Math.Min(v1.Z, v2.Z), v3.Z);
            var maxZ = Math.Max(Math.Max(v1.Z, v2.Z), v3.Z);

            // For each point in bounding box, check if it's inside the triangle
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        var point = new Vector3<int>(x, y, z);
                        if (IsPointInTriangle(point, v1, v2, v3))
                        {
                            points.Add(point);
                        }
                    }
                }
            }
        }

        private bool IsPointInTriangle(Vector3<int> point, Vector3<int> v1, Vector3<int> v2, Vector3<int> v3)
        {
            // Use barycentric coordinates to test if point is inside triangle
            // This is a simplified 3D implementation - for better accuracy, 
            // project to 2D plane of triangle first

            var p = new Vector3<float>(point.X, point.Y, point.Z);
            var a = new Vector3<float>(v1.X, v1.Y, v1.Z);
            var b = new Vector3<float>(v2.X, v2.Y, v2.Z);
            var c = new Vector3<float>(v3.X, v3.Y, v3.Z);

            // Calculate vectors
            var v0 = Subtract(c, a);
            var v1f = Subtract(b, a);
            var v2f = Subtract(p, a);

            // Calculate dot products
            var dot00 = Dot(v0, v0);
            var dot01 = Dot(v0, v1f);
            var dot02 = Dot(v0, v2f);
            var dot11 = Dot(v1f, v1f);
            var dot12 = Dot(v1f, v2f);

            // Calculate barycentric coordinates
            var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            var v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        private Vector3<float> Subtract(Vector3<float> a, Vector3<float> b)
        {
            return new Vector3<float>(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        private float Dot(Vector3<float> a, Vector3<float> b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        private Vector3<int> ScaleAndRoundPoint(Vector3<float> point, float resolution)
        {
            return new Vector3<int>(
                (int)Math.Round(point.X * resolution),
                (int)Math.Round(point.Y * resolution),
                (int)Math.Round(point.Z * resolution)
            );
        }

        private HashSet<Vector3<int>> ApplyErosion(HashSet<Vector3<int>> voxels, int radius)
        {
            if (radius <= 0) return voxels;

            var structuringElement = GenerateStructuringElement(radius);
            var result = new HashSet<Vector3<int>>();

            foreach (var voxel in voxels)
            {
                bool shouldKeep = true;
                
                // Check if all points in structuring element are present
                foreach (var offset in structuringElement)
                {
                    var checkPoint = new Vector3<int>(
                        voxel.X + offset.X,
                        voxel.Y + offset.Y,
                        voxel.Z + offset.Z
                    );
                    
                    if (!voxels.Contains(checkPoint))
                    {
                        shouldKeep = false;
                        break;
                    }
                }

                if (shouldKeep)
                {
                    result.Add(voxel);
                }
            }

            return result;
        }

        private HashSet<Vector3<int>> ApplyDilation(HashSet<Vector3<int>> voxels, int radius)
        {
            if (radius <= 0) return voxels;

            var structuringElement = GenerateStructuringElement(radius);
            var result = new HashSet<Vector3<int>>(voxels);

            foreach (var voxel in voxels)
            {
                foreach (var offset in structuringElement)
                {
                    var newPoint = new Vector3<int>(
                        voxel.X + offset.X,
                        voxel.Y + offset.Y,
                        voxel.Z + offset.Z
                    );
                    result.Add(newPoint);
                }
            }

            return result;
        }

        private HashSet<Vector3<int>> ApplyHollowing(HashSet<Vector3<int>> voxels, int radius)
        {
            var structuringElement = GenerateStructuringElement(radius);
            var result = new HashSet<Vector3<int>>();

            foreach (var voxel in voxels)
            {
                bool isOnSurface = false;
                
                // Check if any point in structuring element is missing (indicating surface)
                foreach (var offset in structuringElement)
                {
                    var checkPoint = new Vector3<int>(
                        voxel.X + offset.X,
                        voxel.Y + offset.Y,
                        voxel.Z + offset.Z
                    );
                    
                    if (!voxels.Contains(checkPoint))
                    {
                        isOnSurface = true;
                        break;
                    }
                }

                if (isOnSurface)
                {
                    result.Add(voxel);
                }
            }

            return result;
        }

        private List<Vector3<int>> GenerateStructuringElement(int radius)
        {
            var element = new List<Vector3<int>>();
            
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        // Use spherical structuring element
                        if (x * x + y * y + z * z <= radius * radius)
                        {
                            element.Add(new Vector3<int>(x, y, z));
                        }
                    }
                }
            }

            return element;
        }

        private BoundingBox CalculateBounds(HashSet<Vector3<int>> voxels)
        {
            if (!voxels.Any())
                return new BoundingBox();

            var minX = voxels.Min(v => v.X);
            var maxX = voxels.Max(v => v.X);
            var minY = voxels.Min(v => v.Y);
            var maxY = voxels.Max(v => v.Y);
            var minZ = voxels.Min(v => v.Z);
            var maxZ = voxels.Max(v => v.Z);

            return new BoundingBox
            {
                Min = new Vector3<int>(minX, minY, minZ),
                Max = new Vector3<int>(maxX, maxY, maxZ)
            };
        }

        private List<Triangle> CenterTrianglesAroundOrigin(List<Triangle> triangles, IProgress<string>? progress)
        {
            if (triangles.Count == 0)
                return triangles;

            // Calculate bounding box of all triangle vertices
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;

            foreach (var triangle in triangles)
            {
                // Check all three vertices
                foreach (var vertex in new[] { triangle.V1, triangle.V2, triangle.V3 })
                {
                    minX = Math.Min(minX, vertex.X);
                    maxX = Math.Max(maxX, vertex.X);
                    minY = Math.Min(minY, vertex.Y);
                    maxY = Math.Max(maxY, vertex.Y);
                    minZ = Math.Min(minZ, vertex.Z);
                    maxZ = Math.Max(maxZ, vertex.Z);
                }
            }

            // Calculate the center point of the bounding box
            var centerX = (minX + maxX) / 2.0f;
            var centerY = (minY + maxY) / 2.0f;
            var centerZ = (minZ + maxZ) / 2.0f;
            var center = new Vector3<float>(centerX, centerY, centerZ);

            progress?.Report($"Centering mesh around origin (center offset: {centerX:F2}, {centerY:F2}, {centerZ:F2})");

            // Center all triangles by subtracting the center point
            var centeredTriangles = new List<Triangle>();
            foreach (var triangle in triangles)
            {
                var centeredV1 = new Vector3<float>(triangle.V1.X - center.X, triangle.V1.Y - center.Y, triangle.V1.Z - center.Z);
                var centeredV2 = new Vector3<float>(triangle.V2.X - center.X, triangle.V2.Y - center.Y, triangle.V2.Z - center.Z);
                var centeredV3 = new Vector3<float>(triangle.V3.X - center.X, triangle.V3.Y - center.Y, triangle.V3.Z - center.Z);
                
                centeredTriangles.Add(new Triangle(centeredV1, centeredV2, centeredV3));
            }

            return centeredTriangles;
        }

        private List<Triangle> ScaleTrianglesForMaxSize(List<Triangle> triangles, int maxSize, IProgress<string>? progress)
        {
            if (triangles.Count == 0)
                return triangles;

            // Calculate bounding box of all triangle vertices
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;

            foreach (var triangle in triangles)
            {
                // Check all three vertices
                foreach (var vertex in new[] { triangle.V1, triangle.V2, triangle.V3 })
                {
                    minX = Math.Min(minX, vertex.X);
                    maxX = Math.Max(maxX, vertex.X);
                    minY = Math.Min(minY, vertex.Y);
                    maxY = Math.Max(maxY, vertex.Y);
                    minZ = Math.Min(minZ, vertex.Z);
                    maxZ = Math.Max(maxZ, vertex.Z);
                }
            }

            // Calculate current mesh size
            var sizeX = maxX - minX;
            var sizeY = maxY - minY;
            var sizeZ = maxZ - minZ;
            var maxDimension = Math.Max(Math.Max(sizeX, sizeY), sizeZ);

            // Calculate scale factor to make the largest dimension equal to maxSize
            var scale = (float)maxSize / maxDimension;
            
            progress?.Report($"Scaling mesh by factor {scale:F3} to target size {maxSize} (from max dimension {maxDimension:F1})");

            // If no scaling needed, return original triangles
            if (Math.Abs(scale - 1.0f) < 0.001f)
            {
                return triangles;
            }

            // Scale all triangles
            var scaledTriangles = new List<Triangle>();
            foreach (var triangle in triangles)
            {
                var scaledV1 = new Vector3<float>(triangle.V1.X * scale, triangle.V1.Y * scale, triangle.V1.Z * scale);
                var scaledV2 = new Vector3<float>(triangle.V2.X * scale, triangle.V2.Y * scale, triangle.V2.Z * scale);
                var scaledV3 = new Vector3<float>(triangle.V3.X * scale, triangle.V3.Y * scale, triangle.V3.Z * scale);
                
                scaledTriangles.Add(new Triangle(scaledV1, scaledV2, scaledV3));
            }

            return scaledTriangles;
        }

        private (HashSet<Vector3<int>> voxels, BoundingBox bounds) TranslateToOrigin(
            HashSet<Vector3<int>> voxels, BoundingBox bounds)
        {
            var translatedVoxels = new HashSet<Vector3<int>>();
            var offset = bounds.Min;

            foreach (var voxel in voxels)
            {
                var translatedVoxel = new Vector3<int>(
                    voxel.X - offset.X,
                    voxel.Y - offset.Y,
                    voxel.Z - offset.Z
                );
                translatedVoxels.Add(translatedVoxel);
            }

            var newBounds = new BoundingBox
            {
                Min = new Vector3<int>(0, 0, 0),
                Max = new Vector3<int>(bounds.Size.X - 1, bounds.Size.Y - 1, bounds.Size.Z - 1)
            };

            return (translatedVoxels, newBounds);
        }
    }
} 