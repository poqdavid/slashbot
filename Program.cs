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
        var cts = new CancellationTokenSource();

        var host = Host.CreateDefaultBuilder(args)
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
             //services.AddSingleton<CommandsExtension>();
             services.AddDiscordClient(configuration["discord:token"] ?? "<token>", DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);
             services.AddSingleton<SlashBotHostedService>();
             services.AddHostedService(s => s.GetRequiredService<SlashBotHostedService>());
             //services.AddHostedService<SlashBotHostedService>();
         })
         .ConfigureLogging(logging =>
         {
             logging.ClearProviders();
             logging.AddSerilog();
         })
         .Build();

        var configuration = host.Services.GetRequiredService<IConfiguration>();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Config.LoadSettings();
        }
        catch (Exception ex) { Log.Logger.Error(ex, "Error loading {SettingPath}", SettingPath); }
        finally { Log.Logger.Information("Settings loaded"); }

        Config.Discord.Token = Environment.GetEnvironmentVariable("Discord_Token") ?? Config.Discord.Token;
        Config.Discord.Token = configuration["discord:token"] ?? Config.Discord.Token;

        Config.Discord.DefaultActivity = Environment.GetEnvironmentVariable("Discord_DefaultActivity") ?? Config.Discord.DefaultActivity;

        if (Enum.TryParse<DiscordActivityType>(Environment.GetEnvironmentVariable("Discord_DefaultActivityType"), out DiscordActivityType at))
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
}