using System;
using System.IO;
using System.Drawing;
using System.Net.Sockets;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;


namespace NetcodeNetworking
{
    public enum Header : byte
    {
        Undefined,
        Message,
        NetCode,
        Application,
        Account,
        Friend,
        Error,
        Admin
    }

    public enum Subheader : byte
    {
        NULL,

        // Message
        TEXTMSG,
        IMGMSG,
        FILEMSG,

        // Netcode
        LIVECODE,
        INVUSR,
        JOINUSR,
        KCKUSR,
        BANUSER,
        EDTAUTH,
        LIVENOTE,

        // Application
        UPDTCLIENT,
        CHECKKUPDT,
        REPORTBUG,
        PING,

        // Account
        LOGIN,
        REGISTER,
        USERNAME,
        CHANGEPASS,
        USERINFO,
        LOGINSUCCESS,
        MYUSERNAME,

        // Friends
        ADDUSR,
        REMOVEUSR,
        BLOCKUSR,
        ACC_REQ,
        DEC_REQ,
        VIEWFRIENDS,

        // Admin
        GETNUMCLIENTS,
        GETUPTIME,

        // Error
        NO_SUCH_USR,
        INVALIDREQ,
        INVALIDUSR,
        DBCON_ERROR,
        DBQRY_ERROR,
        DBREG_ERROR,
        DBCHG_ERROR,
        DBEXST_ERROR,
        SESSIONLOST,
        MSGNOTSENT,
        MSG_ERROR,
        NETC_ERROR,
        APP_ERROR,
        ACC_ERROR,
        FRIEND_ERROR,
        PROJ_ERROR,
        UNAUTH_ERROR
    }

    public class State
    {
        public Socket socket;
        public byte[] tempBuffer;

        private int max_size;

        public State()
        {
        }

        public State(int size, Socket sock)
        {
            tempBuffer = new byte[size];
            max_size = tempBuffer.Length;
            socket = sock;
        }
    }
    
    public class UsernameList
    {
        private List<string> usernames;
        
        private UsernameList(List<String> list)
        {
            usernames = list;
        }

        [Serializable]
        private struct UList
        {
            public List<string> list;

            public UList(List<String> list)
            {
                this.list = list;
            }
        }

        public int Count
        {
            get { return usernames.Count; }
        }

        public void Add(string username)
        {
            this.usernames.Add(username);
        }

        public void Remove(string username)
        {
            this.usernames.Remove(username);
        }

        public string[] ToArray()
        {
            return this.usernames.ToArray();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return usernames.GetEnumerator();
        }

        public void Serialize(ref PacketWriter pw)
        {
            pw.WriteObject(new UList(usernames));
        }

        public static UsernameList Deserialize(ref PacketReader pr)
        {
            return new UsernameList(pr.ReadObject<UList>().list);
        }

    }

    [Serializable]
    public class FileInformation
    {
        public string name;
        public byte[] buffer;
        
        public FileInformation(string n, byte[] b)
        {
            name = n;
            buffer = b;
        }

        public void CreateFile(string path)
        {
            System.IO.File.WriteAllBytes(path + name, buffer);
        }
    }

    [Serializable]
    public struct UserInformation
    {
        public string user_id;
        public string username;
        public string password;
        public string newpassword;

        public UserInformation(string u, string p)
        {
            username = u;
            password = p;
            newpassword = "";
            user_id = "";
        }

        public UserInformation(string u, string p, string np)
        {
            username = u;
            password = p;
            newpassword = np;
            user_id = "";
        }

        public UserInformation(string u, string p, string np, string g)
        {
            username = u;
            password = p;
            newpassword = np;
            user_id = g;
        }
    }

    [Serializable]
    public struct SessionInformation
    {
        public string user_id;
        public string username;

        public SessionInformation(string u, string i)
        {
            username = u;
            user_id = i;
        }
    }

    [Serializable]
    public struct ProjectInformation
    {
        public string project_name;
        public string project_id;
        public string project_user_id;
        public ProjectType project_status;
        public List<ProjectUser> users;

        public ProjectInformation(string name, IEnumerable<ProjectUser> pUsers)
        {
            project_name = name;
            project_id = Guid.NewGuid().ToString();
            project_user_id = Guid.NewGuid().ToString();
            project_status = ProjectType.PUBLIC;
            users = new List<ProjectUser>(pUsers);
        }

        public ProjectInformation(string name, string id, string pu_ud, string status)
        {
            project_name = name;
            project_id = id;
            project_user_id = pu_ud;
            Enum.TryParse(status, out project_status);
            users = new List<ProjectUser>();
        }
        public IEnumerator<ProjectUser> GetEnumerator()
        {
            return users.GetEnumerator();
        }
    }

    [Serializable]
    public struct ProjectUser
    {
        public string user_id;
        public ProjectRole role;

        public ProjectUser(string uid, ProjectRole r)
        {
            user_id = uid;
            role = r;
        }
    }

    [Serializable]
    public enum ProjectRole : byte
    {
        Owner,
        Editor,
        Viewer
    }

    [Serializable]
    public enum ProjectType : byte
    {
        PUBLIC,
        PRIVATE
    }

    [Serializable]
    public struct ProjectFileInformation
    {
        public string file_name;
        public string file_id;
        public byte[] file_data;
        public string project_id;

        public ProjectFileInformation(string f_name, string proj_id, byte[] f_data)
        {
            file_name = f_name;
            file_id = Guid.NewGuid().ToString();
            project_id = proj_id;
            file_data = f_data;
        }

        public ProjectFileInformation(string f_name, string f_id)
        {
            file_name = f_name;
            file_id = f_id;
            project_id = null;
            file_data = null;
        }
    }

    [Serializable]
    public struct FriendInformation
    {
        public string friend_id;
        public string recieved_user_id;
        public string requested_user_id;
        public FriendStatus friend_status;

        public FriendInformation(string recv_user, string req_user)
        {
            recieved_user_id = recv_user;
            friend_id = Guid.NewGuid().ToString();
            requested_user_id = req_user;
            friend_status = FriendStatus.PENDING;
        }

        public FriendInformation(string recv_user, string req_user, string f_id, FriendStatus status)
        {
            recieved_user_id = recv_user;
            friend_id = f_id;
            requested_user_id = req_user;
            friend_status = status;
        }
    }

    [Serializable]
    public enum FriendStatus
    {
        PENDING,
        ACCEPTED,
        BLOCKED
    }

    public class PacketWriter : BinaryWriter
    {
        private MemoryStream ms;
        private BinaryFormatter bf;

        public PacketWriter() : base()
        {
            ms = new MemoryStream();
            bf = new BinaryFormatter();

            OutStream = ms;
        }

        public void WriteInt(int i)
        {
            base.Write(i);
        }

        public void WriteByte(byte b)
        {
            base.Write(b);
        }

        public void WriteByte(byte[] b)
        {
            base.Write(b);
        }

        public void WriteString(string s)
        {
            base.Write(s);
        }

        public void Write(Image image,
            ImageFormat format)
        {
            var ms = new MemoryStream();

            image.Save(ms, format);
            ms.Close();

            byte[] b = ms.ToArray();

            Write(b.Length);
            Write(b);
        }

        public void WriteObject(object obj)
        {
            bf.Serialize(ms, obj);
        }

        public void Write(Image image)
        {
            Write(image, ImageFormat.Png);
        }

        public byte[] GetBytes(int size)
        {
            Close();
            byte[] a = new byte[size];
            Array.Copy(ms.ToArray(), a, ms.ToArray().Length);

            return a;
        }

    }

    public class PacketReader : BinaryReader
    {
        private BinaryFormatter bf;
        private byte[] preBuffer;

        public byte[] Buffer
        {
            get
            {
                return preBuffer;
            }
        }

        public PacketReader(byte[] data) :
            base(new MemoryStream(data))
        {
            preBuffer = data;
            bf = new BinaryFormatter();
        }

        public Image ReadImage()
        {
            int len = ReadInt32();

            Image image;
            using (var ms = new MemoryStream(ReadBytes(len)))
            {
                image = Image.FromStream(ms);
            }

            return image;
        }

        public void CreateFile(string path)
        {
            int len = ReadInt32();
            System.IO.File.WriteAllBytes(path, ReadBytes(len));
        }

        public T ReadObject<T>()
        {
            try
            {
                return (T)bf.Deserialize(BaseStream);
            }
            catch
            {
                T t = (T)new object();
                return t;
            }
        }
    }
}
