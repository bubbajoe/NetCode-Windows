using System;
using System.Drawing;
using FastColoredTextBoxNS;
using System.Windows.Forms;

namespace NetCoding
{
    public partial class ColorPicker : Form
    {
        TabControl tabControl1;

        public ColorPicker(TabControl tc)
        {
            InitializeComponent();
            this.tabControl1 = tc;
        }

        private Color PickColor(Color color)
        {
            var cd = new ColorDialog();
            cd.AnyColor = false;

            if (cd.ShowDialog() == DialogResult.OK)
            {
                return cd.Color;
                //MessageBox.Show("Color set to " + c.ToString());
            }
            else return color;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FastColoredTextBox tb;
            if (tabControl1.HasChildren)
                tb = tabControl1.SelectedTab.Controls[0] as FastColoredTextBox;
            else return;
            tb.BackColor = PickColor(tb.BackColor);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FastColoredTextBox tb;
            if (tabControl1.HasChildren)
                tb = tabControl1.SelectedTab.Controls[0] as FastColoredTextBox;
            else return;
            tb.LineNumberColor = PickColor(tb.LineNumberColor);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FastColoredTextBox tb;
            if (tabControl1.HasChildren)
                tb = tabControl1.SelectedTab.Controls[0] as FastColoredTextBox;
            else return;
            tb.ForeColor = PickColor(tb.ForeColor);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FastColoredTextBox tb;
            if (tabControl1.HasChildren)
                tb = tabControl1.SelectedTab.Controls[0] as FastColoredTextBox;
            else return;
            tb.IndentBackColor = PickColor(tb.IndentBackColor);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FastColoredTextBox tb;
            if (tabControl1.HasChildren)
                tb = tabControl1.SelectedTab.Controls[0] as FastColoredTextBox;
            else return;
            tb.SelectionColor = PickColor(tb.SelectionColor);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FastColoredTextBox tb;
            if (tabControl1.HasChildren)
                tb = tabControl1.SelectedTab.Controls[0] as FastColoredTextBox;
            else return;
            tb.CurrentLineColor = PickColor(tb.CurrentLineColor);
        }

        private void ColorPicker_Load(object sender, EventArgs e)
        {
            foreach (string s in Enum.GetNames(typeof(Language)))
                comboBox1.Items.Add(s);

            FastColoredTextBox tb;
            if (tabControl1.HasChildren)
                tb = tabControl1.SelectedTab.Controls[0] as FastColoredTextBox;
            else return;

            comboBox1.SelectedText = Enum.GetName(typeof(Language), tb.Language);
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FastColoredTextBox tb;
            if (tabControl1.HasChildren)
                tb = tabControl1.SelectedTab.Controls[0] as FastColoredTextBox;
            else return;
            
            tb.Language = (Language)comboBox1.SelectedIndex;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
