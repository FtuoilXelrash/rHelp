# rHelp Plugin

A Rust server plugin that sends players a welcome message on join and provides a configurable help command with a global cooldown.

## Details

- **Plugin Name:** rHelp
- **Author:** Ftuoil Xelrash
- **Version:** 0.0.1
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
- **Join Message** - Message shown when players connect
- **Help Message - Title** - Title of the help guide
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
