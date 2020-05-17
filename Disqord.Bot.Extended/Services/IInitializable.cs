using System.Threading.Tasks;

namespace Disqord.Bot.Extended
{
    internal interface IInitializable
    {
        ValueTask InitializeAsync();
    }
}