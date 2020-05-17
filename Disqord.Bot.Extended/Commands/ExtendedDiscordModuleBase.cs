using Qmmands;

namespace Disqord.Bot.Extended
{
    public abstract class ExtendedDiscordModuleBase<TContext> : ModuleBase<TContext>
        where TContext : DiscordCommandContext
    {
        public DiscordCommandResult Success(string text = default, LocalEmbed embed = default, LocalAttachment attachment = default)
            => DiscordCommandResult.Successful(text, embed, attachment);

        public DiscordCommandResult Error(string text = default, LocalEmbed embed = default, LocalAttachment attachment = default)
            => DiscordCommandResult.Unsuccessful(text, embed, attachment);
    }

    public abstract class ExtendedDiscordModuleBase : ExtendedDiscordModuleBase<DiscordCommandContext>
    { }
}