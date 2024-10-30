using Microsoft.Extensions.Logging;

namespace SatisfactoryLogger;

public interface ILogFileActionHandler
{
    string? HandleAction(LogFileParserResult logFileParserResult);
}

public class LogFileActionHandler : ILogFileActionHandler
{
    private readonly ILogger logger;
    private readonly List<LoggedInUser> loggedInUsers = new List<LoggedInUser>();

    public LogFileActionHandler(
        ILogger<LogFileActionHandler> logger)
    {
        this.logger = logger;
    }

    public string? HandleAction(LogFileParserResult logFileParserResult)
    {
        if (logFileParserResult.Action == LogFileParserResult.Types.Action.LoginUserName)
        {
            var existing = loggedInUsers.SingleOrDefault(_ => _.Username == logFileParserResult.Username);

            if (existing == default)
            {
                existing = loggedInUsers.FirstOrDefault(_ => _.Username == default);

                if (existing == default)
                {
                    existing = new LoggedInUser();
                    this.loggedInUsers.Add(existing);
                }
                existing.Username = logFileParserResult.Username;
                existing.LoginTime = logFileParserResult.TimeStamp;
                if (existing.IpAddress != default)
                {
                    return $"User: {existing.Username} and IP {existing.IpAddress} has logged in.";
                }
                return $"User: {existing.Username} has logged in.";
            }
        }
        else if (logFileParserResult.Action == LogFileParserResult.Types.Action.LoginIp)
        {
            var existing = this.loggedInUsers.FirstOrDefault(_ => _.IpAddress == logFileParserResult.IpAddress);
            if (existing == default)
            {
                existing = this.loggedInUsers.FirstOrDefault(_ => _.IpAddress == default);
                if (existing == default)
                {
                    existing = new LoggedInUser();
                    this.loggedInUsers.Add(existing);
                }
                existing.IpAddress = logFileParserResult.IpAddress;
                if (existing.Username != default)
                {
                    return $"User: {existing.Username} and IP {existing.IpAddress} has logged in.";
                }
                return $"User with IP {existing.IpAddress} has logged in";
            }
        }
        else
        {
            var existing = this.loggedInUsers.Where(_ => _.IpAddress == logFileParserResult.IpAddress).ToList();

            if (existing == default)
            {
                this.logger.LogWarning($"Could not find a logged in user with IP {logFileParserResult.IpAddress}. Ignoring");
                return default;
            }

            foreach (var existingUser in existing)
            {
                this.loggedInUsers.Remove(existingUser);
            }

            if (!existing.Any()) return default;

            var validExisting = existing.FirstOrDefault(_ => _.Username != default);
            if (validExisting != default)
            {
                return $"User {validExisting.Username} with IP {validExisting.IpAddress} is logging out after {logFileParserResult.TimeStamp - validExisting.LoginTime}";
            }

            return $"User with IP {existing.First().IpAddress} is logging out after {logFileParserResult.TimeStamp - existing.First().LoginTime}";
        }

        return default;
    }

    private class LoggedInUser
    {
        public string? Username { get; set; }
        public string? IpAddress { get; set; }
        public DateTime LoginTime { get; set; }
    }
}

