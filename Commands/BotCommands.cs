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

using DescriptionAttribute = DSharpPlus.CommandsNext.Attributes.DescriptionAttribute;

namespace SlashBot.Commands;

public class BotCommands : BaseCommandModule
{
    [Command("table_flip"), Description("Post a table flip")]
    [RequireGuild()]
    [RequireUserPermissions(Permissions.SendMessages)]
    public async Task TableFlip(CommandContext ctx)
    {
        try
        {
            string flip = Program.TableFlips[new Random().Next(Program.TableFlips.Length)];
            await ctx.Message.RespondAsync(flip);
        }
        catch (Exception ex) { await ctx.RespondAsync($"An exception occured during playback: `{ex.GetType()}: {ex.Message}`"); }
    }
}