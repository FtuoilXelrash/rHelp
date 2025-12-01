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
- Join message content
- Help message content (multi-line supported)
- Command cooldown duration
- Enable/disable features

INSTALLATION:
1. Save rHelp.cs to your oxide/plugins/ directory
2. Restart server or use oxide.reload rHelp
3. Configure via the generated config file

NOTES:
- Messages should use plain text only (Rust limitations)
- Cooldown is global - all players share the same 5 minute timer
- Help messages are shown only to the player who runs the command
