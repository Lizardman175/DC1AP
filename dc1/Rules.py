from BaseClasses import CollectionState
from worlds.dc1 import DarkCloudOptions
from worlds.dc1.data import NoruneGeoItems, MatatakiGeoItems, DHCGeoItems


def items_available(state: CollectionState, player: int, names) -> bool:
    r = True
    for name in names:
        r = state.has(name, player)
        if not r:
            break

    return r


class RuleManager:

    def xiao_available(self, state: CollectionState, player: int) -> bool:
        return items_available(state, player, NoruneGeoItems.player_house_ids + NoruneGeoItems.gaffer_buggy_ids)

    def dran_accessible(self, state: CollectionState, player: int) -> bool:
        names = NoruneGeoItems.d_windmill_ids
        return items_available(state, player, names) and self.xiao_available(state, player)

    def goro_available(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        r = items_available(state, player, MatatakiGeoItems.cacao_ids + MatatakiGeoItems.river_ids)

        if r:
            r = self.xiao_available(state, player)

        return r

    def utan_accessible(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        r = items_available(state, player, MatatakiGeoItems.mush_ids) and self.goro_available(state, player, options)

        return r

    def ruby_available(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        return False

    def saia_accessible(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        return False

    def ungaga_available(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        return False

    def curse_accessible(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        return False

    def osmond_available(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        return False

    def joe_accessible(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        return False

    def got_accessible(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        # TODO this logic can likely be further reduced.  Should only need to check Ungaga & Osmond
        return self.osmond_available(state, player, options) and self.ungaga_available(state, player, options) and \
            self.ruby_available(state, player, options) and self.goro_available(state, player, options)

    def genie_accessible(self, state: CollectionState, player: int, options: DarkCloudOptions) -> bool:
        return items_available(state, player, DHCGeoItems.ids.keys()) and self.got_accessible(state, player, options)
