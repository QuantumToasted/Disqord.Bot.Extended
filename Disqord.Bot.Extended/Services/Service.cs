using System.Threading.Tasks;
using Disqord.Logging;

namespace Disqord.Bot.Extended
{
    /// <summary>
    /// A service which is automatically injected into your bot's service provider.
    /// </summary>
    /// <typeparam name="TBot">
    /// The type of bot to pass into the service via Dependency Injection.
    /// <para>Should match your custom inherited <see cref="ExtendedDiscordBot"/> type.</para> 
    /// </typeparam>
    public abstract class Service<TBot> : IInitializable
        where TBot : ExtendedDiscordBot
    {
        /// <summary>
        /// The bot which was passed into the service.
        /// </summary>
        protected readonly TBot _bot;

        protected Service(TBot bot)
        {
            _bot = bot;
        }

        /// <summary>
        /// Overrides the default initialization behavior. This method is called automatically when the bot runs.
        /// <para>For best results, call <c>base.InitializeAsync</c> when overriding this method.</para>
        /// </summary>
        /// <returns></returns>
        public virtual ValueTask InitializeAsync()
        {
            _bot.Log(GetType().Name, LogSeverity.Information, "Service initialized.");
            return new ValueTask();
        }
    }

    public abstract class Service : Service<ExtendedDiscordBot>
    {
        protected Service(ExtendedDiscordBot bot)
            : base(bot)
        { }
    }
}