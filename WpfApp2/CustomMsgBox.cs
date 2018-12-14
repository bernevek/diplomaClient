using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WpfApp2.UserService;

namespace WpfApp2
{
    public partial class CustomMsgBox : Form
    {
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
            UserPortClient client = new UserPortClient("UserPortSoap11");
            GetUserDetailsRequest getUserDetailsRequest = new GetUserDetailsRequest();
            getUserDetailsRequest.id = 5;
            GetUserDetailsResponse response = client.GetUserDetails(getUserDetailsRequest);
            this.textBox1.Text = response.UserDetails.lastName;
            Console.WriteLine(response.UserDetails.lastName);
            close = true;
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
