# Valheim - Harder Feather Fall Mod

**_Works with Ashlands Update!_**

Have you ever thought the Feather Cape seemed too OP? Especially outside of the Mistlands biome? Me too. That's why I made this mod.

This mod features two modes of play:

1. Feather Cape draws its power from Wisplight. When equipped together, the Feather Fall effects work normally. Otherwise, your fall speed and fall damage protection are proportionally bound to your available stamina. Each becomes more perilous the less stamina you have. Where 100% stamina will provide the expected fall speed and damage reduction benefits, 0% stamina feels like the cape isn't even equipped when falling... or landing.
2. Alternatively, using Stamina Mode Override, Feather Fall will drain stamina while aloft if Wisplight unequipped. A dead man's drop awaits if you reach 0% stamina before reaching the ground. The drop damage is calculated proportional to the fall velocity at point of impact.

Users of this mod beware. This is not a QOL improvement, it seeks to reduce the power of the Feather Cape outside of normal Mistlands play.

## Features

* Feather Cape now requires Wisplight to be equipped in order to function normally
* Absent the Wisplight effect, Feather Fall effects are bound to your available stamina, losing more of their effectiveness the lower your stamina, but the cape does not *consume* your stamina unless Stamina Mode Override is enabled
* With v0.2.0, configure the mod in Stamina Mode Override. This overrides the normal mod behavior to allow Feather Fall effects to work normally, but their usage steadily drains stamina if Wisplight is unequipped. Don't let it get to zero!
  * Hardcore mode drains stamina regardless of Wisplight. Only active if Stamina Mode Override is enabled. Like in normal mode, hardcore mode calculates fall speed/damage by your available stamina at point of impact. Zero stamina means zero protection. This is the most difficult play mode.

## Config

Configuration allows:

* **Enable Mod**, Enable the mod, default: true
* **Hard mode: Enable even when Wisplight equipped**, Enable even when Wisplight equipped, default: false (Makes play significantly harder)
* **Drains stamina while aloft when Wisplight unequipped**, Uses stamina to stay aloft without Wisplight, default: false
* **Stamina drain unit (larger drains faster)**, "Stamina drain unit", default: 10
* **Hardcore mode: Drains stamina & affects fall speed/damage (only works if Stamina Mode Override enabled)**, "Uses stamina while aloft, regardless of Wisplight, & affects fall speed/damage", default: false (Hardest mode of play)

Built with [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)

![toggle-movement-mod](https://raw.githubusercontent.com/afilbert/valheim-harder-feather-fall-mod/main/doc/img/HarderFeatherFallMod.png)

## Releases

Releases in github repo are packaged for Thunderstore Mod Manager.

* 0.2.2 Support The Bog Witch update
* 0.2.1 Drop requirement for Official BepInEx ConfigurationManager from Thunderstore manifest.json to allow flexibility
* 0.2.0 Major feature. Adds optional new game mode where Feather Fall ceases to work when stamina reaches 0%.
  * Alternative game mode means that fall speed and damage modifiers remain unchanged as long as you have available stamina
  * Deadman drop at 0% stamina while still aloft
  * Fully configurable and composable with original settings, combine to increase difficulty
  * Configure rate at which stamina drains while aloft
  * Fix potentially fatal bug that didn't fully disable damage modifiers when toggling the Enable Mod option off in midair
  * Fix bug that could cause multiple applications of damage, one for each active status effect
* 0.1.1 Fix Thunderstore manifest `website_url`
* 0.1.0 Initial publication
