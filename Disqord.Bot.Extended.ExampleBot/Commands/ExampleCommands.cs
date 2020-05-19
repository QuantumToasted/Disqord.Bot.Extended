using Qmmands;

namespace Disqord.Bot.Extended.ExampleBot
{
    public sealed class ExampleCommands : ExtendedDiscordModuleBase
    {
        [Command("echo")]
        public DiscordCommandResult Echo([Remainder] string text)
            => Success(text);

        [Command("userinfo")]
        public DiscordCommandResult GetUserInfo([Remainder] CachedUser user = null)
        {
            user ??= Context.User;

            return Success(embed: new LocalEmbedBuilder()
                .WithColor(Color.Green)
                .WithThumbnailUrl(user.GetAvatarUrl())
                .WithTitle($"Information for user {user}")
                .AddField("ID", user.Id)
                .AddField("Joined Discord", user.CreatedAt.ToString("g"))
                .Build());
        }
    }
}