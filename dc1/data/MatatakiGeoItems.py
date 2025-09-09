# Grouping of atla item IDs per building for Matataki Village

from worlds.dc1.Options import DarkCloudOptions
from BaseClasses import ItemClassification
from worlds.dc1.Items import DarkCloudItem

# TODO move to .json?
ids = {
    "Pao's House": 971111200,
    "Cacao's House": 971111201,
    "Bunbuku's House": 971111202,
    "Kye's House": 971111203,
    "Baron's House": 971111204,
    "Couscous's House": 971111205,
    "Gob's House": 971111206,
    "Mushroom House": 971111207,
    "Well 1": 971111208,
    "Well 2": 971111209,
    "Well 3": 971111210,
    "Watermill 1": 971111211,
    "Watermill 2": 971111212,
    "Watermill 3": 971111213,
    "Owl Shop": 971111214,
    "Matataki Bridge": 971111217,
    "Earth A": 971111218,
    "Earth B": 971111219,
    "Matataki River A": 971111220,
    "Matataki River B": 971111221,
    "Matataki River C": 971111222,
    "Matataki River D": 971111223,
    "Matataki River E": 971111224,
    "Matataki River F": 971111225,
    "Matataki River G": 971111226,
    "Matataki River H": 971111227,
    "Matataki Trees A": 971111230,
    "Matataki Trees B": 971111231,

    "Cacao's Cabin": 971111239,
    "Wise Owl Entrance": 971111240,
    "Pao's Laundry": 971111241,
    "Cacao's Laundry": 971111242,
    "Bunbuku's Cabin": 971111243,
    "Kye's Cabin": 971111244,
    "Owl Shop Entrance": 971111245,
    "Baron's Roof": 971111246,
    "Well 1 Roof": 971111247,
    "Well 2 Roof": 971111248,
    "Well 3 Roof": 971111249,
    "Branch": 971111250,
    "Grass": 971111251,
    "Bone": 971111252,
    "Gob's Tree": 971111253,
    "Second Floor": 971111254,
    "Balcony": 971111255,
    "Waterwheel 1": 971111256,
    "Waterwheel 2": 971111257,
    "Waterwheel 3": 971111258,
    "Well 1 Bucket": 971111259,
    "Well 2 Bucket": 971111260,
    "Well 3 Bucket": 971111261,
    "Pao's Stairway": 971111262,
    "Cacao's Stairway": 971111263,
    "Bunbuku's Stairway": 971111264,
    "Kye's Stairway": 971111265,
    "Well 1 Stairway": 971111266,
    "Well 2 Stairway": 971111267,
    "Well 3 Stairway": 971111268,
    "Pao's Sign": 971111269,
    "Cacao's Sign": 971111270,
    "Bunbuku's Sign": 971111271,
    "Kye's Sign": 971111272,
    "Baron's Sign": 971111273,
    "Couscous's Sign": 971111274,
    "Gob's Sign": 971111275,
    "Mushroom Sign": 971111276,
    "Owl Sign": 971111277,
    "Pao's Torch": 971111278,
    "Cacao's Torch": 971111279,
    "Bunbuku's Torch": 971111280,
    "Kye's Torch": 971111281,
    "Baron's Torch": 971111282,
    "Couscous's Torch": 971111283,
    "Gob's Torch": 971111284,
    "Mushroom Torch": 971111285,
    "Owl Torch": 971111286,
    "Well 1 Torch": 971111287,
    "Well 2 Torch": 971111288,
    "Well 3 Torch": 971111289,
    "Watermill 1 Torch": 971111290,
    "Watermill 2 Torch": 971111291,
    "Watermill 3 Torch": 971111292,

    "Baron": 971111380,
    "Bunbuku": 971111381,
    "Kululu": 971111382,
    "Cacao": 971111383,
    "Couscous": 971111384,
    "Gob": 971111385,
    "Mr. Moustache": 971111386,
    "Pao": 971111387,
    "Kye": 971111388,
    "Momo": 971111389,
    "Ro": 971111390,
    "Annie": 971111391
  }

pao_ids = ["Pao's House", "Pao's Stairway"]
cacao_ids = ["Cacao's House", "Cacao's Torch", "Cacao's Sign", "Cacao's Stairway", "Cacao's Laundry", "Cacao's Cabin",
             "Cacao"]
bunbuku_ids = ["Bunbuku's House", "Bunbuku's Stairway", "Bunbuku's Cabin"]  # 20
kye_ids = ["Kye's House", "Kye's Stairway", "Kye's Cabin"]
baron_ids = ["Baron's House", "Branch"]
couscous_ids = ["Couscous's House"]  # 38
gob_ids = ["Gob's House"]
mush_ids = ["Mushroom House", "Mushroom Torch", "Mushroom Sign", "Second Floor", "Balcony", "Ro", "Annie"]
owl_ids = ["Owl Shop", "Owl Torch", "Owl Sign", "Owl Shop Entrance", "Wise Owl Entrance", "Mr. Moustache"]  # 57
well_ids = ["Well 1", "Well 2", "Well 3", "Well 1 Torch", "Well 1 Stairway", "Well 1 Roof", "Well 1 Bucket",
            "Well 2 Torch",
            "Well 2 Stairway", "Well 2 Roof", "Well 2 Bucket", "Well 3 Torch", "Well 3 Stairway", "Well 3 Roof",
            "Well 3 Bucket"]
watermill_ids = ["Watermill 1", "Watermill 2", "Watermill 3", "Watermill 1 Torch", "Waterwheel 1",
                 "Watermill 2 Torch", "Waterwheel 2", "Watermill 3 Torch", "Waterwheel 3"]  # 81
river_ids = ["Matataki River A", "Matataki River B", "Matataki River C", "Matataki River D",
             "Matataki River E", "Matataki River F", "Matataki River G", "Matataki River H"]
other_ids = ["Matataki Trees A", "Matataki Trees B", "Matataki Bridge"]

def create_matataki_atla(options: DarkCloudOptions, player: int) -> {str, "DarkCloudItem"}:
    """Create atla items for Matataki Village based on option settings."""

    items = {}
    matataki_useful_conditional = []

    matataki_useful_conditional.extend(["Pao's Torch", "Pao's Sign", "Pao's Laundry", "Pao"])
    matataki_useful_conditional.extend(["Bunbuku's Torch", "Bunbuku's Sign", "Bunbuku", "Kululu"])
    matataki_useful_conditional.extend(["Kye's Torch", "Kye's Sign", "Kye", "Momo"])
    matataki_useful_conditional.extend(["Gob's Torch", "Gob's Sign", "Bone", "Gob's Tree", "Gob"])
    matataki_useful_conditional.extend(["Couscous's Torch", "Couscous's Sign", "Grass", "Couscous"])
    matataki_useful_conditional.extend(["Baron's Torch", "Baron's Sign", "Baron's Roof", "Baron"])

    # Static definitions for now, as miracle chest checks are added, the conditional blocks will change
    matataki_required = cacao_ids + river_ids + mush_ids
    matataki_useful = (["Earth A", "Earth B"] + pao_ids + bunbuku_ids + kye_ids + baron_ids + couscous_ids + gob_ids +
                       owl_ids + watermill_ids)
    matataki_filler = well_ids + other_ids

    for i in matataki_required:
        items[i] = DarkCloudItem(i, ItemClassification.progression, ids[i], player)

    for i in matataki_useful_conditional:
        items[i] = DarkCloudItem(i, ItemClassification.useful, ids[i], player)

    for i in matataki_useful:
        items[i] = DarkCloudItem(i, ItemClassification.useful, ids[i], player)

    for i in matataki_filler:
        items[i] = DarkCloudItem(i, ItemClassification.filler, ids[i], player)

    return items