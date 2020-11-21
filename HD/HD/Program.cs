using System;
using CommandLine;
using System.Windows.Forms;
using System.IO;
using Microsoft.Xna.Framework;
using System.Security.Policy;
using System.Reflection;
using System.Diagnostics;

#if WINDOWS
namespace ParallelTasks { }
#endif

namespace HD
{
#if WINDOWS || XBOX

    class GameArguments
    {
        [Argument(ArgumentType.AtMostOnce, HelpText = "Run automatically with the first player and world.")]
        public bool autoload = false;
        [Argument(ArgumentType.AtMostOnce, HelpText = "Location of content files.")]
        public string content = null;
        [Argument(ArgumentType.AtMostOnce, HelpText = "New players will start with the Keys of the Kingdom.")]
        public bool cheat = false;
    }

    static class Program
    {
        public static GameArguments Arguments;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            // ALWAYS KEEP THIS AS THE FIRST LINE
            Utility.CreateSaveGameDirectories();

            // check for endianess for networking
            if (!BitConverter.IsLittleEndian)
                throw new Exception("Unable to run on non little endian processors.");

            WebServiceClient.GameRun();

            if (File.Exists("updater.exe"))
            {
                Utility.LogMessage("Starting auto updater process.");
                var process = Process.Start("updater.exe");
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    Utility.LogMessage("Updater found updates. Game exiting.");
                    return;
                }
                var resultString = "";
                switch ((uint)process.ExitCode)
                {
                    case 0xE0000011:
                        resultString = "No updates are available for the application.";
                        break;
                    case 0xE0000019:
                        resultString = "Updater was called with /silent, /silentall or /silentcritical command line options and automatic updates are disabled.";
                        break;
                    case 0xE000001A:
                        resultString = "Updater was called with /silent, /silentall or /silentcritical command line options and according with frequency check interval, time until next update check was not elapsed.";
                        break;
                    case 0xE0000001:
                        resultString = "PATH_EXPAND_ERROR Unable to expand a dynamic path.";
                        break;
                    case 0xE0000002:
                        resultString = "CONFIG_FILE_NOT_FOUND Unable to find the updater configuration file. It should be located near the updater.exe.";
                        break;
                    case 0xE0000003:
                        resultString = "UNDEFINED_CONFIG_FILE_FORMAT The updates configuration file format is invalid.";
                        break;
                    case 0xE0000004:
                        resultString = "UNDEFINED_FILE_VERSION The updater is unable to extract a file version.";
                        break;
                    case 0xE0000005:
                        resultString = "UNABLE_TO_SAVE_FILE The updater is unable to save a file.";
                        break;
                    case 0xE0000006:
                        resultString = "INVALID_COMMAND_LINE The command line is not recognized.";
                        break;
                    case 0xE0000007:
                        resultString = "INVALID_CLIENT_CONFIG The updater configuration file is invalid. It is required that certain entries must not be empty.";
                        break;
                    case 0xE000000E:
                        resultString = "INVALID_SERVER_CONFIG The signature of the updates configuration file is missing or is invalid.";
                        break;
                    case 0xE0000014:
                        resultString = "ERROR_INVALID_UPDATE_ENTRY The updates configuration file is invalid. It is required that certain entries must not be empty";
                        break;
                    case 0xE0000018:
                        resultString = "ERROR_URL_NOT_FOUND The updates configuration file or an update file may be missing from web server.";
                        break;
                }
                Utility.LogMessage("Updater returned: " + process.ExitCode + " " + resultString);
            }

            // see http://msdn.microsoft.com/en-us/library/bb763046.aspx for a possible partial trust security model.

            Arguments = new GameArguments();
            if (CommandLine.Parser.ParseArgumentsWithUsage(args, Arguments))
            {
                if (Arguments.content != null)
                {
                    Arguments.content = Path.GetFullPath(Arguments.content);
                    if (!Directory.Exists(Arguments.content))
                        throw new DirectoryNotFoundException("Content directory not found.");
                    Utility.ContentDirectory = Arguments.content;
                }

                using (var game = new Main())
                {
                    try
                    {
                        game.Run();
                        HD.Main.OnExit();
                    }
                    catch (Exception e)
                    {
                        // try to save?
//                        HD.Main.OnExit();

                        HandleException(e);
                        throw;
                    }
                }
            }
        }

        static void HandleException(Exception e)
        {
            try
            {
                WebServiceClient.ReportCrash(e);
            }
            catch
            {
            }

            try
            {
                Utility.LogMessage(e.ToString());
            }
            catch
            {
            }

            if (!HD.Main.IsFullScreen)
                MessageBox.Show(e.ToString());
        }
    }
#endif
}