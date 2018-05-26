using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EONemulator
{
    public partial class MainWindow : Form
    {
        private List<Domain> domains;
        private CommunicationServer server;

        public MainWindow()
        {
            InitializeComponent();
            Topology topology = Program.ReadTopology(this);
            domains = topology.domains;
            server = new CommunicationServer(topology.componentPorts);
            FillForm();
            ConnectToCloud();
        }

        private void FillForm()
        {
            foreach (Domain d in domains)
                domainListBox.Items.Add(d.ReturnName());
        }

        private void ConnectToCloud()
        {
            foreach (Domain d in domains)
            {
                foreach (Subnetwork s in d.ReturnSubnetworks())
                {
                    foreach (NetworkNode nn in s.ReturnNodes())
                        nn.startNode();
                }
            }
        }

        public void CreateServer(Dictionary<string, int> dictionary)
        {
            server = new CommunicationServer(dictionary);
        }

        private void domainListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            subnetworkListBox.Items.Clear();
            nodeListBox.Items.Clear();
            subnetworkListBox.Items.Add(domains[domainListBox.SelectedIndex].ReturnMainSubnetwork().ReturnName());
            foreach (Subnetwork sn in domains[domainListBox.SelectedIndex].ReturnSubnetworks())
                subnetworkListBox.Items.Add(sn.ReturnName());
        }

        private void domainListBox_DoubleClick(object sender, EventArgs e)
        {
            this.logTextBox.DataBindings.Clear();
            this.logTextBox.DataBindings.Add("Text", domains[domainListBox.SelectedIndex], "logs", false, DataSourceUpdateMode.OnPropertyChanged);

        }

        private void subnetworkListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            nodeListBox.Items.Clear();
            if (subnetworkListBox.SelectedIndex != 0)
            {
                foreach (NetworkNode nn in domains[domainListBox.SelectedIndex].ReturnSubnetworks().ElementAt(subnetworkListBox.SelectedIndex - 1).ReturnNodes())
                    nodeListBox.Items.Add(nn.ReturnName());
            }
        }

        private void subnetworkListBox_DoubleClick(object sender, EventArgs e)
        {
            this.logTextBox.DataBindings.Clear();
            if (subnetworkListBox.SelectedIndex != 0)
                this.logTextBox.DataBindings.Add("Text", domains[domainListBox.SelectedIndex].ReturnSubnetworks().ElementAt(subnetworkListBox.SelectedIndex - 1), "logs", false, DataSourceUpdateMode.OnPropertyChanged);
            else
                this.logTextBox.DataBindings.Add("Text", domains[domainListBox.SelectedIndex].ReturnMainSubnetwork(), "logs", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void nodeListBox_DoubleClick(object sender, EventArgs e)
        {
            this.logTextBox.DataBindings.Clear();
            if (subnetworkListBox.SelectedIndex != 0)
                this.logTextBox.DataBindings.Add("Text", domains[domainListBox.SelectedIndex].ReturnSubnetworks().ElementAt(subnetworkListBox.SelectedIndex - 1).ReturnNodes().ElementAt(nodeListBox.SelectedIndex), "logs", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void crackButton_Click(object sender, EventArgs e)
        {
            int linkID = Int32.Parse(textBox1.Text);
            CommandLibrary.Link l = new CommandLibrary.Link();
            l.linkID = linkID;
            string destination = null;
            if (linkID == 10)
            {
                destination = "A:SN1:SN4:LRM";
            }
            if (linkID == 3 || linkID == 4 || linkID == 5)
            {
                destination = "A:SN1:SN17:LRM";
            }
            if (linkID == 8)
            {
                destination = "B:SN2:SN5:LRM";
            }
            if (linkID == 9 || linkID == 6 || linkID == 1)
            {
                destination = "B:SN2:LRM";
            }
            if (linkID == 0 || linkID == 1 || linkID == 2 || linkID == 7 || linkID == 6)
            {
                destination = "A:SN1:LRM";
            }


            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 60000);
            Socket handler = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            handler.Connect("127.0.0.1", 50000);

            CommandLibrary.Command message = new CommandLibrary.Command("Link Delete");
            message.linkList = new List<CommandLibrary.Link>();
            l.usedSlots.Add(1); //Test!!!!!
            l.usedSlots.Add(2);
            message.linkList.Add(l);
            message.sourceId = "Cracker";
            message.destinationId = destination;
            message.deletedLinkId = linkID;


            using (var stream = new NetworkStream(handler))
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, message);
                stream.Flush();
                stream.Close();
            }
            handler.Close();
        }
    }
}
