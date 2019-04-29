using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;
using Steeltoe.Discovery.Eureka;

namespace Schema
{
    class Program
    {
        static void Main(string[] args)
        {

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new StringEnumConverter()
                }
            };
            var gen = new JSchemaGenerator()
            {
                DefaultRequired = Required.Default,
//                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
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