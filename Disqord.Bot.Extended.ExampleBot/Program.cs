namespace Disqord.Bot.Extended.ExampleBot
{
    public static class Program
    {
        public static void Main()
        {
            using var bot = new ExampleBot();
            bot.Run();
        }
    }
}
