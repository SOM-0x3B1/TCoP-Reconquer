using System;
using System.Runtime.InteropServices;
using System.Threading;
/* from: 
 * https://www.codegrepper.com/code-examples/csharp/disable+quickedit+c%23
 * https://stackoverflow.com/questions/24110600/transparent-console-dllimport
 */

static class AdvancedDisplay
{
    const uint ENABLE_QUICK_EDIT = 0x0040;

    // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
    const int STD_INPUT_HANDLE = -10;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);


    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetConsoleWindow();


    private static int GWL_EXSTYLE = -20;
    private static int WS_EX_LAYERED = 0x80000;
    private static uint LWA_ALPHA = 0x2;
    // Obtain our handle (hWnd)
    private static IntPtr Handle;


    internal static bool DisableConsoleQuickEdit()
    {
        IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);
        // get current console mode
        uint consoleMode;
        if (!GetConsoleMode(consoleHandle, out consoleMode))
            return false; // ERROR: Unable to get console mode.

        // Clear the quick edit bit in the mode flags
        consoleMode &= ~ENABLE_QUICK_EDIT;

        // set the new mode
        if (!SetConsoleMode(consoleHandle, consoleMode))
            return false; // ERROR: Unable to set console mode    

        return true;
    }

    public static void SetOpacity(byte opacity)
    {
        //int LWA_COLORKEY = 0x1;
        // Opacity = 0.5 = (255/2)
        SetLayeredWindowAttributes(Handle, 0, opacity, LWA_ALPHA);
    }

    public static void SetWindowLong()
    {
        Handle = GetConsoleWindow();
        for (int i = 0; i < 2; i++)
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) ^ WS_EX_LAYERED);
    }

    public static void FadeIn(int steps)
    {
        for (int i = 0; i < 255; i += steps)
        {
            SetOpacity(Convert.ToByte(i));
            Thread.Sleep(1);
        }
    }
    public static void FadeOut(int steps)
    {
        for (int i = 255; i > 0; i -= steps)
        {
            SetOpacity(Convert.ToByte(i));
            Thread.Sleep(1);
        }
    }
}