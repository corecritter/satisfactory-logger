A simple C# Program that scans log files produced by the Satisfactory Dedicated Server and posts Login/Logout messages to a Discord Webhook

To Use:
1. Change the SatifactoryLogDirectory in appSettings.json to the directory that the dedicated server logs to
2. Changed the WebookURL in appSettings.json to the Discord webhook you wish to use
3. dotnet build
4. dotnet run

This could easily be contanerized and I may do it in the future