using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EONemulator
{
    class Domain
    {
        private string domainName;
        public MainWindow mainWindow;
        private Subnetwork mainSubnetwork;
        private NCC ncc;
        private PC pc;
        private string logs;

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

        public Domain(string id, string nccId, int nccPort, string pcId, int pcPort, Dictionary<string, string> dictionary, MainWindow window)
        {
            logs = DateTime.UtcNow.ToString("HH:mm:ss.fff") + $" >> {id} >> " + "Domain created." + Environment.NewLine;
            mainWindow = window;
            ncc = new NCC(this, nccPort, nccId);
            pc = new PC(this, pcPort, pcId, dictionary);
            domainName = id;
            mainSubnetwork = null;
        }

        public void AddMainSubnetwork(Subnetwork subnet)
        {
            mainSubnetwork = subnet;
        }
        public void AddSubnetwork(Subnetwork subnetwork)
        {
            mainSubnetwork.AddSubnetwork(subnetwork);
        }

        public string ReturnName()
        {
            return domainName;
        }

        public Subnetwork ReturnMainSubnetwork()
        {
            return mainSubnetwork;
        }

        public List<Subnetwork> ReturnSubnetworks()
        {
            return mainSubnetwork.ReturnSubnetworks();
        }
    }
}
