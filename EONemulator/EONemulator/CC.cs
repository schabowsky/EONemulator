using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLibrary;

namespace EONemulator
{
    class CC
    {
        private string ccId;
        private int ccPort;
        private Subnetwork self;
        private string confirm_id;
        private List<CommandLibrary.Link> path;
        private int counter;
        private string host = System.Configuration.ConfigurationManager.AppSettings["host"];
        private Socket socket;
        private IPEndPoint serverIPEP;
        private IPEndPoint ccIPEP;
        private List<String> destinationCC;
        private List<Connection> connectionList;
        private int startCrack;
        private int endCrack;
        private string modulation;
        private string startId;
        private string endId;
        

        public class Connection
        {
            public int connectionId;
            public string startId;
            public string endId;
            public List<CommandLibrary.Link> usedLinks;//id użytych linków
            public string modulation;//raczej nie użyjemy, bo zestawiamy na nowo i nie wiadomo jaka jest droga
        }

        public CC(string ccId, int ccPort, Subnetwork self)
        {
            connectionList = new List<Connection>();
            this.ccPort = ccPort;
            this.ccId = ccId;
            this.self = self;
            ccIPEP = new IPEndPoint(IPAddress.Parse(host), ccPort);
            serverIPEP = new IPEndPoint(IPAddress.Parse(host), 50000);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ccIPEP);
            socket.BeginConnect(serverIPEP, new AsyncCallback(ConnectCallback), socket);
            counter = 0;
            path = new List<CommandLibrary.Link>();
            startCrack = 0;
            endCrack = 0;
            modulation = string.Empty;
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
                case "Connection Request":
                    if (input.domainOfRequestedClient != input.domainOfRequestingClient)
                    {
                        startCrack = input.startCrack;
                        endCrack = input.endCrack;
                    }
                    path.Clear();
                    if (self.ReturnSubnetworks().Count != 0)
                    {
                        startId = input.startClientId;
                        endId = input.endClientId;
                    }
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Connection Request ({input.startClientId}, {input.endClientId})' from {input.sourceId}" + Environment.NewLine));
                    SetPath(input, handler);
                    confirm_id = input.sourceId;
                    break;
                case "Path Ready":
                    path.AddRange(input.linkList);
                 
                    Connection conn = new Connection();
                    conn.connectionId = input.connectionId;
                    conn.usedLinks = new List<CommandLibrary.Link>();
                    conn.usedLinks.AddRange(input.linkList);
                    connectionList.Add(conn);

                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Query Solved' from {input.sourceId}" + Environment.NewLine));
                    LinkUsage(input, handler);
                    break;
                case "Links Ready":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Links Usage Response' from {input.sourceId}" + Environment.NewLine));
                    if (self.ReturnSubnetworks().Count == 0 || self.ReturnSubnetworks() == null)
                        GoToUpperCC(input, handler);
                    else
                        NextCC(input, handler);
                    break;
                case "Connection Found":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Connection Found' from {input.sourceId}" + Environment.NewLine));
                    ConfirmationFromLowerCC(input, handler);
                    break;
                case "Selected Subcarriers":
                    if (input.startCrack != 0)
                    {
                        startCrack = input.startCrack;
                        endCrack = input.endCrack;
                    }
                    else
                    {
                        input.startCrack = startCrack;
                        input.endCrack = endCrack;
                    }
                    modulation = input.record.modulation;
                    int cracks = input.endCrack - input.startCrack + 1;
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> Signal Defined - modulation: {modulation}, subcarriers: {cracks}" + Environment.NewLine));
                    //if (input.domainOfRequestedClient != input.domainOfRequestingClient)
                    //{
                        foreach (Link l in input.linkList)
                        {
                            for (int i = startCrack; i <= endCrack; i++)
                            {
                                l.usedSlots.Add(i);
                            }
                        }
                    // }
                    //if (input.linkList == null)
                        input.linkList.AddRange(path);

                    // powstana bledy w distinct LRM jak będą puste linki w path?

                   MakeLRMAllocating(input, handler);
                  
                   // EmitPath(input, handler);
                   // ConnectionConfirmed(input, handler);
                    break;
                case "Connection Confirmed":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Connection Confirmed' from {input.sourceId}" + Environment.NewLine));
                    
                        input.startCrack = startCrack;
                        input.endCrack = startCrack;
                    
                    ConnectionConfirmed(input, handler); // Connection confirmed

                    break;
                case "Set Routers":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'OXC's Configuration' from {input.sourceId}" + Environment.NewLine));
                    SetRouters(input, handler);
                    break;
                case "Just Confirm":
                    //self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> SET ROUTERS from {input.sourceId}" + Environment.NewLine));
                    GoToUpperCC(input, handler);
                    break;
                case "Link Deleted":
                    LinkDeleted(input, handler);
                    break;
                case "Critical Link Deleted":
                    CriticalLinkDeleted(input, handler);
                    break;
                case "Clear OXC":
                    ClearOXC(input, handler);
                    break;
                case "OXC Cleared":
                    OXCCleared(input, handler);
                    break;
                case "Slots Locked":
                    EmitPath(input, handler);
                    ConnectionConfirmed(input, handler);
                    break;
            }
        }

        private void SlotsLocked(Command input, Socket handler)
        {
            
        }

        private void OXCCleared( CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command response = new CommandLibrary.Command("OXC Cleared");
            Connection conn = connectionList.Find(x => x.connectionId == command.connectionId);
            if (conn != null)
                connectionList.Remove(conn);

            response.connectionId = command.connectionId;
            response.deletedLinkId = command.deletedLinkId;
            string[] s = self.ReturnName().Split(':');
            response.destinationId = s[0] + ":NCC";
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, response);
                stream.Flush();
                stream.Close();
            }
        }

        private void ClearOXC(CommandLibrary.Command command, Socket handler)
        {
            //Chciałem poprawić, a zjebałem.Trzeba tu ustalić jakas funkcje zeby po routerach rozsylala

            Connection Conn = connectionList.Find(x => x.connectionId == command.connectionId);

            if (Conn == null)
            {
                Conn = new Connection();
                Conn.connectionId = command.connectionId;
                Conn.usedLinks = command.linkList;
            }
            //List<string> desR = new List<string>();
            //foreach (CommandLibrary.Link link in Conn.usedLinks)
            //{
            //    if (!desR.Contains(link.Object1) && link.Object1.Contains('X') && link.Object1.Contains(self.ReturnName()))
            //        desR.Add(link.Object1);
            //    if (!desR.Contains(link.Object2) && link.Object2.Contains('X') && link.Object2.Contains(self.ReturnName()))
            //        desR.Add(link.Object2);
            //}



            CommandLibrary.Command response = new CommandLibrary.Command("Link Delete");
            response.connectionId = command.connectionId;
            response.deletedLinkId = command.deletedLinkId;
            response.destinationId = self.ReturnName() + ":RC";
            response.sourceId = ccId;
            //
            if (Conn !=null)
            response.linkList = Conn.usedLinks;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, response);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Link Delete' send to {response.destinationId}" + Environment.NewLine));

            response.commandType = "Clear Link";
            response.destinationId = self.ReturnName() + ":LRM";

            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, response);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Link Delete' send to {response.destinationId}" + Environment.NewLine));


            if (self.ReturnSubnetworks().Count != 0)
            {
                foreach (Subnetwork s in self.ReturnSubnetworks())
                {
                    // clear oxc
                    response.commandType = "Clear OXC";
                    response.destinationId = s.ReturnName() + ":CC";
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, response);
                        stream.Flush();
                        stream.Close();
                    }
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Clear OXC' send to {response.destinationId}" + Environment.NewLine));
                }
            }
            else
            {
                Thread.Sleep(200);
                List<NetworkNode> nn = self.ReturnNodes();
                foreach (NetworkNode n in nn)
                {
                    CommandLibrary.Command oxcRequest = new CommandLibrary.Command("PortMatch Delete");
                    oxcRequest.connectionId = command.connectionId;
                    oxcRequest.deletedLinkId = command.deletedLinkId;
                    oxcRequest.destinationId = n.ReturnName();
                    if (Conn != null)
                        oxcRequest.linkList = Conn.usedLinks;
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, oxcRequest);
                        stream.Flush();
                        stream.Close();
                    }

                    


                    CommandLibrary.Command back = new CommandLibrary.Command("OXC Clear");
                    back.connectionId = command.connectionId;
                    back.deletedLinkId = command.deletedLinkId;
                    string[] s = self.ReturnName().Split(':');
                    back.destinationId = s[0] + ":" + s[1] + ":CC";
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, back);
                        stream.Flush();
                        stream.Close();
                    }
                }
            }

            if (self.ReturnSubnetworks().Count != 0)
            {
                Thread.Sleep(500);
                CommandLibrary.Command back = new CommandLibrary.Command("OXC Cleared");
                back.connectionId = command.connectionId;
                back.deletedLinkId = command.deletedLinkId;
                back.linkList = command.linkList;
                back.destinationId = ccId;

                using (var stream = new NetworkStream(handler))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, back);
                    stream.Flush();
                    stream.Close();
                }
            }
        }

        private void LinkDeleted(CommandLibrary.Command command, Socket handler)
        {
            foreach (Connection c in connectionList)
            {
                foreach (CommandLibrary.Link l in c.usedLinks)
                {
                    if (l.linkID == command.deletedLinkId)
                    {
                        CommandLibrary.Command response = new CommandLibrary.Command("Critical Link Deleted");
                        response.connectionId = c.connectionId;
                        response.deletedLinkId = command.deletedLinkId;
                        if (self.ReturnSubnetworks().Count != 0)
                        {
                            string[] s = self.ReturnName().Split(':');
                            response.destinationId = s[0] + ":NCC";
                        }
                        else
                        {
                            string[] s = self.ReturnName().Split(':');
                            response.destinationId = s[0] + ":" + s[1] + ":CC";
                        }
                        using (var stream = new NetworkStream(handler))
                        {
                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Serialize(stream, response);
                            stream.Flush();
                            stream.Close();
                        }
                        break;
                    }
                }
            }
        }
        private void CriticalLinkDeleted(CommandLibrary.Command command, Socket handler)
        {
            string[] s = self.ReturnName().Split(':');
            command.destinationId = s[0] + ":NCC";
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, command);
                stream.Flush();
                stream.Close();
            }
        }

         private void SetRouters(CommandLibrary.Command command, Socket handler)
         {
            List<string> destinationRouters = new List<string>();

            foreach (CommandLibrary.Link link in command.linkList)
            {
                if (!link.Object1.Contains("K") && !destinationRouters.Contains(link.Object1) && link.Object1.Contains('X') && link.Object1.Contains(self.ReturnName()))
                {
                    destinationRouters.Add(link.Object1);
                }
                if (!link.Object2.Contains("K") && !destinationRouters.Contains(link.Object2) && link.Object2.Equals('X') && link.Object2.Contains(self.ReturnName()))
                {
                    destinationRouters.Add(link.Object2);
                }
            }
            CommandLibrary.Command send = new CommandLibrary.Command("Set");
            send.bandwidth = command.bandwidth;

            int minSub = command.linkList[0].usedSlots.Min();
            int maxSub = command.linkList[0].usedSlots.Max();
            

            send.record = new CommandLibrary.PortMatch(0, 0, minSub, maxSub);
            
            send.record.modulation = command.record.modulation;
            
            //modulation everywhere
            foreach (string destR in destinationRouters)
            {
                send.record.routerName = destR;
                List<CommandLibrary.Link> temp = new List<CommandLibrary.Link>();
                foreach (CommandLibrary.Link link in command.linkList)
                {
                    if (link.Object1.Equals(destR) || link.Object2.Equals(destR))
                    {
                        temp.Add(link);
                    }
                }
                List<int> ports = new List<int>(); ;
                foreach (CommandLibrary.Link link in temp)
                {
                    if (link.Object1.Equals(destR))
                        ports.Add(link.portObject1);
                    if (link.Object2.Equals(destR))
                        ports.Add(link.portObject2);
                }
                send.sourceId = ccId;
                send.destinationId = destR;
                send.record.receivedPort = ports[0];
                send.domainOfRequestedClient = command.domainOfRequestedClient;
                send.domainOfRequestingClient = command.domainOfRequestingClient;
                send.record.destinationPort = ports[1];
                send.record.connectionFreq = command.record.connectionFreq;
                using (var stream = new NetworkStream(handler))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, send);
                    stream.Flush();
                    stream.Close();
                }
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> Configuration sent to {send.destinationId}" + Environment.NewLine));
            }
        }
        private void EmitPath(CommandLibrary.Command command, Socket handler)
        {
            foreach (CommandLibrary.Link link in command.linkList)
            {
                foreach (Subnetwork s in self.ReturnSubnetworks())
                {
                    foreach (NetworkNode n in s.ReturnNodes())
                    {
                        List<int> temp = n.ReturnPorts();
                        if (temp.Contains(link.portObject1))
                        {
                            link.Object1 = n.ReturnName();
                        }
                        else if (temp.Contains(link.portObject2))
                        {
                            link.Object2 = n.ReturnName();
                        }
                    }
                }
            }

            for (int i = 0; i < destinationCC.Count; i++)
            {
                List<CommandLibrary.Link> destLinkList = new List<CommandLibrary.Link>();
                foreach (CommandLibrary.Link link in command.linkList)
                {
                    if ((link.Object1.Contains(destinationCC[i]) && link.Object1.Contains('X')) || (link.Object2.Contains(destinationCC[i]) && link.Object2.Contains('X')))
                    {
                        destLinkList.Add(link);
                    }
                }
                CommandLibrary.Command send = new CommandLibrary.Command("Set Routers");
                send.sourceId = ccId;
                send.destinationId = destinationCC[i] + ":CC";
                send.bandwidth = command.bandwidth;
                send.linkList = destLinkList;
                send.domainOfRequestedClient = command.domainOfRequestedClient;
                send.domainOfRequestingClient = command.domainOfRequestingClient;
                send.record = command.record;
                

                if ((endId.Contains("K1") && startId.Contains("K2")) || (endId.Contains("K2") && startId.Contains("K1")))
                    send.record.connectionFreq = 235.3;
                else if ((endId.Contains("K1") && startId.Contains("K3")) || (endId.Contains("K3") && startId.Contains("K1")))
                    send.record.connectionFreq = 236.3;
                else if ((endId.Contains("K3") && startId.Contains("K2")) || (endId.Contains("K2") && startId.Contains("K3")))
                    send.record.connectionFreq = 234.3;

                using (var stream = new NetworkStream(handler))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, send);
                    stream.Flush();
                    stream.Close();
                }
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >>  New configuration sent to {send.destinationId}" + Environment.NewLine));
                //counter++;
            }

        }
        private void NextCC(CommandLibrary.Command command, Socket handler)
        {
            destinationCC = new List<string>();
            List<int> fromCC = new List<int>();
            List<int> whereCC = new List<int>();
            
           
            foreach (CommandLibrary.Link link in command.linkList)
            {
                if (!link.Object1.Contains("K") && !destinationCC.Contains(link.Object1) && link.Object1.Length > 1)
                {
                    destinationCC.Add(link.Object1);
                }
                if (!link.Object2.Contains("K") && !destinationCC.Contains(link.Object2) && link.Object2.Length > 1)
                {
                    destinationCC.Add(link.Object2);
                }
            }
            foreach (String dest in destinationCC)
            {
                List<CommandLibrary.Link> temp = new List<CommandLibrary.Link>();
                string destiny = "";
                foreach (CommandLibrary.Link link in command.linkList)
                {
                    if (link.Object1.Equals(dest) || link.Object2.Equals(dest))
                    {
                        temp.Add(link);
                        destiny = dest;
                    }
                }
                List<int> way = new List<int>();
                foreach (CommandLibrary.Link l in temp)
                {
                    if (l.Object1.Equals(destiny))
                    {
                        way.Add(l.portObject1);
                        continue;
                    }
                    if (l.Object2.Equals(destiny))
                    {
                        way.Add(l.portObject2);
                    }
                }
                fromCC.Add(way[0]);
                whereCC.Add(way[1]);
            }
            for (int i = 0; i < destinationCC.Count; i++)
            {
                CommandLibrary.Command send = new CommandLibrary.Command("Connection Request");
                send.sourceId = ccId;
                send.destinationId = destinationCC[i] + ":CC";
                send.startOfPathPort = fromCC[i];
                send.endOfPathPort = whereCC[i];
                send.domainOfRequestedClient = command.domainOfRequestedClient;
                send.domainOfRequestingClient = command.domainOfRequestingClient;
                send.bandwidth = command.bandwidth;
                send.linkList = command.linkList;
                using (var stream = new NetworkStream(handler))
                {
                    BinaryFormatter bformatter = new BinaryFormatter();
                    bformatter.Serialize(stream, send);
                    stream.Flush();
                    stream.Close();
                }
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Connection Request ({send.startOfPathPort}, {send.endOfPathPort})' sent to {send.destinationId}" + Environment.NewLine));
                counter++;
            }
        }
        private void LinksReady(CommandLibrary.Command command, Socket handler)//używamy tego?
        {
            CommandLibrary.Command response = new CommandLibrary.Command("Connection Confirmed");
            response.sourceId = ccId;
            response.destinationId = command.sourceId;
            response.domainOfRequestedClient = command.domainOfRequestedClient;
            response.domainOfRequestingClient = command.domainOfRequestingClient;
            response.bandwidth = command.bandwidth;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, response);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Connection Confirmed' sent to {response.destinationId}" + Environment.NewLine));
        }
        private void ConnectionConfirmed(CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command response = new CommandLibrary.Command("Connection confirmed");
            response.sourceId = ccId;
            response.domainOfRequestedClient = command.domainOfRequestedClient;
            response.domainOfRequestingClient = command.domainOfRequestingClient;
            //
            response.record = new PortMatch(0, 0, 0, 0);
            response.record.modulation = modulation;
            response.startCrack = startCrack;
            response.endCrack = endCrack;
            //
            response.destinationId = confirm_id;
            response.linkList = command.linkList;
            response.bandwidth = command.bandwidth;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, response);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Connection Confirmed' sent to {response.destinationId}" + Environment.NewLine));
            modulation = string.Empty;
            startCrack = 0;
            endCrack = 0;
        }

        private void LinkUsage(CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command linkUsage = new CommandLibrary.Command("Link Usage");
            linkUsage.sourceId = ccId;
            linkUsage.destinationId = self.ReturnName() + ":LRM";
            linkUsage.linkList = command.linkList;
            linkUsage.domainOfRequestedClient = command.domainOfRequestedClient;
            linkUsage.domainOfRequestingClient = command.domainOfRequestingClient;
            linkUsage.bandwidth = command.bandwidth;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, linkUsage);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Links Usage Request' sent to {linkUsage.destinationId}" + Environment.NewLine));
        }

        private void SetPath(CommandLibrary.Command command, Socket handler)
        {
            if (command.record != null)
            {
                if (!String.IsNullOrEmpty(command.record.modulation))
                {
                    modulation = command.record.modulation;
                    startCrack = command.startCrack;
                    endCrack = command.endCrack;
                }
            }
            CommandLibrary.Command setPath = new CommandLibrary.Command("Set Path");
            setPath.destinationId = self.ReturnName() + ":RC";
            setPath.sourceId = ccId;
            if (command.linkList != null)
                setPath.linkList = command.linkList;
            else
                setPath.linkList = new List<Link>();
            setPath.domainOfRequestedClient = command.domainOfRequestedClient;
            setPath.domainOfRequestingClient = command.domainOfRequestingClient;
            setPath.bandwidth = command.bandwidth;
            if (String.IsNullOrEmpty(command.startOfPath) || String.IsNullOrEmpty(command.endOfPath))
            {
                if (command.startOfPathPort != 0 &&  command.endOfPathPort != 0)
                {
                    setPath.startOfPathPort = command.startOfPathPort;
                    setPath.endOfPathPort = command.endOfPathPort;
                }
                else
                {
                    if (command.startClientId.Contains("K"))
                    {
                        string[] id = command.startClientId.Split(':');
                        setPath.startOfPath = id[1];
                    }
                    setPath.endOfPath = command.endOfPath;
                    //setPath.startOfPath = command.startClientId;
                    /*foreach (CommandLibrary.Link l in command.linkList)
                    {
                        string[] temp = l.Object1.Split(':');
                        string[] temp2 = l.Object2.Split(':');
                        string[] dom = self.ReturnName().Split(':');
                        if (temp[0] == dom[0])
                        {
                            foreach (Subnetwork s in self.ReturnSubnetworks())
                            {
                                foreach (NetworkNode n in s.ReturnNodes())
                                {
                                    List<int> tempL = n.ReturnPorts();
                                    if (tempL.Contains(l.portObject1))
                                    {
                                        setPath.endOfPath = n.ReturnName();
                                        l.Object1 = n.ReturnName();
                                    }
                                }
                            }
                        }
                        else if (temp2[0] == dom[0])
                        {
                            foreach (Subnetwork s in self.ReturnSubnetworks())
                            {
                                foreach (NetworkNode n in s.ReturnNodes())
                                {
                                    List<int> tempL = n.ReturnPorts();
                                    if (tempL.Contains(l.portObject2))
                                    {
                                        setPath.endOfPath = n.ReturnName();
                                        l.Object2 = n.ReturnName();
                                    }
                                }
                            }
                        }
                    }*/
                    setPath.linkList.Clear();
                }
            }
            else
            {
                setPath.startOfPath = command.startOfPath;
                setPath.endOfPath = command.endOfPath;
            }
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, setPath);
                stream.Flush();
                stream.Close();
            }
            if (String.IsNullOrEmpty(command.startOfPath))
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Route Table Query ({setPath.startOfPathPort}, {setPath.endOfPathPort})' sent to {setPath.destinationId}" + Environment.NewLine));
            else
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Route Table Query ({setPath.startOfPath}, {setPath.endOfPath})' sent to {setPath.destinationId}" + Environment.NewLine));
        }

        private void GoToUpperCC(CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command connectionFound = new CommandLibrary.Command("Connection Found");
            connectionFound.sourceId = ccId;
            string[] splitted = ccId.Split(':');
            string destination = string.Empty;
            for (int i = 0; i < splitted.Length - 2; i++)
                destination += splitted[i] + ":";
            connectionFound.destinationId = destination + "CC";
            connectionFound.linkList = command.linkList;
            connectionFound.domainOfRequestedClient = command.domainOfRequestedClient;
            connectionFound.domainOfRequestingClient = command.domainOfRequestingClient;
            connectionFound.bandwidth = command.bandwidth;
            Connection c = connectionList.Find(x => x.connectionId == command.connectionId);
            if (c != null)
                c.usedLinks.AddRange(command.linkList);
            
            //
            Thread.Sleep(300);
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, connectionFound);
                stream.Flush();
                stream.Close();
            }
            //self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> CC >> Connection Found" + Environment.NewLine));
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Connection Confirmed' sent to {connectionFound.destinationId}" + Environment.NewLine));
        }

        private void ConfirmationFromLowerCC(CommandLibrary.Command command, Socket handler)
        {
            counter--;
            foreach (CommandLibrary.Link l in command.linkList)
                if (!path.Contains(l))
                    path.Add(l);
            if (counter == 0)
            {
                //dodajemy połączenie
                //Connection conn = new Connection();
                //conn.connectionId = command.connectionId;
                //conn.usedLinks = command.linkList;
                //connectionList.Add(conn);
                //
                Connection c = connectionList.Find(x => x.connectionId == command.connectionId);
                if (c != null)
                    c.usedLinks.AddRange(command.linkList);
                if (!String.IsNullOrEmpty(modulation) && endCrack != 0)
                {
                    CommandLibrary.Command selectedSubcarriers = new CommandLibrary.Command("Selected Subcarriers");
                    selectedSubcarriers.sourceId = "";
                    selectedSubcarriers.destinationId = self.ReturnName() + ":CC";
                    selectedSubcarriers.bandwidth = command.bandwidth;
                    selectedSubcarriers.linkList = command.linkList;
                    selectedSubcarriers.domainOfRequestedClient = command.domainOfRequestedClient;
                    selectedSubcarriers.domainOfRequestingClient = command.domainOfRequestingClient;
                    selectedSubcarriers.record = new PortMatch(0, 0, startCrack, startCrack + endCrack);
                    selectedSubcarriers.record.modulation = modulation;
                    selectedSubcarriers.startCrack = startCrack;
                    selectedSubcarriers.endCrack = endCrack;
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, selectedSubcarriers);
                        stream.Flush();
                        stream.Close();
                    }
                    startCrack = 0;
                    endCrack = 0;
                    modulation = string.Empty;
                }
                else
                {
                    CommandLibrary.Command pathFound = new CommandLibrary.Command("Path Found");
                    pathFound.sourceId = ccId;
                    pathFound.destinationId = self.ReturnName() + ":RC";
                    pathFound.linkList = path;
                    pathFound.domainOfRequestedClient = command.domainOfRequestedClient;
                    pathFound.domainOfRequestingClient = command.domainOfRequestingClient;
                    pathFound.bandwidth = command.bandwidth;
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, pathFound);
                        stream.Flush();
                        stream.Close();
                    }
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> Full Path Found" + Environment.NewLine));
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Signal Parameters Request' sent to {pathFound.destinationId}" + Environment.NewLine));
                }
                counter = 0;
            }
        }

        private void MakeLRMAllocating(CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command allocate = new CommandLibrary.Command("Link Slots Lock");
            allocate.sourceId = ccId;
            allocate.destinationId = self.ReturnName() + ":LRM";
            allocate.linkList = command.linkList;
            allocate.domainOfRequestedClient = command.domainOfRequestedClient;
            allocate.domainOfRequestingClient = command.domainOfRequestingClient;
            allocate.bandwidth = command.bandwidth;
            allocate.record = command.record;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, allocate);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> CC >> 'Link Connection Request' sent to {allocate.destinationId}" + Environment.NewLine));
        }
    }
}
