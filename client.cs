using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;

class TcpClientApp
{
    static bool usrMode = false;
    
    static int port = 5000;
    static int portZwrotny = 5001;
    private static int portDlaWiadomosciUzytkownikow = 5002;
    static string serverIp = "127.0.0.1";
    static string userName;
    static string password;

    private static bool czyDzialaProgram = true;
    
    static List<string> cpArgs = new List<string>{};

    static Thread odbieranieWiadomosci;
    static TcpListener listener;

    static void sendingMessage(string ajpi, int portSerwera,  string message)
    {
        using TcpClient client = new TcpClient(ajpi, portSerwera);
        using NetworkStream stream = client.GetStream();
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }

    static void otherMesage()
    {
        listener = new TcpListener(IPAddress.Any, portDlaWiadomosciUzytkownikow);
        listener.Start();
        while (czyDzialaProgram)
        {
            using TcpClient client = listener.AcceptTcpClient();
            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine(message);
            if (message == "LOGOUT") break;
            Thread.Sleep(300);
        }
    }
    static void MainProgramu()
    {
        if (cpArgs.Count > 0) serverIp = cpArgs[0];
        
        TcpListener listener = new TcpListener(IPAddress.Any, portZwrotny);
        
        Console.Clear();
        
        if ((cpArgs.Count <= 0 || cpArgs[0] == "help" || cpArgs[0] == "?") && usrMode) {
            Console.Write("First Argument is ip of Server\n"); return;
        }
        
        while (true)  {
            Console.Write("User name (old or new): ");
            userName = Console.ReadLine(); Console.Write("\n");
            
            bool czyDozwolona = true;
            for (int i = 0; i < userName.Length && czyDozwolona; i++) if (userName[i] == ' ' || userName[i] == ':' || userName[i] == '`') czyDozwolona = false;
            if (czyDozwolona) {
                Console.Write("User Password (old or new): ");
                password = Console.ReadLine(); Console.Write("\n");
                
                string wiadomosc = "`nameInDataBase'"  + userName + ":" + password;
                sendingMessage(serverIp, port, wiadomosc);
                listener.Start();
                using TcpClient client = listener.AcceptTcpClient();
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string messageBack = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                listener.Stop();
                //Console.WriteLine(messageBack);

                if (messageBack == "registrated") {
                    Console.WriteLine("Successfully Registered!");
                    czyDzialaProgram = false;
                    return;
                }
                else if (messageBack == "login") {
                    Console.WriteLine("Successfully Logged in!");
                    break;
                }
                else Console.WriteLine("Bad Password or not correct login!");
            }
            else Console.WriteLine("User name can't have SPACE/`/:");
        }
        
        odbieranieWiadomosci = new Thread(new ThreadStart(otherMesage));
        odbieranieWiadomosci.Start();
        
        while (true)
        {
            string rozbudowania = "'message'" + userName + ": " + Console.ReadLine() ;
            
            sendingMessage(serverIp, port, rozbudowania);
            listener.Start();
            using TcpClient client = listener.AcceptTcpClient();
            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string messageBack = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            listener.Stop();
            if (messageBack == "LOGOUT")
            {
                czyDzialaProgram = false;
                listener.Stop();
                return;
            }
        }
    }
    static void Main(string[] args)
    {
        for (int i = 0; i < args.Length; i++) cpArgs.Add(args[i]);
        Thread myThread = new Thread(new ThreadStart(MainProgramu));
        myThread.Start();
        while (czyDzialaProgram) Thread.Sleep(300);
        
    }
}