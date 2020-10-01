using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.Hypermedia;

namespace Schema
{
    public class HypermediaEndpointOptionsSchema : HypermediaEndpointOptions
    {
        public Exposure Exposure { get; set; }
    }
}