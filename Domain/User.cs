namespace Domain
{
    public class User
    {
        public string CreatedDate { get; set; }
        public string Description { get; set; }
        public int FriendsCount { get; set; }
        public bool GeoEnabled { get; set; }
        public string Identifier { get; set; }
        public string Language { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
        public string ProfileImageUrl { get; set; }
        public string UserName { get; set; }
        public object TimeZone { get; set; }
        public string Url { get; set; }
        public object UtcOffset { get; set; }
        public string Gender { get; set; }
    }
}