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
        private ObservableCollection<Quran> surahs;
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
                    AyahNumbers = (from i in surahs
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
            get => surahs;
            set
            {
                surahs = value;
                OnPropertyChanged();
                //CollectionVS = CollectionViewSource.GetDefaultView(Surahs);

                if (surahNumbers == null || surahNumbers.Count == 0)
                {
                    //Set sura numbers once
                    SurahNumbers = (from i in surahs
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
                    SelectedAyahText = surahForDetails.Where(a=>a.SuraID == surahAyahLink.SurahNo && a.VerseID == surahAyahLink.AyahNo).First();
                }
            }
        }
        public Quran SelectedAyahText { 
            get => selectedAyahText;
            set
            {
                selectedAyahText = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region COMMANDS
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
            quranDatabase = new QuranDatabase();
            AllItems.Add("All");

            LanguageList = new List<string>
            {
                "1", "42", "59", "63", "79", "120"
            };
        }

        private void IndexWords()
        {
            string message = "This will completely replace the current list";
            MessageBoxResult result = MessageBox.Show(message, "Are you sure?", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                for (int i = 0; i < searchResults.Count; i++)
                {
                    SurahAyahLinks.Clear();

                    SearchResult stag = searchResults.ElementAt(i);
                    if (stag.SearchTag.Length > 2)
                    {
                        foreach (var surah in Ayats)
                        {
                            //The condition below allows identifying words and avoid any trailing or enclosed matches like "for" in "Before" or "top" in "stop". "top" in "stop" must be avoided as it is a part of another word
                            string currenttext = surah.AyahText.ToLower();

                            if (currenttext.Contains(" " + stag.SearchTag) || //words found within text
                                currenttext.Contains(stag.SearchTag) && //words found at the start
                                currenttext.IndexOf(stag.SearchTag) == 0)
                            {
                                if (!stag.SurahAyahLinks.Where(s => s.AyahNo == surah.VerseID).Any() &&
                                    !stag.SurahAyahLinks.Where(s => s.SurahNo == surah.SuraID).Any())
                                {
                                    int position = surah.AyahText.IndexOf(stag.SearchTag);
                                    searchResults.ElementAt(i).SurahAyahLinks.Add(new SurahAyahLink
                                    {
                                        AyahNo = surah.VerseID,
                                        SurahNo = surah.SuraID,
                                        StartPosition = position
                                    });
                                }
                            }
                        }
                    }
                }
                quranDatabase.SaveChanges();
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
            }
            if (root > 5)
            {
                root--;
                FindRoots();
            }
            else
            {

            }
            await quranDatabase.SaveChangesAsync();
        }

        private async void StartSearching()
        {
            string message = "This will completely replace the current list";
            MessageBoxResult result = MessageBox.Show(message, "Are you sure?", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    searchResults.Clear();
                }));

                await Task.Run(() =>
                {
                    Regex sozlar = new Regex(@"([А-Яа-яўқғҳЎҚҒҲёЁ“” /-]+)");
                    if (surahs != null)
                    {
                        for (int i = 0; i < surahs.Count; i++)
                        {
                            var ayahText = sozlar.Matches(surahs[i].AyahText);
                            for (int j = 0; j < ayahText.Count; j++)
                            {
                                if (!indexKeys.Contains(ayahText[j].Value))
                                {
                                    indexKeys.Add(ayahText[j].Value);

                                    searchResults.Add(new SearchResult
                                    {
                                        SearchTag = ayahText[j].Value.ToLower(),
                                        RemoveIt = 0,
                                        SurahAyahLinks = new List<SurahAyahLink>()
                                    });
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
                await quranDatabase.quran.Where(a=>a.DatabaseID==SelectedLanguage).LoadAsync();
                await quranDatabase.SurahAyahLinks.LoadAsync();
                await quranDatabase.SearchResults.Include(a => a.SurahAyahLinks).LoadAsync();

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
