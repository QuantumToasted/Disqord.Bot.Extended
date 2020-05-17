using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Bot.Prefixes;
using Disqord.Events;
using Disqord.Logging;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Disqord.Bot.Extended
{
    public abstract class ExtendedDiscordBot : DiscordBot
    {
        private readonly ExtendedDiscordBotConfiguration _configuration;
        private readonly IDictionary<Type, IEnumerable<IHandler>> _handlerDict; // TODO: Does making this IEnumerable cause multiple enumeration?

        protected ExtendedDiscordBot(RestDiscordClient restClient, IPrefixProvider prefixProvider, ExtendedDiscordBotConfiguration configuration = null) 
            : base(restClient, prefixProvider, configuration is null ? new ExtendedDiscordBotConfiguration() : configuration.CopyAndConfigure())
        {
            _configuration = configuration ?? new ExtendedDiscordBotConfiguration();
            _handlerDict = new Dictionary<Type, IEnumerable<IHandler>>();

            typeof(DiscordBotBase).GetField("_provider", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this,
                _configuration.BaseServiceCollection
                    .AddSingleton(GetType(), this)
                    .DiscoverServices()
                    .BuildServiceProvider());
        }

        protected ExtendedDiscordBot(TokenType tokenType, string token, IPrefixProvider prefixProvider, ExtendedDiscordBotConfiguration configuration = null) 
            : base(tokenType, token, prefixProvider, configuration is null ? new ExtendedDiscordBotConfiguration() : configuration.CopyAndConfigure())
        {
            _configuration = configuration ?? new ExtendedDiscordBotConfiguration();
            _handlerDict = new Dictionary<Type, IEnumerable<IHandler>>();

            typeof(DiscordBotBase).GetField("_provider", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this,
                _configuration.BaseServiceCollection
                    .AddSingleton(GetType(), this)
                    .DiscoverServices()
                    .BuildServiceProvider());
        }

        protected void Log(LogMessageSeverity severity, string message, Exception exception = null)
            => Logger.Log(this, new MessageLoggedEventArgs(GetType().Name, severity, message, exception));

        public void Log(string source, LogMessageSeverity severity, string message, Exception exception = null)
            => Logger.Log(this, new MessageLoggedEventArgs(source, severity, message, exception));

        /// <summary>
        /// Handles hooking events and automatically discovering command modules.
        /// <para>If overridden in your bot, be sure to call <see langword="base"/>.<see cref="RunAsync()"/> to take advantage of this bot's features.</para>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task RunAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (_configuration.ModuleDiscoveryAssembly is { })
            {
                var modules = AddModules(_configuration.ModuleDiscoveryAssembly);
                Log(LogMessageSeverity.Information,
                    $"Discovered {modules.Count} module(s) under the {_configuration.ModuleDiscoveryAssembly.GetName().Name} assembly.");
            }

            foreach (var type in typeof(DiscordEventArgs).Assembly.GetTypes()
                .Concat(typeof(CommandExecutedEventArgs).Assembly.GetTypes())
                .Where(x => typeof(EventArgs).IsAssignableFrom(x) && !x.IsAbstract))
            {
                Log(LogMessageSeverity.Debug,
                    $"Created an event handler entry for type {type.FullName}.");
                _handlerDict[type] = this.GetHandlers(type);
            }

            // Misc
            Ready += HandleEvent;
            CommandExecutionFailed += HandleEvent;
            CommandExecuted += HandleEvent;
            InviteCreated += HandleEvent;
            InviteDeleted += HandleEvent;
            PresenceUpdated += HandleEvent;

            // User token
            // MessageAcknowledged += HandleEventAsync;
            // RelationshipCreated += HandleEventAsync;
            // RelationshipDeleted += HandleEventAsync;
            // UserNoteUpdated += HandleEventAsync;

            // Messages
            MessageReceived += HandleEvent;
            MessageDeleted += HandleEvent;
            MessageUpdated += HandleEvent;
            MessagesBulkDeleted += HandleEvent;

            // Members/Users
            MemberUpdated += HandleEvent;
            MemberBanned += HandleEvent;
            MemberJoined += HandleEvent;
            MemberLeft += HandleEvent;
            MemberUnbanned += HandleEvent;
            UserUpdated += HandleEvent;

            // Channels
            ChannelCreated += HandleEvent;
            ChannelDeleted += HandleEvent;
            ChannelUpdated += HandleEvent;
            ChannelPinsUpdated += HandleEvent;
            TypingStarted += HandleEvent;
            VoiceStateUpdated += HandleEvent;

            // Roles
            RoleCreated += HandleEvent;
            RoleDeleted += HandleEvent;
            RoleUpdated += HandleEvent;

            // Reactions
            ReactionAdded += HandleEvent;
            ReactionRemoved += HandleEvent;
            ReactionsCleared += HandleEvent;
            EmojiReactionsCleared += HandleEvent;

            // Guilds
            JoinedGuild += HandleEvent;
            LeftGuild += HandleEvent;
            GuildUpdated += HandleEvent;
            GuildEmojisUpdated += HandleEvent;
            GuildAvailable += HandleEvent;
            GuildUnavailable += HandleEvent;
            VoiceServerUpdated += HandleEvent;
            WebhooksUpdated += HandleEvent;

            await this.InitializeServicesAsync();
            this.StartSchedules();
            await base.RunAsync(cancellationToken);
        }

        /// <summary>
        /// Handles post-command execution. If overridden in your bot, be sure to call <see langword="base"/>.<see cref="AfterExecutedAsync()"/>.
        /// </summary>
        protected override ValueTask AfterExecutedAsync(IResult _, DiscordCommandContext context)
        {
            if (!(_ is DiscordCommandResult result)) return new ValueTask();
            return SendResultAsync(result, context);
        }

        /// <summary>
        /// Sends <see cref="DiscordCommandResult"/>s returned from commands to the channel the command was used in.
        /// </summary>
        /// <param name="result">The result returned from the executed command.</param>
        /// <param name="context">The context of the executed command.</param>
        protected virtual async ValueTask SendResultAsync(DiscordCommandResult result, DiscordCommandContext context)
        {
            Log(LogMessageSeverity.Information,
                $"Command [{result.Command.FullAliases[0]}] was executed by user [{context.User}]");

            if (result.Attachment is { })
            {
                using (result.Attachment)
                {
                    await context.Channel.SendMessageAsync(result.Attachment, result.Text, embed: result.Embed);
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(result.Text) || result.Embed is { })
            {
                await context.Channel.SendMessageAsync(result.Text, embed: result.Embed);
            }
        }


        // TODO: Performance
        private Task HandleEvent<TArgs>(TArgs args)
            where TArgs : EventArgs
        {
            if (!_configuration.RunHandlersOnGatewayThread) 
                return ProcessHandlersAsync(args);

            _ = Task.Run(async () => await ProcessHandlersAsync(args));
            return Task.CompletedTask;
        }

        // TODO: Performance?
        private async Task ProcessHandlersAsync<TArgs>(TArgs args)
            where TArgs : EventArgs
        {
            foreach (var handler in _handlerDict[typeof(TArgs)])
            {
                try
                {
                    await ((IHandler<TArgs>) handler).HandleAsync(args);
                }
                catch (Exception ex)
                {
                    Log(LogMessageSeverity.Error, "An exception occurred handling this event type.", ex);
                }
            }
        }
    }
}
