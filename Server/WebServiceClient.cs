using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Timers;
using Server;

namespace HD
{
    static class WebServiceClient
    {
        internal static bool IsLoggedIn = false;

        const string serviceAddress = "https://playasteria.com/Service";

        static long sessionId = 0;
        static Timer timer;
        static WebClient client;

        internal static void GameRun()
        {
            try {
                client = new WebClient();
                var arguments = new NameValueCollection();
                arguments.Add("Version", Utility.Version);
                arguments.Add("Host", Dns.GetHostName());
                arguments.Add("Port", World.ServerPort.ToString());

                var bytes = client.UploadValues(serviceAddress + "/ServerRun", arguments);
                var result = Encoding.UTF8.GetString(bytes);
                Utility.LogMessage("Session Id: " + result);
                long.TryParse(result, out sessionId);

                timer = new Timer(1 * 60 * 1000);
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Enabled = true;
            } catch (WebException exception) {
                Utility.LogMessage("Service Start: " + exception.ToString());
            }
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (sessionId != 0) {
                lock (client) {
                    var result = client.DownloadString(serviceAddress + "/ServerPing?SessionId=" + sessionId + "&CurrentPlayers=" + MasterServer.Connections.Count);
                }
            }
        }

        internal static void LogOff()
        {
            if (timer != null) {
                timer.Dispose();
                timer = null;
            }
            if (sessionId != 0) {
                var result = client.DownloadString(serviceAddress + "/LogOff?SessionId=" + sessionId);
                Utility.LogMessage("Service LogOff: " + result);
            }
        }

        internal static void ReportCrash(Exception exception)
        {
            var errorMessage = new StringBuilder();

            errorMessage.Append(exception.ToString());

            if (exception.InnerException != null) {
                errorMessage.Append("\n\n ***INNER EXCEPTION*** \n");
                errorMessage.Append(exception.InnerException.ToString());
            }

            errorMessage.AppendFormat("\n\nSERVER {0} Players: {1} Maps: {2}", Utility.Version, MasterServer.Connections.Count, World.Maps.Count);

            //if (Main.Player != null)
            //{
            //    errorMessage.Append("\n\n ***PLAYER*** \n");
            //    errorMessage.Append(XamlServices.Save(Main.Player));
            //}

            //if (Main.Map != null)
            //{
            //    errorMessage.Append("\n\n ***Map*** \n");
            //    errorMessage.AppendLine(Main.Map.ToString());
            //}

            //if (Main.GameClient != null)
            //{
            //    errorMessage.Append("\n\n ***Server*** \n");
            //    errorMessage.AppendFormat("{0}:{1} as {2}", Main.GameClient.ServerAddress, Main.GameClient.Port, Main.GameClient.LoginName);
            //}

            if (client == null)
                client = new WebClient();
            var arguments = new NameValueCollection();
            if (sessionId != 0)
                arguments.Add("SessionId", sessionId.ToString());
            arguments.Add("report", errorMessage.ToString());
            client.UploadValues(serviceAddress + "/ReportCrash", arguments);
        }
    }
}