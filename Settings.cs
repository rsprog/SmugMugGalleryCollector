namespace SmugMugGalleryCollector
{
    public sealed class Settings
    {
        /// <summary>
        /// SmugMug API key specific to this application
        /// </summary>
        public string ApiKey { get; set; } = "";

        /// <summary>
        /// API key secret used during OAuth1.0 authentication
        /// </summary>
        public string ApiSecret { get; set; } = "";

        /// <summary>
        /// Access token specific to the account being accessed
        /// </summary>
        public string AccessToken { get; set; } = "";

        /// <summary>
        /// Access token secret used during OAuth1.0 authentication
        /// </summary>
        public string AccessTokenSecret { get; set; } = "";

        /// <summary>
        /// Account user name (nickname)
        /// </summary>
        public string UserName { get; set; } = "";

        /// <summary>
        /// Base SmugMug folder that stores actual image and video files
        /// </summary>
        public string PhotoFolder { get; set; } = "";

        /// <summary>
        /// Base SmugMug folder that contains folders for each country
        /// </summary>
        public string CountriesFolder { get; set; } = "";

        /// <summary>
        /// Base local folder that contains folders for each country
        /// </summary>
        public string LocalCountriesFolder { get; set; } = "";

        /// <summary>
        /// Custom SmugMug album settings template to use when creating city galleries
        /// </summary>
        public string AlbumTemplate { get; set; } = "";

        /// <summary>
        /// Uri for SmugMug gallery that contains cover (highlight) images to be used for countries folders (optional)
        /// </summary>
        public string? CoverGallery { get; set; } = null;

        public bool IsValid()
        {
            return !(string.IsNullOrWhiteSpace(ApiKey)
                || string.IsNullOrWhiteSpace(ApiSecret)
                || string.IsNullOrWhiteSpace(AccessToken)
                || string.IsNullOrWhiteSpace(AccessTokenSecret)
                || string.IsNullOrWhiteSpace(UserName)
                || string.IsNullOrWhiteSpace(PhotoFolder)
                || string.IsNullOrWhiteSpace(CountriesFolder)
                || string.IsNullOrWhiteSpace(LocalCountriesFolder)
                || string.IsNullOrWhiteSpace(AlbumTemplate));
        }
    }
}
