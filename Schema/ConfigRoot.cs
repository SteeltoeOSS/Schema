using Steeltoe.Connector.CosmosDb;
using Steeltoe.Connector.Hystrix;
using Steeltoe.Connector.MongoDb;
using Steeltoe.Connector.MySql;
using Steeltoe.Connector.PostgreSql;
using Steeltoe.Connector.RabbitMQ;
using Steeltoe.Connector.Redis;
using Steeltoe.Discovery.Consul.Discovery;
using System.ComponentModel;

namespace Schema
{
    public class ConfigRoot
    {
        [Description("General settings for Steeltoe and Spring Cloud-compatible features")]
        public Spring Spring { get; set; }

        [Description("Settings for service discovery and registration with Spring Cloud Eureka")]
        public EurekaConfig Eureka { get; set; }

        [Description("Settings for service discovery and registration with Hashicorp Consul")]
        public ConsulConfig Consul { get; set; }

        [Description("Settings for Microsoft's ILogging Infrastructure")]
        public LoggingRoot Logging { get; set; }

        [Description("Settings for Steeltoe CosmosDb Connector")]
        public ConnectorConfig<CosmosDbConnectorOptions> CosmosDb { get; set; }

        [Description("Settings for Steeltoe MySQL Connector")]
        public ConnectorConfig<MySqlProviderConnectorOptions> MySql { get; set; }

        [Description("Settings for Steeltoe PostgreSQL Connector")]
        public ConnectorConfig<PostgresProviderConnectorOptions> Postgres { get; set; }

        [Description("Settings for Steeltoe RabbitMQ Connector")]
        public ConnectorConfig<RabbitMQProviderConnectorOptions> Rabbitmq { get; set; }

        [Description("Settings for Steeltoe Redis Connector")]
        public ConnectorConfig<RedisCacheConnectorOptions> Redis { get; set; }

        [Description("Settings for Steeltoe Microsoft SQL Server Connector")]
        public SqlServerConfig SqlServer { get; set; }

        [Description("Settings for Steeltoe MongoDb Connector")]
        public ConnectorConfig<MongoDbConnectorOptions> MongoDb { get; set; }

        [Description("Settings for Hystrix Circuit Breakers")]
        public ConnectorConfig<HystrixProviderConnectorOptions> Hystrix { get; set; }

        [Description("Settings for Steeltoe Authentication Providers")]
        public Security Security { get; set; }

        [Description("Settings for Steeltoe Management Endpoints")]
        public Management Management { get; set; }
    }
}