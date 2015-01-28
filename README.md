# League Scrambler

## Download

Downloads can be found in the releases tab, or you can download the newest version [from here.](https://github.com/MythicManiac/League-of-Legends/releases/download/v1.0.1/League.Scrambler.v1.0.1.zip)

## Usage

There are several things you should make sure before launching the program.
* Disable Peer to Peer filetransfer from the patcher. It seems that sometimes the files which will be scrambled are in use by some other process, and my guess would be the patcher is responsible for that. Disabling peer to peer transfer from the patcher should prevent this from happening.
* Make sure you don't have League of Legends running. The lobby should be fine, but you can't be in game.
* In case you want to be completely safe, you should have ~~3Gb of free space so you can back up your game (Can be done automatically).

It doesn't matter where you extract the program, but just make sure the two provided .dll files can be found in the same directory.
When you launch the program, it will attempt to find your League of Legends installation location. If it can't do that automatically, you will be prompted to select your launcher's location.
After finding the installation folder of your League of Legends, it will load the information it needs from the files. If the program crashesh during this point, refer to the troubleshooting section.

When the program is fully booted up, you will be faced with a menu like this:
```
Select an option
1 - Backup files
2 - Scramble files
3 - Restore files (Work changes backwards)
4 - Restore files from backup (Copy backed up files over the current ones)
5 - Settings
6 - Exit
```

Here's what each option does in detail.

1. Backs up all of the files this program might modify into a safe location. It is recommended to take a backup before scrambling in case something goes wrong.
2. Scrambles your League of Legends files with the settings you've applied at Options.
3. Quickly reverts the changes the scrambler has made. This is the recommended way of reverting changes, as it's extremely fast and should work unless something has gone wrong.
4. Restores your League of Legends files from the backup you've taken with option 1. It should be obvious the files can't be restored if no backup has been taken.
5. Opens up a menu with several settings regarding what game assets will be scrambled.
6. Exits the application. You may as well just hit the close button at top right.

Generally, you want to take a backup before making any changes. If you happen to have around 3Gb of space, you should take the backup now.

Before you scramble the files, it would be a good idea to check out the settings from menu 5. Currently some ability icons get mixed in with shop icons and vice verca, but this will be fixed later on.

To scramble the files, simply hit option 2. The scrambling should take from 1 second to 10 seconds depending on your computers performance.

When starting the scramble, you will be prompted with a prefilled input box asking for a random seed. This is to be used if you want to scramble your files in the same way as someone else.

After you've done scrambling, feel free to scramble as many times as you want, no need to restore original files before doing so, as this is done automatically.

Once you're done playing with scrambled league, just use the option 3. If everything goes as it should, your League should be back to normal. If it isn't and you have a backup, you can restore that with option 4.

### Getting your league scrambled the same way as your friend(s)

In case you're playing with a friend or few who are using the scrambler too, you might want to make your files get scrambled in the same way. Luckily, there's a way to do this.

One of your friends should scramble his or her files first, and when prompted with the seed, copy that seed and send it to the others. Be sure to actually click that window first because just hitting Ctrl+C will end up killing the scrambler!

After one of you have scrambled the files and shared the seed, others can follow by copying that seed and pasting it when they're prompted to.

**Something that should be noted:** To get your files scrambled in the same way, you **MUST** use the same settings. Having even one option differently might end up you having a completely differently looking game.

## Troubleshooting

In case you face an error, the first thing you should try is to run the program as an administrator. If that doesn't help, make sure no other process is using the League of Legends files.

You could bring up your task manager and see if you have "League of Legends.exe" running, because that process is the game itself and obviously uses the game files.

Another thing that might be using your files is the `LoLPatcher.exe`, especially if you haven't unticked the peer to peer setting from the patcher settings.

If you still face the error after closing those two processes, quit your League of Legends completely and kill any related processes.

If nothing above helps you to get the scrambler working, feel free to start an issue here at github. Make sure to post your error from the log, though! (Logfile location is `League of Legends\Mythic\Log.txt`)

## General information

* All of the files used by this program will be placed inside a folder named `Mythic`, which is inside your League of Legends installation folder (So generally `League of Legends\Mythic`. Touching any of these files (besides the `Log.txt` file) might break the program.
* If you happen to get a really good scramble you'd like to share or see again, all the seeds used will be printed to the log located in the previously mentioned folder. It would be a good idea to go find it before you scramble again to avoid losing it.