from BaseClasses import CollectionState

class RuleManager:

    def xiao_available(self, state: CollectionState, player: int) -> bool:
        #TODO might be able to remove the collect_item implementation using state.count("Progressive...")?
        return state.has("Stray Cat", player) and state.has("Gaffer's Lamp", player) and state.has("Pike", player)

    def dran_accessible(self, state: CollectionState, player: int) -> bool:
        return state.has("Dran's Sign", player) and \
            self.xiao_available(state, player)

    def goro_available(self, state: CollectionState, player: int) -> bool:
        return state.has("Matataki River E", player) and state.has("Cacao's Laundry", player) and \
            self.xiao_available(state, player)

    def utan_accessible(self, state: CollectionState, player: int) -> bool:
        return state.has("Balcony", player) and self.goro_available(state, player)

    def ruby_available(self, state: CollectionState, player: int) -> bool:
        return state.has("King's Lamp", player) and self.goro_available(state, player)

    def saia_accessible(self, state: CollectionState, player: int) -> bool:
        return state.has("Holy Mark", player) and state.has("Yaya's Sign", player) and \
            self.ruby_available(state, player)

    def ungaga_available(self, state: CollectionState, player: int) -> bool:
        return state.has("Sisters' Odds & Ends", player) and self.ruby_available(state, player)

    def curse_accessible(self, state: CollectionState, player: int) -> bool:
        return state.has("Chief Bonka's Cabin 2", player) and state.has("Zabo's Hay", player) and \
            state.has("Enga's Roof", player) and self.ungaga_available(state, player)

    def osmond_available(self, state: CollectionState, player: int) -> bool:
        return self.ungaga_available(state, player)

    def joe_accessible(self, state: CollectionState, player: int) -> bool:
        # Just need to finish the head for the admission ticket.
        return state.has("Eye (HD)", player) and self.ungaga_available(state, player)

    def got_accessible(self, state: CollectionState, player: int) -> bool:
        return False

    def genie_accessible(self, state: CollectionState, player: int) -> bool:
        # return items_available(state, player, DHCGeoItems.ids.keys()) and self.got_accessible(state, player, options)
        return False
