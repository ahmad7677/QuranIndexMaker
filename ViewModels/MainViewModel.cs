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
using System.Windows.Data;

namespace QuranIndexMaker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region FIELDS
        QuranDatabase quranDatabase;
        private ObservableCollection<Surahlar>? surahs;
        private RelayCommand startCommand;
        private RelayCommand findRootsCommand;
        private RelayCommand indexCommand;
        private List<string> indexKeys = new List<string>();
        private ObservableCollection<SurahAyahLink> surahAyahLinks;
        private ObservableCollection<SearchResult> searchResults = new ObservableCollection<SearchResult>();
        #endregion
        #region PROPS
        public ObservableCollection<Surahlar>? Surahs
        {
            get => surahs;
            set
            {
                surahs = value;
                CollectionVS = CollectionViewSource.GetDefaultView(Surahs);
            }
        }
        public ICollectionView CollectionVS { get; set; }
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
            startCommand = new RelayCommand(StartSearching);
            findRootsCommand = new RelayCommand(FindRoots);
            indexCommand = new RelayCommand(IndexWords);
            quranDatabase = new QuranDatabase();
            LoadData();
        }

        private void IndexWords()
        {
            for (int i = 0; i < searchResults.Count; i++)
            {
                string stag = searchResults.ElementAt(i).SearchTag;
                if (stag.Length > 2)
                {
                    foreach (var surah in Surahs)
                    {
                        if (surah.SurahText.Contains(stag))
                        {
                            if (searchResults.ElementAt(i).SurahAyahLinks.Count == 0)
                            {
                                searchResults.ElementAt(i).SurahAyahLinks.Add(new SurahAyahLink
                                {
                                    AyahNo = surah.AyahNo,
                                    SurahNo = surah.SurahNo
                                });
                            }
                        }
                    }
                }
            }
        }

        int root = 21;
        string tag = string.Empty;
        private void FindRoots()
        {

            for (int i = searchResults.Count - 1; i > 0; i--)
            {
                tag = searchResults.ElementAt(i).SearchTag;
                if (tag.Length == root)
                {
                    //Search the word in searchResults
                    for (int j = searchResults.Count-1; j > 0; j--)
                    {
                        if(i != j)
                        {
                            if (searchResults.ElementAt(j).SearchTag.StartsWith(tag))
                            {
                                searchResults.ElementAt(j).Remove = 1;
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
            quranDatabase.SaveChanges();
        }

        private void StartSearching()
        {
            Regex sozlar = new Regex(@"([А-Яа-яўқғҳЎҚҒҲёЁ/-]+)");
            if (surahs != null)
            {
                for (int i = 0; i < surahs.Count; i++)
                {
                    var ayahText = sozlar.Matches(surahs[i].SurahText);
                    for (int j = 0; j < ayahText.Count; j++)
                    {
                        if (!indexKeys.Contains(ayahText[j].Value))
                        {
                            indexKeys.Add(ayahText[j].Value);
                            int position = surahs[i].SurahText.IndexOf(ayahText[j].Value);
                            searchResults.Add(new SearchResult
                            {
                                SearchTag = ayahText[j].Value,
                                SurahAyahLinks = new List<SurahAyahLink>()
                            });

                            /*     
                             *     Har bir topilgan suzni butun Quron buyicha qidirib, indeksini saqlash kerak
                                {
                                    new SurahAyahLink()
                                    {
                                        AyahNo = j+1,
                                        SurahNo = i+1,
                                        StartPosition = position
                                    }
                                }*/
                        }
                    }
                }
            }
            quranDatabase.SaveChanges();
        }

        private async void LoadData()
        {
            if (quranDatabase != null && surahs == null)
            {
                await quranDatabase.Suralar.LoadAsync();
                await quranDatabase.SearchResults.LoadAsync();
                await quranDatabase.SurahAyahLinks.LoadAsync();

                Surahs = quranDatabase.Suralar.Local.ToObservableCollection();
                searchResults = quranDatabase.SearchResults.Local.ToObservableCollection();
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
        #endregion
    }
}
