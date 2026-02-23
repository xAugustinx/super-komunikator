using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class usrPswd(string usrName, string password)
{
    public string Password = password;
    public string UserName = usrName;
}
class Program
{
    static List<usrPswd> listOfUser = new List<usrPswd>{};
    static List<string> logInUser = new List<string>{};
    
    static void sendingMessage(string ajpi, int portSerwera,  string message)
    {
        using TcpClient client = new TcpClient(ajpi, portSerwera);
        using NetworkStream stream = client.GetStream();
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
    static void Main()
    {
        int port = 5000;
        int portZwrotny = 5001;
        int portDlaWiadomosciUzytkownikow = 5002;
        
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        
        while (true)
        {
            using TcpClient client = listener.AcceptTcpClient();
            using NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var ipU = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

            string backMessage = "uwu";

            if (message.Contains("`nameInDataBase'"))
            {
                int hasloPoczotek = "`nameInDataBase'".Length;
;
                string czyNazwaJest = "false";
                string czyHasloJest = "false";
                
                string usrName = "";
                string userPsswd = "";
                
                for (int i=hasloPoczotek;i<message.Length &&  message[i] != ':' ;i++) {
                    usrName+=message[i];
                    hasloPoczotek++;
                }
                for (int i = hasloPoczotek+2; i < message.Length; i++) userPsswd+=message[i];
                
                for (int i = 0; i < listOfUser.Count; i++) if (listOfUser[i].UserName == usrName)
                {
                    czyNazwaJest = "true";
                    if (listOfUser[i].Password == userPsswd) czyHasloJest = "true";
                }

                if (czyNazwaJest == "false") {
                    listOfUser.Add(new usrPswd(usrName, userPsswd));
                    backMessage = "registrated";
                }
                else if (czyHasloJest == "true") backMessage = "login";
                else backMessage = "false";
            }
            else if (message.Contains("'message'"))
            {
                backMessage = "";
                if (!logInUser.Contains(ipU.ToString())) logInUser.Add(ipU.ToString());
                for (int i = "'message'".Length; i < message.Length; i++) backMessage += message[i];

                for (int i = 0; i < logInUser.Count; i++) sendingMessage(logInUser[i], portDlaWiadomosciUzytkownikow, backMessage);
                
                if (message.Contains("!logout"))
                {
                    logInUser.Remove(ipU.ToString());
                    backMessage = "LOGOUT";
                    sendingMessage(ipU.ToString(), portDlaWiadomosciUzytkownikow, backMessage);
                }
            }
            
            Thread.Sleep(300);
            sendingMessage(ipU.ToString(), portZwrotny, backMessage);
            Console.WriteLine($"Odebrano: {message} {ipU.ToString()}");
        }
    }
}

