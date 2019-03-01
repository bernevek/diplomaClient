using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WpfApp2.isecurity;
using WpfApp2.request;
using System.Diagnostics;


namespace WpfApp2
{
    public partial class CustomMsgBox : Form
    {
        private Requests requests = Requests.getRequests();
        public CustomMsgBox()
        {
            InitializeComponent();
        }
        static CustomMsgBox msgBox; static DialogResult result = DialogResult.No;
        static IntPtr mainDesktop;
        static IntPtr securityDesktop;
        private Boolean close = false;
        public static DialogResult Show(IntPtr mainDesktop, IntPtr securityDesktop, string text, string caption, string btnOk, string btnCancel)
        {
            CustomMsgBox.mainDesktop = mainDesktop;
            CustomMsgBox.securityDesktop = securityDesktop;
            msgBox = new CustomMsgBox();
            msgBox.label1.Text = text;
            msgBox.button1.Text = btnOk;
            msgBox.ShowDialog();
            return result;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            SecurityClient client = new SecurityClient("ISecuritySoap11");
            
            LoginRequest loginRequest = new LoginRequest();
            loginRequest.computerId = 1;
            loginRequest.login = this.textBox1.Text;
            loginRequest.password = this.textBox2.Text;

            try
            {
                LoginResponse response = client.Login(loginRequest);
                MainWindow.Session = response.session;
            }
            catch (Exception ex)
            {
                return;
            }
            close = true;
//            try
//            {
//                Process[] proc = Process.GetProcessesByName("winword");
//                foreach (Process element in proc)
//                {
//                    Console.WriteLine(proc.Length);
//                    element.Kill();
//                }
//            }
//            catch (Exception ex)
//            {
//            }
            msgBox.Close();
        }
        private void CustomMsgBox_Closing(object sender, FormClosingEventArgs e)
        {
            if (!close)
            {
                e.Cancel = true;
            }
        }
    }
}
