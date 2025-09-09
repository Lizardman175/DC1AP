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
        /// <returns>True if safe to give the player an item & message.</returns>
        public static bool CanGiveItemDungeon()
        {
            // TODO currently only know how to display messages in dungeons.  Similar idea for towns I assume, but still working it out.  Limiting messages to in dungeon for now.
            // TODO .STATE might be the better flag to check here instead of IN_DUNGEON
            // TODO need better check for player entering dungeon floor animation
            return ValidGameState && IsPlayerInDungeon() && Memory.ReadByte(MiscAddrs.DungeonMode) == 1;
        }

        /// <summary>
        /// Determines if the player is in a state in a town that we can give an item/display a message.
        ///  Can't receive in georama menu as it will conditionally overwrite our changes.
        ///  Want to be out of menus to show a dialogue as well.
        /// </summary>
        /// <returns>True if safe to give the player an item & message.</returns>
        public static bool CanGiveItemTown()
        {
            // TODO figure out how to give a message in town.  Probably some code in DC Improved
            //return Memory.ReadByte(MiscAddr.IN_DUNGEON) == -1 && 
            return false;
        }
        
        public static bool IsPlayerInDungeon()
        {
            return Memory.ReadByte(MiscAddrs.InDungeonFlag) != 0xFF;
        }
    }
}
