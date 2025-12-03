#if UNITY_STANDALONE
using System.Runtime.InteropServices;

public static class SystemSleepBlocker
{
    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint state);

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    /// <summary>
    /// Call with true to prevent system sleep / display off; call with false to clear request.
    /// </summary>
    public static void BlockSleep(bool enable)
    {
        if (enable)
        {
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
        }
        else
        {
            SetThreadExecutionState(ES_CONTINUOUS);
        }
    }
}
#endif
