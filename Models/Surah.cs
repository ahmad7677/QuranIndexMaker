using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuranIndexMaker.Models
{
    public class Quran
    {
        public int ID { get; set; }
        public int DatabaseID { get; set; }
        public int SuraID { get; set; }
        public int VerseID { get; set; }
        public string AyahText { get; set; } = string.Empty;
    }
    //public class Suralar
    //{
    //    public int Id { get; set; }
    //    public string SurahText { get; set; } = string.Empty;
    //    public int SurahNo { get; set; }
    //    public int AyahNo { get; set; }
    //    public string Comment { get; set; } = string.Empty;
    //}

    public class SearchResult
    {
        public int SearchResultId { get; set; }
        /// <summary>
        /// a word or a phrase
        /// </summary>
        public string SearchTag { get; set; } = string.Empty;
        public int RemoveIt { get; set; }
        public ICollection<SurahAyahLink> SurahAyahLinks { get; set; } = new List<SurahAyahLink>();
    }
    public class SurahAyahLink
    {
        public int SurahAyahLinkId { get; set; }
        public int SurahNo { get; set; }
        public int AyahNo { get; set; }
        public int StartPosition { get; set; }

        public int SearchResultId { get; set; }
        public virtual SearchResult? SearchResult { get; set; }
    }
}