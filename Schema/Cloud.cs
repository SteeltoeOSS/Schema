using Steeltoe.Extensions.Configuration.ConfigServer;
using System.ComponentModel;

namespace Schema
{
    public class Cloud
    {
        [Description("Settings for interacting with Spring Cloud Config Server")]
        public ConfigServerClientSettings Config { get; set; }

        [Description("Settings for interacing with Kubernetes for Configuration and Service Discovery")]
        public KubernetesConfig Kubernetes { get; set; }
    }
}