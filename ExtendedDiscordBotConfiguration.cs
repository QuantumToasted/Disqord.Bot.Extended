using System;
using Microsoft.Extensions.DependencyInjection;

namespace Disqord.Bot.Extended
{
    public class ExtendedDiscordBotConfiguration : DiscordBotConfiguration
    {
        [Obsolete("Specifying the provider directly is not supported; use BaseServiceCollection instead.")]
        public new Func<DiscordBotBase, IServiceProvider> ProviderFactory { set => throw new NotSupportedException("Specifying the provider directly is not supported; use BaseServiceCollection instead."); }

        /// <summary>
        /// The base service collection. Add your non-<see cref="Service"/> classes to the collection here.
        /// <para>(Adding an <see cref="ExtendedDiscordBot"/> to this collection is redundant as it will be automatically added.)</para>
        /// </summary>
        public IServiceCollection BaseServiceCollection { get; set; }

        /// <summary>
        /// Whether or not events should be handled sequentially on the gateway thread or offloaded into a task queue.
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool RunHandlersOnGatewayThread { get; set; } = true;
    }
}