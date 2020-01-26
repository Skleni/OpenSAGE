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
        [InlineData (0.5f, 2.5f, false)]
        [InlineData (1.5f, 2.5f, true)]
        [InlineData (2.5f, 2.5f, false)]
        [InlineData (3.5f, 2.5f, true)]
        [InlineData (4.5f, 2.5f, false)]
        [InlineData (2.5f, 1.5f, true)]
        [InlineData (2.5f, 0.5f, false)]
        [InlineData (2.5f, 1, true)] //on polygon
        [InlineData (2.5f, 2, false)] //on polygon
        [InlineData (1, 2, true)] //on polygon
        [InlineData (4, 2, false)] //on polygon
        public void OrthogonalPolygon(float x, float y, bool expectedResult)
        {
            // ___   ___
            // | |   | |
            // | |___| |
            // |_______|
            var polygon = BuildPolygon((1,1), (4,1), (4,4), (3,4), (3,2), (2,2), (2,4), (1,4));
            Assert.Equal(expectedResult, TriggerAreaRayShooter.IsInside(new Vector2(x, y), polygon));
        }

        [Theory]
        [InlineData(0, 3.5f, false)]
        [InlineData(1, 3.5f, false)]
        [InlineData(2, 3.5f, false)]
        [InlineData(3.5, 3.5f, true)] //on polygon
        [InlineData(4, 3.5f, true)]
        [InlineData(5, 3.5f, true)]
        [InlineData(6, 3.5f, false)] //on polygon
        [InlineData(7, 3.5f, false)]
        public void NearDiagonalEdge(float x, float y, bool expectedResult)
        {
            //     /|
            //    / |
            //   /  |
            //  /   |
            // /____|
            var polygon = BuildPolygon((6, 1), (6, 6), (5, 5), (4, 4), (3, 3), (2, 2), (1, 1));
            Assert.Equal(expectedResult, TriggerAreaRayShooter.IsInside(new Vector2(x, y), polygon));
        }
        [Theory]
        [InlineData(-3, 0, false)]
        [InlineData(-1, 0, true)]
        [InlineData(-2, 0, true)] //on polygon
        [InlineData( 0, 0, true)]
        [InlineData( 1, 0, true)]
        [InlineData( 2, 0, false)] //on polygon
        [InlineData( 3, 0, false)]
        public void HitVertex(float x, float y, bool expectedResult)
        {
            //  /\
            // /  \
            // \  /
            //  \/
            var polygon = BuildPolygon((-2, 0), (0, -2), (2, 0), (0, 2));
            Assert.Equal(expectedResult, TriggerAreaRayShooter.IsInside(new Vector2(x, y), polygon));
        }

        [Theory]
        [InlineData(0.5f, 2, false)]
        [InlineData(1.5f, 2, true)]
        [InlineData(2.5f, 2, false)] //on polygon
        [InlineData(3.5f, 2, true)]
        [InlineData(4.5f, 2, false)]
        public void HitEdge(float x, float y, bool expectedResult)
        {
            //  _   _
            // | |_| |
            // |_____|
            var polygon = BuildPolygon((2, 2), (2, 4), (1, 3), (1, 1), (4, 1), (4, 3), (3, 3), (3,2));
            Assert.Equal(expectedResult, TriggerAreaRayShooter.IsInside(new Vector2(x, y), polygon));
        }
        [Theory]
        [InlineData( 0,  1, true)]
        [InlineData( 0, -1, true)]
        [InlineData(-2,  1, false)]
        [InlineData(-2, -1, false)]
        public void NonSimplePolygon(float x, float y, bool expectedResult)
        {
            //  ____
            //  \  /
            //   \/
            //   /\
            //  /__\
            var polygon = BuildPolygon((2,2),(-2,2),(2,-2),(-2,-2));
            Assert.Equal(expectedResult, TriggerAreaRayShooter.IsInside(new Vector2(x, y), polygon));
        }

        [Theory]
        [InlineData(-1, 0, false)]
        [InlineData( 1, 0, false)]
        [InlineData( 4, 0, false)]
        public void DegeneratePolygon(float x, float y, bool expectedResult)
        {
            //  _____
            //
            var polygon = BuildPolygon((1, 0), (2, 0), (3, 0), (0, 0));
            Assert.Equal(expectedResult, TriggerAreaRayShooter.IsInside(new Vector2(x, y), polygon));
        }

        private IReadOnlyList<Vector2> BuildPolygon(params (float x, float y)[] coordinates)
        {
            return coordinates.Select(c => new Vector2(c.x, c.y)).ToArray();
        }
    }
}
