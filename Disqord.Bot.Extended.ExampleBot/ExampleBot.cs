using System;
using System.Reflection;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;

namespace Disqord.Bot.Extended.ExampleBot
{
    public sealed class ExampleBot : ExtendedDiscordBot
    {
        public ExampleBot() 
            : base(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN", EnvironmentVariableTarget.Machine), 
                new DefaultPrefixProvider().AddPrefix('!'), new ExtendedDiscordBotConfiguration
                {
                    BaseServiceCollection = new ServiceCollection().AddSingleton<Random>(),
                    ModuleDiscoveryAssembly = Assembly.GetEntryAssembly(),
                    Logger = new ExtendedSimpleLogger(new ExtendedSimpleLoggerConfiguration
                    {
                        EnableTraceLogSeverity = false,
                        EnableDebugLogSeverity = false
                    })
                })
        { }
    }
}