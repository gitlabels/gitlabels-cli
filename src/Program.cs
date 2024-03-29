﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using Octokit;
using Octokit.Internal;

namespace Goit.GitHubLabels
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            var exitCode = await result.MapResult(
                async (Options options) => await RunProgram(options),
                err => Task.FromResult(1)
            );

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }

            return exitCode;
        }

        private static async Task<int> RunProgram(Options options)
        { 
            var tmp = options.RepositoryFullName.Split('/');
            var username = tmp[0];
            var repoName = tmp[1];

            var token = options.Token;
            if (String.IsNullOrWhiteSpace(token))
            {
                token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            }

            if (String.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Provide GitHub login token with -t parameter or with GITHUB_TOKEN environment variable.");
                return 1;
            }

            var credentials = new InMemoryCredentialStore(new Credentials(token));
            var github = new GitHubClient(new ProductHeaderValue("gitlabels.exe"), credentials);
            var repo = await github.Repository.Get(username, repoName);
            var repoLabels = await github.Issue.Labels.GetAllForRepository(username, repoName);

            Console.WriteLine($"Syncing repository: {username}/{repoName}");

            IList<NewLabel> configLabels = null;
            if (String.IsNullOrEmpty(options.ConfigFile))
            {
                Console.WriteLine("Loading labels from repository...");
                configLabels = await LoadLabelsFromRepository(github, repo);
            }
            else
            {
                Console.WriteLine("Loading labels from config file...");
                configLabels = LoadLabelsFromConfig(options.ConfigFile);
            }

            foreach (var repoLabel in repoLabels)
            {
                var configLabel = configLabels.FirstOrDefault(l => l.Name == repoLabel.Name);

                if (configLabel == null)
                {
                    await github.Issue.Labels.Delete(repo.Id, repoLabel.Name);
                    Console.WriteLine($"  - {repoLabel.Name}");
                    continue;
                }

                if (!string.Equals(configLabel.Color, repoLabel.Color, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(configLabel.Description, repoLabel.Description, StringComparison.OrdinalIgnoreCase))
                {
                    var update = new LabelUpdate(repoLabel.Name, configLabel.Color);
                    update.Description = configLabel.Description;
                    await github.Issue.Labels.Update(repo.Id, repoLabel.Name, update);
                    Console.WriteLine($"  * {repoLabel.Name}");
                }
            }

            foreach (var configLabel in configLabels)
            {
                if (!repoLabels.Any(l => String.Equals(l.Name, configLabel.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    await github.Issue.Labels.Create(repo.Id, configLabel);
                    Console.WriteLine($"  + {configLabel.Name}");
                }
            }

            Console.WriteLine("Done.");
            return 0;
        }

        private static async Task<IList<NewLabel>> LoadLabelsFromRepository(IGitHubClient client, Repository repo)
        {
            var configFilePath = ".github/labels.json";
            var branch = repo.DefaultBranch;

            var content = await client.Repository.Content.GetAllContentsByRef(repo.Id, configFilePath, branch);

            if (content.Count != 1)
            {
                throw new InvalidOperationException($"Failed to find file '{configFilePath}' in branch '{branch}'.");
            }

            var json = content[0].Content;
            return ParseLabelsConfig(json);
        }

        private static IList<NewLabel> LoadLabelsFromConfig(string configFile)
        {
            var filename = Path.GetFullPath(configFile);

            if (!File.Exists(filename))
            {
                throw new InvalidOperationException($"Configuration file with labels was not found. Path: {filename}");
            }

            var json = File.ReadAllText(filename);
            return ParseLabelsConfig(json);
        }

        private static IList<NewLabel> ParseLabelsConfig(string json)
        {
            var labels = new List<NewLabel>();

            var parserOptions = new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
                MaxDepth = 5
            };

            using var document = JsonDocument.Parse(json, parserOptions);
            foreach (var label in document.RootElement.EnumerateArray())
            {
                var name = label.GetProperty("name").GetString();
                var color = label.GetProperty("color").GetString();
                if (color[0] == '#')
                {
                    color = color.Substring(1);
                }

                var lbl = new NewLabel(name, color);
                if (label.TryGetProperty("description", out var descriptionProperty))
                {
                    lbl.Description = descriptionProperty.GetString();
                }

                labels.Add(lbl);
            }

            return labels;
        }
    }
}
