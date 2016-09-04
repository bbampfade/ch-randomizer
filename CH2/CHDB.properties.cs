using CH2.Helpers;
using CH2.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace CH2
{
    internal partial class CHDB
    {
        internal enum BreakMode
        {
            Set,
            Between,
            Random
        };

        private readonly NotifyProperty<BreakMode> _breakModeProperty;
        public BreakMode CurrentBreakMode
        {
            get { return _breakModeProperty.Value; }
            set { _breakModeProperty.SetValue(value); }
        }

        private readonly NotifyProperty<bool> _onBreakProperty;
        public bool OnBreak
        {
            get { return _onBreakProperty.Value; }
            set { _onBreakProperty.SetValue(value); }
        }

        private readonly NotifyProperty<double> _breakProgressProperty;
        public double BreakProgress
        {
            get { return _breakProgressProperty.Value; }
            set { _breakProgressProperty.SetValue(value); }
        }

        private readonly NotifyProperty<double> _breakMinimumProperty;
        public double BreakMinimum
        {
            get { return _breakMinimumProperty.Value; }
            set { _breakMinimumProperty.SetValue(value); }
        }

        private readonly NotifyProperty<double> _breakMaximumProperty;
        public double BreakMaximum
        {
            get { return _breakMaximumProperty.Value; }
            set { _breakMaximumProperty.SetValue(value); }
        }

        string preferredTimeFormat = @"hh\:mm\:ss\.ff";

        private readonly NotifyProperty<bool> _roundModeProperty;
        public bool RoundMode
        {
            get { return _roundModeProperty.Value; }
            set { _roundModeProperty.SetValue(value); }
        }

        private readonly NotifyProperty<int> _breakBetweenMaxLengthProperty;
        public int MaxBetweenBreakLength
        {
            get { return _breakBetweenMaxLengthProperty.Value; }
            set { _breakBetweenMaxLengthProperty.SetValue(value); }
        }

        private readonly NotifyProperty<int> _breakSetMaxLengthProperty;
        public int MaxSetBreakLength
        {
            get { return _breakSetMaxLengthProperty.Value; }
            set { _breakSetMaxLengthProperty.SetValue(value); }
        }

        private readonly NotifyProperty<int> _breakBetweenMinLengthProperty;
        public int MinBetweenBreakLength
        {
            get { return _breakBetweenMinLengthProperty.Value; }
            set { _breakBetweenMinLengthProperty.SetValue(value); }
        }

        
        private readonly NotifyProperty<double> _currentPlayerPos;

        public double CurrentPlayerPosition
        {
            get { return _currentPlayerPos.Value; }
            set { _currentPlayerPos.SetValue(value); }
        }

        public string Name
        {
            get
            {
                return Root.Name.NamespaceName;
            }
        }

        public string rawXML
        {
            get
            {
                return Root.ToString();
            }
        }

        private readonly NotifyProperty<bool> _bothFilesExist;
        public bool BothFilesExist
        {
            get { return _bothFilesExist.Value; }
            set { _bothFilesExist.SetValue(value); }
        }

        private readonly NotifyProperty<XElement> _selectedRoundProperty;

        public XElement SelectedRound
        {
            get { return _selectedRoundProperty.Value; }
            set
            {
                if (IsValidForSelection(value))
                {
                    var previousRound = _selectedRoundProperty.Value;
                    _selectedRoundProperty.SetValue(value);
                    if (wasPlaying)
                    {
                        if (( RoundMode ) ||
                            (!value.Parent.Equals(previousRound.Parent)))
                        {
                            startPlaying();
                        }
                    }
                }
            }
        }

        private readonly DerivedNotifyProperty<XElement> _selectedVideoProperty;
        public XElement SelectedVideo
        {
            get { return _selectedVideoProperty.Value; }
            set
            {
                if (value != null)
                {
                    var round = value.Element("Round") ?? createDummyRound();
                    SelectedRound = round;
                }
            }
        }

        public ObservableCollection<XElement> AllRounds
        {
            get
            {
                return new ObservableCollection<XElement>(
                    Root.Descendants("Round").
                    // from the set of videos that exist
                    Where(round => round.Parent.Attribute("Exists") != null
                    // and rounds that aren't buried
                    && round.Attribute("Buried") == null).
                    // get round
                    Select(round => round));
            }
            // do i want to implement this?
            private set
            {
                Console.WriteLine("AllRounds.set called with {0}", value);
            }
        }

        public ObservableCollection<XElement> AllVideos
        {
            get
            {
                     return new ObservableCollection<XElement>(Root.Descendants("VIDEO").
                                 Where(vid => vid.Attribute("Exists") != null &&
                                 ((vid.Descendants().Attributes("Buried").Count() != vid.Elements("Round").Count()) // make sure it has unburied rounds
                                 || (vid.Elements("Round").Count() == 0)) // or no rounds 
                                 ).Select(vid => vid));
            }

            private set
            {
                // do nothing 
                Console.WriteLine("AllVideos.set called with {0}", value);
            }
        }


        private DelegateCommand _startButtonPressed;

        public DelegateCommand StartButtonPressed
        {
            get
            {
                return _startButtonPressed ?? (_startButtonPressed = new DelegateCommand(
                    () =>
                    {
                        if (SelectedRound != null)
                        {
                            TimeSpan currentTime = TimeSpan.FromSeconds(PlayerHelper.CurrentPosition);
                            SelectedRound.SetElementValue("StartTime", currentTime.ToString(preferredTimeFormat));
                        }
                    }));
            }
        }

        private DelegateCommand _endButtonPressed;

        public DelegateCommand EndButtonPressed
        {
            get
            {
                return _endButtonPressed ?? (_endButtonPressed = new DelegateCommand(
                    () =>
                    {
                        if (SelectedRound != null)
                        {
                            TimeSpan currentTime = TimeSpan.FromSeconds(PlayerHelper.CurrentPosition);
                            SelectedRound.SetElementValue("EndTime", currentTime.ToString(preferredTimeFormat));

                            if (RoundMode)
                            {
                                // timer should take care of this case
                            }
                            else
                            {
                                if (PlayerHelper.CurrentPosition < PlayerHelper.CurrentMediaDuration)
                                {
                                    XElement nextRound = createDummyRound();
                                    if (SelectedRound.ElementsAfterSelf().Count() > 0)
                                    {
                                        SelectedRound = SelectedRound.ElementsAfterSelf().First();
                                    }
                                    else
                                    {
                                        nextRound.SetElementValue("StartTime", currentTime.ToString(preferredTimeFormat));
                                        SelectedVideo.Add(nextRound);
                                        SelectedRound = nextRound;
                                    }
                                }
                            }
                        }
                    }));
            }
        }

        private DelegateCommand _back15ButtonPressed;

        public DelegateCommand Back15ButtonPressed
        {
            get
            {
                return _back15ButtonPressed ?? (_back15ButtonPressed = new DelegateCommand(
                    () =>
                    {
                        PlayerHelper.AdjustPosition(-15);
                    }));
            }
        }

        private DelegateCommand _back5ButtonPressed;

        public DelegateCommand Back5ButtonPressed
        {
            get
            {
                return _back5ButtonPressed ?? (_back5ButtonPressed = new DelegateCommand(
                    () =>
                    {
                        PlayerHelper.AdjustPosition(-5);
                    }));
            }
        }

        private DelegateCommand _forward15ButtonPressed;

        public DelegateCommand Forward15ButtonPressed
        {
            get
            {
                return _forward15ButtonPressed ?? (_forward15ButtonPressed = new DelegateCommand(
                    () =>
                    {
                        PlayerHelper.AdjustPosition(15);
                    }));
            }
        }

        private DelegateCommand _forward5ButtonPressed;

        public DelegateCommand Forward5ButtonPressed
        {
            get
            {
                return _forward5ButtonPressed ?? (_forward5ButtonPressed = new DelegateCommand(
                    () =>
                    {
                        PlayerHelper.AdjustPosition(5);
                    }));
            }
        }

        private DelegateCommand _resetRoundButtonPressed;

        public DelegateCommand ResetRoundButtonPressed
        {
            get
            {
                return _resetRoundButtonPressed ?? (_resetRoundButtonPressed = new DelegateCommand(
                    () =>
                    {
                        PlayerHelper.CurrentPosition = ((DateTime)SelectedRound.Element("StartTime")).TimeOfDay.TotalSeconds;
                    }));
            }
        }

        private DelegateCommand _resetVideoButtonPressed;

        public DelegateCommand ResetVideoButtonPressed
        {
            get
            {
                return _resetVideoButtonPressed ?? (_resetVideoButtonPressed = new DelegateCommand(
                    () =>
                    {
                        if (RoundMode)
                        {
                            var round = SelectedVideo.Element("Round");
                            PlayerHelper.CurrentPosition = ((DateTime)round.Element("StartTime")).TimeOfDay.TotalSeconds;
                        }
                        else
                        {
                            PlayerHelper.CurrentPosition = 0;
                        }
                    }));
            }
        }

        private DelegateCommand _endRoundButtonPressed;

        public DelegateCommand EndRoundButtonPressed
        {
            get
            {
                return _endRoundButtonPressed ?? (_endRoundButtonPressed = new DelegateCommand(
                    () =>
                    {
                        PlayerHelper.CurrentPosition = ((DateTime)SelectedRound.Element("EndTime")).TimeOfDay.TotalSeconds - 1;
                    }));
            }
        }

        private DelegateCommand _endVideoButtonPressed;

        public DelegateCommand EndVideoButtonPressed
        {
            get
            {
                return _endVideoButtonPressed ?? (_endVideoButtonPressed = new DelegateCommand(
                    () =>
                    {
                        PlayerHelper.CurrentPosition = PlayerHelper.CurrentMediaDuration - 1;

                    }));
            }
        }

        private DelegateCommand _deleteRoundButtonPressed;

        public DelegateCommand DeleteRoundButtonPressed
        {
            get
            {
                return _deleteRoundButtonPressed ?? (_deleteRoundButtonPressed = new DelegateCommand(
                    () =>
                    {
                        var closestSibling = (SelectedRound.ElementsBeforeSelf() != null) ? SelectedRound.ElementsBeforeSelf().Last() : (SelectedRound.ElementsAfterSelf() != null) ? SelectedRound.ElementsAfterSelf().First() : null;
                        var newRound = (closestSibling == null) ? createDummyRound() : closestSibling;

                        if (closestSibling == null)
                        {
                            SelectedVideo.Add(newRound);
                        }
                        SelectedRound.Remove();
                        SelectedRound = newRound;
                    }));
            }
        }

        private DelegateCommand _addRoundButtonPressed;

        public DelegateCommand AddRoundButtonPressed
        {
            get
            {
                return _addRoundButtonPressed ?? (_addRoundButtonPressed = new DelegateCommand(
                    () =>
                    {
                        var newRound = createDummyRound();
                        SelectedVideo.Add(newRound);
                    }));
            }
        }

        private DelegateCommand _startPlaybackRandom = null;

        public DelegateCommand StartPlaybackRandom
        {
            get
            {
                return _startPlaybackRandom ?? (_startPlaybackRandom = new DelegateCommand(
                  () =>
                  {
                      if (RoundMode)
                      {
                          PickRound();
                      }
                      else
                      {
                          PickVideo();
                      }

                      startPlaying();
                  }
                  ));
            }
        }

        private DelegateCommand _buryRound = null;

        public DelegateCommand BuryRoundCommand
        {
            get
            {
                return _buryRound ?? (_buryRound = new DelegateCommand(
                    () =>
                    {
                        SelectedRound.SetAttributeValue("Buried", "true");
                        if (wasPlaying && StartPlaybackRandom.CanExecute(null))
                        {
                            StartPlaybackRandom.Execute(null);
                        }
                    }));
            }
        }

        private DelegateCommand _buryVideo = null;

        public DelegateCommand BuryVideoCommand
        {
            get
            {
                return _buryVideo ?? (_buryVideo = new DelegateCommand(
                    () =>
                    {
                        foreach (XElement round in SelectedVideo.Elements("Round"))
                        {
                            round.SetAttributeValue("Buried", "true");
                        }
                        if (wasPlaying && StartPlaybackRandom.CanExecute(null))
                        {
                            StartPlaybackRandom.Execute(null);
                        }
                        RaisePropertyChanged("AllVideos");
                        RaisePropertyChanged("AllRounds");
                    }));
            }
        }

        private DelegateCommand _restoreBuried = null;

        public DelegateCommand RestoreBuriedCommand
        {
            get
            {
                return _restoreBuried ?? (_restoreBuried = new DelegateCommand(
                    () =>
                    {
                        Root.Descendants().Attributes("Buried").Remove();
                        RaisePropertyChanged("AllVideos");
                        RaisePropertyChanged("AllRounds");
                    }));
            }
        }
    }
}
