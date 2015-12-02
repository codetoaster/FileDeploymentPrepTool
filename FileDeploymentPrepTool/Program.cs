using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using System.IO;

namespace FileDeploymentPrepTool
{
    class Program
    {
        private static Dictionary<string, DateTime> m_fileLastModifiedDates = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private static HashSet<string> m_excludeFolders = new HashSet<string>();
        private static CommandLineOptions m_commandLineOptions = null;

        static bool IsFileOrFolderExcluded(string Path)
        {
            foreach (string Exclusion in m_excludeFolders)
            {
                if (Path.ToLower().Contains(Exclusion.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the part of the path exclusive to the current or updated folder to ensure correct file name comparisons.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        static string RewriteFileSystemName(bool IsCurrentFolder, string Path)
        {
            if (IsCurrentFolder)
                return Path.Replace(m_commandLineOptions.CurrentFolder, "");
            else
                return Path.Replace(m_commandLineOptions.UpdateFolder, "");            
        }

        static void IndexCurrentDirectory(string Path)
        {
            if (IsFileOrFolderExcluded(Path))
            {
                Console.WriteLine("Excluded path: {0}", Path);
                return;
            }

            foreach (string File in Directory.GetFiles(Path))
            {
                string RewrittenFileName = RewriteFileSystemName(true, File);
                Console.WriteLine("Indexing {0}", RewrittenFileName);

                m_fileLastModifiedDates.Add(RewrittenFileName, System.IO.File.GetLastWriteTime(File));
            }
            foreach (string Directory in Directory.GetDirectories(Path))
            {
                if (Path != Directory)
                {
                    IndexCurrentDirectory(Directory);
                }
            }
        }

        static void IndexUpdateDirectory(string Path)
        {
            if (IsFileOrFolderExcluded(Path))
            {
                Console.WriteLine("Excluded path: {0}", Path);
                return;
            }

            foreach (string File in Directory.GetFiles(Path))
            {
                string RewrittenFileName = RewriteFileSystemName(false, File);
                if (!m_fileLastModifiedDates.ContainsKey(RewrittenFileName))
                {
                    if (m_commandLineOptions.SkipNewFiles){ continue; }
                    else
                    {
                        //this is a file that does not exist in the current fodler structure. 
                        string DestinationPath = m_commandLineOptions.OutputFolder + RewrittenFileName;
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DestinationPath));
                        System.IO.File.Copy(m_commandLineOptions.CurrentFolder + RewrittenFileName, DestinationPath);
                        continue;
                    }
                }

                DateTime updatedLastModifiedDate = System.IO.File.GetLastWriteTime(File);         
                DateTime originalLastModifiedDate = m_fileLastModifiedDates[RewrittenFileName];                

                if (updatedLastModifiedDate.CompareTo(originalLastModifiedDate) > 0)
                {
                    Console.WriteLine("Found modified file: " + RewrittenFileName);

                    string OutputPath = m_commandLineOptions.OutputFolder + RewrittenFileName;
                    string BackupPath = m_commandLineOptions.BackupFolder + RewrittenFileName;

                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(OutputPath));
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(BackupPath));
                    System.IO.File.Copy(m_commandLineOptions.UpdateFolder + RewrittenFileName, OutputPath);
                    System.IO.File.Copy(m_commandLineOptions.CurrentFolder + RewrittenFileName, BackupPath);
                }

            }
            foreach (string Directory in Directory.GetDirectories(Path))
            {
                if (Path != Directory)
                {
                    IndexUpdateDirectory(Directory);
                }
            }
        }

        static void Main(string[] args)
        {
            m_commandLineOptions = new CommandLineOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, m_commandLineOptions))
            {
                Console.WriteLine("Option Summary:\n----------------------------------------------\n");
                Console.WriteLine("Current Folder: {0}", m_commandLineOptions.CurrentFolder);
                Console.WriteLine("Update Folder: {0}", m_commandLineOptions.UpdateFolder);
                Console.WriteLine("Backup Folder: {0}", m_commandLineOptions.BackupFolder);
                Console.WriteLine("Output Folder: {0}", m_commandLineOptions.OutputFolder);
                Console.WriteLine("Excluded Files/Folders: {0}", m_commandLineOptions.ExcludeFoldersContaining);
                Console.WriteLine("Skip new files: {0}\n\n", m_commandLineOptions.SkipNewFiles.ToString());

                Console.WriteLine("Begin scanning for changes? Y/N");
                if (Console.ReadLine() != "Y")
                    return;

                if (!Directory.Exists(m_commandLineOptions.CurrentFolder))
                {
                    Console.WriteLine("The specified current folder does not exist or is inacessible");
                    return;
                }

                if (!Directory.Exists(m_commandLineOptions.BackupFolder))
                {
                    Console.WriteLine("The specified backup folder does not exist or is inacessible");
                    return;
                }

                if (!Directory.Exists(m_commandLineOptions.UpdateFolder))
                {
                    Console.WriteLine("The specified update folder does not exist or is inacessible");
                    return;
                }

                if (m_commandLineOptions.CurrentFolder.ToLower().Contains(m_commandLineOptions.OutputFolder.ToLower()))
                {
                    Console.WriteLine("The output folder cannot be a subfolder of the folder containing currently deployed files. Select a different output folder before continuing.");
                    return;
                }

                if (Directory.GetFileSystemEntries(m_commandLineOptions.BackupFolder).Count() > 0)
                {
                    Console.WriteLine("The backup folder must be empty! Please empty the folder before continuing!");
                    return;
                }

                if (Directory.GetFileSystemEntries(m_commandLineOptions.OutputFolder).Count() > 0)
                {
                    Console.WriteLine("The output folder must be empty! Please empty the folder before continuing!");
                    return;
                }

                if (!Directory.Exists(m_commandLineOptions.OutputFolder))
                {
                    Console.WriteLine("The specified output folder does not exist or is inacessible");
                    return;
                }

                foreach (string folder in m_commandLineOptions.ExcludeFoldersContaining.Split(','))
                {
                    m_excludeFolders.Add(folder);
                }

                IndexCurrentDirectory(m_commandLineOptions.CurrentFolder);
                IndexUpdateDirectory(m_commandLineOptions.UpdateFolder);

            }

            Console.WriteLine("Indexing Completed");

            Console.ReadLine();
        }
    }
}
