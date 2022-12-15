using System.Diagnostics;
using EnsureThat;
using OneBeyond.Studio.Crosscuts.Strings;

namespace OneBeyond.Studio.Crosscuts.Activities;

public static class ActivityExtensions
{
    public static Activity DoStart(this Activity activity, string? parentId, string? parentTraceState)
    {
        EnsureArg.IsNotNull(activity, nameof(activity));

        if (!parentId.IsNullOrWhiteSpace())
        {
            activity.SetParentId(parentId);
            activity.TraceStateString = parentTraceState;
        }
        return activity.Start();
    }

    public static Activity DoStart(this Activity activity)
        => activity.DoStart(parentId: default, parentTraceState: default);
}
