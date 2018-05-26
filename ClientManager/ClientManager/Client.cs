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
using System.Windows.Forms;

namespace ClientManager
{
    class Client : INotifyPropertyChanged
    {
        Socket clientSocket;
        private string host = System.Configuration.ConfigurationManager.AppSettings["host"];
        private int port;
        private string cloudHost = System.Configuration.ConfigurationManager.AppSettings["host"];
        private int cloudPort;
        private string clientName;
        private string domainName;
        private int portName;
        private string logs;
        private CPCC cpcc;
        public MainWindow mainWindow;
        private bool flag;

        public Client(string name, string domainId, int port, int cloudPort, int portName, int cpccPort, MainWindow window)
        {
            flag = true;
            clientName = name;
            mainWindow = window;
            cpcc = new CPCC(cpccPort, this);
            logs = DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {clientName} >> " + "CLIENT CONNECTED" + Environment.NewLine;
            domainName = domainId;
            this.port = port;
            this.portName = portName;
            this.cloudPort = cloudPort;
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

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

        public string ReturnName()
        {
            return clientName;
        }

        public string ReturnDomainName()
        {
            return domainName;
        }

        public void SendRequest(string destination, int bandwidth)
        {
            if (!cpcc.IsClientConnected(destination))
                cpcc.CallRequest(destination, bandwidth);
            else
                Task.Run(() => { MessageBox.Show("Connection already established."); });
        }

        public void MakeConnection()
        {
            clientSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));
            AsyncCallback aCallback = new AsyncCallback(ConnectCallback);
            clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(cloudHost), cloudPort), aCallback, clientSocket);
        }

        public void CloseConnection()
        {
            flag = false;
            clientSocket.Close();
            cpcc.CloseConnection();
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket cloudSocket = (Socket)result.AsyncState;
                cloudSocket.EndConnect(result);
                Task.Run(() => { Receive(cloudSocket); });
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

        private void Receive(Socket s)
        {
            Socket handler = s;
            NetworkStream stream = new NetworkStream(handler);
            BinaryFormatter bformatter = new BinaryFormatter();
            SignalLibrary.Signal sg = null;

            while (flag)
            {
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

                    String message = sg.data;

                    logs += DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {clientName} >> Received from: " + sg.sender + " >> " + message + Environment.NewLine;
                }

                Thread.Sleep(10);
            }
        }

        public void Send(string message, string receiver)
        {
            if (cpcc.IsClientConnected(receiver))
            {
                Socket handler = clientSocket;
                NetworkStream stream = new NetworkStream(handler);

                double freq = 235.3;
                SignalLibrary.Signal sg = new SignalLibrary.Signal(clientName, portName, receiver, freq, message);
                if (sg.sender == "K1")
                {
                    if (sg.destination == "K2")
                        sg.frequency = 235.3;
                    else if (sg.destination == "K3")
                        sg.frequency = 236.3;
                }
                else if (sg.sender == "K2")
                {
                    if (sg.destination == "K1")
                        sg.frequency = 235.3;
                    else if (sg.destination == "K3")
                        sg.frequency = 234.3;
                }
                else if (sg.sender =="K3")
                {
                    if (sg.destination == "K1")
                        sg.frequency = 236.3;
                    else if (sg.destination == "K2")
                        sg.frequency = 234.3;
                }
                BinaryFormatter binformatter = new BinaryFormatter();
                binformatter.Serialize(stream, sg);
                stream.Flush();
                stream.Close();
            }
            else
                Task.Run(() => { MessageBox.Show("Client not available."); });
        }
    }
}
