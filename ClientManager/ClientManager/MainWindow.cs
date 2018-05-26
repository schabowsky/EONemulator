using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientManager
{
    public partial class MainWindow : Form
    {
        public static MainWindow self;
        private List<Client> clients;
        public string messageDestination
        {
            get { return messageDestinationComboBox.SelectedItem.ToString(); }
        }

        public MainWindow()
        {
            InitializeComponent();
            clients = Program.ReadClients(this);
            StartClients();
            self = this;
        }

        private void StartClients()
        {
            foreach (Client c in clients)
                c.MakeConnection();
        }

        private void client1RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.logTextBox.DataBindings.Clear();
            this.logTextBox.DataBindings.Add("Text", clients.Find(x => x.ReturnName() == "K1"), "logs", false, DataSourceUpdateMode.OnPropertyChanged);
            this.destinationComboBox.SelectedItem = null;
            this.destinationComboBox.Items.Clear();
            this.destinationComboBox.Items.AddRange(new string[] { "K2", "K3" });
            this.messageDestinationComboBox.SelectedItem = null;
            this.messageDestinationComboBox.Items.Clear();
            this.messageDestinationComboBox.Items.AddRange(new string[] { "K2", "K3" });
        }

        private void client2RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.logTextBox.DataBindings.Clear();
            this.logTextBox.DataBindings.Add("Text", clients.Find(x => x.ReturnName() == "K2"), "logs", false, DataSourceUpdateMode.OnPropertyChanged);
            this.destinationComboBox.SelectedItem = null;
            this.destinationComboBox.Items.Clear();
            this.destinationComboBox.Items.AddRange(new string[] { "K1", "K3" });
            this.messageDestinationComboBox.SelectedItem = null;
            this.messageDestinationComboBox.Items.Clear();
            this.messageDestinationComboBox.Items.AddRange(new string[] { "K1", "K3" });
        }

        private void client3RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            this.logTextBox.DataBindings.Clear();
            this.logTextBox.DataBindings.Add("Text", clients.Find(x => x.ReturnName() == "K3"), "logs", false, DataSourceUpdateMode.OnPropertyChanged);
            this.destinationComboBox.SelectedItem = null;
            this.destinationComboBox.Items.Clear();
            this.destinationComboBox.Items.AddRange(new string[] { "K1", "K2" });
            this.messageDestinationComboBox.SelectedItem = null;
            this.messageDestinationComboBox.Items.Clear();
            this.messageDestinationComboBox.Items.AddRange(new string[] { "K1", "K2" });
        }

        private void requestButton_Click(object sender, EventArgs e)
        {
            string destination = string.Empty;
            int bandwidth = 0;
            try
            {
                destination = destinationComboBox.SelectedItem.ToString();
                bandwidth = Int32.Parse(bandwidthTextBox.Text);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            if (client1RadioButton.Checked && bandwidth != 0 && destination != "")
                clients.Find(x => x.ReturnName() == "K1").SendRequest(destination, bandwidth);
            else if (client2RadioButton.Checked && bandwidth != 0 && destination != "")
                clients.Find(x => x.ReturnName() == "K2").SendRequest(destination, bandwidth);
            else if (client3RadioButton.Checked && bandwidth != 0 && destination != "")
                clients.Find(x => x.ReturnName() == "K3").SendRequest(destination, bandwidth);
        }

        private void messageButton_Click(object sender, EventArgs e)
        {
            string destination = string.Empty;
            string text = string.Empty;
            try
            {
                destination = messageDestinationComboBox.SelectedItem.ToString();
                text = textTextBox.Text;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
            if (client1RadioButton.Checked && text != "" && destination != "")
                clients.Find(x => x.ReturnName() == "K1").Send(text, destination);
            else if (client2RadioButton.Checked && text != "" && destination != "")
                clients.Find(x => x.ReturnName() == "K2").Send(text, destination);
            else if (client3RadioButton.Checked && text != "" && destination != "")
                clients.Find(x => x.ReturnName() == "K3").Send(text, destination);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (Client c in clients)
            {
                c.CloseConnection();
            }
        }
    }
}
