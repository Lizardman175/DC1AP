# TODO for miracle chests, split out only necessary items to be required rather than the whole building.
#      Ex lamps don't yield miracle chests, but a cabin addition might
# Grouping of item IDs per building for Norune Village
from typing import List

from worlds.dc1.Items import DarkCloudItem
from worlds.dc1.Options import DarkCloudOptions
from BaseClasses import ItemClassification

ids = {
    "Player's House": 971111000,
    "Macho's House": 971111001,
    "Laura's House": 971111002,
    "Paige's House": 971111003,
    "Claude's House": 971111004,
    "Hag's House": 971111005,
    "Alnet's House": 971111006,
    "Gaffer's Buggy": 971111007,
    "Dran's Windmill": 971111008,
    "Windmill 1": 971111009,
    "Windmill 2": 971111010,
    "Windmill 3": 971111011,
    "Pond": 971111012,
    "Norune Trees A": 971111013,
    "Norune Trees B": 971111014,
    "Norune Bridge": 971111016,
    "Norune Road A": 971111021,
    "Norune Road B": 971111022,
    "Norune Road C": 971111023,
    "Norune Road D": 971111024,
    "Norune Road E": 971111025,
    "Norune River A": 971111031,
    "Norune River B": 971111032,
    "Norune River C": 971111033,
    "Norune River D": 971111034,

    "Claude's Bench": 971111039,
    "Player's Storage": 971111040,
    "Player's Keg": 971111041,
    "Player's Chimney": 971111042,
    "Alnet's Stairway": 971111043,
    "Annex": 971111044,
    "Barbell": 971111045,
    "Tricycle": 971111046,
    "Wheels": 971111047,
    "Hag's Bench": 971111048,
    "Candy Box": 971111049,
    "Jar": 971111050,
    "Supplies": 971111051,
    "Dran's Blade": 971111052,
    "Dran's Torch": 971111053,
    "Dran's Horn": 971111054,
    "Gaffer's Sign": 971111055,
    "Dran's Sign": 971111056,
    "Macho's Fence": 971111057,
    "Laura's Fence": 971111058,
    "Paige's Fence": 971111059,
    "Claude's Fence": 971111060,
    "Hag's Fence": 971111061,
    "Macho's Lamp": 971111062,
    "Laura's Lamp": 971111063,
    "Paige's Lamp": 971111064,
    "Claude's Lamp": 971111065,
    "Hag's Lamp": 971111066,
    "Alnet's Lamp": 971111067,
    "Gaffer's Lamp": 971111068,
    "Laura's Cabin": 971111069,
    "Paige's Cabin": 971111070,
    "Claude's Cabin": 971111071,
    "Hag's Cabin": 971111072,
    "Alnet's Cabin": 971111073,
    "Windmill Vanes 1": 971111074,
    "Windmill Vanes 2": 971111075,
    "Windmill Vanes 3": 971111076,
    "Windmill Ladder 1": 971111077,
    "Windmill Ladder 2": 971111078,
    "Windmill Ladder 3": 971111079,

    "Player's Llama": 971111080,
    "Claude": 971111081,
    "Komacho": 971111082,
    "Macho": 971111083,
    "Hag": 971111084,
    "Pike": 971111085,
    "Paige": 971111086,
    "Carl": 971111087,
    "Alnet": 971111088,
    "Gina": 971111089,
    "Laura": 971111090,
    "Gaffer": 971111091,
    "Renee": 971111092,
    "Stray Cat": 971111094,
    "Alnet's Llama": 971111095
  }

player_house_ids = ["Player's House", "Player's Keg", "Player's Chimney", "Player's Storage", "Player's Llama",
                    "Renee", "Stray Cat"]
macho_house_ids = ["Macho's House", "Macho's Fence", "Macho's Lamp", "Annex", "Barbell", "Macho", "Komacho"]
laura_house_ids = ["Laura's House", "Laura's Fence", "Laura's Lamp", "Laura's Cabin", "Tricycle", "Laura", "Gina"]
# Paige's House & Pike are in the Gaffer list to always be required TODO could find the flag and just always give good gaffer
paige_house_ids = ["Paige's Fence", "Paige's Lamp", "Paige's Cabin", "Wheels", "Paige"]
claude_house_ids = ["Claude's House", "Claude's Fence", "Claude's Lamp", "Claude's Cabin", "Claude's Bench", "Candy Box", "Claude"]
hag_house_ids = ["Hag's House", "Hag's Fence", "Hag's Lamp", "Hag's Cabin", "Hag's Bench", "Jar", "Hag"]
alnet_house_ids = ["Alnet's House", "Alnet's Lamp", "Alnet's Cabin", "Alnet's Stairway", "Alnet", "Carl", "Alnet's Llama"]
gaffer_buggy_ids = ["Paige's House", "Pike", "Gaffer's Buggy", "Supplies", "Gaffer's Sign", "Gaffer's Lamp", "Gaffer"]
d_windmill_ids = ["Dran's Windmill", "Dran's Sign", "Dran's Blade", "Dran's Torch", "Dran's Horn"]
windmill_ids = ["Windmill 1", "Windmill 2", "Windmill 3", "Windmill Vanes 1", "Windmill Vanes 2", "Windmill Vanes 3",
                "Windmill Ladder 1", "Windmill Ladder 2", "Windmill Ladder 3"]
other_ids = ["Norune Trees A", "Norune Trees B", "Norune Bridge", "Norune Road A", "Norune Road B", "Norune Road C",
             "Norune Road D", "Norune Road E", "Norune River A", "Norune River B", "Norune River C", "Norune River D"]


def create_norune_atla(options: DarkCloudOptions, player: int) -> {str, "DarkCloudItem"}:
    """Create atla items for Norune Village based on option settings."""
    # DarkCloudWorld.item_name_to_id += ids
    items = {}

    # Parts always required
    norune_required = player_house_ids + gaffer_buggy_ids

    # Parts that can be required but otherwise useful (miracle chests/villager rewards)
    norune_useful_conditional = ["Pond"] + hag_house_ids + laura_house_ids + paige_house_ids + claude_house_ids + alnet_house_ids

    # Filler that may be needed for miracle chests/villager rewards but otherwise nothing needed for atla shuffle
    norune_filler_conditional = macho_house_ids

    # Always filler (roads etc.)
    norune_filler_always = windmill_ids + other_ids

    # Dran's windmill is only full required if Dran is required
    if options.all_bosses:
        norune_required.extend(d_windmill_ids)
    else:
        norune_useful_conditional.extend(d_windmill_ids)

    for i in norune_required:
        items[i] = DarkCloudItem(i, ItemClassification.progression, ids[i], player)

    for i in norune_useful_conditional:
        items[i] = DarkCloudItem(i, ItemClassification.useful, ids[i], player)

    for i in norune_filler_conditional:
        items[i] = DarkCloudItem(i, ItemClassification.filler, ids[i], player)

    for i in norune_filler_always:
        items[i] = DarkCloudItem(i, ItemClassification.filler, ids[i], player)

    return items