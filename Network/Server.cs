using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using System.Collections.Generic;


namespace NetcodeNetworking
{
    public class Server
    {
        Socket serverSocket;
        List<ServerUserInformation> clients;
        Stopwatch serverTimer;

        private const int BUFFER_LENGTH = 1024;

        private static ManualResetEvent allDone = new ManualResetEvent(false);

        [Serializable]
        public enum AccountType : int
        {
            NULL,
            BASIC,
            PRO,
            ADMIN
        }

        [Serializable]
        public struct ServerUserInformation
        {
            public string username;
            public string password;
            public string user_id;
            public AccountType role;
            public Socket socket;
            public DateTime lastRequest { get; set; }

            public ServerUserInformation(string u, string p, string g, AccountType r)
            {
                username = u;
                password = p;
                user_id = g;
                role = r;
                lastRequest = DateTime.Now;
                socket = null;
            }

            public ServerUserInformation(Socket sock)
            {
                username = "";
                password = "";
                user_id = "";
                role = AccountType.BASIC;
                lastRequest = DateTime.Now;
                socket = sock;
            }
        }

        public Server(int ip)
        {
            serverTimer = new Stopwatch();
            serverTimer.Start();

            clients = new List<ServerUserInformation>();

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, ip));
            }
            catch (SocketException)
            {
                Console.WriteLine("Port occupied!");
            }

            serverSocket.Listen(1);

            try
            {
                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for incoming client connections...");
                    serverSocket.BeginAccept(new AsyncCallback(Accept), serverSocket);

                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadLine();
        }

        private void Accept(IAsyncResult ar)
        {
            allDone.Set();
            Console.WriteLine("Accepted new client connection...");
            RemoveExpiredClients();

            try
            {
                State cs = new State(BUFFER_LENGTH, serverSocket.EndAccept(ar));
                clients.Add(new ServerUserInformation(cs.socket));
                Console.WriteLine("Client added");
                cs.socket.BeginReceive(cs.tempBuffer, 0, BUFFER_LENGTH, SocketFlags.None, new AsyncCallback(Recieve), cs);
            }
            catch
            {
                Console.WriteLine("Error 121");
                return;
            }

        }

        private void Recieve(IAsyncResult ar)
        {
            State cs = (State)ar.AsyncState;
            UpdateClientInfo(FindUser(cs.socket));

            Console.Write(cs.tempBuffer.Length + " bytes - ");
            try
            {
                PacketHandler(cs.socket, new PacketReader(cs.tempBuffer));
            }
            catch (SocketException)
            {
                Console.WriteLine("Socket recieve error!!");
                Dispose(cs.socket);
            }
            return;
        }

        public void PacketHandler(Socket socket, PacketReader pr) // Reads first byte of packet
        {
            int hcode = pr.ReadInt32();
            int shcode = pr.ReadInt32();
            Console.WriteLine($"{Enum.GetName(typeof(Header), hcode)}, {Enum.GetName(typeof(Subheader), shcode)}");
            Header h = (Header)hcode;
            Subheader sh = (Subheader)shcode;

            switch (h)
            {
                case Header.Account:
                    Account(socket, sh, pr);
                    break;
                case Header.Friend:
                    Friends(socket, sh, pr);
                    break;
                case Header.Message:
                    Message(socket, sh, pr);
                    break;
                case Header.NetCode:
                    NetCode(socket, sh, pr);
                    break;
                case Header.Application:
                    Application(socket, sh, pr);
                    break;
                case Header.Admin:
                    Admin(socket, sh, pr);
                    break;
                case Header.Error:
                    Error(socket, sh, pr);
                    break;
                default: // Invalid Header
                    Dispose(socket);
                    Console.WriteLine("Packet disposal: " + (int)h);
                    //SendError(socket, Subheader.INVALIDREQ, null);
                    break;
            }

            // After packet is handled
            if (socket.Connected)
            {
                State cs = new State(BUFFER_LENGTH, socket);
                cs.socket.BeginReceive(cs.tempBuffer, 0, BUFFER_LENGTH, SocketFlags.None, new AsyncCallback(Recieve), cs);
            }
        }

        private void Error(Socket socket, Subheader sh, PacketReader pr)
        {
            Console.WriteLine("Error name: " + Enum.GetName(typeof(Subheader), sh) + "\n\tError Code " + (int)sh);
        }

        private void Admin(Socket socket, Subheader sh, PacketReader pr)
        {
            ServerUserInformation sentUser = FindUser(pr.ReadObject<SessionInformation>());


            switch (sh)
            {
                case Subheader.GETNUMCLIENTS:
                    Console.WriteLine(clients.Count + " clients");
                    break;

                case Subheader.GETUPTIME:
                    Console.WriteLine(serverTimer.ElapsedMilliseconds + " uptime");
                    break;

                default: // Invalid Subheader
                    Dispose(socket);
                    Console.WriteLine("Admin disposal");
                    //SendError(socket, Subheader.INVALIDREQ, null);
                    break;
            }
        }

        private void NetCode(Socket socket, Subheader sh, PacketReader pr)
        {
            throw new NotImplementedException();
        }

        private void Application(Socket socket, Subheader sh, PacketReader pr)
        {
            ServerUserInformation sentUser = FindUser(pr.ReadObject<SessionInformation>());

            switch (sh)
            {
                case Subheader.ADDUSR:
                    if (DatabaseQuery.AddFriend(sentUser, pr.ReadObject<UserInformation>()) != Subheader.NULL)
                        SendError(socket, sh, null);
                    break;

                case Subheader.REMOVEUSR:
                case Subheader.DEC_REQ:
                    if (DatabaseQuery.RemoveFriend(sentUser, pr.ReadObject<FriendInformation>()) != Subheader.NULL)
                        SendError(socket, sh, null);
                    break;

                case Subheader.BLOCKUSR:
                case Subheader.ACC_REQ:
                    if (DatabaseQuery.UpdateFriendRequest(sentUser, pr.ReadObject<FriendInformation>()) != Subheader.NULL)
                        SendError(socket, sh, null);
                    break;

                case Subheader.VIEWFRIENDS:
                    if (DatabaseQuery.ViewFriends(sentUser) != Subheader.NULL)
                        SendError(socket, sh, null);
                    break;
                case Subheader.PING:
                    SendHeaders(socket, Header.Application, sh);
                    break;

                default: // Invalid Subheader
                    Dispose(socket);
                    Console.WriteLine("Application disposal: " + (int)sh);
                    //SendError(socket, Subheader.INVALIDREQ, null);
                    break;
            }
        }

        private void Friends(Socket socket, Subheader sh, PacketReader pr)
        {
            ServerUserInformation sentUser = FindUser(pr.ReadObject<SessionInformation>());

            switch (sh)
            {
                case Subheader.ADDUSR:
                    if (DatabaseQuery.AddFriend(sentUser, pr.ReadObject<UserInformation>()) != Subheader.NULL)
                        SendError(socket, sh, null);
                    break;

                case Subheader.REMOVEUSR:
                case Subheader.DEC_REQ:
                    if (DatabaseQuery.RemoveFriend(sentUser, pr.ReadObject<FriendInformation>()) != Subheader.NULL)
                        SendError(socket, sh, null);
                    break;

                case Subheader.BLOCKUSR:
                case Subheader.ACC_REQ:
                    if (DatabaseQuery.UpdateFriendRequest(sentUser, pr.ReadObject<FriendInformation>()) != Subheader.NULL)
                        SendError(socket, sh, null);
                    break;

                case Subheader.VIEWFRIENDS:
                    if (DatabaseQuery.ViewFriends(sentUser) != Subheader.NULL)
                        SendError(socket, sh, null);
                    break;

                default: // Invalid Subheader
                    Dispose(socket);
                    Console.WriteLine("Friend disposal");
                    //SendError(socket, Subheader.INVALIDREQ, null);
                    break;
            }
        }

        private void Message(Socket socket, Subheader sh, PacketReader pr)
        {
            //Header h = Header.Message;

            SessionInformation session = pr.ReadObject<SessionInformation>(); // Reads Session Information from User Identity
            ServerUserInformation sUsers = FindUser(session); // 

            if (!UserExist(session))
            {
                SendError(socket, Subheader.SESSIONLOST, null);
                return;
            }

            var users = UsernameList.Deserialize(ref pr);


            switch (sh)
            {
                case Subheader.TEXTMSG: // (Header,Subheader,SessionInformation,UsernameList,String)
                    var text = pr.ReadString();
                    Console.WriteLine(text);
                    //Redirect(users, (user) => {RedirectPacket(h, sh, socket, sUsers.username, user, text); });
                    break;
                /*
            case Subheader.IMGMSG: // (Header,Subheader,SessionInformation,UsernameList,Image)
                var img = pr.ReadImage();
                Redirect(users, (user) => {
                    RedirectPacket(h, sh, socket, sUsers.username, user, img); });
                break;

            case Subheader.FILEMSG: // (Header,Subheader,SessionInformation,UsernameList,File)
                var file = pr.ReadObject<File>();
                Redirect(users, (user) => {
                    RedirectPacket(h, sh, socket, sUsers.username, user, file);
                });
                break;
                */
                default: // Invalid Subheader
                    //Console.Write((int)sh);
                    Dispose(socket);
                    Console.WriteLine("Message disposal");
                    SendError(socket, sh, (pw) => { pw.Write("server - Error with sending test message"); });
                    break;
            }
        }

        private void Account(Socket socket, Subheader sh, PacketReader pr)
        {
            ServerUserInformation usrInfo = new ServerUserInformation();
            UserInformation info;

            switch (sh)
            {
                case Subheader.LOGIN:
                    info = pr.ReadObject<UserInformation>();
                    sh = DatabaseQuery.Login(ref info, ref usrInfo);
                    UpdateClientInfo(usrInfo);
                    if (sh != Subheader.NULL) SendError(socket, sh, null);
                    else SendUserInfo(socket, info);
                    break;

                case Subheader.REGISTER:
                    info = pr.ReadObject<UserInformation>();
                    sh = DatabaseQuery.Register(ref info, ref usrInfo);
                    UpdateClientInfo(usrInfo);
                    if (sh != Subheader.NULL) SendError(socket, sh, null);
                    else SendUserInfo(socket, info);
                    break;

                case Subheader.USERNAME:
                    sh = DatabaseQuery.UserExists(pr.ReadString());
                    if (sh != Subheader.NULL) SendError(socket, sh, null);
                    else SendHeaders(socket, Header.Account, Subheader.USERNAME);
                    break;

                case Subheader.CHANGEPASS:
                    info = pr.ReadObject<UserInformation>();
                    sh = DatabaseQuery.ChangePassword(ref info, ref usrInfo);
                    UpdateClientInfo(usrInfo);
                    if (sh != Subheader.NULL) SendError(socket, sh, null);
                    else SendUserInfo(socket, info);
                    break;

                default: // Invalid Subheader
                    SendError(socket, Subheader.INVALIDREQ, null);
                    break;
            }
        }

        private void RedirectPacket<T>(Socket socket, string username, byte[] buffer)
        {
            var toSock = FindByUsername(username).socket;
            if (toSock == null)
            {
                SendError(socket, Subheader.MSGNOTSENT,
                    (w) => { w.Write(username); });
            }
            else
            {
                SendCustom(toSock, buffer);
            }
        }


        private delegate void RedirectDelegate(string s);

        private void Redirect(UsernameList users, RedirectDelegate redirecter)
        {
            foreach (var u in users)
            {
                if (redirecter != null)
                    redirecter.Invoke(u);
            }
        }

        private void SendUserInfo(Socket socket, UserInformation info)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)Header.Account);
            pw.WriteInt((int)Subheader.USERINFO);
            pw.WriteObject(info);
            SendCustom(socket, pw.GetBytes(BUFFER_LENGTH));
        }

        private delegate void ErrorDelegate(PacketWriter pw);

        private void SendError(Socket socket, Subheader sh, ErrorDelegate del)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)Header.Error);
            pw.WriteInt((int)sh);
            if (del != null)
                del.Invoke(pw);
            SendCustom(socket, pw.GetBytes(BUFFER_LENGTH));
        }

        private void SendHeaders(Socket socket, Header h, Subheader sh)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)h);
            pw.WriteInt((int)sh);
            SendCustom(socket, pw.GetBytes(BUFFER_LENGTH));
        }

        private void SendCustom(Socket socket, byte[] buffer)
        {
            if (socket.Connected)
                socket.Send(buffer, buffer.Length, SocketFlags.None);
            else Dispose(socket);
        }

        private void UpdateClientInfo(ServerUserInformation usrInfo)
        {
            usrInfo.lastRequest = DateTime.Now;
            int i = clients.FindIndex(usr => usr.socket == usrInfo.socket);
            if (i > 0)
                clients[i] = usrInfo;
        }

        private ServerUserInformation FindByGuid(string guid)
        {
            return clients.Find(u => u.user_id == guid);
        }

        private ServerUserInformation FindByUsername(string username)
        {
            return clients.Find(u => u.username == username);
        }

        private ServerUserInformation FindUser(Socket sock)
        {
            return clients.Find(u => u.socket == sock);
        }

        private ServerUserInformation FindUser(SessionInformation si)
        {
            return clients.Find(u => u.username == si.username && u.user_id == si.user_id);
        }

        public bool UserExist(SessionInformation si)
        {
            return clients.Exists(u => u.username == si.username && u.user_id == si.user_id);
        }

        public void RemoveExpiredClients()
        {
            for (var i = 0; i < clients.Count; i++)
            {
                if (DateTime.Compare(clients[i].lastRequest.AddMinutes(61), DateTime.Now) < 0)
                {
                    clients.Remove(clients[i]);
                    Console.WriteLine("Client removed");
                }
            }
        }

        private void Dispose(Socket socket)
        {
            var user = clients.Find(u => u.socket == socket);
            if (user.username != "")
            {
                Console.WriteLine($"{user.username} disconneted");
            }
            if (user.socket == socket)
                clients.Remove(user);

            socket.Dispose();
            if (socket.Connected)
                socket.Close();
        }

        private class DatabaseQuery // TODO: Implement Dates 
        {

            public static MySqlConnection con = new MySqlConnection("Userid=root; Server=localhost; Database=NetCode;");

            public static List<UserInformation> userList = new List<UserInformation>();
            public static List<FriendInformation> friendList = new List<FriendInformation>();
            public static List<ProjectFileInformation> fileList = new List<ProjectFileInformation>();
            public static List<ProjectInformation> projectList = new List<ProjectInformation>();

            public static Subheader UserExists(string username)
            {
                MySqlCommand cmd = new MySqlCommand("SELECT username FROM Users WHERE username ='" + username + "';", con);

                try
                {
                    string user = (string)cmd.ExecuteScalar();
                    if (username != user) return Subheader.NO_SUCH_USR;
                    else return Subheader.NULL;
                }
                catch (MySqlException)
                {
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader Login(ref UserInformation userInfo, ref ServerUserInformation serverUser)
            {
                try
                {
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand("SELECT username, password, user_id, user_role FROM Users " +
                        "WHERE username ='" + userInfo.username + "' AND password = '" + userInfo.password + "';", con);

                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (!rdr.Read()) return Subheader.INVALIDUSR;


                    serverUser.username = (string)rdr["username"];
                    serverUser.password = (string)rdr["password"];
                    serverUser.user_id = (string)rdr["user_id"];
                    serverUser.role = (AccountType)Enum.Parse(typeof(AccountType), (string)rdr["user_role"]);
                    userInfo.user_id = serverUser.user_id;

                    Console.WriteLine($"{serverUser.username}, {serverUser.password}, {serverUser.user_id}, {Enum.GetName(typeof(AccountType), serverUser.role)}");

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.Message);
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader ChangePassword(ref UserInformation userInfo, ref ServerUserInformation serverUser)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("UPDATE Users SET password = '" + userInfo.newpassword + "' WHERE user_id = '" + userInfo.user_id + "' AND username = '" + userInfo.username + "';", con);

                }
                catch (Exception) { return Subheader.DBCON_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBCHG_ERROR;

                    serverUser.username = userInfo.username;
                    serverUser.password = userInfo.newpassword;
                    userInfo.password = userInfo.newpassword;
                    userInfo.newpassword = "";
                    serverUser.user_id = userInfo.user_id;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader Register(ref UserInformation userInfo, ref ServerUserInformation serverUser)
            {
                string username = userInfo.username;
                string password = userInfo.password;
                string guid = Guid.NewGuid().ToString();
                AccountType role = AccountType.BASIC;
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("INSERT INTO Users (user_id, username, password, user_role) VALUES ('" + guid + "','" + username + "','" + password + "', '" + Enum.GetName(typeof(AccountType), role) + "');", con);

                }
                catch (Exception) { return Subheader.DBCON_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBREG_ERROR;

                    serverUser.username = username;
                    serverUser.password = password;
                    serverUser.user_id = guid;
                    serverUser.role = (AccountType)role;
                    userInfo.user_id = guid;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader ChangeUserType(ServerUserInformation serverUser, UserInformation userInfo, AccountType type)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("UPDATE Users SET user_role = '" + Enum.GetName(typeof(AccountType), type) + "' WHERE user_id = '" + userInfo.user_id + "';", con);

                }
                catch (Exception) { return Subheader.DBCON_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBCHG_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }


            public static Subheader CreateProject(ServerUserInformation serverUser, ProjectInformation projInfo)
            {
                MySqlCommand cmd;

                try
                {
                    string query = "INSERT INTO Projects (project_id, project_name) VALUES ('" + projInfo.project_id + "','" + projInfo.project_name + "'); ";
                    foreach (ProjectUser user in projInfo)
                    {
                        query += "INSERT INTO Project_Users (project_users_id,project_id,user_id) VALUES ('" + projInfo.project_user_id + "','" + projInfo.project_id + "','" + user.user_id + "'); ";
                    }
                    cmd = new MySqlCommand(query, con);
                }
                catch { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader AddProjectUser(ServerUserInformation serverUser, UserInformation userInfo, ProjectInformation projInfo)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("INSERT INTO Project_Users (project_users_id,project_id,user_id) VALUES ('" + projInfo + "','" + projInfo.project_id + "','" + userInfo.user_id + "'); ", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader RemoveProjectUser(ServerUserInformation serverUser, UserInformation userInfo, ProjectInformation projInfo)
            {
                if (!ProjectAuth(serverUser, projInfo, ProjectRole.Owner))
                    return Subheader.UNAUTH_ERROR;

                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("DELETE FROM Project_Users WHERE project_id = '" + projInfo.project_id + "';"
                        + "DELETE FROM Projects WHERE project_id = '" + projInfo.project_id + " ';", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader RemoveProject(ServerUserInformation serverUser, ProjectInformation projInfo)
            {
                if (!ProjectAuth(serverUser, projInfo, ProjectRole.Owner))
                    return Subheader.UNAUTH_ERROR;

                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("DELETE FROM Files WHERE project_id = '" + projInfo.project_id + "'; " +
                        "DELETE FROM Project_Users WHERE project_id = '" + projInfo.project_id + "'; "
                        + "DELETE FROM Projects WHERE project_id = '" + projInfo.project_id + " ';", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }


            public static Subheader CreateFile(ServerUserInformation serverUser, ProjectInformation projInfo, ProjectFileInformation fileInfo)
            {
                MySqlCommand cmd;
                try
                {
                    cmd = new MySqlCommand("INSERT INTO Users (file_id, file_data, file_name, project_id) " +
                        "VALUES ('" + projInfo.project_id + "',@file_data,'" + fileInfo.file_name +
                        "', '" + fileInfo.project_id + "');", con);
                    cmd.Parameters.Add("@file_data", MySqlDbType.Binary, fileInfo.file_data.Length).Value = fileInfo.file_data;
                }
                catch { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBREG_ERROR;
                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader UpdateFile(ServerUserInformation serverUser, ProjectFileInformation fileInfo)
            {
                MySqlCommand cmd;
                try
                {
                    cmd = new MySqlCommand("UPDATE Files SET file_data = @file_data, file_name = '" + fileInfo.file_name + "' WHERE file_id = '" + fileInfo.file_id + "';", con);
                    cmd.Parameters.Add("@file_data", MySqlDbType.Binary, fileInfo.file_data.Length).Value = fileInfo.file_data;
                }
                catch { return Subheader.DBCON_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBREG_ERROR;
                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader RemoveFile(ServerUserInformation serverUser, ProjectFileInformation fileInfo)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("DELETE FROM Files WHERE file_id = '" + fileInfo.file_id + "';", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }


            public static Subheader AddFriend(ServerUserInformation serverUser, UserInformation userInfo)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("INSERT INTO Friends (friend_id, requested_user_id, received_user_id) " +
                        "VALUES ('" + Guid.NewGuid() + "','" + serverUser.user_id + "','" + userInfo.user_id + "'); ", con);
                }
                catch { return Subheader.FRIEND_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader RemoveFriend(ServerUserInformation serverUser, FriendInformation friendInfo)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("DELETE FROM Friends WHERE friend_id = '" + friendInfo.friend_id + "';", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader UpdateFriendRequest(ServerUserInformation serverUser, FriendInformation friendInfo)
            {
                MySqlCommand cmd;
                try
                {
                    cmd = new MySqlCommand("UPDATE Friends SET friend_status = '" + Enum.GetName(typeof(FriendStatus), friendInfo.friend_status) +
                        "' WHERE friend_id = '" + friendInfo.friend_id + "';", con);
                }
                catch { return Subheader.DBCON_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBREG_ERROR;
                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }


            public static Subheader ViewProjects(ServerUserInformation serverUser, UserInformation userInfo)
            {
                MySqlCommand cmd;
                MySqlDataReader rdr;
                try
                {
                    cmd = new MySqlCommand("SELECT project_id, project_name FROM Projects WHERE project_type = '" + Enum.GetName(typeof(ProjectType), ProjectType.PUBLIC)
                        + "' project_id = (SELECT project_id FROM Project_Users WHERE Project_Users.user_id = '" + userInfo.user_id + "')", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    rdr = cmd.ExecuteReader();
                    if (rdr.FieldCount <= 0) return Subheader.DBQRY_ERROR;
                    projectList.Clear();
                    while (rdr.Read())
                    {
                        projectList.Add(new ProjectInformation(rdr["project_name"] + "", rdr["project_id"] + "", rdr["project_user_id"] + "", rdr["project_type"] + ""));
                    }

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader ViewProjectFiles(ServerUserInformation serverUser, ProjectInformation projInfo)
            {
                MySqlCommand cmd;
                MySqlDataReader rdr;

                try
                {
                    cmd = new MySqlCommand("SELECT file_id, file_name FROM Files WHERE project_id = '" + projInfo.project_id + "';", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    rdr = cmd.ExecuteReader();
                    if (rdr.FieldCount <= 0) return Subheader.DBQRY_ERROR;
                    projectList.Clear();
                    while (rdr.Read())
                    {
                        fileList.Add(new ProjectFileInformation(rdr["file_name"] + "", rdr["file_id"] + ""));
                    }

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader ViewProjectUsers(ServerUserInformation serverUser, ProjectInformation projInfo)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("SELECT file_id, file_name FROM Files WHERE project_id = '" + projInfo.project_id + "';", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader ViewFiles(ServerUserInformation serverUser)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("SELECT file_id, file_name FROM Files WHERE project_id = " +
                        "(SELECT project_id FROM Project_Users WHERE user_id = '" + serverUser.user_id + "' )", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader ViewFriends(ServerUserInformation serverUser)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("SELECT friend_id FROM Friends WHERE requested_user_id ='" +
                        serverUser.user_id + "' OR received_user_id = '" + serverUser.user_id + "';", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static Subheader ViewFiles(ServerUserInformation serverUser, UserInformation userInfo)
            {
                MySqlCommand cmd;

                try
                {
                    cmd = new MySqlCommand("SELECT file_id, file_name FROM Files WHERE project_id = " +
                        "(SELECT project_id FROM Project_Users WHERE user_id = '" + userInfo.user_id + "' )", con);
                }
                catch (Exception) { return Subheader.PROJ_ERROR; }

                try
                {
                    if (cmd.ExecuteNonQuery() <= 0)
                        return Subheader.DBQRY_ERROR;

                    return Subheader.NULL;
                }
                catch (MySqlException e)
                {
                    if (e.Number == 2627)
                        return Subheader.DBEXST_ERROR;
                    return Subheader.DBQRY_ERROR;
                }
            }

            public static AccountType UserAccountType(ServerUserInformation serverUser)
            {
                MySqlCommand cmd;


                try
                {
                    cmd = new MySqlCommand("SELECT user_role FROM Users WHERE user_id = '" + serverUser.user_id + "';", con);

                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        return (AccountType)Enum.Parse(typeof(AccountType), (string)rdr["user_role"]);
                    }
                    else return AccountType.NULL;
                }
                catch
                {
                    return (AccountType)1;
                }
            }


            private static bool FriendAuth(ServerUserInformation serverUser, ProjectInformation projInfo, ProjectRole? role)
            {
                foreach (ProjectUser user in projInfo)
                {
                    if (user.user_id == serverUser.user_id)
                        if (role == null || user.role == role)
                            return true;
                }
                return false;
            }

            private static bool ProjectAuth(ServerUserInformation serverUser, ProjectInformation projInfo, ProjectRole? role)
            {
                foreach (ProjectUser user in projInfo)
                {
                    if (user.user_id == serverUser.user_id)
                        if (role == null || user.role == role)
                            return true;
                }
                return false;
            }

        }

    }
}