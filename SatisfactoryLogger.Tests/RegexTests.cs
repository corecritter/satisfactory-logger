using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SatisfactoryLogger.Tests
{
    public class RegexTests
    {
        [Fact]
        public void Parse_login_username()
        {
            var pattern = @"\[(\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3})\]\[\d+\]LogNet: Join succeeded: (\w+)";
            var match = Regex.Match("[2024.10.15-19.15.28:929][962]LogNet: Join succeeded: RideOfUrLife", pattern);

            if (match.Success)
            {
                string timestamp = match.Groups[1].Value;
                var dateTime = default(DateTime);
                DateTime.TryParseExact(timestamp, "yyyy.MM.dd-HH.mm.ss:fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
                string username = match.Groups[2].Value;
            }
        }

        [Fact]
        public void Parse_login_ip()
        {
            string logMessage = "[2024.10.14-19.21.43:938][129]LogNet: NotifyAcceptingConnection accepted aggregation: 192.168.50.159:58443 (50)";
            string pattern = @"\[(\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3})\]\[\d+\]LogNet: NotifyAcceptingConnection accepted aggregation: ([\d\.]+):\d+ \(\d+\)";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(logMessage);

            if (match.Success)
            {
                string timestamp = match.Groups[1].Value;
                string ipAddress = match.Groups[2].Value;
                DateTime dateTime;
                DateTime.TryParseExact(timestamp, "yyyy.MM.dd-HH.mm.ss:fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            }
        }

        [Fact]
        public void Parse_logout_ip()
        {
            string logMessage = "[2024.10.14-19.22.37:955][706]LogNet: UChannel::CleanUp: ChIndex == 0. Closing connection. [UChannel] ChIndex: 0, Closing: 0 [UNetConnection] RemoteAddr: 192.168.50.159:59946, Name: IpConnection_2147444421, Driver: GameNetDriver FGDSIpNetDriver_2147461209, IsServer: YES, PC: BP_PlayerController_C_2147444406, Owner: BP_PlayerController_C_2147444406, UniqueId: Steam:2 (ForeignId=[Type=6 Handle=1 RepData=[7D95790601001001])";
            string pattern = @"\[(\d{4}\.\d{2}\.\d{2}-\d{2}\.\d{2}\.\d{2}:\d{3})\].*RemoteAddr: ([\d\.]+)";

            Regex regex = new Regex(pattern);
            Match match = regex.Match(logMessage);

            if (match.Success)
            {
                string timestamp = match.Groups[1].Value;
                string ipAddress = match.Groups[2].Value;
                DateTime dateTime;
                DateTime.TryParseExact(timestamp, "yyyy.MM.dd-HH.mm.ss:fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            }
        }
    }
}