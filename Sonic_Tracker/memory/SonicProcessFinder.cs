using System.Diagnostics;

namespace Sonic_Tracker.Memory;

public static class SonicProcessFinder
{
    public static Process? Find()
    {
        Process[] processes = Process.GetProcessesByName("SonicOrigins");

        if (processes.Length == 0)
        {
            return null;
        }

        return processes[0];
    }
}