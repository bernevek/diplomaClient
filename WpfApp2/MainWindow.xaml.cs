using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;
using WpfApp2.request;
using WpfApp2.service;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        // Fields
        private static String _session = null;
        private static long _id;
        private Requests requests = Requests.getRequests();
        private ApplicationService applicationService = ApplicationService.getApplicationService();

        public static string Session { get => _session; set => _session = value; }
        public static long id { get => _id; set => _id = value; }

        private const uint GENERIC_ALL = 0x1ff;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseWindowStation(IntPtr hWinSta);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateDesktop(string lpszDesktop, string lpszDevice, IntPtr pDevmode, uint dwFlags, uint dwDesiredAccess, IntPtr Zero);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr pcZero, IntPtr thZero, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowStation(string lpWinSta, uint dwFlags, uint acessMask, IntPtr zero);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumProc lpEnumProc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string cap, string cls);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetProcessWindowStation();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetThreadDesktop(int thId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder sb, int max);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SwitchDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr OpenInputDesktop(uint dwFlags, bool Inherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessWindowStation(IntPtr hWinSta);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetThreadDesktop(IntPtr hDesktop);


        // Nested Types
        private delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public MainWindow()
        {
            InitializeComponent();

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Lock.ico");
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //UserPortClient client = new UserPortClient("UserPortSoap11");
            //GetUserDetailsRequest getUserDetailsRequest = new GetUserDetailsRequest();
            //getUserDetailsRequest.id = 5;
            //GetUserDetailsResponse response = client.GetUserDetails(getUserDetailsRequest);
            //Console.WriteLine(response.UserDetails.id);
            //Console.WriteLine(response.UserDetails.firstName);
            //Console.WriteLine(response.UserDetails.lastName);


            //requests.logout(MainWindow.Session);
            MainWindow.Session = null;
            MainWindow.id = requests.sendComputerDetails();
            if (MainWindow.id != 0)
            {
                Thread myThread = new Thread(new ThreadStart(newDesktop));
                myThread.Start(); // запускаем поток
            }
        }

        private void newDesktop()
        {
            int num;
            string temp;
            IntPtr hObject = CreateDesktop("NewDesktop", null, IntPtr.Zero, 1, 0x1ff, IntPtr.Zero);
            if (!SUCCESS(hObject))
            {
                throw new Win32Exception(_geterr());
            }
            if (!SetThreadDesktop(hObject) && ((num = _geterr()) != 0))
            {
                throw new Win32Exception(num);
            }
            IntPtr hDesktop = OpenInputDesktop(1, false, 0x1ff);
            if (!SwitchDesktop(hObject) && ((num = _geterr()) != 0))
            {
                throw new Win32Exception(num);
            }

            //MessageBox.Show("bla bla");
            CustomMsgBox.Show(hDesktop, hObject, "bla bla", "MSG", "Close", "Send");


            //            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam) {
            //                if (!string.IsNullOrEmpty(temp = _getwincap(hWnd)))
            //                {
            //                    Console.WriteLine(temp);
            //                }
            //                return true;
            //            }, IntPtr.Zero);

            if (!SwitchDesktop(hDesktop) && ((num = _geterr()) != 0))
            {
                throw new Win32Exception(num);
            }
            else
            {
                Thread myThread = new Thread(new ThreadStart(periodicMessage));
                myThread.Start();
                Thread myThread2 = new Thread(new ThreadStart(trackUserActivity));
                myThread2.Start();
            }
        }

        private void periodicMessage()
        {
            int count = 0;
            requests.getAndApplyPolicy(MainWindow.Session);
            while (true)
            {
                if (MainWindow.Session != null)
                {
                    if (count > 300)
                    {
                        requests.getAndApplyPolicy(MainWindow.Session);
                        count = 0;
                    }
                }
                else
                {
                    break;
                }
                count++;
                Thread.Sleep(1000);
            }
        }

        private void trackUserActivity()
        {
            int count = 0;
            uint userActivity = GetLastUserInput.GetLastInputTime();
            while (true)
            {
                if (MainWindow.Session != null)
                {
                    if (userActivity != GetLastUserInput.GetLastInputTime())
                    {
                        userActivity = GetLastUserInput.GetLastInputTime();
                        count = 0;
                    }
                    else
                    {
                        if (count > 60)
                            Button_Click(null, null);
                    }
                }
                else
                {
                    break;
                }
                count++;
                Thread.Sleep(1000);
            }
        }

        private int _geterr()
        {
            return Marshal.GetLastWin32Error();
        }

        private string _getwincap(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(0xff);
            int length = GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString(0, length);
        }

        private void _shell(string file, string desktop)
        {
            STARTUPINFO startupinfo;
            PROCESS_INFORMATION process_information;
            int num;
            startupinfo = new STARTUPINFO
            {
                cb = Marshal.SizeOf(typeof(STARTUPINFO)),
                lpDesktop = desktop
            };
            if (!CreateProcess(file, null, IntPtr.Zero, IntPtr.Zero, false, 0x20, IntPtr.Zero, null, ref startupinfo, out process_information) && ((num = _geterr()) != 0))
            {
                throw new Win32Exception(num);
            }
        }

        private bool SUCCESS(IntPtr hObject)
        {
            return (hObject != IntPtr.Zero);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            MainWindow.Session = null;
        }
    }
}