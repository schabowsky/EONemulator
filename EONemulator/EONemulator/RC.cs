using CommandLibrary;
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
    class RC
    {
        private List<Vertex> nodes;
        private List<Edge> edges;
        private string rcId;
        private int rcPort;
        private Subnetwork self;
        private List<Link> links;

        private string host = System.Configuration.ConfigurationManager.AppSettings["host"];
        private Socket socket;
        private IPEndPoint serverIPEP;
        private IPEndPoint rcIPEP;

        public RC(string rcId, int rcPort, Subnetwork self)
        {
            this.rcPort = rcPort;
            this.rcId = rcId;
            this.self = self;
            links = new List<Link>();
            Reader();
            rcIPEP = new IPEndPoint(IPAddress.Parse(host), rcPort);
            serverIPEP = new IPEndPoint(IPAddress.Parse(host), 50000);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(rcIPEP);
            socket.BeginConnect(serverIPEP, new AsyncCallback(ConnectCallback), socket);
        }
        public void Reader()
        {
            string[] split = self.ReturnName().Split(new char[] { ':' });
            string join = string.Join("", split);
            var path = System.Configuration.ConfigurationManager.AppSettings[join + "RC"];
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
                            links.Add(l);
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
                case "Set Path":
                    SetPath(input, handler);
                    break;
                case "Path Found":
                    self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> RC >> Calculating signal's parameteres" + Environment.NewLine));
                    FindSubcarriers(input, handler);
                    break;
                case "Link Delete":
                    LinkDelete(input, handler);
                    break;
            }
        }

        private void LinkDelete(CommandLibrary.Command command, Socket handler)
        {
          
                links.Remove(new CommandLibrary.Link() { linkID = command.deletedLinkId });
        }

        private void FindSubcarriers(CommandLibrary.Command command, Socket handler)
        {
            int pathLength = 0;
            IEnumerable<int> distinctSlots;
            List<int> temp = new List<int>();
            foreach (Link l in command.linkList)
            {
                temp.AddRange(l.usedSlots);
                pathLength += l.length;
            }
            distinctSlots = temp.Distinct();

            string modulation = string.Empty;
            int value = 0;
            if (pathLength < 125)
            {
                modulation = "64-QAM";
                value = 6;
            }
            else if (pathLength > 125 && pathLength <= 250)
            {
                modulation = "32-QAM";
                value = 5;
            }
            else if (pathLength > 250 && pathLength <= 500)
            {
                modulation = "16-QAM";
                value = 4;
            }
            else if (pathLength > 500 && pathLength <= 1000)
            {
                modulation = "8-QAM";
                value = 3;
            }
            else if (pathLength > 1000 && pathLength <= 2000)
            {
                modulation = "QPSK";
                value = 2;
            }
            else if (pathLength > 2000)
            {
                modulation = "BPSK";
                value = 1;
            }
            double bauds = command.bandwidth / value;
            double band = 2 * bauds;
            int numberOfSubcarriers = (int)(Math.Ceiling(band / 6.25));
            //numberOfSubcarriers = numberOfSubcarriers == 0 ? 10 : numberOfSubcarriers;// ZMIENIC
            List<int> list = new List<int>();
            int min = 0;
            if (distinctSlots.Count() != 0)
                min = distinctSlots.Max();

            //foreach (Link l in command.linkList)
            //{
            //    for (int i = min + 2; i < min + 2 + numberOfSubcarriers; i++)
            //        if (!l.usedSlots.Contains(i))
            //            l.usedSlots.Add(i);
            //}
            //jak przyznać szczeliny???
            CommandLibrary.Command selectedSubcarriers = new CommandLibrary.Command("Selected Subcarriers");
            selectedSubcarriers.sourceId = rcId;
            selectedSubcarriers.destinationId = command.sourceId;
            selectedSubcarriers.bandwidth = command.bandwidth;
            selectedSubcarriers.linkList = command.linkList;
            selectedSubcarriers.domainOfRequestedClient = command.domainOfRequestedClient;
            selectedSubcarriers.domainOfRequestingClient = command.domainOfRequestingClient;
            selectedSubcarriers.record = new PortMatch(0, 0, min, min + numberOfSubcarriers);
            selectedSubcarriers.record.modulation = modulation;
            selectedSubcarriers.startCrack = min + 2;
            selectedSubcarriers.endCrack = selectedSubcarriers.startCrack + numberOfSubcarriers -1;
            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, selectedSubcarriers);
                stream.Flush();
                stream.Close();
            }
        }

        private void SetPath(Command command, Socket handler)
        {
            if (String.IsNullOrEmpty(command.startOfPath))
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> RC >> Trying to solve route element ({command.startOfPathPort}, {command.endOfPathPort})" + Environment.NewLine));
            else
                self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> RC >> Trying to solve route element ({command.startOfPath}, {command.endOfPath})" + Environment.NewLine));

            if (String.IsNullOrEmpty(command.startOfPath) || String.IsNullOrEmpty(command.endOfPath))
            {
                List<NetworkNode> checkList = self.ReturnNodes();
                bool flag = false;
                string thatNode = string.Empty;
                foreach (NetworkNode n in checkList)
                {
                    List<int> tempInt = n.ReturnPorts();
                    if (tempInt.Contains(command.startOfPathPort) && tempInt.Contains(command.endOfPathPort))
                    {
                        //oba porty należą do jednego OXC! nie szukamy ścieżki
                        flag = true;
                        thatNode = n.ReturnName();
                    }
                    else
                    {
                        //porty z różnych oxc, więc szukamy ścieżki w podsieci
                    }
                }
                if (!flag)
                {
                    Command response = new Command("Path Ready");
                    List<string> temp = new List<string>();
                    for (int i = 0; i < links.Count; i++)
                    {
                        if (links[i].portObject1 == command.startOfPathPort || links[i].portObject1 == command.endOfPathPort)
                        {
                            temp.Add(links[i].Object1);
                        }
                        if (links[i].portObject2 == command.endOfPathPort || links[i].portObject2 == command.startOfPathPort)
                        {
                            temp.Add(links[i].Object2);
                        }
                    }
                    if (temp.Count == 1)
                    {
                        command.startOfPath = temp[0];
                        response.startOfPath = temp[0];
                        command.endOfPath = temp[0];
                        response.endOfPath = temp[0];
                    }
                    else
                    {
                        command.startOfPath = temp[0];
                        response.startOfPath = temp[0];
                        command.endOfPath = temp[1];
                        response.endOfPath = temp[1];
                    }
                    response.startOfPathPort = command.startOfPathPort;
                    response.endOfPathPort = command.endOfPathPort;
                    response.linkList = getShortestPath(command.startOfPath, command.endOfPath);
                    response.sourceId = rcId;
                    response.destinationId = command.sourceId;
                    response.startOfPath = command.startOfPath;
                    response.endOfPath = command.endOfPath;
                    response.domainOfRequestedClient = command.domainOfRequestedClient;
                    response.domainOfRequestingClient = command.domainOfRequestingClient;
                    response.bandwidth = command.bandwidth;
                    response.startCrack = command.startCrack;
                    response.endCrack = command.endCrack;
                    using (var stream = new NetworkStream(handler))
                    {
                        BinaryFormatter bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, response);
                        stream.Flush();
                        stream.Close();
                    }
                }
                else
                {
                    Command response = new Command("Just Confirm");
                    response.linkList = new List<Link>();
                    foreach (Link l in command.linkList)
                    {
                        if (l.Object1 == self.ReturnName())
                            l.Object1 = thatNode;
                        if (l.Object2 == self.ReturnName())
                            l.Object2 = thatNode;
                    }
                    response.linkList.AddRange(command.linkList);
                    response.sourceId = rcId;
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
                }
            }
            else
            {
                Command response = new Command("Path Ready");
                response.startOfPathPort = command.startOfPathPort;
                response.endOfPathPort = command.endOfPathPort;
                response.linkList = getShortestPath(command.startOfPath, command.endOfPath);
                response.sourceId = rcId;
                response.destinationId = command.sourceId;
                response.startOfPath = command.startOfPath;
                response.endOfPath = command.endOfPath;
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
            }
        }

        public List<Link> getShortestPath(String from, String where)
        {
            nodes = new List<Vertex>();
            edges = new List<Edge>();

            foreach (Link link in links)
            {
                Vertex location = new Vertex(link.Object1);
                if (!nodes.Contains(location))
                {
                    nodes.Add(location);
                }
                Vertex location2 = new Vertex(link.Object2);
                if (!nodes.Contains(location2))
                {
                    nodes.Add(location2);
                }
                addLane(link.Object1, link.Object2, link.length);
            }
            Graph graph = new Graph(nodes, edges);
            DijkstraAlgorithm dijkstra = new DijkstraAlgorithm(graph);
            String id = from;
            Vertex temp = new Vertex(id);
            id = where;
            Vertex temp2 = new Vertex(id);
            dijkstra.execute(nodes[nodes.IndexOf(temp)]);
            List<Vertex> path = dijkstra.getPath(nodes[nodes.IndexOf(temp2)]);

            List<Link> result = new List<Link>();
            self.mainWindow.Invoke(new Action(() => self.Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> RC >> Route found: " + Environment.NewLine));
            for (int i = 1; i < path.Count; i++)
                foreach (Link link in links)
                {
                    if (link.Object1.Equals(path[i].id) && link.Object2.Equals(path[i - 1].id) && !result.Contains(link))
                    {
                        if (result.Count != 0)
                        {
                                List<Link> tempList = new List<Link>();
                                tempList.AddRange(result);
                                foreach (Link r in result)
                                {
                                    if (r.linkID != link.linkID)
                                    {
                                        if ((r.Object1 == link.Object1 && r.Object2 == link.Object2) || (r.Object1 == link.Object2 && r.Object2 == link.Object1))
                                        {
                                            if (r.length > link.length)
                                            {
                                                tempList.Remove(r);
                                                tempList.Add(link);
                                               
                                            }
                                        }
                                        else
                                        {
                                            tempList.Add(link);
                                           
                                        }
                                    }
                                }
                                result.Clear();
                                result.AddRange(tempList);
                                continue;
                        }
                        else
                        {
                            result.Add(link);
                            
                            continue;
                        }
                    }
                    if (link.Object1.Equals(path[i - 1].id) && link.Object2.Equals(path[i].id) && !result.Contains(link))
                    {
                        if (result.Count != 0)
                        {
                            
                                List<Link> tempList = new List<Link>();
                                tempList.AddRange(result);
                                foreach (Link r in result)
                                {
                                    if (r.linkID != link.linkID)
                                    {
                                        if ((r.Object1 == link.Object1 && r.Object2 == link.Object2) || (r.Object1 == link.Object2 && r.Object2 == link.Object1))
                                        {
                                            if (r.length > link.length)
                                            {
                                                tempList.Remove(r);
                                                tempList.Add(link);
                                                
                                            }
                                        }
                                        else
                                        {
                                            tempList.Add(link);
                                            
                                        }
                                    }
                                }
                                result.Clear();
                                result.AddRange(tempList);
                                continue;
                            
                        }
                        else
                        {
                            result.Add(link);
                            
                            continue;
                        }
                    }
                }
            List<Link> list = new List<Link>();
            foreach (Link l in result)
            {
                if (!list.Contains(new Link() { linkID = l.linkID }))
                {
                    list.Add(l);
                    self.mainWindow.Invoke(new Action(() => self.Logs += "            Link " + l.linkID + ": (" + l.Object1 + " - " + l.Object2 + ")" + Environment.NewLine));
                }
            }
            result.Clear();
            result.AddRange(list);
            return result;
        }
        private void addLane(string sourceLocNo, string destLocNo, int weight)
        {
            Vertex start = null;
            Vertex end = null;

            foreach (Vertex v in nodes)
            {

                if (v.id.Equals(sourceLocNo.ToString()))
                {
                    System.Console.WriteLine(start);
                    start = v;
                }
                if (v.id.Equals(destLocNo.ToString()))
                    end = v;
            }

            Edge lane = new Edge(nodes[nodes.IndexOf(new Vertex(sourceLocNo))], nodes[nodes.IndexOf(new Vertex(destLocNo))], weight);
            if (!edges.Contains(lane))
                edges.Add(lane);
        }
    }

    class Vertex
    {
        public String id;

        public Vertex(String id)
        {
            this.id = id;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null)
                return false;

            Vertex other = (Vertex)obj;

            if (id == null)
            {
                if (other.id != null)
                    return false;
            }
            else if (!id.Equals(other.id))
                return false;
            System.Console.WriteLine("true");
            return true;
        }

        public override int GetHashCode()
        {
            int result = 31;

            result = result * 23 + id.GetHashCode();

            return result;
        }

        public override string ToString()
        {
            return id;
        }
    }


    class Graph
    {
        public List<Vertex> vertexes;
        public List<Edge> edges;

        public Graph(List<Vertex> vertexes, List<Edge> edges)
        {
            this.vertexes = vertexes;
            this.edges = edges;
        }
    }

    class Edge
    {

        public Vertex source;
        public Vertex destination;
        public int weight;

        public Edge(Vertex source, Vertex destination, int weight)
        {

            this.source = source;
            this.destination = destination;
            this.weight = weight;
        }
    }


    class DijkstraAlgorithm
    {
        private List<Vertex> nodes;
        private List<Edge> edges;
        private HashSet<Vertex> settledNodes;
        private HashSet<Vertex> unSettledNodes;
        private Dictionary<Vertex, Vertex> predecessors;
        private Dictionary<Vertex, int> distance;

        public DijkstraAlgorithm(Graph graph)
        {

            this.nodes = new List<Vertex>(graph.vertexes);
            this.edges = new List<Edge>(graph.edges);
        }

        public void execute(Vertex source)
        {
            settledNodes = new HashSet<Vertex>();
            unSettledNodes = new HashSet<Vertex>();
            distance = new Dictionary<Vertex, int>();
            predecessors = new Dictionary<Vertex, Vertex>();
            distance[source] = 0;
            unSettledNodes.Add(source);
            while (unSettledNodes.Count() > 0)
            {
                Vertex node = getMinimum(unSettledNodes);
                settledNodes.Add(node);
                unSettledNodes.Remove(node);
                findMinimalDistances(node);

            }
        }

        private void findMinimalDistances(Vertex node)
        {
            List<Vertex> adjacentNodes = getNeighbors(node);
            foreach (Vertex target in adjacentNodes)
            {

                if (getShortestDistance(target) > getShortestDistance(node) + getDistance(node, target))
                {

                    distance[target] = getShortestDistance(node) + getDistance(node, target);
                    predecessors[target] = node;


                    unSettledNodes.Add(target);
                }
            }

        }

        private int getDistance(Vertex node, Vertex target)
        {
            foreach (Edge edge in edges)
            {
                if (edge.source.Equals(node) && edge.destination.Equals(target))
                {
                    return edge.weight;
                }
                if (edge.destination.Equals(node) && edge.source.Equals(target))
                {
                    return edge.weight;
                }
            }
            throw new Exception("Should not happen");
        }

        private List<Vertex> getNeighbors(Vertex node)
        {
            List<Vertex> neighbors = new List<Vertex>();
            foreach (Edge edge in edges)
            {
                if (edge.source.Equals(node) && !isSettled(edge.destination))
                {
                    neighbors.Add(edge.destination);
                    continue;
                }
                if (edge.destination.Equals(node) && !isSettled(edge.source))
                {
                    neighbors.Add(edge.source);
                }
            }
            return neighbors;
        }

        private Vertex getMinimum(HashSet<Vertex> vertexes)
        {
            Vertex minimum = null;
            foreach (Vertex vertex in vertexes)
            {
                if (minimum == null)
                {
                    minimum = vertex;
                }
                else
                {
                    if (getShortestDistance(vertex) < getShortestDistance(minimum))
                    {
                        minimum = vertex;
                    }
                }
            }
            return minimum;
        }

        private bool isSettled(Vertex vertex)
        {
            return settledNodes.Contains(vertex);
        }

        private int getShortestDistance(Vertex destination)
        {

            if (distance.ContainsKey(destination))
            {

                return distance[destination];
            }
            else
            {

                return int.MaxValue;
            }
        }


        public List<Vertex> getPath(Vertex target)
        {
            List<Vertex> path = new List<Vertex>();
            Vertex step = target;
            // czy sciezka istnieje
            if (predecessors[step] == null)
            {
                return null;
            }
            path.Add(step);
            while (predecessors.ContainsKey(step))
            {
                step = predecessors[step];
                path.Add(step);
            }

            path.Reverse();

            return path;
        }
    }
}
