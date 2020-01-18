﻿using System.Collections.Generic;
using OpenSage.Data.Map;

namespace OpenSage.Scripting.Conditions
{
    public static class ConditionLookup
    {
        private static readonly Dictionary<ScriptConditionType, ScriptingCondition> Conditions = new Dictionary<ScriptConditionType, ScriptingCondition>
        {
            { ScriptConditionType.False, MiscConditions.False},
            { ScriptConditionType.Flag, MiscConditions.Flag },
            { ScriptConditionType.True, MiscConditions.True },
            { ScriptConditionType.TimerExpired, CounterAndTimerConditions.TimerExpired },
            { ScriptConditionType.Counter, CounterAndTimerConditions.Counter },
            { ScriptConditionType.NamedInsideArea, UnitConditions.NamedInsideArea }
        };

        public static ScriptingCondition Get(ScriptCondition condition)
        {
            if (!Conditions.TryGetValue(condition.ContentType, out var conditionFunction))
            {
                // TODO: Implement this condition type.
                return MiscConditions.False;
            }

            return conditionFunction;
        }
    }
}
