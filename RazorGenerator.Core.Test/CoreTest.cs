﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Extensions;

namespace RazorGenerator.Core.Test
{
    public class CoreTest
    {
        private static readonly string[] _testNames = new[] 
        { 
            "WebPageTest",
            "WebPageHelperTest",
             "MvcViewTest",
            "MvcHelperTest",
            "TemplateTest",
            "_ViewStart",
            "DirectivesTest",
            "TemplateWithBaseTypeTest",
            "VirtualPathAttributeTest",
            "SuffixTransformerTest"
        };

        public CoreTest()
        {
            HostManager.AssemblyDirectory = Environment.CurrentDirectory;
        }

        [Theory]
        [PropertyData("V1Tests")]
        [PropertyData("V2Tests")]
        public void TestTransformerType(string testName, RazorRuntime runtime)
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            using (var razorGenerator = new HostManager(workingDirectory, loadExtensions: false, defaultRuntime: runtime))
            {
                string inputFile = SaveInputFile(workingDirectory, testName);
                var host = razorGenerator.CreateHost(inputFile, testName + ".cshtml");
                host.DefaultNamespace = GetType().Namespace;
                host.EnableLinePragmas = false;

                var output = host.GenerateCode();
                AssertOutput(testName, output, runtime);
            }
        }

        public static IEnumerable<object[]> V1Tests
        {
            get
            {
                return _testNames.Select(c => new object[] { c, RazorRuntime.Version1 });
            }
        }

        public static IEnumerable<object[]> V2Tests
        {
            get
            {
                return _testNames.Select(c => new object[] { c, RazorRuntime.Version2 });
            }
        }

        private static string SaveInputFile(string outputDirectory, string testName)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            string outputFile = Path.Combine(outputDirectory, testName + ".cshtml");
            File.WriteAllText(outputFile, GetManifestFileContent(testName, "Input"));
            return outputFile;
        }

        private static void AssertOutput(string testName, string output, RazorRuntime runtime)
        {
            var expectedContent = GetManifestFileContent(testName, "Output_v" + (int)runtime);
            output = Regex.Replace(output, @"Runtime Version:[\d.]*", "Runtime Version:N.N.NNNNN.N")
                          .Replace(typeof(HostManager).Assembly.GetName().Version.ToString(), "v.v.v.v");

            Assert.Equal(expectedContent, output);
        }

        private static string GetManifestFileContent(string testName, string fileType)
        {
            var extension = fileType.Equals("Input", StringComparison.OrdinalIgnoreCase) ? "cshtml" : "txt";
            var resourceName = String.Join(".", "RazorGenerator.Core.Test.TestFiles", fileType, testName, extension);

            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
