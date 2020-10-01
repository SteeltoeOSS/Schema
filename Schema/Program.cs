using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using Steeltoe.Common;
using Steeltoe.Common.Kubernetes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Schema
{
    class Program
    {
        static void Main(string[] args)
        {
            var propertyIgnoringResolver = new PropertyIgnoreContractResolver();
            propertyIgnoringResolver.IgnoreProperty(typeof(ApplicationInstanceInfo), "ApplicationRoot", "SpringApplicationRoot", "ServicesRoot", "EurekaRoot", "ConfigServerRoot", "ConsulRoot", "KubernetesRoot", "ManagementRoot", "Application_Id", "EurekaInstanceNameKey", "PlatformNameKey", "ManagementNameKey", "KubernetesNameKey", "ConsulInstanceNameKey", "ConfigServerNameKey", "AppNameKey", "AppInstanceIdKey", "Instance_Id", "DefaultAppName");
            propertyIgnoringResolver.IgnoreProperty(typeof(KubernetesApplicationOptions), KubernetesConfig.IgnoredProperties);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Converters = new List<JsonConverter>() { new StringEnumConverter() } };
            var gen = new JSchemaGenerator() { DefaultRequired = Required.DisallowNull, ContractResolver = propertyIgnoringResolver };
            var myType = typeof(ConfigRoot);
            var schema = gen.Generate(myType);
            
            var schemaObj = JObject.Parse(schema.ToString());
            var enumNodes = schemaObj.SelectTokens("$..enum");
            foreach (var node in enumNodes)
            {
                node.Parent.Parent["type"] = "string";    
            }
            
            Console.WriteLine(schemaObj.ToString());
//            return;
            if (args.Any())
            {
                File.WriteAllText(args[0], schemaObj.ToString());
            }
        }
    }
}