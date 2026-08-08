using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Mem;
using System.Collections.ObjectModel;


namespace DC1AP.Items
{
    internal class Fish
    {
        private const int MaxFishLog = 20;

        /// <summary>
        /// Checks the Fishing Log for any caught fish that haven't been sent
        /// </summary>
        internal static void CheckFishLog()
        {
            if (Options.FishSanity == 0) return;

            ReadOnlyCollection<long> locations = App.Client.CurrentSession.Locations.AllMissingLocations;
            uint addr = ItemValues.FirstFishLogAddr;

            for (int i = 0; i < MaxFishLog; i++)
            {
                int fishId = Memory.ReadInt(addr);
                if (fishId == 0)
                {
                    short size = Memory.ReadShort(addr + ItemValues.FishLogSizeOffset);
                    if (size == 0) break;
                }

                int fishApId = ItemValues.FishLogToApId[fishId];
                if (locations.Contains(fishApId))
                    App.SendLocation(fishApId);

                addr += ItemValues.FishLogEntryOffset;
            }
        }

        internal static void WatchFishCatchField()
        {
            if (FishChecksRemain())
                Memory.MonitorAddressForAction<int>(ItemValues.FishCatchAddr, CheckFishCatch, (o) => { return o != -1; });
        }

        /// <summary>
        /// Don't bother watching fish catches if they have all been caught.
        /// </summary>
        /// <returns></returns>
        private static bool FishChecksRemain()
        {
            ReadOnlyCollection<long> locations = App.Client.CurrentSession.Locations.AllMissingLocations;
            foreach (int i in ItemValues.FishCatchToApId.Values)
            {
                if (locations.Contains(i))
                    return true;
            }

            return false;
        }

        private static void CheckFishCatch()
        {
            // Field will reset to zero on a reset so don't send fish on reset.
            if (!PlayerState.PlayerReady()) return;

            int fishId = Memory.ReadInt(ItemValues.FishCatchAddr);

            // Somehow too slow and missed the catch?  Check the log (I don't think this can happen)
            if (fishId == -1)
            {
                CheckFishLog();
            }
            else
            {
                ItemValues.FishCatchToApId.TryGetValue(fishId, out int fishApId);
                if (App.Client.CurrentSession.Locations.AllMissingLocations.Contains(fishApId))
                    App.SendLocation(fishApId);
            }

            // Reset the monitor
            Memory.MonitorAddressForAction<int>(ItemValues.FishCatchAddr, WatchFishCatchField, (o) => { return o == -1; });
        }
    }
}
