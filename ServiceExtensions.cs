using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Disqord.Bot.Extended
{
    internal static class ServiceExtensions
    {
        public static IServiceCollection DiscoverServices(this IServiceCollection collection)
        {
            foreach (var type in Assembly.GetEntryAssembly().GetTypes()
                .Where(x => typeof(Service).IsAssignableFrom(x) && !x.IsAbstract))
            {
                collection.AddSingleton(type);
            }

            return collection;
        }

        public static async Task InitializeServicesAsync(this IServiceProvider provider)
        {
            foreach (var type in Assembly.GetEntryAssembly().GetTypes()
                .Where(x => typeof(Service).IsAssignableFrom(x) && !x.IsAbstract))
            {
                await ((Service)provider.GetRequiredService(type)).InitializeAsync();
            }
        }

        public static IEnumerable<IHandler> GetHandlers(this IServiceProvider provider, Type argType)
        {
            var handlerType = typeof(IHandler<>).MakeGenericType(argType);
            foreach (var type in Assembly.GetEntryAssembly().GetTypes().Where(x =>
                    handlerType.IsAssignableFrom(x)))
            {
                yield return (IHandler) provider.GetRequiredService(type);
            }
        }
    }
}