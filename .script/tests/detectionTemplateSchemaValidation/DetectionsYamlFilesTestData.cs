﻿using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace Kqlvalidations.Tests
{
    public class DetectionsYamlFilesTestData : TheoryData<string>
    {
        public DetectionsYamlFilesTestData()
        {
            List<string> detectionPaths = GetDetectionPaths();
            var files = GetDetectionFiles(detectionPaths);
            files.ForEach(f => AddData(Path.GetFileName(f)));
        }

        public static List<string> GetDetectionPaths()
        {
            var rootDir = Directory.CreateDirectory(GetAssemblyDirectory());
            List<string> dirPaths = new List<string>() { "Detections", "Solutions" };
            var testFolderDepth = 6;
            List<string> detectionPaths = new List<string>();
            for (int i = 0; i < testFolderDepth; i++)
            {
                rootDir = rootDir.Parent;
            }

            foreach (var dirName in dirPaths)
            {
                detectionPaths.Add(Path.Combine(rootDir.FullName, dirName));
            }

            return detectionPaths;
        }

        public static string GetRootPath()
        {
            var rootDir = Directory.CreateDirectory(GetAssemblyDirectory());
            var testFolderDepth = 6;
            for (int i = 0; i < testFolderDepth; i++)
            {
                rootDir = rootDir.Parent;
            }
            return rootDir.FullName;
        }

        public static string GetSkipTemplatesPath()
        {
            var rootDir = Directory.CreateDirectory(GetAssemblyDirectory());
            var testFolderDepth = 3;
            for (int i = 0; i < testFolderDepth; i++)
            {
                rootDir = rootDir.Parent;
            }
            return rootDir.FullName;
        }

        private static string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        private static List<string> GetDetectionFiles(List<string> detectionPaths)
        {
            int prNumber = 0;
            int.TryParse(System.Environment.GetEnvironmentVariable("PRNUM"), out prNumber);

            var files = Directory.GetFiles(detectionPaths[0], "*.yaml", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(detectionPaths[1], "*.yaml", SearchOption.AllDirectories)
                .Where(s => s.Contains("Analytic Rules")));

            if (prNumber != 0)
            {
                var client = new GitHubClient(new Octokit.ProductHeaderValue("MicrosoftSentinelValidationApp"));
                var prFiles = client.PullRequest.Files("Azure", "Azure-Sentinel", prNumber).Result;
                var prFilesListModified = new List<string>();
                var basePath = GetRootPath();
                foreach (var file in prFiles)
                {
                    var modifiedFile = Path.Combine(basePath, file.FileName.Replace('/', Path.DirectorySeparatorChar));
                    prFilesListModified.Add(modifiedFile);
                }

                files = files.Where(file => prFilesListModified.Any(prFile => file.Contains(prFile)));
            }

            var fileList = files.ToList();

            if (fileList.Count == 0)
            {
                fileList.Add("NoFile.yaml");
            }

            return fileList;
        }

    }
}