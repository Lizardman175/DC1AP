from BaseClasses import ItemClassification
from worlds.dc1.Items import DarkCloudItem
from worlds.dc1.Options import DarkCloudOptions

ids = {
    "Progressive Player's House": 971110100,
    "Progressive Macho's House": 971110101,
    "Progressive Laura's House": 971110102,
    "Progressive Paige's House": 971110103,
    "Progressive Claude's House": 971110104,
    "Progressive Hag's House": 971110105,
    "Progressive Alnet's House": 971110106,
    "Progressive Gaffer's Buggy": 971110107,
    "Progressive Dran's Windmill": 971110108,
    "Progressive Windmill 1": 971110109,
    "Progressive Windmill 2": 971110110,
    "Progressive Windmill 3": 971110111,
    "Pond": 971110112,
    "Norune Trees": 971110113,
    "Norune Road": 971110114,
    "Norune River": 971110115,
    "Norune Bridge": 971110116,
}

player_house_ids = ["Progressive Player's House", "Progressive Player's House", "Progressive Player's House",
                    "Progressive Player's House", "Progressive Player's House", "Progressive Player's House",
                    "Progressive Player's House"]
# Paige's House & Pike are in the Gaffer list to always be required TODO could find the flag and just always give good gaffer?
gaffer_buggy_ids = ["Progressive Paige's House", "Progressive Paige's House", "Progressive Gaffer's Buggy",
                    "Progressive Gaffer's Buggy", "Progressive Gaffer's Buggy", "Progressive Gaffer's Buggy",
                    "Progressive Gaffer's Buggy"]

# Just windmill (majors! make windmill always progressive (and required by dran/goro?))
d_windmill_ids = ["Progressive Dran's Windmill", "Progressive Dran's Windmill", "Progressive Dran's Windmill",
                  "Progressive Dran's Windmill"]

# Macho: House + annex (all minors)
macho_house_ids = ["Progressive Macho's House", "Progressive Macho's House", "Progressive Macho's House",
                   "Progressive Macho's House", "Progressive Macho's House"]

# House (gourd+fruit) + cabin (minors)
laura_house_ids = ["Progressive Laura's House", "Progressive Laura's House", "Progressive Laura's House",
                   "Progressive Laura's House", "Progressive Laura's House"]

# Paige MCs: Just house, all minors.  Cabin doesn't have chests Paige's house does give the pocket though!
paige_house_ids = ["Progressive Paige's House", "Progressive Paige's House",
                   "Progressive Paige's House", "Progressive Paige's House", "Progressive Paige's House"]
# House + cabin
claude_house_ids = ["Progressive Claude's House", "Progressive Claude's House", "Progressive Claude's House",
                    "Progressive Claude's House", "Progressive Claude's House"]

# House + cabin (minor)
hag_house_ids = ["Progressive Hag's House", "Progressive Hag's House", "Progressive Hag's House",
                 "Progressive Hag's House", "Progressive Hag's House"]

# House (fruit+gourd) + stairs (minors) + cabin (minor)
alnet_house_ids = ["Progressive Alnet's House", "Progressive Alnet's House",
                   "Progressive Alnet's House", "Progressive Alnet's House"]

windmill_ids = ["Progressive Windmill 1", "Progressive Windmill 1", "Progressive Windmill 1",
                "Progressive Windmill 2", "Progressive Windmill 2", "Progressive Windmill 2",
                "Progressive Windmill 3", "Progressive Windmill 3", "Progressive Windmill 3"]
other_ids = ["Norune Trees", "Norune Trees", "Norune Bridge", "Norune Road", "Norune Road", "Norune Road",
             "Norune Road", "Norune Road", "Norune River", "Norune River", "Norune River", "Norune River"]

# TODO split these lists into 2 based on items that spawn from the first half of a dungeon or the second
# Atla that give MCs by content quality (unless handled otherwise). If MC shuffle is on, these all need to be required
mc_useful = ["Progressive Hag's House", "Progressive Hag's House",
             "Progressive Laura's House", "Progressive Alnet's House"]
mc_filler = ["Progressive Macho's House", "Progressive Macho's House", "Progressive Claude's House",
             "Progressive Claude's House", "Progressive Laura's House", "Progressive Alnet's House",
             "Progressive Alnet's House"]

# Always required/useful/filler items
required = ["Pond", "Progressive Dran's Windmill"] + player_house_ids + gaffer_buggy_ids
useful = hag_house_ids + paige_house_ids
filler = windmill_ids + other_ids + alnet_house_ids + claude_house_ids + laura_house_ids + macho_house_ids


def create_norune_atla(options: DarkCloudOptions, player: int) -> list["DarkCloudItem"]:
    """Create atla items for Norune Village based on option settings."""
    items = []

    norune_required = required
    norune_useful = useful
    norune_filler = filler

    # Dran's windmill is only full required if Dran is required
    if options.all_bosses:
        norune_required.extend(d_windmill_ids)
    else:
        norune_useful.extend(d_windmill_ids)

    # TODO MC: if miracle shuffle, add these to norune_required
    # if options.miracle_chests:
    # norune_required.extend(mc_useful)
    # norune_required.extend(mc_filler)
    # else:
    norune_useful.extend(mc_useful)
    norune_filler.extend(mc_filler)

    for i in norune_required:
        items.append(DarkCloudItem(i, ItemClassification.progression, ids[i], player))

    for i in norune_useful:
        items.append(DarkCloudItem(i, ItemClassification.useful, ids[i], player))

    for i in norune_filler:
        items.append(DarkCloudItem(i, ItemClassification.filler, ids[i], player))

    return items
