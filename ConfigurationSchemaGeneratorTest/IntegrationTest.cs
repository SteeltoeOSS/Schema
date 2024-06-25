// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System.Reflection;

namespace ConfigurationSchemaGeneratorTest;

public sealed class IntegrationTest
{
    [Fact]
    public void Merges_files_as_expected()
    {
        MethodInfo entryPoint = typeof(Program).Assembly.EntryPoint!;

        string sourceDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../Baseline/InputFiles"));
        string outputDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../Baseline"));
        string outputPath = Path.Combine(outputDirectory, "merge-actual.json");

        string[] args =
        [
            outputPath,
            sourceDirectory
        ];

        entryPoint.Invoke(null, [args]);

        string outputJson = File.ReadAllText(outputPath);
        string expectedJson = File.ReadAllText(Path.Combine(outputDirectory, "merge-expected.json"));

        outputJson.Should().Be(expectedJson);
    }
}
