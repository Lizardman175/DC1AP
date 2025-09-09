from dataclasses import dataclass

from Options import Choice, Toggle, PerGameCommonOptions, Range


# TODO Other possible goal options:
#  fish - catch a mardan? or get the upgraded sword? x fish points?
#  triforce hunt style thing of collect X item?
#  demon shaft - I don't wanna...
class Goal(Range):
    """Select Dungeon from 2-6 to be the goal.  Currently only 2 is supported."""
    display_name = "Boss Goal"
    default = 2
    range_start = 2
    range_end = 2


class AllBosses(Toggle):
    """Requires every boss up to the goal boss in order to finish the game."""
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


# TODO
# class MiracleChests(Choice):
#     """Shuffle Miracle Chests that spawn as the game progresses as location checks.  Only those from towns within the
#       selected goal will be randomized.  The three glitched chests by Treant will not be randomized.
#       (Option currently disabled!)"""
#     option_exclude = 0
#     # option_include = 1
#     default = 0
#     display_name = "Miracle Chests"

# TODO death link.
# class DeathLink(DeathLink):

# TODO other option ideas

@dataclass
class DarkCloudOptions(PerGameCommonOptions):
    boss_goal: Goal
    all_bosses: AllBosses
    open_dungeon: OpenDungeon
    # miracle_chests: MiracleChests
