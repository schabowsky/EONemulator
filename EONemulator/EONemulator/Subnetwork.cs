using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EONemulator
{
    class Subnetwork : INotifyPropertyChanged
    {
        private List<NetworkNode> networkNodeList;
        private List<Subnetwork> subnetworkList;
        private string subnetworkName;
        public MainWindow mainWindow;
        private string logs;
        private LinkResourceManager lrm;
        private RC rc;
        private CC cc;

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

        public Subnetwork(string subnetworkId, string lrmId, int lrmPort, string ccId, int ccPort, string rcId, int rcPort, MainWindow window)
        {
            mainWindow = window;
            subnetworkName = subnetworkId;
            logs = DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {subnetworkName} >> " + "Subnetwork created." + Environment.NewLine;
            networkNodeList = new List<NetworkNode>();
            subnetworkList = new List<Subnetwork>();
            lrm = new LinkResourceManager(lrmId, lrmPort, this);
            cc = new CC(ccId, ccPort, this);
            rc = new RC(rcId, rcPort, this);
        }

        public Subnetwork(Subnetwork subnet)
        {
            subnetworkName = subnet.subnetworkName;
            logs = DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {subnetworkName} >> " + "Subnetwork created." + Environment.NewLine;
            networkNodeList = new List<NetworkNode>();
            networkNodeList.AddRange(subnet.networkNodeList);
        }

        public void AddNode(NetworkNode netNode)
        {
            networkNodeList.Add(netNode);
        }

        public void AddSubnetwork(Subnetwork subnet)
        {
            subnetworkList.Add(subnet);
        }

        public string ReturnName()
        {
            return subnetworkName;
        }

        public List<Subnetwork> ReturnSubnetworks()
        {
            return subnetworkList;
        }

        public List<NetworkNode> ReturnNodes()
        {
            return networkNodeList;
        }
    }
}
