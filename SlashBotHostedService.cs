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

internal class SlashBotHostedService(ILogger<SlashBotHostedService> logger, DiscordClient discord) : IHostedService
{
    private readonly ILogger<SlashBotHostedService> _logger = logger;
    private readonly DiscordClient _discordClient = discord;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _discordClient.SessionCreated += OnSessionCreated;
        _discordClient.GuildAvailable += OnGuildAvailable;
        //_discordClient.ClientErrored += OnClientError;

        CommandsExtension commandsExtension = _discordClient.UseCommands(new CommandsConfiguration()
        {
            DebugGuildId = 0,
            UseDefaultCommandErrorHandler = false,
            RegisterDefaultCommandProcessors = false
        });

        commandsExtension.CommandExecuted += OnCommandExecuted;
        commandsExtension.CommandErrored += OnCommandErrored;
        // Add all commands by scanning the current assembly
        commandsExtension.AddCommands(typeof(BotCommands).Assembly);

        List<ICommandProcessor> processors = [];
        SlashCommandProcessor slashCommandProcessor = new();
        slashCommandProcessor.AddConverters(typeof(BotCommands).Assembly);
        processors.Add(slashCommandProcessor);

        await commandsExtension.AddProcessorsAsync(processors);

        await _discordClient.ConnectAsync();

        await Task.Delay(-1);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _discordClient.DisconnectAsync();
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
        Thread.CurrentThread.Name = "MainThread";

        sender.Logger.LogInformation(Program.BotEventId, "Guild available: {GuildName}", e.Guild.Name);

        return Task.CompletedTask;
    }

    private Task OnClientError(DiscordClient sender, ClientErrorEventArgs e)
    {
        Thread.CurrentThread.Name = "MainThread";

        sender.Logger.LogError(Program.BotEventId, e.Exception, "Exception occured");

        return Task.CompletedTask;
    }

    private async Task OnCommandExecuted(CommandsExtension sender, DSharpPlus.Commands.EventArgs.CommandExecutedEventArgs e)
    {
        e.Context.Client.Logger.LogInformation(Program.BotEventId, "{Username} successfully executed '{CommandName}'", e.Context.User.Username, e.Context.Command.Name);

        await e.Context.Client.UpdateStatusAsync(new DiscordActivity()
        {
            ActivityType = Program.Config.Discord.DefaultActivityType,
            Name = Program.Config.Discord.DefaultActivity
        }, DiscordUserStatus.Online);
    }

    private async Task OnCommandErrored(CommandsExtension sender, DSharpPlus.Commands.EventArgs.CommandErroredEventArgs e)
    {
        e.Context.Client.Logger.LogError(Program.BotEventId, "{Username} tried executing '{CommandName}' but it errored: {Type}: {Message}", e.Context.User.Username, e.Context?.Command.Name ?? "<unknown command>", e.Exception.GetType(), e.Exception.Message ?? "<no message>");

        if (e.Exception is ChecksFailedException)
        {
            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Access denied",
                Description = $"{emoji} You do not have the permissions required to execute this command.",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"{embed}"));
        }

        await e.Context.Client.UpdateStatusAsync(new DiscordActivity()
        {
            ActivityType = DiscordActivityType.Watching,
            Name = "Errors!"
        }, DiscordUserStatus.DoNotDisturb);
    }
}