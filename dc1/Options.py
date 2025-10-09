from dataclasses import dataclass
from Options import Choice, Toggle, PerGameCommonOptions, Range, Visibility


class Goal(Range):
    """Select Dungeon from 2-6 to be the goal.  Currently only 2-3 are supported."""
    display_name = "Boss Goal"
    default = 3
    range_start = 2
    range_end = 3

class AllBosses(Toggle):
    """Requires defeating every boss up to the goal boss in order to finish the game."""
    display_name = "All Bosses"
    default = 0

class OpenDungeon(Choice):
    """Open all dungeon floors as they become logically available."""
    display_name = "Open Dungeon"
    default = 1
    option_closed = 0
    option_open = 1

class BetterStartingWeapons(Toggle):
    """Give each character a Tier 1 weapon in addition to their unbreakable starter."""
    display_name = "Better Starting Weapons"
    default = 1

class MiracleSanity(Toggle):
    """Currently doesn't do anything but change item classification for certain items. Only added for now to begin logic coding for MCs.
    Don't use if you find this!!"""
    display_name = "Miracle Sanity"
    default = 0
    # TODO make visible with MC shuffle update
    visibility = Visibility.none

class AbsMultiplier(Choice):
    """Adjust the ABS gained from enemies."""
    display_name = "ABS Multiplier"
    option_half = 0
    option_normal = 1
    option_one_and_half = 2
    option_double = 3
    option_double_and_half = 4
    option_triple = 5
    default = 3

# TODO haven't found a way to make the miracle chests despawn.  Even setting the flag doesn't seem to do anything.
# class GivePockets(Toggle):
#     """Start with all available pockets received based on settings."""
#     display_name = "Start With Pockets"
#     default = 0

# TODO death link.
# class DeathLink(DeathLink):

@dataclass
class DarkCloudOptions(PerGameCommonOptions):
    boss_goal: Goal
    all_bosses: AllBosses
    open_dungeon: OpenDungeon
    starter_weapons: BetterStartingWeapons
    miracle_sanity: MiracleSanity
    abs_multiplier: AbsMultiplier
    # give_pockets: GivePockets
