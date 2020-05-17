using System;
using System.Collections.Generic;
using System.Linq;
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
            : base(restClient, prefixProvider, configuration is null ? new ExtendedDiscordBotConfiguration() : configuration = new ExtendedDiscordBotConfiguration
            {
                Activity = configuration.Activity,
                CommandServiceConfiguration =  configuration.CommandServiceConfiguration,
                DefaultMentions = configuration.DefaultMentions,
                Logger = new Optional<ILogger>(configuration.Logger.GetValueOrDefault() ?? new ExtendedSimpleLogger()),
                MessageCache = configuration.MessageCache,
                Serializer = configuration.Serializer,
                ShardCount = configuration.ShardCount,
                ShardId = configuration.ShardId,
                Status = configuration.Status,
                BaseServiceCollection = configuration.BaseServiceCollection ?? new ServiceCollection()
            })
        {
            _configuration = configuration ?? new ExtendedDiscordBotConfiguration();
            _handlerDict = new Dictionary<Type, IEnumerable<IHandler>>();

            // overrides the serviceprovider
            GetType().GetField("_provider").SetValue(this,
                _configuration.BaseServiceCollection
                    .AddSingleton(GetType(), this)
                    .DiscoverServices()
                    .BuildServiceProvider());
        }

        protected ExtendedDiscordBot(TokenType tokenType, string token, IPrefixProvider prefixProvider, ExtendedDiscordBotConfiguration configuration = null) 
            : base(tokenType, token, prefixProvider, configuration is null ? new ExtendedDiscordBotConfiguration() : configuration = new ExtendedDiscordBotConfiguration
            {
                Activity = configuration.Activity,
                CommandServiceConfiguration = configuration.CommandServiceConfiguration,
                DefaultMentions = configuration.DefaultMentions,
                Logger = new Optional<ILogger>(configuration.Logger.GetValueOrDefault() ?? new ExtendedSimpleLogger()),
                MessageCache = configuration.MessageCache,
                Serializer = configuration.Serializer,
                ShardCount = configuration.ShardCount,
                ShardId = configuration.ShardId,
                Status = configuration.Status,
                BaseServiceCollection = configuration.BaseServiceCollection ?? new ServiceCollection()
            })
        {
            _configuration = configuration ?? new ExtendedDiscordBotConfiguration();
            _handlerDict = new Dictionary<Type, IEnumerable<IHandler>>();

            // overrides the serviceprovider
            GetType().GetField("_provider").SetValue(this,
                _configuration.BaseServiceCollection
                    .AddSingleton(GetType(), this)
                    .DiscoverServices()
                    .BuildServiceProvider());
        }

        public override async Task RunAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var type in typeof(DiscordEventArgs).Assembly.GetTypes()
                .Concat(typeof(CommandExecutedEventArgs).Assembly.GetTypes())
                .Where(x => typeof(EventArgs).IsAssignableFrom(x) && !x.IsAbstract))
            {
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
            await base.RunAsync(cancellationToken);
        }

        public void Log(LogMessageSeverity severity, string message, Exception exception = null)
            => Logger.Log(this, new MessageLoggedEventArgs(GetType().Name, severity, message, exception));

        public void Log(string source, LogMessageSeverity severity, string message, Exception exception = null)
            => Logger.Log(this, new MessageLoggedEventArgs(source, severity, message, exception));

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
