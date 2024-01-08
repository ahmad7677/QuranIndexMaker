using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public class UniqueRootWord
    {
        public int UniqueRootWordId { get; set; }
        public int DatabaseID { get; set; }
        public string RootWord { get; set; } = string.Empty;

        public List<SearchResult> SearchResults { get; set; } = new List<SearchResult>();
    }

    public class SearchResult
    {
        [Key]
        public int SearchResultId { get; set; }
        /// <summary>
        /// a word or a phrase
        /// </summary>
        //public string SearchTag { get; set; } = string.Empty;
        //public int RemoveIt { get; set; } = 0;
        public int UniqueRootWordId { get; set; }
        public int DatabaseID { get; set; }
        public int? SurahNo { get; set; }
        public int? AyahNo { get; set; }        
    }

    
    public class SurahAyahLink
    {
        [Key]        
        public int SurahAyahLinkId { get; set; }
        public int SurahNo { get; set; }
        public int AyahNo { get; set; }
        //public int StartPosition { get; set; }

        public int SearchResultId { get; set; }
        //public virtual SearchResult SearchResult { get; set; }
    }
}