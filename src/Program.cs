using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json.Linq;
using Octokit;
using Octokit.Internal;

namespace Goit.GitHubLabels
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                Console.WriteLine(options.GetUsage(null));
                return Parser.DefaultExitCodeFail;
            }

            MainAsync(options).GetAwaiter().GetResult();

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }

            return 0;
        }

        private static async Task MainAsync(Options options)
        {
            var tmp = options.RepositoryFullName.Split('/');
            var username = tmp[0];
            var repoName = tmp[1];

            var credentials = new InMemoryCredentialStore(new Credentials(options.Token));
            var github = new GitHubClient(new ProductHeaderValue("gitlabels.exe"), credentials);
            var repo = await github.Repository.Get(username, repoName);
            var repoLabels = await github.Issue.Labels.GetAllForRepository(username, repoName);
            var configLabels = LoadLabelsFromConfig(options.ConfigFile);

            Console.WriteLine($"Syncing repository: {username}/{repoName}");


            foreach (var repoLabel in repoLabels)
            {
                var configLabel = configLabels.FirstOrDefault(l => l.Name == repoLabel.Name);

                if (configLabel == null)
                {
                    await github.Issue.Labels.Delete(repo.Id, repoLabel.Name);
                    Console.WriteLine($"  - {repoLabel.Name}");
                    continue;
                }

                if (!String.Equals(configLabel.Color, repoLabel.Color, StringComparison.OrdinalIgnoreCase))
                {
                    var update = new LabelUpdate(repoLabel.Name, configLabel.Color);
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
        }

        private static IList<NewLabel> LoadLabelsFromConfig(string configFile)
        {
            var filename = Path.GetFullPath(configFile);

            if (!File.Exists(filename))
            {
                throw new InvalidOperationException($"Configuration file with labels was not found. Path: {filename}");
            }

            var labels = new List<NewLabel>();

            var json = File.ReadAllText(filename);
            var config = JArray.Parse(json);
            foreach (var label in config.Values<JObject>())
            {
                var name = label["name"].Value<string>();
                var color = label["color"].Value<string>();
                if (color[0] == '#')
                {
                    color = color.Substring(1);
                }

                var lbl = new NewLabel(name, color);
                labels.Add(lbl);

            }
            return labels;
        }
    }
}
