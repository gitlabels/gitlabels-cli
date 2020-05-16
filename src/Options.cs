using System;
using CommandLine;

namespace Goit.GitHubLabels
{
    class Options
    {
        [Option('r', "repo", Required = true, HelpText = "GitHub repository name")]
        public string RepositoryFullName { get; set; }

        [Option('t', "token", Required = false, HelpText = "GitHub authorization token. If not set, the GITHUB_TOKEN environment variable will be used")]
        public string Token { get; set; }

        [Option('c', "config", Required = false, HelpText = "Configuration file with labels")]
        public string ConfigFile { get; set; }
    }
}
