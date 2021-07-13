using Steeltoe.Messaging.RabbitMQ.Config;

namespace Schema
{
    public class Spring
    {
        public Application Application { get; set; }

        public Cloud Cloud { get; set; }

        public RabbitOptions RabbitMQ { get; set; }
    }
}