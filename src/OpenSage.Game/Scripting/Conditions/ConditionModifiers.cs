namespace OpenSage.Scripting.Conditions
{
    internal static class ConditionModifiers
    {
        public static ScriptingCondition Not(ScriptingCondition scriptingCondition) =>
            (condition, context) => !scriptingCondition(condition, context);

        public static ScriptingCondition ValueChangedTo(bool value, ScriptingCondition scriptingCondition, bool initialValue)
        {
            var previousValue = initialValue;
            return (condition, context) =>
            {
                var currentValue = scriptingCondition(condition, context);
                var result = previousValue != value && currentValue == value;
                previousValue = currentValue;
                return result;
            };
        }
    }
}
