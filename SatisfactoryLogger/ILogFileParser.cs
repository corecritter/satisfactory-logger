﻿using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SatisfactoryLogger;

public interface ILogFileParser
{
    public Task<List<LogFileParserResult>> ParseFile(string fileName, CancellationToken cancellationToken);
}

public class LogFileParser : ILogFileParser
{
    private readonly Dictionary<string, Func<Match, LogFileParserResult>> RegexParsers = new Dictionary<string, Func<Match, LogFileParserResult>>
    {
        {
            @"\[(\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3})\]\[\d+\]LogNet: Join succeeded: (\w+)",
            ParseLoginUserName
        },
        {
            @"\[(\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3})\]\[\d+\]LogNet: NotifyAcceptingConnection accepted aggregation: ([\d\.]+):\d+ \(\d+\)",
            ParseLoginIpAddress
        },
        {
            @"\[(\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3})\].*RemoteAddr: ([\d\.]+)",
            ParseLogoutIpAddress
        }
    };

    private const string TimeStampFormat = "yyyy.MM.dd-HH.mm.ss:fff";

    public async Task<List<LogFileParserResult>> ParseFile(string fileName, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(fileName, FileMode.Open);

        var buffer = new byte[int.MaxValue];
        var readLength = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

        var destArray = new byte[readLength];
        Array.Copy(buffer, destArray, readLength);

        var contents = UTF8Encoding.UTF8.GetString(destArray);

        return contents.Split(Environment.NewLine)
            .Select(this.ParseLine)
            .Where(_ => _ != null)
            .ToList()!;
    }

    public LogFileParserResult? ParseLine(string line)
    {
        foreach (var kvp in this.RegexParsers)
        {
            var regex = new Regex(kvp.Key);
            var match = regex.Match(line);

            if (match.Success)
            {
                return kvp.Value.Invoke(match);
            }
        }

        return default;
    }

    private static LogFileParserResult ParseLoginUserName(Match match) => new()
    {
        Action = LogFileParserResult.Types.Action.LoginUserName,
        Username = match.Groups[2].Value,
        TimeStamp = ParseDateTime(match.Groups[1].Value)
    };

    private static LogFileParserResult ParseLoginIpAddress(Match match) => new()
    {
        Action = LogFileParserResult.Types.Action.LoginIp,
        IpAddress = match.Groups[2].Value,
        TimeStamp = ParseDateTime(match.Groups[1].Value)
    };

    private static LogFileParserResult ParseLogoutIpAddress(Match match) => new()
    {
        Action = LogFileParserResult.Types.Action.Logout,
        IpAddress = match.Groups[2].Value,
        TimeStamp = ParseDateTime(match.Groups[1].Value)
    };

    private static DateTime ParseDateTime(string value)
    {
        var dateTime = default(DateTime);
        DateTime.TryParseExact(value, TimeStampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
        return dateTime;
    }
}

public class LogFileParserResult
{
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string Message { get; set; } = null!;
    public DateTime TimeStamp { get; set; }
    public Types.Action Action { get; set; }

    public static class Types
    {
        public enum Action
        {
            LoginUserName = 1,
            LoginIp = 2,
            Logout = 3
        }
    }
}