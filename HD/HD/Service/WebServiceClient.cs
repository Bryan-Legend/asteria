using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;

#if WINDOWS
using Newtonsoft.Json;
using System.Timers;
using System.Xaml;
#endif

namespace HD
{
    static class WebServiceClient
    {
#if DESURA || STEAM
        internal static bool IsDRMFree = true;
#else
        internal static bool IsDRMFree = false;
#endif

        internal static bool IsLoggedIn = false;


        //const string serviceAddress = "https://playasteria.com/Service";
        const string serviceAddress = "http://store.playasteria.com/Service";

        static long? sessionId;
        static DateTime loginTime;
        static LoginResult loginResult;
#if WINDOWS
        static Timer timer;
#endif

        public static bool IsDemo
        {
            get
            {
                if (loginResult != null)
                    return loginResult.IsDemo;
                return false;
            }
        }

        internal static void GameRun()
        {
#if WINDOWS
            try {
                using (var client = new WebClient()) {
                    var arguments = new NameValueCollection();
                    arguments.Add("Version", Utility.Version);

                    var bytes = client.UploadValues(serviceAddress + "/GameRun", arguments);
                    var result = Encoding.UTF8.GetString(bytes);
                    Utility.LogMessage("Session Id: " + result);
                    sessionId = JsonConvert.DeserializeObject<long>(result);
                }
            } catch (WebException exception) {
                Utility.LogMessage("Service Start: " + exception.ToString());
            }
#endif
        }

        internal static bool Login(string name, string password)
        {
#if WINDOWS
            try {
                var arguments = new NameValueCollection();
                arguments.Add("Account", name);
                arguments.Add("Password", password);
                arguments.Add("Version", Utility.Version);
                if (sessionId != null)
                    arguments.Add("SessionId", sessionId.ToString());

                using (var client = new WebClient()) {
                    var bytes = client.UploadValues(serviceAddress, arguments);
                    var result = Encoding.UTF8.GetString(bytes);
                    Utility.LogMessage("Login Server Result: " + result);
                    loginResult = JsonConvert.DeserializeObject<LoginResult>(result);
                    sessionId = loginResult.SessionId;

                    if (String.IsNullOrWhiteSpace(loginResult.ErrorMessage)) {
                        GameSettings.Default.AccountName = name;
                        GameSettings.Default.AccountPassword = password;
                        GameSettings.Default.Save();

                        if (Main.StartMenu != null)
                            Main.StartMenu.Show();
                        IsLoggedIn = true;
                        loginTime = DateTime.UtcNow;

                        timer = new Timer(10 * 60 * 1000);
                        timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                        timer.Enabled = true;
                    } else {
                        Main.ShowMessageBox(loginResult.ErrorMessage, (obj) => { Main.ReturnToPreviousMenu(); }, false);
                    }
                }
            } catch (WebException exception) {
                Utility.LogMessage("Service Login: " + exception.ToString());
                IsLoggedIn = true;

                if (Main.StartMenu != null)
                    Main.StartMenu.Show();

                //Main.ShowMessageBox(exception.Message, (obj) => { Main.ReturnToPreviousMenu(); }, false);
            }
#endif

            return IsLoggedIn;
        }

        internal static void Register(string userName, string email, string password, string passwordMatch)
        {
#if WINDOWS
            try
            {
                var arguments = new NameValueCollection();
                arguments.Add("UserName", userName);
                arguments.Add("Email", email);
                arguments.Add("Password", password);
                arguments.Add("PasswordMatch", passwordMatch);
                arguments.Add("Version", Utility.Version);
                if (sessionId != null)
                    arguments.Add("SessionId", sessionId.ToString());

                using (var client = new WebClient())
                {
                    var bytes = client.UploadValues(serviceAddress + "/Register", arguments);
                    var result = Encoding.UTF8.GetString(bytes);
                    Utility.LogMessage("Login Server Result: " + result);
                    loginResult = JsonConvert.DeserializeObject<LoginResult>(result);
                    sessionId = loginResult.SessionId;

                    if (String.IsNullOrWhiteSpace(loginResult.ErrorMessage))
                    {
                        GameSettings.Default.AccountName = userName;
                        GameSettings.Default.AccountPassword = password;
                        GameSettings.Default.Save();

                        if (Main.StartMenu != null)
                            Main.StartMenu.Show();
                        IsLoggedIn = true;
                        loginTime = DateTime.UtcNow;

                        timer = new Timer(10 * 60 * 1000);
                        timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                        timer.Enabled = true;
                    }
                    else
                    {
                        Main.ShowMessageBox(loginResult.ErrorMessage, (obj) => { Main.ReturnToPreviousMenu(); }, false);
                    }
                }
            }
            catch (WebException exception)
            {
                Utility.LogMessage("Service Login: " + exception.ToString());
                IsLoggedIn = true;

                if (Main.StartMenu != null)
                    Main.StartMenu.Show();

                //Main.ShowMessageBox(exception.Message, (obj) => { Main.ReturnToPreviousMenu(); }, false);
            }
#endif
        }

#if WINDOWS
        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try {
                // ping server
                using (var client = new WebClient()) {
                    var result = client.DownloadString(serviceAddress + "/Ping?SessionId=" + loginResult.SessionId);
                    //Utility.LogMessage("Service Pinged: " + result);

                    if (loginResult != null && loginResult.IsDemo && loginTime.AddMinutes(loginResult.DemoMinutesLeft) < DateTime.UtcNow) {
                        Main.ShowMessageBox("Sorry, but your 30 minute demo is now up.\nAsteria will save and exit.\nPlease buy the game at PlayAsteria.com. Thank you!", (obj) => { Main.Instance.Exit(); }, false);
                        Main.CurrentMenu.CloseOnEscape = false;
                    }
                }
            } catch (WebException exception) {
                Utility.LogMessage("Ping: " + exception.ToString());
            }
        }
#endif

        internal static void LogOff()
        {
#if WINDOWS
            if (timer != null) {
                timer.Dispose();
                timer = null;
            }
            if (loginResult != null) {
                try {
                    using (var client = new WebClient()) {
                        var result = client.DownloadString(serviceAddress + "/LogOff?SessionId=" + loginResult.SessionId);
                        Utility.LogMessage("Service LogOff: " + result);
                        loginResult = null;
                    }
                } catch (WebException e) {
                    Utility.LogMessage("Service LogOff: " + e.ToString());
                }
            }
#endif
        }

        internal static void SignOff()
        {
            LogOff();

            IsLoggedIn = false;
            GameSettings.Default.AccountPassword = null;
            GameSettings.Default.Save();

            if (Main.Player != null)
                Main.Player.Save();
            if (Main.Map != null)
                World.Save();
            Main.Quit();

            Main.LoginMenu.Show();
        }

        internal static void ReportCrash(Exception exception)
        {
#if WINDOWS
            var errorMessage = new StringBuilder();

            errorMessage.Append(exception.ToString());

            if (exception.InnerException != null) {
                errorMessage.Append("\n\n ***INNER EXCEPTION*** \n");
                errorMessage.Append(exception.InnerException.ToString());
            }

            errorMessage.Append("\n\n");
            foreach (var asm in World.LoadedPlugins)
                errorMessage.AppendLine(asm.FullName);

            var platform = "store.playasteria.com";            
#if STEAM
            platform = "Steam";
#endif
#if DESURA
            platform = "Desura";
#endif

            errorMessage.AppendFormat("\n\n{5} {6} {0}x{1} Fullscreen {2}, {3} FPS, God Mode {4}", Main.ResolutionWidth, Main.ResolutionHeight, Main.IsFullScreen, Main.FrameRate, Main.Player == null ? false : Main.Player.IsGod, Utility.Version, platform);

            if (Main.Player != null) {
                errorMessage.Append("\n\n ***PLAYER*** \n");
                errorMessage.Append(XamlServices.Save(Main.Player));
            }

            if (Main.Map != null) {
                errorMessage.Append("\n\n ***Map*** \n");
                errorMessage.AppendLine(Main.Map.ToString());
            }

            if (Main.GameClient != null) {
                errorMessage.Append("\n\n ***Server*** \n");
                errorMessage.AppendFormat("{0}:{1} as {2}", Main.GameClient.ServerAddress, Main.GameClient.Port, Main.GameClient.LoginName);
            }

            try {
                using (var client = new WebClient()) {
                    var arguments = new NameValueCollection();
                    if (loginResult != null)
                        arguments.Add("SessionId", loginResult.SessionId.ToString());
                    arguments.Add("report", errorMessage.ToString());
                    client.UploadValues(serviceAddress + "/ReportCrash", arguments);
                }
            } catch (WebException e) {
                Utility.LogMessage("Report Crash: " + e.ToString());
            }
#endif
        }
    }
}
