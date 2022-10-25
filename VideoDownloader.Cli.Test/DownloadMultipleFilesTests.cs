using System;
using System.IO.Abstractions.TestingHelpers;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace VideoDownloader.Cli.Test;

public class DownloadMultipleFilesTests
{
    [Fact]
    public void ShouldCreateDownloadsFolderInConstructor()
    {
        var mockHttp = new MockHttpMessageHandler();
        var client = new HttpClient(mockHttp);
        var mockFileSystem = new MockFileSystem();

        mockFileSystem.AllDirectories.Should().NotContain("/Downloads");

        _ = new DownloadMultipleFiles(client, mockFileSystem, "/Downloads");
        mockFileSystem.AllDirectories.Should().Contain("/Downloads");
    }

    [Fact]
    public async Task ShouldDownloadVideos()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://hostname.com/filename.mp4")
            .Respond("application/text", "mock-binary-content");
        var client = new HttpClient(mockHttp);
        var mockFileSystem = new MockFileSystem();
        var d = new DownloadMultipleFiles(client, mockFileSystem, "/Downloads");
        await d.Download(new[] { (new Uri("https://hostname.com/filename.mp4"), "filename") });
        mockFileSystem.AllFiles.Should().Contain("/Downloads/filename");
    }
}