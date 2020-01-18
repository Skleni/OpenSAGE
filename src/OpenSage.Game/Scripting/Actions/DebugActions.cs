using System.Diagnostics;
using OpenSage.Data.Map;

namespace OpenSage.Scripting.Actions
{
    public static class DebugActions
    {
        public static ActionResult DebugString(ScriptAction action, ScriptExecutionContext context)
        {
            var text = action.Arguments[0].StringValue;
            Debug.WriteLine(text);

            return ActionResult.Finished;
        }
    }
}
