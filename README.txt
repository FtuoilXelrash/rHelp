rHelp Plugin - Text Documentation
==================================

PLUGIN NAME: rHelp
AUTHOR: Ftuoil Xelrash
VERSION: 0.0.1
DESCRIPTION: Sends players a help message on join and provides a !help command with cooldown

FEATURES:
- Welcome message sent to players when they join the server
- Informs players about the !help command
- !help command displays configurable help information
- Global 5-minute cooldown on !help command
- Private messages (only player and server see them)
- Multi-line message support in configuration

COMMANDS:
- !help - Shows the help message (5 minute cooldown)

CONFIGURATION:
The plugin automatically creates a configuration file. You can customize:
- Join message content (MULTI-LINE SUPPORTED - supports placeholders below)
- Join message color (default: 00FFFF - dark blue)
- Help message title and content (multi-line supported)
- Help message color (default: FFFF00 - yellow)
- Command cooldown duration
- Enable/disable console output for messages (F1 console)
- Enable/disable features

SUPPORTED PLACEHOLDERS (for join message):
The following variables can be used in the join message and will be automatically replaced:
- {player} - Name of the player joining
- {server} - Server name (from server hostname setting)
- {online_players} - Current number of online players
- {max_players} - Maximum player capacity
- {sleeping_players} - Number of players sleeping
- {player_count} - Formatted as "online/max" (e.g., "15/50")

SINGLE-LINE EXAMPLES:
- "Welcome {player} to {server}!"
- "Welcome {player}! Server is at {player_count} capacity."
- "Welcome {player}! There are {online_players} players online."
- "Join us {player}! {online_players}/{max_players} slots filled on {server}."

MULTI-LINE EXAMPLE (in config JSON):
"Join Message": [
  "Welcome to the server {player}!",
  "Type !help to see available commands and features available on {server}."
]

This will display as two separate lines in both chat and F1 console.

INSTALLATION:
1. Save rHelp.cs to your oxide/plugins/ directory
2. Restart server or use oxide.reload rHelp
3. Configure via the generated config file

NOTES:
- Messages should use plain text only (Rust limitations)
- Cooldown is global - all players share the same 5 minute timer
- Help messages are shown only to the player who runs the command
