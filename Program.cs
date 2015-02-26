using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageRename
{
    class Program
    {
        bool createNewFolder = false;
        string fileType = string.Empty;
        string srcFolderPath = string.Empty;
        string filePrefix = string.Empty;
        string destFolderName = string.Empty;

        public Program()
        {
            createNewFolder = true;
            fileType = "ALL";
            filePrefix = "IMG";
            srcFolderPath = Environment.CurrentDirectory;
            destFolderName = srcFolderPath;
        }

        static void Main(string[] args)
        {
            Program prog = new Program();

            if (prog.ValidateParameters(args))
            {
                prog.SetOutputFolder();

                Console.WriteLine(string.Format("\tFiles of {1} type/s in folder {2} will be {0}\t- Sorted by File Creation DateTime.{0}\t- Renamed to {3}<Counter>.{0}\t- And placed in folder {4}",
                    Environment.NewLine, prog.fileType, prog.srcFolderPath, prog.filePrefix, prog.destFolderName));
                Console.WriteLine("Do you want to Continue? (Y/N)");
                ConsoleKeyInfo key = Console.ReadKey();

                if (key.KeyChar.Equals('y') || key.KeyChar.Equals('Y'))
                {
                    string result = prog.RenameFiles();

                    Console.WriteLine(string.Format("{0}{1}", Environment.NewLine, result));
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Files will be copied in given folder location with new name
        /// </summary>
        /// <returns>Result as error messages or successful</returns>
        private string RenameFiles()
        {
            int fileCount = 0;
            string errorLog = string.Empty;
            string destFileName = string.Empty;
            string searchPattern  = fileType.Equals("ALL") ? "*.*" : string.Format("*{0}",fileType);

            List<FileInfo> files = new DirectoryInfo(srcFolderPath).GetFiles(searchPattern)
                   .OrderBy(t => t.CreationTime).ToList();

            if (files.Count.Equals(0))
            {
                return string.Format("Files with pattern \"{0}\" does not found in folder \"{1}\"", searchPattern, srcFolderPath);
            }

            // Create Destination Folder
            Directory.CreateDirectory(destFolderName);

            foreach (FileInfo file in files)
            {
                fileCount++;
                try
                {
                    destFileName = Path.Combine(destFolderName, string.Format("{0}{1}{2}", filePrefix, fileCount, file.Extension));
                    //Console.WriteLine(string.Format("Copying {0} as in {1}", file.FullName, destFileName));
                    File.Copy(file.FullName, destFileName);
                }
                catch (Exception ex)
                {
                    errorLog = string.Format("{0}{1}Error: \"{2}\" occurred while copying file {3} to {4}",
                        errorLog, Environment.NewLine, ex.Message, file.FullName, destFolderName);
                }
            }

            if (!errorLog.Equals(string.Empty))
            {
                errorLog = string.Format("Below errors occurred while copying:{0}{1}", Environment.NewLine, errorLog);
            }
            else
            {
                errorLog = string.Format("{3}Successfully copied {0} files from {1} to {2}",
                               files.Count, srcFolderPath, destFolderName, Environment.NewLine);
            }

            return errorLog;
        }

        /// <summary>
        /// Set the name of output directory
        /// </summary>
        private void SetOutputFolder()
        {
            if (createNewFolder)
            {
                if (srcFolderPath.Equals(Directory.GetDirectoryRoot(srcFolderPath)))
                {
                    destFolderName = srcFolderPath;
                }
                else
                {
                    destFolderName = Directory.GetParent(srcFolderPath).ToString();
                }

                int fileNameCnt = 0;
                string folder = Path.Combine(destFolderName, filePrefix);
                while (true)
                {
                    folder = Path.Combine(destFolderName, string.Format("{0}{1}", filePrefix, fileNameCnt));
                    fileNameCnt++;

                    if (!Directory.Exists(folder))
                    {
                        destFolderName = folder;
                        break;   
                    }
                }
            }
        }

        /// <summary>
        /// Validates the parameters
        /// </summary>
        /// <param name="args">parameters from main()</param>
        /// <returns>TRUE or FALSE to continue the program</returns>
        private bool ValidateParameters(string[] args)
        {
            for (int argIndex = 0; argIndex < args.Length; argIndex++)
            {
                switch (argIndex)
                {
                    case 0: // HELP or FILE PRIFIX
                        if ((args[argIndex] == "/?") || (args[argIndex] == "?"))
                        {
                            DisplayHelp();
                            return false;
                        }
                        else
                         {
                            filePrefix = args[argIndex];

                            string lastChar = filePrefix.Substring(filePrefix.Length - 1);

                            if (!lastChar.Equals('_'))
                            {
                                filePrefix = string.Format("{0}_", args[argIndex]);
                            }
                        }
                        break;

                    case 1: // CREATE NEW FOLDER
                        if (args[argIndex].Equals("FALSE", StringComparison.CurrentCultureIgnoreCase))
                        {
                            createNewFolder = false;
                        }
                        break;

                    case 2: // 
                        srcFolderPath = args[argIndex];
                        if (!Directory.Exists(srcFolderPath))
                        {
                            Console.WriteLine("Error: Provided Path \"" + srcFolderPath + "\" does not found on system. Please check help...");
                            DisplayHelp();
                            return false;
                        }

                        destFolderName = srcFolderPath;
                        break;

                    case 3:
                        fileType = args[argIndex];
                        break;
                    default:
                        Console.WriteLine("Provided parameter values more that required. Please check help...");
                        DisplayHelp();
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Print HELP Content
        /// </summary>
        private void DisplayHelp()
        {
            Console.WriteLine("Help to ImageRename: \r\n--------------------");
            Console.WriteLine("\t Argument 1: 'File Prefix'");
            Console.WriteLine("\t\t\tDefault Value: IMG ");
            Console.WriteLine("\t\t\t  Description: Prefix for file name.");
            Console.WriteLine("\t\t\t    Example 1: C:\\Users\\v-ansab\\Pictures>D:\\ImageRename.exe Vegas_" + Environment.NewLine);

            Console.WriteLine("\t Argument 2: 'Create New Folder'");
            Console.WriteLine("\t\t\tDefault Value: TRUE");
            Console.WriteLine("\t\t\t  Description: System will not create new folder for renamed images if provided FALSE." + Environment.NewLine);

            Console.WriteLine("\t Argument 3: 'Path'");
            Console.WriteLine("\t\t\tDefault Value: Current Folder (Program should be ran from the folder where images reside).");
            Console.WriteLine("\t\t\t  Description: Provide the path of images folder.");
            Console.WriteLine("\t\t\t    Example 1: C:\\Users\\v-ansab\\Pictures>D:\\ImageRename.exe");
            Console.WriteLine("\t\t\t    Example 2: D:\\>ImageRename.exe \"TRUE\" \"C:\\Users\\v-ansab\\Pictures\"" + Environment.NewLine);

            Console.WriteLine("\t Argument 4: 'Type of Files to Rename'");
            Console.WriteLine("\t\t\tDefault Value: ALL");
            Console.WriteLine("\t\t\t  Description: Specify Type of File by extension (\".png\" OR \".bmp\" OR \".jpeg\" OR \".mp4\" OR \".dat\")");
            Console.WriteLine("\t\t\t    Example 1: C:\\Users\\v-ansab\\Pictures>D:\\ImageRename.exe Vegas_ TRUE C:\\Users\\v-ansab\\Pictures\\ \".jpg\" " + Environment.NewLine);
        }

        private async void getUsers()
        {

        }
    }
}