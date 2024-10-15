using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryLogger
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Build(this IServiceCollection services)
        {
            services.AddSingleton<LoggerService>();
            services.AddTransient<ILogFileParser, LogFileParser>();
            services.AddSingleton<IFileChangeSniffer, FileChangedSniffer>();
            services.AddSingleton<ILogFileActionHandler, LogFileActionHandler>();
            services.AddTransient<IMessagePoster, DiscordMessagePoster>();

            return services;
        }
    }
}
