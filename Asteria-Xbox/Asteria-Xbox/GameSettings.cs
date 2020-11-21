using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace HD
{
    public class GameSettings
    {
        public static GameSettings Default = new GameSettings();

        public int ResolutionWidth;
        public int ResolutionHeight;
        public bool IsFullscreen;
        public string LoginName;
        public string Password;

        public StringCollection ChatLog;

        public string Server;
        public string AccountName;
        public string AccountPassword;
        public string ServerPort;

        public float MusicVolume;
        public float SoundEffectVolume;

        public int SelectedSkin;

        internal void Save()
        {
            throw new NotImplementedException();
        }
    }
}
