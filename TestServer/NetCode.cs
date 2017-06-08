using System;
using System.Collections.Generic;
using System.Linq;
using FastColoredTextBoxNS;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace NetCoding
{
    public partial class NetCode : Form
    {
        MyNotifications notificationWindow;
        TabPage currentTab;

        public NetCode()
        {
            InitializeComponent();
            
            tabControl1.SelectedIndexChanged += TabChanged;
        }

        protected void TabChanged(object sender, EventArgs e)
        {
            currentTab = tabControl1.SelectedTab;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tab = new TabPage("Untitled.cs");
            var fctb = new FastColoredTextBox();
            fctb.Dock = DockStyle.Fill;
            fctb.Language = Language.CSharp;

            currentTab = tab;
            tab.Controls.Add(fctb);
            tabControl1.Controls.Add(tab);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            notificationWindow = new MyNotifications();
            notificationWindow.Show();
            //if(!notificationWindow.Visible) notificationWindow.Visible = true;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox fctb;
            if (tabControl1.HasChildren)
                fctb = (FastColoredTextBox)tabControl1.SelectedTab.Controls[0];
            else return;
            if(fctb.UndoEnabled)
                fctb.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox fctb;
            if (tabControl1.HasChildren)
                fctb = (FastColoredTextBox)tabControl1.SelectedTab.Controls[0];
            else return;
            if (fctb.RedoEnabled)
                fctb.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox fctb;
            if (tabControl1.HasChildren)
                fctb = (FastColoredTextBox)tabControl1.SelectedTab.Controls[0];
            else return;
            if (fctb.RedoEnabled)
                fctb.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox fctb;
            if (tabControl1.HasChildren)
                fctb = (FastColoredTextBox)tabControl1.SelectedTab.Controls[0];
            else return;
            if (fctb.RedoEnabled)
                fctb.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FastColoredTextBox fctb;
            if (tabControl1.HasChildren)
                fctb = (FastColoredTextBox)tabControl1.SelectedTab.Controls[0];
            else return;
            if (fctb.RedoEnabled)
                fctb.Paste();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = @"~";
            ofd.Title = "Open File";

            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            ofd.DefaultExt = "txt";
            ofd.Filter = "Text files (*.txt)|*.txt |All files (*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;
            
            ofd.ShowReadOnly = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var tab = new TabPage(Path.GetFileName(ofd.FileName));
                var fctb = new FastColoredTextBox();
                fctb.BackColor = fctb.IndentBackColor = Color.FromArgb(30,30,30);
                fctb.ForeColor = Color.White;
                fctb.Dock = DockStyle.Fill;
                string ext = Path.GetExtension(ofd.FileName);
                Console.WriteLine(ext);
                switch (ext)
                {
                    case ".cs":
                        fctb.Language = Language.CSharp;
                        break;
                    case ".html":
                        fctb.Language = Language.HTML;
                        break;
                    case ".vb":
                        fctb.Language = Language.VB;
                        break;
                    case ".lua":
                        fctb.Language = Language.Lua;
                        break;
                    case ".php":
                        fctb.Language = Language.PHP;
                        break;
                    case ".sql":
                        fctb.Language = Language.SQL;
                        break;
                    case ".xml":
                        fctb.Language = Language.XML;
                        break;
                    default:
                        fctb.Language = Language.Custom;
                        break;
                }

                //if (fctb.Language != Language.Custom)
                fctb.Text = File.ReadAllText(ofd.FileName);

                currentTab = tab;
                tab.Controls.Add(fctb);
                tabControl1.Controls.Add(tab);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Text files (*.txt)|*.txt |All files (*.*)|*.*";

            FastColoredTextBox fctb;
            if (tabControl1.HasChildren)
                fctb = (FastColoredTextBox)tabControl1.SelectedTab.Controls[0];
            else return;

            File.WriteAllText(sfd.FileName,fctb.Text);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Text files (*.txt)|*.txt |All files (*.*)|*.*";

            FastColoredTextBox fctb;
            if (tabControl1.HasChildren)
                fctb = (FastColoredTextBox)tabControl1.SelectedTab.Controls[0];
            else return;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, fctb.Text);
            }
        }
    }
}
