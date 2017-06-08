using System;
using NetCoding;
using System.Windows.Forms;

namespace NetCoding
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            NetCode.Login(pwTxt.Text,userTxt.Text);
        }
    }
}
