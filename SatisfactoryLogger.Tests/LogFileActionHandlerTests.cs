using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace SatisfactoryLogger.Tests
{
    public class LogFileActionHandlerTests
    {
        private readonly ILogFileActionHandler handler;

        public LogFileActionHandlerTests()
        {
            this.handler = new ServiceCollection()
                .AddLogging(_ =>
                {
                    _.ClearProviders();
                    _.AddConsole();
                })
                .AddSingleton<ILogFileActionHandler, LogFileActionHandler>()
                .BuildServiceProvider()
                .GetRequiredService<ILogFileActionHandler>();
        }

        [Fact]
        public void Login_with_username_returns_expected_result()
        {
            var userName = "Cooker";
            var action = new LogFileParserResult
            {
                Action = LogFileParserResult.Types.Action.LoginUserName,
                Username = userName,
                TimeStamp = new DateTime(2023, 12, 12, 12, 0, 0, DateTimeKind.Utc)
            };

            var result = this.handler.HandleAction(action);

            result.ShouldBe($"User: {userName} has logged in.");
        }

        [Fact]
        public void Login_with_ip_and_no_prior_username_returns_expected_result()
        {
            var ip = "192.168.0.1";
            var action = new LogFileParserResult
            {
                Action = LogFileParserResult.Types.Action.LoginIp,
                IpAddress = ip,
                TimeStamp = new DateTime(2023, 12, 12, 12, 0, 0, DateTimeKind.Utc)
            };

            var result = this.handler.HandleAction(action);

            result.ShouldBeNull();
        }

        [Fact]
        public void Login_with_ip_and_prior_username_returns_expected_result()
        {
            var userName = "Cooker";
            var action = new LogFileParserResult
            {
                Action = LogFileParserResult.Types.Action.LoginUserName,
                Username = userName,
                TimeStamp = new DateTime(2023, 12, 12, 12, 0, 0, DateTimeKind.Utc)
            };

            this.handler.HandleAction(action);
            var ip = "192.168.0.1";
            action = new LogFileParserResult
            {
                Action = LogFileParserResult.Types.Action.LoginIp,
                IpAddress = ip,
                TimeStamp = new DateTime(2023, 12, 12, 12, 0, 0, DateTimeKind.Utc)
            };

            var result = this.handler.HandleAction(action);

            result.ShouldBeNull();
        }

        [Fact]
        public void Logout_with_no_login_returns_expected_result()
        {
            var ip = "192.168.0.1";
            var action = new LogFileParserResult
            {
                Action = LogFileParserResult.Types.Action.Logout,
                IpAddress = ip
            };

            var result = this.handler.HandleAction(action);

            result.ShouldBeNull();
        }

        [Fact]
        public void Logout_with_prior_login_returns_expected_result()
        {
            var userName = "Cooker";
            var action = new LogFileParserResult
            {
                Action = LogFileParserResult.Types.Action.LoginUserName,
                Username = userName,
                TimeStamp = new DateTime(2023, 12, 12, 12, 0, 0, DateTimeKind.Utc)
            };
            this.handler.HandleAction(action);
            var ip = "192.168.0.1";
            action = new LogFileParserResult
            {
                Action = LogFileParserResult.Types.Action.LoginIp,
                IpAddress = ip,
                TimeStamp = new DateTime(2023, 12, 12, 12, 0, 0, DateTimeKind.Utc)
            };
            this.handler.HandleAction(action);

            action = new LogFileParserResult
            {
                Action = LogFileParserResult.Types.Action.Logout,
                IpAddress = ip,
                TimeStamp = new DateTime(2023, 12, 12, 13, 0, 0, DateTimeKind.Utc)
            };

            var result = this.handler.HandleAction(action);

            result.ShouldBe("User Cooker with IP 192.168.0.1 is logging out after 01:00:00");
        }
    }
}
