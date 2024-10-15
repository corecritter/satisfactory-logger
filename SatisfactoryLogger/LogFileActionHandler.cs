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
    private LoggedInUser? lastAssignedUserName = default;

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
                existing = new LoggedInUser
                {
                    Username = logFileParserResult.Username!
                };
            }
            existing.LoginTime = logFileParserResult.TimeStamp;
            this.lastAssignedUserName = existing;
            return $"User: {existing.Username} has logged in.";
        }
        else if (logFileParserResult.Action == LogFileParserResult.Types.Action.LoginIp)
        {
            var existing = this.lastAssignedUserName;
            if (existing == default)
            {
                this.logger.LogWarning("Ip Address being added without username");
                existing = this.loggedInUsers.FirstOrDefault(_ => _.IpAddress == logFileParserResult.IpAddress);

                if (existing == default)
                {
                    existing = new LoggedInUser
                    {
                        IpAddress = logFileParserResult.IpAddress
                    };
                    this.loggedInUsers.Add(existing);
                }

                return $"User with IP: {existing.IpAddress} has logged in.";
            }
            existing.IpAddress = logFileParserResult.IpAddress;
            this.lastAssignedUserName = default;
        }
        else
        {
            var existing = this.loggedInUsers.SingleOrDefault(_ => _.IpAddress == logFileParserResult.IpAddress);

            if (existing == default)
            {
                this.logger.LogWarning($"Could not find a logged in user with IP {logFileParserResult.IpAddress}. Ignoring");
                return default;
            }

            this.loggedInUsers.Remove(existing);

            return $"User {existing.Username} is logging out after {logFileParserResult.TimeStamp - existing.LoginTime}";
        }

        return default;
    }

    private class LoggedInUser
    {
        public string Username { get; set; } = null!;
        public string? IpAddress { get; set; }
        public DateTime LoginTime { get; set; }
    }
}

