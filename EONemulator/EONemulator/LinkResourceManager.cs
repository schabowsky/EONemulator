using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace EONemulator
{


    class LinkResourceManager
    {
        public string LRMName;
        private string subnetworkName;
        private List<CommandLibrary.Link> linkList;
        Subnetwork self;
        IPEndPoint LRMIPEP;
        IPEndPoint serverIPEP;
        Socket socket;
        private int counter;
        private string host = System.Configuration.ConfigurationManager.AppSettings["host"];

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
                case "Link Usage":
                    LinkUsage(input, handler);
                    break;

                case "Link Slots Lock":
                    //self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> LINK SLOTS LOCK from {input.sourceId}" + Environment.NewLine));

                    LinkSlotsLock(input, handler);
                    counter = 1;
                    break;

                case "Link Slots Unlock":
                    //self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> LINK SLOTS UNLOCK from {input.sourceId}" + Environment.NewLine));
                    LinkSlotsUnlock(input, handler);
                    break;

                case "Link Add":
                    //self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> LINK ADD from {input.sourceId}" + Environment.NewLine));
                    LinkAdd(input, handler);
                    break;

                case "Link Delete":
                    //self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> LINK DELETE from {input.sourceId}" + Environment.NewLine));
                    LinkDelete(input, handler);
                    break;
                case "Clear Link":
                    ClearLink(input, handler);
                    break;
                case "Slots Locked":
                    if (counter == 1)
                    {
                        SlotsLocked(input, handler);
                        counter = 0;
                    }
                    break;

            }
        }

        private void ClearLink(CommandLibrary.Command command, Socket handler)
        {
            //linkList.Remove(new CommandLibrary.Link() { linkID = command.deletedLinkId });
            foreach (CommandLibrary.Link l in command.linkList)
            {
                foreach (CommandLibrary.Link ll in linkList)
                    if (l.linkID == ll.linkID)
                    {
                        ll.usedSlots.Clear();
                        self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> Link {ll.linkID} cleared" + Environment.NewLine));
                    }
            }
        }

        private void SlotsLocked(CommandLibrary.Command command, Socket handler)
        {
            command.destinationId = self.ReturnName() + ":CC";
            command.sourceId = LRMName;

            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, command);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> 'Subcarrier Slots Lock Request' sent to {command.destinationId}" + Environment.NewLine));
        }

        private void LinkUsage(CommandLibrary.Command command, Socket handler)
        {
            CommandLibrary.Command response = new CommandLibrary.Command("Links Ready");
            response.destinationId = self.ReturnName() + ":CC";
            response.sourceId = LRMName;
            response.domainOfRequestedClient = command.domainOfRequestedClient;
            response.domainOfRequestingClient = command.domainOfRequestingClient;
            response.linkList = new List<CommandLibrary.Link>();
            response.bandwidth = command.bandwidth;

            foreach (CommandLibrary.Link l in command.linkList)
            {
                foreach (CommandLibrary.Link ll in linkList)
                {
                    if (l.linkID == ll.linkID)
                    {
                        l.usedSlots.AddRange(ll.usedSlots);
                        response.linkList.Add(l);

                    }
                }
            }

            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, response);
                stream.Flush();
                stream.Close();
            }
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> Checking usage of links" + Environment.NewLine));
        }

        // LRM odbiera tablice linków, linki mają mieć wypełnione ID i sloty

        private void LinkSlotsLock(CommandLibrary.Command command, Socket handler)
        {
           
            List<int> distinctID = new List<int>();
            List<CommandLibrary.Link> distinctLink = new List<CommandLibrary.Link>();
            string linkString = "";

            foreach (CommandLibrary.Link l in command.linkList)
            {
                if (!distinctID.Contains(l.linkID))
                {
                    distinctLink.Add(l);
                    distinctID.Add(l.linkID);
                }
            }

            if (distinctLink.Count() != 0)
            {
                if (distinctLink[0].usedSlots.Count() != 0)

                {
                    int min = distinctLink[0].usedSlots.Min();
                    int max = distinctLink[0].usedSlots.Max();

                    foreach (CommandLibrary.Link l in linkList)
                    {
                        foreach (CommandLibrary.Link ll in distinctLink)
                        {

                            if (l.linkID == ll.linkID)
                            {
                                linkString += l.linkID.ToString() + "  ";

                                foreach (int k in ll.usedSlots)
                                {
                                    if (!(l.usedSlots.Contains(k)))
                                    {
                                        l.usedSlots.Add(k);


                                    }
                                    else
                                    {
                                    //    Console.WriteLine("proba nadpisania slotow w " + LRMName);
                                    }
                                }
                            }

                        }
                    }

                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> Subcarrier slots locked from " + min + " to " + max + " in links  " + linkString + Environment.NewLine));                   
                   

                    if (self.ReturnSubnetworks() == null || self.ReturnSubnetworks().Count == 0)
                    {
                        command.commandType = "Slots Locked";
                        command.destinationId = command.sourceId;
                        command.sourceId = LRMName;

                        using (var stream = new NetworkStream(handler))
                        {
                            BinaryFormatter bformatter = new BinaryFormatter();
                            bformatter.Serialize(stream, command);
                            stream.Flush();
                            stream.Close();
                        }

                        //self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> SLOTS LOCKED sent to {command.destinationId}" + Environment.NewLine));
                    }
                    else
                    {
                        foreach (Subnetwork s in self.ReturnSubnetworks())
                        {
                            command.destinationId = s.ReturnName() + ":LRM";
                            command.sourceId = LRMName;
                            using (var stream = new NetworkStream(handler))
                            {
                                BinaryFormatter bformatter = new BinaryFormatter();
                                bformatter.Serialize(stream, command);
                                stream.Flush();
                                stream.Close();
                            }
                           // self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> LINK SLOTS LOCK sent to {command.destinationId}" + Environment.NewLine));

                        }
                    }
                }
            }
        }

        // LRM odbiera tablice linków, linki mają mieć wypełnione ID i sloty

        private void LinkSlotsUnlock(CommandLibrary.Command command, Socket handler)
        {
            //    CommandLibrary.Command response = new CommandLibrary.Command("SLOTS UNLOCKED");
            //    response.destinationId = command.sourceId;
            //    response.sourceId = LRMName;
            //    foreach (CommandLibrary.Link l in linkList)
            //    {
            //        foreach (CommandLibrary.Link ll in command.linkList)
            //        {
            //            if (l.linkID == ll.linkID)
            //            {
            //                foreach (int k in ll.usedSlots)
            //                {
            //                    if (l.usedSlots.Contains(k))
            //                    {
            //                        l.usedSlots.Remove(k);
            //                    }
            //                    else
            //                    {
            //                        self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> LRM >> ERROR slot byl pusty !!!!" + Environment.NewLine));
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    using (var stream = new NetworkStream(handler))
            //    {
            //        BinaryFormatter bformatter = new BinaryFormatter();
            //        bformatter.Serialize(stream, response);
            //        stream.Flush();
            //        stream.Close();
            //    }
            //    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> SLOTS UNLOCKED from {response.destinationId}" + Environment.NewLine));
        }

        // LRM odbiera tablice linków, linki mają mieć wypełnione ID

        private void LinkAdd(CommandLibrary.Command command, Socket handler)
        {
            //CommandLibrary.Command response = new CommandLibrary.Command("LINK ADDED");
            //response.destinationId = command.sourceId;
            //response.sourceId = LRMName;
            //foreach (CommandLibrary.Link l in command.linkList)
            //{
            //    if (!linkList.Contains(new CommandLibrary.Link { linkID = l.linkID }))
            //    {
            //        linkList.Add(l);
            //    }
            //}
            //using (var stream = new NetworkStream(handler))
            //{
            //    BinaryFormatter bformatter = new BinaryFormatter();
            //    bformatter.Serialize(stream, response);
            //    stream.Flush();
            //    stream.Close();
            //}
            //self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> LINK ADDED from {response.destinationId}" + Environment.NewLine));
        }

        // LRM odbiera tablice linków, linki mają mieć wypełnione ID

        private void LinkDelete(CommandLibrary.Command command, Socket handler)
        {
            foreach (CommandLibrary.Link l in linkList)
            {
                //zmienna
                if (l.linkID == command.deletedLinkId)
                {

                    CommandLibrary.Command response = new CommandLibrary.Command("Link Deleted");
                    response.destinationId = self.ReturnName() + ":CC";
                    response.sourceId = LRMName;
                    //zmienna
                    response.deletedLinkId = command.deletedLinkId;
                    response.domainOfRequestedClient = command.domainOfRequestedClient;
                    response.domainOfRequestingClient = command.domainOfRequestingClient;
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, response);
                        stream.Flush();
                        stream.Close();
                    }
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> LRM >> Link " + l.linkID + " deleted" + Environment.NewLine));

                }

            }
            // zmienna
            linkList.Remove(new CommandLibrary.Link() { linkID = command.deletedLinkId });

        }



        public LinkResourceManager(string lrmId, int lrmPort, Subnetwork selfSub)
        {
            LRMName = lrmId;
            self = selfSub;
            subnetworkName = self.ReturnName();
            linkList = new List<CommandLibrary.Link>();
            Reader();
            LRMIPEP = new IPEndPoint(IPAddress.Parse(host), lrmPort);
            serverIPEP = new IPEndPoint(IPAddress.Parse(host), 50000);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(LRMIPEP);
            socket.BeginConnect(serverIPEP, new AsyncCallback(ConnectCallback), socket);
            Console.WriteLine("finish");
        }

        public LinkResourceManager(LinkResourceManager LRM)
        {
            LRMName = LRM.LRMName;
            subnetworkName = LRM.subnetworkName;
            linkList = new List<CommandLibrary.Link>();
            linkList.AddRange(LRM.linkList);
            self = LRM.self;
        }

        public void Reader()
        {
            string[] split = self.ReturnName().Split(new char[] { ':' });
            string join = string.Join("", split);
            var path = System.Configuration.ConfigurationManager.AppSettings[join + "LRM"];
            XmlTextReader reader = new XmlTextReader(path);
            int linkID;
            string Object1;
            string Object2;
            int portObject1;
            int portObject2;
            int length;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "link")
                        {
                            linkID = Int32.Parse(reader.GetAttribute("id"));
                            Object1 = reader.GetAttribute("o1");
                            Object2 = reader.GetAttribute("o2");
                            portObject1 = Int32.Parse(reader.GetAttribute("p1"));
                            portObject2 = Int32.Parse(reader.GetAttribute("p2"));
                            length = Int32.Parse(reader.GetAttribute("len"));
                            CommandLibrary.Link l = new CommandLibrary.Link(linkID, Object1, Object2, portObject1, portObject2, length);
                            l.usedSlots = new List<int>();
                            linkList.Add(l);
                        }
                        break;
                    case XmlNodeType.Text:
                        Console.WriteLine(reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "link")
                        {
                            Console.WriteLine("End mark reached");
                        }
                        break;
                }
            }
        }


        //public void AddLink(int id, string O1, string O2, int pO1, int pO2, int len)
        //{
        //    Link l = new Link(id, O1, O2, pO1, pO2, len);
        //    linkList.Add(l);
        //}

        //public void AddLink(Link l)
        //{
        //    linkList.Add(l);
        //}

        //public void AddUsedSlots(Link l, int begin, int end)
        //{
        //    l.AddUsedSlots(begin, end);
        //}

        //public void DeleteUsedSlots(Link l, int begin, int end)
        //{
        //    l.DeleteUsedSlots(begin, end);
        //}

        //public void SlotsRequestHandler(int ID)
        //{
        //    linkList.Find(x => x.linkID == ID);
        //}

    }
}
