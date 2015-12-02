using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace FileDeploymentPrepTool
{
    class CommandLineOptions
    {
        [Option('u', "updatefolder", Required = true,
          HelpText = "The full path to the root folder containing updated code, files, and other resources.")]
        public string UpdateFolder { get; set; }

        [Option('c', "currentfolder", Required = true,
          HelpText = "The full path to the root folder containing original project code, files, and other resources.")]
        public string CurrentFolder { get; set; }

        [Option('b', "backupFolder", Required = true,
          HelpText = "The full path to the root folder that will contain any files that would be overwritten if files in the output folder are eventually manually copied to the current folder.")]
        public string BackupFolder
        {
            get; set;
        }

        [Option('o', "outputfolder", Required = true,
          HelpText = "The full path to the root folder that will be updated with modified files from the specified Update Folder.")]
        public string OutputFolder { get; set; }

        [Option('s', "skipNewFiles", DefaultValue =true,
          HelpText = "Set to true to skip over files that do not already exist in the current folder structure.")]
        public bool SkipNewFiles { get; set; }

        [Option('e', "excludefolders",
          HelpText = "A comma-delimited list of folder or file names that should be skipped during examination.")]
        public string ExcludeFoldersContaining { get; set; }
        

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    
}
