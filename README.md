<h1 align="center">
  <br>
  <img src="./docs/images/valheimtooler_logo.png" alt="Valheim Tooler" width="300">
</h1>

<p align="center">
  <a href="https://github.com/Astropilot/ValheimTooler/release">
    <img src="https://img.shields.io/github/downloads/Astropilot/ValheimTooler/total"
         alt="Download counter">
  </a>
  <img src="https://img.shields.io/github/v/tag/Astropilot/ValheimTooler">
  <a href="https://github.com/Astropilot/ValheimTooler/blob/master/LICENSE">
    <img src="https://img.shields.io/github/license/Astropilot/ValheimTooler"
         alt="MIT License">
  </a>
  <img src="https://img.shields.io/badge/Made%20with-%E2%9D%A4%EF%B8%8F-red.svg">
</p>

<p align="center">
    <a href="https://github.com/Astropilot/ValheimTooler/blob/master/README-FR.md">French Version <img src="https://cdn.countryflags.com/thumbs/france/flag-800.png" height="16"></a>
</p>

<p align="center">
  <a href="#about">About</a> •
  <a href="#usage">Usage</a> •
  <a href="#contributing">Contributing</a> •
  <a href="#special-mentions-and-credits">Special mentions and credits</a>
</p>

## About

ValheimTooler is a free software that allows you to cheat on the Valheim game via a multitude of options offered. This project is purely for educational purposes, I am not responsible for the use you make of it. Be reasonable some features are particularly game breaker.

Here is the list of features offered by this tool:

* **Player**
  * God Mode, you don't lose any more life
  * Unlimited stamina for you
  * Unlimited stamina for the other players
  * No stamina for the other players
  * Fly Mode, sweet creative mode
  * Ghost Mode, the monsters cannot see you
  * No placement cost
  * Explore all your minimap (Irreversible on the world on which it is activated!)
  * Teleport to another player (works only if the player is visible on the minimap)
  * Instantly heals a player
  * Instantly heals all players
  * Activate a Guardian Power for you
  * Activate a Guardian Power for all players
  * Raise/Decrease a skill to any level
* **Entities & Items**
  * Spawn any entity
  * Delete all drops on the ground
  * An Item Giver to add any item you want in your inventory
* **Miscellaneous**
  * Inflict damage to a player (ignores the no-pvp mode)
  * Kill all entities except players
  * Kill all players
  * Send a event message to all players (the yellow one on the middle of the screen)
  * Send a chat message as any username
  * A simple ESP

**⚠️ Warning ⚠️**: On each feature that allows you to choose a player, the list will only include players who are at a certain distance from you (quite large but not the whole map for all that). This is not a personal choice but a technical limitation of the game. Until I find a way to get around it (if possible) this behavior will remain.

## Usage

I made sure that you can install and run this tool easily. Just [go here](https://github.com/Astropilot/ValheimTooler/release) to download the latest version. Take the .zip and extract it to any folder. Then run the `ValheimToolerLauncher.exe` and click on `Install`.

If the button is not available it is likely that the program has not found the path to the game, specify it just below and the button should become clickable.

Once the installation is complete, run the game with the launcher open. Once you get to the main menu (AND NOT BEFORE) of the game you can click on `Launch` and the cheat should appear in game. To show/hide the cheat window you can press `Del` on your keyboard.

When a cheat update is available, the launcher allows you to update the cheat directly. A button will appear and you just have to click on it to automatically install the latest version!

If you want to uninstall the cheat you just have to press the `Uninstall` button.

**Note**: If the installation or uninstallation ever prevents the game from launching, you can always ask Steam to check the integrity of the game files and it will repair the corrupted files.

Here is also a video tutorial explaining its installation and a demonstration of its features:

<p align="center">
<a href="https://www.youtube.com/watch?feature=player_embedded&v=YOUTUBE_VIDEO_ID_HERE" target="_blank">
  <img src="https://img.youtube.com/vi/YOUTUBE_VIDEO_ID_HERE/0.jpg" width="240" height="180" border="10">
</a>
</p>

## Contributing

The project is open for contributions! Feel free to open merge requests.
Please respect the rules in editorconfig!

**Important**: This program aims to keep it simple (I initially coded it only to learn how to develop cheats on Unity games), that's why I won't go into modifying the game methods but only calling or modifying the class variables. So it is not planned to use libraries like Harmony to patch methods even if it reduces the possibilities of features.

## Special mentions and credits

* Thanks to the [Guided Hacking](https://guidedhacking.com/threads/how-to-hack-unity-games-using-mono-injection-tutorial.11674/) and [Unknown Cheats](https://www.unknowncheats.me/forum/unity/285864-beginners-guide-hacking-unity-games.html) forums for their tutorials that gave me the basics to design an injectable cheat
* Many thanks to [wh0am15533](https://github.com/wh0am15533) for its [Unity Runtime DevTools](https://www.unknowncheats.me/forum/unity/388951-unity-runtime-devtools-v1-01-a.html) utility from which I got some parts related to resource loading in the assembly and its [SharpMonoInjector](https://github.com/wh0am15533/SharpMonoInjector) update
* Many thanks to [BastienMarais](https://github.com/BastienMarais) for his precious help during the development phase for his tests and feature ideas!
