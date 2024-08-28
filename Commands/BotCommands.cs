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

namespace SlashBot.Commands;

public class BotCommands
{
    [Command("table_flip"), Description("Post a table flip")]
    [RequireGuild()]
    [RequirePermissions(DiscordPermissions.SendMessages)]
    public static async Task TableFlip(CommandContext context)
    {
        try
        {
            string flip = Program.TableFlips[new Random().Next(Program.TableFlips.Length)];
            await context.RespondAsync(new DiscordInteractionResponseBuilder().WithContent(flip));
        }
        catch (Exception ex)
        {
            await context.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"An exception occured: `{ex.GetType()}: {ex.Message}`"));
        }
    }

    [Command("ping")]
    [Description("Replies with Pong and Discord Websocket latency for Client to your ping")]
    public static async Task Ping(CommandContext context)
    {
        if (context.Guild is not null)
        {
            await context.RespondAsync(new DiscordInteractionResponseBuilder()
            {
                Content = $"Pong! Discord Websocket latency for Client is {context.Client.GetConnectionLatency(context.Guild.Id).Milliseconds}ms.",
                IsEphemeral = true
            });
        }
        else
        {
            await context.RespondAsync(new DiscordInteractionResponseBuilder().WithContent($"Guild ID Null."));
        }
    }

    [Command("test")]
    [Description("Testing 1234")]
    public static async Task Test(CommandContext context)
    {
        Console.WriteLine("Test command ran");

        await context.RespondAsync(new DiscordInteractionResponseBuilder()
        {
            Content = "Hello, world!",
            IsEphemeral = true
        });
    }
}