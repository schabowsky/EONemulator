using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ClientManager
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

        public static List<Client> ReadClients(MainWindow window)
        {
            var path = System.Configuration.ConfigurationManager.AppSettings["clients"];
            XmlTextReader reader = new XmlTextReader(path);

            string clientName = string.Empty;
            string domainId = string.Empty;
            int port = 0;
            int cloudPort = 0;
            int portName = 0;
            int cpccPort = 0;
            List<Client> clientList = new List<Client>();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "client")
                        {
                            clientName = reader.GetAttribute("id");
                            domainId = reader.GetAttribute("domainId");
                            port = Int32.Parse(reader.GetAttribute("port_number"));
                            cloudPort = Int32.Parse(reader.GetAttribute("cloud_port"));
                            portName = Int32.Parse(reader.GetAttribute("port_name"));
                            cpccPort = Int32.Parse(reader.GetAttribute("cpcc_port"));
                        }
                        break;
                    case XmlNodeType.Text:
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name == "client")
                        {
                            clientList.Add(new Client(clientName, domainId, port, cloudPort, portName, cpccPort, window));
                        }
                        break;
                }
            }
            return clientList;
        }
    }
}
