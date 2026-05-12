# tld personal mods 2.55

**The Long Dark 2.55** personal customization layer — config snapshot + self-made mods on top of a 117-mod community pack.

> ⚠️ **Configs and my own mods only — does NOT include third-party mod binaries.** To use this repo, install the 117-mod pack first, then overlay `configs/Mods/`* on top. Third-party mod copyrights belong to their respective authors.

> 中文版见 [README.md](README.md). This is the detailed English introduction.

---

## What's included

```
├── BunkerDefaults/         ← My original mod, free to use/modify
│   ├── BunkerDefaults.dll      Compiled, drop into Mods/
│   └── src/                    C# source + project, dotnet build
├── FoodStackable/          ← My original mod, merged into TldHacks (kept here for history)
│   ├── FoodStackable.dll
│   └── src/
├── TldHacks/               ← My original all-in-one cheat/QoL mod, includes FoodStackable + full CT cheats
│   ├── TldHacks.dll            (with v2.7.0+ no need to also install FoodStackable)
│   └── src/                    C# source (13 files) + HANDOFF.md dev log
├── ModSettingsQuickNav/    ← My original mod, adds "quick-jump" list to ModSettings.dll
│   ├── ModSettingsQuickNav.dll
│   └── src/
├── configs/                ← Config snapshots for all mods (203 files)
│   ├── Mods/                   All *.json / *.txt under Mods/ (no .dll)
│   └── UserData/               MelonLoader global preferences
├── tools/
│   └── PopulateFastTravel.ps1  PS script: manually batch-inject FastTravel waypoints
├── CHANGELOG.md            ← Detailed list of changes made to upstream mods
└── LICENSE
```

## What's NOT included (and why)


| Type                                  | Reason                                                                   |
| ------------------------------------- | ------------------------------------------------------------------------ |
| 117-mod pack `.dll` / `.modcomponent` | Other people's code, can't redistribute without permission               |
| MelonLoader runtime                   | Get from [LavaGang/MelonLoader](https://github.com/LavaGang/MelonLoader) |
| The game itself                       | Buy it                                                                   |
| Save files (`sandbox*` / `*.moddata`) | Private data                                                             |


---

## BunkerDefaults — original mod

**Problem solved:** Pathoschild's FastTravel mod requires you to **physically walk** to a location before saving it as a waypoint. This mod scans `Mods/ModData/*.moddata` at game start and **pre-fills 9 region bunker coordinates** into the FastTravel save, so new playthroughs work out of the box.

**Hotkeys (inherits FastTravel's, reordered in v1.0.3 by preference):**


| Key  | Destination                         | Scene name           | Default coord                               |
| ---- | ----------------------------------- | -------------------- | ------------------------------------------- |
| `5`  | Forsaken Airfield emergency shelter | AirfieldRegion       | **Empty, walk to save**                     |
| `6`  | Mystery Lake bunker                 | LakeRegion           | ✅                                           |
| `7`  | Coastal Highway gas station         | CoastalRegion        | **Empty, walk to save**                     |
| `8`  | Mountain Town bunker                | MountainTownRegion   | ✅                                           |
| `9`  | Desolation Point bunker             | CanneryRegion        | ✅                                           |
| `F2` | Whaling Station outpost             | WhalingStationRegion | **Empty, walk to save**                     |
| `F3` | Blackrock bunker                    | BlackrockRegion      | ✅                                           |
| `F4` | Ash Canyon bunker                   | AshCanyonRegion      | ✅                                           |
| `F7` | Mountain Pass / Timberwolf bunker   | MountainPassRegion   | **Empty** (community Y=207 falls off cliff) |


5 with `✅` work by default. The 4 `Empty` ones — walk to the scene first time, press `**=` (Equals) + the matching number** to save real coords. CheatEngine community-table coords for MountainPass / Marsh both clip through the world / underground in tests, removed from default injection. Rural / RiverValley / Marsh also removed by user preference (save them yourself when needed).

**Workflow for a new save (important):**

1. Create a new sandbox, play until first save trigger (sleep / `ESC→Save` / scene change)
2. BunkerDefaults injects 9 waypoints into `<save>.moddata` on next scene change
3. `**ESC → Main Menu → Reload save`** — forces FastTravel to refresh its in-memory cache
4. Press `5` to teleport directly to Mystery Lake bunker

Step 3's reload is **a one-time per-save thing**, never needed again afterward.

---

## FoodStackable — original mod

**Problem solved:** vanilla inventory shows each food item (Jerky / CannedBeans / Soda etc.) in its own slot. Pick up 6 of one, get 6 separate slots. Visual clutter.

**How it works:** hooks `Panel_Inventory.RefreshTable`, deduplicates same-type items (same prefab + same opened state) inside `m_FilteredInventoryList`, displays only one representative cell + an `x6` counter badge in the bottom-right (reuses the game's built-in `m_StackLabel`).

**Doesn't touch the underlying inventory** — `Inventory.m_Items` still has all 6 GearItem entries:

- Eat one → 5 remain, next refresh shows `x5`
- Each ages independently per its own condition, save data stays clean
- Dropping/storing only affects the representative entry

**Bonus:** representative = the lowest-condition entry (the game's filter naturally sorts ascending by condition + my first-wins strategy) → eating consumes the rotten ones first, optimal survival behavior.

**Skipped:** pure liquids (LampFuel / JerrycanRusty / water) and Soda dual-types (vanilla already handles these).

**Version highlights** (full changelog in Chinese README):

- `v0.4.0–0.4.9` (2026-04-27) initial UI dedup + counter badge, then iterative bugfixes:
  - weight label per-stack display (handles imperial/metric)
  - blacklist for items with hidden quirks (`MixedNuts`, cards, etc.)
  - dict key migration (`gi.Pointer` → `dataItem.Pointer` → `gi.name`) to fix mis-merging
  - StackManager compatibility tightening
  - per-frame `OnLateUpdate` reapply to survive game's label-clearing on click/drag
- `v2.7.0` (2026-04-28) **merged into TldHacks**. Cell-reuse bug final fix: `SeenItems` stores `(item, di, giPtr)`, every frame verifies `item.m_GearItem.Pointer == giPtr` — mismatch means the cell got rebound to a different gear (scrolling/category change), skip and wait for `RefreshDataItem.Postfix` to re-register. Old standalone `FoodStackable.dll` kept for history; new installs use `TldHacks.dll`.

---

## TldHacks — original all-in-one cheat/QoL mod

**What it does:** ports most features from the Cheat Engine `The Long Dark.CT` table into a MelonMod + Harmony patch implementation. Press `Tab` (rebindable) to open a 3-tab IMGUI menu (Main / uConsole / Items&Teleport). ~60 toggles cover godmode, infinite stamina, all afflictions immunity, weapon enhancements, aim stability, fast craft/firestart, 15-region teleport, 911-entry item spawner (358 vanilla + 553 auto-scanned mod items with `[ModName]` tag), max skills, unlock feats/blueprints/maps, freeze cold value, and more.

**Key implementation notes:**

- **Il2CppInterop strips all of GUILayout** — only `GUI.Xxx(Rect, ...)` works → menu code hand-codes every coordinate, not GUILayout-style
- *Aim/weapon uses the game's built-in `m_DisableAim` bool fields** (vp_FPSWeapon / vp_FPSCamera.m_DisableAmbientSway) — more reliable than patching getters, toggles sync both ways
- **Most uConsole commands are no-ops in the release build**, only the `spawn` family is solid; critical cheats are Harmony-patched directly
- **Stacking hooks `RefreshDataItem.Postfix` at cell-bind time**, not at the click symptom layer; `OnLateUpdate` uses giPtr verification to prevent cell-reuse from writing stale data

**Bilingual UI (v3.0.4+):** the mod menu and HUD messages now switch between Chinese/English based on `I18n` runtime setting. Item categories, jump-blocked HUD, disease-cleared logs, and 26 teleport waypoints all carry `*En` fields and dispatch through `DisplayLabel` / `DisplayCategory()`.

**Full design / implementation details / pitfall log** in `TldHacks/src/HANDOFF.md` (400+ lines).

**Version:** v3.0.4 — bilingual UI + Architect/SafehouseCustomization integration + 9-mod merge under previous v3.0.x line.

---

## ModSettingsQuickNav — original ModSettings extension

**Problem solved:** with 50+ mods installed, `ModSettings.dll`'s mod-settings panel only lets you click arrows to flip through them one at a time. Finding a specific mod takes dozens of clicks.

**How it works:** Harmony-patches `ModSettings.ModSettingsGUI.OnEnable/OnDisable` (both internal classes, accessed via `AccessTools.TypeByName` + reflection bypass). When the ModSettings tab is active, an IMGUI toggle button appears in the top-right + a ``` (backquote) hotkey opens an overlay:

- A-Z letter jump (13×2 grid of buttons)
- Filter clear
- Scrolling list of all mods, click any to jump directly
- Internal switching done via reflection on `ModSettingsGUI`'s private `SelectMod(string)`

Works in main menu and in-game pause menu. Doesn't touch TldHacks, runs independently.

---

## Quick machine-restore

```powershell
# 1. Install your modpack into <TLD>/Mods/
# 2. Clone this repo
git clone https://github.com/Jcxu97/tld--2.55.git

# 3. Overlay configs
cp -r tld--2.55/configs/Mods/*       <TLD>/Mods/
cp -r tld--2.55/configs/UserData/*   <TLD>/UserData/
cp    tld--2.55/BunkerDefaults/BunkerDefaults.dll       <TLD>/Mods/
cp    tld--2.55/TldHacks/TldHacks.dll                   <TLD>/Mods/
cp    tld--2.55/ModSettingsQuickNav/ModSettingsQuickNav.dll <TLD>/Mods/
# With TldHacks v2.7.0+ skip FoodStackable — functionality is built in
```

## Known issues

- **Modpack scene-change occasional crash + boot hang on restart**: a known issue with the 117-mod pack. Press `F5` (SaveManager) to quicksave before scene transitions; on hang, retry restart / reboot the PC. See `CHANGELOG.md`.
- **DarkerNights.dll (v1.3 by Xpazeman)**: broken on TLD 2.55, loading it causes an init deadlock. **Do NOT install it.** The author's other mods (AmbientLights / PlacingAnywhere / HouseLights / GearDecayModifier) still work.
- **StackManager's `AddStackableComponent` only affects newly-spawned items**: existing soda/jerky in your inventory still won't stack, you'd need to re-pick them up from a fresh container.

## License

- Everything I wrote under `BunkerDefaults/` / `FoodStackable/` / `TldHacks/` / `ModSettingsQuickNav/`: MIT (see `LICENSE`)
- The `configs/` directory is just a snapshot of my settings — config formats belong to each mod's author, this is just "what values I used". Take whatever you like.

---

# Recommended companion mod set

> Curated baseline mod pack to use alongside TldHacks. **Excludes** story regions / clothing-food content packs / aesthetic skins — keeps only **framework + core gameplay reinforcement**.
> Content packs (`.modcomponent`) are user-added as needed, not part of this set.

---

## 1. Required frameworks (8)

Without these TldHacks won't work or external mods fail to load.


| dll                     | Purpose                                                                  |
| ----------------------- | ------------------------------------------------------------------------ |
| MelonLoader             | Mod framework (install before the game)                                  |
| ModSettings.dll         | Mod config panel entry point, TldHacks's detailed params depend on it    |
| ModData.dll             | Used for TldHacks serialization                                          |
| ModComponent.dll        | Loads `.modcomponent` extension item packs                               |
| ComplexLogger.dll       | Shared logging library, multiple mods depend on it                       |
| AudioManager.dll        | Audio replacement API, multiple mods depend on it                        |
| AfflictionComponent.dll | Disease/status extension API                                             |
| ExamineActionsAPI.dll   | Item Examine interaction extension API                                   |
| DeveloperConsole.dll    | Required for TldHacks's ConsoleBridge (refresh-trader etc. depend on it) |


## 2. Core


| dll                         | Purpose                                                               |
| --------------------------- | --------------------------------------------------------------------- |
| **TldHacks.dll**            | Main body of this set, integrates 30+ standalone mod features         |
| disable_integrated_mods.bat | Disable script — renames mods integrated into TldHacks to `.disabled` |


## 3. Safehouse + transport (user-picked)


| dll                                        | Purpose                                                      |
| ------------------------------------------ | ------------------------------------------------------------ |
| **SafehouseCustomizationPlus.dll**         | Customize safehouse — change indoor layout / item placement  |
| **Architect.dll + Architect.modcomponent** | **Press Y to move stoves/furniture**, free-form construction |


## 4. Map / navigation (3)


| dll                     | Purpose                                                                                                                  |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| MapManager.dll          | Player arrow / centering / survey range / no-Polaroid / full-reveal / bunker hotkey (TldHacks no longer integrates this) |
| MotionTrackerLite.dll   | Motion-tracker radar, alerts on nearby wildlife                                                                          |
| ModSettingsQuickNav.dll | Quick-jump nav for ModSettings panel                                                                                     |


## 5. Item management (4)


| dll                        | Purpose                                     |
| -------------------------- | ------------------------------------------- |
| GearInfo.dll               | Item details panel (weight / HP / category) |
| GearToolbox.dll            | Equipment toolbox, batch organize           |
| ItemPicker.dll             | Pickup enhancement + auto-stack hint        |
| InventoryReassignments.dll | Inventory / hotbar reorder                  |


## 6. General reinforcement (3)


| dll                  | Purpose                                                           |
| -------------------- | ----------------------------------------------------------------- |
| SaveManager.dll      | Multi-save / manual save                                          |
| PlacingAnywhere.dll  | Place items anywhere (outdoors / on roads)                        |
| BlueprintCleaner.dll | Recipe list cleanup (TldHacks has `unpatch`ed its broken Postfix) |


## 7. Compatibility fixes (2, optional)


| dll                    | Purpose                                      |
| ---------------------- | -------------------------------------------- |
| BricklayersDoorFix.dll | Fixes door interaction bug from certain mods |
| RavineBridgeFix.dll    | Fixes some bridge collision bugs             |


---

## Total

- **Required**: 8 (framework) + 1 (TldHacks) = **9 dlls**
- **Strongly recommended**: 1 (safehouse) + 1 (Architect) + 3 (map) + 4 (items) + 3 (reinforcement) = **12 dlls** + 1 modcomponent
- **Optional**: 2 (compatibility fixes)
- **Grand total**: 23 dlls + 1 modcomponent + 1 bat

Small footprint, fast launch, stable.

---

## Player-added (not in this set)

Add by personal preference, **all compatible with this set**:

- Difficulty: MiseryModePlus / Minor_Miseries / SeasonedInterloping / InterloperHudPro / StalkerAidsAndSupplements
- Content extensions (`.modcomponent`): FoodPackByTKG / ClothingExpanded / FirePack / CampingTools / IndoorsGreenery / ZC8787 series etc.
- Aesthetic skins: RetroFood / RetroTextures
- Region mods: LitharsRidge
- Misc: AlcoholMod / WildFire / RestandReadMod / PineNeedleTea / EdiblePlants / Bountiful_Foraging / FortifiedLookouts / PrepperCache

---

## NOT recommended (already integrated into TldHacks, will conflict)

`disable_integrated_mods.bat` automatically disables these 33:
SonicMode / Jump / GunZoom / VehicleFov / FastTravel / StretchArmstrong / TorchTweaker / KeroseneLampTweaks /
AutoToggleLights / GearDecayModifier / BowRepair / DisableAutoEquipCharcoal / RememberBreakDownItem /
DroppableUndroppables / CraftAnywhereRedux / MoreCookingSlots / TimeScaleHotkey / FullSwing / SilentWalker /
QoL / AutoSurvey / TinyTweaks-(MapTextOuline/NoSaveOnSprain/WakeUpCall/BuryHumanCorpses) /
SleepWithoutABed / SkipIntroRedux / PlaceFromInventory / CaffeinatedSodas / Sprainkle / RnStripped /
ExtraGraphicsSettings / UniversalTweaks / StackManager

- known conflicts (GfxBoost / LightCull / DarkerNights)

---

## Installation

1. Install MelonLoader (TLD 2.55+)
2. Copy all files in this set to `<TLD>/Mods/`
3. Double-click `disable_integrated_mods.bat` (one-time, renames integrated redundancies to `.disabled`; will say "not found" if those mods aren't in your `Mods/` directory)
4. Launch the game, press `Tab` to open the TldHacks menu

---

# Modpack diff log

Baseline: `[2026-04-21] V2.54 modpack / [V2.54-modpack] / [Step 2] / Mods`
Current: `D:\Steam\steamapps\common\TheLongDark\Mods` (TLD 2.55)

## Removed from upstream pack (6)


| DLL                    | Note                                                                                                      |
| ---------------------- | --------------------------------------------------------------------------------------------------------- |
| `DarkerNights.dll`     | ⚠️ **Must remove** — last updated 2023-01-24 (TLD 2.06), causes init deadlock on 2.55. See "Known issues" |
| `CatchColdMod.dll`     | not installed                                                                                             |
| `OxygenLevels.dll`     | not installed                                                                                             |
| `PhotoOfALovedOne.dll` | not installed                                                                                             |
| `ReducedLoot.dll`      | not installed                                                                                             |
| `TheLongMood.dll`      | not installed                                                                                             |


## Added on top (7)


| DLL                       | Source / use                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `BunkerDefaults.dll`      | **This repo's original** — pre-fills 9 bunker FastTravel waypoints per save                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| `FoodStackable.dll`       | **This repo's original** (v0.4.9, merged into TldHacks v2.7.0) — visual same-type food stacking ×N, doesn't touch underlying items                                                                                                                                                                                                                                                                                                                                                                                                          |
| `TldHacks.dll`            | **This repo's original** (v3.0.4) — all-in-one cheat. Replaces FoodStackable (includes its full functionality) + ports ~90 cheats from the Cheat Engine table: godmode / immunities / infinite stamina / aim stability / no recoil / fast craft / 100% firestart / 5× rope climb / 15-region teleport / **911 item-spawn entries (358 vanilla + 553 auto-scanned mod items, with `[ModName]` tag)** / max skills / unlock feats·blueprints·maps / freeze cold value (snapshots current on enable, restores natural change on disable), etc. |
| `ModSettingsQuickNav.dll` | **This repo's original** (v1.0.0) — quick-jump for ModSettings panel: press ``` to open IMGUI overlay, A-Z letter jump + scrolling list click to switch directly to a mod's settings. Harmony patches `ModSettingsGUI.OnEnable/OnDisable` (internal class via `AccessTools.TypeByName` reflection)                                                                                                                                                                                                                                          |
| `AudioCore.dll`           | dependency for some other mod                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| `ImprovedTrader.dll`      | enhances merchant interactions                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| `RecipeRequirements.dll`  | recipe requirement adjustments                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |


## Extra modcomponents

- `Rare-ExclusiveLoot.modcomponent`
- `ZC8787JerkyChipsblueprints.modcomponent`

## Major config changes

### `Mods/FastTravel.json`

- `OnlyBetweenDestinations`: `true` → `**false**`
  - Original setting required your current position to also be a saved waypoint
  - Changed: teleport from **any position** to the 9 bunkers
- `LogDebugInfo`: kept `false` (debug spam)

### `Mods/StackManager/config.json`

Added these items to `STACK_MERGE` + `AddStackableComponent`:

- `GEAR_Soda` — Soda
- `GEAR_SodaEnergy` — Energy drink
- `GEAR_BeefJerky` — Beef jerky
- `GEAR_CannedBeans` — Canned beans
- `GEAR_SprayPaintCan` — Spray paint can

⚠️ Only takes effect on **newly-spawned** items; pre-existing inventory items still won't stack.

### `Mods/ItemPickerCustomList.txt`

Auto-pickup list expanded from ~130 lines to **627 lines** (cumulative across 3 batches on 2026-04-27: 68 clothes + 25 misc + a full sweep of `catalog.json` adding 246 items). New categories:

- **Ammo / weapon consumables**: `Bullet`, `GunpowderCan`, `ScrapLead`, `ArrowHead`, `Accelerant`, `RifleCleaningKit`, `SharpeningStone`, etc.
- **Building / tools**: `Prybar`, `SimpleTools`, `SewingKit`, `BeeHive`, `Flint`, `WireBundle`, `CarBattery`, `Battery9V`, `Fuse`, `ScrapPlastic`, `ElectronicParts`, `GlassShards`, `NutsNBolts`, `NutsNBoltsBox`, `TarpSheet`, `Charcoal`, `SprayPaintCan`, `FlareA`, `BlueFlare`, `TapeRoll`, `BottleHydrogenPeroxide`, etc.
- **Food / drinks**: `CuredMeat`* (bear/bird/moose/orca/ptarmigan/rabbit/deer/wolf), `SaltedMeat`* (same), `CuredFish*` / `SaltedFish*` (9 fish types), `BirdMeatRaw/Cooked`, `BirdEggRaw/Boiled`, `OrcaMeatRaw/Cooked`, `CannedChili/Stew/Spaghetti/Pears/Mangos/Pineapples/Beans`, `Cooked*` (cakes/stews/scrambled-eggs/pizza/sandwiches), `BabyFood*`, `Tea*`, `CoffeeCupSugar`, etc.
- **Consumables**: `PineNeedle` series, `PineNeedleDried`, `FilmBoxColour/BW/Sepia`, `MapleSyrup`, `GranolaBar`, `CondensedMilk`, `BariumCarbonate`, `LampFuel`, `PackMatches`, `WoolSocks`, `BasicBoots`, `FleeceSweater`, etc.
- **Vanilla canned items missed by upstream**: `Soda`, `SodaEnergy`, `KetchupChips`, `AuroraEnergyDrink`, `CheeseDoodles`, `SwedishMeatballs`, `icecreamCup`, etc.
- **Clothing (68 items added 2026-04-27)**: coats/Parka (14), vests (3), sweaters/hoodies (6), shirts (4), pants (8), boots/shoes (11), socks (2), gloves (9), hats (7), scarves/balaclavas (4). Includes `BearSkinCoat` / `WolfSkinCape` / `Improvised`* and other crafted variants. Prefab names sourced wholesale from `tld_Data/StreamingAssets/aa/catalog.json` (848 entries).
- **Misc gap-filling (cumulative 2026-04-27)**: `SodaGrape`, `DogFood`, `Hacksaw`, `CanOpener`, `RecycledCan`, `HeavyBandage`, `MagnifyingLens`, `CandyBar`, `MashedPotatoes`, `EmergencyStim`, `HighQualityTools`, `HatchetImprovised`, `Hammer`, `Rope`, `RevolverAmmoBox`, `MixedNuts`, `PinnacleCanPeaches`, `InsulatedFlask_C`, `Hatchet`, `Jeans`, `Toque`, `LongUnderwear`, `Flour`, `CookingPot`, `Skillet`.

**Final full sweep (2026-04-27)**: extracted 848 GEAR_ prefabs from `tld_Data/StreamingAssets/aa/catalog.json`, removed `_Mat/_Dif` material entries, added the 246-item difference. Explicitly excluded (122 entries) **story-breaking key items**:

- Story keys (~20: `BIKey1/2`, `LakeCabinKey_`*, `BankManagerHouseKey`, `DepositBoxKey`, etc.) — auto-pickup might break story progression
- Story notes (~70: `BackerNote`*, `VisorNote*`, `Blackrock*Note`, `DeadmanNote1-5`, etc.) — clutters inventory
- Postcards (21: `PostCard_*`) — collectibles
- Carcasses (3: `*Carcass`) — must be harvested, not picked up (`*Quarter` meat chunks kept)
- Water-bottle system (4: `WaterSupply*`, `WaterBottle*`) — water doesn't go through pickup
- Placeholders / creatures / story items (4: `NULL`, `Stalker`, `ElevatorCrank`, large `BackPack_A*`)

Full list in `configs/Mods/ItemPickerCustomList.txt`.

### Other auto-generated mod configs

- `Mods/AmbientLights.json` — ambient lighting defaults (auto-written by mod)
- `Mods/UniversalTweaks.json` — UniversalTweaks tuning (auto-written by mod)
- `Mods/MapManager.json` — map manager defaults
- `Mods/ImprovedTrader.json` / `SilentWalker.json` / `PrepperCache.json` — mod defaults

### `Mods/ModData/*.moddata` (not in repo, per-save)

BunkerDefaults pre-fills the 9 bunker FastTravel waypoints at runtime. Not bundled because moddata is your private save data.

## Known modpack-level issues (not caused by my changes)

- **Scene-change occasional crash → restart hangs at `CraftAnywhere is online` log line**: a known issue with the 117-mod pack. Community workarounds:
  - Press `F5` (SaveManager) to quicksave before scene transitions
  - On boot deadlock, retry restart; if still stuck, reboot the PC
  - Be patient — sometimes Unity is still loading in the background, only the MelonLoader log has gone quiet
- **CampingTools v2.2.1 throws MissingMethodException**: ExamineActionsAPI v2.0.7 changed its interface signature, CampingTools hasn't kept up. Not fatal (CampingTools fails on its own, others continue), but CampingTools features won't work.

---

# Video script (English version)

---

## Opening

Hi everyone, today I'm introducing our TldHacks mod pack.

We've integrated 29 standalone mods into a single DLL.
All features can be toggled and tuned in real-time via an in-game IMGUI menu.
No need to quit the game, no need to edit config files.

---

## Speed & motion

**SonicMode** — super sprint mode. Movement speed, climb speed, crouch speed all adjustable.

**FullSwing** — full-power melee swings. Damage multiplier and knockback strength both maxable.

**VehicleFov** — adjust vehicle FOV, both driving and passenger.

---

## Visuals & performance

**GfxBoost** — one-click visual performance optimization. Shadow distance, LOD, tree render distance — 11 parameters all adjustable.

**ExtraGraphicsSettings** — extra visual settings. Resolution scale, anisotropic filtering, texture quality — 12 parameters.

---

## Weapons & aiming

**GunZoom** — gun aim zoom adjustment.

**BowRepair** — allows bow repair.

---

## Sleep & time

**WakeUpCall** — sunrise wake notifications, aurora awareness, sleep time display, prevent full blackout. Four independent toggles.

**SleepWithoutABed** — sleep anywhere. No bed needed. 13 adjustable parameters: fatigue recovery rate, frostbite coefficient, low-HP interrupt, cooldown, etc.

**TimeScaleHotkey** — time-acceleration hotkey. Press once to speed up, again to restore.

---

## Lighting & torches

**TorchTweaker** — torch parameter tuning. Brightness, burn time, throw distance — all adjustable.

**KeroseneLampTweaks** — kerosene lamp tuning. Fuel consumption, brightness, illumination range — 6 parameters.

**AutoToggleLights** — auto-toggle lights when entering / leaving indoor scenes.

---

## Items & crafting

**CraftAnywhereRedux** — craft anywhere. No workbench required.

**MoreCookingSlots** — more cooking slots. You decide how many things to cook at once.

**CaffeinatedSodas** — sodas contain caffeine, drinking boosts fatigue recovery. 9 parameters control effect strength.

**DroppableUndroppables** — makes undroppable items droppable.

**DisableAutoEquipCharcoal** — don't auto-equip charcoal after surveying.

**RememberBreakDownItem** — remember last tool used for breakdown.

---

## Map & survey

**AutoSurvey** — auto-survey. Maps as you walk, range and delay adjustable. Full reveal also supported.

**MapTextOutline** — map text outline. 0–3 thickness levels.

---

## Stealth & sound

**SilentWalker** — silent footsteps. Walk, run, metal/wood/water/general — 5 audio channels independently controlled.

---

## Gear decay

**GearDecayModifier** — gear decay rate modifier. 37 categories independently adjustable: food, drinks, clothing, tools, weapons — full coverage. Hunting knife / hatchet / hacksaw harvest decay can also be tuned individually.

---

## Sprains & injuries

**Sprainkle** — sprain system overhaul. 16 parameters: sprain probability, weight threshold, recovery time, bandage effect, etc.

**NoSaveOnSprain** — sprains don't trigger autosave. Fall sprains can be controlled separately.

---

## Skips & misc

**SkipIntroRedux** — skip intro animation and warning screens.

**RnStripped** — two features: corpse drag-move + flashlight infinite battery.

**BuryHumanCorpses** — bury human corpses. Hold right-click for progress bar, fully buried = removed. Persists across saves.

---

## Closing

29 mods, one DLL.
Menu has 4 tabs: cheat features, character attributes, quality of life, quick navigation.
All parameters adjustable in real-time, changes take effect immediately.

After installing, run our one-click disable script to disable the original standalone DLLs and avoid duplicate-load conflicts.

Thanks for watching, enjoy.