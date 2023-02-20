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

global using DSharpPlus;
global using DSharpPlus.CommandsNext;
global using DSharpPlus.CommandsNext.Attributes;
global using DSharpPlus.CommandsNext.Exceptions;
global using DSharpPlus.Entities;
global using DSharpPlus.EventArgs;
global using DSharpPlus.SlashCommands;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using Serilog;
global using Serilog.Core;
global using SlashBot.Commands;
global using SlashBot.Datas;
global using SlashBot.Utils;
global using System;
global using System.ComponentModel;
global using System.IO;
global using System.Text.Json;
global using System.Text.Json.Nodes;
global using System.Text.Json.Serialization;

namespace SlashBot;