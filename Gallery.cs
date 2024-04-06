namespace SmugMugGalleryCollector
{
    public class Gallery(string uri, string name, string urlPath)
    {
        public string Uri { get; set; } = uri;
        public string Name { get; set; } = name;
        public string UrlPath { get; set; } = urlPath;
    }

    public class GalleryFile(string uri, string filename, string md5)
    {
        public string Uri { get; set; } = uri;
        public string Filename { get; set; } = filename;
        public string Md5 { get; set; } = md5;

        public override string ToString() => $"{Filename} {Uri} {Md5}";
    }
}
