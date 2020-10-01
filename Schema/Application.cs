using System.ComponentModel;

namespace Schema
{
    public class Application
    {
        [Description("Application name. Can generally be overridden under specific components.")]
        public string Name { get; set; }
    }
}