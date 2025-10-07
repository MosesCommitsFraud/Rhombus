using System.Windows;
using Rhombus.App.ViewModels;
using System.Windows.Shell;
using System.Windows.Interop;



namespace Rhombus.App
{
    public partial class MainWindow : Window
    {
        public MainWindow(SoundboardViewModel vm)
        {

            InitializeComponent();
            DataContext = vm;

            // Navigate to Soundboard Page on Startup
            MainFrame.Navigate(new SoundboardPage());
            SoundboardButton.IsChecked = true;

            // Used for resize logic
            var chrome = new WindowChrome
            {
                ResizeBorderThickness = new Thickness(5),
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(0),
                GlassFrameThickness = new Thickness(0),
                UseAeroCaptionButtons = false
            };
            WindowChrome.SetWindowChrome(this, chrome);
        }

        // Window resizing
        const int WM_NCHITTEST = 0x84;
        const int HTLEFT = 10;
        const int HTRIGHT = 11;
        const int HTTOP = 12;
        const int HTTOPLEFT = 13;
        const int HTTOPRIGHT = 14;
        const int HTBOTTOM = 15;
        const int HTBOTTOMLEFT = 16;
        const int HTBOTTOMRIGHT = 17;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            if (source != null)
                source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == WM_NCHITTEST)
            {
                int x = (short)(lParam.ToInt32() & 0xFFFF);
                int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

                var screenPoint = new System.Windows.Point(x, y);
                var windowPoint = this.PointFromScreen(screenPoint);
                double width = this.ActualWidth;
                double height = this.ActualHeight;
                const int grip = 8; // pixels

                if (windowPoint.Y >= 0 && windowPoint.Y < grip)
                {
                    if (windowPoint.X >= 0 && windowPoint.X < grip)
                    {
                        handled = true;
                        return (IntPtr)HTTOPLEFT;
                    }
                    
                    if (windowPoint.X <= width && windowPoint.X > width - grip)
                    {
                        handled = true;
                        return (IntPtr)HTTOPRIGHT;
                    }
                    handled = true;
                    return (IntPtr)HTTOP;
                }
                
                if (windowPoint.Y <= height && windowPoint.Y > height - grip)
                {
                    if (windowPoint.X >= 0 && windowPoint.X < grip)
                    {
                        handled = true;
                        return (IntPtr)HTBOTTOMLEFT;
                    }

                    if (windowPoint.X <= width && windowPoint.X > width - grip)
                    {
                        handled = true;
                        return (IntPtr)HTBOTTOMRIGHT;
                    }
                    handled = true;
                    return (IntPtr)HTBOTTOM;
                }

                if (windowPoint.X >= 0 && windowPoint.X < grip)
                {
                    handled = true;
                    return (IntPtr)HTLEFT;
                }

                if (windowPoint.X <= width && windowPoint.X > width - grip)
                {
                    handled = true;
                    return (IntPtr)HTRIGHT;
                }
            }

            return IntPtr.Zero;
        }
        
        // Page changing
        // Go to Downloader
        private void Downloader_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DownloaderPage());
        }

        // Go to Soundboard
        private void Soundboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SoundboardPage());
        }

        // Go to options
        private void Options_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OptionsPage());
        }

        // Top right buttons
        // Minimize window
        private void Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Toggle Maximize Window
        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        // Quit Window
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Window dragging
        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double click to maximize/restore
                Max_Click(sender, e);
            }
            else
            {
                // Single click to drag
                this.DragMove();
            }
        }
    }
}
