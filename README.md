# Valheim - Harder Feather Fall Mod

**_Works with Mistlands Update!_**

Have you ever thought the Feather Cape seemed too OP? Especially outside of the Mistlands biome? Me too. That's why I made this mod.

This mod features two modes of play:

1. Feather Cape draws its power from Wisplight. When equipped together, the Feather Fall effects work normally. Otherwise, your fall speed and fall damage protection are proportionally bound to your available stamina. Each becomes more perilous the less stamina you have. Where 100% stamina will provide the expected fall speed and damage reduction benefits, 0% stamina feels like the cape isn't even equipped when falling... or landing.
2. Alternatively, Feather Cape will drain stamina while aloft. A dead man's drop awaits if you reach 0% stamina before reaching the ground.

Users of this mod beware. This is not a QOL improvement, it seeks to reduce the power of the Feather Cape outside of normal Mistlands play.

## Features?

* Feather Cape now requires Wisplight to be equipped in order to function normally
* Absent the Wisplight effect, Feather Fall effects are bound to your available stamina, losing more of their effectiveness the lower your stamina, but the cape does not *consume* your stamina
* With v0.2.0, configure the mod to drain stamina while Feather Cape is in use. This detaches the dependency on the Wisplight by default. Settings allow you to regulate how quickly stamina is used while aloft, and also allows combinations of the Wisplight teather and stamina drain to increase play difficulty

## Config

The [Official BepInEx ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) is a required dependency.

Configuration allows:

* **Enable Mod**, Enable the mod, default: true
* **Override: Enable even when Wisplight equipped**, Enable even when Wisplight equipped, default: false (Makes play significantly harder)

Built with [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)

![toggle-movement-mod](https://raw.githubusercontent.com/afilbert/valheim-harder-feather-fall-mod/main/doc/img/HarderFeatherFallMod.png)

## Releases

Releases in github repo are packaged for Thunderstore Mod Manager.

* 0.2.0 Major feature. Adds optional new game mode where Feather Fall ceases to work when stamina reaches 0%.
  * Alternative game mode means that fall speed and damage modifiers remain unchanged as long as you have available stamina
  * Deadman drop at 0% stamina while still aloft
  * Fully configurable and composible with original settings, combine to increase difficulty
  * Configure rate at which stamina drains while aloft
* 0.1.1 Fix Thunderstore manifest `website_url`
* 0.1.0 Initial publication
