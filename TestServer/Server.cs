using System;
using NetcodeNetworking;
using System.Drawing;
using MySql.Data.MySqlClient;

namespace TestServer
{
    class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating Server...");
            /* 
            MySqlConnection con = new MySqlConnection("User id=root; Server=localhost; Database=NetCode;");
            con.Open();

            MySqlCommand cmd = new MySqlCommand("SELECT * FROM users;",con);
           var rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.Write((string)rdr["username"] + " ");
                Console.Write((string)rdr["user_id"] + " ");
                Console.Write((string)rdr["password"] + " ");
                Console.WriteLine((string)rdr["user_role"]);
            }
            Console.Read();
            */
            new NetcodeNetworking.Server(31337);
            /*
            PacketWriter pw = new PacketWriter();
            pw.WriteByte(34);
            pw.WriteString(Guid.NewGuid().ToString());
            //pw.WriteByte(123);
            byte[] transfer = pw.GetBytes(1024);


            PacketReader pr = new PacketReader(transfer);
            var i = pr.ReadByte();
            var l = pr.ReadString();
            //var j = pr.ReadInt32();
            Console.Write(transfer.Length +  " | " + l + " ");
            Console.ReadLine();
            */
        }
    }
}
