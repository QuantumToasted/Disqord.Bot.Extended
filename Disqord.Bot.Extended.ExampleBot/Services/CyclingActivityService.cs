using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Disqord.Bot.Extended.ExampleBot
{
    public sealed class CyclingActivityService : ScheduledService<ExampleBot>
    {
        private readonly Random _random;
        private readonly IReadOnlyList<LocalActivity> _activities;

        public CyclingActivityService(ExampleBot bot)
            : base(bot)
        {
            _random = bot.GetRequiredService<Random>();
            _activities = new[]
            {
                new LocalActivity("ready to serve you!", ActivityType.Playing),
                new LocalActivity("cool tunes", ActivityType.Listening),
                new LocalActivity("Half-Life 3 Closed Alpha", ActivityType.Playing),
            };
        }

        protected override async ValueTask InvokeAsync()
        {
            var activity = _activities[_random.Next(_activities.Count)];
            _bot.Log("CyclingActivity", LogMessageSeverity.Information,
                $"Setting activity to: {activity.Type} {activity.Name}");

            await _bot.SetPresenceAsync(activity);
        }

        protected override TimeSpan Interval => TimeSpan.FromSeconds(45);
    }
}