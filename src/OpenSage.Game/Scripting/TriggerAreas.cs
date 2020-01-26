﻿using System;
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
            return TriggerAreaRayShooter.IsInside(position, Points);
        }
    }

    public sealed class TriggerAreaRayShooter
    {
        public static bool IsInside(Vector2 P, IReadOnlyList<Vector2> area)
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
            //      -> result gets unreliable, even though the point is clearly inside the polygon
            //      -> convention: exactly same y belongs to area below the ray
            //      -> careful when doing this with floats..
            //      _____   ___
            //     /     \ /   \
            //     |  P---V-----+-----> ray
            //     |           /
            //     \__________/

            if (area.Count < 3)
            {
                return false;
            }
            // For each point of polygon, find out where it lies relative to the Ray
            // split the plane in 4 quadrants: left/right of ray and before/behind ray start
            //     B
            //     |
            //  A  P------> ray  B
            //     |
            //     A
            
            var prevPoint = area.Last();
            var prev = GetRelativePointPosition(prevPoint, P);

            // now go through all points. Skip those on the ray,
            // keep track of the last handled point off the ray (only use its Orthogonal component, though)
            RelPos next;
            int counter = 0;
            foreach (var point in area)
            {
                next = GetRelativePointPosition(point, P);
                if (next.SideY != prev.SideY)
                {
                    if (next.SideX == Side.B && prev.SideX == Side.B)
                    {
                        // intersect x axis on right side of the ray start
                        ++counter;
                    }
                    else if (next.SideX == Side.B || prev.SideX == Side.B)
                    {
                        // in this case, can't decide by the quadrants alone
                        // in both cases below, one polygon point lies in top-right
                        // and one in bottom-left quadrant:
                        // (so P lies in the axis-aligned bounding box of the edge)
                        //            _______                   _______
                        //           /                         /
                        //        P-/-----------> ray         /
                        //         /                         / 
                        //        /                         / P------------> ray
                        //  _____/                    _____/

                        var edgeX = point.X - prevPoint.X;
                        var edgeY = point.Y - prevPoint.Y;
                        var offsetY = P.Y - prevPoint.Y;
                        var intersectionX = prevPoint.X + edgeX * (offsetY / edgeY);
                        if (intersectionX > P.X)
                        {
                            ++counter;
                        }
                    }
                }
                prev = next;
                prevPoint = point;
            }
            return (counter % 2 != 0) ? true : false;
        }

        private enum Side
        {
            A,
            B
        }
        private class RelPos
        {
            public RelPos(Side x, Side y)
            {
                SideX = x;
                SideY = y;
            }
            public Side SideX { get; set; }
            public Side SideY { get; set; }
        }

        private static RelPos GetRelativePointPosition(in Vector2 point, in Vector2 rayStart)
        {
            var x = point.X > rayStart.X ? Side.B : Side.A;
            var y = point.Y > rayStart.Y ? Side.B : Side.A;
            return new RelPos(x, y);
        }
    }
}
