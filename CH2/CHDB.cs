using CH2.Helpers;
using CH2.MVVM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using TokenizedTag;

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
        private XElement TagDBElement;

        private string CHDBFilename = @"CHDB.xml";
        private string InWorkFilename = @"orphans.xml";
        private string TagDBFilename = @"tagdb.xml";

        private string pathToCHDB;
        private string pathToInWork;
        private string pathToTagDB;
        private string CurrentFile;


        public CHDB(string applicationPath)
        {
            pathToCHDB = Path.Combine(applicationPath, CHDBFilename);
            pathToInWork = Path.Combine(applicationPath, InWorkFilename);
            pathToTagDB = Path.Combine(applicationPath, TagDBFilename);

            CHDBElement = loadOrCreateXElement(pathToCHDB, XName.Get("CH"));
            InWorkElement = loadOrCreateXElement(pathToInWork, XName.Get("CH"));
            TagDBElement = loadOrCreateXElement(pathToTagDB, XName.Get("Tags"));

            CurrentFile = pathToCHDB;

            Root = CHDBElement ?? new XElement("CH");

            CHDBElement.Changed += CHDB_Changed;
            InWorkElement.Changed += InWorkElement_Changed;
            TagDBElement.Changed += TagDBElement_Changed;

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
            _tagTreeViewSyncedToSelectedRound = NotifyProperty.CreateNotifyProperty(this, () => TagTreeViewSyncedToSelectedRound);
            _treeViewSelectedItemProperty = NotifyProperty.CreateNotifyProperty(this, () => TreeViewSelectedItem);
            _treeViewSelectedItemTagsDerivedProperty = DerivedNotifyProperty.CreateDerivedNotifyProperty<XElement, List<TokenizedTagItem>>(this, () => TreeViewSelectedTags, _treeViewSelectedItemProperty, GetTagsFromSelectedItem);

            BothFilesExist = File.Exists(pathToCHDB) && File.Exists(pathToInWork);

            TagTreeViewSyncedToSelectedRound = false;

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
                
                MessageBox.Show(String.Format("Could not find any videos! Exiting... check your XML files. path used: {0}", applicationPath));
                Environment.Exit(1);
            }

            // cannot access anything that needs "Exists" or "Buried" until after startup check
            SelectedRound = AllRounds.First();
        }

        private void InWorkElement_Changed(object sender, XObjectChangeEventArgs e)
        {
            InWorkElement.Save(pathToInWork);
        }

        private void TagDBElement_Changed(object sender, XObjectChangeEventArgs e)
        {
            TagDBElement.Save(pathToTagDB);
        }

        private XElement loadOrCreateXElement(string pathToXMLFile, XName root)
        {
            XElement returnElement = null;
            bool needCreate = false;
            try
            {
                // attempt load
                returnElement = XElement.Load(pathToXMLFile);
            }
            catch ( FileNotFoundException fnfe )
            {
                fnfe.Log();
                needCreate = true;
            }
            catch ( XmlException xe )
            {
                xe.Log();
                needCreate = true;
            }
            catch (Exception e)
            {
                // annoy the user with this exception since it was unexpected
                e.Log().Display();
                needCreate = true;
            }

            if (needCreate)
            {
                returnElement = new XElement(root);
                returnElement.Save(pathToXMLFile);
            }

            return returnElement;
        }

        private void doStartupCheck()
        {
            XElement[] elements = new XElement[] { CHDBElement, InWorkElement };
            foreach ( XElement element in elements)
            {
                IEnumerable<XElement> queryPathsNeedChecking =
                    // loop on videos
                    from vid in element.Descendants("VIDEO")
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


            // sync up tag DB
            // using "select new" to only retain tag + value
            // so tags will look the same in the union

            var TagIEqualityComparer = new FuncEqualityComparer<XElement>(
                (x1, x2) => 
                {
                    if (x1 == null)
                    {
                        return (x2 == null);
                    }
                    if (x2 == null)
                    {
                        return (x1 == null);
                    }
                    return x1.Value.Equals(x2.Value);
                }, 
                (x) => 
                {
                    if (x == null)
                    {
                        return 0;
                    }
                    return x.Value.GetHashCode();
                });

            var allTags = from tag in CHDBElement.Descendants("Tag") select new XElement(tag.Name, tag.Value);
            allTags = allTags.Union(from tag in InWorkElement.Descendants("Tag") select new XElement(tag.Name, tag.Value), TagIEqualityComparer);
            var tagsInTagDB = from tag in TagDBElement.Descendants("Tag") select new XElement(tag.Name, tag.Value);
            // allTags = allTags.Union(tagsInTagDB); is it worth doing this??
            var needToAddTags = allTags.Except(tagsInTagDB, TagIEqualityComparer);

            // since at this point we should have parentless tags, and should have only tags not already in tagDB
            // ... add them
            TagDBElement.Add(needToAddTags);
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

        internal void moveElements(string source, string dest, IList itemsToMove)
        {
            var xElems = (from tag in (from tagCat
                     in TagDBElement.Descendants("TagCategory")
                                 where tagCat.Attribute("id").Value.Equals(source)
                                 select tagCat
                     ).Descendants("Tag")
                     where itemsToMove.Contains(tag.Value)
                    select tag).ToList();

            XElement newParent = (from tagCat in TagDBElement.Descendants("TagCategory")
                                 where tagCat.Attribute("id").Value.Equals(dest)
                                 select tagCat).First();

            foreach (XElement xElem in xElems)
            {
                newParent.Add(xElem);
                xElem.Remove();
            }

            // fire all three since i am lazy
            RaisePropertyChanged("BlacklistTags");
            RaisePropertyChanged("WhitelistTags");
            RaisePropertyChanged("UncategorizedTags");

            // then all rounds and all videos since preferences may have changed
            RaisePropertyChanged("AllRounds");
            RaisePropertyChanged("AllVideos");
        }

        private void CHDB_Changed(object sender, XObjectChangeEventArgs e)
        {
            CHDBElement.Save(pathToCHDB);
        }

        private void resetRoot(XElement newRoot)
        {
            Console.WriteLine("Reset Root called newroot is CHDB? {0}", newRoot.Equals(CHDBElement));
            Console.WriteLine("AllRounds.Count(): {0}", AllRounds.Count());
            Console.WriteLine("AllVideos.Count(): {0}", AllVideos.Count());
            PlayerHelper.Stop();
            wasPlaying = false;
            Root = newRoot;

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
                        wasPlaying = false;

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

        private List<TokenizedTagItem> GetTagsFromSelectedItem(XElement selectedItem)
        {
            if (selectedItem == null)
            {
                return new List<TokenizedTagItem>();
            }
            var temp = from tags in selectedItem.Elements("Tag")
                                            select new TokenizedTagItem
                                            {
                                                Text = tags.Value
                                            };
            if (temp.Count() == 0)
            {
                return new List<TokenizedTagItem>();
            }
            return temp.ToList();
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

        internal void TagRemoved(object sender, TokenizedTagEventArgs e)
        {
            if (TreeViewSelectedItem == null || e.Item == null || e.Item.Text == null)
            {
                return;
            }

            var myTag = from tag in TreeViewSelectedItem.Elements("Tag") where string.Equals(tag.Value, e.Item.Text) select tag;
            myTag.Remove();
            // see if this tag is anywhere else
            var tagStillInCHDB = from tag in CHDBElement.Descendants("Tag") where string.Equals(tag.Value, e.Item.Text) select tag;
            var tagStillInInWork = from tag in InWorkElement.Descendants("Tag") where string.Equals(tag.Value, e.Item.Text) select tag;
            if (( tagStillInCHDB.Count() + tagStillInInWork.Count()) == 0)
            {
                var tagInTagDB = from tag in TagDBElement.Descendants("Tag") where string.Equals(tag.Value, e.Item.Text) select tag;
                tagInTagDB.Remove();
                // fire all three since i am lazy
                RaisePropertyChanged("BlacklistTags");
                RaisePropertyChanged("WhitelistTags");
                RaisePropertyChanged("UncategorizedTags");
            }

            // then all rounds and all videos since preferences may have changed
            RaisePropertyChanged("AllRounds");
            RaisePropertyChanged("AllVideos");
        }

        internal void TagApplied(object sender, TokenizedTagEventArgs e)
        {
            if (TreeViewSelectedItem == null || e.Item == null || e.Item.Text == null)
            {
                return;
            }

            var myTag = from tag in TreeViewSelectedItem.Elements("Tag") where string.Equals(tag.Value, e.Item.Text) select tag;
            if (myTag.Count() == 0)
            {
                TreeViewSelectedItem.Add(new XElement(XName.Get("Tag"), e.Item.Text));
                // then all rounds and all videos since preferences may have changed
                RaisePropertyChanged("AllRounds");
                RaisePropertyChanged("AllVideos");
            }
            // maybe add it to the tag db?
            var myTag2 = from tag in TagDBElement.Descendants("Tag") where string.Equals(tag.Value, e.Item.Text) select tag;
            if (myTag2.Count() == 0)
            {
                var uncategorizedTags = from tagCat in TagDBElement.Elements(XName.Get("TagCategory"))
                                        where tagCat.Attribute(XName.Get("id")).Value.Equals("None")
                                        select tagCat;

                uncategorizedTags.First().Add(new XElement(XName.Get("Tag"), e.Item.Text));
                RaisePropertyChanged("UncategorizedTags");
            }
        }
    }
}
