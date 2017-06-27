using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Octokit;

namespace Goit.GitHubLabels
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: gitlabels.exe <user>/<repo>");
                return 1;
            }

            var arg1 = args[0];
            if (!arg1.Contains("/"))
            {
                Console.WriteLine("Usage: gitlabels.exe <user>/<repo>");
                return 1;
            }

            var x = arg1.Split('/');
            var username = x[0];
            var repoName = x[1];

            MainAsync(username, repoName).GetAwaiter().GetResult();

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }

            return 0;
        }

        private static async Task MainAsync(string username, string repoName)
        {
            var github = new GitHubClient(new ProductHeaderValue("gitlabels.exe"));
            var labels = await github.Issue.Labels.GetAllForRepository(username, repoName);

            Console.WriteLine($"Repository: {username}/{repoName}");
            foreach (var label in labels)
            {
                Console.WriteLine($"  {label.Name}: #{label.Color}");
            }
        }
    }
}
