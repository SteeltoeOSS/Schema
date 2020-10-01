using Steeltoe.Connector.OAuth;

namespace Schema
{
    public class Security
    {
        public ConnectorConfig<OAuthConnectorOptions> Oauth2 { get; set; }
    }
}