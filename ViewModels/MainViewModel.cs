using Microsoft.EntityFrameworkCore;
using QuranIndexMaker.Commands;
using QuranIndexMaker.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace QuranIndexMaker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region FIELDS
        private QuranDatabase quranDatabase;
        private ObservableCollection<Quran> ayats;
        private List<Quran> surahForDetails;
        private RelayCommand startCommand;
        private RelayCommand findRootsCommand;
        private RelayCommand indexCommand;

        private List<string> indexKeys = new List<string>();
        int root = 21;
        string tag = string.Empty;
        private ObservableCollection<SurahAyahLink> surahAyahLinks;
        private ObservableCollection<SearchResult> searchResults;
        private ICollectionView colvs;
        private List<int> surahNumbers;
        private List<int> ayahNumbers;
        private int selectedSurahNumber;
        private int selectedAyahNumber;
        private string progressMessage;
        private Quran selectedAyahText;
        private SurahAyahLink surahAyahLink;
        private List<string> allItems = new List<string>();
        private int selectedLanguage;
        private RelayCommand saveCommand;
        #endregion
        #region PROPS

        public List<string> LanguageList { get; set; }
        public int SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                selectedLanguage = value;

                LoadData();
            }
        }
        public int SelectedSurahNumber
        {
            get
            {
                return selectedSurahNumber;
            }
            set
            {
                if (selectedSurahNumber != value)
                {
                    selectedSurahNumber = value;
                    SelectedAyahNumber = 0;
                    Ayats = new ObservableCollection<Quran>(quranDatabase.quran.Local.Where(a => a.SuraID == selectedSurahNumber).ToList());
                    AyahNumbers = (from i in ayats
                                   where i.SuraID == selectedSurahNumber
                                   select i.VerseID).ToList();

                }
            }
        }
        public int SelectedAyahNumber
        {
            get
            {
                return selectedAyahNumber;
            }
            set
            {
                selectedAyahNumber = value;
                if (selectedAyahNumber == 0)
                {
                    Ayats = new ObservableCollection<Quran>(quranDatabase.quran.Local.Where(a => a.SuraID == selectedSurahNumber).ToList());
                }
                else
                {
                    Ayats = new ObservableCollection<Quran>(quranDatabase.quran.Local.Where(a => a.SuraID == selectedSurahNumber && a.VerseID == selectedAyahNumber).ToList());
                }
            }
        }
        public List<int> SurahNumbers
        {
            get { return surahNumbers; }
            set
            {
                surahNumbers = value;
                OnPropertyChanged();
            }
        }
        public List<string> AllItems
        {
            get => allItems;
            set
            {
                allItems = value;
                OnPropertyChanged();
            }
        }
        public List<int> AyahNumbers
        {
            get
            {
                return ayahNumbers;
            }
            set
            {
                ayahNumbers = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<SurahAyahLink> SurahAyahLinks
        {
            get => surahAyahLinks;
            set
            {
                surahAyahLinks = value;
                CollectionSuraLinks = CollectionViewSource.GetDefaultView(SurahAyahLinks);
                CollectionSuraLinks.Refresh();
            }
        }
        public ObservableCollection<SearchResult> SearchResults
        {
            get { return searchResults; }
            set
            {
                searchResults = value;
                OnPropertyChanged();
            }
        }
        public string ProgressMessage { get => progressMessage; set => SetProperty(ref progressMessage, value); }
        public ObservableCollection<Quran> Ayats
        {
            get => ayats;
            set
            {
                ayats = value;
                OnPropertyChanged();
                //CollectionVS = CollectionViewSource.GetDefaultView(Surahs);

                if (surahNumbers == null || surahNumbers.Count == 0)
                {
                    //Set sura numbers once
                    SurahNumbers = (from i in ayats
                                    select i.SuraID).Distinct<int>().ToList();
                }
            }
        }
        public List<Quran> AyatInDetails
        {
            get => surahForDetails;
            set
            {
                surahForDetails = value;
            }
        }
        public ICollectionView CollectionVS
        {
            get => colvs;
            set
            {
                colvs = value;
                CollectionVS.Refresh();
            }
        }
        public ICollectionView CollectionSuraLinks { get; set; }
        public SurahAyahLink SurahAyahLink
        {
            get => surahAyahLink;
            set
            {
                surahAyahLink = value;
                OnPropertyChanged();
                if (surahAyahLink != null)
                {
                    SelectedAyahText = surahForDetails.Where(a => a.SuraID == surahAyahLink.SurahNo && a.VerseID == surahAyahLink.AyahNo).First();
                }
            }
        }
        public Quran SelectedAyahText
        {
            get => selectedAyahText;
            set
            {
                selectedAyahText = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region COMMANDS
        public RelayCommand SaveCommand
        {
            get => saveCommand;
            set => saveCommand = value;
        }
        public RelayCommand StartCommand
        {
            get => startCommand;
            set => startCommand = value;
        }
        public RelayCommand FindRootsCommand
        {
            get => findRootsCommand;
            set => findRootsCommand = value;
        }
        public RelayCommand IndexCommand
        {
            get => indexCommand;
            set => indexCommand = value;
        }
        #endregion

        #region METHODS
        public MainViewModel()
        {
            surahAyahLinks = new ObservableCollection<SurahAyahLink>();
            searchResults = new ObservableCollection<SearchResult>();
            startCommand = new RelayCommand(StartSearching);
            findRootsCommand = new RelayCommand(FindRoots);
            indexCommand = new RelayCommand(IndexWords);
            saveCommand = new RelayCommand(SaveChanges);
            quranDatabase = new QuranDatabase();
            AllItems.Add("All");

            LanguageList = new List<string>
            {
                "1", "42", "59", "63", "79", "120"
            };
        }

        private void SaveChanges()
        {
            try
            {
                quranDatabase.SaveChanges();
            }
            catch (Exception ex)
            {

            }
        }

        private void IndexWords()
        {
            string message = "This will completely replace the current list";
            MessageBoxResult result = MessageBox.Show(message, "Are you sure?", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                for (int i = 0; i < searchResults.Count; i++)
                {
                    //SurahAyahLinks.Clear();

                    SearchResult stag = searchResults.ElementAt(i);
                    if (stag.SearchTag.Length > 2)
                    {
                        foreach (var ayat in Ayats)
                        {
                            //The condition below allows identifying words and avoid any trailing or enclosed matches like "for" in "Before" or "top" in "stop".
                            //"top" in "stop" must be avoided as it is a part of another word
                            string currenttext = ayat.AyahText.ToLower();

                            if (currenttext.Contains(" " + stag.SearchTag) || //words found within text
                                currenttext.Contains(stag.SearchTag) && //words found at the start
                                currenttext.IndexOf(stag.SearchTag) == 0)
                            {
                                if (!stag.SurahAyahLinks.Where(s => s.AyahNo == ayat.VerseID).Any() &&
                                    !stag.SurahAyahLinks.Where(s => s.SurahNo == ayat.SuraID).Any())
                                {
                                    //int position = ayat.AyahText.IndexOf(stag.SearchTag);
                                    SearchResults.ElementAt(i).SurahAyahLinks.Add(new SurahAyahLink
                                    {
                                        AyahNo = ayat.VerseID,
                                        SurahNo = ayat.SuraID
                                    });
                                }
                            }
                        }
                    }
                }

            }
        }



        private async void FindRoots()
        {
            for (int i = searchResults.Count - 1; i > 0; i--)
            {
                tag = searchResults.ElementAt(i).SearchTag;
                if (tag.Length == root)
                {
                    //Search the word in searchResults
                    for (int j = searchResults.Count - 1; j > 0; j--)
                    {
                        if (i != j)
                        {
                            if (searchResults.ElementAt(j).SearchTag.StartsWith(tag))
                            {
                                //mark derivatives for deletion
                                searchResults.ElementAt(j).RemoveIt = 1;
                            }
                        }
                    }
                }
                break;
            }
            if (root > 5)
            {
                root--;
                FindRoots();
            }
            else
            {

            }
            //await quranDatabase.SaveChangesAsync();
        }

        private async void StartSearching()
        {
            string message = "This will completely replace the current list";
            MessageBoxResult result = MessageBox.Show(message, "Are you sure?", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                //Application.Current.Dispatcher.Invoke(new Action(() =>
                //{
                //    SearchResults.Clear();
                //}));
                string regex = @"[(),.!?:;'`“”*0-9]";
                string regex2 = @"([\s]){2,}";
                string regex3 = @"/";
                string replacement = string.Empty;
                char[] splitters = new char[] { ' ', '/' };
                await Task.Run(() =>
                {
                if (ayats != null)
                {
                    for (int i = 0; i < ayats.Count; i++)
                    {
                            ayats[i].AyahText = Regex.Replace(ayats[i].AyahText, regex, replacement);
                            ayats[i].AyahText = Regex.Replace(ayats[i].AyahText, regex2, " ");
                            ayats[i].AyahText = Regex.Replace(ayats[i].AyahText, regex3, " ");

                        //var ayahText = sozlar.Matches(ayats[i].AyahText);
                        string[] ayahText = ayats[i].AyahText.Split(splitters);
                                                        
                            for (int j = 0; j < ayahText.Length; j++)
                            {

                                if (!indexKeys.Contains(ayahText[j]))
                                {
                                    indexKeys.Add(ayahText[j]);
                                    Application.Current.Dispatcher.Invoke(new Action(() =>
                                    {
                                        SearchResults.Add(new SearchResult
                                        {
                                            SearchTag = ayahText[j].ToLower(),
                                            DatabaseID = SelectedLanguage,
                                            RemoveIt = 0,
                                            SurahAyahLinks = new List<SurahAyahLink>()
                                        });
                                    }));
                                }
                            }
                        }
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            ProgressMessage = "Search result:\n" + searchResults.Count;
                        }));
                    }
                });
                //await quranDatabase.SaveChangesAsync();
            }
        }

        private async void LoadData()
        {
            if (quranDatabase == null)
                quranDatabase = new QuranDatabase();

            //if (surahs != null && surahs.Count == 0)
            {
                await quranDatabase.quran.Where(a => a.DatabaseID == SelectedLanguage).LoadAsync();
                await quranDatabase.SurahAyahLinks.LoadAsync();
                await quranDatabase.SearchResults.Where(a => a.DatabaseID == SelectedLanguage).Include(a => a.SurahAyahLinks).LoadAsync();

                Ayats = quranDatabase.quran.Local.ToObservableCollection();
                AyatInDetails = quranDatabase.quran.Local.ToList();
                SearchResults = quranDatabase.SearchResults.Local.ToObservableCollection();
                SurahAyahLinks = quranDatabase.SurahAyahLinks.Local.ToObservableCollection();
            }
        }
        #endregion

        #region PROPERTYCHANGED
        public event PropertyChangedEventHandler? PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        #endregion
    }
}
