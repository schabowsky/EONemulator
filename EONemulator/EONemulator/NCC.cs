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
    class NCC
    {
        private string host = System.Configuration.ConfigurationManager.AppSettings["host"];
        private Socket socket;
        private IPEndPoint serverIPEP;
        private IPEndPoint nccIPEP;
        private Domain self;
        private string nccId;
        private string callRequester;
        private string callDestination;
        private List<Connection> connectionList;
        private Connection clearedConnection;

        public class Connection
        {
            public int connectionId;
            public int bandwidth;
            public string domainOfRequester;
            public string startId;
            public string endId;
            public List<CommandLibrary.Link> usedLinks;//id użytych linków
            public string modulation;//raczej nie użyjemy, bo zestawiamy na nowo i nie wiadomo jaka jest droga
        }

        public NCC (Domain self, int nccPort, string nccId)
        {
            connectionList = new List<Connection>();
            callRequester = string.Empty;
            this.self = self;
            this.nccId = nccId;
            nccIPEP = new IPEndPoint(IPAddress.Parse(host), nccPort);
            serverIPEP = new IPEndPoint(IPAddress.Parse(host), 50000);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(nccIPEP);
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
                case "Call Request":
                    callRequester = input.sourceId;
                    callDestination = input.endClientId;
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Call Request (destination: {input.endClientId}, bandwidth: " + input.bandwidth + $"Gb/s)' from { input.sourceId }" + Environment.NewLine));
                    CallRequest(input, handler);
                    break;
                case "Directory Request confirmed":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> NCC >> 'Directory Request Confirmed'" + Environment.NewLine));
                    DirectoryRequestConfirmed(input, handler);
                    break;
                case "Policy Out confirmed":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> NCC >> 'Policy Out Confirmed'" + Environment.NewLine));
                    PolicyOutConfirmed(input, handler);
                    break;
                case "Call Coordination"://to przy 2 domenach, na razie zostaje
                    callRequester = input.startClientId;
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Call Coordination' from {input.sourceId}" + Environment.NewLine));
                    CallCoordination(input, handler);
                    break;
                case "Call confirmed":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Call Confirmed' from {input.sourceId}" + Environment.NewLine));
                    CallConfirmed(input, handler);
                    break;
                case "Connection confirmed":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Connection Confirmed' from {input.sourceId}" + Environment.NewLine));
                    ConnectionConfirmed(input, handler);
                    break;
                case "Critical Link Deleted":
                    CriticalLinkDeleted(input, handler);
                    break;
                case "OXC Cleared":
                    OXCCleared(input, handler);
                    break;
            }
        }

        private void OXCCleared(CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command request = new CommandLibrary.Command("Call Request");
            request.bandwidth = clearedConnection.bandwidth;
            request.startClientId = clearedConnection.startId;
            request.endClientId = clearedConnection.endId;
            request.domainOfRequestingClient = clearedConnection.domainOfRequester;
            CallRequest(request, handler);
        }

        private void CriticalLinkDeleted(CommandLibrary.Command command, Socket handler)
        {
            clearedConnection = connectionList.Find(x => x.connectionId == command.connectionId);
            CommandLibrary.Command response = new CommandLibrary.Command("Clear OXC");
            response.connectionId = command.connectionId;
            response.deletedLinkId = command.deletedLinkId;
            response.destinationId = self.ReturnMainSubnetwork().ReturnName() + ":CC";
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, response);
                stream.Flush();
                stream.Close();
            }
            connectionList.Remove(clearedConnection);
        }

        private void CallRequest(CommandLibrary.Command command, Socket handler)
        {
            Connection conn = new Connection();
            conn.bandwidth = command.bandwidth;
            conn.connectionId = connectionList.Count;
            conn.domainOfRequester = command.domainOfRequestingClient;
            conn.startId = command.startClientId;
            conn.endId = command.endClientId;
            connectionList.Add(conn);

            CommandLibrary.Command directoryRequest = new CommandLibrary.Command("Directory Request");
            directoryRequest.sourceId = nccId;
            directoryRequest.destinationId = self.ReturnName() + ":PC";
            directoryRequest.endClientId = command.endClientId;
            directoryRequest.startClientId = command.startClientId;
            directoryRequest.domainOfRequestingClient = command.domainOfRequestingClient;
            directoryRequest.bandwidth = command.bandwidth;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, directoryRequest);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Directory Request ({directoryRequest.endClientId})' sent to {directoryRequest.destinationId}" + Environment.NewLine));
        }

        private void DirectoryRequestConfirmed(CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command policyOut = new CommandLibrary.Command("Policy Out");
            policyOut.sourceId = nccId;
            policyOut.destinationId = self.ReturnName() + ":PC";
            policyOut.endClientId = command.endClientId;
            policyOut.startClientId = command.startClientId;
            policyOut.bandwidth = command.bandwidth;
            policyOut.domainOfRequestedClient = command.domainOfRequestedClient;
            policyOut.domainOfRequestingClient = command.domainOfRequestingClient;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, policyOut);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Policy Out (from {policyOut.startClientId} to {policyOut.endClientId})' sent to {policyOut.destinationId}" + Environment.NewLine));
        }

        private void PolicyOutConfirmed(CommandLibrary.Command command, Socket handler)
        {
            if (command.isClientAccessible)
            {
                if (command.domainOfRequestedClient == self.ReturnName())
                {
                    CommandLibrary.Command callIndication = new CommandLibrary.Command("Call Indication");
                    callIndication.sourceId = nccId;
                    callIndication.destinationId = command.domainOfRequestedClient + ":" + command.endClientId + ":CPCC";
                    callIndication.endClientId = command.endClientId;
                    callIndication.startClientId = command.startClientId;
                    callIndication.domainOfRequestedClient = command.domainOfRequestedClient;
                    callIndication.domainOfRequestingClient = command.domainOfRequestingClient;
                    callIndication.callIndicationClientId = command.startClientId;
                    callIndication.bandwidth = command.bandwidth;
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, callIndication);
                        stream.Flush();
                        stream.Close();
                    }
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Call Indication' sent to {callIndication.destinationId}" + Environment.NewLine));
                }
                else
                {
                    //klient w innej domenie, trzeb wysłać Call Coordination

                    CommandLibrary.Command callCoordination = new CommandLibrary.Command("Call Coordination");
                    callCoordination.sourceId = nccId;
                    callCoordination.destinationId = command.domainOfRequestedClient + ":NCC";
                    callCoordination.endClientId = command.endClientId;
                    callCoordination.startClientId = command.startClientId;
                    callCoordination.domainOfRequestedClient = command.domainOfRequestedClient;
                    callCoordination.domainOfRequestingClient = command.domainOfRequestingClient;
                    callCoordination.callIndicationClientId = command.startClientId;
                    callCoordination.bandwidth = command.bandwidth;
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, callCoordination);
                        stream.Flush();
                        stream.Close();
                    }
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Call Coordination' sent to {callCoordination.destinationId}" + Environment.NewLine));
                }
            }
        }

        private void CallConfirmed(CommandLibrary.Command command, Socket handler)
        {
            if (command.isClientAccepting)
            {
                if (command.domainOfRequestingClient == self.ReturnName())
                {
                    string[] temp = command.sourceId.Split(':');
                    if (temp.ElementAt(temp.Length - 1) == "NCC")//jeśli dostajemy request od drugiego NCC
                    {
                        //dodajemy połączenie
                        Connection conn = new Connection();
                        try { conn.connectionId = command.connectionId; }
                        catch { };
                        conn.usedLinks = command.linkList;
                        conn.startId = command.startClientId;
                        conn.endId = command.endClientId;
                        connectionList.Add(conn);
                        //
                        CommandLibrary.Command connectionRequest = new CommandLibrary.Command("Connection Request");
                        connectionRequest.sourceId = nccId;
                        connectionRequest.destinationId = self.ReturnMainSubnetwork().ReturnName() + ":CC";
                        connectionRequest.endClientId = callDestination;
                        connectionRequest.startClientId = callRequester;
                        connectionRequest.startOfPath = command.startClientId;
                        connectionRequest.endOfPath = command.domainOfRequestedClient;
                        connectionRequest.domainOfRequestedClient = command.domainOfRequestedClient;
                        connectionRequest.domainOfRequestingClient = command.domainOfRequestingClient;
                        connectionRequest.endCrack = command.endCrack;
                        //connectionRequest.linkList = new List<CommandLibrary.Link>();
                        //connectionRequest.linkList.AddRange(command.linkList);
                        connectionRequest.startCrack = command.startCrack;
                        connectionRequest.record = new CommandLibrary.PortMatch(0, 0, 0, 0);
                        connectionRequest.record.modulation = command.record.modulation;
                        connectionRequest.bandwidth = command.bandwidth;
                        using (var stream = new NetworkStream(handler))
                        {
                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Serialize(stream, connectionRequest);
                            stream.Flush();
                            stream.Close();
                        }
                        self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Connection Request' sent to {connectionRequest.destinationId}" + Environment.NewLine));
                    }
                    else//jeśli dostajemy request od CPCC
                    {
                        //dodajemy połączenie
                        Connection conn = new Connection();
                        conn.connectionId = connectionList.Count;
                        conn.usedLinks = command.linkList;
                        conn.startId = command.startClientId;
                        conn.endId = command.endClientId;
                        connectionList.Add(conn);
                        //
                        CommandLibrary.Command connectionRequest = new CommandLibrary.Command("Connection Request");
                        connectionRequest.sourceId = nccId;
                        connectionRequest.destinationId = self.ReturnMainSubnetwork().ReturnName() + ":CC";
                        connectionRequest.endClientId = command.endClientId;
                        connectionRequest.startClientId = command.startClientId;
                        connectionRequest.startOfPath = command.startClientId;
                        connectionRequest.endOfPath = command.endClientId;
                        connectionRequest.domainOfRequestedClient = command.domainOfRequestedClient;
                        connectionRequest.domainOfRequestingClient = command.domainOfRequestingClient;

                        connectionRequest.bandwidth = command.bandwidth;
                        using (var stream = new NetworkStream(handler))
                        {
                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Serialize(stream, connectionRequest);
                            stream.Flush();
                            stream.Close();
                        }
                        self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Connection Request' sent to {connectionRequest.destinationId}" + Environment.NewLine));
                    }
                }
                else//jesteśmy w innej domenie
                {
                    string[] temp = command.sourceId.Split(':');
                    if (temp.ElementAt(temp.Length - 1) == "NCC")//jeśli dostajemy request od NCC, chyba nigdy się nie zdarzy
                    {
                        CommandLibrary.Command connectionRequest = new CommandLibrary.Command("Connection Request");
                        connectionRequest.sourceId = nccId;
                        connectionRequest.destinationId = self.ReturnMainSubnetwork().ReturnName() + ":CC";
                        connectionRequest.endClientId = command.endClientId;
                        connectionRequest.startClientId = callRequester;
                        connectionRequest.startOfPath = command.startClientId;
                        connectionRequest.endOfPath = command.endClientId;
                        connectionRequest.domainOfRequestedClient = command.domainOfRequestedClient;
                        connectionRequest.domainOfRequestingClient = command.domainOfRequestingClient;

                        connectionRequest.bandwidth = command.bandwidth;
                        using (var stream = new NetworkStream(handler))
                        {
                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Serialize(stream, connectionRequest);
                            stream.Flush();
                            stream.Close();
                        }
                        self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Connection Reuqest' sent to {connectionRequest.destinationId}" + Environment.NewLine));
                    }
                    else//dostajemy request od CPCC
                    {
                        //dodajemy połączenie
                        Connection conn = new Connection();
                        conn.connectionId = connectionList.Count;
                        conn.usedLinks = command.linkList;
                        connectionList.Add(conn);
                        //
                        CommandLibrary.Command connectionRequest = new CommandLibrary.Command("Connection Request");
                        connectionRequest.sourceId = nccId;
                        connectionRequest.destinationId = self.ReturnMainSubnetwork().ReturnName() + ":CC";
                        connectionRequest.endClientId = command.endClientId;
                        connectionRequest.startClientId = callRequester;
                        connectionRequest.startOfPath = command.domainOfRequestingClient;//granica drugiej domeny
                        connectionRequest.endOfPath = command.endClientId;
                        connectionRequest.domainOfRequestedClient = command.domainOfRequestedClient;
                        connectionRequest.domainOfRequestingClient = command.domainOfRequestingClient;
                        connectionRequest.bandwidth = command.bandwidth;
                        using (var stream = new NetworkStream(handler))
                        {
                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Serialize(stream, connectionRequest);
                            stream.Flush();
                            stream.Close();
                        }
                        self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Connection Request' sent to {connectionRequest.destinationId}" + Environment.NewLine));
                    }
                }
            }
            else
            {
                //klient nie zezwala na nawiązanie połączenia
            }
        }

        private void ConnectionConfirmed(CommandLibrary.Command command, Socket handler)
        {
            if (self.ReturnName() == command.domainOfRequestingClient)
            {
                CommandLibrary.Command callConfirmed = new CommandLibrary.Command("Call confirmed");
                callConfirmed.sourceId = nccId;
                callConfirmed.destinationId = callRequester;
                callConfirmed.endClientId = command.endClientId;
                callConfirmed.startClientId = command.startClientId;
                callConfirmed.bandwidth = command.bandwidth;
                callConfirmed.isClientAccepting = true;
                callConfirmed.isClientAccessible = true;
                callConfirmed.callConfirmedClientId = callDestination;
                callConfirmed.domainOfRequestedClient = command.domainOfRequestedClient;
                callConfirmed.domainOfRequestingClient = command.domainOfRequestingClient;
                //dodane:
                callConfirmed.endCrack = command.endCrack;
                callConfirmed.startCrack = command.startCrack;
                callConfirmed.record = new CommandLibrary.PortMatch(0, 0, 0, 0);
                callConfirmed.record.modulation = command.record.modulation;
                //
                using (var stream = new NetworkStream(handler))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, callConfirmed);
                    stream.Flush();
                    stream.Close();
                }
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Call Confirmed' sent to {callConfirmed.destinationId}" + Environment.NewLine));
            }
            else
            {
                CommandLibrary.Command callConfirmed = new CommandLibrary.Command("Call confirmed");
                callConfirmed.sourceId = nccId;
                callConfirmed.destinationId = command.domainOfRequestingClient + ":NCC";
                callConfirmed.endClientId = command.endClientId;
                callConfirmed.startClientId = command.startClientId;
                callConfirmed.bandwidth = command.bandwidth;
                callConfirmed.callConfirmedClientId = callDestination;
                callConfirmed.isClientAccepting = true;
                callConfirmed.isClientAccessible = true;
                callConfirmed.domainOfRequestedClient = command.domainOfRequestedClient;
                callConfirmed.domainOfRequestingClient = command.domainOfRequestingClient;
                //callConfirmed.linkList = new List<CommandLibrary.Link>();
                //callConfirmed.linkList.AddRange(command.linkList);
                 //dodane:
                callConfirmed.endCrack = command.endCrack;
                callConfirmed.startCrack = command.startCrack;
                callConfirmed.record = new CommandLibrary.PortMatch(0, 0, 0, 0);
                callConfirmed.record.modulation = command.record.modulation;
                //
                //modulacja
                using (var stream = new NetworkStream(handler))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, callConfirmed);
                    stream.Flush();
                    stream.Close();
                }
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Call Confirmed' sent to {callConfirmed.destinationId}" + Environment.NewLine));

            }

        }

        private void CallCoordination(CommandLibrary.Command command, Socket handler)
        {

                CommandLibrary.Command callIndication = new CommandLibrary.Command("Call Indication");
                callIndication.sourceId = nccId;
                callIndication.destinationId = command.domainOfRequestedClient + ":" + command.endClientId + ":CPCC";
                callIndication.endClientId = command.endClientId;
                callIndication.startClientId = command.startClientId;
                callIndication.domainOfRequestedClient = command.domainOfRequestedClient;
                callIndication.domainOfRequestingClient = command.domainOfRequestingClient;
                callIndication.callIndicationClientId = command.startClientId;
                callIndication.bandwidth = command.bandwidth;
                
                using (var stream = new NetworkStream(handler))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, callIndication);
                    stream.Flush();
                    stream.Close();
                }
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> NCC >> 'Call Indication' sent to {callIndication.destinationId}" + Environment.NewLine));
        }
    }
}
