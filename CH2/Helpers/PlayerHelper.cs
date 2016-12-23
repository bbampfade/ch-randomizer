using AxWMPLib;
using CH2.MVVM;
using System;
using System.IO;
using System.Xml.Linq;
using WMPLib;

namespace CH2.Helpers
{
    public class PlayerHelper
    {
        private static bool WasFullscreen
        {
            get; set;
        }

        public static string SoftPath
        {
            get; set;
        }

        private static AxWindowsMediaPlayer _player;

        public static AxWindowsMediaPlayer Player
        {
            get
            {
                return _player;
            }
            internal set
            {
                _player = value; _player.PlayStateChange += Player_PlayStateChange;
            }
        }
        public static double CurrentPosition
        {
            get { return Player.Ctlcontrols.currentPosition; }
            set { Player.Ctlcontrols.currentPosition = value; }
        }

        public static double CurrentMediaDuration { get; internal set; }

        public static void startPlaying(XElement elem)
        {
            if (elem == null)
                throw new ArgumentNullException(ObjectNamingExtensions.GetName(() => elem));
            if (elem.Name.Equals(XName.Get("VIDEO")))
            {
                startPlayingVideo(elem);
            }
            else if (elem.Name.Equals(XName.Get("Round")))
            {
                startPlayingRound(elem);
            }
        }

        private static void startPlayingRound(XElement round)
        {
            double startPos = ((DateTime)round.Element("StartTime")).TimeOfDay.TotalSeconds;
            string URI = round.Parent.Element("Filename").Value;
            startPlayingURL(URI, startPos);
        }

        private static void startPlayingVideo(XElement video)
        {
            startPlayingURL(video.Element("Filename").Value, 0);
        }

        private static void startPlayingURL(string URL, double startPos)
        {
            if (!PathHelper.IsFullPath(URL))
            {
                URL = SoftPath + "\\" + URL;
            }
            Player.URL = URL;
            Player.Ctlcontrols.currentPosition = startPos;
            Player.Ctlcontrols.play();
        }

        internal static void UpdateFullscreen()
        {
            WasFullscreen = Player.fullScreen;
        }

        private static void Player_PlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
        {
            if ((WMPPlayState)e.newState == WMPPlayState.wmppsPlaying)
            {
                // re-assert fullscreen, if necessary
                Player.fullScreen = WasFullscreen;
                CurrentMediaDuration = Player.currentMedia.duration;
            }
            else if ( ( (WMPPlayState)e.newState == WMPPlayState.wmppsStopped ) ||
                      ( (WMPPlayState)e.newState == WMPPlayState.wmppsTransitioning ) )
            {
                CurrentMediaDuration = 0;
            }
        }

        internal static void Stop()
        {
            Player.Ctlcontrols.stop();
        }

        internal static void AdjustPosition(double adjustment)
        {
            Player.Ctlcontrols.currentPosition += adjustment;
        }

        internal static bool CheckFileExists(string filename_i)
        {
            string filename = "";
            if (!PathHelper.IsFullPath(filename_i))
            {
                filename = SoftPath + "\\";
            }
            filename += filename_i;
            Console.WriteLine("checking path {0}", filename);
            return File.Exists(filename);
        }
    }
}
