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

internal class SlashBotHostedService(ILogger<SlashBotHostedService> logger, IHostApplicationLifetime applicationLifetime, IServiceProvider serviceProvider, DiscordClient client) : IHostedService
{
    private readonly ILogger<SlashBotHostedService> _logger = logger;
    private readonly IHostApplicationLifetime applicationLifetime = applicationLifetime;
    private readonly DiscordClient discordClient = client;
    private readonly IServiceProvider serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bot is starting...");

        CommandsExtension commandsExtension = serviceProvider.GetRequiredService<CommandsExtension>();

        commandsExtension.CommandExecuted += OnCommandExecuted;
        commandsExtension.CommandErrored += OnCommandErrored;

        await discordClient.ConnectAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bot is shutting down...");

        if (discordClient != null)
        {
            await discordClient.DisconnectAsync();
            discordClient.Dispose();
            
        }
    }

    private async Task OnCommandExecuted(CommandsExtension sender, DSharpPlus.Commands.EventArgs.CommandExecutedEventArgs e)
    {
        _logger.LogInformation(Program.BotEventId, "{Username} successfully executed {CommandName}", e.Context.User.Username, e.Context.Command.Name);

        await e.Context.Client.UpdateStatusAsync(new DiscordActivity()
        {
            ActivityType = Program.Config.Discord.DefaultActivityType,
            Name = Program.Config.Discord.DefaultActivity
        }, DiscordUserStatus.Online);
    }

    private async Task OnCommandErrored(CommandsExtension sender, DSharpPlus.Commands.EventArgs.CommandErroredEventArgs e)
    {
        if (e.Context is not null)
        {
            var context = e.Context;

            _logger.LogError(Program.BotEventId, "{Username} tried executing {CommandName} but it errored: {Type}: {Message}", e.Context.User.Username, e.Context?.Command.Name ?? "<unknown command>", e.Exception.GetType(), e.Exception.Message ?? "<no message>");

            if (e.Exception is ChecksFailedException)
            {
                var emoji = DiscordEmoji.FromName(context.Client, ":no_entry:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000)
                };
                await context.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"{embed}"));
            }

            await context.Client.UpdateStatusAsync(new DiscordActivity()
            {
                ActivityType = DiscordActivityType.Watching,
                Name = "Errors!"
            }, DiscordUserStatus.DoNotDisturb);
        }
    }
}