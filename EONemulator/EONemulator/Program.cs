using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace EONemulator
{
    static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }

        public static Topology ReadTopology(MainWindow window)
        {
            var path = System.Configuration.ConfigurationManager.AppSettings["topology"];
            XmlTextReader reader = new XmlTextReader(path);

            string domainId = string.Empty;
            string subnetworkId = string.Empty;
            string oxcId = string.Empty;
            string portId = string.Empty;
            string portNumber = string.Empty;
            string cloudPort = string.Empty;
            string nccId = string.Empty;
            int nccPort = 0;
            string lrmId = string.Empty;
            int lrmPort = 0;
            string ccId = string.Empty;
            int ccPort = 0;
            string rcId = string.Empty;
            int rcPort = 0;
            string pcId = string.Empty;
            int pcPort = 0;
            string mainSubnetId = string.Empty;
            int oxcCCPort = 0;
            List<int> portList = new List<int>();
            NetworkNode netNode = null;
            Subnetwork subnet = null;
            Subnetwork mainSubnet = null;
            List<NetworkNode> nodeList = new List<NetworkNode>();
            List<Subnetwork> subnetList = new List<Subnetwork>();
            List<Domain> domainList = new List<Domain>();
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            Dictionary<string, string> clientsDictionary = new Dictionary<string, string>();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "domain")
                        {
                            domainId = reader.GetAttribute("id");
                            nccId = domainId + ":NCC";
                            nccPort = Int32.Parse(reader.GetAttribute("ncc_port"));
                            dictionary.Add(nccId, nccPort);
                            mainSubnetId = domainId + ":" + reader.GetAttribute("subnetwork_id");
                            string lrm = mainSubnetId + ":LRM";
                            int lrmP = Int32.Parse(reader.GetAttribute("lrm_port"));
                            dictionary.Add(lrm, lrmP);
                            string rc = mainSubnetId + ":RC";
                            int rcP = Int32.Parse(reader.GetAttribute("rc_port"));
                            dictionary.Add(rc, rcP);
                            string cc = mainSubnetId + ":CC";
                            int ccP = Int32.Parse(reader.GetAttribute("cc_port"));
                            dictionary.Add(cc, ccP);
                            mainSubnet = new Subnetwork(mainSubnetId, lrm, lrmP, cc, ccP, rc, rcP, window);
                            pcPort = Int32.Parse(reader.GetAttribute("pc_port"));
                            pcId = domainId + ":PC";
                            dictionary.Add(pcId, pcPort);
                        }
                        else if (reader.Name == "subnetwork")
                        {
                            subnetworkId = mainSubnetId + ":" + reader.GetAttribute("id");
                            ccId = subnetworkId + ":CC";
                            ccPort = Int32.Parse(reader.GetAttribute("cc_port"));
                            dictionary.Add(ccId, ccPort);
                            rcId = subnetworkId + ":RC";
                            rcPort = Int32.Parse(reader.GetAttribute("rc_port"));
                            dictionary.Add(rcId, rcPort);
                            lrmId = subnetworkId + ":LRM";
                            lrmPort = Int32.Parse(reader.GetAttribute("lrm_port"));
                            dictionary.Add(lrmId, lrmPort);
                        }
                        else if (reader.Name == "oxc")
                        {
                            oxcId = subnetworkId + ":" + reader.GetAttribute("id");
                            portNumber = reader.GetAttribute("port_number");
                            cloudPort = reader.GetAttribute("cloud_port");
                            oxcCCPort = Int32.Parse(reader.GetAttribute("cc_port"));
                            dictionary.Add(oxcId, oxcCCPort);
                        }
                        else if (reader.Name == "port")
                        {
                            portId = reader.GetAttribute("id");
                        }
                        else if (reader.Name == "client")
                        {
                            string clientId = reader.GetAttribute("id");
                            string portName = reader.GetAttribute("port_name");
                            int cpccPort = Int32.Parse(reader.GetAttribute("cpcc_port"));
                            clientsDictionary.Add(clientId, domainId);
                            dictionary.Add(domainId + ":" + clientId + ":CPCC", cpccPort);
                        }
                        break;
                    case XmlNodeType.Text:
                        Console.WriteLine(reader.Value);
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "domain")
                        {
                            Domain domain = new Domain(domainId, nccId, nccPort, pcId, pcPort, clientsDictionary, window);
                            domain.AddMainSubnetwork(mainSubnet);
                            foreach (Subnetwork sn in subnetList)
                                domain.AddSubnetwork(sn);
                            domainList.Add(domain);
                            subnetList.Clear();
                            subnet = null;
                        }
                        else if (reader.Name == "subnetwork")
                        {
                            subnet = new Subnetwork(subnetworkId, lrmId, lrmPort, ccId, ccPort, rcId, rcPort, window);
                            foreach (NetworkNode nn in nodeList)
                                subnet.AddNode(nn);
                            subnetList.Add(subnet);
                            nodeList.Clear();
                        }
                        else if (reader.Name == "oxc")
                        {
                            Console.WriteLine("koniec oxc!");
                            netNode = new NetworkNode(oxcId, Int32.Parse(portNumber), Int32.Parse(cloudPort), oxcCCPort, window);
                            foreach (int p in portList)
                                netNode.AddPort(p);
                            nodeList.Add(netNode);
                            portList.Clear();
                        }
                        else if (reader.Name == "port")
                        {
                            Console.WriteLine("koniec portu!");
                            portList.Add(Int32.Parse(portId));
                        }
                        break;
                }
            }
            Topology topology = new Topology(dictionary, domainList);
            return topology;
        }
    }
}
