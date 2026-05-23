using System.Runtime.InteropServices;

public static class WinInput
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;

    public static void Click(int x, int y)
    {
        SetCursorPos(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
        Thread.Sleep(50);
        mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
    }

    public static void MoveTo(int x, int y)
    {
        SetCursorPos(x, y);
    }
}