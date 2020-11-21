using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CommandLine;
using System.IO;
using HD;

namespace Server
{
    static class Program
    {
        class GameArguments
        {
            [Argument(ArgumentType.AtMostOnce, HelpText = "Location of content files.")]
            public string content = null;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Save game name.")]
            public string saveGame = "Multiplayer";

            [Argument(ArgumentType.AtMostOnce, HelpText = "Port number.")]
            public int port = 1701;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // ALWAYS KEEP THIS AS THE FIRST LINE
            Utility.CreateSaveGameDirectories();

            // check for endianess for networking
            if (!BitConverter.IsLittleEndian)
                throw new Exception("Unable to run on non little endian processors.");

            World.IsServer = true;

            var arguments = new GameArguments();
            if (CommandLine.Parser.ParseArgumentsWithUsage(args, arguments))
            {
                if (arguments.content != null)
                {
                    arguments.content = Path.GetFullPath(arguments.content);
                    if (!Directory.Exists(arguments.content))
                        throw new DirectoryNotFoundException("Content directory not found at " + arguments.content);
                    Utility.ContentDirectory = arguments.content;
                }

                World.ServerPort = arguments.port;
                MasterServer.SaveGame = arguments.saveGame;
            }

            WebServiceClient.GameRun();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            WebServiceClient.LogOff();
        }
    }
}
