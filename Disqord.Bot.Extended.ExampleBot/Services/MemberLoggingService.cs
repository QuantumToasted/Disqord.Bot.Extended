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

        public ValueTask HandleAsync(MemberLeftEventArgs args)
        {
            _bot.Log("MemberLogging", LogMessageSeverity.Information, 
                $"Member {args.User} left guild {args.Guild.Name}.");

            return new ValueTask();
        }

        public ValueTask HandleAsync(MemberJoinedEventArgs args)
        {
            _bot.Log("MemberLogging", LogMessageSeverity.Information,
                $"Member {args.Member} joined guild {args.Member.Guild.Name}.");

            return new ValueTask();
        }
    }
}