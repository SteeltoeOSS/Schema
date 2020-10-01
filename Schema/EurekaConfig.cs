using Steeltoe.Discovery.Eureka;
using System.ComponentModel;

namespace Schema
{
    public class EurekaConfig
    {
        [Description("Settings for connection to the Eureka server")]
        public IEurekaClientConfig Client { get; set; }

        [Description("Settings for how this application instance should register itself with Eureka")]
        public IEurekaInstanceConfig Instance { get; set; }
    }
}