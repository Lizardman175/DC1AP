using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Models;
using DC1AP.Constants;
using DC1AP.Mem;
using DC1AP.Threads;
using DC1AP.Utils;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace DC1AP.Items
{
    internal class InventoryMgmt
    {
        private static ConcurrentDictionary<long, InvItem>? ItemData;
        private static ConcurrentDictionary<long, Attachment>? AttachmentData;
        internal static readonly Dictionary<short, Attachment> AttachmentDataByGameId = [];
        private static readonly Random random = new();

        private static readonly ConcurrentDictionary<long, int> itemCounts = [];
        private static readonly ConcurrentDictionary<long, int> attachCounts = [];

        private const uint InvMaxAddr = 0x01CDD8AC;  // Byte.  Can't exceed 100 or we run past the buffer.
        private const uint InvCurAddr = 0x01CDD8AD;  // Byte.  Next byte starts the active item shorts, followed by 3 shorts giving count of the active items per slot, then shorts for the other items.
        private const uint IDSize = sizeof(short);

        private const uint FirstInvAddr = 0x01CDD8BA;
        /*
         *  0 for most items. Duration for things like feathers and amulets. Gives value item restores as well for curatives
         *  but doesn't seem to do anything if changed. -1 or 0 for no item (sometimes ghost values as well caused by moving items from the active list)
         */
        private const uint FirstItemDurationAddr = 0x01CDD988; // Short
        private const uint FirstActiveItemCountAddr = 0x01CDD8B4;

        // Add 1 byte for the CurAddr field, then 2 for each short past the first addr
        //private static uint[] ActiveItemAddrs = [InvCurAddr + 1, InvCurAddr + 3, InvCurAddr + 5];
        //private static uint[] ActiveItemCountAddrs = [InvCurAddr + 7, InvCurAddr + 9, InvCurAddr + 11];

        private const uint FirstAttchAddr = 0x01CE1A48;
        private const int MaxAttachCount = 40;
        private const uint AttachmentSize = 0x20;
        private const uint FirstAttachAttrOffset = 0x08;
        private const int FirstAttachDefaultValue = 3;

        //private static readonly List<(short, short)> attachChanges = []; // (index, item ID)
        private static List<short> attachmentInv = new(MaxAttachCount);

        private struct AttachmentStr()
        {
#pragma warning disable IDE0044 // Add readonly modifier
            short id = -1;
            short synthedWeaponId = 0;  // Matches weapon ID for id 5A, synthsphere
            // These shorts contain data about how many levels and what boons/banes the attach provides (for synthspheres)
            short ignored2 = 0;
            short ignored3 = 0;

            short attack = 0;
            short endurance = 0;
            short speed = 0;
            short magic = 0;

            byte fire = 0;
            byte ice = 0;
            byte thunder = 0;
            byte wind = 0;
            byte holy = 0;
            
            byte drag = 0;
            byte undead = 0;
            byte fish = 0;
            byte rock = 0;
            byte plant = 0;
            byte beast = 0;
            byte sky = 0;
            byte metal = 0;
            byte mimic = 0;
            byte mage = 0;
            private byte padding = 0;
#pragma warning restore IDE0044 // Add readonly modifier
        }

        internal static void InitInventoryMgmt()
        {
            ItemData = Resources.Embedded.Items;
            AttachmentData = Resources.Embedded.Attachments;

            OpenMem.InitItemCountAddrs(ItemData.Keys.ToArray(), AttachmentData.Keys.ToArray());
        }

        /// <summary>
        /// The game is very unreliable about counting the player's inventory.  Manually count here to account for hotbar items.
        /// </summary>
        /// <returns></returns>
        private static bool HasAvailableInventory()
        {
            byte invCount = 0;
            byte maxCount = Memory.ReadByte(InvMaxAddr);

            byte activeCount = (byte)(Memory.ReadByte(FirstActiveItemCountAddr) + Memory.ReadByte(FirstActiveItemCountAddr
                + sizeof(short)) + Memory.ReadByte(FirstActiveItemCountAddr + 2 * sizeof(short)));

            invCount += activeCount;

            for (int ii = 0; ii < maxCount; ii++)
            {
                uint addr = (uint)(FirstInvAddr + IDSize * ii);
                short itemValue = Memory.ReadShort(addr);

                if (itemValue != -1 && itemValue != 0)
                {
                    invCount++;
                }
            }

            return invCount < maxCount;
        }

        /// <summary>
        /// Searches for an empty inventory slot and gives the player the item supplied.  Returns true if successful, false if inventory is full.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        internal static bool GiveItem(long itemId, bool updateFlag=true)
        {
            byte maxInv = Memory.ReadByte(InvMaxAddr);
            InvItem item = ItemData[itemId];

            if (itemCounts.ContainsKey(itemId) && itemCounts[itemId] <= OpenMem.ReadItemCountValue(itemId))
            {
                return true;
            }

            if (HasAvailableInventory())
            {
                for (int ii = 0; ii < maxInv; ii++)
                {
                    uint addr = (uint)(FirstInvAddr + IDSize * ii);
                    short itemValue = Memory.ReadShort(addr);

                    if (itemValue == -1 || itemValue == 0)
                    {
                        Memory.Write(addr, item.ItemID);
                        uint durationAddr = FirstItemDurationAddr + (uint)(IDSize * ii);
                        if (item.ValueMax > 0)
                        {
                            // Items with usage values like amulets/dran's feather need this value set or they break immediately.
                            // Other items like bread & water have a value but it doesn't seem to matter; set them just in case.
                            if (item.ValueMax == item.ValueMin)
                                Memory.Write(durationAddr, (short)item.ValueMin);
                            else
                                Memory.Write(durationAddr, (short)random.Next(item.ValueMin, item.ValueMax + 1));
                        }
                        else
                        {
                            Memory.Write(durationAddr, -1);
                        }

                        string msg = "Received " + item.Name + ".";
                        if (PlayerState.IsPlayerInDungeon())
                        {
                            ItemQueue.AddMsg(msg);
                        }
                        else
                        {
                            Log.Logger.Information(msg);
                            App.Client.AddOverlayMessage(msg);
                        }

                        if (updateFlag)
                            OpenMem.IncItemCountValue(itemId);

                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool GiveAttachment(long itemId, bool updateFlag = true)
        {
            Attachment item = AttachmentData[itemId];

            if (attachCounts.ContainsKey(itemId) && attachCounts[itemId] <= OpenMem.ReadItemCountValue(itemId))
            {
                return true;
            }

            for (int ii = 0; ii < MaxAttachCount; ii++)
            {
                uint addr = (uint)(FirstAttchAddr + AttachmentSize * ii);
                short itemValue = Memory.ReadShort(addr);

                if (itemValue == -1 || itemValue == 0)
                {
                    string msg = "Received " + item.Name + ".";

                    // Game sometimes leaves junk data in the attachments, need to clear that before adding our own
                    Memory.WriteStruct<AttachmentStr>(addr, new AttachmentStr());
                    Memory.Write(addr, item.ItemID);
                    
                    // Some of these fields are actually shorts but we shouldn't be setting large enough values to matter.
                    for (int val = 0; val < item.ValueOffsets.Length; val++)
                    {
                        byte value = (byte)item.Values[val];
                        if (Options.AttachMultConfig > 0)
                            value = (byte)Math.Ceiling(value * Options.AttachMultiplier);
                        Memory.WriteByte((ulong)(addr + item.ValueOffsets[val]), value);
                    }

                    if (PlayerState.IsPlayerInDungeon())
                    {
                        ItemQueue.AddMsg(msg);
                    }
                    else
                    {
                        Log.Logger.Information(msg);
                        App.Client.AddOverlayMessage(msg);
                    }

                    if (updateFlag)
                        OpenMem.IncItemCountValue(itemId);
                    return true;
                }
            }
            return false;
        }

        internal static bool RemoveInvItem(short itemId)
        {
            bool success = false;
            byte maxInv = Memory.ReadByte(InvMaxAddr);
            uint addr = FirstInvAddr - IDSize + (maxInv * IDSize);

            for (int ii = maxInv - 1; ii >= 0; ii--)
            {
                short id = Memory.ReadShort(addr);
                if (id == itemId)
                {
                    Memory.Write(addr, (ushort)0xFFFF);
                    // We probably should overwrite the durability value for the item but it shouldn't matter.
                    success = true;
                    break;
                }
                addr -= IDSize;
            }

            return success;
        }

        // Now unused, holding onto in case we find a reason to use it.
        //internal static bool RemoveAttchItem(short itemId)
        //{
        //    uint addr;
        //    bool result = false;

        //    for (int ii = 0; ii < attachChanges.Count; ii++)
        //    {
        //        (short, short) item = attachChanges[ii];
        //        if (item.Item2 == itemId)
        //        {
        //            addr = (uint)(FirstAttchAddr + (item.Item1 * AttachmentSize));
        //            Memory.Write(addr, 0x0000FFFF);
        //            for (int x = 4; x < AttachmentSize; x += 4)
        //                Memory.Write((ulong)(addr + x), 0);
        //            result = true;
        //            break;
        //        }
        //    }

        //    return result;
        //}

        internal static void GiveFreeFeather()
        {
            GiveItem(MiscConstants.FeatherId, false);
        }

        internal static void IncItemCount(long apId)
        {
            itemCounts.TryGetValue(apId, out int value);
            itemCounts[apId] = value + 1;
        }

        internal static void IncAttachCount(long apId)
        {
            attachCounts.TryGetValue(apId, out int value);
            attachCounts[apId] = value + 1;
        }

        /// <summary>
        /// Compares the GameState item counts to how many of each item are saved to memory, giving the player the difference.
        /// </summary>
        internal static void VerifyItems()
        {
            // Clear current values, check what the server thinks first, then compare that against the save file.
            itemCounts.Clear();
            attachCounts.Clear();

            foreach (ItemInfo itemInfo in App.Client.CurrentSession.Items.AllItemsReceived)
            {
                long apId = itemInfo.ItemId;
                if (apId > MiscConstants.AttachIdBase)
                    IncAttachCount(apId);
                else if (apId > MiscConstants.ItemIdBase)
                    IncItemCount(apId);
            }

            foreach (long itemId in itemCounts.Keys)
            {
                byte value = OpenMem.ReadItemCountValue(itemId);
                if (itemCounts[itemId] > value)
                {
                    for (int ii = value; ii < itemCounts[itemId]; ii++)
                    {
                        if (CanGiveItem(itemId))
                            ItemQueue.AddItem(itemId);
                    }
                }
            }

            foreach (long attachId in attachCounts.Keys)
            {
                byte value = OpenMem.ReadItemCountValue(attachId);
                if (attachCounts[attachId] > value)
                {
                    for (int ii = value; ii < attachCounts[attachId]; ii++)
                        ItemQueue.AddAttachment(attachId);
                }
            }
        }

        /// <summary>
        /// Determines if we can give the given item to the player. Defense items are only given if the player has 
        /// the relevant char, HP/Water items are given in multiples of 7 (for simplicity) based on char count.
        /// </summary>
        /// <param name="apId">Item ID to test</param>
        /// <returns></returns>
        internal static bool CanGiveItem(long apId)
        {
            bool result = true;

            // Don't give defense buff items until the char is recruited
            if ((apId == MiscConstants.CookieId && !CharFuncs.Osmond) ||
                    (apId == MiscConstants.JerkyId && !CharFuncs.Ungaga) ||
                    (apId == MiscConstants.ParfaitId && !CharFuncs.Ruby) ||
                    (apId == MiscConstants.GrassCakeId && !CharFuncs.Goro) ||
                    (apId == MiscConstants.FishCandyId && !CharFuncs.Xiao))
                result = false;
            // Limit FoE/Gourd based on recruited chars to avoid flooding inventory with unusable items.
            else if (apId == MiscConstants.GourdId || apId == MiscConstants.FruitOfEdenId)
            {
                byte count = OpenMem.ReadItemCountValue(apId);
                byte max = 7;

                if (CharFuncs.Osmond)
                    return true;
                else if (CharFuncs.Ungaga)
                    max *= 5;
                else if (CharFuncs.Ruby)
                    max *= 4;
                else if (CharFuncs.Goro)
                    max *= 3;
                else if (CharFuncs.Xiao)
                    max *= 2;

                result = max > count;
            }

            return result;
        }

        // Now unused, holding onto in case we find a reason to use it.
        /// <summary>
        /// Checks the player's current attachment inventory and optionally compares it against the most recent inventory so when we remove 
        /// an attachment, we only remove the new one.  This is due to atk/spd/mg/end attachments all having the same ID but different values.
        /// </summary>
        /// <param name="firstInit">Don't compare against the existing data, this is an initialization call.</param>
        //internal static void CheckAttachments(bool firstInit)
        //{
        //    uint addr = FirstAttchAddr;
        //    List<short> newAttachmentInv = new(MaxAttachCount);

        //    for (int ii = 0; ii < MaxAttachCount; ii++)
        //    {
        //        newAttachmentInv.Add(Memory.ReadShort(addr));
        //        addr += AttachmentSize;
        //    }

        //    if (!firstInit)
        //    {
        //        attachChanges.Clear();
        //        for (short ii = 0; ii < attachmentInv.Count; ii++)
        //        {
        //            if (attachmentInv[ii] != newAttachmentInv[ii])
        //            {
        //                attachChanges.Add((ii, newAttachmentInv[ii]));
        //            }
        //        }
        //        attachmentInv = newAttachmentInv;
        //    }
        //}

        internal static void MultiplyAttachments()
        {
            if (Options.AttachMultConfig != AttachMultConfig.All || Options.AttachMultiplier == 1.0f)
                return;

            bool checkIfMultiplied = true;
            uint addr = MiscAddrs.FirstAttachmentDataAddr;
            short id = Memory.ReadByte(addr);

            while (id != 0)
            {
                // Index of first attribute we want to multiply
                uint addr2 = addr + FirstAttachAttrOffset;
                for (uint i = 0; i < AttachmentSize - FirstAttachAttrOffset; i++)
                {
                    byte b = Memory.ReadByte(addr2+i);
                    if (b > 0)
                    {
                        // Don't multiply if already multiplied
                        if (checkIfMultiplied)
                        {
                            if (b != FirstAttachDefaultValue)
                                return;
                            checkIfMultiplied = false;
                        }
                        b = (byte)Math.Ceiling(b * Options.AttachMultiplier);
                        Memory.WriteByte(addr2+i, b);
                    }
                }

                addr += AttachmentSize;
                id = Memory.ReadByte(addr);
            }
        }
    }
}
