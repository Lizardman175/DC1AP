from BaseClasses import Item, ItemClassification
from worlds.dc1.game_id import dc1_name


class DarkCloudItem(Item):
    # type = None
    game: str = dc1_name

    def __init__(self, name: str,
                 classification: ItemClassification,
                 code: int | None,
                 player: int):
        super().__init__(name, classification, code, player)
        self.game = dc1_name