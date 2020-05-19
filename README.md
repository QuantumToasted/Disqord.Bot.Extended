# Disqord.Bot.Extended
An extension of [Disqord.Bot](https://github.com/Quahu/Disqord/tree/master/src/Disqord.Bot) designed with streamlining and simplifying bot creation and deployment, reducing the amount of boilerplate and duplicate code needed, especially when developing multiple bots.

The major goals of this extension are to:

- Automate and streamline service discovery and initialization
- Allow services to easily handle bot events without `+=`/`-=` everywhere
- Extend the base bot itself, as well as the configuration class
- Add a simple, configurable logging class which is automatically enabled if no overridden logger is passed to to the bot configuration



## Major features

A barebones example detailing most major features can be found in the the [ExampleBot](https://github.com/QuantumToasted/Disqord.Bot.Extended/tree/master/Disqord.Bot.Extended.ExampleBot) project in this repo.

#### ExtendedDiscordBot
This class inherits from `DiscordBot`, with a slightly different constructor accepting an `ExtendedDiscordBotConfiguration`. It can be used directly or inherited; if you do the latter, you must implement at least one of its constructors:

```cs
public sealed class ExampleBot : ExtendedDiscordBot
{
    public ExampleBot() 
    : base(TokenType.Bot, "YOUR_BOT_TOKEN",
        new DefaultPrefixProvider().AddPrefix('!'), new ExtendedDiscordBotConfiguration
        {
            BaseServiceCollection = new ServiceCollection().AddSingleton<Random>(),
            ModuleDiscoveryAssembly = Assembly.GetEntryAssembly(),
            Logger = new ExtendedSimpleLogger(new ExtendedSimpleLoggerConfiguration
            {
                EnableTraceLogSeverity = false,
                EnableDebugLogSeverity = false
            })
        })
    { }
}
```

This creates a bot with a single prefix `!`, a service collection which adds a `Random` singleton, and specifies that your command modules can be found in the assembly returned by `Assembly.GetEntryAssembly()`. The bot will automatically add valid modules from this assembly. It also specifies a customized `ExtendedSimpleLogger` which specifies to not log Trace- or Debug-severity messages.


#### ExtendedDiscordBotConfiguration

This class inherits from `DiscordBotConfiguration`, which adds a few extra configuration options, but most importantly, __deprecates `DiscordBot`'s ProviderFactory property__.


```cs
public IServiceCollection BaseServiceCollection { get; set; } = new ServiceCollection();
```

As the bot handles automatic discovery of your user-defined services via `Service<TBot>`, the only way to both allow the user to provide external services to their provider *and* automatically discover services was to instead have them provide a service collection which contained any additional non-`Service<TBot>` services. It is important to note that directly adding the bot to the `BaseServicecollection` is not required (ignoring the fact that it shouldn't be possible), as the internals will add the bot itself to the collection before it is built into a service provider.



```cs
public bool RunHandlersOnGatewayThread { get; set; } = true;
```

This option allows any class which implements `IHandler<TArgs>` to have its handling methods offloaded from the gateway thread via `Task.Run()` when set to `false` - this is disabled by default as it could introduce possible race conditions or lead to a stale client cache (depending on how long your handlers run) - however if you don't care, the option exists. Each client event will run all handlers for that event before handling the *next* event.



```cs
public Assembly ModuleDiscoveryAssembly { get; set; }
```

This option is where you would define the assembly in which your command modules are located. Valid command modules will automatically be added via `AddModules` at runtime. This does not have a default value, as it could cause undesired modules to be loaded. If you have a complex system for deciding or finding modules, you can still call `AddModule`/`AddModules` as you normally would. This option is for users who just have any modules in the same assembly and want to just add them all easily. The most likely value you'd want to set this to would be `Assembly.GetEntryAssembly()`.



#### ExtendedSimpleLogger

This is a simple console and file logger which is meant to be a direct drop-in right into the bot via the `Logger` property on the configuration class. If no logger is passed to the bot, this logger will be used along with all of its default values. The configuration file is completely documented, so if you wish to learn about the details of the options, feel free to check those out. Here is a barebones example of what the logger looks like starting up (with the settings in the `ExampleBot` project):
![Example logger image](https://i.imgur.com/Z17JBBs.png)


#### Service / Service\<TBot>

This class is what inspired the project in the first place - automatic discovery and initialization of services without needing to remember to add them to your service provider/bot. If you are using a custom `ExtendedDiscordBot` (in our case, `ExampleBot`) you should make your services inherit from `Service<ExampleBot>` instead of just `Service`, which is just sugar for `Service<ExtendedDiscordBot>`.
All you need to do to add your service is...

```cs
public class MyService : Service<ExampleBot>
{
    public MyService(ExampleBot bot)
        : base(bot)
    { }
}
```

...and that's it! Now, your service will automatically be added to your service provider/bot. You should obviously implement its functionality, but that's up to you. You can also override the `InitializeAsync()` method to implement any initialization logic you need for your service. It's recommended to call the base method in your overridden method, as it will automatically log that the service was initialized. This method is called once, the first time the `Ready` event fires, so if you rely on cache in this method you can ensure what you need is probably there.



#### ScheduledService / ScheduledService\<TBot>

This class is a `Service`/`Service<TBot>` with one important distinction - it also has the ability to run a scheduled task at a specified interval. By inheriting the class you must implement both the task which is run and the interval at which it is run. You can also override the `CheckIfRunnableAsync()` method to determine whether the task *should* run. Note that if this method returns `false` than it will not immediately retry, but instead wait the specified interval again.

```cs
public class MyScheduledService : ScheduledService<ExampleBot>
{
    public MyScheduledService(ExampleBot bot)
        : base(bot)
    { }

    protected override async ValueTask InvokeAsync()
    {
        Console.WriteLine("Test!");
    }

    protected override TimeSpan Interval => TimeSpan.FromSeconds(30);
}
```

This creates a service which prints `Test!` to the console every 30 seconds.
**These schedules are first started on the first `Ready` event from the bot as well - in the above example, `Test!` will be written to console 30 seconds after the first `Ready` event.*



#### IHandler\<TArgs>

This interface is designed to be coupled with your `Service<TBot>` classes, defining methods which handle events fired by your bot class. Any `EventArgs` within the Qmmands or Disqord namespace are valid. Any other types of `EventArgs` will simply never have their methods run unless you call them manually. `IHandler<TArgs>` can be put on any class you choose, even your bot class, if you like. A class can have as many handlers as it likes.

```cs
public class MemberStateHandler : IHandler<MemberLeftEventArgs>, IHandler<MemberJoinedEventArgs>
{
    public ValueTask HandleAsync(MemberLeftEventArgs e)
    {
        Console.WriteLine($"Member {e.User} left guild {e.Guild.Name}.");
        return new ValueTask();
    }

    public ValueTask HandleAsync(MemberJoinedEventArgs e)
    {
        Console.WriteLine($"Member {e.Member} joined guild {e.Member.Guild.Name}.");
        return new ValueTask();
    }
}
```

This simple handler will print a message to console when a member leaves or joins a guild your bot is in.



#### DiscordCommandResult / ExtendedDiscordModuleBase\<TContext>

These two classes are meant to take full advantage of Qmmands' `CommandResult` pattern coupled with `DiscordBot`'s `AfterExecutedAsync` method. Depending on the result returned from your commands, a response message will automatically be sent when a command is executed. `DiscordCommandResult` can be inherited and casted to and from where necessary, especially when used in an overridden `SendResultAsync` (available on `ExtendedDiscordBot`). A simple echo command would now look like this:


```cs
public class MyModule : ExtendedDiscordModule<ExampleBot>
{
    [Command("echo")]
    public DiscordCommandResult Echo([Remainder] string text)
        => Success(text);
}
```

This command will cause the bot to reply with whatever text was passed to the command!
**Note: Similar to Service\<T>, `ExtendedDiscordModuleBase` on its own is a shortcut for `ExtendedDiscordModuleBase\<DiscordCommandContext>`. If you have your own extension of `DiscordCommandContext`, supply it as the `TContext` instead of using the generic-less version.*
