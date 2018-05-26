using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EONemulator
{
    class Topology
    {
        public Dictionary<string, int> componentPorts;
        public List<Domain> domains;

        public Topology(Dictionary<string, int> componentPorts, List<Domain> domains)
        {
            this.componentPorts = componentPorts;
            this.domains = domains;
        }
    }
}
