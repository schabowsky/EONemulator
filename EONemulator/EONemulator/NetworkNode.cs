using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EONemulator
{
    class NetworkNode
    {
        private Socket nodeSocket;
        private Socket ccSocket;
        private List<CommandLibrary.PortMatch> portMatchList;
        private List<int> portList;
        private string nodeName;
        private string host = System.Configuration.ConfigurationManager.AppSettings["host"];
        private int port;
        private string cloudHost = System.Configuration.ConfigurationManager.AppSettings["host"];
        private int cloudPort;
        private int ccPort;
        private string logs;
        private MainWindow mainWindow;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public string Logs
        {
            get { return logs; }
            set
            {
                if (value != logs)
                {
                    logs = value;
                    OnPropertyChanged("Logs");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public NetworkNode(string name, int port, int cloudPort, int ccPort, MainWindow window)
        {
            nodeName = name;
            mainWindow = window;
            logs = DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {nodeName} >> Connected to network" + Environment.NewLine;
            this.ccPort = ccPort;
            this.port = port;
            this.cloudPort = cloudPort;
            portMatchList = new List<CommandLibrary.PortMatch>();
            portList = new List<int>();
            nodeSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            ccSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public NetworkNode(NetworkNode netNode, MainWindow window)
        {
            nodeName = netNode.nodeName;
            mainWindow = window;
            logs = DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {nodeName} >> Connected to network" + Environment.NewLine;
            this.port = netNode.port;
            this.cloudPort = netNode.cloudPort;
            portMatchList = new List<CommandLibrary.PortMatch>();
            portList = new List<int>();
            portList.AddRange(netNode.portList);
            nodeSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            ccSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public string ReturnName()
        {
            return nodeName;
        }

        public void AddPort(int port)
        {
            portList.Add(port);
        }

        public List<int> ReturnPorts()
        {
            return portList;
        }

        public void startNode()
        {
            nodeSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            AsyncCallback aCallback = new AsyncCallback(ConnectCallback);
            nodeSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(cloudHost), cloudPort), aCallback, nodeSocket);
            ccSocket.Bind(new IPEndPoint(IPAddress.Parse(host), ccPort));
            AsyncCallback ccCallback = new AsyncCallback(CCConnectCallback);
            ccSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(cloudHost), 50000), ccCallback, ccSocket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket handler = (Socket)result.AsyncState;
                handler.EndConnect(result);
                Task.Run(() => { Receive(handler); });
            }
            catch (ObjectDisposedException e1)
            {
                Console.WriteLine(e1);
            }
            catch (SocketException e2)
            {
                Console.WriteLine(e2);
            }
        }

        private void CCConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket handler = (Socket)result.AsyncState;
                handler.EndConnect(result);
                Task.Run(() => { CCReceive(handler); });
            }
            catch (ObjectDisposedException e1)
            {
                Console.WriteLine(e1);
            }
            catch (SocketException e2)
            {
                Console.WriteLine(e2);
            }
        }

        private void CCReceive(Socket s)
        {
            NetworkStream stream = new NetworkStream(s);
            BinaryFormatter bformatter = new BinaryFormatter();
            CommandLibrary.Command command = null;
            


            while (true)
            {
                if (stream.DataAvailable == true)
                {
                    try
                    {
                        command = (CommandLibrary.Command)bformatter.Deserialize(stream);
                        
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }
                    if (command.record != null)
                    {
                        command.record.routerName = nodeName;
                        portMatchList.Add(command.record);
                        CommandLibrary.PortMatch temp = new CommandLibrary.PortMatch(command.record.destinationPort, command.record.receivedPort, 0, 0);
                        temp.modulation = command.record.modulation;
                        temp.routerName = command.record.routerName;
                        temp.startFreq = command.record.startFreq;
                        temp.endFreq = command.record.endFreq;
                        temp.connectionFreq = command.record.connectionFreq;
                        portMatchList.Add(temp);
                        mainWindow.Invoke(new Action(() => Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {nodeName} >> Table update: ports " + command.record.receivedPort + " and " + command.record.destinationPort + " matched, freqency: " + command.record.startFreq + " - " + command.record.endFreq + " THz (modulation " + command.record.modulation + ")" + Environment.NewLine));
                    }
                    else
                    {
                        if (command.commandType == "PortMatch Delete")
                        {
                            List<CommandLibrary.PortMatch> tempList = new List<CommandLibrary.PortMatch>();
                            tempList.AddRange(portMatchList);
                            foreach (CommandLibrary.PortMatch pm in portMatchList)
                            {
                                
                                if (pm.destinationPort == command.linkList[0].portObject1 || pm.destinationPort == command.linkList[0].portObject2)

                                    mainWindow.Invoke(new Action(() => Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {nodeName} >> Table update: deleted PortMatch between " + pm.receivedPort + " and " + pm.destinationPort + Environment.NewLine));
                                    tempList.Remove(pm);

                            }
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void Receive(Socket s)
        {
            NetworkStream stream = new NetworkStream(s);
            BinaryFormatter bformatter = new BinaryFormatter();
            SignalLibrary.Signal signal = null;

            while (true)
            {
                if (stream.DataAvailable == true)
                {
                    try
                    {
                        signal = (SignalLibrary.Signal)bformatter.Deserialize(stream);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc);
                    }

                    if (signal.frequency == 235.3)
                    {
                        if (signal.destination == "K1")
                            signal.destination = "A:SN1:SN4:OXC1";
                        else if (signal.destination == "K2")
                            signal.destination = "A:SN1:SN17:OXC4";

                        TranspondSignal(signal);
                        bformatter.Serialize(stream, signal);
                        stream.Flush();
                    }
                    else if (signal.frequency == 234.3)
                    {
                        if (signal.destination == "K1")
                            signal.destination = "A:SN1:SN4:OXC1";
                        else if (signal.destination == "K3")
                            signal.destination = "B:SN2:SN5:OXC6";

                        TranspondSignal(signal);
                        bformatter.Serialize(stream, signal);
                        stream.Flush();
                    }
                    else if (signal.frequency == 236.3)
                    {
                        if (signal.destination == "K3")
                            signal.destination = "B:SN2:SN5:OXC6";
                        else if (signal.destination == "K2")
                            signal.destination = "A:SN1:SN17:OXC4";

                        TranspondSignal(signal);
                        bformatter.Serialize(stream, signal);
                        stream.Flush();
                    }
                    else
                    {
                        foreach (CommandLibrary.PortMatch pm in portMatchList)
                        {
                            double cFreq = (double)((pm.endFreq + pm.startFreq) / 2);
                            if (pm.receivedPort.Equals(signal.lastPort) && pm.routerName.Equals(signal.destination) && cFreq == signal.frequency)
                            {
                                DetranspondSignal(signal);
                                bformatter.Serialize(stream, signal);
                                stream.Flush();
                                break;
                            }
                            else if (pm.receivedPort.Equals(signal.lastPort) && cFreq == signal.frequency)
                            {
                                mainWindow.Invoke(new Action(() => Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {nodeName} >> Redirecting signal from {signal.lastPort} to {pm.destinationPort}, frequency {signal.frequency} THz" + Environment.NewLine));
                                signal.lastPort = pm.destinationPort;
                                bformatter.Serialize(stream, signal);
                                stream.Flush();
                                break;
                            }
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void TranspondSignal(SignalLibrary.Signal signal)
        {
            foreach (CommandLibrary.PortMatch pm in portMatchList)
            {
                if (pm.receivedPort.Equals(signal.lastPort) && pm.connectionFreq == signal.frequency)
                {
                    double freq = signal.frequency;
                    signal.frequency = (double)((pm.endFreq + pm.startFreq) / 2);
                    mainWindow.Invoke(new Action(() => Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {nodeName} >> Redirecting signal from Client {signal.sender} (port {signal.lastPort}) to {pm.destinationPort}, frequency changed from {freq} THz to {signal.frequency} THz" + Environment.NewLine));
                    signal.modulation = pm.modulation;
                    signal.lastPort = pm.destinationPort;
                    signal.subcarriers = (Int32)((pm.endFreq - pm.startFreq) / 0.00625);
                   
                    break;
                }

            }
        }

        private void DetranspondSignal(SignalLibrary.Signal signal)
        {
            foreach (CommandLibrary.PortMatch pm in portMatchList)
            {
                if (pm.receivedPort.Equals(signal.lastPort))
                {
                    mainWindow.Invoke(new Action(() => Logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {nodeName} >> Redirecting signal from {signal.lastPort} to Client (port {pm.destinationPort}), frequency changed from {signal.frequency} THz to 235.3 THz" + Environment.NewLine));
                    signal.modulation = null;
                    signal.lastPort = pm.destinationPort;
                    signal.subcarriers = null;
                    signal.frequency = 235.3;
                    break;
                }
            }
        }
    }
}
