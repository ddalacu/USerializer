using UnityEngine;
using log4net.Appender;
using log4net.Core;
using UnityEngine.Scripting;

[Preserve]
public class UnityDebugAppender : AppenderSkeleton
{
    protected override void Append(LoggingEvent loggingEvent)
    {
        var message = RenderLoggingEvent(loggingEvent);
        if (loggingEvent.Level < Level.Warn)
            Debug.Log(message);
        else
        if (loggingEvent.Level < Level.Error)
            Debug.LogWarning(message);
        else
            Debug.LogError(message);
    }
}