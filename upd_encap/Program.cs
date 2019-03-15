using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace upd_encap
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Packages
    {
        public int idPlayer;
        public int idHero;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] coords;
        public int rotation;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string action;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public int[] targetCoords;
        public float healthPoint;
        public float manaPoint;

        public Packages(int x, int y, int rotation, int playerId, int heroId, string action, float healthPoint, float manaPoint, int targetCoordsX = 0, int targetCoordsY = 0)
        {
            this.coords = new int[2];
            this.coords[0] = x;
            this.coords[1] = y;
            this.healthPoint = healthPoint;
            this.manaPoint = manaPoint;
            this.rotation = rotation;
            this.idPlayer = playerId;
            this.idHero = heroId;
            this.action = action;
            this.targetCoords = new int[2];
            this.targetCoords[0] = targetCoordsX;
            this.targetCoords[1] = targetCoordsY;
        }
    }

    class UDPEncap
    {
        const int port = 1014;

        Thread thread = null;
        public void Start()
        {
            if (thread != null)
                throw new Exception("Already started, stop current thread!");
            Console.WriteLine("Started listening...");
            StartListening();
        }

        public void Stop()
        {
            try
            {
                udp.Close();
                Console.WriteLine("Stopped listening");
                thread.Abort();
            }
            catch { }
        }

        public readonly UdpClient udp = new UdpClient(port);
        IAsyncResult ar_ = null;

        private void StartListening()
        {
            Console.WriteLine("Start listening");
            ar_ = udp.BeginReceive(ReceiveStruct, new object());
        }

        public void BeginReceive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            byte[] bytes = udp.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            Console.WriteLine("From {0} received: {1} ", ip.Address.ToString(), message);
            StartListening();
        }


        public void ReceiveStruct(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            byte[] bytes = udp.EndReceive(ar, ref ip);
            Packages recvPacket = BytresToStruct(bytes);
            Console.WriteLine("Pakcages received");
            string recvPacketStr = "X = " + recvPacket.coords[0].ToString() +
                ", Y = " + recvPacket.coords[1].ToString() +
                ", Rotation = " + recvPacket.rotation.ToString() +
                ", IdPlayer = " + recvPacket.idPlayer.ToString() +
                ", IdHero = " + recvPacket.idHero.ToString() +
                ", Action = " + recvPacket.action +
                ", IdTarget X = " + recvPacket.targetCoords[0].ToString() +
                ", IdTarget Y = " + recvPacket.targetCoords[1].ToString() +
                ", HP = " + recvPacket.healthPoint.ToString() +
                ", MP = " + recvPacket.manaPoint.ToString();
            Console.WriteLine("From {0} received: {1} ", ip.Address.ToString(), recvPacketStr);
            StartListening();
        }

        public void Send(string message)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, port);
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }

        public void SendBytes(byte[] message)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, port);
            client.Send(message, message.Length, ip);
            client.Close();
        }

        public byte[] GetStructBytes(Packages str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public Packages BytresToStruct(byte[] arr)
        {
            Packages str = new Packages();
            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(arr, 0, ptr, size);
            str = (Packages)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);
            return str;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            UDPEncap udp = new UDPEncap();
            Console.WriteLine("Choose side :");
            Console.WriteLine("1. Server");
            Console.WriteLine("2. Client");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("Waiting for client to send packages");
                    udp.Start();
                    while (true) ;
                case "2":
                    do
                    {
                        Console.WriteLine("Write Message : Unit X, Unit Y, Unit Rotation, Player ID, Hero ID, Action, HP, Mana, Target X (opt), Target Y (opt)");
                        string message = Console.ReadLine();
                        string[] dataArray = message.Split(',');
                        if (dataArray.Length < 8)
                        {
                            udp.Stop();
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Sending package data");
                            Packages packet = new Packages(Int32.Parse(dataArray[0]), Int32.Parse(dataArray[1]), Int32.Parse(dataArray[2]), Int32.Parse(dataArray[3]), Int32.Parse(dataArray[4]), dataArray[5], Int32.Parse(dataArray[6]), Int32.Parse(dataArray[7]), Int32.Parse(dataArray[8]), Int32.Parse(dataArray[9]));
                            udp.SendBytes(udp.GetStructBytes(packet));
                        }
                    } while (true);
                    break;
                default:
                    break;
            }
        }
    }
}