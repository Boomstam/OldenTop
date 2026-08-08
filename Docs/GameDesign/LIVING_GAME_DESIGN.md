# Prehistoric Tribe Game — Living Design Reference

> **Status: exploratory and subject to change.**
>
> This document captures the current game concept and the full set of design ideas extracted from the initial brainstorming conversation. Nothing here should be treated as final merely because it is written down. Numbers, terminology, historical framing, systems, content, and even core assumptions may change through prototyping and playtesting.

## How to read this document

- **Current direction** records an idea that the brainstorming converged on.
- **Proposal to test** records a concrete suggestion that has not yet been validated.
- **Open question** records an unresolved decision.
- **Example** illustrates an intended possibility rather than prescribing final content or balance.
- Later clarifications take precedence over earlier suggestions. Superseded ideas remain noted where they explain the design's evolution.

The game is intended to have a rich simulation underneath a compressed, legible player-facing representation. The design should create stories through interacting systems rather than primarily through scripted choice cards.

## 1. High concept

Players control prehistoric tribes on a shared, procedurally generated hex map. Every turn is one season. Players commit individual tribe members to places and tasks, while hidden but already-determined world events alter the conditions under which those assignments resolve.

The game began as a worker-placement concept, but the stronger current description is:

> A simultaneous, spatial worker-programming game about managing a living culture across seasonal cycles.

The central seasonal question is:

> Where do I send each irreplaceable person, and what do I ask them to do, given what I know about the world and what I think is about to happen?

A person is simultaneously:

- labor;
- a mouth to feed;
- a repository of skills and cultural knowledge;
- a potential teacher or learner;
- a family member and part of a future generation;
- vulnerable to age, hunger, exposure, predators, conflict, disease, and calamity;
- potentially a specialist such as a hunter, craftsperson, sailor, builder, or priest.

Losing a person therefore means more than losing one action. A tribe might lose its only advanced flintknapper, bowyer, firemaker, navigator, or interpreter of omens.

### Design pillars

1. **People are the technology tree.** Knowledge lives primarily in individuals and can be learned, taught, forgotten, or lost through death.
2. **The world acts before players commit.** Randomness is determined first and revealed later, making uncertainty a strategic information problem rather than an arbitrary post-decision die roll.
3. **Seasonal survival and generational continuity coexist.** Players prepare for winter while watching several generations pass during one game.
4. **The map remembers history.** Settlement, farming, depletion, fire, floods, roads, monuments, and recovery visibly transform the landscape.
5. **Prosperity, Prestige, and Sacrality form one economy.** All three are valuable, spendable, and interconnected.
6. **Calamities reshape history.** Eclipses are known macro-clocks whose hidden consequences test different kinds of tribes and leave permanent or semi-permanent changes.
7. **Systemic stories over prescribed dilemmas.** Interesting choices should emerge from people, knowledge, ecology, geography, information, and social interaction colliding.

## 2. World and map

### 2.1 Scale and generation

- The world is a hex map, initially imagined at roughly **20 × 20 hexes** for prototyping.
- It is randomly generated for every game.
- Generation should produce coherent, natural-feeling geography rather than independent noise:
  - several distinct woodland patches;
  - rivers that flow as connected systems;
  - lakes;
  - sensible relationships between terrain and resources;
  - interesting travel, settlement, and strategic choices.
- The intended scale is larger than a Settlers of Catan board and smaller than a Civilization-style world.

### 2.2 Terrain

Initial terrain types:

- **Woodland**
  - timber;
  - fallen wood, small sticks, large sticks, and logs;
  - berries;
  - nuts;
  - mushrooms;
  - many animal populations, especially deer and some dangerous animals;
  - possible hidden features such as nut groves, berry patches, beehives, ancient trees, and migration routes.
- **Grassland**
  - hare;
  - mushrooms;
  - berries;
  - space for building;
  - convertible into farmland or pasture;
  - grazing and migratory opportunities.
- **Mountain**
  - firestone;
  - flint;
  - building stone;
  - decorative or “pretty” stone;
  - salt deposits/mines;
  - caves as natural shelter;
  - wolves and bears;
  - comparatively rarer than woodland or grassland;
  - possible features such as springs, sheltered valleys, seams, outcrops, and cave systems.
- **Water**
  - lakes occupy hexes;
  - reeds useful for rope;
  - fish;
  - fresh water;
  - fast movement once boats are available.

### 2.3 Rivers

**Hard constraint:** rivers occupy the **edges of the hex grid**. Every river segment runs along an actual hex side, and connected segments meet at hex corners. Rivers must never be drawn from one hex center to another or pass through hex centers. Lakes remain water-terrain hexes; rivers are a separate edge layer.

Rivers may:

- provide water and fish;
- support irrigation;
- enable boat movement;
- impede crossing until a ford, bridge, or suitable boat is available;
- flood adjacent lowlands;
- create fertile floodplains;
- connect multiple parts of the map into one hydrological system.

### 2.4 Layered map state

The map should separate several layers:

1. **Terrain:** woodland, grassland, mountain, lake; highly persistent.
2. **Site/resource:** nut grove, berry patch, timber stand, mushrooms, reeds, flint seam, salt deposit, and so on; usually persistent but destroyable or replaceable.
3. **Current abundance:** high, normal, poor, or exhausted; fluctuates with years, seasons, exploitation, and events.
4. **Landscape transformation:** rarer changes such as woodland becoming clearing, grassland becoming farmland, flood creating marsh, fire opening forest, mine collapse, or river-course change.

### 2.5 Discovery and geographical knowledge

Terrain should contain discoverable features rather than functioning only as a resource color. Players may not initially know every site.

Geographical knowledge can live in people just like craft knowledge. A hunter might know the location of a remote flint outcrop or migration route. If that person dies before sharing the knowledge, the tribe may lose access to the information.

### 2.6 Human transformation of the world

Possible transformations include:

- woodland → clearing → pasture or farmland;
- grassland → farmland or pasture;
- field exhaustion;
- pasture overgrazing;
- slow forest regrowth;
- resource depletion and ecological recovery;
- animal migration or local disappearance under hunting pressure;
- roads, crossings, bridges, irrigation, settlements, storage, and monuments appearing;
- rivers changing course;
- floods creating marsh or fertile land;
- mines and caves collapsing or new resources being exposed.

After many turns, the board should visibly tell the history of the tribes. Prosperity should be partly visible in the altered landscape.

## 3. Tribes and people

### 3.1 Starting conditions

- Each player controls one tribe.
- The game starts in **Spring**.
- A tribe should begin with a mixture of ages rather than only same-aged adults.
- Initial assignment/movement was tentatively limited to the starting location and neighboring spaces. Exact placement and travel range remain an **open question**.

### 3.2 Person attributes

Each person is currently expected to have:

- sex/gender;
- age group;
- nutrition state;
- location;
- assigned task;
- a small, readable set of skills/knowledge;
- possibly health, pregnancy, or other temporary states where needed.

The player-facing representation should remain compressed—for example, age icon, sex, nutrition marker, and roughly two to four visible skill icons—rather than exposing a large character sheet.

### 3.3 Age groups and compressed time

Ageing is intentionally **not realistic in calendar length**. Turns must remain seasons because preparing for winter is essential, while accelerated aging allows the player to experience generational change within a playable game.

Initial proposal to test:

| Age group | Approximate duration | Role |
|---|---:|---|
| Infant | 2 seasons | Fragile, needs food and care, contributes no or almost no direct labor |
| Child | 3 seasons | Sturdier; learns while helping with suitable tasks |
| Adult | 7–8 seasons | Main skilled and physical workforce |
| Elder | Until death; average about 3 seasons | Fragile, uncertain lifespan, strong knowledge and teaching role, suited to non-physical tasks |

These timings are placeholders for testing.

### 3.4 Age-specific roles

- **Infants** require care and sufficient food and may die quickly when neglected.
- **Children** can learn while contributing to light tasks such as foraging, watching infants, helping adults, and tending hearths.
- **Adults** drive physical work and skilled production.
- **Elders** retain and pass on knowledge, can perform less physical tasks, and should be disproportionately effective teachers. Their impending but uncertain death creates pressure to preserve rare skills.

### 3.5 Pregnancy and care

Initial proposal:

- pregnancy lasts one season;
- a pregnant woman cannot perform physical labor during that season;
- infant care creates a further labor cost;
- a woman caring for an infant may be unable to begin another pregnancy until the infant becomes a child, as a compressed anti-snowball rule rather than a claim of literal realism.

Exact reproduction rules, player control, representation, and demographic pacing are open questions.

### 3.6 Population as both power and liability

More people create more potential actions, which risks a runaway loop: more workers → more food → more children → more workers. Population must therefore impose meaningful costs:

- food;
- winter shelter;
- infant care;
- training and transmission of knowledge;
- clothing;
- settlement space;
- crowding and disease risk;
- exposure to population-scaled calamities.

A small, skilled, well-fed tribe should be capable of outperforming a large, unprepared one.

## 4. Nutrition, food, and survival

### 4.1 Nutrition states

Current proposed ladder:

**Starving → Hungry → Stable → Well-fed → Fat**

- Starving people have a chance of dying.
- Better nutrition improves productivity through Well-fed.
- Fat does not further increase productivity but buffers one additional season of hunger.

Exact transition rates, food costs, death risks, and modifiers are open for balance testing.

### 4.2 Diet quality

Avoid tracking detailed macronutrients or vitamins per person. Food can instead belong to broad groups, such as:

- plant foods;
- animal foods;
- staples;
- special foods.

Sufficient quantity maintains nutrition. Diverse meals improve meal quality and may raise people to better nutrition states. A monotonous staple supply can prevent starvation without necessarily producing Well-fed people. Feasts may improve nutrition rapidly.

This preserves roles for hunting, fishing, herding, and foraging even after farming becomes productive.

### 4.3 Fire and the hearth

Fire should be a fundamental settlement state/economy, not merely one skill unlock. It connects:

- wood to warmth;
- cooking;
- food preservation;
- protection;
- craft production;
- ritual;
- light;
- winter survival.

Maintaining the hearth provides useful work for children, elders, pregnant people, and others unable to perform heavy labor. Loss of reliable firemaking knowledge should be a serious early-game threat.

## 5. Knowledge, skills, and teaching

### 5.1 Knowledge as technology

Technology resides primarily in people. People gain ability by doing tasks, learn from teachers, and teach others. A tribe can lose access to a technology when its last knowledgeable person dies.

Learning should avoid visible numerical XP where possible. A proposed compact ladder is:

**Untrained → Familiar → Skilled → Master**

Some skills, such as basic foraging, should be faster to learn from scratch than complex ones such as toolmaking, firemaking, advanced hunting, or priestly interpretation.

Knowledge must be fragile enough to create meaningful succession decisions without being lost so frequently that optimal play is always redundant teaching.

### 5.2 Work and teaching modes

Proposal to test:

- **Work alone:** maximum immediate output for that worker.
- **Apprenticeship:** teacher and learner work together; output is lower than two independent experts but not necessarily limited to exactly one worker's output; the apprentice learns.
- **Instruction:** little or no production, but faster or more flexible learning.

Illustrative example only:

- Master alone: 3 output.
- Novice alone: 1 output.
- Master + apprentice: around 3–3.5 output and learning progress.
- Pure instruction: 0 output and accelerated learning.

Elders may provide an explicit learning bonus.

### 5.3 Skill families

The initial skill/knowledge space includes:

- **Foraging**
  - berries;
  - nuts;
  - mushrooms, including ritual mushrooms;
  - fallen sticks and other gathered plant materials.
- **Hunting and animal processing**
  - deer;
  - hare;
  - fowl;
  - aurochs;
  - possibly boar, wolves, and bears;
  - protection from dangerous animals;
  - extracting meat, hides, leather, bones, antlers, shells, and other secondary products.
- **Clothing and adornment**
  - hide/leather preparation;
  - clothes;
  - jewelry and decorative objects.
- **Fighting**
  - conflict with people;
  - defense against predators;
  - bows and arrows, spears, clubs, axes, and related tactics.
- **Woodcutting and timber handling**
  - cutting wood;
  - separating or processing small sticks, large sticks, timber, and logs.
- **Toolmaking and weapon making**
  - spear;
  - axe;
  - club;
  - bow and arrow;
  - needles;
  - baskets;
  - ropes;
  - other stone, bone, antler, shell, hide, and wood tools.
- **Mining and stonework**
  - firestone;
  - flint;
  - building stone;
  - decorative stone;
  - preparing and working stone;
  - mining salt.
- **Fishing, sailing, and reeds**
  - fishing;
  - boat use and water travel;
  - gathering reeds for rope and construction.
- **Building and infrastructure**
  - hide/wood houses and tents;
  - shelter;
  - roads;
  - monuments;
  - food storage;
  - arsenals or weapon storage;
  - irrigation;
  - river crossings.
- **Cooking and food preservation**
  - cooking;
  - smoking;
  - salting;
  - drying;
  - fermenting;
  - producing salt by boiling water.
- **Herding and domestication**
  - cattle from aurochs;
  - milk from females;
  - males as breeding stock, bulls, and beasts of burden;
  - dogs from wolves;
  - pigs from wild boar;
  - possible chicken and cat relationships, subject to historical reframing.
- **Farming**
  - grain;
  - legumes;
  - flax for rope or textiles.
- **Priesthood and ritual**
  - sacrifices;
  - rituals;
  - feasts;
  - omens and divination;
  - interaction with the gods/world of fate.
- **Firemaking and fire tending**
  - reliable ignition;
  - maintaining hearths;
  - using fire safely and productively.
- **Geographical/ecological knowledge**
  - locations of hidden sites;
  - migration routes;
  - seasonal resource patterns;
  - safe routes and crossings.

### 5.4 Historical scope and taxonomy

The feature set spans a very broad prehistoric period: foraging, stonework, mining, agriculture, domestication, roads, irrigation, monuments, and sailing. The current framing can embrace this as a compressed era of cultural development rather than claiming to depict one precise prehistoric year.

Taxonomy and historical relationships require later research. Known cautions from the brainstorm:

- aurochs → cattle, wolves → dogs, and wild boar → pigs are useful relationships;
- chicken ancestry should not be represented as generic “flightless birds” but principally as junglefowl;
- cat domestication is better framed as a commensal relationship involving settlements, rodents, and wildcats rather than straightforward hunting/domestication.

## 6. Seasonal economy

### 6.1 Four-season cycle

Every turn is one season, cycling:

**Spring → Summer → Autumn → Winter**

Seasonality should follow recognizable natural rhythms:

- Spring brings appropriate emerging foods and resources and is the sowing season.
- Summer brings summer resources and growing-season labor.
- Autumn brings resources such as mushrooms, harvesting, and preparation for the next agricultural cycle.
- Winter sharply reduces gathering and makes shelter, warmth, clothing, stored food, and fire essential.

Shelter protects against wild animals in all seasons and becomes necessary to avoid freezing in winter.

### 6.2 Farming

Early idea: plough in Autumn, sow in Spring, harvest in Summer.

**Later proposal to test:**

1. Autumn: clear/plough.
2. Spring: sow.
3. Summer: tend/protect.
4. Autumn: harvest.

Agriculture should occupy labor for much of a year, carry crop-failure risk, and reward the investment with large, storable yields.

This creates distinct subsistence profiles:

- **Foraging:** immediate, local food with little setup.
- **Agriculture:** repeated investment and risk for a potentially enormous store.
- **Herding:** mobile, renewable, resilient, and lower-yield than a great harvest.
- **Hunting:** uncertain and low-setup with valuable secondary products.
- **Fishing:** steady but geographically constrained.

### 6.3 Animals

Animals should move, but the game should model populations or herds rather than every individual animal.

- Deer herds may occupy woodland regions and migrate seasonally.
- Aurochs can move between grassland and woodland.
- Fish abundance fluctuates.
- Predators move toward prey, livestock, or settlements.
- Overhunting reduces or collapses populations.
- Domestication converts uncertain, moving ecological resources into controlled but labor-intensive resources.

### 6.4 Sustainable and intensive exploitation

Players should choose between sustainable and aggressive extraction. Examples:

- nut grove: harvest 2 sustainably or strip it for 4 and reduce future abundance;
- timber: gather dead/fallen wood or cut aggressively for much more timber;
- fishing: ordinary catch or intensive fishing that reduces future stock;
- animal population: hunt selectively or push the herd toward collapse;
- reeds: cut carefully or strip the bank.

Human choices and the Gods/world system jointly shape ecology. A poor berry year may be the delayed result of player overuse rather than divine punishment.

### 6.5 Uncertain recovery

**Current direction:** an exhausted site may eventually recover without returning as the same resource. Recovery can reroll or replace the site.

Depletion therefore sacrifices both sustainability and certainty: something will return, but players cannot rely on it being the resource they removed.

## 7. Turn structure and hidden fate

### 7.1 Core principle

All event randomness and targets should be fixed before workers are committed. Avoid outcomes such as “players commit to the river, then a flood and its target are rolled.” Instead, the flood and affected river already exist; players simply do not know them unless they investigate or infer them.

### 7.2 Three horizons of hidden information

The Gods/world can hold up to three concurrent hidden plans:

| Hidden plan | Determined | Duration |
|---|---|---|
| Year Condition | At the beginning of Spring | Entire year |
| Seasonal Event | At the beginning of every season | Current season |
| Eclipse Calamity | At game setup or immediately after the previous Eclipse | Until the next Eclipse |

During Year 3, the hidden **Eclipse Season** is also fixed for that year's Spring, Summer, Autumn, or Winter.

“The Gods' turn” represents uncontrolled fate and world events; it does not require every event to be literally supernatural.

### 7.3 Proposed seasonal sequence

1. **Gods phase**
   - No player decisions.
   - In Spring, place the new Year Condition face down.
   - Every season, place a Seasonal Event face down.
   - Predetermine any targets, paths, locations, or other event details.
   - In Year 3, Eclipse timing tokens are already assigned.
2. **Divination/ritual phase**
   - Before assignments, eligible players may spend Sacrality to inspect hidden plans privately.
3. **Commitment phase**
   - Players place/commit people and assign their tasks, likely simultaneously.
4. **Reveal and execution**
   - Reveal relevant Year, Season, and Eclipse information.
   - Resolve world changes and task modifiers.
   - Execute the committed tasks under those conditions.
5. **Upkeep and aftermath**
   - Resolve consumption, nutrition, learning, births, aging, death, recovery, and event aftermath in an order to be specified.

The target for this full seasonal loop was suggested as roughly **10–15 minutes**, but that timing is not yet validated.

### 7.4 Event timing vocabulary

Use only a few clear timing windows:

- **BEFORE WORK:** the world changes immediately.
- **DURING WORK:** the event modifies actions this season.
- **AFTER WORK:** consequences occur after tasks.

Example:

> **Flash Flood**  
> BEFORE WORK: river-adjacent grassland floods.  
> DURING WORK: flooded hexes cannot be farmed or built on.  
> AFTER WORK: unprotected food stored on flooded hexes is destroyed.

## 8. Divination and information play

### 8.1 Current information rule

An early suggestion considered granular or ambiguous omens. The later clarification supersedes it:

> **Spend 2 Sacrality to privately inspect one eligible hidden Gods card/token and see its exact information.**

No vague hint is required. The balancing lever is which hidden plan a person's priestly knowledge permits them to inspect.

Proposed access progression:

- basic ritual: inspect the current Seasonal Event;
- experienced priest: inspect the Year Condition;
- advanced priest: inspect the Eclipse Calamity;
- astronomical/sacred expertise: inspect Eclipse timing.

Costs, access tiers, and whether a priest must take an action remain proposals to test.

### 8.2 Information as a multiplayer resource

Private knowledge can be:

- kept secret;
- sold for goods;
- exchanged for favors;
- shared with allies;
- lied about;
- inferred from another player's suspicious assignments or preparations.

There need not be a formal truthfulness rule. Information warfare should arise without a large espionage subsystem.

The three victory currencies can interact through information:

- Sacrality buys foresight directly.
- Prosperity buys information economically.
- Prestige obtains cooperation, trust, or social access.

### 8.3 Advanced priestly abilities

Possible priest abilities should manipulate or mitigate fate without erasing it:

- **Interpretation:** inspect Year or Eclipse plans.
- **Ritual Protection:** protect one hex from one specified effect.
- **Appeasement:** reduce part of a calamity.
- **Sacrifice:** convert goods or animals into Sacrality.
- **Prophetic Authority:** publicly predict an event and gain Prestige if correct.
- **Sacred Ground:** protect a settlement from certain effects.
- **Celestial Knowledge:** inspect Eclipse timing.
- **Rite of Renewal:** accelerate post-disaster recovery.

Players should not be able to pay enough Sacrality to cancel a calamity completely. They may save one settlement, prevent deaths, protect selected stores, or otherwise reduce harm, but the event must still change the world's history.

## 9. Year Conditions

Year Conditions establish broad ecological tendencies for all four seasons. They should make years feel qualitatively different rather than simply applying a universal ±1.

Proposed examples:

- **Wet Year**
  - mushrooms replenish strongly;
  - berries are good;
  - reeds are abundant;
  - rivers are high;
  - floods become more likely/stronger;
  - wood is harder to dry.
- **Dry Year**
  - berries and mushrooms are poor;
  - reeds are reduced;
  - firewood dries well;
  - pasture deteriorates;
  - wildfire becomes more dangerous.
- **Mast Year**
  - nuts are exceptionally abundant;
  - boar populations increase;
  - woodland animals concentrate around nut resources.
- **Cold Year**
  - Spring foods arrive late;
  - Winter resources worsen;
  - hides and clothing matter more;
  - predators linger near settlements.
- **Warm Year**
  - growing season lengthens;
  - agriculture benefits;
  - food may spoil faster;
  - water shortages become more dangerous.
- **Great Bloom**
  - grassland forage increases;
  - hare and fowl thrive;
  - grazing improves;
  - later fire risk grows.

Because the Spring assignment occurs before the Year Condition is publicly revealed, the first commitment of each year contains genuine uncertainty unless someone divines it.

## 10. Seasonal Events

Seasonal Events are narrower and more immediate than Year Conditions. They should include benefits and opportunities as well as danger.

### 10.1 Event families and candidate events

- **Weather**
  - Heavy Rain;
  - Heat Wave;
  - Late Frost;
  - Early Snow;
  - Storm;
  - Dense Fog;
  - Dry Wind;
  - Sudden Thaw;
  - Long Rain;
  - Mild Weather.
- **Plant/resource**
  - Mushroom Flush;
  - Berry Bloom;
  - Nut Mast;
  - Poor Blossom;
  - Reed Explosion;
  - Seed Failure;
  - Rot;
  - Wild Honey;
  - Flax Blight.
- **Animal**
  - Deer Migration;
  - Aurochs Migration;
  - Fish Run;
  - Wolf Pack;
  - Bear Awakens;
  - Boar Surge;
  - Fowl Nesting;
  - Rodent Explosion;
  - Predators Driven Hungry.
- **Landscape** — comparatively rare
  - Flood;
  - Wildfire;
  - Landslide;
  - Cave Collapse;
  - Riverbank Collapse;
  - Spring Appears or Dries Up.
- **Human/social**
  - Wanderers Arrive;
  - Raiders;
  - Refugees;
  - Travelling Craftsperson;
  - Rival Hunting Party;
  - Neighboring Tribe Requests Aid;
  - Feud;
  - Runaway/Exile.
- **Sacred/strange**
  - Unusual Moon;
  - Comet;
  - Animal Omen;
  - Sacred Birth;
  - Lightning Strike;
  - Unseasonal Darkness;
  - Strange Animal Behavior.

Sacred/strange events need not prove literal magic; they are events people interpret religiously.

### 10.2 Opportunity examples

**Great Salmon Run**

- DURING WORK: fish sites produce +2 this season.
- Catching a threshold amount may also generate Prestige.
- Result: several tribes suddenly compete for or negotiate access to the river.

**Aurochs Migration**

- BEFORE WORK: move a herd through a predetermined route of grassland hexes.
- DURING WORK: hunting and domestication attempts along the route gain bonuses.
- Danger to unprotected people along the route rises.

**Great Mushroom Flush**

- Mushroom sites produce heavily.
- Priests can collect rare ritual mushrooms.
- Unskilled foragers may face a food-safety risk.

### 10.3 State-dependent effects

Events should care about the existing map and player state:

- wildfire interacts with woodland, dry conditions, wind, fire, settlements, and cleared land;
- flood interacts with rivers, elevation, storage, boats, settlements, and farmland;
- wolves interact with livestock, children, hunters, dogs, and shelters.

The same event should be disastrous in one game and exploitable in another.

## 11. Eclipses and great calamities

### 11.1 Macro-clock

The proposed large-scale rhythm is one Eclipse every **three years**, or twelve seasons.

- Small clock: Spring → Summer → Autumn → Winter.
- Large clock: Year 1 → Year 2 → Year 3 → Eclipse.

Players continually balance preparing for the next Winter against preparing for the next Eclipse.

The Eclipse date is partly predictable, while its consequences are hidden but determined far in advance.

### 11.2 Hidden Eclipse timing

At the beginning of Year 3, assign four hidden celestial tokens to Spring, Summer, Autumn, and Winter:

- one **ECLIPSE**;
- three **Ordinary Moon**.

Reveal the corresponding token only during each season's execution. If Spring is ordinary, Summer becomes a one-in-three possibility; if Summer is ordinary, Autumn is one-in-two; Winter becomes certain if all earlier seasons were ordinary. Suitable priestly knowledge can inspect timing tokens.

### 11.3 Calamity determination

- At game setup, secretly determine the first Eclipse Calamity.
- Immediately after each Eclipse, secretly determine the next one.
- The coming crisis therefore exists roughly three years before it occurs and can be investigated and prepared for.

The eventual name for these cards/events should be more thematic than the generic development term “calamity.”

### 11.4 Calamity structure

Each great calamity should have:

1. **Shock:** an immediate dramatic effect.
2. **Crisis:** rules affecting the whole Eclipse season.
3. **Aftermath:** a permanent or semi-permanent world change.

Great calamities must be qualitatively larger than ordinary seasonal events and test the civilization a player built, not merely remove a fixed amount of food.

### 11.5 Candidate calamities

#### The Great Hunger

- **Shock:** each tribe reduces stored edible food to a maximum of 8.
- **Crisis:** natural food yields are reduced.
- **Aftermath:** some resource sites become Poor during the following year.
- **Primary test:** food efficiency and population size; large stored surpluses cannot make a tribe automatically invulnerable.

#### The Great Deluge

- **Shock:** river-adjacent lowlands flood and unprotected stores there are destroyed.
- **Crisis:** crossing rivers requires boats/bridges; fishing improves; floodplain buildings are threatened.
- **Aftermath:** some grasslands become exceptionally fertile and a river may change course.
- **Primary test:** geography and infrastructure; a prepared river civilization may benefit.

#### The Black Winter

- Occurs as a severe cold collapse regardless of the current season.
- **Shock:** temperature plummets.
- **Crisis:** every person needs shelter and active fire; unsheltered people lose nutrition and risk death; gathering falls; firewood use rises.
- **Aftermath:** animals migrate and some sites temporarily disappear.
- **Primary test:** wood, fire, shelter, clothing, and storage.

#### The Great Drought

- **Shock:** minor water sources dry up.
- **Crisis:** unirrigated agriculture collapses; pasture deteriorates; animals crowd around surviving water; wildfire risk rises.
- **Aftermath:** some grassland degrades and surviving water becomes more strategically valuable.
- **Primary test:** water access, agriculture, geography, and mobility.

#### The Burning Land

- **Shock:** several predetermined woodland areas ignite.
- **Crisis:** fire spreads through connected woodland according to wind and Year conditions; people may fight it.
- **Aftermath:** burned woodland becomes open ground; some resources disappear; later fertility may improve.
- **Primary test:** geography, land management, and firefighting.

#### The Beast Year

- **Shock:** predators emerge or migrate into multiple regions.
- **Crisis:** wolves and bears threaten livestock, children, and solitary workers; killing them yields major Prestige and valuable hides/bones; dogs, weapons, fighting, shelters, and hunting skill become important.
- **Aftermath:** prey may rebound if predators were reduced or collapse if predators succeeded.
- **Primary test:** hunting, military capacity, and protection.

#### The Great Sickness

- Avoid arbitrary random deaths; risk should depend strongly on state.
- **Shock:** large concentrated settlements become vulnerable.
- **Crisis:** crowding and poor nutrition increase danger; dispersal, good nutrition, herbal knowledge, sanitation, and priestly practices mitigate it.
- **Aftermath:** survivors may gain some resistance.
- **Primary test:** nutrition and settlement pattern; checks mega-settlement population growth.

#### The Raiding Host

- The Gods' phase includes uncontrolled human events, not only celestial ones.
- **Shock:** a large outside group enters the map.
- **Crisis:** they seek food, animals, valuable materials, or territory; players may fight, hide, pay tribute, flee, ally, or misdirect them.
- **Aftermath:** survivors may remain as immigrants, enemies, or trading partners.
- **Primary test:** fighting, Prestige, and diplomacy; victory against them creates great Prestige.

#### The Failing Earth

- **Shock:** mountain and cave regions become dangerous.
- **Crisis:** mines collapse, roads break, stone structures are damaged, and caves become inaccessible.
- **Aftermath:** new mineral resources may be exposed.
- **Primary test:** mining and built infrastructure.

#### The Darkened Sun

- The Eclipse itself causes widespread fear.
- **Shock:** each tribe enters a social crisis.
- **Crisis:** high Sacrality and Prestige preserve cohesion; low values lead to fear, lost productivity, desertion, or unrest; priests become especially powerful.
- **Aftermath:** surviving successfully can generate major Sacrality.
- **Primary test:** Sacrality and Prestige rather than primarily material stores.

### 11.6 Diverse tests and heroic opportunities

No single preparation—such as stockpiling 50 food—should solve every calamity. Different crises should reward different cultural strategies.

Calamities should also create heroic opportunities:

- rescue another tribe's people during a flood → Prestige;
- kill a dangerous bear during Beast Year → Prestige;
- feed outsiders during famine → major Prestige;
- defeat raiders → major Prestige;
- save a sacred structure from fire → Prestige and/or Sacrality.

Prepared players may intentionally exploit a known calamity rather than merely endure it.

## 12. Prosperity, Prestige, Sacrality, and victory

### 12.1 The three standings

#### Prosperity — what the tribe possesses

Possible sources:

- food stores;
- herds;
- valuable raw materials;
- crafted goods;
- infrastructure;
- productive land;
- buildings and other current wealth.

Prosperity is naturally spent or lost when goods are consumed, traded, gifted, sacrificed, destroyed, or invested.

#### Prestige — what people believe about the tribe

Possible sources:

- generosity and feasts;
- heroic action;
- successful dangerous hunts;
- defeating raiders or winning conflict;
- monuments and communal achievements;
- diplomacy and visible social power.

Possible uses:

- call in favors;
- attract temporary followers;
- negotiate passage;
- claim priority;
- organize communal works;
- secure alliances;
- influence other tribes.

Military capability is **not itself Prestige**. It consists of people, weapons, and fighting knowledge. Using it successfully can create Prestige, while military defeat may reduce Prestige.

#### Sacrality — the tribe's relationship with the supernatural order

Possible sources:

- sacrifices;
- rites;
- feasts;
- priests/priestesses;
- sacred sites or monuments;
- surviving and interpreting sacred events.

Possible uses:

- read omens and hidden plans;
- request limited protection or mitigation;
- sanctify places;
- perform major rites;
- understand Eclipse timing and calamities.

### 12.2 Conversions and interaction

The standings should not be three isolated minigames.

- Prosperity + labor → feast → Prestige and/or Sacrality.
- Prosperity + labor → monument → Prestige and Sacrality.
- Prestige → alliances, labor, influence, and access → Prosperity.
- Sacrality → foresight and mitigation → protects Prosperity.

Feasts may be one of the central conversion actions. They consume large quantities of food, drink, animals, decorations, and labor. Their conduct may determine whether the result emphasizes Prestige, Sacrality, or both.

### 12.3 Victory proposal

Original idea: Prosperity + Prestige + Sacrality reach a target total.

Problems identified:

- spendable points encourage late-game hoarding;
- one mathematically efficient track could dominate and make the others irrelevant.

Current proposal to test:

- victory is evaluated only at major calendrical moments, especially after each Eclipse;
- resolve the calamity, workers, survival, and permanent aftermath first;
- then evaluate the three standings;
- require both a total threshold and a minimum in every category.

Illustrative balance example only:

- total of at least 30;
- no category below 5.

This permits distinct tribes such as 18 Prosperity / 7 Prestige / 5 Sacrality and 7 / 12 / 11 without permitting total neglect of one cultural dimension.

The intended rhythm is:

> spend → build → spend → consolidate → judgment

Pre-Eclipse spending should be tense: spending Sacrality on foresight reduces current score but may prevent much larger losses.

## 13. Social interaction, trade, and conflict

The shared map and private information should create interaction without requiring a separate espionage game.

Possible interactions include:

- trading goods, materials, animals, or knowledge;
- selling or sharing prophecy;
- bluffing or lying about hidden events;
- inferring information from another tribe's movement and construction;
- negotiating access, passage, priority, or resource rights;
- requesting or providing aid;
- alliances and favors purchased with Prestige;
- competition for seasonal opportunities;
- warfare, raiders, territorial pressure, and defense;
- heroic rescue or generosity during crisis.

Exact rules for simultaneous commitment, contested spaces, negotiation windows, promises, lying, combat, and player elimination remain open questions.

## 14. Representation and bookkeeping constraints

The design currently contains a large potential state space:

- up to roughly 400 hexes;
- individual people with age, sex, nutrition, location, tasks, skills, learning, health, and family states;
- many raw materials and food types;
- buildings, herds, crops, weather, event plans, territory, and ecological change.

The guiding rule is:

> Complex simulation underneath; extremely compressed representation for the player.

Design implications:

- avoid numerous per-person numerical stats;
- prefer a few state bands and icons;
- prefer skill tiers over XP totals;
- model animal populations rather than individuals;
- use broad food groups rather than nutritional accounting;
- use Year Conditions for correlated ecological variation rather than rerolling every resource independently;
- limit event timing to a few windows;
- keep seasonal resolution deterministic once assignments are committed and hidden plans are revealed;
- make the UI carry bookkeeping without obscuring causality.

The simulation should not be reduced prematurely, but every system must eventually justify its cognitive and interface cost.

## 15. Illustrative emergent dilemma

This scenario captures the intended kind of decision:

It is Autumn of Year 3. The Eclipse may come next Spring. The tribe has eleven people. Its only master flintknapper is an elder. A grain crop needs four people to harvest before Winter. A priest has learned that water will bring death—or, under the later exact-information rule, may know the specific hidden water calamity—but other players may not know what the priest knows. The strongest settlement lies beside a river. Neighbors have boats. A child needs one more season of apprenticeship to preserve flintknapping. Stored food will cover Winter, but a large feast could bring enough Prestige and Sacrality to contend for victory at the Eclipse.

The player may need to choose among:

- harvesting;
- teaching the child;
- building or moving uphill;
- constructing boats;
- performing further divination;
- holding the feast;
- moving people into the mountains;
- trading with a neighbor who may know more.

The goal is for almost all of this dilemma to emerge from systemic interaction rather than from a card that simply asks the player to choose option A or B.

## 16. Current unknowns and prototype questions

The brainstorm deliberately leaves many values and mechanisms unsettled. Important open questions include:

### Core loop

- What is the exact order for movement, work, competition, teaching, consumption, nutrition, pregnancy/birth, aging, death, recovery, and event aftermath?
- Are assignments fully simultaneous, phased, or initiative-based?
- How long does a digital season take in practice?
- What information remains visible while players commit?

### People and time

- Exact age durations and elder mortality curve.
- Exact pregnancy, birth, caregiving, and population-control rules.
- Whether sex/gender creates rules beyond pregnancy and how to represent it thoughtfully.
- How many skills one person can know and display.
- Skill learning rates, decay, teaching capacity, and succession safeguards.

### Map and movement

- Final map size and player count.
- Starting placement, starting knowledge, and initial travel/assignment radius.
- Movement cost, range, roads, boats, river crossing, and remote work.
- Territorial control, shared hex use, and settlement rules.
- Procedural-generation constraints that produce fair but non-symmetrical starts.

### Economy and survival

- Resource quantities, storage, spoilage, and inventory abstraction.
- Nutrition transitions and productivity effects.
- Exact fire, clothing, shelter, and Winter rules.
- Sustainable versus intensive extraction rates and recovery times.
- Agriculture's final seasonal sequence and yields.
- Herd lifecycle, domestication, migration, and predator behavior.

### Gods and events

- How Year Conditions become public during reveal.
- Whether base divination always costs 2 Sacrality.
- Which priest tier can inspect which plan and whether inspection consumes an action.
- Event targeting constraints and how to communicate predetermined targets clearly.
- Size and composition of initial Year, Season, and Calamity decks.
- Final thematic terminology for the Gods area and calamities.
- How much mitigation is possible without invalidating the macro-clock.

### Social and victory systems

- Player count and negotiation model.
- Formal versus informal trades, promises, and lies.
- Combat and the consequences of war.
- Exact uses of Prestige.
- Exact sources and uses of Sacrality.
- What counts toward Prosperity and when values update.
- Victory threshold, minimum-category requirement, ties, and whether every Eclipse is a victory checkpoint.
- Game length if nobody wins at the first Eclipse.

### Scope, interface, and historical framing

- Which systems belong in the first playable prototype.
- Which resources can be combined without losing meaningful decisions.
- How the UI reveals causality and avoids an accounting-simulator feel.
- Precise prehistoric inspirations, names, animal taxonomy, and cultural sensitivity.
- Whether apparently supernatural effects remain ambiguous, literal, or dependent on the chosen presentation.

## 17. Recommended first prototype boundary

This is a prioritization suggestion from the brainstorming, not a committed production plan.

First validate the anatomy of a single season and the hidden-fate loop before adding the full technology and content breadth:

- a small coherent hex map;
- a few people with age, nutrition, and limited skills;
- placement, movement, work, and apprenticeship;
- Spring/Summer/Autumn/Winter;
- food, shelter, fire, and one or two resource chains;
- one hidden Year Condition;
- a compact Seasonal Event deck;
- Sacrality-based exact inspection;
- deterministic reveal and resolution;
- one Eclipse timing cycle and a few distinct calamities;
- basic Prosperity, Prestige, and Sacrality accounting.

The brainstorming suggested that, after validating the loop, a useful content-design milestone would be approximately:

- 30 concrete Seasonal Events;
- 10 Year Conditions;
- 10 Eclipse Calamities;

Those counts are provisional.

## 18. Change policy

This reference is intentionally a snapshot of evolving thought.

- Treat every numeric value as provisional unless later promoted into a tested rules document.
- Keep rejected or superseded ideas identifiable when they explain why the current rule exists.
- When a prototype answers an open question, record the result and the evidence rather than silently rewriting history.
- Separate thematic possibility from verified historical fact.
- Prefer explicit versioned decisions over assuming this brainstorm is binding.
