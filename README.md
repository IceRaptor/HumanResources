# Human Resources
This mod for the [HBS BattleTech](http://battletechgame.com/) game breathes life into the crews onboard the Argo. Aerospace pilots, MechTech crews, MedTech crews, and Vehicle crews can be acquired from the Hiring Hall. These crew require a hiring bonus in addition to their monthly salary, offer contracts of varying lengths, and will have their own loyalties to manage. These crews are mercenaries and will leave at the end of their contract, or when someone makes them a better offer. You'll always have Wang, Sumire and the rest - but otherwise you'll need to keep an eye on the people that form the backbone of your mercenary company.

:information_source: This mod uses icons from [https://game-icons.net/](https://game-icons.net/), which are distributed under [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/). 

**Features**

 * Hirable Aerospace, MechTech, MedTech, and Vehicle crews
 * Expanded management options for all MechWarriors and crews
    * Fully customizable lifepath system for generated crews
    * Customizable salary based upon an exponential formula
    * Crews have a contract length and must be re-hired periodically
    * Crews require a hiring bonus each time they are hired 
    * (Optional) Crews may demand hazard-pay for combat drops
    * (Optional) Crews may demand kill-bonuses for units they destroy in a mission
 * Scarcity of hirable crews driven by a gaussian distribution
    * Scarcity of hirable crews based upon planetary tags
 * Crews have an attitude towards the company and may leave if they become unhappy
    * Crews may have a loyalty or hatred for specific factions. 
 * (Optional) Crews can be head-hunted by other mercenaries companies and start a bidding war

**Warnings**

:warning: This mod requires the [IRBTModUtils](https://github.com/battletechmodders/irbtmodutils/) mod. Download the most recent version and make sure it's enabled before loading this mod.

:warning: This mod introduces new pilot tags (see below). If removed from an active career, the pilots will retain those tags but no definitions will be present for these custom tags. The pilots will have long strings in their 'attributes' section but there should be no other impact. There is unfortunately no convenient way to clean these up during an uninstall. 

## **General Concepts**

This mod creates different types of Pilots, separated between *Combat Crews* (MechWarriors, Vehicle Crews) and *Support Crews* (Aerospace Wings, MechTech Crews, and MedTech Crews).

*Combat Crews* are handled as per the base game, with customizable skills that the player can select. They can be injured or die during combat. The mod controls the distribution of their skills and the salary they require. Vehicle Crews are compatible with [CustomUnits](https://github.com/BattletechModders/CustomBundle/), and will have the `pilot_vehicle_crew` and `pilot_nomech_crew` tags.

*Support Crews* are non-combat units that do not have selectable skills. They instead have a *skill* and *size* rating, which combines to determine a flat value that improve the company's MechTech, MedTech, or Aerospace squad size rating. Both ratings range from 1-5 with customizable labels in `mod_localization.json` (indexed by rating value). Default values are given below:

| Name | 1 | 2 | 3 | 4 | 5 |
| -- | -- | -- | -- | -- | -- |
| Skill | Rookie | Regular | Veteran | Elite | Legendary |
| Size | Tiny | Small | Medium | Large | Huge |

:information_source: This mod does NOT change Ronin pilots, which are the pilots with defined backstories that were backers of the HBS BattleTech Kickstarter. 

### Scarcity

This mod defines **Scarcity** for all crew types, and makes a limited number of crews available on planets. The number of each type of crew per planet can be configured, and can be further modified by the tags defined on the planet. Anywhere the mod refers to scarcity we mean that only a limited number of crews will be available in the hiring fall.

### Distributions

This mod relies upon [Gaussian distributions](https://en.wikipedia.org/wiki/Normal_distribution) (aka normal distributions) for almost all of the random elements. The most common values in a Gaussian distribution are clustered around a central value, with less common values along either side of the curve. 

This central value is defined as *mu* , which is the most common expected value a random sample should return. If you define the mu value as 2.3, then random results will be likely to be in the 1.5 - 3.0 range (depending on sigma). This is the *height* of the curve. 

The second value that influences the distribution is the *sigma* value. This determines how far from the central value a randomly selected value will be. It defines the *width* of the curve. Small sigma values will have most selected values cluster around the expected value, while larger sigma values will have selected values range across the full value. 

Most features in the mod that depend upon a distribution will allow you to modify the selected values by changing the mu value. For instance, the *SkillDistribution* value can be influenced by planet tags. A planet tag will apply a plus or minus value to the mu value used to generate the random value. 

:warning: While both sigma and mu are fully customizable, the default configuration expects values of mu = 1.0 and sigma = 0.0. If you modify sigma and mu, you will need to change most other configuration values to reflect your adjusted distribution.

# Hiring 

The vast majority of the mod's functionality relates to the hiring and generating of random crew members. There are several features within the hiring umbrella:

* *LifePath* - configures the lifepaths used to generate crews
* *Scarcity* - limits many crew are available on a planet by planet basis
* *Skill and Size Distribution* - configures the skill and size frequencies for crews
* *Salary and Bonuses* - defines the money the crew requires as salary, hiring bonuses, hazard pay, and others
* *Crew Configuration* - various values specific to the mod not reflected by the above



## LifePath

This mod completely replaces the default HBS lifepath system with a simplified solution. The HBS system allows for branching paths and is built to simulate character creation in an RPG, with extensive hooks to build in tags and other customizations for the resultant pilot. The lifepath system in this mod is significantly simpler, with each crew randomly selected one lifepath from a weighted list to determine their relevant background. 

All lifepaths are defined in the `lifepaths.json` file. There are two levels of hierarchy in the file. The top level is a a *lifepath class*, which is just a container for similar lifepaths. Some *lifepath classes* are **criminal**, **military**, **civilian**, or **pirate**. A lifepath class is simply defined with a dictionary of lifepath elements, and a weight.

```json
"lifepath_class" : 
{
    "weight" : 5.0,
    "lifepaths" : {
    }
}
```

The **weight** element is used to influence how frequently the lifepath_class should be picked. At mod initialization, the weight value for every `lifepath_class` is summed together. When a lifepath has to be chosen, a random number between 1 and the sum of the weights is selected. The lifepath classes are then iterated one by one, with their weights being added to each other. When the randomly selected number matches the summed weight, the lifepath class is chosen.

> Example: There are 4 lifepath classess defined, with weights given as criminal=4, military=3, civilian=10, pirate=3. The sum of all weights is 20. If the random number is 1-4, criminal will be chosen. If 5-7, military. If 8-17 civilian and if 18-20 pirate will be chosen.

Each class contains one or more **lifepaths**, which are weighted in a similar fashion to lifepath classes. Lifepath weights are summed across the class only, and selections occur only across the class. Thus the selection is two step - randomly determine a lifepath class, then randomly determine a lifepath.

```json
{
    "descriptionKey" : "LIFEPATH_CIVILIAN_FACTORY_WORKER",
    "requiredTags" : [  ],
    "randomTags" : [ ],
    "skillMod" : {
        "mechwarrior" : 0,
        "vehicle" : 0,
        "aerospace" : 0,
        "mechtech" : 0,
        "medtech" : 0
  	}
}
```

Each lifepath has a `requiredtags` and `randomTags` element. Every tag listed in `requiredTags` will be added to the newly created crew. For each tag in the `randomTags` element a random roll will be made. If the value is less than `HiringHall.Lifepath.RandomTagChance` in the `mod.json` value, the tag will be added to the crew. 

The `skillMod` element in a lifepath defines the modifiers that should be applied to specific crew types. For instance to increase the overall skill rating of an aerospace crew from a particular lifepath, set `skillMod.aerospace = 0.5`. Crew that select this lifepath will increase their overall rating by 0.5, if they are generated as aerospace crew.

Lifepaths must include a `descriptionKey` element in them. This key links the lifepath to the `mod_localized_text.json` file, and must correspond to a value in the `LifePathDescriptions` dictionary. As an example, for a value of `"descriptionKey" : "LIFEPATH_CIVILIAN_FACTORY_WORKER"` in the lifepath, the `mod_localized_text.json` entry must be:

```json
	"LifePathDescriptions": {

		"LIFEPATH_CIVILIAN_FACTORY_WORKER": {
			"title": "Factory Worker",
			"description": "Factories run by human labor aren't uncommon in the Inner Sphere, and are everywhere in the Periphery. These places always need strong hands that can do repetitive, dull work hour after hour every day of the week. This crew member eventually decided to embrace mercenary life to break the monotony and now seeks adventure among the stars. They are happy to join your mercenary company so long as there are new places to explore and people to kill. Just don't ask them to go on yet another patrol route, unless you want to get an earful."
		},
```

:information_source: I've chosen to simplify the lifepath system because I don't find there to be significant depth in the pilots concept. Generally the most important aspects of the pilots (skills and abilities) are configured by the player directly, while the background elements have no mechanical effect. Because of this the branching lifepath system doesn't make much sense to me, since ultimately players will care about the defined abilities, the skills, and maybe the tags if using a mod that provides benefits based upon tag names.

## Scarcity

The availability of crews are determined on a planet by planet basis, through planet tags. Each crew type (Aerospace, MechWarrior, etc) is given a default scarcity defined in `Scarcity.Defaults`. The `Scarcity.PlanetTagModifiers` dictionary allows you to associate a specific planet tag with modifiers to those scarcity defaults. The format for a modifier is: ` "planet_tag" : { "MechWarriors" : 0.0, "VehicleCrews" : 1.0, "MechTechs" : 2.0, "MedTechs" : 3.0, "Aerospace" : 4.0 }`. 

All the modifiers from all the tags on the planet are added together, the rounded up to the nearest integer to determine the maximum number of each crew that will be randomly generated. The lower bound of each crew type is half the upper bound, rounded to zero. 

> Example: The tag modifiers includes `planet_other_capital` with `MedTechs=1.3`, `planet_pop_small` with `Medtechs=0.8` and `planet_industry_recreation` with `MedTechs=0.5`. The sum of these is 2.6, which is rounded up to 3 for the upper bound. The lower bound is half the upper bound rounded down, or 3 / 2 = 1.5 for 1. This planet will generate between 1 and 3 MedTechs.



## Skill and Size Distribution

Support Crew skill and size are randomly determined, using a [Gaussian](https://en.wikipedia.org/wiki/Normal_distribution) distribution. These distributions are customizable through the `SkillDistribution` and `SizeDistribution` values in mod.json. Each distribution is configured with a *Sigma* and *Mu* value, representing the standard deviation of the distribution and the center-point of the distribution. *Mu* represents the most common value in the distribution around which all other values will cluster.

Each distribution also defines four breakpoints representing the progression from rating 1 to 5 for the values. Values below the first breakpoint will be treated as rating 1, values below the second breakpoint will be treated as rating 2, etc. These are defined in the `SkillDistribution.Breakpoints` and `SizeDistribution.Breakpoints` arrays.

> Example: The skill distribution is setup with a sigma of 1, a mu of 0, and breakpoints of [ -1, 1, 2, 3 ]. Most random values will center around point 0. If a value of -3.8 is pulled from the distribution, it will be treated as rating of 1. If value of 1.3 is pulled from the distribution, it will be treated as a rating of 3. 

### Crew Value

#### Combat Crew Value

The value of a combat crew is determined as the sum of all their skills. This value gets mapped to the 5 common categories (Rookie, Regular, Veteran, Elite, Legendary) of all crew types through the `Mechwarriors.SkillToExpertiseThresholds` and `VehicleCrews.SkillToExpertiseThresholds` settings. These are integer arrays with exactly five values, representing the progression given above. Each position determines the maximum value (i.e. sum of all skills) that applies to that index position. If the pilot has a greater value than the index position, the next index is evaluated.

> Example: A pilot has Gunnery 5, Guts 4, Piloting 6 and Tactics 5. Their value is 5 + 4 + 6 + 5 = 20. If SkillToExpertiseThresholds : [ 10, 18, 27, 35, 99 ], the pilot is considered a Veteran. Their value of 20 is greater than 10 so they are not a rookie; it's greater than 18 so they are not a Regular. Their value is less than or equal to 27, so they are Veteran.

#### AeroSpace Points

Aerospace points are applied to the statistic `HR_AerospaceSkill`. It's not used directly by any mechanic in the HBS game, but may be used by other mods. The allocation of points are determined by the configuration in `PointsBySkillAndSize.Aerospace`.

| Crew Size | Rookie | Regular | Veteran | Elite | Legendary |
| --------- | ------ | ------- | ------- | ----- | --------- |
| Tiny      | 1      | 2       | 3       | 5     | 8         |
| Small     | 2      | 4       | 6       | 10    | 16        |
| Medium    | 3      | 6       | 9       | 15    | 24        |
| Large     | 4      | 8       | 12      | 20    | 32        |
| Huge      | 5      | 10      | 15      | 25    | 40        |

#### MechTech Points

MechTech points are applied to the CompanyStat `MechTechSkill`. This value determines how quickly the Mech workqueue is resolved. The allocation of points are determined by the configuration in `PointsBySkillAndSize.MechTech`.

| Crew Size | Rookie | Regular | Veteran | Elite | Legendary |
| --------- | ------ | ------- | ------- | ----- | --------- |
| Tiny      | 1      | 2       | 3       | 5     | 8         |
| Small     | 2      | 4       | 6       | 10    | 16        |
| Medium    | 3      | 6       | 9       | 15    | 24        |
| Large     | 4      | 8       | 12      | 20    | 32        |
| Huge      | 5      | 10      | 15      | 25    | 40        |

#### MedTech Points

MedTech points are applied to the CompanyStat `MedTechSkill`. This value determines how quickly the injuries work queue is resolved. The allocation of points are determined by the configuration in `PointsBySkillAndSize.MedTech`.

| Crew Size | Rookie | Regular | Veteran | Elite | Legendary |
| --------- | ------ | ------- | ------- | ----- | --------- |
| Tiny      | 1      | 2       | 3       | 5     | 8         |
| Small     | 2      | 4       | 6       | 10    | 16        |
| Medium    | 3      | 6       | 9       | 15    | 24        |
| Large     | 4      | 8       | 12      | 20    | 32        |
| Huge      | 5      | 10      | 15      | 25    | 40        |



## Salary and Bonuses 

### Salary

A crew's base salary is driven by an exponential function `ab^x` where a = `SalaryMulti`, b = `SalaryExponent` and x = the value of the pilot, rounded down to the nearest integer value. A pilot's value is defined as the number of points they contribute (Aerospace, MedTech, MechTech) or the sum of all their skills (MechWarriors or Vehicle Crews). 

> Example: For a SalaryMulti of 30,000 and SalaryExponent of 1.1:
>
> A MechWarrior with gunnery 2, guts 3, piloting 2, and tactics 2 would have a base salary of (30,000) x (1.1 ^ (2 + 3 + 2 + 2)) = 70,738.43 = 70,738.
>
> A Vehicle crew with gunnery 7, guts 8, piloting 9, and tactics 7 would have a base salary of (30,000) x (1.1 ^ (7 + 8 + 9 + 7)) = 575,830.274 = 575,830.
>
> A MedTech crew with MechTech points 12 would have a base salary of (30,000) x (1.1 ^ 12) = 94,152.85 = 94,152.

Once the base salary is calculated, a variance is applied to randomize the amount. This is controlled by the `SalaryVariance` multiplier.  A random value between the base salary and the salary x `SalaryVariance` will be used as the final salary the mercenary asks for.

### Hiring Bonus
Each mercenary asks for a hiring bonus when their contract is renewed. This bonus is calculated using the same formula as the salary, but used `BonusVariance` multiplier instead. 

> Example: A mercenary with base salary 95,000 has `SalaryVariance` = 1.1 and `BonusVariance` = 1.5. 
>
> Their salary will be randomly chosen between 95,000 and 104,500 (i.e. 95,000 x 1.1)
>
> Their bonus will be randomly chosen between 95,000 and 142,500 (i.e. 95,000 x 1.5)


### Hazard Pay

Combat crews (MechWarriors and Vehicle Pilots) often get paid a bonus simply for being exposed to enemy fire. At the end of a contract, the **Hazard Pay** for all combat crews is summed together and displayed as an objective result. The amount of hazard pay each merc requires is a function of their salary times *MechWarriors.HazardPayRatio* or *VehicleCrews.HazardPayRatio*. This ratio is a float percentage between 0 and 1, thus that 0.33 is 33%. The hazard pay will be rounded up to the nearest *HazardPayUnits* in c-bills.

> Example: A mechwarrior has a salary of 11,500 and MechWarriors.HazardPayRatio is set to 0.15. Their hazard pay would be 11,500 x 0.15 = 1,725. Because HazardPayUnits is set to 500, this is rounded up to the nearest 500s value, or 2,000 c-bills.



## Crew Configuration

#### Contract Length

When you sign a contract with a mercenary, it's only good for a certain number of days. When a crew is created, a random contract length is determined for them and they will always use this contract length no matter how many times they are hired. The length is determined by calculating a random integer between `MinContractDaysMulti` and `MaxContractDaysMulti`, and multiplying that random integer by `BaseDaysInContract`. 

> Example: A crew has MinContractDaysMulti = 3, MaxContractDaysMulti = 12, and BaseDaysInContract 15. A random value between 3 and 12 will be chosen,  in this case 8. That's then multiplied by 15 for 8 x 15 = 120 days.

### Hiring Limits

There are several limitations in the mod that can restrict a player's ability to hire a crew. They are detailed in the sections below.

#### MRB Restriction

More experienced MechWarriors will not work for you until your company is well known, represented by your MRBC rating. Each crew has a **Value**, which is calculated differently for combat and support crews. This value is then compared against the `ValueThresholdForMRBLevel` for that crew type. This value must be an array of integers from least to greatest. Each array position is the maximum *crew value* that applies to the MRBC level of the array position. If the player's MRBC level is greater than the specified value, the next position is checked and so on.

> Example: The player has a MRBC level of 2, an Aerospace crew of value 12, and ValueThresholdForMRBLevel : [ 8, 16, 24, 32, 99 ]. If the crew value was 8 or less, they can be hired at MRBC level 1. If the value is 16 or less (as it is here), the crew can be hired at MRBC level 2. 

See the [Value](#Crew_Value) section below details on how each crew's value is calculated.


#### Crew Type Limits

You may not want players to be able to acquire too many crews of the same time. If `MaxOfType` is set to -1, any number of crews can be hired up to the current berth limits. If `MaxOfType` is set to a value greater 0, the player won't be able to hire more than `MaxOfType` crews.

> Example: If MedTechCrews.MaxOfType = 2, then the player cannot hire more than 2 MedTech crews.



# Attitude

Mercenaries are known for their individuality, and don't always choose to stay with one employer. Your actions can make them hate you and want off your boat at the first port, or make them think your company is their best chance to make it big. A crew's attitude is tracked between -100 and 100, with several breakpoints between the extremes. There are five attitude bands: best, good, neutral, poor, worst. The labels for each of these are defined in `mod_localization_text.json` and are configured via the `Attitude.Threshold` values in `mod.json`.

A mercenary at or above `Attitude.ThresholdBest` is likely to ignore head-hunting offers, be willing to forgo a re-hiring bonus, or offer similar perks. At `Attitude.ThresholdWorst` or below they are likely to take the first chance to leave your company they find. You can expect them to willing be head-hunted or just slip out in the middle of the night.

Attitudes change over time, with changes occurring at the end of every mission and the end of the quarter. 

If a mercenary is rehired, they experience a small attitude boost thanks to avoiding the drudgery of a job search. The value of this change is exposed through the `Attitude.RehireBonusMod` modifier, which defaults to 2.

**Monthly Changes**

Each quarter all crews experience an attitude change equal to the current _Expenses Level_ of the company (i.e. Spartan, Restricted, Normal, etc). The change is controlled through the following variables in `mod.json`:

| Setting | Default Value |
| -- | -- |
| Attitude.Monthly.EconSpartanMod | -6 |
| Attitude.Monthly.EconRestrictiveMod | -3 |
| Attitude.Monthly.EconomyNormalMod | 0 |
| Attitude.Monthly.EconomyGenerousMod | 2 |
| Attitude.Monthly.EconomyExtravagantMod | 4 |

**Decay**

Mercs aren't known for having long memories, and positive attitude decays over time. At the end of each month, the mercs attitude will be decreased by **Attitude.Monthly.Decay** times their current attitude. For example, a merc with attitude of 50 and **Attitude.Monthly.Decay** of 0.05 (the default) would lose 3 attitude points. Their opinion naturally shifts back to neutral unless you constantly refresh it.

**Per Mission**

 After each mission all crew in the company receives a bonus or penalty based upon the contract status and whether or not any pilots were lost. Mercs accept that death is a risk of the occupation, but don't enjoy reminders of that fact. Additionally combat crews (MechWarriors and Vehicle Crews) gain a boost in attitude if they were deployed to a mission, while those that were left out are slightly unhappy. While some are happy to ride the bench, most want to do their job.

The values of these changes are controlled through the following `mod.json` settings:

| Setting | Default Value |
| -- | -- |
| Attitude.PerMission.ContractSuccessMod | 1 |
| Attitude.PerMission.ContractFailedGoodFaithMod | -2 |
| Attitude.PerMission.ContractFailedMod | -10 |
| Attitude.PerMission.PilotKilledMod | -10 |
| Attitude.PerMission.DeployedOnMissionMod | 1 |
| Attitude.PerMission.BenchedOnMissionMod | -2 |


## Faction Loyalty and Hatred

While most mercenaries are willing to work for whomever will pay them, a few have deep-seated loyalties or hatred for specific factions. One crew may simply hate Davion due to their upbringing as a Taurian loyalist, while another believes completely in the Steiner way of life. Mercenaries with **Faction Loyalty** gain attitude when you complete contracts where the employer is their preferred faction. They also gain loyalty at the end of each month if you are allied with their preferred faction. They will lose attitude if you target their preferred faction.

Conversely mercs with **Faction Hatred** lose attitude when you complete contracts where their hated faction is the employer, but gain attitude if they are the target. In addition they take a large hit each month you are allied with their hated faction.

The chance for a crew to have a loyalty or hatred is controlled through the `Attitude.FavoredFactionChance` and `Attitude.HatedFactionChance` settings in ``mod.json`. These are float percentage values, with a range of 0 to 1. Thus 0.33 is 33%. Note that a crew can have both a loyalty and a hatred, as the rolls are separate.

The amount of attitude gain or loss is controlled through several settings in `mod.json`:

| Setting | Default Value |
| -- | -- |
| Attitude.Monthly.FavoredEmployerAlliedMod | 6 |
| Attitude.Monthly.HatedEmployerAlliedMod | -30 |
| Attitude.PerMission.FavoredFactionIsEmployerMod | 1 |
| Attitude.PerMission.FavoredFactionIsTargetMod | -3 |
| Attitude.PerMission.HatedEmployerIsEmployerMod | -3 |
| Attitude.PerMission.HatedEmployerIsTargetMod | 3 |

# Head Hunting

Skilled mercenaries are in high demand, and most of them care more about the size of their paycheck than concepts like loyalty or honor. When the player is orbiting an inhabited planet there's a chance that someone will attempt to head-hunt your crews. When a crew is headhunted, the player will be presented with a dialog with four choices:

* You persuade them to remain with the company, effectively no change. _Requires attitude at or above the best mark_
* You counter-offer, and pay them an additional (higher) hiring bonus to keep them on payroll. 
* You let them take the offer, and their new employer pays you a buyout bonus equal to their hiring bonus.
* They desert in the night. You get nothing and the pilot is gone. _Requires attitude at or below the worst mark_.

Checks to head-hunt a crew occurs periodically. When the player first arrives at a planet, a random day between `HeadHuting.FailedCooldownIntervalMin` and `HeadHunting.FailedCooldownIntervalMax`is added to the current day. On that day, assuming no other events are firing a head-hunting check will be made. If the check fails another random day between `HeadHuting.FailedCooldownIntervalMin` and `HeadHunting.FailedCooldownIntervalMax` will be added to the date. If the check is successful (i.e. an event is spawn) a random day between `HeadHuting.SuccessCooldownIntervalMin` and `HeadHunting.SuccessCooldownIntervalMax` will be added to the date. Note that a low period between failures may be frustrating when the player has many experienced crews, as they are more likely to be head-hunted than lower-skill crews (see below). 

A head-hunting check is a random roll between 0 and 1, modified by `HeadHunting.EconMods`. This modifier is indexed by the current company expense setting (i.e. Spartan, Restrictive, Regular, etc). The list of all crew is randomized and the modified check result is compared against the the _Headhunting Chance_. The expertise level of a crew (from 1-5, i.e. Green through Elite) indexes the `HeadHunting.ChanceBySkill` configuration which determines the _HeadHunting Chance_ for each crew. If any crew's _Headhunting Chance_ is greater than or equal to the modified result, they are the target of the headhunting event. Otherwise, every other crew is checked until all crew have been checked for head-hunting.

> Example: There are three crews with with skill 1, 2, 3. The company's expenses are set to regular, which gives a 0 modifier for economy. A random roll of 0.73 is made, making the modified check 0.73. The list of crew is randomly sorted into 2,3,1. The first crew has a headhunting chance of 0.1, which is less than 0.73 and thus they are skipped. The next crew has a chance of 0.15, and thus is also skipped. The final crew has a chance of 0.05 and is skipped. This counts as a failed headhunting attempt, and a random value of 5 (between 3 and 7) is determined. The system will check for headhunting again in 5 days.

WIP: If a crew is head-hunted but retained by the company, they cannot be head-hunted in the near future. A random value between `HeadHunting.CrewCooldownIntervalMin` and `HeadHunting.CrewCooldownIntervalMax` will be added to the current days elapsed. They will automatically fail any head-hunting checks until this date has passed.

During a head-hunting event, the counter-offer value is controlled by the `HeadHunting.CounterOfferVariance` value. A random value between the crew's current HiringBonus and __HiringBonus x CounterOfferVariance__ will be set as the counter-offer value. This value is only used in the head-hunting event and does not change the crew's hiring bonus value.

WIP: Head-hunting events will not spawn on planets with one or more tags specified in `HeadHunting.PlanetBlacklist`. These are treated as automatic failures for tracking purposes.

This feature can be completely disabled by setting `HeadHunting.Enabled=false` in mod.json.

# Miscellaneous

Many vanilla events target MechWarriors, and this mod's 'crews' are all considered MechWarriors by the game code. For this reason the mod does a prefix=false patch on `SimGameEventTracker.IsEventValid` and prohibits any crews from being targeted by vanilla events. This means vehicle crews won't get into fights, MechTech crews won't get low spirits, and Aerospace pilots can't get injured.

## New Pilot Tags

This mod adds several pilot tags to the MDDB. They function like normal pilot tags in all respects, and can be used with other mods that tie pilot tags to mechanical advantages.   

(!) Pilot tags are not removed when the mod is disabled or uninstalled. They will persist throughout the save. It's not safe to remove them without cleaning up every pilot that has them, so they remain in the system for all current and future saves to use. To completely remove them, you must wipe out your Mods/.modtek directory and start a new save.

| Vanilla Behavior Tags | Vanilla Origin Tags | HR Behavior Tags | HR Origin Tags |
| -- | -- | -- | -- |
| pilot\_athletic<br>pilot\_assassin<br>pilot\_brave<br>pilot\_bookish<br>pilot\_cautious<br>pilot\_criminal<br>pilot\_command<br>pilot\_comstar<br>pilot\_dependable<br>pilot\_disgraced<br>pilot\_dishonest<br>pilot\_drunk<br>pilot\_gladiator<br>pilot\_honest<br>pilot\_jinxed<br>pilot\_klutz<br>pilot\_lostech<br>pilot\_lucky<br>pilot\_mechwarrior<br>pilot\_military<br>pilot\_naive<br>pilot\_officer<br>pilot\_rebellious<br>pilot\_reckless<br>pilot\_spacer<br>pilot\_tech<br>pilot\_unstable<br>pilot\_wealthy | pilot\_aurigan<br>pilot\_davion<br>pilot\_kurita<br>pilot\_liao<br>pilot\_innersphere<br>pilot\_magistracy<br>pilot\_marik<br>pilot\_noble<br>pilot\_periphery<br>pilot\_steiner<br>pilot\_taurian<br>pilot\_tortuga |

### Internal Tags

Some tags are not displayed to the player, and are used....
		
## Dev Notes




## TODO

* Abilities for combat crews need to be auto-generated
	- Be aware of https://github.com/bloodydoves/BattleTech-Advanced/tree/development/Abilifier which adds multiple abilities
* Add dropship maintenance crew; reduces Argo maintenance cost?
* Hiring cost varies by planet tag *not* faction owner
  * Faction reputation influences the cost (if they are faction affiliated and you are hated, more expensive)
* skill distribution mu and sigma modified by planet tags
	- partial; done for MW/VC
* size distribution mu and sigma modified by planet tags
* Mercenary loyalty
  * After AAR report, will complain if you work against their faction
* Allow mod-makers to set faction hatred/favors on pre-generated pilots
* Add pilot favor/hatred to pre-generated pilots
* I'm not updating pilots value, etc as their skills change; need to ensure that getzs addressed at least at contract renegotiation
* Context tooltip from events needs to account for crews
* Optional kill bonus where each unit killed gives a bonus to the pilot
* Doco note: salary only changes on renegotiation
* Update salary for mechwarriors/vcrew prior to event fire
* MedTechs, MechTechs, Aerospace should improve as you use them
* Check if player character is paid a salary
* What happens if you fire then re-hire the same mechwarrior? Does the old state persist?
* Random list of merc company names as the hiring company
* Account for inflated hiring bonuses when recalculating on rehire
* Check events for crew callsigns; I think they only have names?
* Renewal should update terms, not 'same terms'
* Fix/eliminate smear campaign event
* <Partial> Implement planet blacklist for head hunting
* Implement CrewCooldownInterval for head-hunting
* Hiring bonus should not change as you keep crew? Or should it?
* Add profit sharing as an event?
* Add attitude decay based upon pilot tags?  
* Chance to ignore rehire bonus/salary increase at very high attitude?
* Chance to leave at Worst/Poor thresholds
* Chance to offer personal contracts at Good/Best?
* Combat bonuses/penalties for attitude?
	- Auto-eject at worst from time to time (I'm not getting paid enough for this)
	- Attack bonus if they are really happy?
	- Chance of a better health outcome?

* EVENT: Add slackers event for support; reduces their value for N days has a change in attitude
* EVENT: Add motivated event for support; increases their value for N days if you fund it 
* EVENT: Add feud event; two crews are fighting with each other.

* BUG: Max injuries should be hidden on hiring screen
* BUG: Contract desc / pilot isn't getting reset between events
* BUG: Can click hire even if at max berths
* BUG: Colors not updating when you first look at screen
* BUG: Colors not updating for Mechs in list
* BUG: Compound faction names aren't whitespace separated (in events?)
* BUG: Hiring scrollbars don't update
* BUG: MRB hiring limits should be based on unit skill, NOT points

* IDEA: Age based interactions... for combat, younger pilots have less value but heal quicker? Older pilots take longer to heal?

* REQUEST (BD): Allow tags that let him set salaryMulti, salaryExponent on a pilot. Allow him to set a tag that defines how many re-hires happen before this is lost. After that, use normal values
	
* (REJECTED) Expense costs should be exponential? 
* (REJECTED) Expense changes should have personnel costs (luxury cost, maybe tied to 	
* (REJECTED) Company reputation feels like a step too far; too much to manage but it's damn compelling. How would I make your aggregate reputation make sense?

* IDEA: Disorderly Withdrawal - add strafing using aerospace assets on a combat drop? 