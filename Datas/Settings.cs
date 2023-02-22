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

namespace SlashBot.Datas;

#nullable disable

public class Settings
{
    private Discord defaultDiscord = new();

    [JsonPropertyName("discord")]
    public Discord Discord
    {
        get => defaultDiscord;
        set
        {
            defaultDiscord = value;
        }
    }

    public Settings()
    {
        defaultDiscord = new Discord();
    }

    /// <summary>
    /// Saves the App settings in selected path.
    /// </summary>
    public void SaveSetting()
    {
        if (!Directory.Exists(Program.CurrentDir))
        {
            _ = Directory.CreateDirectory(Program.CurrentDir);
        }

        JsonSerializerOptions options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        File.WriteAllText(Program.SettingPath, JsonSerializer.Serialize(Program.Config, options));
    }

    /// <summary>
    /// Loads the App settings from the selected path.
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            string json_string = File.ReadAllText(Program.SettingPath);
            if (Json.IsValid(json_string))
            {
                JsonSerializerOptions options = new()
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                Program.Config = JsonSerializer.Deserialize<Settings>(json_string, options);
            }
            else
            {
                SaveSetting();
                LoadSettings();
            }
        }
        catch (Exception)
        {
            SaveSetting();
            LoadSettings();
        }
    }
}

public class Discord
{
    private string defaultToken = "<token>";
    private string defaultDefaultActivity = "Commands...";
    private ActivityType defaultActivityType = ActivityType.ListeningTo;

    public Discord()
    {
        Token = defaultToken;
        DefaultActivity = defaultDefaultActivity;
    }

    public Discord(string token, string defaultactivity)
    {
        Token = token;
        DefaultActivity = defaultactivity;
    }

    [JsonPropertyName("token")]
    [DefaultValue("<token>")]
    public string Token
    {
        get => defaultToken;
        set
        {
            defaultToken = value;
        }
    }

    [JsonPropertyName("defaultactivity")]
    [DefaultValue("Commands...")]
    public string DefaultActivity
    {
        get => defaultDefaultActivity;
        set
        {
            defaultDefaultActivity = value;
        }
    }

    [JsonPropertyName("defaultactivitytype")]
    [DefaultValue(ActivityType.ListeningTo)]
    public ActivityType DefaultActivityType
    {
        get => defaultActivityType;
        set
        {
            defaultActivityType = value;
        }
    }
}