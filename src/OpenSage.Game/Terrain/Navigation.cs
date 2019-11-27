using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenSage.Mathematics;
using SharpNav;
using SharpNav.Geometry;

namespace OpenSage.Terrain
{
    public sealed class Navigation : DisposableBase
    {
        private Heightfield _heightfield;
        private CompactHeightfield _compactHeightfield;

        private NavMeshBuilder _builder;
        private NavMeshQuery _query;

        internal Navigation(HeightMap heightMap)
        {
            var points = GeneratePoints(heightMap);
            var indices = GenerateIndices(heightMap);

            var boundingBox = BoundingBox.CreateFromPoints(points);

            var settings = NavMeshGenerationSettings.Default;
            //settings.CellSize = HeightMap.HorizontalScale;
            //settings.CellHeight = heightMap._verticalScale;

            _heightfield = new Heightfield(new BBox3(boundingBox.Min, boundingBox.Max), settings);
            _heightfield.RasterizeTrianglesIndexed(points, indices);

            _compactHeightfield = new CompactHeightfield(_heightfield, settings);

            var contourSet = _compactHeightfield.BuildContourSet(settings);
            var polyMesh = new PolyMesh(contourSet, settings);
            var polyMeshDetail = new PolyMeshDetail(polyMesh, _compactHeightfield, settings);

            //TODO:
        }

        private Vector3[] GeneratePoints(HeightMap heightMap)
        {
            var numVertices = heightMap.Width * heightMap.Height;
            var points = new Vector3[numVertices];

            var vertexIndex = 0;
            for (var y = 0; y < heightMap.Height; y++)
            {
                for (var x = 0; x < heightMap.Width; x++)
                {
                    var position = heightMap.GetPosition(x, y);
                    points[vertexIndex++] = position;
                }
            }

            return points;
        }

        private uint CalculateNumIndices(int width, int height) => (uint) ((width - 1) * (height - 1) * 6);

        private int[] GenerateIndices(HeightMap heightMap)
        {
            var numIndices = CalculateNumIndices(heightMap.Width, heightMap.Height);
            var indices = new int[numIndices];

            for (int y = 0, indexIndex = 0; y < heightMap.Height - 1; y++)
            {
                var yThis = y * heightMap.Width;
                var yNext = (y + 1) * heightMap.Width;

                for (var x = 0; x < heightMap.Width - 1; x++)
                {
                    // Triangle 1
                    indices[indexIndex++] = (yThis + x);
                    indices[indexIndex++] = (yThis + x + 1);
                    indices[indexIndex++] = (yNext + x);

                    // Triangle 2
                    indices[indexIndex++] = (yNext + x);
                    indices[indexIndex++] = (yThis + x + 1);
                    indices[indexIndex++] = (yNext + x + 1);
                }
            }

            return indices;
        }
    }
}
