/*
 *      This file is part of SlashBot distribution (https://github.com/sysvdev/slashbot).
 *      Copyright (c) 2023 contributors
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
    private static Logger? logger;

    public static Logger Logger
    { get { return logger ?? throw new Exception("Logger Is Null"); } set => logger = value; }

    public static string CurrentDir { get => Environment.CurrentDirectory; }
    public static string SettingPath { get; set; } = Path.Combine(CurrentDir, "config.json");

    public static Settings Config { get; set; } = new();

    public readonly EventId BotEventId = new(42, "SlashBot");

    public static DiscordClient? Client { get; set; }
    public CommandsNextExtension Commands { get; set; }

    public static DiscordChannel? lastdiscordChannel = null;

    public static string[] TableFlips = new string[] {
        "┳━┳ ヽ(ಠل͜ಠ)ﾉ",
        "┬─┬ノ( º _ ºノ)",
        "(˚Õ˚)ر ~~~~╚╩╩╝",
        "ヽ(ຈل͜ຈ)ﾉ︵ ┻━┻",
        "(ノಠ益ಠ)ノ彡┻━┻",
        "(╯°□°)╯︵ ┻━┻",
        "(┛◉Д◉)┛彡┻━┻",
        "(☞ﾟヮﾟ)☞ ┻━┻",
        "(┛ಠ_ಠ)┛彡┻━┻"
        };

    private static void Main(string[] args)
    {
        Thread.CurrentThread.Name = "MainThread";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Config.LoadSettings();
        }
        catch (Exception ex) { Logger.Error(ex, "Error loading {SettingPath}", SettingPath); }
        finally { logger.Information("Settings loaded"); }

        var prog = new Program();

        try { prog.RunBotAsync().GetAwaiter().GetResult(); }
        catch (Exception ex) { Logger.Error(ex, ex.Message); }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public Program()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Config.Discord.Token = Environment.GetEnvironmentVariable("Discord_Token") ?? Config.Discord.Token;
        Config.Discord.CommandPrefix = Environment.GetEnvironmentVariable("Discord_CommandPrefix") ?? Config.Discord.CommandPrefix;

        Config.SaveSetting();
    }

    public async Task RunBotAsync()
    {
        ILoggerFactory logFactory = new LoggerFactory().AddSerilog(logger);

        var cfg = new DiscordConfiguration
        {
            Token = Config.Discord.Token,
            TokenType = TokenType.Bot,

            AutoReconnect = true,
            LoggerFactory = logFactory
        };

        Client = new DiscordClient(cfg);

        Client.Ready += this.Client_Ready;
        Client.GuildAvailable += this.Client_GuildAvailable;
        Client.ClientErrored += this.Client_ClientError;

        var ccfg = new CommandsNextConfiguration
        {
            StringPrefixes = new[] { Config.Discord.CommandPrefix },

            EnableDms = false,

            EnableMentionPrefix = true
        };

        var slash = Client.UseSlashCommands();
        this.Commands = Client.UseCommandsNext(ccfg);

        this.Commands.CommandExecuted += this.Commands_CommandExecuted;
        this.Commands.CommandErrored += this.Commands_CommandErrored;

        slash.SlashCommandExecuted += Slash_SlashCommandExecuted;
        slash.SlashCommandErrored += Slash_SlashCommandErrored;
        slash.SlashCommandInvoked += Slash_SlashCommandInvoked;

        this.Commands.RegisterCommands<BotCommands>();
        slash.RegisterCommands<BotSlashCommands>();

        await Client.ConnectAsync();

        await Task.Delay(-1);
    }

    private Task Slash_SlashCommandInvoked(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandInvokedEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        e.Context.Client.Logger.LogInformation(BotEventId, "{Username} successfully invoked '{CommandName}'", e.Context.User.Username, e.Context.CommandName);

        return Task.CompletedTask;
    }

    private async Task Slash_SlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";
        InteractionContext ic = e.Context;

        ic.Client.Logger.LogError(BotEventId, "{Username} tried executing '{CommandName}' but it errored: {Type}: {Message}", e.Context.User.Username, ic.CommandName ?? "<unknown command>", e.Exception.GetType(), e.Exception.Message ?? "<no message>");

        if (e.Exception is ChecksFailedException)
        {
            var emoji = DiscordEmoji.FromName(ic.Client, ":no_entry:");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Access denied",
                Description = $"{emoji} You do not have the permissions required to execute this command.",
                Color = new DiscordColor(0xFF0000)
            };

            await ic.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{embed}"));
        }
    }

    private Task Slash_SlashCommandExecuted(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandExecutedEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        e.Context.Client.Logger.LogInformation(BotEventId, "{Username} successfully executed '{CommandName}'", e.Context.User.Username, e.Context.CommandName);

        return Task.CompletedTask;
    }

    private Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        sender.Logger.LogInformation(BotEventId, "Client is ready to process events.");

        return Task.CompletedTask;
    }

    private Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        sender.Logger.LogInformation(BotEventId, "Guild available: {GuildName}", e.Guild.Name);

        return Task.CompletedTask;
    }

    private Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");

        return Task.CompletedTask;
    }

    private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
    {
        e.Context.Client.Logger.LogInformation(BotEventId, "{Username} successfully executed '{QualifiedName}'", e.Context.User.Username, e.Command.QualifiedName);

        return Task.CompletedTask;
    }

    private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        e.Context.Client.Logger.LogError(BotEventId, "{Username} tried executing '{QualifiedName}' but it errored: {Type}: {Message}", e.Context.User.Username, e.Command?.QualifiedName ?? "<unknown command>", e.Exception.GetType(), e.Exception.Message ?? "<no message>");

        if (e.Exception is ChecksFailedException)
        {
            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Access denied",
                Description = $"{emoji} You do not have the permissions required to execute this command.",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.RespondAsync(embed);
        }
    }
}