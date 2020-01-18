using System.Collections.Generic;
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
            return BoundingBox.Contains(position);
        }
    }
}
