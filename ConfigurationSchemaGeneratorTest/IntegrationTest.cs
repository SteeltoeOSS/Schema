// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Reflection;

namespace ConfigurationSchemaGeneratorTest;

public sealed class IntegrationTest
{
    private const string JsonFileName = "ConfigurationSchema.json";

    [Fact]
    public void Refresh_baseline_files_from_Steeltoe_repository()
    {
        char[] separators =
        [
            Path.DirectorySeparatorChar,
            '.'
        ];

        string inputDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../../Steeltoe/src"));
        string outputDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../Baseline/InputFiles"));

        if (!Directory.Exists(inputDirectory))
        {
            throw new InvalidOperationException("Please clone the Steeltoe git repository first.");
        }

        Directory.Delete(outputDirectory, true);
        Directory.CreateDirectory(outputDirectory);

        foreach (string sourcePath in Directory.EnumerateFiles(inputDirectory, JsonFileName, SearchOption.AllDirectories))
        {
            string relativeSourcePath = sourcePath[inputDirectory.Length..];
            string relativeSourceDirectory = Path.GetDirectoryName(relativeSourcePath)!;

            List<string> directorySegments = relativeSourceDirectory.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (directorySegments.Contains("obj"))
            {
                continue;
            }

            int srcIndex = directorySegments.IndexOf("src");

            if (srcIndex != -1)
            {
                directorySegments.RemoveAt(srcIndex);
            }

            string targetSubdirectoryName = string.Join('_', directorySegments);
            string targetDirectory = Path.Combine(outputDirectory, targetSubdirectoryName);
            Directory.CreateDirectory(targetDirectory);

            string targetPath = Path.Combine(targetDirectory, JsonFileName);
            string content = File.ReadAllText(sourcePath);
            File.WriteAllText(targetPath, content);
        }
    }

    [Fact]
    public void Merges_files_as_expected()
    {
        // When this test fails, it is likely that the test above updated the baseline schema files from Steeltoe.
        // To make this test succeed, manually verify that the changed merge-actual.json is correct, then overwrite merge-expected.json.

        MethodInfo entryPoint = typeof(Program).Assembly.EntryPoint!;

        string inputDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../Baseline/InputFiles"));
        string outputDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../Baseline"));
        string outputPath = Path.Combine(outputDirectory, "merge-actual.json");

        string[] args =
        [
            outputPath,
            inputDirectory
        ];

        entryPoint.Invoke(null, [args]);

        string outputJson = File.ReadAllText(outputPath);
        string expectedJson = File.ReadAllText(Path.Combine(outputDirectory, "merge-expected.json")).ReplaceLineEndings();

        outputJson.Should().Be(expectedJson);
    }
}
