Auto Restock

A mod for Supermarket Simulator that lets you instantly stock shelves from a held box, and toss the empty box in the trash — all from one configurable key.

It's a revival of the old AutoRayon mod, rebuilt for the current version of the game.


What it does


Hold an open box and press a key (default Y) to instantly place its products onto every matching labeled shelf slot.
Press the key again while holding the now-empty box to throw it into the nearest trash bin. (Uses the game's own trash mechanics, so the janitor still works as normal.)


Features


One-key instant restocking from a held box to labeled shelves.
Optional one-key trashing of empty boxes (can be turned off).
Configurable activation key.


Requirements


BepInEx 6 (the IL2CPP build), installed into your Supermarket Simulator folder.


Installation


Install BepInEx 6 (IL2CPP) if you don't already have it.
Drop AutoRestock.dll into Supermarket Simulator\BepInEx\plugins.
Launch the game once to generate the config file.


Configuration

After running the game once, settings live in BepInEx\config\AutoRestock.cfg:


RestockKey — the key that triggers restocking / trashing. Default: Y.
TrashEmptyBox — set to false to disable throwing empty boxes in the trash (useful if it conflicts with another mod, such as New Box Spawner). Default: true.


You can edit these by hand in the .cfg file, or with an in-game config manager.

Building from source


Open AutoRestock.csproj.
Edit the single <GameDir> line near the top to point at your own Supermarket Simulator install folder.
Build. The compiled AutoRestock.dll appears in bin\Debug\net6.0\.


A note of honesty

I'm not a programmer. This mod was built with AI assistance. It works great in my own game, but that also means I can't reliably troubleshoot or patch bugs if something breaks for you. The source is here for anyone who'd like to read it, learn from it, or improve it.

Credits


The original AutoRayon author, FlowHelyanwe137, whose mod inspired this one.
Built with AI assistance.


License

Released under the MIT License — do whatever you'd like with it.
