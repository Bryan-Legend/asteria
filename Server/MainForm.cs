using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HD;
using System.Threading;

namespace Server
{
    public partial class MainForm : Form
    {
        Thread m_UIThread;

        public MainForm()
        {
            InitializeComponent();

            m_UIThread = Thread.CurrentThread;

            Utility.LogMessage(Utility.GameName + " Server, version " + Utility.Version);

            Text = Utility.GameName + " Server - " + World.ServerPort.ToString();
            Utility.OnLogMessage += AddLogMessage;

            World.RegisterPlugins();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            MasterServer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Utility.OnLogMessage -= AddLogMessage;

            MasterServer.Stop();
        }

        //public void AddLogMessage(string message)
        //{
        //    if (!IsDisposed)
        //    {
        //        this.Invoke(new Action(() =>
        //        {
        //            if (logTextBox.Text.Length > 10000)
        //                logTextBox.Text = "";
        //            logTextBox.AppendText(message + "\n");
        //            logTextBox.ScrollToCaret();
        //            UpdatesPerSecondTextBox.Text = MasterServer.UpdatesPerSecond.ToString();
        //        }));
        //    }
        //}

        public void AddLogMessage(string message)
        {
            if (Thread.CurrentThread != m_UIThread)
            {
                if (!IsDisposed)
                {
                    // Need for invoke if called from a different thread
                    this.Invoke((ThreadStart)delegate()
                        {
                            AddLogMessage(message);
                        });
                }
            }
            else
            {
                // add this line at the top of the log
                logListBox.Items.Insert(0, message);

                // keep only a few lines in the log
                while (logListBox.Items.Count > 1000)
                {
                    logListBox.Items.RemoveAt(logListBox.Items.Count - 1);
                }

                //UpdatesPerSecondTextBox.Text = MasterServer.UpdatesPerSecond.ToString();
            }
        }
    }
}
