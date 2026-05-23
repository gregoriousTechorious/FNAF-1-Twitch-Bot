using System.Runtime.InteropServices;

public static class CoordFinder
{
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    private const int VK_F1 = 0x70;
    private const int VK_F2 = 0x71;

    public static void Start()
    {
        Console.WriteLine("In FNAF, hover over a button and press F1 to record coords.");
        Console.WriteLine("Press F2 to finish.\n");

        while (true)
        {
            if ((GetAsyncKeyState(VK_F2) & 0x8000) != 0)
            {
                Console.WriteLine("Done recording.");
                break;
            }

            if ((GetAsyncKeyState(VK_F1) & 0x8000) != 0)
            {
                GetCursorPos(out POINT point);
                Console.WriteLine($"Recorded: X={point.X}, Y={point.Y}");
                Thread.Sleep(500); // debounce so one press doesn't record multiple times
            }

            Thread.Sleep(10);
        }
    }
}