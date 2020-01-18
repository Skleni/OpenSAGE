using System;
using System.Collections.Generic;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Scripting
{
    public sealed class TriggerAreaCollection
    {
        private Dictionary<uint, TriggerArea> _areasByID;
        private Dictionary<string, TriggerArea> _areasByName;

        public TriggerAreaCollection()
        {
            // Note that we explicitly allow duplicate waypoint names.

            _areasByID = new Dictionary<uint, TriggerArea>();
            _areasByName = new Dictionary<string, TriggerArea>();
        }

        public TriggerAreaCollection(IEnumerable<TriggerArea> areas)
            : this()
        {
            foreach (var area in areas)
            {
                _areasByID[area.ID] = area;
                _areasByName[area.Name] = area;
            }
        }
    }

    public sealed class MapTriggerArea
    {
        private readonly Lazy<IReadOnlyList<Line2D>> _edges;

        public TriggerArea TriggerArea { get; }
        public IReadOnlyList<Line2D> Edges => _edges.Value;

        public MapTriggerArea(TriggerArea area)
        {
            TriggerArea = area;
        }
    }
}
