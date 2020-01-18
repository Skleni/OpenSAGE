using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Scripting
{
    public sealed class TriggerAreaCollection
    {
        private Dictionary<uint, TriggerArea> _areasByID;
        private Dictionary<string, TriggerArea> _areasByName;

        public TriggerAreaCollection()
        {
            _areasByID = new Dictionary<uint, TriggerArea>();
            _areasByName = new Dictionary<string, TriggerArea>();
        }

        public TriggerAreaCollection(IEnumerable<Data.Map.TriggerArea> areas)
            : this()
        {
            foreach (var area in areas)
            {
                var triggerArea = new TriggerArea(area);
                _areasByID[area.ID] = triggerArea;
                _areasByName[area.Name] = triggerArea;
            }
        }

        public TriggerAreaCollection(IEnumerable<PolygonTrigger> areas)
            : this()
        {
            foreach (var area in areas)
            {
                var triggerArea = new TriggerArea(area);
                _areasByID[area.UniqueId] = triggerArea;
                _areasByName[area.Name] = triggerArea;
            }
        }

        public bool TryGetByName(string name, out TriggerArea area)
        {
            return _areasByName.TryGetValue(name, out area);
        }
    }


    public sealed class TriggerArea
    {
        private static readonly IEnumerable<Vector2> StandardRayDirections
            = new[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };

        public IReadOnlyList<Vector2> Points { get; }
        public RectangleF BoundingBox { get; }

        private TriggerArea(IReadOnlyList<Vector2> points)
        {
            Points = points;

            if (points.Count == 0)
            {
                BoundingBox = new RectangleF();
            }
            else
            {
                var minX = points.Min(p => p.X);
                var maxX = points.Max(p => p.X);
                var minY = points.Min(p => p.Y);
                var maxY = points.Max(p => p.Y);

                BoundingBox = new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }
        }

        public TriggerArea(Data.Map.TriggerArea area)
            : this(area.Points)
        {
        }

        public TriggerArea(PolygonTrigger trigger)
            : this(trigger.Points.Select(p => new Vector2(p.X, p.Y)).ToArray())
        {
        }

        public bool Contains(in Vector2 position)
        {
            if (!BoundingBox.Contains(position))
            {
                return false;
            }
            foreach (var direction in StandardRayDirections)
            {
                var result = TriggerAreaRayShooter.Analyze(position, direction, this);
                if (result != TriggerAreaRayShooter.PointPosition.Unknown)
                {
                    return result == TriggerAreaRayShooter.PointPosition.Inside;
                }
            }
            
            Debug.Assert(false, "no valid ray direction found - try with more directions or smaller epsilon?");
            return false;
        }

    }

    public sealed class TriggerAreaRayShooter
    {
        private static float Epsilon = 0.1f;
        public enum PointPosition
        {
            Inside,
            Outside,
            Unknown
        }

        public static PointPosition Analyze(Vector2 unitPosition, Vector2 rayDirection, TriggerArea area)
        {
            // To find out if position P is contained, start a ray there to a given direction
            // and count number of intersections with the boundary polygon.
            // even number -> not contained, odd number -> contained
            //      _____       ___
            //     /     \     /   |
            //     |  P---1---2----3--> ray
            //     |       \_/     |
            //     \______________/
            //
            // BUT: in case a polygon point lies too close to the ray, we get problems
            //      do incoming edges intersect the ray? both/none (as in V), or just one of them (as in +)?
            //      -> result gets completely unreliable, even though the point is clearly inside the polygon
            //      -> return 'Unknown' and try again with different ray direction
            //      _____   ___
            //     /     \ /   \
            //     |  P---V-----+-----> ray
            //     |           /
            //     \__________/

            if (area.Points.Count < 3)
            {
                return PointPosition.Outside;
            }
            // For each point of polygon, find out where it lies relative to the Ray
            // split the plane in 4 quadrants: left/right of ray and before/behind ray start
            //     B
            //     |
            //  A  P------> ray  B
            //     |
            //     A
            var prevPoint = area.Points.Last();
            var prev = GetRelativePointPosition(prevPoint, unitPosition, rayDirection);
            if (prev.Orthogonal == Side.Unknown)
            {
                return PointPosition.Unknown;
            }
            RelPos next;
            int counter = 0;
            foreach (var point in area.Points)
            {
                next = GetRelativePointPosition(point, unitPosition, rayDirection);
                if (next.Orthogonal == Side.Unknown)
                {
                    return PointPosition.Unknown;
                }
                if (next.Orthogonal != prev.Orthogonal)
                {
                    if (next.InDirection == Side.Unknown && prev.InDirection == Side.Unknown)
                    {
                        //the tested point lies on this polygon edge more or less
                        return PointPosition.Inside;
                    }
                    if (next.InDirection == Side.B && prev.InDirection == Side.B)
                    {
                        ++counter;
                    }
                    else if (next.InDirection == Side.B || prev.InDirection == Side.B)
                    {
                        if (next.InDirection == Side.Unknown || prev.InDirection == Side.Unknown)
                        {
                            ++counter;
                        }
                        // in this case, can't decide by the quadrants alone
                        // in both cases below, one polygon point lies in top-right
                        // and one in bottom-left quadrant: 
                        //            _______                   _______
                        //           /                         /
                        //        P-/-----------> ray         /
                        //         /                         / 
                        //        /                         / P------------> ray
                        //  _____/                    _____/

                        var edgeDir = Vector2.Normalize(point - prevPoint);
                        var toRayStart = (unitPosition - point);
                        var sinPoint = Vector2Utility.Cross(edgeDir, toRayStart);
                        var sinRay = Vector2Utility.Cross(rayDirection, edgeDir);
                        if (sinRay > 0 && sinPoint > -Epsilon)
                        {
                            ++counter;
                        }
                        else if (sinRay < 0 && sinPoint < Epsilon)
                        {
                            ++counter;
                        }
                    }
                }
                prev = next;
                prevPoint = point;
            }
            return (counter % 2 != 0) ? PointPosition.Inside : PointPosition.Outside;
        }

        private enum Side
        {
            A,
            B,
            Unknown
        }
        private class RelPos
        {
            public RelPos(Side inDir, Side orth)
            {
                InDirection = inDir;
                Orthogonal = orth;
            }
            public Side InDirection { get; set; }
            public Side Orthogonal { get; set; }
        }

        private static RelPos GetRelativePointPosition(in Vector2 polygonPoint, in Vector2 unitPos, in Vector2 rayDirection)
        {
            var toPoint = (polygonPoint - unitPos);
            var cosTop = Vector2.Dot(toPoint, rayDirection);
            var face = cosTop > Epsilon ? Side.B : (cosTop < -Epsilon ? Side.A : Side.Unknown);
            var sinTop = Vector2Utility.Cross(toPoint, rayDirection);
            var side = sinTop > Epsilon ? Side.B : (sinTop < -Epsilon ? Side.A : Side.Unknown);
            return new RelPos(face, side);
        }
    }
}
