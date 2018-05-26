using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CommandLibrary
{
    [Serializable()]
    public class Link : ISerializable
    {
        public int linkID;
        public string Object1;
        public string Object2;
        public int portObject1;
        public int portObject2;
        public int length;
        public List<int> usedSlots;

        public Link(SerializationInfo info, StreamingContext ctxt)
        {
            linkID = (int)info.GetValue("linkID", typeof(int));
            Object1 = (string)info.GetValue("Object1", typeof(string));
            Object2 = (string)info.GetValue("Object2", typeof(string));
            portObject1 = (int)info.GetValue("portObject1", typeof(int));
            portObject2 = (int)info.GetValue("portObject2", typeof(int));
            length = (int)info.GetValue("length", typeof(int));
            usedSlots = (List<int>)info.GetValue("usedSlots", typeof(List<int>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("linkID", linkID);
            info.AddValue("Object1", Object1);
            info.AddValue("Object2", Object2);
            info.AddValue("portObject1", portObject1);
            info.AddValue("portObject2", portObject2);
            info.AddValue("length", length);
            info.AddValue("usedSlots", usedSlots);
        }

        public Link()
        {
            linkID = 0;
            Object1 = null;
            Object2 = null;
            portObject1 = 0;
            portObject2 = 0;
            length = 0;
            usedSlots = new List<int>();
        }

        public Link(int id, string O1, string O2, int pO1, int pO2, int len)
        {
            linkID = id;
            Object1 = O1;
            Object2 = O2;
            portObject1 = pO1;
            portObject2 = pO2;
            length = len;
            usedSlots = new List<int>();
        }

        public Link(Link l)
        {
            linkID = l.linkID;
            Object1 = l.Object1;
            Object2 = l.Object2;
            portObject1 = l.portObject1;
            portObject2 = l.portObject2;
            length = l.length;
            usedSlots = new List<int>();
        }

        public void AddUsedSlots(int begin, int end)
        {
            for (int i=begin; i<=end; i++)
            {
                usedSlots.Add(i);
            }
        }

        public void DeleteUsedSlots(int begin, int end)
        {
            for (int i = begin; i <= end; i++)
            {
                usedSlots.Remove(i);
            }
        }

        public override int GetHashCode()
        {
            return 177814412 + linkID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Link other = (Link)obj;
            if (obj == null) return false;
            return (this.linkID.Equals(other.linkID));
        }

        public static bool operator ==(Link left, Link right)
        {
            if (object.ReferenceEquals(left.linkID, null))
            {
                return object.ReferenceEquals(right.linkID, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(Link left, Link right)
        {
            return !(left.linkID == right.linkID);
        }

        // modyfikacja polaczenia
    }
}
