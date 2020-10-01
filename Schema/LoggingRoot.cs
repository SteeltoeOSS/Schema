using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Schema
{
    public class LoggingRoot// : Dictionary<string,Logging>
    {
        public bool IncludeScopes { get; set; }
        public Dictionary<string,LogLevel> LogLevel { get; set; }
    }

    public class Logging
    {
        public bool IncludeScopes { get; set; }

        public Dictionary<string, LogLevel> LogLevel { get; set; }
    }

}