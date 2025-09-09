from typing import List

from worlds.dc1.Items import DarkCloudItem
from worlds.dc1.Options import DarkCloudOptions

ids = {
    "Chief's House": 1600,
    "Jibubu's House": 1601,
    "Zabo's House": 1602,
    "3 Sisters' House": 1603,
    "Brooke's House": 1604,
    "Enga's House": 1605,
    "Prisoner Cabin": 1606,
    "Toto's House": 1607,
    "Totem Pole A": 1608,
    "Totem Pole B": 1609,
    "Totem Pole C": 1610,
    "Oasis": 1611,
    "Muska Lacka Trees": 1612,
    "Muska Lacka Road": 1613


  }

chief_ids = [1600]
zabo_ids = [1602]
sister_ids = [1603]

def create_muska_atla(options: DarkCloudOptions, player: int) -> List["DarkCloudItem"]:
    return []