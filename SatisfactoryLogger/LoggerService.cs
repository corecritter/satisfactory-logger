using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryLogger;

public class LoggerService
{
    private readonly IFileChangedWatcher fileChangeSniffer;
    private readonly ILogFileParser logFileParser;
    private readonly ILogFileActionHandler logFileActionHandler;
    private readonly IMessagePoster messagePoster;
    private readonly AppSettings appSettings;
    private readonly ILogger logger;

    public LoggerService(
        IFileChangedWatcher fileChangeSniffer,
        ILogFileParser logFileParser,
        ILogFileActionHandler logFileActionHandler,
        IMessagePoster messagePoster,
        AppSettings appSettings,
        ILogger<LoggerService> logger)
    {
        this.fileChangeSniffer = fileChangeSniffer;
        this.logFileParser = logFileParser;
        this.logFileActionHandler = logFileActionHandler;
        this.messagePoster = messagePoster;
        this.appSettings = appSettings;
        this.logger = logger;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Staring {nameof(LoggerService)}");
        var fileSnifferTask = this.fileChangeSniffer.Start(cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            var changedFile = await this.fileChangeSniffer.WaitForFileChange(cancellationToken);
            this.logger.LogInformation($"Parsing file {changedFile}");
            var actions = await this.logFileParser.ParseFile(changedFile, DateTime.UtcNow, this.appSettings.FileOptions.MaxLogAge, cancellationToken);
            foreach(var action in actions)
            {
                this.logger.LogInformation($"Handling action for message: {action.Message}");
                var postMessage = this.logFileActionHandler.HandleAction(action);

                if (postMessage != null)
                {
                    try
                    {
                        this.logger.LogInformation($"Posting message: {postMessage}");
                        await this.messagePoster.PostMessage(postMessage);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Error Posting Message");
                    }
                }
            }
        }
        await fileSnifferTask;
    }
}
