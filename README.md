# Dark Cloud 1 Randomizer

Home of Dark Cloud 1 randomizer for Archipelago

## Requirements
- Windows only for now
- Install PCSX2 1.7+  (2.6+ recommended)
- Legally obtained copy of Dark Cloud 1 NTSC version
- Install [Archipelago](https://archipelago.gg/tutorial/Archipelago/setup_en) 0.6.7 or later
- Download and install the dc1.apworld file from the latest release like normal ('run' the .apworld file)
- Download the client .exe from the latest release

## Running the game
- Generate a game with Archipelago
- Start the server
- Start Dark Cloud 1 on PCSX2
- Start the client
- Click the burger icon in the client, put in host/slot/password info and click connect. Make sure to connect before entering a dungeon!

## Things of note
- Save states are not 100% supported yet.  If you must use them, reset the game before loading the state.  Otherwise consider closing the client if save stating for perfect score on duels.
- For multiworlds, consider forcing "Progressive Gaffer's Buggy" and "Progressive Paige's House" local.  These are logically required for Xiao so this will ensure they are always in the first half of the DBC or Norune town chests so you don't have to wait long for a shop.
- Georama can be received in town, but buildings for the current town cannot yet. Need to go to another town or into a dungeon to get them for now.
- The .pnach file is optional.  It contains a mod for pcsx2 to show all georama tabs regardless if the player has been to the town.
- If you aren't seeing a miracle chest try these steps before posting:
  - Chests won't spawn until you've entered the local dungeon once, don't forget to do that first! Also, if not doing open dungeon, you'll need to progress the dungeon for certain chests to spawn as well.
  - Use R2 for first person camera.  Some chests are tricky to find
  - Go into a dungeon to receive georama.  It might be logically available on the tracker, but the required georama is missing in your inventory.
  - Some chests may show logically available but not be seen if you haven't recruited a character that is now available.
  - If the above fails, ask in the Dark Cloud thread

## Linux
There is an upstream patch pending which will allow DC1AP to work seamlessly on native Linux, but some extra steps are required in the meantime.

### Program Name
You MUST run the pcsx2-qt program and NOT the pcsx2.

If you are not sure which name your emulator is running under, you can check with
`sudo ps -axu | grep -i pcsx2-qt`

Your output should look something like this
```
$ sudo ps -axu | grep -i pcsx2-qt
2949467  7.8  0.1 5879308 113656 ?      Sl   19:09   0:00 pcsx2-qt
2949555  0.0  0.0   7544  4660 pts/0    S+   19:09   0:00 grep --color=auto -i pcsx2-qt
```
if you only see
```
2949555  0.0  0.0   7544  4660 pts/0    S+   19:09   0:00 grep --color=auto -i pcsx2-qt
```
You need to either invoke the emulator via directly `/usr/bin/pcsx2-qt` OR if you are using an AppImage, rename the appimage to `pcsx2-qt`.
The Flatpak version of PCSX2 is untested, and probably won't work due to sandboxing issues (at least not without major headaches), but you're welcome to try, there is an off chance it will work when the patch upstream for PCSX2 gets merged.

### Commands
To ensure the client can connect correctly on Linux, you'll need to run the following commands (or build PCSX2 from [this branch](https://github.com/Inertia-Squared/pcsx2/tree/linux-modding-support))
1. `sudo setcap -r /usr/bin/pcsx2-qt`
and
2. `echo 0 | sudo tee /proc/sys/kernel/yama/ptrace_scope`

- Command 1 applies to the binary and will be overwritten on a new build/install.
- Command 2 is ephemeral and will clear on reboot, but can also be made permanent with an additional command if you like to live life on the edge, but please don't.

**PLEASE NOTE** These commands, while benign, will reduce the overall security of your system and should be reversed when you are not using the client.
- Command 2 allows any program to read the memory of another program which is running under the same uid as it. It is not dangerous on its own, but can make a bad situation much worse if your machine does get compromised.
- Command 1 doesn't have any security implications, but makes changes to the binary which will break PCSX2's raw-socket networking on some Linux builds, so games which use network services may break until you revert the change.

### Reversing Compatibility Commands
Command 1 can be reversed by running:
`sudo setcap cap_net_admin,cap_net_raw=eip /usr/bin/pcsx2-qt`

Command 2 is reversed on reboot, or by swapping the 0 for a 1 and rerunning the command.
