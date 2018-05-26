using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientManager
{
    class CPCC
    {
        private string host = System.Configuration.ConfigurationManager.AppSettings["host"];
        private Socket socket;
        private IPEndPoint serverIPEP;
        private IPEndPoint cpccIPEP;
        private List<string> allowedClients;
        private Client client;
        private bool flag;

        public CPCC(int cpccPort, Client client)
        {
            flag = true;
            this.client = client;
            allowedClients = new List<string>();
            cpccIPEP = new IPEndPoint(IPAddress.Parse(host), cpccPort);
            serverIPEP = new IPEndPoint(IPAddress.Parse(host), 50000);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(cpccIPEP);
            socket.BeginConnect(serverIPEP, new AsyncCallback(ConnectCallback), socket);
        }

        public void CloseConnection()
        {
            flag = false;
            socket.Close();
        }

        public Boolean IsClientConnected(string clientId)
        {
            Boolean value = allowedClients.Exists(x => x == clientId);
            return value;
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket handler = (Socket)result.AsyncState;
                handler.EndConnect(result);
                Task.Run(() => { Receive(handler); });
            }
            catch (ObjectDisposedException e0)
            {
                Console.WriteLine(e0);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        private void Receive(Socket handler)
        {
            NetworkStream stream = new NetworkStream(handler);
            BinaryFormatter bformatter = new BinaryFormatter();
            CommandLibrary.Command message = null;

            while (flag)
            {
                if (stream.DataAvailable == true)
                {
                    try
                    {
                        message = (CommandLibrary.Command)bformatter.Deserialize(stream);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }
                    CheckInput(message, handler);
                }
                Thread.Sleep(10);
            }
        }

        private void CheckInput(CommandLibrary.Command input, Socket handler)
        {
            switch (input.commandType)
            {
                case "Call Indication":
                    CallIndication(input, handler);
                    break;
                case "Call confirmed":
                    CallConfirmation(input);
                    break;
            }
        }

        private void CallIndication(CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command response = new CommandLibrary.Command("Call confirmed");
            response.isClientAccepting = true;//zawsze akceptujemy żądanie drugiego klienta
            response.destinationId = client.ReturnDomainName() + ":NCC";
            response.sourceId = client.ReturnDomainName() + ":" + client.ReturnName() + ":CPCC";
            response.startClientId = command.startClientId;
            response.endClientId = command.endClientId;
            response.domainOfRequestedClient = command.domainOfRequestedClient;
            response.domainOfRequestingClient = command.domainOfRequestingClient;
            response.bandwidth = command.bandwidth;
            if (!allowedClients.Exists(x => x.Equals(command.callIndicationClientId)))
            {
                allowedClients.Add(command.callIndicationClientId);
            }
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, response);
                stream.Flush();
                stream.Close();
            }
            client.mainWindow.Invoke(new Action(() => client.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CPCC >> CONFIRMING THE CALL FROM '{command.callIndicationClientId}'" + Environment.NewLine));
        }

        private void CallConfirmation(CommandLibrary.Command command)
        {
            if (!allowedClients.Exists(x => x.Equals(command.callConfirmedClientId)))
            {
                allowedClients.Add(command.callConfirmedClientId);
            }
            client.mainWindow.Invoke(new Action(() => client.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CPCC >> CONFIRMATION OF CALL TO {command.callConfirmedClientId} RECEIVED" + Environment.NewLine));
        }

        public void CallRequest(string requestedDestination, int bandwidth)
        {
            CommandLibrary.Command command = new CommandLibrary.Command("Call Request");
            command.startClientId = client.ReturnName();
            command.endClientId = requestedDestination;
            command.bandwidth = bandwidth;
            command.destinationId = client.ReturnDomainName() + ":NCC";
            command.sourceId = client.ReturnDomainName() + ":" + client.ReturnName() + ":CPCC";
            command.domainOfRequestingClient = client.ReturnDomainName();
            using (var stream = new NetworkStream(socket))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, command);
                stream.Flush();
                stream.Close();
            }
            client.mainWindow.Invoke(new Action(() => client.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CPCC >> MAKING CALL REQUEST TO '{command.endClientId}'" + Environment.NewLine));
        }
    }
}
