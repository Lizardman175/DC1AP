from dataclasses import dataclass
from Options import Choice, Toggle, PerGameCommonOptions, Range


class Goal(Range):
    """Select Dungeon from 2-6 to be the goal.  Currently only 2 is supported."""
    display_name = "Boss Goal"
    default = 2
    range_start = 2
    range_end = 2


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
    # option_char = 2  # TODO handle? Idea is dungeons only open when chars that can handle obstacles are available rather than just enough chars to prevent a crash.
    # TODO can we open door locks in the dungeons so all chars aren't required with the open?


class MiracleSanity(Toggle):
    """Currently doesn't do anything but change item classification for certain items. Only added for now to begin logic coding for MCs.
    Don't use if you find this!!"""
    display_name = "Miracle Sanity"
    default = 0
    # TODO make visible with MC shuffle update
    visibility = Visibility.none

# TODO death link.
# class DeathLink(DeathLink):


@dataclass
class DarkCloudOptions(PerGameCommonOptions):
    boss_goal: Goal
    all_bosses: AllBosses
    open_dungeon: OpenDungeon
    miracle_sanity: MiracleSanity

