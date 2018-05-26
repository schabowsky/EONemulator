using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SignalLibrary
{
    [Serializable()]
    public class Signal : ISerializable //derive your class from ISerializable
    {
        public string sender;
        public int lastPort;
        public string destination;
        public string modulation;
        public double frequency;
        public int? subcarriers;
        public string data;

        public Signal(string sender, int latestPort, string destination, double freq, string data)
        {
            this.sender = sender;
            lastPort = latestPort;
            this.destination = destination;
            modulation = null;
            frequency = freq;
            subcarriers = null;
            this.data = data;
        }

        //Deserialization constructor.
        public Signal(SerializationInfo info, StreamingContext ctxt)
        {
            sender = (string)info.GetValue("sender", typeof(string));
            lastPort = (int)info.GetValue("lastPort", typeof(int));
            destination = (string)info.GetValue("destination", typeof(string));
            modulation = (string)info.GetValue("modulation", typeof(string));
            frequency = (double)info.GetValue("frequency", typeof(double));
            subcarriers = (int?)info.GetValue("subcarriers", typeof(int?));
            data = (string)info.GetValue("data", typeof(string));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sender", sender);
            info.AddValue("lastPort", lastPort);
            info.AddValue("destination", destination);
            info.AddValue("modulation", modulation);
            info.AddValue("frequency", frequency);
            info.AddValue("subcarriers", subcarriers);
            info.AddValue("data", data);
        }
    }
}
