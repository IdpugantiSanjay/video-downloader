using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace VideoDownloader.Cli.Test;

public class DownloadsUrlParserTests
{
    [Fact]
    public async Task ShouldReturnListOfUrls()
    {
        var mockHttp = new MockHttpMessageHandler();

        mockHttp.When("https://host.com")
            .Respond("application/text",
                string.Join('\n',
                    "https://host/id/folder%20name/001-Introduction-git.ir.mp4",
                    "https://host/id/folder%20name/001-Introduction-git.ir.vtt",
                    "https://host/id/folder%20name/002-What%20is%20AWS%20Elastic%20Beanstalk-git.ir.mp4",
                    "https://host/id/folder%20name/002-What%20is%20AWS%20Elastic%20Beanstalk-git.ir.vtt"));

        var client = new HttpClient(mockHttp);
        var parser = new DownloadsUrlParser(client, "https://host.com");
        var parsed = await parser.Parse();
        parsed.FolderName.Should().Be("folder name");
        parsed.Urls.Count.Should().Be(2);
    }
    
    [Fact]
    public Task ShouldThrowIfNotVideosExist()
    {
        var mockHttp = new MockHttpMessageHandler();

        mockHttp.When("https://host.com")
            .Respond("application/text",
                string.Join('\n',
                    "https://host/id/folder%20name/002-What%20is%20AWS%20Elastic%20Beanstalk-git.ir.vtt"));

        var client = new HttpClient(mockHttp);
        var parser = new DownloadsUrlParser(client, "https://host.com");

        return Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await parser.Parse();
        });
    }
}