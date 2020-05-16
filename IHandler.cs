using System;
using System.Threading.Tasks;

namespace Disqord.Bot.Extended
{
    public interface IHandler
    { }

    public interface IHandler<in T1> : IHandler
        where T1 : EventArgs
    {
        Task HandleAsync(T1 args);
    }
}