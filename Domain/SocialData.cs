namespace Domain
{
    public class SocialData
    {
        /// <summary>
        /// Formatted as Geojson
        /// </summary>
        public object Coordinates { get; set; }
        public string CreatedDate { get; set; }
        public bool Favorited { get; set; }
        public string FavoritedCount { get; set; }
        public string Identifier { get; set; }
        public object Place { get; set; }
        public string Source { get; set; }
        public string Text { get; set; }
        public User User { get; set; }
    }
}
