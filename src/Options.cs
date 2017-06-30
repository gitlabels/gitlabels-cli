using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

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
