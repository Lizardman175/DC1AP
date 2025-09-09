
namespace DC1AP.Constants
{
    // Misc address values that aren't enough to warrant grouping to their own file.
    internal class MiscAddrs
    {
        public const uint GameIdAddr = 0x1DA82FD;

        // 0x00A2 is A, 0x00BC is a.  Go up from there to get other letters.  Before A2 is Japanese.
        //public const uint ToanNameAddr = 0x1CD4188;
        //public const uint XiaoNameAddr = 0x1CD41C8;
        //public const uint GoroNameAddr = 0x1CD4208;
        //public const uint RubyNameAddr = 0x1CD4248;
        //public const uint UngagaNameAddr = 0x1CD4288;
        //public const uint OsmondNameAddr = 0x1CD42C8;

        // 1CD4100 : B10B

        //public const uint GildaAddr = 0x01CDD892;

        // For managing adding items
        //public const uint InvStart = 0x01CDD8BA;
        //public const uint AttachStart = 0x01CE1A48;

        // Set to 1 to enable the world map
        public const uint MapFlagAddr = 0x1CDD86C;

        // Addresses for visit count of each town/dungeon on the map.  value >0 makes them show up on the map (curious what negative numbers would do)
        // Value > 0 also prevents dungeons from initializing by default.
        //public const int NoruneCountAddr = 0x1CE7028; // Norune village counter (clearing it makes Norune unavailable; unless doing a non-standard start, don't edit!)
        public const uint DBCCountAddr = 0x1CE70C8; // DBC counter

        // There is no value that will disable Matataki other than the map flag itself
        public const uint WOFCountAddr = 0x1CE70CA; // Wise owl forest

        //public const uint BrownbooCountAddr = 0x1CE7044; // Brownboo Village

        public const uint QueensCountAddr = 0x1CE702C;
        public const uint SWCountAddr = 0x01CE70CC;

        public const uint MuskaCountAddr = 0x01CE702E;
        public const uint SMTExtCountAddr = 0x01CE707C; // sun/moon temple exterior counter
        public const uint SMTIntCountAddr = 0x01CE70CE; // SMT interior (dungeon)

        public const uint MFCountAddr = 0x01CE7030; // Moon factory
        public const uint YDCountAddr = 0x01CE7056; // Yellow Drops
        public const uint MSCountAddr = 0x01CE70D0; // Moon sea

        public const uint GOTCountAddr = 0x01CE70D2; // Gallery of Time
        public const uint DHCCountAddr = 0x01CE7078; // DHC? Seems to also activate ThE sHaFt

        // >0 will open the demon shaft dungeon for business
        //public const uint ShaftCounterAddr = 0x1CE70A0;

        // How many floors are available for a given dungeon.  0 indexed.  Setting these early will skip normal dungeon initialization.
        public const uint DBCFloorCountAddr = 0x01CDD80B;
        public const uint WOFFloorCountAddr = 0x01CDD80C;
        public const uint SWFloorCountAddr  = 0x01CDD80D;
        public const uint SMTFloorCountAddr = 0x01CDD80E;
        public const uint MSFloorCountAddr  = 0x01CDD80F;
        public const uint DHCFloorCountAddr = 0x01CDD810;
        public const uint DSFloorCountAddr  = 0x01CDD811;

        public static uint[] FloorCountAddrs = [DBCFloorCountAddr, WOFFloorCountAddr, SWFloorCountAddr, SMTFloorCountAddr, MSFloorCountAddr, DHCFloorCountAddr];
        public static byte[] FloorCountFront = [7, 8, 8, 8, 7, 23];
        public static byte[] FloorCountRear  = [14, 16, 17, 17, 14, 23];

        ///<summary>
        ///     1 = Walking Mode
        /// <br>2 = On Menu</br>
        /// <br>3 = Door Menu</br>
        /// <br>5 = Ally Quick Select</br>
        /// <br>7 = Next Floor Screen</br>
        /// 
        /// Only give items/atla in dungeon when this is 1
        ///</summary>
        public const uint DungeonMode = 0x002A355C;
        public const uint CurDungeon = 0x002A3594;    // 0 index
        //public const uint CurDungeon = 0x1CD954C;     // 0 index - I don't think this is correct, sometimes it is not the correct dungeon value
        public const uint CurFloor = 0x01CD954E;      // 0 index
        public const uint InDungeonFlag = 0x1CD954F;  // -1 if not in dungeon, 0 if in dungeon

        /// <summary>
        ///     0 = Main title
        /// <br>1 = Intro</br>
        /// <br>2 = Town</br>
        /// <br>3 = Dungeon</br>
        /// <br>4 = ? (doesnt crash in dungeon)</br>
        /// <br>5 = Opening cutscene (dark shrine)</br>
        /// <br>7 = Debug menu</br>
        /// </summary>
        public const uint PlayerState = 0x002A2534;

        //public const uint CrystalFlag = 0x002A35A0;  TODO could allow auto crsytal/map option (possibly clear them from the loot table as well?)
        //public const uint MapFlag = 0x002A359C;
        //public const uint MinimapFlag = 0x002A35B0;  TODO could add a feature to disable minimap if people want it 
        
        /// <summary>
        ///     0 = No NPCS/Player
        /// <br>1 = Walking Mode</br>
        /// <br>2 = Returns to the last NPC spoken to (Crashes if no NPC has been interacted with before a load)</br>
        /// <br>3 = Fade in transition (Reloads state to 1)</br>
        /// <br>4 = Georama Mode</br>
        /// <br>5 = Last Menu accessed</br>
        /// <br>6 = Georama Menu</br>
        /// <br>7 = Transitioning to the last previous menu</br>
        /// <br>8 = Is on a menu (If forcing it while on a menu, goes to state 1, if on state 1 already it just gives a blank screen)</br>
        /// <br>9 = Pause</br>
        /// <br>10 = Pause without character models on the background</br>
        /// <br>11 = Transition to Interior Mode (If used in Walking Mode: Freezes the game except some audio [CANNOT UNDO]))</br>
        /// <br>12 = Interior Mode (if used in Walking Mode: Same as 11 excepts BGM still plays [CANNOT UNDO])</br>
        /// <br>14 = Time Transition (Cannot Set)</br>
        /// <br>16 = Fishing Mode</br>
        /// </summary>
        //public const uint TownModeAddr = 0x002A1F50;  // TODO may be useful for player state when giving items.  Don't want to be in geo menus when giving geo.

        /// <summary>
        ///     0 = Walking Mode
        /// <br>1 = Transitioning to the outside</br>
        /// <br>2 = Finished unloading the interior</br>
        /// <br>3 = Talking to NPC</br>
        /// <br>4 = ???</br>
        /// <br>5 = Pause the game</br>
        /// <br>6 = Transitioning to the last previous menu</br>
        /// <br>7 = Last previous menu</br>
        /// <br>8+ = Freeze (CAN UNDO)</br>
        /// <br>14 = Camera Zoom (still freeze, CAN UNDO)</br>
        /// </summary>
        //public const uint InteriorModeAddr = 0x002A2A84;   // TODO may be useful for player state when giving items.
        //public const int dunPauseTitle = 0x002A35C4;   //Show the "PAUSE" title on screen (0 = OFF/1 = ON)        

        public const uint DunMsgAddr = 0x00998BB8;     //The address pointing to the text of the 10th dungeon message. 157 Byte array
        public const uint DunMsgDurAddr = 0x01EA7694;  //How long to show the message


        public const uint DunMsgIdAddr = 0x01EA76B4;
        //public const uint DunToggle1 = 0x01EA7690;               // Toggles the message on/off
        //public const uint DunPausePlayer = 0x002A3564;           // Is the player model in the pause state (0 = OFF/1 = ON)
        //public const uint DunPauseEnemy = 0x002A34DC;            // Are the enemy models in the pause state (0 = OFF/1 = ON)
        //public const uint dunToggle2 = 0x01EA76AC;               // Toggles the message on/off
        //public const uint dunMessageWidth = 0x01EB6438;          // Value is equal to the number of chars in a string (Ex: 5 -> width of a 5 char string)
        //public const uint dunMessageHeight = 0x01EB643C;         // Value is equal to the number of lines of a string paragraph (Ex: 2 -> paragraph with 2 lines)
        //public const uint dunMessage11 = 0x00998C8E;             // The address pointing to the text of the 11th dungeon message. 172 Byte array
        //public const uint dunMessageLastEnemyName = 0x00999EE8;  // The address for the last enemy/last message content
        //public const uint dunMessageDelay = 0x01EA7698;          // How long to wait until the message is shown

        ///<summary>
        ///     1 = Walking Mode
        /// <br>2 = On Menu</br>
        /// <br>3 = Door Menu</br>
        /// <br>5 = Ally Quick Select</br>
        /// <br>7 = Next Floor Screen</br>
        ///</summary>
        public const uint dungeonMode = 0x002A355C;

        // Technically these are the kill counts on the boss floors, but can only be 1
        public const uint DranFlag = 0x01CDD200;
        public const uint UtanFlag = 0x01CDD2CC;
        public const uint SaiaFlag = 0x01CDD396;  // TODO sets to 2? verify (probably just check if != 0)
        public const uint CurseFlag = 0x01CDD45E;
        public const uint JoeFlag = 0x01CDD520;
        public const uint GenieFlag = 0x01CDD5FC;

        public static readonly uint[] BossKillFlags = [DranFlag, UtanFlag, SaiaFlag, CurseFlag, JoeFlag, GenieFlag];
    }
}
