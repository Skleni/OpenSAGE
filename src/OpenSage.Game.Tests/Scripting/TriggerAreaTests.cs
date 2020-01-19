using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenSage.Scripting;
using Xunit;

namespace OpenSage.Tests.Scripting
{
    public class TriggerAreaRayShooterTests
    {
        [Theory]
        [InlineData (0.5f, 2.5f, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData (1.5f, 2.5f, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData (2.5f, 2.5f, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData (3.5f, 2.5f, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData (4.5f, 2.5f, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData (2.5f, 1.5f, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData (2.5f, 0.5f, TriggerAreaRayShooter.PointPosition.Outside)]
        public void OrthogonalPolygon(float x, float y, TriggerAreaRayShooter.PointPosition expectedResult)
        {
            // ___   ___
            // | |   | |
            // | |___| |
            // |_______|
            var polygon = BuildPolygon((1,1), (4,1), (4,4), (3,4), (3,2), (2,2), (2,4), (1,4));
            var ray = Vector2.UnitX;
            Assert.Equal(expectedResult, TriggerAreaRayShooter.Analyze(new Vector2(x, y), ray, polygon));
        }

        [Theory]
        [InlineData(0, 3.5f, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData(1, 3.5f, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData(2, 3.5f, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData(4, 3.5f, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(5, 3.5f, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(6, 3.5f, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(7, 3.5f, TriggerAreaRayShooter.PointPosition.Outside)]
        public void NearDiagonalEdge(float x, float y, TriggerAreaRayShooter.PointPosition expectedResult)
        {
            //     /|
            //    / |
            //   /  |
            //  /   |
            // /____|
            var polygon = BuildPolygon((6, 1), (6, 6), (5, 5), (4, 4), (3, 3), (2, 2), (1, 1));
            var ray = Vector2.UnitX;
            Assert.Equal(expectedResult, TriggerAreaRayShooter.Analyze(new Vector2(x, y), ray, polygon));
        }
        [Theory]
        [InlineData(-3, 0, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData(-1, 0, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(0, 0, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(1, 0, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(3, 0, TriggerAreaRayShooter.PointPosition.Outside)]
        public void HitVertex(float x, float y, TriggerAreaRayShooter.PointPosition expectedResult)
        {
            //  /\
            // /  \
            // \  /
            //  \/
            var polygon = BuildPolygon((-2, 0), (0, -2), (2, 0), (0, 2));
            var ray = Vector2.UnitX;
            Assert.Equal(expectedResult, TriggerAreaRayShooter.Analyze(new Vector2(x, y), ray, polygon));
        }

        [Theory]
        [InlineData(0.5f, 2, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData(1.5f, 2, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(2.5f, 2, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(3.5f, 2, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(4.5f, 2, TriggerAreaRayShooter.PointPosition.Outside)]
        public void HitEdge(float x, float y, TriggerAreaRayShooter.PointPosition expectedResult)
        {
            //  _   _
            // | |_| |
            // |_____|
            var polygon = BuildPolygon((2, 2), (2, 4), (1, 3), (1, 1), (4, 1), (4, 3), (3, 3), (3,2));
            var ray = Vector2.UnitX;
            Assert.Equal(expectedResult, TriggerAreaRayShooter.Analyze(new Vector2(x, y), ray, polygon));
        }
        [Theory]
        [InlineData(0, 1, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(0, -1, TriggerAreaRayShooter.PointPosition.Inside)]
        [InlineData(-2, 1, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData(-2, -1, TriggerAreaRayShooter.PointPosition.Outside)]
        public void NonSimplePolygon(float x, float y, TriggerAreaRayShooter.PointPosition expectedResult)
        {
            //  ____
            //  \  /
            //   \/
            //   /\
            //  /__\
            var polygon = BuildPolygon((2,2),(-2,2),(2,-2),(-2,-2));
            var ray = Vector2.UnitX;
            Assert.Equal(expectedResult, TriggerAreaRayShooter.Analyze(new Vector2(x, y), ray, polygon));
        }

        [Theory]
        [InlineData(-1, 0, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData(1, 0, TriggerAreaRayShooter.PointPosition.Outside)]
        [InlineData(4, 0, TriggerAreaRayShooter.PointPosition.Outside)]
        public void DegeneratePolygon(float x, float y, TriggerAreaRayShooter.PointPosition expectedResult)
        {
            //  _____
            //
            var polygon = BuildPolygon((1, 0), (2, 0), (3, 0), (0, 0));
            var ray = Vector2.UnitX;
            Assert.Equal(expectedResult, TriggerAreaRayShooter.Analyze(new Vector2(x, y), ray, polygon));
        }

        private IReadOnlyList<Vector2> BuildPolygon(params (float x, float y)[] coordinates)
        {
            return coordinates.Select(c => new Vector2(c.x, c.y)).ToArray();
        }
    }
}
