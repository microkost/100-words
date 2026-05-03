using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications; //tile design library
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel;
using Windows.UI.StartScreen;

namespace words100
{
    public sealed partial class MainPage : Page
    {
        List<String> languages; //language names shown in ComboBoxes (driven by JSON)
        List<Phrase> vocabulary; //globally used vocabulary
        DispatcherTimer dispatcherTimer; //refresh values event countdown
        Double timerRefreshValueinMinutes = 120;
        bool includeAdvanced = false;
        private static Random rng = new Random();

        //permanent settings in computer
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            // Load advanced setting before loading vocabulary
            if (localSettings.Values["100wordsIncludeAdvanced"] is string advStr)
                includeAdvanced = advStr == "true";

            vocabulary = Dictionary.GetListOfWords(includeAdvanced);

            try //languages order settings from permanent storage
            {
                languages = ((string[])localSettings.Values["100wordsLanguageOrder"]).ToList();

                // Validate loaded languages - check for duplicates or missing languages
                var expectedLanguages = Dictionary.GetListOfLanguages().Select(l => l.Name).ToList();
                if (languages.Count != expectedLanguages.Count ||
                    languages.Distinct().Count() != languages.Count ||
                    !expectedLanguages.All(lang => languages.Contains(lang)))
                {
                    languages = expectedLanguages;
                    localSettings.Values["100wordsLanguageOrder"] = languages.ToArray();
                }
            }
            catch
            {
                languages = Dictionary.GetListOfLanguages().Select(l => l.Name).ToList();
            }

            this.InitializeComponent(); //gui start
            AdvancedWordsToggle.IsOn = includeAdvanced;
            RefreshVocabulary(); //shuffle & show

            //automatic timebased refresh of dictionary
            if (Double.TryParse((string)localSettings.Values["100wordsRefreshTime"], out double timerValue))
            {
                DispatcherTimerSetup(TimeSpan.FromHours(timerValue)); //(hh:mm:ss)
                UpdateTime.Text = timerValue.ToString();
                timerRefreshValueinMinutes = timerValue;
            }
            else //failure
            {
                int value = 120;
                DispatcherTimerSetup(new TimeSpan(0, value, 0)); //set time default when not saved (hh:mm:ss)
                UpdateTime.Text = value.ToString();
                timerRefreshValueinMinutes = value;
            }
        }
        internal void RefreshVocabulary()
        {
            vocabulary = Shuffle(vocabulary); //mix it
            if (vocabulary.Count == 0) //last word was removed
            {
                vocabulary = Dictionary.GetListOfWords(includeAdvanced);
                vocabulary = Shuffle(vocabulary);
            }
            MakePhraseVisible(vocabulary.First(), languages);
        }

        public void MakePhraseVisible(Phrase phrase, List<String> specifiedOrder)
        {
            var langDefs = Dictionary.GetListOfLanguages(); // code + name + flag from JSON

            List<Tuple<string, string>> phraseInOrder = new List<Tuple<string, string>>(); // (word, flagUri)
            foreach (var langName in specifiedOrder)
            {
                var def = langDefs.FirstOrDefault(l => l.Name == langName);
                if (def != null)
                    phraseInOrder.Add(new Tuple<string, string>(phrase.GetTranslation(def.Code), def.Flag));
            }

            Word0.Text = phraseInOrder[0].Item1;
            Word0Flag.Source = new BitmapImage(new Uri(phraseInOrder[0].Item2, UriKind.Absolute));
            Word1.Text = phraseInOrder[1].Item1;
            Word1Flag.Source = new BitmapImage(new Uri(phraseInOrder[1].Item2, UriKind.Absolute));
            Word2.Text = phraseInOrder[2].Item1;
            Word2Flag.Source = new BitmapImage(new Uri(phraseInOrder[2].Item2, UriKind.Absolute));
            Word3.Text = phraseInOrder[3].Item1;
            Word3Flag.Source = new BitmapImage(new Uri(phraseInOrder[3].Item2, UriKind.Absolute));

            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();
            var notification = new TileNotification(GetNotificationScheme(phraseInOrder[0].Item1, phraseInOrder[1].Item1, phraseInOrder[2].Item1, phraseInOrder[3].Item1).GetXml());
            notification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(timerRefreshValueinMinutes);
            updater.Update(notification);
        }

        private void ButtonShuffle_Click(object sender, RoutedEventArgs e)
        {
            RefreshVocabulary(); //called from gui
        }

        private void ButtonSaveSettings_Click(object sender, RoutedEventArgs e) //process gui settings
        {
            //time change
            dispatcherTimer.Stop();
            if (Double.TryParse(UpdateTime.Text, out double timerValue))
            {
                DispatcherTimerSetup(TimeSpan.FromMinutes(timerValue));
                localSettings.Values["100wordsRefreshTime"] = timerValue.ToString();
            }
            else
            {
                int value = 120;
                DispatcherTimerSetup(new TimeSpan(0, value, 0));
                UpdateTime.Text = value.ToString();
            }
            dispatcherTimer.Start();

            //advanced words toggle
            includeAdvanced = AdvancedWordsToggle.IsOn;
            localSettings.Values["100wordsIncludeAdvanced"] = includeAdvanced ? "true" : "false";
            vocabulary = Dictionary.GetListOfWords(includeAdvanced);

            //lang selection
            List<String> langOrder = new List<String>
            {
                Language1.SelectedItem.ToString(),
                Language2.SelectedItem.ToString(),
                Language3.SelectedItem.ToString(),
                Language4.SelectedItem.ToString()
            };
            languages = langOrder;

            MakePhraseVisible(vocabulary.First(), languages);
            localSettings.Values["100wordsLanguageOrder"] = languages.ToArray();

            MenuSettingsChangeVisibility();
        }

        private async void ButtonTile_Click(object sender, RoutedEventArgs e)
        {
            //https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/primary-tile-apis
            if (ApiInformation.IsTypePresent("Windows.UI.StartScreen.StartScreenManager"))
            {
                // Primary tile API's supported!

                // Get your own app list entry
                // (which is always the first app list entry assuming you are not a multi-app package)
                AppListEntry entry = (await Package.Current.GetAppListEntriesAsync())[0];                

                // Check if Start supports your app
                bool isSupported = StartScreenManager.GetDefault().SupportsAppListEntry(entry);

                // Check if your app is currently pinned
                bool isPinned = await StartScreenManager.GetDefault().ContainsAppListEntryAsync(entry);

                // And pin it to Start
                isPinned = await StartScreenManager.GetDefault().RequestAddAppListEntryAsync(entry);
            }
            else
            {
                // Older version of Windows, no primary tile API's
            }
        }

        public List<Phrase> Shuffle<Phrase>(List<Phrase> list) //mixing available dictionary to show first element
        {
            try
            {
                list.RemoveAt(0); //remove already showed word
            }
            catch
            {
                return null; //when last word was removed
            }

            int n = list.Count;
            while (n > 1) //doing mixing
            {
                n--;
                int k = rng.Next(n + 1);
                Phrase value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
        public void DispatcherTimerSetup(TimeSpan ts)
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_TimeElapsedEvent;
            dispatcherTimer.Interval = ts;
            dispatcherTimer.Start();
        }

        void DispatcherTimer_TimeElapsedEvent(object sender, object e) //countdown event method
        {
            dispatcherTimer.Stop();
            RefreshVocabulary(); //reoder vocabulary and show it again
            dispatcherTimer.Start();
        }

        private void MenuSettingsChangeVisibility() //used for opening menu view
        {
            //https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/split-view

            if (SettingsView.IsPaneOpen)
            {
                SettingsView.IsPaneOpen = false;
            }
            else
            {
                SettingsView.IsPaneOpen = true;
            }
        }

        internal TileContent GetNotificationScheme(string word0, string word1, string word2, string word3) //creating tile "XML" file
        {
            //https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/create-adaptive-tiles

            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    DisplayName = "100 finnish words",
                    Branding = TileBranding.NameAndLogo, //text should be name of dictionary

                    //TileLarge (only for desktop) //all languages showed
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = word0,
                                    HintStyle = AdaptiveTextStyle.HeaderNumeral, //super size
                                    HintWrap = true
                                },

                                new AdaptiveText()
                                {
                                    Text = word1,
                                    HintStyle = AdaptiveTextStyle.TitleSubtle, //big
                                    HintWrap = true
                                },

                                new AdaptiveText()
                                {
                                    Text = word2,
                                    HintStyle = AdaptiveTextStyle.TitleSubtle, //big
                                    HintWrap = true
                                },

                                new AdaptiveText()
                                {
                                    Text = word3,
                                    HintStyle = AdaptiveTextStyle.TitleSubtle, //big
                                    HintWrap = true
                                },
                            }
                        }
                    },

                    //TileWide
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = word0,
                                    HintStyle = AdaptiveTextStyle.HeaderNumeral //extra size
                                },

                                new AdaptiveText()
                                {
                                    Text = String.Format("{0} / {1} / {2}", word1, word2, word3),
                                    HintStyle = AdaptiveTextStyle.BodySubtle, //two words on same line, medium
                                    HintWrap = true
                                },
                            }
                        }
                    },

                    //TileMedium
                    TileMedium = new TileBinding()
                    {
                        Branding = TileBranding.Logo,
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = word0,
                                    HintStyle = AdaptiveTextStyle.TitleNumeral //big size
                                },

                                new AdaptiveText()
                                {
                                    Text = word1,
                                    HintStyle = AdaptiveTextStyle.Caption //small bold
                                },

                                new AdaptiveText()
                                {
                                    Text = word2,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle //small
                                }
                            }
                        }
                    },

                    //TileSmall - not used, cannot effectively show something
                }
            };
            return content;
        }
    }
}
