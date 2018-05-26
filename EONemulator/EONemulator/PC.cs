using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EONemulator
{
    class PC
    {
        private string host = System.Configuration.ConfigurationManager.AppSettings["host"];
        private Socket socket;
        private IPEndPoint serverIPEP;
        private IPEndPoint pcIPEP;
        private Domain self;
        private string pcId;
        private Dictionary<string, string> clientsAndDomains;

        public PC(Domain self, int pcPort, string pcId, Dictionary<string, string> dictionary)
        {
            clientsAndDomains = dictionary;
            this.self = self;
            this.pcId = pcId;
            pcIPEP = new IPEndPoint(IPAddress.Parse(host), pcPort);
            serverIPEP = new IPEndPoint(IPAddress.Parse(host), 50000);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(pcIPEP);
            socket.BeginConnect(serverIPEP, new AsyncCallback(ConnectCallback), socket);
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

            while (true)
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
                case "Directory Request":
                    DirectoryRequest(input, handler);
                    break;
                case "Policy Out":
                    PolicyOut(input, handler);
                    break;
            }
        }

        private void DirectoryRequest(CommandLibrary.Command command, Socket handler)
        {
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> PC >> Translating destination address..." + Environment.NewLine));
            CommandLibrary.Command callConfirmed = new CommandLibrary.Command("Directory Request confirmed");
            callConfirmed.sourceId = pcId;
            callConfirmed.destinationId = self.ReturnName() + ":NCC";
            callConfirmed.endClientId = command.endClientId;
            callConfirmed.startClientId = command.startClientId;
            callConfirmed.bandwidth = command.bandwidth;
            clientsAndDomains.TryGetValue(command.endClientId, out callConfirmed.domainOfRequestedClient);
            callConfirmed.domainOfRequestingClient = command.domainOfRequestingClient;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, callConfirmed);
                stream.Flush();
                stream.Close();
            }
        }

        private void PolicyOut(CommandLibrary.Command command, Socket handler)
        {
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> PC >> Checking if call is possible..." + Environment.NewLine));
            CommandLibrary.Command callConfirmed = new CommandLibrary.Command("Policy Out confirmed");
            callConfirmed.sourceId = pcId;
            callConfirmed.destinationId = self.ReturnName() + ":NCC";
            callConfirmed.endClientId = command.endClientId;
            callConfirmed.startClientId = command.startClientId;
            callConfirmed.bandwidth = command.bandwidth;
            callConfirmed.domainOfRequestedClient = command.domainOfRequestedClient;
            callConfirmed.domainOfRequestingClient = command.domainOfRequestingClient;
            callConfirmed.isClientAccessible = true;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, callConfirmed);
                stream.Flush();
                stream.Close();
            }
        }
    }
}
