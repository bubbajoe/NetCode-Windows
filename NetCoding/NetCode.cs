using System;
using System.IO;
using System.Text;
using System.Drawing;
using NetcodeNetworking;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;


namespace NetCoding
{
    public partial class NetCode : Form
    {
        MyNotifications notificationWindow;
        Register registerWindow;
        Login loginWindow;
        public static Client me = new Client();

        public NetCode()
        {
            InitializeComponent();
            //ApplicationSession.OpenDefault();
        }
        
        private void FCTBTextChanged(object src, TextChangedEventArgs e)
        {
            var fctb = src as FastColoredTextBox;
            Console.WriteLine(e.ChangedRange.FromLine);
            Console.WriteLine(fctb.Lines[0] + " " + fctb);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tab = new TabPage("Untitled.cs");
            var fctb = new FastColoredTextBox();
            fctb.Dock = DockStyle.Fill;
            fctb.Language = Language.CSharp;
            fctb.Size = fctb.ClientSize;
            
            tab.Controls.Add(fctb);
            tabControl1.Controls.Add(tab);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            notificationWindow = new MyNotifications();
            notificationWindow.Show();
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
            ofd.Multiselect = true;

            ofd.DefaultExt = "txt";
            ofd.Filter = "Text files (*.txt)|*.txt |All files (*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                foreach (string path in ofd.FileNames)
                {
                    var tab = new TabPage(Path.GetFileName(path));
                    var fctb = new FastColoredTextBox();
                    fctb.BackColor = fctb.IndentBackColor = Color.FromArgb(30, 30, 30);
                    fctb.ForeColor = Color.White;
                    fctb.Dock = DockStyle.Fill;

                    string ext = Path.GetExtension(path);
                    switch (ext)
                    {
                        case ".cs":
                            fctb.Language = Language.CSharp;
                            break;
                        case ".js":
                            fctb.Language = Language.JS;
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
                    fctb.TextChanged += FCTBTextChanged;
                    fctb.OpenFile(path);
                    tab.Controls.Add(fctb);
                    tabControl1.Controls.Add(tab);
                }
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

            //File.WriteAllText(sfd.FileName,fctb.Text);
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

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loginWindow = new Login();
            loginWindow.Show();
        }

        public static void Login(string username, string password)
        {
            me.LoginRequest(username, password);

        }

        public static void Register(string username, string password)
        {


        }

        private void registerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            registerWindow = new Register();
            registerWindow.Show();
        }

        private void openSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.InitialDirectory = @"~";
            ofd.Title = "Open File";

            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            ofd.DefaultExt = "netcode";
            ofd.Filter = "NetCode Session File (*.netcode)|*.netcode| All files (*.*)|*.*";
            ofd.FilterIndex = 2;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Session s = new Session(tabControl1);
                s = ApplicationSession.Open(ofd.FileName, s);
                tabControl1 = s.AddTabs(tabControl1);
            }
        }

        private void saveSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();

            sfd.DefaultExt = "netcode";
            sfd.Filter = "NetCode Session File (*.netcode)|*.netcode| All files (*.*)|*.*";
            sfd.FilterIndex = 2;

            FastColoredTextBox fctb;
            if (tabControl1.HasChildren)
                fctb = (FastColoredTextBox)tabControl1.SelectedTab.Controls[0];

            else return;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Session s = new Session(tabControl1);
                ApplicationSession.Write(sfd.FileName, s);
            }
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.Controls.Clear();
        }
    }

    [Serializable()]
    public class Session : ISerializable
    {
        public List<TabPage> tpl;
        private TabControl tc;

        public Session()
        {
            tpl = new List<TabPage>();
        }

        public Session(TabControl tc)
        {
            this.tc = tc;
            tpl = new List<TabPage>();
        }
/*
        private List<A> Load()
        {
            string file = "filepath";
            List<A> listofa = new List<A>();
            XmlSerializer formatter = new XmlSerializer(A.GetType());
            FileStream aFile = new FileStream(file, FileMode.Open);
            byte[] buffer = new byte[aFile.Length];
            aFile.Read(buffer, 0, (int)aFile.Length);
            MemoryStream stream = new MemoryStream(buffer);
            return (List<A>)formatter.Deserialize(stream);
        }

        
        private void Save(List<T> listofa)
        {
            string path = "filepath";
            FileStream outFile = File.Create(path);
            XmlSerializer formatter = new XmlSerializer(T.GetType());
            formatter.Serialize(outFile, listofa);
        }
*/
        public Session(SerializationInfo info, StreamingContext context) : this()
        {
            FastColoredTextBox fctb;
            TabPage tb;
            int count = Convert.ToInt32(info.GetValue("NumTabs", typeof(int)));
            for (int i = 0; i < count; i++)
            {
                fctb = new FastColoredTextBox();
                tb = new TabPage(Convert.ToString(info.GetValue("FCTB Title"+i,typeof(string))));
                fctb.Text = Convert.ToString(info.GetValue("FCTB Text"+i,typeof(string)));
                fctb.Language = (Language)
                    Convert.ToByte(info.GetValue("FCTB Language"+i,typeof(byte)));
                fctb.BackColor = fctb.IndentBackColor = Color.FromArgb(30, 30, 30);
                fctb.ForeColor = Color.White;
                fctb.Dock = DockStyle.Fill;

                tb.Controls.Add(fctb);
                tpl.Add(tb);
            }
        }
        
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FastColoredTextBox fctb;
            info.AddValue("NumTabs", tc.Controls.Count);

            for(int i = 0; i < tc.Controls.Count; i++)
            {
                fctb = tc.Controls[i].Controls[0] as FastColoredTextBox;
                info.AddValue("FCTB Title" + i, tc.TabPages[i].Name);
                info.AddValue("FCTB Language" + i, (byte)fctb.Language);
                info.AddValue("FCTB Text"+i, fctb.Text);
            }
        }

        public TabControl AddTabs(TabControl tc1)
        {
            foreach(TabPage tp in tpl)
            {
                tc1.Controls.Add(tp);
            }

            return tc1;
        }
    }

    public class ApplicationSession
    {
        public static Session OpenDefault()
        {
            var path = @"~/default-session.netcode";
            Stream s;
            try
            {
                s = new MemoryStream(AES.Decrypt(File.ReadAllBytes(path)));
            } catch
            {
                return null;
            }
            var bf = new BinaryFormatter();
            Session sess = (Session)bf.Deserialize(s);
            //try { } catch { return null; }
            s.Close();

            return sess;
        }

        public static void WriteDefault(Session s)
        {
            var path = @"~/default-session.netcode";
            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(ms, s);
            File.WriteAllBytes(path, AES.Encrypt(ms.ToArray()));
            var f = new FileInfo(path).Attributes = FileAttributes.Hidden;
            
            ms.Close();
        }

        public static Session Open(string path, Session sess)
        {
            Stream s = new MemoryStream(AES.Decrypt(File.ReadAllBytes(path)));
            var bf = new BinaryFormatter();
            sess = (Session)bf.Deserialize(s);
            //try { } catch { return null; }
            s.Close();

            return sess;
        }

        public static void Write(string path, Session ss)
        {
            var fs = new FileStream(path,FileMode.Create);
            var bf = new BinaryFormatter();
            bf.Serialize(fs, ss);
            var f = new FileInfo(path).Attributes = FileAttributes.Hidden;

            fs.Close();
        }
    }

    class AES
    {
        private static string IV = "net-knL5dSS-code";
        private static string Key = "net-WbzjMXvTQYjKJDbyMHtIsLO-code";

        public static byte[] Encrypt(byte[] buf)
        {
            AesCryptoServiceProvider ed = new AesCryptoServiceProvider();
            ed.BlockSize = 128;
            ed.KeySize = 256;
            ed.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            ed.IV = ASCIIEncoding.ASCII.GetBytes(IV);
            ed.Padding = PaddingMode.PKCS7;
            ed.Mode = CipherMode.CBC;

            using (ICryptoTransform e = ed.CreateEncryptor())
            {

                return e.TransformFinalBlock(buf, 0, buf.Length);
            }
        }

        public static byte[] Decrypt(byte[] buf)
        {
            AesCryptoServiceProvider ed = new AesCryptoServiceProvider();
            ed.BlockSize = 128;
            ed.KeySize = 256;
            ed.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            ed.IV = ASCIIEncoding.ASCII.GetBytes(IV);
            ed.Padding = PaddingMode.PKCS7;
            ed.Mode = CipherMode.CBC;

            using (ICryptoTransform d = ed.CreateDecryptor())
            {
                return d.TransformFinalBlock(buf, 0, buf.Length);
            }
        }
    }
}
