using Steeltoe.Discovery.Consul;
using Steeltoe.Discovery.Consul.Discovery;

namespace Schema
{
    public class ConsulConfig : ConsulOptions
    {
        public ConsulDiscoveryOptions Discovery { get; set; }
    }
}
