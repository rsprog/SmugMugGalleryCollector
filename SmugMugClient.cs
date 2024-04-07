using RestSharp.Authenticators;
using RestSharp;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace SmugMugGalleryCollector
{
    public class SmugMugClient
    {
        private readonly RestClient client;
        private readonly string apiKey;

        public SmugMugClient(string apiKey, string apiSecret, string accessToken, string accessTokenSecret)
        {
            this.apiKey = apiKey;

            client = new RestClient(new RestClientOptions
            {
                Authenticator = OAuth1Authenticator.ForAccessToken(apiKey, apiSecret, accessToken, accessTokenSecret, RestSharp.Authenticators.OAuth.OAuthSignatureMethod.PlainText),
                BaseUrl = new Uri(@"https://api.smugmug.com")
            });
        }

        public IEnumerable<Gallery> GetGalleries(string userName, string folderName)
        {
            var response = client.Execute(new RestRequest($"/api/v2/folder/user/{userName}/{folderName}!albumlist?APIKey={apiKey}"));

            if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content is not null)
            {
                var json = JsonSerializer.Deserialize<JsonObject>(response.Content);

                if (json is not null)
                {
                    var galleries = json["Response"]!["AlbumList"];
                    if (galleries is JsonArray)
                    {
                        foreach (var gallery in galleries.AsArray())
                        {
                            var uri = gallery!["Uri"]!.ToString();
                            var name = gallery!["Name"]!.ToString();
                            var urlPath = gallery!["UrlPath"]!.ToString();

                            if (uri is not null)
                                yield return new Gallery(uri, name, urlPath);
                        }
                    }
                }
            }
        }

        public IEnumerable<GalleryFile> GetFiles(string galleryUri)
        {
            var response = client.Execute(new RestRequest($"{galleryUri}!images?count=5000&APIKey={apiKey}"));

            if (response.StatusCode == System.Net.HttpStatusCode.OK && response.Content is not null)
            {
                var json = JsonSerializer.Deserialize<JsonObject>(response.Content);

                if (json is not null)
                {
                    var images = json["Response"]!["AlbumImage"];
                    if (images is JsonArray)
                    {
                        foreach (var image in images.AsArray())
                        {
                            var uri = image!["Uri"]!.ToString();
                            var filename = image!["FileName"]!.ToString();
                            var md5 = image!["ArchivedMD5"]!.ToString();                            
                                                        
                            if (uri is not null)
                                yield return new GalleryFile(uri, filename, md5);
                        }
                    }
                }
            }
        }

        public string? CreateSubfolder(string userName, string parentFolder, string newFolder)
        {
            var request = new RestRequest($"/api/v2/folder/user/{userName}/{parentFolder}!folders?APIKey={apiKey}");

            var body = new
            {
                Name = newFolder,
                Privacy = "Public",
                SecurityType = "GrantAccess",
                SmugSearchable = "No",
                WorldSearchable = "No",
                UrlName = newFolder.Replace(' ', '-'),
            };

            request.AddJsonBody(JsonSerializer.Serialize(body));

            var response = client.ExecutePost(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created && response.Content is not null)
            {
                var json = JsonSerializer.Deserialize<JsonObject>(response.Content);

                if (json is not null)
                {
                    return json["Response"]!["Uri"]!.ToString();
                }
            }
            return null;
        }

        // for some reason, specifying highlight image on folder creation does not work, so we will do a patch instead
        public bool AddHighlightImageToFolder(string folderUri, string highlightImageUri)
        {
            var request = new RestRequest($"{folderUri}?APIKey={apiKey}");

            var body = new
            {
                HighlightImageUri = highlightImageUri,
            };

            request.AddJsonBody(JsonSerializer.Serialize(body));

            var response = client.Execute(request, Method.Patch);

            return response.IsSuccessStatusCode;
        }

        public string? CreateGallery(string folderUri, string galleryName, string albumTemplate)
        {
            var request = new RestRequest($"{folderUri}!albumfromalbumtemplate?APIKey={apiKey}");

            var body = new
            {
                AlbumTemplateUri = albumTemplate,
                Name = galleryName,
                SecurityType = "GrantAccess",
                UrlName = galleryName.Replace(' ', '-'),
            };

            request.AddJsonBody(JsonSerializer.Serialize(body));

            var response = client.ExecutePost(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created && response.Content is not null)
            {
                var json = JsonSerializer.Deserialize<JsonObject>(response.Content);

                if (json is not null)
                {
                    return json["Response"]!["Uri"]!.ToString();
                }
            }
            return null;
        }

        public bool CollectImages(string galleryUri, IEnumerable<string> fileCollection)
        {
            var request = new RestRequest($"{galleryUri}!collectimages?APIKey={apiKey}");

            var body = new
            {
                CollectUris = string.Join(',', fileCollection)
            };

            request.AddJsonBody(JsonSerializer.Serialize(body));

            var response = client.ExecutePost(request);

            return response.IsSuccessStatusCode;
        }

        public string? GetImageUri(string albumImageUri)
        {
            var request = new RestRequest(albumImageUri);
            var response = client.Execute(request);
            if (response.IsSuccessStatusCode && response.Content is not null) {
                var json = JsonSerializer.Deserialize<JsonObject>(response.Content);
                return json!["Response"]!["AlbumImage"]!["Uris"]!["Image"]!["Uri"]!.ToString();
            }
            return null;
        }
    }
}
