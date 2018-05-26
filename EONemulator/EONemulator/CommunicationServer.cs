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
    class CommunicationServer
    {
        private IPEndPoint ipep;
        private Socket socket;
        private List<Socket> handlers;
        private Dictionary<string, int> componentPorts;

        public CommunicationServer(Dictionary<string, int> dictionary)
        {
            handlers = new List<Socket>();
            componentPorts = dictionary;
            ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50000);
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipep);
            socket.Listen(30);
            socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            Socket listener = (Socket)result.AsyncState;
            Socket handler = listener.EndAccept(result);
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            handlers.Add(handler);
            Task.Run(() => { Receive(handler); });
        }

        private void Receive(Socket handler)
        {
            NetworkStream stream = new NetworkStream(handler);
            BinaryFormatter bformatter = new BinaryFormatter();
            CommandLibrary.Command message = null;
            while(true)
            {
                if (stream.DataAvailable == true)
                {
                    message = (CommandLibrary.Command)bformatter.Deserialize(stream);
                    int port = 0; 
                    bool isExisting = componentPorts.TryGetValue(message.destinationId, out port);
                    if (isExisting)
                    {
                        int index = handlers.FindIndex(x => ((IPEndPoint)x.RemoteEndPoint).Port == port);
                        if (index != -1)
                        {
                            NetworkStream senderStream = new NetworkStream(handlers[index]);
                            bformatter.Serialize(senderStream, message);
                            senderStream.Flush();
                            senderStream.Close();
                        }
                    }
                }
                Thread.Sleep(10)
;           }
        }
    }
}
