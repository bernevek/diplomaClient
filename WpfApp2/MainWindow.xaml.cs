using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        // Fields
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
            Thread myThread = new Thread(new ThreadStart(newDesktop));
            myThread.Start(); // запускаем поток
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
 //           if (!SetThreadDesktop(hDesktop) && ((num = _geterr()) != 0))
 //           {
 //               throw new Win32Exception(num);
 //           }
 //           if (!CloseDesktop(hObject) && ((num = _geterr()) != 0))
 //               throw new Win32Exception(num);
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

    }
}