using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("rHelp", "Ftuoil Xelrash", "0.0.1")]
    [Description("Displays help information and server commands on join and via !help command")]

    public class rHelp : RustPlugin
    {
        #region Configuration

        private ConfigData config;
        private DateTime lastHelpCommandTime = DateTime.MinValue;

        public class ConfigData
        {
            [JsonProperty("Settings")] public PluginSettings Settings = new PluginSettings();
        }

        public class PluginSettings
        {
            [JsonProperty("Enable !help Command")] public bool EnableHelpCommand = true;
            [JsonProperty("Command Cooldown (minutes)")] public float CommandCooldown = 5f;
            [JsonProperty("Show Help Command Info in Join Message")] public bool ShowHelpInJoin = true;

            [JsonProperty("Join Message")] public string JoinMessage = "Welcome to the server! Type !help to see available commands and features.";

            [JsonProperty("Help Message - Title")] public string HelpMessageTitle = "Player Commands Guide";

            [JsonProperty("Help Message - Content")] public List<string> HelpMessageContent = new List<string>
            {
                "",
                "PLAYER STATS:",
                "/stats - Check your current player stats",
                "",
                "ECONOMICS:",
                "/balance - Check your current balance",
                "",
                "RAIDABLE BASES:",
                "/buyraid - Buy a private Raidable Base!",
                "",
                "SIGN ARTIST:",
                "/sil <url> - Load image from URL onto sign you're looking at",
                "  Example: /sil https://example.com/image.jpg",
                "  Add raw for raw format",
                "/silt <message> - Create text on signs",
                "  Example: /silt Hello World",
                "  Optional: font size, colors, background",
                "/silrestore - Restore sign to original texture",
                "Multi-texture signs (shop fronts):",
                "  /sil <1-4> <url> - Load to specific texture",
                "  /silt <1-4> <message> - Text to specific texture",
                "",
                "POPULATION:",
                "!pop - Show server statistics in chat",
                "  Online/sleeping players count",
                "  Has cooldown between uses",
                "",
                "SCHEDULED RESTARTS & WIPES:",
                "!restart - Show server next scheduled restart in chat",
                "  Has cooldown between uses",
                "",
                "QUICK TIPS:",
                "- Check /balance before buying raids",
                "- Use !pop to see server activity",
                "- Practice /silt on wooden signs"
            };
        }

        protected override void LoadDefaultConfig()
        {
            config = new ConfigData();
            SaveConfig();
            Puts("Default configuration created.");
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<ConfigData>();
                if (config == null)
                {
                    LoadDefaultConfig();
                    return;
                }
            }
            catch (Exception ex)
            {
                PrintError($"Error loading configuration: {ex.Message}");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }

        #endregion

        #region Hooks

        private void Init()
        {
            LoadConfig();
        }

        private void OnUserConnected(IPlayer player)
        {
            if (!config.Settings.EnableHelpCommand)
                return;

            // Send join message
            player.Reply(config.Settings.JoinMessage);
        }

        private void OnPlayerChat(BasePlayer player, string message, ConVar.Chat.ChatChannel channel)
        {
            if (message.ToLower() == "!help")
            {
                HandleHelpCommand(player);
            }
        }

        #endregion

        #region Help Command

        private void HandleHelpCommand(BasePlayer player)
        {
            if (!config.Settings.EnableHelpCommand)
            {
                player.ChatMessage("The !help command is currently disabled.");
                return;
            }

            var now = DateTime.Now;
            var timeSinceLastUse = now - lastHelpCommandTime;

            if (timeSinceLastUse.TotalMinutes < config.Settings.CommandCooldown)
            {
                var remainingTime = TimeSpan.FromMinutes(config.Settings.CommandCooldown) - timeSinceLastUse;
                player.ChatMessage($"Help command is on cooldown. Try again in {GetCooldownTime(remainingTime)}.");
                return;
            }

            lastHelpCommandTime = now;

            // Build and send help message
            string helpMessage = $"{config.Settings.HelpMessageTitle}\n";
            helpMessage += "=".PadRight(config.Settings.HelpMessageTitle.Length, '=') + "\n\n";

            foreach (var line in config.Settings.HelpMessageContent)
            {
                helpMessage += line + "\n";
            }

            player.ChatMessage(helpMessage);
        }

        private string GetCooldownTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalSeconds < 60)
                return $"{(int)timeSpan.TotalSeconds} seconds";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")}";
            else
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")}";
        }

        #endregion
    }
}
