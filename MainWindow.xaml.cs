using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Drawing;
using Application = System.Windows.Application; // Avoid conflict with WinForms

namespace LiveMTLApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Flag to track overlay state
    private bool _isOverlaying = true;

    // NotifyIcon for system tray
    private NotifyIcon? _notifyIcon;

    // Win32 constants
    const int GWL_EXSTYLE = -20;
    const int WS_EX_TRANSPARENT = 0x00000020;
    const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public MainWindow()
    {
        InitializeComponent();
        SetupTrayIcon();
    }

    private void SetupTrayIcon()
    {
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Icon = SystemIcons.Application; // You can load your own .ico here
        _notifyIcon.Visible = true;
        _notifyIcon.Text = "LiveMTLApp";

        // Create the context menu initially
        UpdateContextMenu();

        // Double-click tray icon to toggle window visibility
        _notifyIcon.DoubleClick += (s, e) => ToggleWindow();
    }

    private void UpdateContextMenu()
    {
        var contextMenu = new ContextMenuStrip();
        
        if (_isOverlaying)
        {
            contextMenu.Items.Add("Hide Overlay", null, (s, e) => HideWindow());
        }
        else
        {
            contextMenu.Items.Add("Show Overlay", null, (s, e) => ShowWindow());
        }
        
        contextMenu.Items.Add("Exit", null, (s, e) => ExitApp());
        if (_notifyIcon != null)
        {
            _notifyIcon.ContextMenuStrip = contextMenu;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    private void ShowWindow()
    {
        this.Show();
        this.WindowState = WindowState.Maximized; // Changed from Normal to maintain fullscreen
        this.Activate();
        _isOverlaying = true;
        UpdateContextMenu(); // Update menu after state change
    }

    private void HideWindow()
    {
        this.Hide();
        _isOverlaying = false;
        UpdateContextMenu(); // Update menu after state change
    }

    private void ToggleWindow()
    {
        if (_isOverlaying)
        {
            HideWindow();
        }
        else
        {
            ShowWindow();
        }
    }

    private void ExitApp()
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
        Application.Current.Shutdown();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
        base.OnClosed(e);
    }
}