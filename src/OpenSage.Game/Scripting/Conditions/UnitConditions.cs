using System;
using System.Collections.Generic;
using System.Text;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Scripting.Conditions
{
    public static class UnitConditions
    {
        public static bool NamedInsideArea(ScriptCondition condition, ScriptExecutionContext context)
        {
            var unitName = condition.Arguments[0].StringValue;
            var areaName = condition.Arguments[1].StringValue;

            if (!context.Scene.GameObjects.TryGetObjectByName(unitName, out var unit))
            {
                ScriptingSystem.Logger.Warn($"Unit \"{unitName}\" does not exist.");
                return false;
            }

            if (unit.Destroyed)
            {
                return false;
            }

            if (!context.Scene.TriggerAreas.TryGetByName(areaName, out var area))
            {
                ScriptingSystem.Logger.Warn($"Trigger area \"{areaName}\" does not exist.");
                return false;
            }

            return area.Contains(unit.Transform.Translation.Vector2XY());
        }
    }
}
