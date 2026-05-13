using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;

namespace words100
{
    public sealed partial class MainPage : Page
    {
        List<String> languages = new List<String>(); //language names shown in ComboBoxes (driven by JSON)
        List<String> languageOptions = new List<String>(); //all options including empty for ComboBoxes
        List<Phrase> vocabulary = new List<Phrase>(); //globally used vocabulary
        DispatcherTimer? dispatcherTimer;
        bool includeAdvanced = false;
        private static Random rng = new Random();
        private const int DefaultRefreshMinutes = 5;

        //permanent settings in computer
        readonly LocalSettingsHelper localSettingsHelper = new LocalSettingsHelper("100words");
        LocalSettingsValues localSettings => localSettingsHelper.Values;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply theme here — page is fully in the visual tree so XamlRoot is ready
            string savedTheme = localSettings["100wordsTheme"] as string ?? "Default";
            ApplyTheme(savedTheme);
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
            try
            {
                await LoadPageDataAsync();
            }
            catch (Exception ex)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                EmptyPanel.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine($"Page load failed: {ex}");
            }
        }

        private async Task LoadPageDataAsync()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            contentWindow.Visibility = Visibility.Collapsed;
            EmptyPanel.Visibility = Visibility.Collapsed;

            // Unsubscribe before changing index to avoid firing during init
            ThemeSelector.SelectionChanged -= ThemeSelector_SelectionChanged;

            if (localSettings["100wordsIncludeAdvanced"] is string advStr)
                includeAdvanced = advStr == "true";

            vocabulary = await Dictionary.GetListOfWordsAsync(includeAdvanced);

            try
            {
                languages = ((string[])localSettings["100wordsLanguageOrder"]!).ToList();
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

            string savedTheme = localSettings["100wordsTheme"] as string ?? "Default";
            ThemeSelector.SelectedIndex = savedTheme switch { "Light" => 1, "Dark" => 2, _ => 0 };
            ThemeSelector.SelectionChanged += ThemeSelector_SelectionChanged;

            RefreshVocabulary();
            LoadingPanel.Visibility = Visibility.Collapsed;

            if (Double.TryParse((string?)localSettings["100wordsRefreshTime"], out double timerValue))
            {
                DispatcherTimerSetup(TimeSpan.FromMinutes(timerValue));
                UpdateTime.Text = timerValue.ToString();
            }
            else
            {
                DispatcherTimerSetup(TimeSpan.FromMinutes(DefaultRefreshMinutes));
                UpdateTime.Text = DefaultRefreshMinutes.ToString();
            }
        }

        internal async void RefreshVocabulary()
        {
            if (vocabulary == null || vocabulary.Count == 0 || languages == null || languages.Count == 0
                || languages.All(l => l == string.Empty))
            {
                contentWindow.Visibility = Visibility.Collapsed;
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


        }

        private void ButtonShuffle_Click(object sender, RoutedEventArgs e)
        {
            _ = SaveSettingsAsync();
            RefreshVocabulary();
        }

        private async void ButtonReset_Tapped(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Restore default settings",
                Content = "All saved preferences will be permanently cleared and the application will return to its initial state. This action cannot be undone.",
                PrimaryButtonText = "Restore defaults",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                return;

            localSettingsHelper.Reset();

            try
            {
                await LoadPageDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reset reload failed: {ex}");
            }
        }

        private void ShuffleAccelerator_Invoked(KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            RefreshVocabulary();
            args.Handled = true;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            bool isOpening = !MainSplitView.IsPaneOpen;
            MainSplitView.IsPaneOpen = isOpening;
            if (!isOpening)
                _ = SaveSettingsAsync();
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

        private async Task SaveSettingsAsync()
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
                DispatcherTimerSetup(TimeSpan.FromMinutes(DefaultRefreshMinutes));
                UpdateTime.Text = DefaultRefreshMinutes.ToString();
            }
            dispatcherTimer?.Start();

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
            localSettings["100wordsLanguageOrder"] = languages.ToArray();
        }

        public List<Phrase> Shuffle<Phrase>(List<Phrase> list)
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
            RefreshVocabulary();
            dispatcherTimer?.Start();
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

        private static readonly bool _isPackaged = GetCurrentPackageFullName(out _) == 0;

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int GetCurrentPackageFullName(out uint packageFullNameLength, System.Text.StringBuilder? packageFullName = null);

        private static bool IsPackaged() => _isPackaged;


    }
}
