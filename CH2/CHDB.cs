using CH2.Helpers;
using CH2.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;

namespace CH2
{
    internal partial class CHDB : BaseViewModel
    {



        bool wasPlaying = false; // are we playing a video currently, or were we very recently?

        static Random random = new Random();

        DispatcherTimer timer = new DispatcherTimer();

        protected XElement Root { get; private set; }

        private XElement CHDBElement;
        private XElement InWorkElement;

        private string CHDBFilename = @"CHDB.xml";
        private string InWorkFilename = @"orphans.xml";

        private string pathToCHDB;
        private string pathToInWork;
        private string CurrentFile;

        public CHDB(string applicationPath)
        {
            pathToCHDB = Path.Combine(applicationPath, CHDBFilename);
            pathToInWork = Path.Combine(applicationPath, InWorkFilename);
      
            CHDBElement = XElement.Load(pathToCHDB);
            InWorkElement = XElement.Load(pathToInWork);

            CurrentFile = pathToCHDB;

            Root = CHDBElement ?? new XElement("CH");

            Root.Changed += Root_Changed;

            _roundModeProperty = NotifyProperty.CreateNotifyProperty(this, () => RoundMode);
            _selectedRoundProperty = NotifyProperty.CreateNotifyProperty(this, () => SelectedRound);
            _selectedVideoProperty = DerivedNotifyProperty.CreateDerivedNotifyProperty<XElement, XElement>(this, () => SelectedVideo, _selectedRoundProperty, GetVideoFromRound);
            _currentPlayerPos = NotifyProperty.CreateNotifyProperty(this, () => CurrentPlayerPosition);
            _breakModeProperty = NotifyProperty.CreateNotifyProperty(this, () => CurrentBreakMode);
            _onBreakProperty = NotifyProperty.CreateNotifyProperty(this, () => OnBreak);
            _breakProgressProperty = NotifyProperty.CreateNotifyProperty(this, () => BreakProgress);
            _breakMinimumProperty = NotifyProperty.CreateNotifyProperty(this, () => BreakMinimum);
            _breakMaximumProperty = NotifyProperty.CreateNotifyProperty(this, () => BreakMaximum);
            _breakBetweenMinLengthProperty = NotifyProperty.CreateNotifyProperty(this, () => MinBetweenBreakLength);
            _breakBetweenMaxLengthProperty = NotifyProperty.CreateNotifyProperty(this, () => MaxBetweenBreakLength);
            _breakSetMaxLengthProperty = NotifyProperty.CreateNotifyProperty(this, () => MaxSetBreakLength);
            _bothFilesExist = NotifyProperty.CreateNotifyProperty(this, () => BothFilesExist);

            BothFilesExist = File.Exists(pathToCHDB) && File.Exists(pathToInWork);

            CurrentBreakMode = BreakMode.Set;
            OnBreak = false;

            BreakMinimum = 0;
            BreakMaximum = 100;
            BreakProgress = 0;

            RoundMode = true;

            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100); // 100 milliseconds

            MinBetweenBreakLength = 1;
            MaxBetweenBreakLength = 8;
            MaxSetBreakLength = 60;

            doStartupCheck();

            if ( AllVideos.Count() == 0) // if at this point we have no videos... die horrific death?
            {
                MessageBox.Show("Could not find any videos! Exiting... check your XML files.");
                Environment.Exit(1);
            }

            // cannot access anything that needs "Exists" or "Buried" until after startup check
            SelectedRound = AllRounds.First();
        } 


        private void doStartupCheck()
        {
            IEnumerable<XElement> queryPathsNeedChecking =
                // loop on videos
                from vid in Root.Descendants("VIDEO")
                    // where they either have never been seen
                            where (string)vid.Attribute("Exists") == null
                // or the last time they were checked is unknown
                || (string)vid.Attribute("LastChecked") == null
                // or the last time they where checked was more than two days agp
                || DateTime.Now.Subtract((DateTime)vid.Attribute("LastChecked")).TotalDays > 2
                // get the filename
                select vid.Element("Filename");

            // Check for their existence.
            foreach (var file in queryPathsNeedChecking)
            {
                if (PlayerHelper.CheckFileExists(file.Value))
                {
                    file.Parent.SetAttributeValue("Exists", true);
                    file.Parent.SetAttributeValue("LastChecked", DateTime.Now);
                    Console.WriteLine(file.ToString() + " exists");
                }
                else
                {
                    file.Parent.Attributes("Exists").Remove();
                    file.Parent.Attributes("LastChecked").Remove();
                    Console.WriteLine(file.ToString() + " does not exist");
                }
            }
        }

        internal void LoadNewRoot(string selected)
        {
            XElement newRoot = null;
            Console.WriteLine("selected: {0}, current: {1}", selected, CurrentFile);
            if (selected.Equals("CHDB"))
            {
                // do nothing
                if (CurrentFile.EndsWith(InWorkFilename))
                {
                    newRoot = CHDBElement;
                    CurrentFile = pathToCHDB;
                }
            }
            else
            {
                if (CurrentFile.EndsWith(CHDBFilename))
                {
                    newRoot = InWorkElement;
                    RoundMode = false;
                    CurrentFile = pathToInWork;
                }
            }

            if (newRoot != null)
            {
                resetRoot(newRoot);
            }
        }

        private void Root_Changed(object sender, XObjectChangeEventArgs e)
        {
            Root.Save(CurrentFile);
        }

        private void resetRoot(XElement newRoot)
        {
            Console.WriteLine("Reset Root called newroot is CHDB? {0}", newRoot.Equals(CHDBElement));
            Console.WriteLine("AllRounds.Count(): {0}", AllRounds.Count());
            Console.WriteLine("AllVideos.Count(): {0}", AllVideos.Count());
            PlayerHelper.Stop();
            wasPlaying = false;
            Root.Changed -= Root_Changed;
            Root = newRoot;
            Root.Changed += Root_Changed;
            doStartupCheck();

            SelectedRound = AllRounds.First();
            RaisePropertyChanged("AllVideos");
            RaisePropertyChanged("AllRounds");
        }

        private static int everyTenTicksAfterTimerStarted = 0;
        private DateTime BreakEndsAt;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (SelectedRound == null)
            {
                PlayerHelper.Stop();
                wasPlaying = false;
                return;
            }
    
            if ( ( wasPlaying ) && ( (everyTenTicksAfterTimerStarted++ % 10) == 0) && ( CurrentPlayerPosition != 0 ) )
            {
                PlayerHelper.UpdateFullscreen();
            }

            if ( RoundMode )
            {
                if (!OnBreak)
                {
                    if  ( PlayerHelper.CurrentPosition > ((DateTime)SelectedRound.Element("EndTime")).TimeOfDay.TotalSeconds )
                    {
                        PlayerHelper.Stop();
                        // deliberately not setting wasPlaying here???
                        switch (CurrentBreakMode)
                        {
                            case BreakMode.Between:
                                {
                                    BreakMinimum = 0;
                                    BreakProgress = 0;
                                    BreakMaximum = random.Next(MinBetweenBreakLength, MaxBetweenBreakLength);
                                    break;
                                }
                            case BreakMode.Random:
                                {
                                    // shit, i dunno, pick between 1-600 seconds?
                                    BreakMinimum = 0;
                                    BreakMaximum = random.Next(1, 3600);
                                    BreakProgress = 0;
                                    break;
                                }
                            case BreakMode.Set:
                                {
                                    BreakMinimum = 0;
                                    BreakProgress = 0;
                                    BreakMaximum = MaxSetBreakLength;
                                    break;
                                }
                        }
                        //Console.WriteLine("min: {0}, max: {1}, progress: {2}", BreakMinimum, BreakMaximum, BreakProgress);
                        OnBreak = true;
                        BreakEndsAt = DateTime.Now.AddSeconds(BreakMaximum);
                    }
                }
                else
                {
                    DateTime now = DateTime.Now;
                    // see if break is over
                    if (now > BreakEndsAt)
                    {
                        OnBreak = false;
                        StartPlaybackRandom.Execute(null);
                    }
                    else
                    {
                        // how much do we have left
                        var deltaMillis = BreakEndsAt.TimeOfDay.TotalMilliseconds - now.TimeOfDay.TotalMilliseconds;

                        var progress = ( (BreakMaximum * 1000) - deltaMillis ) / 1000;
                        BreakProgress = progress;

                        //Console.WriteLine("min: {0}, max: {1}, progress: {2}", BreakMinimum, BreakMaximum, BreakProgress);
                    }
                }
            }

            CurrentPlayerPosition = PlayerHelper.CurrentPosition;
        }

        private XElement GetVideoFromRound(XElement round)
        {
            return round.Parent;
        }
                

        private bool IsValidForSelection(XElement value)
        {
            return value != null;
        }

        private void PickRound()
        {
            SelectedRound = AllRounds[random.Next(AllRounds.Count)];
        }

        private void PickVideo()
        {
            var parentVideo = AllVideos[random.Next(AllVideos.Count)];
            if (parentVideo.Elements("Round").Count() == 0)
            {
                XElement newRound = createDummyRound();
                parentVideo.Add(newRound);
            }
            SelectedRound = parentVideo.Element("Round"); // SelectedVideo is a derivative of selected round, so set it this way (stupid maybe?)
        }
        
        private void startPlaying()
        {
            OnBreak = false;
            PlayerHelper.startPlaying(SelectedRound);
            timer.Start();
            wasPlaying = true;
        }

        private XElement createDummyRound()
        {
            XElement round = new XElement("Round");
            round.SetElementValue("StartTime", "00:00:00");
            if (PlayerHelper.CurrentMediaDuration > 0)
            {
                TimeSpan span = TimeSpan.FromSeconds(PlayerHelper.CurrentMediaDuration);
                round.SetElementValue("EndTime", span.ToString(@"hh\:mm\:ss\.ff"));
            }
            else
            {
                round.SetElementValue("EndTime", "23:59:59");
            }
            round.SetElementValue("SkillRating", 1);

            return round;
        }
    }
}
