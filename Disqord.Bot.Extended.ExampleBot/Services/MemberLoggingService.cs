using System.Threading.Tasks;
using Disqord.Events;
using Disqord.Logging;

namespace Disqord.Bot.Extended.ExampleBot
{
    public sealed class MemberLoggingService : Service<ExampleBot>,
        IHandler<MemberLeftEventArgs>,
        IHandler<MemberJoinedEventArgs>
    {
        public MemberLoggingService(ExampleBot bot) 
            : base(bot)
        { }

        public ValueTask HandleAsync(MemberLeftEventArgs e)
        {
            _bot.Log("MemberLogging", LogSeverity.Information, 
                $"Member {e.User} left guild {e.Guild.Name}.");

            return new ValueTask();
        }

        public ValueTask HandleAsync(MemberJoinedEventArgs e)
        {
            _bot.Log("MemberLogging", LogSeverity.Information,
                $"Member {e.Member} joined guild {e.Member.Guild.Name}.");

            return new ValueTask();
        }
    }
}