using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;


namespace NetcodeNetworking
{
    public class Client
    {
        Socket clientSocket;

        private const int BUFFER_LENGTH = 1024;
        private Stopwatch timer;

        public delegate void ClientEventHandler(object source, object data);
        //public event ClientEventHandler OnConnect;
        //public event ClientEventHandler OnReconnect;
        //public event ClientEventHandler OnDisconnect;
        //public event ClientEventHandler OnRecieve;
        public event ClientEventHandler OnJoin;
        public event ClientEventHandler OnSend;

        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        public Client()
        {
            OnSend += AfterSend;
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 31337));

                //OnConnect(this, clientSocket);
                //StartTimer();
                //SendCustom(Header.Application, Subheader.PING, null);

                State s = new State(BUFFER_LENGTH, null);
                clientSocket.BeginReceive(s.tempBuffer, 0, s.tempBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), s);

                OnJoin(this, clientSocket);
            }
            catch
            {
                return;
            }

        }

        public void AfterSend(object o, object data)
        {
            State s = new State(BUFFER_LENGTH, null);
            clientSocket.BeginReceive(s.tempBuffer, 0, s.tempBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), s);
            //Console.WriteLine("Pkg sent!");
        }

        public void Reconnect()
        {

            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 31337));

                State s = new State();
                clientSocket.BeginReceive(s.tempBuffer, 0, s.tempBuffer.Length,
                    SocketFlags.None, new AsyncCallback(ReceiveCallback), s);
            }
            catch
            {
                Console.WriteLine("Unable to connect to client");
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                State state = (State)ar.AsyncState;
                Console.WriteLine($"{state.tempBuffer.Length}");
                //Console.WriteLine("Handling Packet");
                HandlePacket(state.tempBuffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void HandlePacket(byte[] buffer)
        {
            PacketReader pr = new PacketReader(buffer);
            Header h = (Header)pr.ReadInt32();
            Subheader sh = (Subheader)pr.ReadInt32();

            Console.WriteLine($" {(int)h} - {(int)sh}");

            switch (h)
            {
                case Header.Account:
                    AccountHandler(sh, pr);
                    break;

                case Header.NetCode:
                    NetCodeHandler(sh, pr);
                    break;

                case Header.Message:
                    MessageHandler(sh, pr);
                    break;

                case Header.Application:
                    ApplicationHandler(sh, pr);
                    break;

                case Header.Friend:
                    FriendHandler(sh, pr);
                    break;

                case Header.Error:
                    ErrorHandler(sh, pr);
                    break;

                case Header.Undefined:
                    Console.WriteLine("6");
                    //Ignore
                    break;

                default:
                    Console.WriteLine("Invalid header");
                    SendError(Header.Error, Subheader.INVALIDREQ, null);
                    break;
            }
        }

        private void ApplicationHandler(Subheader sh, PacketReader pr)
        {
            switch (sh)
            {
                case Subheader.PING:
                    Console.WriteLine(EndTimer() + " ms");
                    break;

                default:
                    Console.WriteLine("Application error - " + (int)sh);
                    //SendError(Header.Error, Subheader.INVALIDREQ, null);
                    break;
            }
        }

        private void AccountHandler(Subheader sh, PacketReader pr)
        {
            switch (sh)
            {
                case Subheader.USERINFO:
                    var user =
                    pr.ReadObject<UserInformation>();

                    Console.WriteLine("Successful login, " + user.username + "!");
                    break;

                default:
                    Console.WriteLine("Invalid Account Subheader");
                    break;
            }
        }

        private void NetCodeHandler(Subheader sh, PacketReader pr)
        {
            throw new NotImplementedException();
        }

        private void MessageHandler(Subheader sh, PacketReader pr)
        {
            throw new NotImplementedException();
        }

        private void FriendHandler(Subheader sh, PacketReader pr)
        {
            throw new NotImplementedException();
        }

        private void ErrorHandler(Subheader sh, PacketReader pr)
        {
            throw new NotImplementedException();
        }

        public delegate void WriteCustom(PacketWriter pw);

        public void SendError(Header h, Subheader sh, WriteCustom wc)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)h);
            pw.WriteInt((int)sh);
            if (wc != null)
                wc.Invoke(pw);
            Send(pw.GetBytes(BUFFER_LENGTH));
        }

        private void SendCustom(Header h, Subheader sh, WriteCustom wc)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)h);
            pw.WriteInt((int)sh);
            if (wc != null)
                wc.Invoke(pw);
            Send(pw.GetBytes(BUFFER_LENGTH));
        }

        public void SendAuthCustom(Header h, Subheader sh, UserInformation i, UsernameList l, WriteCustom wc)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)h);
            pw.WriteInt((int)sh);
            pw.WriteObject(i);
            pw.WriteObject(l);
            if (wc != null)
                wc.Invoke(pw);
            Send(pw.GetBytes(BUFFER_LENGTH));
        }

        public void SendWait(Header h, Subheader sh, WriteCustom wc)
        {
            PacketWriter pw = new PacketWriter();
            pw.Write((byte)h);
            pw.Write((byte)sh);
            if (wc != null)
                wc.Invoke(pw);
            Send(pw.GetBytes(BUFFER_LENGTH));
        }

        public void SendMessage(string str)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)Header.Message);
            pw.WriteInt((int)Subheader.TEXTMSG);
            pw.WriteString(str);

            Send(pw.GetBytes(BUFFER_LENGTH));
        }

        public void SendNumClientRequest()
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)Header.Admin);
            pw.WriteInt((int)Subheader.GETNUMCLIENTS);

            Send(pw.GetBytes(BUFFER_LENGTH));
        }

        public void SendUptimeRequest()
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)Header.Admin);
            pw.WriteInt((int)Subheader.GETUPTIME);

            Send(pw.GetBytes(BUFFER_LENGTH));
        }

        public void SendPingRequest()
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)Header.Application);
            pw.WriteInt((int)Subheader.PING);

            StartTimer();
            Send(pw.GetBytes(BUFFER_LENGTH));

        }

        public void Send(byte[] buffer)
        {
            clientSocket.Send(buffer);
            OnSend(this, buffer);
        }

        public void LoginRequest(string name, string pass)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteInt((int)Header.Account);
            pw.WriteInt((int)Subheader.LOGIN);
            pw.WriteObject(new UserInformation(name, pass));

            Send(pw.GetBytes(BUFFER_LENGTH));
        }

        private void StartTimer()
        {
            timer = new Stopwatch();
            timer.Start();
        }

        private int EndTimer()
        {
            timer.Stop();
            return timer.Elapsed.Milliseconds;
        }
    }
}
