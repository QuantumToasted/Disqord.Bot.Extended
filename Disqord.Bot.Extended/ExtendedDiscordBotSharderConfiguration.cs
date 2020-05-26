using System;
using System.Reflection;
using Disqord.Bot.Sharding;
using Disqord.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Disqord.Bot.Extended
{
    public class ExtendedDiscordBotSharderConfiguration : DiscordBotSharderConfiguration
    {
        [Obsolete("Specifying the provider directly is not supported; use BaseServiceCollection instead.", true)]
        public new Func<DiscordBotBase, IServiceProvider> ProviderFactory { set => throw new NotSupportedException("Specifying the provider directly is not supported; use BaseServiceCollection instead."); }

        /// <summary>
        /// The base service collection. Add your non-<see cref="Service"/> classes to the collection here.
        /// <para>(Adding an <see cref="ExtendedDiscordBot"/> to this collection is redundant as it will be automatically added.)</para>
        /// <para>Defaults to an empty <see cref="ServiceCollection"/>.</para>
        /// </summary>
        public IServiceCollection BaseServiceCollection { get; set; } = new ServiceCollection();

        /// <summary>
        /// Whether or not events should be handled sequentially on the gateway thread or offloaded into a task queue.
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool RunHandlersOnGatewayThread { get; set; } = true;

        /// <summary>
        /// The <see cref="Assembly"/> to automatically discover command modules from.
        /// <para>Use <see cref="Assembly.GetEntryAssembly()"/> if unsure.</para>
        /// </summary>
        public Assembly ModuleDiscoveryAssembly { get; set; }

        public new static ExtendedDiscordBotConfiguration Default => new ExtendedDiscordBotConfiguration();

        internal ExtendedDiscordBotSharderConfiguration CopyAndConfigure()
        {
            return new ExtendedDiscordBotSharderConfiguration
            {
                ModuleDiscoveryAssembly = ModuleDiscoveryAssembly,
                Activity = Activity,
                CommandServiceConfiguration = CommandServiceConfiguration,
                DefaultMentions = DefaultMentions,
                Logger = new Optional<ILogger>(Logger.GetValueOrDefault() ?? new ExtendedSimpleLogger()),
                MessageCache = MessageCache,
                Serializer = Serializer,
                ShardCount = ShardCount,
                Status = Status,
                BaseServiceCollection = BaseServiceCollection ?? new ServiceCollection()
            };
        }
    }
}