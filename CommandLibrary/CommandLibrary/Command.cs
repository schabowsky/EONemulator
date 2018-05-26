using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommandLibrary
{
    [Serializable()]
    public class Command : ISerializable
    {
        public string commandType;//nazwa, np. "CallIndication"
        public string sourceId;//kto wysłał
        public string destinationId;//do kogo
        public string callIndicationClientId;//kto chce połączenie (Call Indication)
        public string callConfirmedClientId;//kto pozwala na połączenie (Call confirmed)
        public string startClientId;//początek łącza (Call Request)
        public string endClientId;//koniec łącza (Call Request)
        public int bandwidth;//żądana przepustowość (Call Request)
        public PortMatch record;
        public List<PortMatch> commutationTable;
        public List<Link> linkList;
        public string domainOfRequestedClient;//zwracane przez PC
        public bool isClientAccessible;//zwracane przez PC
        public bool isClientAccepting;//zwracane po Call Indication przez klienta
        public string startOfPath;//początek szukanej przez RC ścieżki
        public string endOfPath;//koniec szukanej przez RC ścieżki
        public int startOfPathPort;
        public int endOfPathPort;
        public int startCrack;
        public int endCrack;
        public string domainOfRequestingClient;
        public int connectionId;
        public int deletedLinkId;

        public Command(string type, PortMatch record)
        {
            this.commandType = type;
            this.record = record;
        }

        public Command(string type)
        {
            this.commandType = type;
        }

        public Command(string type, List<PortMatch> table)
        {
            this.commandType = type;
            this.commutationTable = table;
        }

        public Command(SerializationInfo info, StreamingContext ctxt)
        {
            commandType = (string)info.GetValue("commandType", typeof(string));
            sourceId = (string)info.GetValue("sourceId", typeof(string));
            destinationId = (string)info.GetValue("destinationId", typeof(string));
            callIndicationClientId = (string)info.GetValue("callIndicationClientId", typeof(string));
            callConfirmedClientId = (string)info.GetValue("callConfirmedClientId", typeof(string));
            startClientId = (string)info.GetValue("startClientId", typeof(string));
            endClientId = (string)info.GetValue("endClientId", typeof(string));
            bandwidth = (int)info.GetValue("bandwidth", typeof(int));
            record = (PortMatch)info.GetValue("record", typeof(PortMatch));
            commutationTable = (List<PortMatch>)info.GetValue("commutationTable", typeof(List<PortMatch>));
            linkList = (List<Link>)info.GetValue("linkList", typeof(List<Link>));
            domainOfRequestedClient = (string)info.GetValue("domainOfRequestedClient", typeof(string));
            isClientAccessible = (bool)info.GetValue("isClientAccessible", typeof(bool));
            isClientAccepting = (bool)info.GetValue("isClientAccepting", typeof(bool));
            startOfPath = (string)info.GetValue("startOfPath", typeof(string));
            endOfPath = (string)info.GetValue("endOfPath", typeof(string));
            startOfPathPort = (int)info.GetValue("startOfPathPort", typeof(int));
            endOfPathPort = (int)info.GetValue("endOfPathPort", typeof(int));
            startCrack = (int)info.GetValue("startCrack", typeof(int));
            endCrack = (int)info.GetValue("endCrack", typeof(int));
            domainOfRequestingClient = (string)info.GetValue("domainOfRequestingClient", typeof(string));
            connectionId = (int)info.GetValue("connectionId", typeof(int));
            deletedLinkId = (int)info.GetValue("deletedLinkId", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("commandType", commandType);
            info.AddValue("sourceId", sourceId);
            info.AddValue("destinationId", destinationId);
            info.AddValue("callIndicationClientId", callIndicationClientId);
            info.AddValue("callConfirmedClientId", callConfirmedClientId);
            info.AddValue("startClientId", startClientId);
            info.AddValue("endClientId", endClientId);
            info.AddValue("bandwidth", bandwidth);
            info.AddValue("record", record);
            info.AddValue("commutationTable", commutationTable);
            info.AddValue("linkList", linkList);
            info.AddValue("domainOfRequestedClient", domainOfRequestedClient);
            info.AddValue("isClientAccessible", isClientAccessible);
            info.AddValue("isClientAccepting", isClientAccepting);
            info.AddValue("startOfPath", startOfPath);
            info.AddValue("endOfPath", endOfPath);
            info.AddValue("startOfPathPort", startOfPathPort);
            info.AddValue("endOfPathPort", endOfPathPort);
            info.AddValue("startCrack", startCrack);
            info.AddValue("endCrack", endCrack);
            info.AddValue("domainOfRequestingClient", domainOfRequestingClient);
            info.AddValue("connectionId", connectionId);
            info.AddValue("deletedLinkId", deletedLinkId);
        }
    }
}
