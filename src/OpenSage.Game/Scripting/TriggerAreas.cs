using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Map;

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
    }

    public sealed class TriggerArea
    {
        public IReadOnlyList<Vector2> Points { get; }

        public TriggerArea(Data.Map.TriggerArea area)
        {
            Points = area.Points;
        }

        public TriggerArea(PolygonTrigger trigger)
        {
            Points = trigger.Points
                .Select(p => new Vector2(p.X, p.Y))
                .ToArray();
        }
    }
}
