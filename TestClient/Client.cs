using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetcodeNetworking;
using System.Net;

namespace TestClient
{
    class Client
    {
        static void Main(string[] args)
        {
            var c = new NetcodeNetworking.Client();

            //c.SendMessage(Console.ReadLine());

            while (true) {
                var msg = Console.ReadLine();
                if (msg.Trim() == "uptime")
                {
                    Console.WriteLine("Uptime Request");
                    c.SendUptimeRequest();
                } else if (msg == "ping")
                {
                    c.SendPingRequest();
                    Console.WriteLine("Ping Request");
                }
                else if (msg.Trim() == "numclients")
                {
                    Console.WriteLine("Num client request");
                    c.SendNumClientRequest();
                }
                else if (msg.Length > 7 && msg.Substring(0,7) == "/login ")
                {
                    string[] info = msg.Split(new char[] { ' ' });
                    if (info.Length != 3)
                        continue;
                    
                    c.LoginRequest(info[1], info[2]);
                }
                else
                {
                    c.SendMessage(msg);
                }
            }
        }
    }
}
