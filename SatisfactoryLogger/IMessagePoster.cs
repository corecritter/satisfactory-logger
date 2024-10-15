using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SatisfactoryLogger;

public interface IMessagePoster
{
    Task PostMessage(string message);
}

public class DiscordMessagePoster : IMessagePoster
{
    private readonly AppSettings appSettings;

    public DiscordMessagePoster(AppSettings appSettings)
    {
        this.appSettings = appSettings;
    }

    public async Task PostMessage(string message)
    {
        var jsonContent = JsonConvert.SerializeObject(new MessageBody
        {
            content = message
        });
        using var client = new HttpClient();
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(this.appSettings.DiscordOptions.WebhookURL, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with {response.StatusCode}");
        }
    }
}

public class MessageBody
{
    public string content { get; set; } = null!;
}
