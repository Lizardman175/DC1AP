using Archipelago.Core.Util;


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
        /// <param name="removeDefaultItem">Should always be true except on first init</param>
        /// <returns>True if the chest has been collected.</returns>
        internal bool CheckChest(bool removeDefaultItem)
        {
            if (!collected)
            {
                byte testByte = Memory.ReadByte(addr);
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
