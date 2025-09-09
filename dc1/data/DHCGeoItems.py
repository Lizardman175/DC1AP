from typing import List

from BaseClasses import ItemClassification
from worlds.dc1.Items import DarkCloudItem
from worlds.dc1.Options import DarkCloudOptions

ids = {
    "Crowning Day": 971112000,
    "Night of the Ceremony": 971112001,
    "Reunion in the Storm": 971112002,
    "First Campaign": 971112003,
    "Menace of the West": 971112004,
    "The Deal": 971112005,
    "Dark Power": 971112006,
    "The Assassin": 971112007,
    "Must be Protected": 971112008,
    "Birth of the Demon": 971112009,
    "Things Lost": 971112010,
    "Departure": 971112011,

    "Father": 971112040,
    "Sophia": 971112041,
    "Bed": 971112042,
    "Moon": 971112043,
    "Castle": 971112044,
    "Memories Torch": 971112045,
    "Crown": 971112046,
    "Minister": 971112047,
    "Buggy": 971112048,
    "Horserider": 971112049,
    "Sword": 971112050,
    "Wizard": 971112051,
    "Fort": 971112052,
    "Black Robed Man": 971112053,
    "Bloody Agreement": 971112054,
    "Black Blood": 971112055,
    "Light": 971112056,
    "Wine": 971112057,
    "Door": 971112058,
    "Assassin": 971112059,
    "Knife": 971112060,
    "Bloody Dress": 971112061,
    "Dark Cloud": 971112062,
    "Ruined Castle": 971112063,
    "Grave": 971112064,
    "The Broken Sword": 971112065,
    "Sandglass": 971112066,
    "Prophet": 971112067,
    "Book of Curses": 971112068
  }

def create_castle_atla(options: DarkCloudOptions, player: int) -> List["DarkCloudItem"]:
    """Create atla items for Dark Heaven Castle."""
    # DarkCloudWorld.item_name_to_id += ids
    items = []
    # All castle atla are required for the genie fight
    # TODO need duplicates for dupe items, so still need more work here
    for i in ids:
        items.append(DarkCloudItem(i, ItemClassification.progression, ids[i], player))

    return items
