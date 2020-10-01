using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Env;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.HeapDump;
using Steeltoe.Management.Endpoint.Info;
using Steeltoe.Management.Endpoint.Loggers;
using Steeltoe.Management.Endpoint.Mappings;
using Steeltoe.Management.Endpoint.Metrics;
using Steeltoe.Management.Endpoint.Refresh;
using Steeltoe.Management.Endpoint.ThreadDump;
using Steeltoe.Management.Endpoint.Trace;

namespace Schema
{
    public class Endpoints
    {
        public string Path { get; set; }

        public string Enabled { get; set; }

        public string Sensitive { get; set; }

        public CloudFoundryEndpointOptions CloudFoundry { get; set; }

        public EnvEndpointOptions Env { get; set; }

        public HealthEndpointOptions Health { get; set; }

        public HeapDumpEndpointOptions HeapDump { get; set; }

        public HttpTraceEndpointOptions HttpTrace { get; set; }

        public HypermediaEndpointOptionsSchema Actuator { get; set; }

        public InfoEndpointOptions Info { get; set; }

        public LoggersEndpointOptions Loggers { get; set; }

        public MappingsEndpointOptions Mappings { get; set; }

        public MetricsEndpointOptions Metrics { get; set; }

        public RefreshEndpointOptions Refresh { get; set; }

        public ThreadDumpEndpointOptions Dump { get; set; }

        public TraceEndpointOptions Trace { get; set; }
    }
}