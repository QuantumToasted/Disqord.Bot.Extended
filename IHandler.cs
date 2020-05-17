using System;
using System.Threading.Tasks;

namespace Disqord.Bot.Extended
{
    public interface IHandler
    { }

    public interface IHandler<in T1> : IHandler
        where T1 : EventArgs
    public interface IHandler<in TArgs> : IHandler
        where TArgs : EventArgs
    {
        ValueTask HandleAsync(TArgs args);
    }
}