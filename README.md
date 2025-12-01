# rHelp Plugin

A Rust server plugin that sends players a welcome message on join and provides a configurable help command with a global cooldown.

## Details

- **Plugin Name:** rHelp
- **Author:** Ftuoil Xelrash
- **Version:** 0.0.40
- **Description:** Displays help information and server commands on join and via !help command

## Features

- **Welcome Message** - Automatically sent to players when they join the server
- **Help Command** - Players can type `!help` to view available server commands
- **Global 5-Minute Cooldown** - Prevents spam; all players share the same cooldown timer
- **Private Messages** - Help responses are sent only to the player who requested them
- **Multi-line Configuration** - Easy-to-edit help message content in the config file
- **Plain Text Format** - Compatible with Rust's text limitations

## Commands

### !help
Shows the Player Commands Guide with all available server commands and features.

- **Cooldown:** 5 minutes (global)
- **Permission:** None required (available to all players)
- **Output:** Private message (only visible to the player)

## Configuration

The plugin creates a configuration file automatically on first run. You can customize:

- **Enable !help Command** - Toggle the command on/off
- **Command Cooldown (minutes)** - Set the cooldown duration
- **Join Message** - Multi-line message list shown when players connect (supports placeholders)
- **Join Message Color** - Color code for join message in chat (default: 00FFFF - dark blue)
- **Send Join Message to Console** - Send join message to F1 console (default: true)
- **Help Message - Title** - Title of the help guide
- **Help Message Color** - Color code for help message in chat (default: FFFF00 - yellow)
- **Send Help Message to Console** - Send help message to F1 console (default: true)
- **Help Message - Content** - Multi-line list of commands and info

### Default Help Message Includes:
- Player Stats (`/stats`)
- Economics (`/balance`)
- Raidable Bases (`/buyraid`)
- Sign Artist (`/sil`, `/silt`, `/silrestore`)
- Population (`!pop`)
- Scheduled Restarts (`!restart`)
- Quick Tips

All of these are fully customizable in the config file.

### String Placeholders

Both the **join message** and **help message** support the following placeholders that are automatically replaced:

- `{player}` - Name of the player joining
- `{server}` - Server name (from server hostname setting)
- `{online_players}` - Current number of online players
- `{max_players}` - Maximum player capacity
- `{sleeping_players}` - Number of players sleeping
- `{player_count}` - Formatted as "online/max" (e.g., "15/50")

**Single-Line Examples:**
- `"Welcome {player} to {server}!"`
- `"Welcome {player}! Server is at {player_count} capacity."`
- `"Welcome {player}! There are {online_players} players online."`
- `"Join us {player}! {online_players}/{max_players} slots filled on {server}."`

**Multi-Line Example (as configured in JSON):**
```json
"Join Message": [
  "Welcome to the server {player}!",
  "Type !help to see available commands and features available on {server}."
]
```

This will display as two separate lines in chat and console.

**Help Message Example with Placeholders:**
```json
"Help Message - Content": [
  "Server: {server}",
  "Players Online: {player_count}",
  "",
  "Welcome {player}!"
]
```

Placeholders work the same way in help messages as they do in join messages.

### Color Codes

Colors are specified as hex codes (without the # symbol). Some examples:
- `00FFFF` - Cyan (dark blue)
- `FFFF00` - Yellow
- `FF0000` - Red
- `00FF00` - Green
- `FFFFFF` - White
- `FFA500` - Orange

The full message will be displayed in the specified color in chat.

### Console Messages

Both join and help messages can be sent to the F1 console for easier reading. This is enabled by default and can be toggled in the config:
- `Send Join Message to Console` - (default: true)
- `Send Help Message to Console` - (default: true)

## Installation

1. Save `rHelp.cs` to your `oxide/plugins/` directory
2. Restart the server or use `oxide.reload rHelp` command
3. Configure via the generated config file (edit as needed)
4. Reload the plugin again to apply config changes

## Configuration File Location

The config file is automatically generated at:
```
oxide/config/rHelp.json
```

## Notes

- Messages should use plain text only (Rust has limitations with special characters)
- The cooldown is **global** - once the !help command is used, all players must wait 5 minutes
- Join messages appear as private messages to each connecting player
- The help message can be as long as needed with multiple lines

## Technical Details

- Uses `OnUserConnected` hook for join messages
- Uses `OnPlayerChat` hook to detect !help commands
- Global `DateTime` tracking for cooldown (matches rPop pattern)
- JSON configuration file for easy editing
- Proper cooldown time formatting (seconds/minutes/hours)
- Fixed JSON deserialization issue that was causing duplicate message content in loaded configurations
