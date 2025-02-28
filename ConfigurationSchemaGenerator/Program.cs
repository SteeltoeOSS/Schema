// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using ConfigurationSchemaGenerator;

if (args.Length != 2)
{
    Console.WriteLine("A tool that merges ConfigurationSchema.json files, which contain JSON schemas for .NET configuration keys.");
    Console.WriteLine();
    Console.Write($"Usage: {Assembly.GetEntryAssembly()!.GetName().Name} <output-path> <input-directory>");
    Console.Write($@"Example: {Assembly.GetEntryAssembly()!.GetName().Name} C:\Temp\output.json C:\Repos\Steeltoe");
}
else
{
    string outputPath = args[0];
    string rootDirectory = args[1];

    var merger = new JsonSchemaMerger();

    foreach (string path in Directory.EnumerateFiles(rootDirectory, "ConfigurationSchema.json", SearchOption.AllDirectories))
    {
        Console.WriteLine($"Merging from file: {path}");
        merger.AddSourceFile(path);
    }

    merger.RemoveLogLevels();
    string? json = merger.GetResult();

    Console.WriteLine($"Writing results to file: {Path.GetFullPath(outputPath)}");
    File.WriteAllText(outputPath, json);
}
