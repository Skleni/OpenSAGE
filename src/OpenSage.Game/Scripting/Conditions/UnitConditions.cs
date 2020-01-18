using System;
using System.Collections.Generic;
using System.Text;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Scripting.Conditions
{
    private class RayDirection
    {
        Ray
    }

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
            var unitPosition = unit.Transform.Translation;


            if (!context.Scripting.Flags.TryGetValue(flagName, out var flagValue))
            {
                return false;
            }

            return flagValue == comparedFlagValue;
        }
    }
}
