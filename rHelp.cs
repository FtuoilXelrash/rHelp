using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("rHelp", "Ftuoil Xelrash", "0.0.20")]
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
            [JsonProperty("Join Message Delay (seconds)")] public float JoinMessageDelay = 1f;

            [JsonProperty("Join Message")] public string JoinMessage = "Welcome to the server {player}!\nType !help to see available commands and features available on {server}.";
            [JsonProperty("Join Message Color")] public string JoinMessageColor = "00FFFF";  // Dark Blue
            [JsonProperty("Send Join Message to Console")] public bool SendJoinToConsole = true;

            [JsonProperty("Help Message - Title")] public string HelpMessageTitle = "Player Commands Guide";
            [JsonProperty("Help Message Color")] public string HelpMessageColor = "FFFF00";  // Yellow
            [JsonProperty("Send Help Message to Console")] public bool SendHelpToConsole = true;

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

            timer.Once(config.Settings.JoinMessageDelay, () =>
            {
                if (player == null || !player.IsConnected)
                    return;

                BasePlayer basePlayer = player.Object as BasePlayer;
                if (basePlayer == null)
                    return;

                // Replace placeholders in join message
                string serverName = ConVar.Server.hostname ?? "Unknown Server";
                int onlinePlayers = BasePlayer.activePlayerList.Count;
                int maxPlayers = ConVar.Server.maxplayers;
                int sleepingPlayers = BasePlayer.sleepingPlayerList.Count;

                string joinMessage = config.Settings.JoinMessage
                    .Replace("{player}", player.Name)
                    .Replace("{server}", serverName)
                    .Replace("{online_players}", onlinePlayers.ToString())
                    .Replace("{max_players}", maxPlayers.ToString())
                    .Replace("{sleeping_players}", sleepingPlayers.ToString())
                    .Replace("{player_count}", $"{onlinePlayers}/{maxPlayers}");

                // Add color to message for chat
                string coloredMessage = $"<color=#{config.Settings.JoinMessageColor}>{joinMessage}</color>";

                // Send to chat using BasePlayer.ChatMessage
                basePlayer.ChatMessage(coloredMessage);

                // Send to console if enabled (plain text, no colors)
                if (config.Settings.SendJoinToConsole)
                {
                    basePlayer.ConsoleMessage(joinMessage);
                }
            });
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

            // Get placeholder values for help message
            string serverName = ConVar.Server.hostname ?? "Unknown Server";
            int onlinePlayers = BasePlayer.activePlayerList.Count;
            int maxPlayers = ConVar.Server.maxplayers;
            int sleepingPlayers = BasePlayer.sleepingPlayerList.Count;

            // Build help message with placeholder replacement
            string helpMessage = $"{config.Settings.HelpMessageTitle}\n";
            helpMessage += "=".PadRight(config.Settings.HelpMessageTitle.Length, '=') + "\n\n";

            foreach (var line in config.Settings.HelpMessageContent)
            {
                string processedLine = line
                    .Replace("{player}", player.displayName)
                    .Replace("{server}", serverName)
                    .Replace("{online_players}", onlinePlayers.ToString())
                    .Replace("{max_players}", maxPlayers.ToString())
                    .Replace("{sleeping_players}", sleepingPlayers.ToString())
                    .Replace("{player_count}", $"{onlinePlayers}/{maxPlayers}");

                helpMessage += processedLine + "\n";
            }

            // Add color for chat
            string coloredHelpMessage = $"<color=#{config.Settings.HelpMessageColor}>{helpMessage}</color>";

            // Send to chat
            player.ChatMessage(coloredHelpMessage);

            // Send to console if enabled
            if (config.Settings.SendHelpToConsole)
            {
                player.ConsoleMessage(helpMessage);
            }
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
