using Archipelago.Core.Util;
using DC1AP.Constants;


namespace DC1AP.Items
{
    internal class MiracleChest(long locationId, uint addr, byte mask)
    {
        private readonly long locationId = locationId;
        private readonly uint addr = addr;
        private readonly byte mask = mask;
        private bool collected = false;

        /// <summary>
        /// Checks if the chest has been collected and optionally sends the location check to the server.
        /// If sendIfCollected is true, also remove the chest's normal item from the player's inventory.
        /// </summary>
        /// <returns>True if the chest has been collected.</returns>
        internal bool CheckChest()
        {
            if (!collected)
            {
                byte testByte = Memory.ReadByte(addr);

                // Edge case for the Horned Key chest in D's windmill.  The flag will be set until the chest is
                // accessible, but the 2 bit will also be set.
                if (locationId == MiscConstants.HornedKeyChestId && (testByte & 0x02) > 0)
                {
                    return false;
                }
                
                if ((testByte & mask) > 0)
                {
                    collected = true;

                    if (!App.Client.CurrentSession.Locations.AllLocationsChecked.Contains(locationId))
                    {
                        App.SendLocation((int)locationId);
                    }
                }
            }

            return collected;
        }
    }
}
