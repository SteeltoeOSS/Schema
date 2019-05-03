using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Steeltoe.CloudFoundry.Connector.Hystrix;
using Steeltoe.CloudFoundry.Connector.MongoDb;
using Steeltoe.CloudFoundry.Connector.MySql;
using Steeltoe.CloudFoundry.Connector.OAuth;
using Steeltoe.CloudFoundry.Connector.PostgreSql;
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.CloudFoundry.Connector.SqlServer;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Extensions.Configuration.ConfigServer;
using Steeltoe.Management.Endpoint;
using Steeltoe.Management.Endpoint.CloudFoundry;
using Steeltoe.Management.Endpoint.Env;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.HeapDump;
using Steeltoe.Management.Endpoint.Hypermedia;
using Steeltoe.Management.Endpoint.Info;
using Steeltoe.Management.Endpoint.Loggers;
using Steeltoe.Management.Endpoint.Mappings;
using Steeltoe.Management.Endpoint.Metrics;
using Steeltoe.Management.Endpoint.Refresh;
using Steeltoe.Management.Endpoint.ThreadDump;
using Steeltoe.Management.Endpoint.Trace;

namespace Schema
{
    public class ConfigRoot
    {
        /// <summary>
        /// Spring configuration
        /// </summary>
        public Spring Spring { get; set; }
        /// <summary>
        /// Eureka configuration
        /// </summary>
        public EurekaConfig Eureka { get; set; }
        public LoggingRoot Logging { get; set; }
        public ConnectorConfig<MySqlProviderConnectorOptions> MySql { get; set; }
        public ConnectorConfig<PostgresProviderConnectorOptions> Postgres { get; set; }
        public ConnectorConfig<RabbitMQProviderConnectorOptions> Rabbitmq { get; set; }
        public ConnectorConfig<RedisCacheConnectorOptions> Redis { get; set; }
        public ConnectorConfig<SqlServerProviderConnectorOptions> SqlServer { get; set; }
        public ConnectorConfig<MongoDbConnectorOptions> MongoDb { get; set; }
        public ConnectorConfig<HystrixProviderConnectorOptions> Hystrix { get; set; }
        public Security Security { get; set; }
        public Management Management { get; set; }
    }

    public class Security
    {
        public ConnectorConfig<OAuthConnectorOptions> Oauth2 { get; set; }
    }
    public class Management
    {
        public Endpoints Endpoints { get; set; }
    }

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

    public class HypermediaEndpointOptionsSchema : HypermediaEndpointOptions
    {
        public Exposure Exposure { get; set; }
    }
    public class LoggingRoot// : Dictionary<string,Logging>
    {
        public bool IncludeScopes { get; set; }
        public Dictionary<string,LogLevel> LogLevel { get; set; }
    }
    public class Logging
    {
        public bool IncludeScopes { get; set; }
        public Dictionary<string,LogLevel> LogLevel { get; set; }
    }
    public class Cloud
    {
        public ConfigServerClientSettings Config { get; set; }
    }

    public class ConnectorConfig<T>
    {
        public T Client { get; set; }
    }

    public class EurekaConfig
    {
        /// <summary>
        /// Eureka client configuration
        /// </summary>
        public IEurekaClientConfig Client { get; set; }
        /// <summary>
        /// Configuration for how application instance is registered in eureka
        /// </summary>
        public IEurekaInstanceConfig Instance { get; set; }
    }

    public class Spring
    {
        public Application Application { get; set; }
        public Cloud Cloud { get; set; }
    }

    public class Application
    {
        /// <summary>
        /// Application name
        /// </summary>
        public string Name { get; set; }
    }
}