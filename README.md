# Exhaustible Resources v1.0

### A mod for Offworld Trading Company

[Offworld Trading Company](http://www.offworldgame.com/) is an "economic RTS"
by Mohawk Games (founded by Soren Johnson, the lead developer of Civilization 4)
about the colonization and economic exploitation of Mars.

This is a pretty small mod I worked on primarily as a proof-of-concept, to find
out just how moddable Offworld Trading Company actually is. As of this writing
(July 2016), there are very few other mods out there. But Civilization 4 was
an extremely moddable game, and one of the only games I've actually released
mods for, and Offworld *does* support modding, so I was curious.

In Offworld Trading Company, you can establish mines and quarries on resource
deposits. But (as in many other strategy games) the resources you are mining never
run out! That low deposit of iron you establish a mine on at the beginning of the
game will remain a low deposit of iron for the rest of the game (unless someone 
detonates an underground nuke beneath it, of course).

This has always struck me as unrealistic (probably because I came into the
strategy genre through Age of Empires), so combined with my curiosity about
Offworld moddability, I decided to see what I could do to change that.

## Installation

Download a zip archive of the mod from [here](https://github.com/TC01/ExhaustibleResources/releases)
and unzip it. Then copy the "Exhaustible Resources" folder into your Offworld\Mods
folder directory, e.g.

```
C:\Users\username\Documents\My Games\Offworld\Mods\
```

Then, launch Offworld Trading Company, and under "Options - Gameplay", for "Game
Mod", select "Exhaustible Resources".

## Features

Exhaustible Resources implements one new feature. At the end of every Martian day,
the game will randomly iterate over all tiles with resources that have buildings
mining them.

Currently, there is a 10% chance (but this is configurable and subject to change
as necessary) that a resource being mined, quarried, or pumped will be "exhausted"
down to the next resource level. So a High Iron deposit may suddenly become a
Medium Iron Deposit, or a Low Water deposit will become a Trace Water deposit.

Deposits will not go below Trace.

### Limitations

Most of the limitations are, unfortunately, to do with telling the player what's
happening. For more accurate information about resource exhaustion, check the Offworld log
file in e.g. C:\Users\username\Documents\My Games\Offworld\Logs\

* The event messages do not tell you *where* your mines depleted a resource, they
simply say what resource it occurred at. I wasn't able to figure out how to trigger
an event *at* a tile.

* The notifications (ab)use the game's event system, meaning I believe they will
fire for all players.

* It was my intention to add a Game Option for enabling this feature, but the list
of GameOptions are hardcode as an enum in the DLL, and it wasn't immediately
apparent how I could override that.

### Configuration

If you wish to change the probability of daily depletion, adjust ```EXHAUSTIBLE_RESOURCES_DEFAULT_THRESHOLD```,
a value in Exhaustible Resources\Data\globals-init-add.xml, from any value between 0 and 100.
The mod rolls a random number (up to 100) and checks if it's below the threshold; if it
is, the resource is depleted, otherwise, nothing happens. (So 100 would mean "always deplete",
for instance).

## Links

* [Github Repository](https://github.com/TC01/ExhaustibleResources): the mod's git
repository.

## Credits

Developers:

* Ben 'TC01' Rosser

This is, of course, *not* an official project of Mohawk Games or Stardock,
the developers / publishers / owners of Offworld Trading Company. Thanks to them
for making a cool (and moddable) game!

This wouldn't have been possible without the [mod tutorials](http://www.mohawkgames.com/2016/02/03/modding-tutorials/)
provided by Jason Winakur of Mohawk.

None of the above parties are liable for any damage that this mod inflicts
upon your computer. 

The mod itself is as "open source" as a mod for a proprietary video game can
get, and any original content and code should be assumed to be under the
MIT License (see "LICENSE"). That means you are free to reuse any new content
or code in this mod in other projects as you see fit, without asking permission
(be they other Offworld Trading Company mods or something else entirely).