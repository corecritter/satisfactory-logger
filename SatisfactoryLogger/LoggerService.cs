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
    private readonly IFileChangeSniffer fileChangeSniffer;
    private readonly ILogFileParser logFileParser;
    private readonly ILogFileActionHandler logFileActionHandler;
    private readonly IMessagePoster messagePoster;
    private readonly ILogger logger;
    public LoggerService(
        IFileChangeSniffer fileChangeSniffer,
        ILogFileParser logFileParser,
        ILogFileActionHandler logFileActionHandler,
        IMessagePoster messagePoster,
        ILogger<LoggerService> logger)
    {
        this.fileChangeSniffer = fileChangeSniffer;
        this.logFileParser = logFileParser;
        this.logFileActionHandler = logFileActionHandler;
        this.messagePoster = messagePoster;
        this.logger = logger;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            this.logger.LogInformation("Waiting for file sniffer.");
            var changedFile = await this.fileChangeSniffer.WaitForFileChange(cancellationToken);
            this.logger.LogInformation($"Parsing file {changedFile}");
            var actions = await this.logFileParser.ParseFile(changedFile, cancellationToken);
            this.logger.LogInformation($"Handling {actions.Count} actions");
            foreach(var action in actions)
            {
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
    }
}
