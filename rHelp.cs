using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("rHelp", "Ftuoil Xelrash", "0.0.40")]
    [Description("Displays help information and server commands on join and via !help command")]

    public class rHelp : RustPlugin
    {
        #region Configuration

        private ConfigData config;
        private DateTime lastHelpCommandTime = DateTime.MinValue;
        private DateTime lastHelpProcessed = DateTime.MinValue;

        public class ConfigData
        {
            [JsonProperty("Settings")] public PluginSettings Settings;
        }

        public class PluginSettings
        {
            [JsonProperty("Enable !help Command")] public bool EnableHelpCommand = true;
            [JsonProperty("Command Cooldown (minutes)")] public float CommandCooldown = 5f;
            [JsonProperty("Show Help Command Info in Join Message")] public bool ShowHelpInJoin = true;
            [JsonProperty("Join Message Delay (seconds)")] public float JoinMessageDelay = 1f;

            [JsonProperty("Join Message")] public List<string> JoinMessage;
            [JsonProperty("Join Message Color")] public string JoinMessageColor = "00FFFF";  // Dark Blue
            [JsonProperty("Send Join Message to Console")] public bool SendJoinToConsole = false;

            [JsonProperty("Help Message - Title")] public string HelpMessageTitle = "Player Commands Guide";
            [JsonProperty("Help Message Color")] public string HelpMessageColor = "FFFF00";  // Yellow
            [JsonProperty("Send Help Message to Console")] public bool SendHelpToConsole = false;

            [JsonProperty("Help Message - Content")] public List<string> HelpMessageContent;
        }

        private List<string> GetDefaultJoinMessage()
        {
            return new List<string>
            {
                "Welcome to the server {player}!",
                "Type !help to see available commands and features available on {server}."
            };
        }

        private List<string> GetDefaultHelpContent()
        {
            return new List<string>
            {
                "",
                "ECONOMICS:",
                "/balance - Check your current Economics balance",
                "",
                "POPULATION:",
                "!pop - Show server statistics in chat",
                "  Online/sleeping players count",
                "",
                "RAIDABLE BASES:",
                "/buyraid - Buy a private Raidable Base!",
                "",
                "SCHEDULED RESTARTS & WIPES:",
                "!restart - Shows servers next scheduled restart in chat",
                "!wipe(COMING SOON!) - Shows servers next scheduled wipe date/time in chat",                
                "",
                "SIGN ARTIST:",
                "/sil <url> - Load image from URL onto sign you're looking at",
                "  Example: /sil https://example.com/image.jpg",
                "  Add raw for raw format",
                "/silt <message> - Create text on signs",
                "  Example: /silt Hello World",
                "/silrestore - Restore sign to original texture",
                "",
                "PLAYER STATS:",
                "/stats - Check your current player stats",
                ""
            };
        }

        protected override void LoadDefaultConfig()
        {
            config = new ConfigData();
            config.Settings = new PluginSettings();
            config.Settings.JoinMessage = GetDefaultJoinMessage();
            config.Settings.HelpMessageContent = GetDefaultHelpContent();
            SaveConfig();
            Puts("Default configuration created.");
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<ConfigData>();
                if (config == null || config.Settings == null)
                {
                    LoadDefaultConfig();
                    return;
                }
                if (config.Settings.JoinMessage == null)
                    config.Settings.JoinMessage = GetDefaultJoinMessage();
                if (config.Settings.HelpMessageContent == null)
                    config.Settings.HelpMessageContent = GetDefaultHelpContent();
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

            BasePlayer basePlayer = player.Object as BasePlayer;
            if (basePlayer == null)
                return;

            // Replace placeholders in join message
            string serverName = ConVar.Server.hostname ?? "Unknown Server";
            int onlinePlayers = BasePlayer.activePlayerList.Count;
            int maxPlayers = ConVar.Server.maxplayers;
            int sleepingPlayers = BasePlayer.sleepingPlayerList.Count;

            // Build the full join message from all lines
            string fullJoinMessage = "";
            string consoleJoinMessage = "";

            foreach (var line in config.Settings.JoinMessage)
            {
                string processedLine = line
                    .Replace("{player}", player.Name)
                    .Replace("{server}", serverName)
                    .Replace("{online_players}", onlinePlayers.ToString())
                    .Replace("{max_players}", maxPlayers.ToString())
                    .Replace("{sleeping_players}", sleepingPlayers.ToString())
                    .Replace("{player_count}", $"{onlinePlayers}/{maxPlayers}");

                fullJoinMessage += processedLine + "\n";
                consoleJoinMessage += processedLine + "\n";
            }

            // Remove trailing newline
            fullJoinMessage = fullJoinMessage.TrimEnd();
            consoleJoinMessage = consoleJoinMessage.TrimEnd();

            // Add color to message for chat
            string coloredMessage = $"<color=#{config.Settings.JoinMessageColor}>{fullJoinMessage}</color>";

            // Send to chat as one message
            basePlayer.ChatMessage(coloredMessage);

            // Send to console if enabled (plain text, no colors)
            if (config.Settings.SendJoinToConsole)
            {
                basePlayer.ConsoleMessage(consoleJoinMessage);
            }
        }

        private void OnPlayerChat(BasePlayer player, string message, ConVar.Chat.ChatChannel channel)
        {
            if (player == null || string.IsNullOrEmpty(message))
                return;

            if (message.ToLower() == "!help")
            {
                var now = DateTime.Now;
                // Skip if processed within last 10ms
                if ((now - lastHelpProcessed).TotalMilliseconds < 10)
                    return;
                lastHelpProcessed = now;

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

            // Process content lines with placeholder replacement
            var processedContent = new List<string>();
            foreach (var line in config.Settings.HelpMessageContent)
            {
                string processedLine = line
                    .Replace("{player}", player.displayName)
                    .Replace("{server}", serverName)
                    .Replace("{online_players}", onlinePlayers.ToString())
                    .Replace("{max_players}", maxPlayers.ToString())
                    .Replace("{sleeping_players}", sleepingPlayers.ToString())
                    .Replace("{player_count}", $"{onlinePlayers}/{maxPlayers}");

                processedContent.Add(processedLine);
            }

            // Build help message
            string headerLine = "=".PadRight(config.Settings.HelpMessageTitle.Length, '=');
            string helpMessage = config.Settings.HelpMessageTitle + "\n" + headerLine + "\n\n" + string.Join("\n", processedContent);
            string consoleHelpMessage = config.Settings.HelpMessageTitle + "\n" + headerLine + "\n\n" + string.Join("\n", processedContent);

            // Add color for chat
            string coloredHelpMessage = $"<color=#{config.Settings.HelpMessageColor}>{helpMessage}</color>";

            // Send to chat as one message
            player.ChatMessage(coloredHelpMessage);

            // Send to console if enabled (plain text, no colors)
            if (config.Settings.SendHelpToConsole)
            {
                player.ConsoleMessage(consoleHelpMessage);
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
