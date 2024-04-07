using Microsoft.Extensions.Configuration;
using SmugMugGalleryCollector;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var settings = config.GetRequiredSection("Settings").Get<Settings>();

if (settings is null || !settings.IsValid())
{
    Console.WriteLine("Could not obtain settings from appsettings.json");
    return;
}

var client = new SmugMugClient(settings.ApiKey, settings.ApiSecret, settings.AccessToken, settings.AccessTokenSecret);
var map = new Dictionary<string, string>();

// build a map of existing file uris and filenames on remote
foreach (var gallery in client.GetGalleries(settings.UserName, settings.PhotoFolder))
{
    Console.Write($"Getting contents of gallery {gallery.Name}...");
    var files = client.GetFiles(gallery.Uri);

    foreach (var file in files)
    {
        if (!map.ContainsKey(file.Filename))
            map[file.Filename] = file.Uri;
    }

    Console.WriteLine(" Done");
}

Console.WriteLine();

var dir = new DirectoryInfo(settings.LocalCountriesFolder);
var localGalleries = new List<DirectoryInfo>();
var countryUriMap = new Dictionary<string, string>();

foreach (var folder in dir.GetDirectories()) // iterate through local country folders
{
    var countryFolderUri = client.CreateSubfolder(settings.UserName, settings.CountriesFolder, folder.Name);
    if (countryFolderUri is null)
    {
        Console.WriteLine($"Could not create remote folder: {folder.Name}");
        continue;
    }
    countryUriMap[folder.Name] = countryFolderUri;
    Console.WriteLine($"Created country folder {folder.Name} with URI {countryFolderUri})");

    var subdir = new DirectoryInfo(folder.FullName);

    foreach (var subfolder in subdir.GetDirectories()) // iterate through local city folders
    {
        localGalleries.Add(subfolder);
    }
}

foreach (var subfolder in localGalleries.OrderBy(s=>s.CreationTimeUtc)) // iterate through local galleries in order of creation time
{
    var cityGalleryUri = client.CreateGallery(countryUriMap[subfolder.Parent!.Name], subfolder.Name, settings.AlbumTemplate);
    if (cityGalleryUri is null)
    {
        Console.WriteLine($"Could not create remote gallery: {subfolder.Name}");
        continue;
    }
    Console.WriteLine($"Created city gallery {subfolder.Name} with URI {cityGalleryUri})");

    var fileCollection = new List<string>();

    foreach (var file in subfolder.EnumerateFiles("*.*", SearchOption.AllDirectories)) // iterate through all files inside city folder (including videos)
    {
        if (file.Name == "Thumbs.db")
            continue;

        if (!map.TryGetValue(file.Name, out string? value))
        {
            // check with uppercase extension (which seems to be how some files end up stored)
            if (!map.TryGetValue(Path.GetFileNameWithoutExtension(file.Name) + Path.GetExtension(file.Name).ToUpperInvariant(), out value))
            {
                Console.WriteLine($"File not found on remote: {file.FullName}");
            }
        }

        if (value is not null)
        { 
            fileCollection.Add(value);
            Console.WriteLine($"Collecting file {file.Name} with URI {value} into gallery {subfolder.Name}");
        };
    }

    if (!client.CollectImages(cityGalleryUri, fileCollection))
        Console.WriteLine($"Could not collect images into: {subfolder.Name}");
}

