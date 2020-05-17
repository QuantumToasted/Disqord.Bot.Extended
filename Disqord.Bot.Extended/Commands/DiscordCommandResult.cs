using System.Threading.Tasks;
using Qmmands;

namespace Disqord.Bot.Extended
{
    public class DiscordCommandResult : CommandResult
    {
        private DiscordCommandResult(string text, LocalEmbed embed, LocalAttachment attachment, bool isSuccessful)
        {
            Text = text;
            Embed = embed;
            Attachment = attachment;
            IsSuccessful = isSuccessful;
        }

        public string Text { get; }

        public LocalEmbed Embed { get; }

        public LocalAttachment Attachment { get; }

        public override bool IsSuccessful { get; }

        public static DiscordCommandResult Successful(string text = default, LocalEmbed embed = default, LocalAttachment attachment = null)
            => new DiscordCommandResult(text, embed, attachment, true);

        public static DiscordCommandResult Unsuccessful(string text = default, LocalEmbed embed = default, LocalAttachment attachment = null)
            => new DiscordCommandResult(text, embed, attachment, false);

        public static implicit operator Task<DiscordCommandResult>(DiscordCommandResult result)
            => Task.FromResult(result);

        public static implicit operator ValueTask<DiscordCommandResult>(DiscordCommandResult result)
            => new ValueTask<DiscordCommandResult>(result);
    }
}