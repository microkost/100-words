using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI.Notifications;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel;
using Windows.UI.StartScreen;

namespace words100
{
    public sealed partial class MainPage : Page
    {
        List<String> languages = new List<String>(); //language names shown in ComboBoxes (driven by JSON)
        List<String> languageOptions = new List<String>(); //all options including empty for ComboBoxes
        List<Phrase> vocabulary = new List<Phrase>(); //globally used vocabulary
        DispatcherTimer? dispatcherTimer; //refresh values event countdown
        Double timerRefreshValueinMinutes = 120;
        bool includeAdvanced = false;
        private static Random rng = new Random();

        //permanent settings in computer
        readonly LocalSettingsHelper localSettingsHelper = new LocalSettingsHelper("100words");
        LocalSettingsValues localSettings => localSettingsHelper.Values;

        public MainPage()
        {
            this.InitializeComponent();
            this.Unloaded += MainPage_Unloaded;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= DispatcherTimer_TimeElapsedEvent;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadingPanel.Visibility = Visibility.Visible;
            contentWindow.Visibility = Visibility.Collapsed;
            EmptyPanel.Visibility = Visibility.Collapsed;
            ShuffleButton.Visibility = Visibility.Collapsed;

            // Load advanced setting before loading vocabulary
            if (localSettings["100wordsIncludeAdvanced"] is string advStr)
                includeAdvanced = advStr == "true";

            vocabulary = await Dictionary.GetListOfWordsAsync(includeAdvanced);

            try //languages order settings from permanent storage
            {
                languages = ((string[])localSettings["100wordsLanguageOrder"]!).ToList();

                // Validate loaded languages - check for invalid (non-empty) entries
                var expectedLanguages = (await Dictionary.GetListOfLanguagesAsync()).Select(l => l.Name).ToList();
                bool hasInvalidEntry = languages.Count != 4 ||
                    languages.Where(l => l != string.Empty).Distinct().Count() != languages.Where(l => l != string.Empty).Count() ||
                    languages.Any(l => l != string.Empty && !expectedLanguages.Contains(l));
                if (hasInvalidEntry)
                {
                    languages = expectedLanguages.Take(4).ToList();
                    localSettings["100wordsLanguageOrder"] = languages.ToArray();
                }
            }
            catch
            {
                languages = (await Dictionary.GetListOfLanguagesAsync()).Select(l => l.Name).ToList();
            }

            languageOptions = new List<String> { string.Empty }.Concat(
                (await Dictionary.GetListOfLanguagesAsync()).Select(l => l.Name)).ToList();

            AdvancedWordsToggle.IsOn = includeAdvanced;

            // Load theme setting - only apply if user explicitly chose Light or Dark
            string savedTheme = localSettings["100wordsTheme"] as string ?? "Default";
            ThemeSelector.SelectedIndex = savedTheme switch { "Light" => 1, "Dark" => 2, _ => 0 };
            ThemeSelector.SelectionChanged += ThemeSelector_SelectionChanged;
            if (savedTheme != "Default")
                ApplyTheme(savedTheme);

            // Load pane pin setting
            if (localSettings["100wordsPanePinned"] is string pinned && pinned == "true")
            {
                PinPaneButton.IsChecked = true;
                NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
            }

            RefreshVocabulary(); //shuffle & show
            LoadingPanel.Visibility = Visibility.Collapsed;

            //automatic timebased refresh of dictionary
            if (Double.TryParse((string?)localSettings["100wordsRefreshTime"], out double timerValue))
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
        internal async void RefreshVocabulary()
        {
            if (vocabulary == null || vocabulary.Count == 0 || languages == null || languages.Count == 0
                || languages.All(l => l == string.Empty))
            {
                contentWindow.Visibility = Visibility.Collapsed;
                ShuffleButton.Visibility = Visibility.Collapsed;
                EmptyPanel.Visibility = Visibility.Visible;
                return;
            }

            vocabulary = Shuffle(vocabulary);
            if (vocabulary.Count == 0)
            {
                vocabulary = await Dictionary.GetListOfWordsAsync(includeAdvanced);
                vocabulary = Shuffle(vocabulary);
            }

            contentWindow.Visibility = Visibility.Visible;
            ShuffleButton.Visibility = Visibility.Visible;
            EmptyPanel.Visibility = Visibility.Collapsed;
            MakePhraseVisible(vocabulary.First(), languages);
        }

        public async void MakePhraseVisible(Phrase phrase, List<String> specifiedOrder)
        {
            var langDefs = await Dictionary.GetListOfLanguagesAsync(); // code + name + flag from JSON

            // Always produce exactly 4 slots; empty name → hide word and flag
            List<Tuple<string, string?>> phraseInOrder = new List<Tuple<string, string?>>();
            foreach (var langName in specifiedOrder)
            {
                var def = langDefs.FirstOrDefault(l => l.Name == langName);
                phraseInOrder.Add(def != null
                    ? new Tuple<string, string?>(phrase.GetTranslation(def.Code), def.Flag)
                    : new Tuple<string, string?>(string.Empty, null));
            }

            Word0.Text = phraseInOrder[0].Item1;
            Word0Flag.Source = phraseInOrder[0].Item2 is string f0 ? new BitmapImage(ResolveUri(f0)) : null;
            Word1.Text = phraseInOrder[1].Item1;
            Word1Flag.Source = phraseInOrder[1].Item2 is string f1 ? new BitmapImage(ResolveUri(f1)) : null;
            Word2.Text = phraseInOrder[2].Item1;
            Word2Flag.Source = phraseInOrder[2].Item2 is string f2 ? new BitmapImage(ResolveUri(f2)) : null;
            Word3.Text = phraseInOrder[3].Item1;
            Word3Flag.Source = phraseInOrder[3].Item2 is string f3 ? new BitmapImage(ResolveUri(f3)) : null;

            if (ApiInformation.IsTypePresent("Windows.UI.Notifications.TileUpdateManager") && IsPackaged())
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueue(true);
                updater.Clear();
                var tileXml = GetNotificationXml(phraseInOrder[0].Item1, phraseInOrder[1].Item1, phraseInOrder[2].Item1, phraseInOrder[3].Item1);
                var notification = new TileNotification(tileXml);
                notification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(timerRefreshValueinMinutes);
                updater.Update(notification);
            }
        }

        private void ButtonShuffle_Click(object sender, RoutedEventArgs e)
        {
            RefreshVocabulary();
        }

        private void ButtonShuffle_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            RefreshVocabulary();
        }

        private void ShuffleAccelerator_Invoked(KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            RefreshVocabulary();
            args.Handled = true;
        }

        private void PinPaneButton_Checked(object sender, RoutedEventArgs e)
        {
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
            localSettings["100wordsPanePinned"] = "true";
        }

        private void PinPaneButton_Unchecked(object sender, RoutedEventArgs e)
        {
            NavView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
            localSettings["100wordsPanePinned"] = "false";
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                ApplyTheme(tag);
                localSettings["100wordsTheme"] = tag;
            }
        }

        private void ApplyTheme(string theme)
        {
            ElementTheme elementTheme = theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
            if (XamlRoot?.Content is FrameworkElement root)
                root.RequestedTheme = elementTheme;
        }

        private async void ButtonSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            //time change
            dispatcherTimer?.Stop();
            if (Double.TryParse(UpdateTime.Text, out double timerValue))
            {
                DispatcherTimerSetup(TimeSpan.FromMinutes(timerValue));
                localSettings["100wordsRefreshTime"] = timerValue.ToString();
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
            localSettings["100wordsIncludeAdvanced"] = includeAdvanced ? "true" : "false";
            vocabulary = await Dictionary.GetListOfWordsAsync(includeAdvanced);

            //lang selection
            List<String> langOrder = new List<String>
            {
                Language1.SelectedItem?.ToString() ?? string.Empty,
                Language2.SelectedItem?.ToString() ?? string.Empty,
                Language3.SelectedItem?.ToString() ?? string.Empty,
                Language4.SelectedItem?.ToString() ?? string.Empty
            };
            languages = langOrder;

            MakePhraseVisible(vocabulary.First(), languages);
            localSettings["100wordsLanguageOrder"] = languages.ToArray();
        }

        private async void ButtonTile_Click(object sender, RoutedEventArgs e)
        {
            //https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/primary-tile-apis
            if (ApiInformation.IsTypePresent("Windows.UI.StartScreen.StartScreenManager") && IsPackaged())
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
        }

        public List<Phrase> Shuffle<Phrase>(List<Phrase> list) //mixing available dictionary to show first element
        {
            try
            {
                list.RemoveAt(0); //remove already showed word
            }
            catch
            {
                return new List<Phrase>(); //when last word was removed, return empty list
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
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= DispatcherTimer_TimeElapsedEvent;
            }
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_TimeElapsedEvent;
            dispatcherTimer.Interval = ts;
            dispatcherTimer.Start();
        }

        void DispatcherTimer_TimeElapsedEvent(object? sender, object e) //countdown event method
        {
            dispatcherTimer?.Stop();
            RefreshVocabulary(); //reoder vocabulary and show it again
            dispatcherTimer.Start();
        }

        private static Uri ResolveUri(string uri)
        {
            if (!IsPackaged() && uri.StartsWith("ms-appx:///", StringComparison.OrdinalIgnoreCase))
            {
                var relativePath = uri.Substring("ms-appx:///".Length).Replace('/', System.IO.Path.DirectorySeparatorChar);
                var fullPath = System.IO.Path.Combine(AppContext.BaseDirectory, relativePath);
                return new Uri(fullPath, UriKind.Absolute);
            }
            return new Uri(uri, UriKind.Absolute);
        }

        private static bool IsPackaged()
        {
            try { var _ = Package.Current; return true; }
            catch { return false; }
        }

        internal Windows.Data.Xml.Dom.XmlDocument GetNotificationXml(string word0, string word1, string word2, string word3)
        {
            string xmlString = $@"
<tile>
  <visual displayName=""100 finnish words"" branding=""nameAndLogo"">
    <binding template=""TileLarge"">
      <text hint-style=""headerNumeral"" hint-wrap=""true"">{System.Security.SecurityElement.Escape(word0)}</text>
      <text hint-style=""titleSubtle"" hint-wrap=""true"">{System.Security.SecurityElement.Escape(word1)}</text>
      <text hint-style=""titleSubtle"" hint-wrap=""true"">{System.Security.SecurityElement.Escape(word2)}</text>
      <text hint-style=""titleSubtle"" hint-wrap=""true"">{System.Security.SecurityElement.Escape(word3)}</text>
    </binding>
    <binding template=""TileWide"">
      <text hint-style=""headerNumeral"">{System.Security.SecurityElement.Escape(word0)}</text>
      <text hint-style=""bodySubtle"" hint-wrap=""true"">{System.Security.SecurityElement.Escape(word1)} / {System.Security.SecurityElement.Escape(word2)} / {System.Security.SecurityElement.Escape(word3)}</text>
    </binding>
    <binding template=""TileMedium"" branding=""logo"">
      <text hint-style=""titleNumeral"">{System.Security.SecurityElement.Escape(word0)}</text>
      <text hint-style=""caption"">{System.Security.SecurityElement.Escape(word1)}</text>
      <text hint-style=""captionSubtle"">{System.Security.SecurityElement.Escape(word2)}</text>
    </binding>
  </visual>
</tile>";
            var doc = new Windows.Data.Xml.Dom.XmlDocument();
            doc.LoadXml(xmlString);
            return doc;
        }
    }
}
