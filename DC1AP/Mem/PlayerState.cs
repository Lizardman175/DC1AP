using Archipelago.Core.Util;
using DC1AP.Constants;

namespace DC1AP.Mem
{
    /// <summary>
    /// Class for accessing player state memory values
    /// </summary>
    public static class PlayerState
    {
        public static bool ValidGameState = false;

        /// <summary>
        /// Game is loaded and ready to connect with.
        /// </summary>
        /// <returns></returns>
        public static bool PlayerReady()
        {
            // Player name is only set when a game is started/loaded and the client accepts the save is valid for the slot.  Otherwise, Toan will have no name.
            return ValidGameState && Memory.ReadByte(MiscAddrs.PlayerState) > 1;
        }

        /// <summary>
        /// Determines if the player is in a state in a dungeon that we can give an item/display a message.
        ///  Can't receive in georama menu as it will conditionally overwrite our changes.
        ///  Want to be out of menus to show a dialogue as well.
        /// </summary>
        /// <returns>True if safe to give the player an item.</returns>
        public static bool CanGiveItemDungeon()
        {
            return PlayerReady() && IsPlayerInDungeon() && Memory.ReadByte(MiscAddrs.DungeonMode) == 1;
        }

        /// <summary>
        /// Determines if the player is in a state in a town or dungeon that we can give an item/display a message.
        ///  Want to be out of menus to show a dialogue as well.
        /// </summary>
        /// <returns>True if safe to give the player an item.</returns>
        public static bool CanGiveItem()
        {
            return PlayerReady() && ((IsPlayerInDungeon() && Memory.ReadByte(MiscAddrs.DungeonMode) == 1) ||
                                     PlayerMovableTown());
        }

        public static bool PlayerMovableTown()
        {
            byte townState = Memory.ReadByte(MiscAddrs.PlayerTownState);
            return townState == MiscAddrs.PlayerTownOverworld ||
                (townState == MiscAddrs.PlayerTownInterior && Memory.ReadByte(MiscAddrs.PlayerInteriorState) == 0x00);
        }
        
        public static bool IsPlayerInDungeon()
        {
            return Memory.ReadByte(MiscAddrs.InDungeonFlag) != 0xFF;
        }

        public static bool IsPlayerInTown()
        {
            return PlayerReady() && Memory.ReadByte(MiscAddrs.PlayerState) == 2;
        }
    }
}
