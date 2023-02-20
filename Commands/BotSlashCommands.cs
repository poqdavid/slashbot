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

namespace SlashBot.Commands;

public class BotSlashCommands : ApplicationCommandModule
{
    [SlashCommand("table_flip", "Post a table flip")]
    [RequireGuild()]
    [RequireUserPermissions(Permissions.SendMessages)]
    public async Task TableFlip(InteractionContext ctx)
    {
        try
        {
            string flip = Program.TableFlips[new Random().Next(Program.TableFlips.Length)];
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(flip));
        }
        catch (Exception ex) { await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"An exception occured: `{ex.GetType()}: {ex.Message}`")); }
    }
}