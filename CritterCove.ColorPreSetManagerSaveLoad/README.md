Color Preset Save/Load
======================

With this mod, your saved colors for the character creator are persisted across
sessions, and you are able to hand edit to load in hex colors directly.

Usage
-----

If you want to preseed your colors, go to your Critter Cove save folder
(usually `%USERPROFILE%\AppData\LocalLow\Gentleman Rat Studios\Critter Cove\GameSaves\{64BitSteamID}`
where `{64BitSteamID}` is your Steam account ID) and create a file called
`color_presets.json`. Open this in a text editor, and paste the following in it:

```json
{"character_creator":["000000","111111","222222","333333","444444","555555","666666"]}
```

For each of the hex numbers, replace it with the hex number for your desired
color. If you want to leave a slot empty, replace the hex number string with
`null` (e.g. replace `"666666"` with `null`, deleting the quote marks). You can
have up to 7 preset colors.

Steam may ask you whether to use cloud save or local save if you modify save
files while the game is not running. If the current device is the last device
you ran the game on, you should be safe to choose local save. Otherwise, you
may want to choose cloud save, exit the game after it's started, then try
modifying the presets, and choose local save when Steam prompts you again.
