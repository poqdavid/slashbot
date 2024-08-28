/*
 *      This file is part of SlashBot distribution (https://github.com/sysvdev/slashbot).
 *      Copyright (c) 2024 contributors
 *
 *      SlashBot is free software: you can redistribute it and/or modify
 *      it under the terms of the GNU General Public License as published by
 *      the Free Software Foundation, either version 3 of the License, or
 *      (at your option) any later version.
 *
 *      SlashBot is distributed in the hope that it will be useful,
 *      but WITHOUT ANY WARRANTY; without even the implied warranty of
 *      MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *      GNU General Public License for more details.
 *
 *      You should have received a copy of the GNU General Public License
 *      along with SlashBot.  If not, see <https://www.gnu.org/licenses/>.
 */

namespace SlashBot;

internal class Program
{
    public static string CurrentDir { get => Environment.CurrentDirectory; }
    public static string SettingPath { get; set; } = Path.Combine(CurrentDir, "config.json");

    public static Settings Config { get; set; } = new();

    public static readonly EventId BotEventId = new(42, "SlashBot");

    public static DiscordChannel? lastdiscordChannel = null;

    public static string[] TableFlips = [
        "┳━┳ ヽ(ಠل͜ಠ)ﾉ",
        "┬─┬ノ( º _ ºノ)",
        "(˚Õ˚)ر ~~~~╚╩╩╝",
        "ヽ(ຈل͜ຈ)ﾉ︵ ┻━┻",
        "(ノಠ益ಠ)ノ彡┻━┻",
        "(╯°□°)╯︵ ┻━┻",
        "(┛◉Д◉)┛彡┻━┻",
        "(☞ﾟヮﾟ)☞ ┻━┻",
        "(┛ಠ_ಠ)┛彡┻━┻"
        ];

    private static async Task Main(string[] args)
    {
        var program = new Program();

        Thread.CurrentThread.Name = "MainThread";

        var cts = new CancellationTokenSource();

        var host = Host.CreateDefaultBuilder(args)
         .UseSerilog()
         .UseConsoleLifetime()
         .ConfigureAppConfiguration((context, config) =>
         {
             config.SetBasePath(Directory.GetCurrentDirectory());
             config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
             config.AddJsonFile("config.json", optional: true, reloadOnChange: true);
             config.AddEnvironmentVariables();
             config.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
         })
         .ConfigureServices((context, services) =>
         {
             var configuration = context.Configuration;
             services.AddLogging(logging =>
             {
                 logging.ClearProviders().AddSerilog();
                 Log.Logger = new LoggerConfiguration()
                     .ReadFrom.Configuration(configuration)
                     .CreateLogger();
             });

             services.AddSingleton<IClientErrorHandler, ErrorHandler>();
             services.AddDiscordClient(configuration["discord:token"] ?? "<token>", DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents).ConfigureEventHandlers(b =>
             {
                 b.HandleGuildAvailable(program.OnGuildAvailable)
                 .HandleGuildDownloadCompleted(program.OnGuildDownloadCompleted)
                 .HandleGuildCreated(program.OnGuildCreated)
                 .HandleSessionCreated(program.OnSessionCreated);
             });

             var StrDebugGuildId = configuration["discord:debugguildid"] ?? "0";
             services.AddCommandsExtension(extension =>
             {
                 // Add all commands by scanning the current assembly
                 extension.AddCommands(typeof(BotCommands));

                 List<ICommandProcessor> processors = [];
                 SlashCommandProcessor slashCommandProcessor = new();
                 slashCommandProcessor.AddConverters(typeof(BotCommands).Assembly);
                 processors.Add(slashCommandProcessor);

                 extension.AddProcessors(processors);
             },
             new CommandsConfiguration()
             {
                 DebugGuildId = ulong.Parse(StrDebugGuildId),
                 UseDefaultCommandErrorHandler = false,
                 RegisterDefaultCommandProcessors = false
             });

             services.AddSingleton<SlashBotHostedService>();
             services.AddHostedService(s => s.GetRequiredService<SlashBotHostedService>());
         })
         .Build();

        var configuration = host.Services.GetRequiredService<IConfiguration>();

        try
        {
            Config.LoadSettings();
        }
        catch (Exception ex) { Log.Logger.Error(ex, "Error loading {SettingPath}", SettingPath); }
        finally { Log.Logger.Information("Settings loaded"); }

        Config.Discord.Token = configuration["discord:token"] ?? Config.Discord.Token;

        Config.Discord.DefaultActivity = configuration["discord:defaultactivity"] ?? Config.Discord.DefaultActivity;

        if (Enum.TryParse<DiscordActivityType>(configuration["discord:defaultactivitytype"], out DiscordActivityType at))
        {
            Config.Discord.DefaultActivityType = at;
        }
        else
        {
            Config.Discord.DefaultActivityType = DiscordActivityType.ListeningTo;
        }

        Config.SaveSetting();

        try
        {
            await host.RunAsync(cts.Token);
        }
        catch (TaskCanceledException)
        {
            Log.Logger.Information("Bot is shutting down...");
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "Bot crashed");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private async Task OnSessionCreated(DiscordClient sender, SessionCreatedEventArgs e)
    {
        sender.Logger.LogInformation(Program.BotEventId, "Client is ready to process events.");

        await sender.UpdateStatusAsync(new DiscordActivity()
        {
            ActivityType = Program.Config.Discord.DefaultActivityType,
            Name = Program.Config.Discord.DefaultActivity
        }, DiscordUserStatus.Online);
    }

    private Task OnGuildAvailable(DiscordClient sender, GuildAvailableEventArgs e)
    {
        sender.Logger.LogInformation(Program.BotEventId, "Guild available: {GuildName}", e.Guild.Name);

        return Task.CompletedTask;
    }

    private Task OnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        sender.Logger.LogInformation(Program.BotEventId, "Guild download completed: {GuildName}", e.Guilds.Values);

        return Task.CompletedTask;
    }

    private Task OnGuildCreated(DiscordClient sender, GuildCreatedEventArgs e)
    {
        sender.Logger.LogInformation(Program.BotEventId, "Guild joined: {GuildName}", e.Guild.Name);

        return Task.CompletedTask;
    }
}