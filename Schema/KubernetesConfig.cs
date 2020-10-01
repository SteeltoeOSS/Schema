using Steeltoe.Common.Kubernetes;
using Steeltoe.Discovery.Kubernetes.Discovery;
using System.ComponentModel;

namespace Schema
{
    public class KubernetesConfig : KubernetesApplicationOptions
    {
        public static string[] IgnoredProperties = { "PlatformConfigRoot", "PlatformRoot" };

        [Description("Settings for using the Kubernetes API for service discovery")]
        public KubernetesDiscoveryOptions Discovery { get; set; }
    }
}
