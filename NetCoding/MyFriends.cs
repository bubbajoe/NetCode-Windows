using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetCoding
{
    public partial class MyFriends : Form
    {
        public MyFriends()
        {
            InitializeComponent();
            
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add("item");
            
        }

        private void MyProjects_Load(object sender, EventArgs e)
        {

        }
    }
}
