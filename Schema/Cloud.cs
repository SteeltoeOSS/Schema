using Steeltoe.Extensions.Configuration.ConfigServer;
using Steeltoe.Stream.Config;
using System.ComponentModel;

namespace Schema
{
    public class Cloud
    {
        [Description("Settings for interacting with Spring Cloud Config Server")]
        public ConfigServerClientSettings Config { get; set; }

        [Description("Settings for interacting with Kubernetes for Configuration and Service Discovery")]
        public KubernetesConfig Kubernetes { get; set; }

        [Description("Settings for Steeltoe Stream")]
        public BindingServiceOptions Stream { get; set; }
    }
}