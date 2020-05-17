using System;
using System.Threading.Tasks;

namespace Disqord.Bot.Extended
{
    /// <summary>
    /// A marker interface for event handlers internally. Use <see cref="IHandler{TArgs}"/> instead.
    /// </summary>
    public interface IHandler
    { }

    /// <summary>
    /// Defines a handler for a specific type of event fired by a <see cref="ExtendedDiscordBot"/>.
    /// </summary>
    /// <typeparam name="TArgs">Any <see cref="EventArgs"/> which exists under the Disqord or Qmmands namespace.</typeparam>
    public interface IHandler<in TArgs> : IHandler
        where TArgs : EventArgs
    {
        ValueTask HandleAsync(TArgs args);
    }
}