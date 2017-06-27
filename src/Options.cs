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
        [Option('r', "repo", HelpText = "GitHub repository name")]
        public string RepositoryFullName { get; set; }

        [Option('t', "token", HelpText = "GitHub authorization token")]
        public string Token { get; set; }

        [Option('c', "config", HelpText = "Configuration file with labels")]
        public string ConfigFile { get; set; }


        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
