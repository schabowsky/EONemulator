using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace NetworkCloud
{

    public partial class Form1 : Form
    {
        private static List<SocketMatch> socketMatchList;//lista pokazujaca, który element sieci odpowiada danemu socketowi w chmurze
        private static List<PortMatch> portMatchList;
        private static List<Link> linkList;//połączenia odczytane z pliku
        //kilka portów, żeby przyspieszyć generowanie socketów
        private static int[] cloudPorts;
        private static int[] routerPorts;
        private static int[] clientPorts;
        private static String[] cIds;//identyfikatory elementów sieci
        private static String[] rIds;//identyfikatory elementów sieci
        private static List<Socket> cloudSockets;
        private static List<Socket> workingCloudSockets;//gdy element sieci podłączy się do odpowiadającego mu socketa chmury to powstanie nowy socket, tutaj je
        private static int numberOfElements;

        public class SocketMatch
        {
            //przyporządkowuje symbol elementu sieciowego do socketa chmury
            public Socket s;
            public String elementId;

            public SocketMatch(String i, Socket sckt)
            {
                this.s = sckt;
                this.elementId = i;
            }
        }

        public class PortMatch
        {
            //przyporządkowuje symbol elementu sieciowego do portu jego socketa
            public int port;
            public String elementId;

            public PortMatch(String i, int p)
            {
                this.port = p;
                this.elementId = i;
            }
        }
        public class Link
        {
            //reprezentuje połączenie, np. port A elementu WS1 do portu C K3
            public String sourceId;
            public int sourcePort;
            public String receiverId;
            public int receiverPort;
        }
        public Form1()
        {
            InitializeComponent();
            linkList = new List<Link>();//inicjacja
            portMatchList = new List<PortMatch>();//inicjacja
            socketMatchList = new List<SocketMatch>();//inicjacja
            workingCloudSockets = new List<Socket>();//inicjacja
            cloudSockets = new List<Socket>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            readPorts();
            readLinks();//wpisujemy połączenia z pliku do listy
            generateSockets();//tworzymy wszystkie potrzebne sockety
            makeConnection(this);
        }

        private void generateSockets()
        {
            for (int i = 0; i < numberOfElements; i++)
            {
                Socket tempCloudSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                tempCloudSocket.Bind(new IPEndPoint(IPAddress.Parse(System.Configuration.ConfigurationManager.AppSettings["host"]), cloudPorts[i]));
                tempCloudSocket.Listen(5);//od razu nasłuchuję
                cloudSockets.Add(tempCloudSocket);
            }
        }

        private void makeConnection(Form1 f)
        {
            //tutaj rozpoczynamy próbę akceptacji od naszych elementów sieci
            AsyncCallback aCallback = new AsyncCallback(AcceptCallback);// delegat (patrz AcceptCallback)
            for (int i = 0; i < numberOfElements; i++)
            {
                Object[] o = new object[2];
                o[0] = cloudSockets[i];
                o[1] = f;
                cloudSockets[i].BeginAccept(aCallback, o);//zaczynamy akceptację  
            }       
        }

        private void AcceptCallback(IAsyncResult result)
        {
            Object[] ob = (Object[])result.AsyncState;
            Socket listener = (Socket)ob[0];
            Form1 fo = (Form1)ob[1];
            Socket handler = listener.EndAccept(result);
            workingCloudSockets.Add(handler);

            foreach (PortMatch pm in portMatchList)
            {
                if (pm.port.ToString() == ((IPEndPoint)handler.RemoteEndPoint).Port.ToString())
                {
                    int index = workingCloudSockets.FindIndex(x => ((IPEndPoint)x.LocalEndPoint).Port.ToString() == ((IPEndPoint)handler.LocalEndPoint).Port.ToString());
                    socketMatchList.Add(new SocketMatch(pm.elementId, workingCloudSockets[index]));

                    fo.textBox1.Invoke(new Action(() =>
                    {
                        fo.textBox1.AppendText(DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> New device connected to the cloud! Id: " + pm.elementId + Environment.NewLine);
                    }));
                }
            }

            receive(handler, fo).Wait();
        }

        private static Task receive(Socket s, Form1 f)
        {
            Socket handler = s;
            NetworkStream stream = new NetworkStream(handler);
            BinaryFormatter bformatter = new BinaryFormatter();
            SignalLibrary.Signal sg = null;

            while (true)
            {
                string timestamp = DateTime.UtcNow.ToString("HH:mm:ss.fff");
                int port = 0;
                string id = null;
                int nextPort = 0;
                string nextId = null;

                if (stream.DataAvailable == true)
                {
                    try
                    {
                        sg = (SignalLibrary.Signal)bformatter.Deserialize(stream);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }

                    port = sg.lastPort;

                    foreach (SocketMatch sm in socketMatchList)
                    {
                        if (((IPEndPoint)sm.s.LocalEndPoint).Port.ToString() == ((IPEndPoint)handler.LocalEndPoint).Port.ToString())
                        {
                            id = sm.elementId;
                        }
                    }

                    //sprawdza ID elementu oraz numer portu, na ktory nalezy przekierowac wiadomosc
                    foreach (Link l in linkList)
                    {
                        if (l.sourceId == id && l.sourcePort == port)
                        {
                            nextPort = l.receiverPort;
                            nextId = l.receiverId;
                        }
                    }

                    f.textBox1.Invoke(new Action(() =>
                    {
                        f.textBox1.AppendText(DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> Signal received from: " + id + " Port: " + sg.lastPort + Environment.NewLine);
                    }));

                    int passer = socketMatchList.FindIndex(x => x.elementId == nextId);
                    Socket tempSocket = socketMatchList[passer].s;

                    if (passer >= 0)
                    {
                        f.textBox1.Invoke(new Action(() =>
                        {
                            f.textBox1.AppendText(DateTime.UtcNow.ToString("HH: mm:ss.fff") + " >> Passing signal to: " + nextId + Environment.NewLine);
                        }));


                        NetworkStream senderStream = new NetworkStream(workingCloudSockets.Find(x => x.RemoteEndPoint == tempSocket.RemoteEndPoint));
                        sg.lastPort = nextPort;
                        bformatter.Serialize(senderStream, sg);
                        senderStream.Flush();
                        senderStream.Close();
                    }
                    else
                    {
                        f.textBox1.Invoke(new Action(() =>
                        {
                            f.textBox1.AppendText(DateTime.UtcNow.ToString("HH:mm:ss.fff") + " >> This port is not connected! End of channel." + Environment.NewLine);
                        }));
                    }
                }
                Thread.Sleep(20);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (cloudSockets.Count != 0)
                {
                    foreach (Socket cs in cloudSockets)
                        cs.Shutdown(SocketShutdown.Both);
                }

                if (workingCloudSockets.Count != 0)
                {
                    foreach (Socket wcs in workingCloudSockets)
                        wcs.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }

            Application.Exit();
        }

        private void readLinks()
        {
            //odczyt z XML
            XmlDocument XmlDoc = new XmlDocument();
            try
            {
                XmlDoc.Load(System.Configuration.ConfigurationManager.AppSettings["connections"]);
                Console.WriteLine("Dokument załadowany!");
                int count = XmlDoc.GetElementsByTagName("Link").Count;
                for (int i = 0; i < count; i++)
                {
                    Link tempLink1 = new Link();
                    XmlAttributeCollection coll = XmlDoc.GetElementsByTagName("Link").Item(i).Attributes;
                    tempLink1.sourceId = coll.Item(0).InnerText;
                    tempLink1.sourcePort = Int32.Parse(coll.Item(1).InnerText);
                    tempLink1.receiverId = coll.Item(2).InnerText;
                    tempLink1.receiverPort = Int32.Parse(coll.Item(3).InnerText);
                    linkList.Add(tempLink1);
                }
            }
            catch (XmlException exc)
            {
                Console.WriteLine(exc.Message);
            }

            ListViewItem[] listViewList = new ListViewItem[linkList.Count];
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.Columns.Add("Link ID");
            listView1.Columns.Add("Source ID");
            listView1.Columns.Add("Source Port");
            listView1.Columns.Add("Receiver ID");
            listView1.Columns.Add("Receiver Port");

            for (int i = 0; i < linkList.Count; i++)
            {
                Console.WriteLine(linkList[i].sourceId + " " + linkList[i].sourcePort
                                    + " " + linkList[i].receiverId + " " + linkList[i].receiverPort);

                ListViewItem tempItem = new ListViewItem((i + 1).ToString());
                tempItem.SubItems.Add(linkList[i].sourceId);
                tempItem.SubItems.Add(linkList[i].sourcePort.ToString());
                tempItem.SubItems.Add(linkList[i].receiverId);
                tempItem.SubItems.Add(linkList[i].receiverPort.ToString());
                listViewList[i] = tempItem;
            }
            listView1.Items.AddRange(listViewList);
        }

        private void readPorts()
        {
            XmlDocument XmlDoc = new XmlDocument();
            try
            {
                XmlDoc.Load(System.Configuration.ConfigurationManager.AppSettings["ports"]);
                Console.WriteLine("Dokument załadowany!");

                int count = XmlDoc.GetElementsByTagName("Router").Count;
                routerPorts = new int[count];
                rIds = new String[count];

                for (int i = 0; i < count; i++)
                {
                    int port = int.Parse(XmlDoc.GetElementsByTagName("Router").Item(i).Attributes.Item(0).InnerText);
                    String id = XmlDoc.GetElementsByTagName("Router").Item(i).InnerText;
                    routerPorts[i] = port;
                    rIds[i] = id;
                    PortMatch pm = new PortMatch(id, port);
                    portMatchList.Add(pm);
                }

                count = XmlDoc.GetElementsByTagName("Cloud").Count;
                cloudPorts = new int[count];

                for (int i = 0; i < count; i++)
                {
                    int port = int.Parse(XmlDoc.GetElementsByTagName("Cloud").Item(i).Attributes.Item(0).InnerText);
                    cloudPorts[i] = port;
                }

                count = XmlDoc.GetElementsByTagName("Client").Count;
                clientPorts = new int[count];
                cIds = new String[count];

                for (int i = 0; i < count; i++)
                {
                    int port = int.Parse(XmlDoc.GetElementsByTagName("Client").Item(i).Attributes.Item(0).InnerText);
                    String id = XmlDoc.GetElementsByTagName("Client").Item(i).InnerText;
                    clientPorts[i] = port;
                    cIds[i] = id;
                    PortMatch pm = new PortMatch(id, port);
                    portMatchList.Add(pm);
                }

                numberOfElements = cIds.Length + rIds.Length;
            }
            catch (XmlException exc)
            {
                Console.WriteLine(exc.Message);
            }
        }
    }
}
