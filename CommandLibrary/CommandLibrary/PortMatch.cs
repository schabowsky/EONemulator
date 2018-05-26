using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommandLibrary
{
    [Serializable()]
    public class PortMatch : ISerializable
    {
        public int receivedPort;
        public int destinationPort;
        public double startFreq;
        public double endFreq;
        public string modulation;
        public string routerName;
        public double connectionFreq;

        public PortMatch(int received, int destination, int start, int end)
        {
            this.receivedPort = received;
            this.destinationPort = destination;
            this.startFreq = (193.1 + (double)start * 0.00625);
            this.endFreq = (193.1 + (double)end * 0.00625);
            this.modulation = null;
            this.routerName = null;
        }

        public PortMatch(int received, int destination, double start, double end)
        {
            this.receivedPort = received;
            this.destinationPort = destination;
            this.startFreq = start;
            this.endFreq = end;
            this.modulation = null;
            this.routerName = null;
        }

        public PortMatch(SerializationInfo info, StreamingContext ctxt)
        {
            receivedPort = (int)info.GetValue("receivedPort", typeof(int));
            destinationPort = (int)info.GetValue("destinationPort", typeof(int));
            modulation = (string)info.GetValue("modulation", typeof(string));
            routerName = (string)info.GetValue("routerName", typeof(string));
            startFreq = (double)info.GetValue("startFreq", typeof(double));
            endFreq = (double)info.GetValue("endFreq", typeof(double));
            connectionFreq = (double)info.GetValue("connectionFreq", typeof(double));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("receivedPort", receivedPort);
            info.AddValue("destinationPort", destinationPort);
            info.AddValue("modulation", modulation);
            info.AddValue("routerName", routerName);
            info.AddValue("startFreq", startFreq);
            info.AddValue("endFreq", endFreq);
            info.AddValue("connectionFreq", connectionFreq);
        }
    }
}
