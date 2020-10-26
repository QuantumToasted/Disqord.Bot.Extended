using System;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Logging;

namespace Disqord.Bot.Extended
{
    /// <summary>
    /// A <see cref="Service"/> which also contains a scheduled task to run at a specified interval.
    /// </summary>
    /// <typeparam name="TBot">
    /// The type of bot to pass into the service via Dependency Injection.
    /// <para>Should match your custom inherited <see cref="ExtendedDiscordBot"/> type.</para> 
    /// </typeparam>
    public abstract class ScheduledService<TBot> : Service<TBot>, IStartable
        where TBot : ExtendedDiscordBot
    {
        private CancellationTokenSource _tokenSource;

        protected ScheduledService(TBot bot)
            : base(bot)
        { }

        /// <summary>
        /// The task to run at the interval specified by <see cref="Interval"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract ValueTask InvokeAsync();

        /// <summary>
        /// Checks if the scheduled task should (or is able to) run.
        /// </summary>
        protected virtual ValueTask<bool> CheckIfRunnableAsync()
            => new ValueTask<bool>(true);

        /// <summary>
        /// The interval of time upon which the scheduled task will be run.
        /// </summary>
        protected abstract TimeSpan Interval { get; }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource(Interval);

            Task.Delay(-1, _tokenSource.Token)
                .ContinueWith(_ => BeginInvokeAsync());
        }

        private async Task BeginInvokeAsync()
        {
            try
            {
                if (!await CheckIfRunnableAsync()) return;
                await InvokeAsync();
            }
            catch (Exception ex)
            {
                _bot.Log(GetType().Name, LogSeverity.Error, "An exception occurred running a scheduled task.",
                    ex);
            }

            Start();
        }
    }

    /// <summary>
    /// A <see cref="Service"/> which also contains a scheduled task to run at a specified interval, passing in a <see cref="ExtendedDiscordBot"/>.
    /// </summary>
    public abstract class ScheduledService : ScheduledService<ExtendedDiscordBot>
    {
        protected ScheduledService(ExtendedDiscordBot bot)
            : base(bot)
        { }
    }
}