using System;
using CommandLine;

namespace Goit.GitHubLabels
{
    class Options
    {
        [Option('r', "repo", Required = true, HelpText = "GitHub repository name")]
        public string RepositoryFullName { get; set; }

        [Option('t', "token", Required = true, HelpText = "GitHub authorization token")]
        public string Token { get; set; }

        [Option('c', "config", Required = false, HelpText = "Configuration file with labels")]
        public string ConfigFile { get; set; }
    }
}
