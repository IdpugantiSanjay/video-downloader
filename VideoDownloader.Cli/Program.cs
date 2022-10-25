// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Humanizer;

var environmentVariableNames = new
{
    DownloadPath = "VIDEO_DOWNLOADER_DOWNLOAD_PATH"
};

var downloadPath = Environment.GetEnvironmentVariable(environmentVariableNames.DownloadPath);

if (downloadPath is null)
    throw new ArgumentException(
        $"Environment variable value {environmentVariableNames.DownloadPath} not found");

var client = new HttpClient();
var parser = new DownloadsUrlParser(client, args[0]);

var parsed = await parser.Parse();

var d = new DownloadMultipleFiles(client, new FileSystem(),
    Path.Join(downloadPath, parsed.FolderName));

SendNotification($"⏳ Downloading {parsed.FolderName}");
var stopwatch = new Stopwatch();

stopwatch.Start();

await d.Download(parsed.Urls);

stopwatch.Stop();

SendNotification(
    $"⌛ Downloaded {parsed.FolderName}. Took {stopwatch.Elapsed.Humanize()}");

void SendNotification(string message)
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

    using var process = Process.Start(
        new ProcessStartInfo
        {
            FileName = "notify-send",
            ArgumentList = { message }
        });
    process.WaitForExit();
}

public sealed class DownloadsUrlParser
{
    private readonly HttpClient _client;
    private readonly string _downloadUrl;

    public DownloadsUrlParser(HttpClient client, string downloadUrl)
    {
        _client = client;
        _downloadUrl = downloadUrl;
    }

    public async Task<(string FolderName, List<(Uri Uri, string FileName)> Urls)> Parse()
    {
        var urlsString = await _client.GetStringAsync(_downloadUrl);
        var urls = urlsString.Split("\n");

        bool IsVideo(string str) => str.EndsWith("mp4") || str.EndsWith("mkv");

        var list = urls
            .Where(IsVideo)
            .Select(x => x.Trim())
            .Select(Uri.UnescapeDataString)
            .Select(x => new Uri(x))
            .Select(x => (
                    Uri: x,
                    FileName: x.ToString().Split("/")[^1].Replace("-git.ir", "")
                        .Replace("git.ir", "")
                )
            ).ToList();

        if (list is { Count: 0 })
        {
            throw new InvalidOperationException("No video files in download-url");
        }

        var folderName = list[0].Uri.ToString().Split("/")[^2].Replace("-git.ir", "")
            .Replace("git.ir", "");

        return (folderName, list);
    }
}

public sealed class DownloadMultipleFiles
{
    private readonly HttpClient _client;
    private readonly IFileSystem _fs;
    private readonly IDirectoryInfo _directoryInfo;

    public DownloadMultipleFiles(HttpClient client, IFileSystem fileSystem,
        string downloadPath)
    {
        _client = client;
        _fs = fileSystem;

        _directoryInfo = _fs.DirectoryInfo.FromDirectoryName(downloadPath);

        if (_directoryInfo.Exists is false)
        {
            _directoryInfo.Create();
        }
    }

    public async Task Download(IEnumerable<(Uri url, string fileName)> list)
    {
        foreach (var (uri, fileName) in list)
        {
            var stream = await _client.GetStreamAsync(uri);

            await using var fileStream = _fs.File.Create(
                _fs.Path.Combine(_directoryInfo.FullName, fileName)
            );

            await stream.CopyToAsync(fileStream);
        }
    }
}