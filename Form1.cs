using System.Runtime.InteropServices;

namespace CenterWindowGUI;

public partial class Form1 : Form
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromRect(ref Rect lprc, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
        uint uFlags);

    private const uint SwpNosize = 0x0001; // SWP_NOSIZE
    private const uint SwpNozorder = 0x0004; // SWP_NOZORDER
    private const uint SwpShowwindow = 0x0040; // SWP_SHOWWINDOW

    private const int VkAlt = 0x12; // Virtual key code for Alt
    private const int VkShift = 0x10; // Virtual key code for Shift
    private const int VkO = 0x4F; // Virtual key code for O

    private const uint MonitorDefaulttonearest = 0x00000002; // MONITOR_DEFAULTTONEAREST

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public Form1()
    {
        InitializeComponent();
        NotifyIcon notifyIcon = new()
        {
            Visible = true,
            Icon = SystemIcons.Application, 
            Text = "CenterWindowGUI",
            ContextMenuStrip = new ContextMenuStrip
            {
                Items =
                {
                    new ToolStripMenuItem("Exit", null, (sender, args) => Application.Exit())
                },
            }
        };

        WindowState = FormWindowState.Minimized;
        Hide();
        ShowInTaskbar = false;

        while (true)
        {
            var pressed = (GetAsyncKeyState(VkAlt) & 0x8000) != 0 &&
                          (GetAsyncKeyState(VkShift) & 0x8000) != 0 &&
                          (GetAsyncKeyState(VkO) & 0x8000) != 0;

            if (pressed)
            {
                var hWnd = GetForegroundWindow();
                CenterWindow(hWnd);
            }

            Thread.Sleep(50);
        }
    }

    private static void CenterWindow(IntPtr hWnd)
    {
        if (!GetWindowRect(hWnd, out var windowRect)) return;

        var hMonitor = MonitorFromRect(ref windowRect, MonitorDefaulttonearest);
        MonitorInfo monitorInfo = new()
        {
            cbSize = Marshal.SizeOf(typeof(MonitorInfo))
        };
        GetMonitorInfo(hMonitor, ref monitorInfo);

        var monitorWidth = monitorInfo.rcWork.Right - monitorInfo.rcWork.Left;
        var monitorHeight = monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top;

        var windowWidth = windowRect.Right - windowRect.Left;
        var windowHeight = windowRect.Bottom - windowRect.Top;

        var newX = monitorInfo.rcWork.Left + (monitorWidth - windowWidth) / 2;
        var newY = monitorInfo.rcWork.Top + (monitorHeight - windowHeight) / 2;

        SetWindowPos(hWnd, IntPtr.Zero, newX, newY, 0, 0, SwpNosize | SwpNozorder | SwpShowwindow);
    }
}