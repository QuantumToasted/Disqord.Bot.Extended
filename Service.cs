using System.Threading.Tasks;
using Disqord.Logging;

namespace Disqord.Bot.Extended
{
    public abstract class Service<TBot>
        where TBot : ExtendedDiscordBot
    {
        private protected readonly TBot _bot;

        protected Service(TBot bot)
        {
            _bot = bot;
        }

        public virtual ValueTask InitializeAsync()
        {
            _bot.Log(GetType().Name, LogMessageSeverity.Information, "Service initialized.");
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