# Human Resources
This mod for the [HBS BattleTech](http://battletechgame.com/) game breathes life into the crews onboard the Argo. Aerospace pilots, MechTech crews, MedTech crews, and Vehicle crews can be acquired from the Hiring Hall. These crew require a hiring bonus in addition to their monthly salary, offer contracts of varying lengths, and will have their own loyalties to manage. These crews are mercenaries and will leave at the end of their contract, or when someone makes them a better offer. You'll always have Wang, Sumire and the rest - but otherwise you'll need to keep an eye on the people that form the backbone of your mercenary company.

**Features**

 * Hirable Aerospace, MechTech, MedTech, and Vehicle crews in the Hiring Hall
 * Fully customizable salary and hiring bonus calculations based upon an exponential formula
 * Scarcity of all hirable crews based upon planetary tags
 * Scarcity of all hirable crews driven by a gaussian distribution
 * Crews have a customizable contract length
 * (Optional) Crews can be poached by other employers when the conditions are right

This mod requires the [IRBTModUtils](https://github.com/battletechmodders/irbtmodutils/) mod. Download the most recent version and make sure it's enabled before loading this mod.

### Combat Crews and Support Crews

This mod distinguishes between *Combat Crews* (MechWarriors and Vehicle Crews) and *Support Crews* (Aerospace Wings, MechTech Crews, and MedTech Crews). Some configuration elements will be common across these two types.

*Combat Crews* are handled normally, though their distribution and prices are handled by the mod. Overall there are few changes to these units. Vehicle Crews are treated as MechWarriors, but assigned the `pilot_vehicle_crew` and `pilot_nomech_crew` tags for CustomUnits compatibility.

*Support Crews* are given a skill and size rating. Both ratings go from 1-5, and are labeled in `mod_localization.json` by their rating:

| Name | 1 | 2 | 3 | 4 | 5 |
| -- | -- | -- | -- | -- | -- |
| Skill | Rookie | Regular | Veteran | Elite | Legendary |
| Size | Tiny | Small | Medium | Large | Huge |

# Hiring 

All crews are available to be hired from the HiringHall. Mechwarriors are untouched, but all other crews are represented with different icons:

* TODO: Add icon images

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

### Contract Length

When you sign a contract with a mercenary, it's only good for a certain number of days. When a crew is created, a random contract length is determined for them and they will always use this contract length no matter how many times they are hired. The length is determined by calculating a random integer between `MinContractDaysMulti` and `MaxContractDaysMulti`, and multiplying that random integer by `BaseDaysInContract`. 

> Example: A crew has MinContractDaysMulti = 3, MaxContractDaysMulti = 12, and BaseDaysInContract 15. A random value between 3 and 12 will be chosen,  in this case 8. That's then multiplied by 15 for 8 x 15 = 120 days.

### Crew Limits

You may not want players to be able to acquire too many crews of the same time. If `MaxOfType` is set to -1, any number of crews can be hired up to the current berth limits. If `MaxOfType` is set to a value greater 0, the player won't be able to hire more than `MaxOfType` crews.

> Example: If MedTechCrews.MaxOfType = 2, then the player cannot hire more than 2 MedTech crews.

### Skill and Size Distribution

Support Crew skill and size are randomly determined, using a [Gaussian](https://en.wikipedia.org/wiki/Normal_distribution) distribution. These distributions are customizable through the `SkillDistribution` and `SizeDistribution` values in mod.json. Each distribution is configured with a *Sigma* and *Mu* value, representing the standard deviation of the distribution and the center-point of the distribution. *Mu* represents the most common value in the distribution around which all other values will cluster.

Each distribution also defines four breakpoints representing the progression from rating 1 to 5 for the values. Values below the first breakpoint will be treated as rating 1, values below the second breakpoint will be treated as rating 2, etc. These are defined in the `SkillDistribution.Breakpoints` and `SizeDistribution.Breakpoints` arrays.

> Example: The skill distribution is setup with a sigma of 1, a mu of 0, and breakpoints of [ -1, 1, 2, 3 ]. Most random values will center around point 0. If a value of -3.8 is pulled from the distribution, it will be treated as rating of 1. If value of 1.3 is pulled from the distribution, it will be treated as a rating of 3. 

### Scarcity

The availability of crews are determined on a planet by planet basis, through planet tags. Each crew type (Aerospace, MechWarrior, etc) is given a default scarcity defined in `Scarcity.Defaults`. The `Scarcity.PlanetTagModifiers` dictionary allows you to associate a specific planet tag with modifiers to those scarcity defaults. The format for a modifier is: ` "planet_tag" : { "MechWarriors" : 0.0, "VehicleCrews" : 1.0, "MechTechs" : 2.0, "MedTechs" : 3.0, "Aerospace" : 4.0 }`. 

All the modifiers from all the tags on the planet are added together, the rounded up to the nearest integer to determine the maximum number of each crew that will be randomly generated. The lower bound of each crew type is half the upper bound, rounded to zero. 

> Example: The tag modifiers includes `planet_other_capital` with `MedTechs=1.3`, `planet_pop_small` with `Medtechs=0.8` and `planet_industry_recreation` with `MedTechs=0.5`. The sum of these is 2.6, which is rounded up to 3 for the upper bound. The lower bound is half the upper bound rounded down, or 3 / 2 = 1.5 for 1. This planet will generate between 1 and 3 MedTechs.


# Head Hunting

Skilled mercenaries are in high demand, and most of them care more about the size of their paycheck than concepts like loyalty or honor. When the player is orbiting an inhabited planet there's a chance that someone will attempt to head-hunt your crews. When a crew is headhunted, the player will be presented with a dialog with four choices:

* You persuade them to remain with the company, effectively no change. _Requires attitude at or above the best mark_
* You counter-offer, and pay them an additional (higher) hiring bonus to keep them on payroll. 
* You let them take the offer, and their new employer pays you a buyout bonus equal to their hiring bonus.
* They desert in the night. You get nothing and the pilot is gone. _Requires attitude at or below the worst mark_.

Checks to head-hunt a crew occurs periodically. When the player first arrives at a planet, a random day between `HeadHuting.FailedCooldownIntervalMin` and `HeadHunting.FailedCooldownIntervalMax`is added to the current day. On that day, assuming no other events are firing a head-hunting check will be made. If the check fails another random day between `HeadHuting.FailedCooldownIntervalMin` and `HeadHunting.FailedCooldownIntervalMax` will be added to the date. If the check is successful (i.e. an event is spawn) a random day between `HeadHuting.SuccessCooldownIntervalMin` and `HeadHunting.SuccessCooldownIntervalMax` will be added to the date. Note that a low period between failures may be frustrating when the player has many experienced crews, as they are more likely to be head-hunted than lower-skill crews (see below). 

A head-hunting check is a random roll between 0 and 1, modified by `HeadHunting.EconMods`. This modifier is indexed by the current company expense setting (i.e. Spartan, Restrictive, Regular, etc). The list of all crew is randomized and the modified check result is compared against the the _Headhunting Chance_. The expertise level of a crew (from 1-5, i.e. Green through Elite) indexes the `HeadHunting.ChanceBySkill` configuration which determines the _HeadHunting Chance_ for each crew. If any crew's _Headhunting Chance_ is greater than or equal to the modified result, they are the target of the headhunting event. Otherwise, every other crew is checked until all crew have been checked for head-hunting.

> Example: There are three crews with with skill 1, 2, 3. The company's expenses are set to regular, which gives a 0 modifier for economy. A random roll of 0.73 is made, making the modified check 0.73. The list of crew is randomly sorted into 2,3,1. The first crew has a headhunting chance of 0.1, which is less than 0.73 and thus they are skipped. The next crew has a chance of 0.15, and thus is also skipped. The final crew has a chance of 0.05 and is skipped. This counts as a failed headhunting attempt, and a random value of 5 (between 3 and 7) is determined. The system will check for headhunting again in 5 days.

This feature can be completely disabled by setting `HeadHunting.Enabled=false` in mod.json. 


# Crew Value

### Combat Crew Value

The value of a combat crew is determined as the sum of all their skills, plus any bonuses for those skills. 

### AeroSpace Points

Aerospace points are applied to the statistic `HR_AerospaceSkill`. It's not used directly by any mechanic in the HBS game, but may be used by other mods. The allocation of points are determined by the configuration in `PointsBySkillAndSize.Aerospace`.

| Crew Size | Rookie | Regular | Veteran | Elite | Legendary |
| --------- | ------ | ------- | ------- | ----- | --------- |
| Tiny      | 1      | 2       | 3       | 5     | 8         |
| Small     | 2      | 4       | 6       | 10    | 16        |
| Medium    | 3      | 6       | 9       | 15    | 24        |
| Large     | 4      | 8       | 12      | 20    | 32        |
| Huge      | 5      | 10      | 15      | 25    | 40        |

### MechTech Points

MechTech points are applied to the CompanyStat `MechTechSkill`. This value determines how quickly the Mech workqueue is resolved. The allocation of points are determined by the configuration in `PointsBySkillAndSize.MechTech`.

| Crew Size | Rookie | Regular | Veteran | Elite | Legendary |
| --------- | ------ | ------- | ------- | ----- | --------- |
| Tiny      | 1      | 2       | 3       | 5     | 8         |
| Small     | 2      | 4       | 6       | 10    | 16        |
| Medium    | 3      | 6       | 9       | 15    | 24        |
| Large     | 4      | 8       | 12      | 20    | 32        |
| Huge      | 5      | 10      | 15      | 25    | 40        |

### MedTech Points

MedTech points are applied to the CompanyStat `MedTechSkill`. This value determines how quickly the injuries work queue is resolved. The allocation of points are determined by the configuration in `PointsBySkillAndSize.MedTech`.

| Crew Size | Rookie | Regular | Veteran | Elite | Legendary |
| --------- | ------ | ------- | ------- | ----- | --------- |
| Tiny      | 1      | 2       | 3       | 5     | 8         |
| Small     | 2      | 4       | 6       | 10    | 16        |
| Medium    | 3      | 6       | 9       | 15    | 24        |
| Large     | 4      | 8       | 12      | 20    | 32        |
| Huge      | 5      | 10      | 15      | 25    | 40        |

### TODO

* Descriptions for MedTechs/MercTechs
  * Background
  * Attributes
  * Faction loyalty
* Hiring cost varies by planet tag *not* faction owner
  * Faction reputation influences the cost (if they are faction affiliated and you are hated, more expensive)
* skill distribution mu and sigma modified by planet tags
* size distribution mu and sigma modified by planet tags
* MechWarrior skill distribution to reflect our distribution
* Mercenary loyalty
  * Low morale decays their loyalty slightly, high morale increases it
  * Fighting against their faction has a slight reduction
  * Fighting against a faction they hate has a improvement. Decays over time?
  * Pilot losses impact their loyalty
  * Staying on the bench hurts as well; not deploying reduces loyalty and deploying increases it
  * Company reputation; go with options that make you hated and it becomes harder to hire
  * After AAR report, will complain if you work against their faction
  * Poaching - sometimes they will want to break a contract (for more pay) and you have to decide what to do
* Max injuries should be hidden on hiring screen
* BUG: Colors not updating when you first look at screen
* BUG: Colors not updating for Mechs in list
* Contract desc / pilot isn't getting reset between events
* MRBC rating should apply somehow?
* Allow mod-makers to set faction hatred/favors on pre-generated pilots
* Company reputation feels like a step too far; too much to manage but it's damn compelling. How would I make your aggregate reputation make sense?
* Add MSRB restrictions for each crew member
* Add pilot favor/hatred to pre-generated pilots
* Compound faction names aren't whitespace separated
* I'm not updating pilots value, etc as their skills change; need to ensure that gets addressed at least at contract renegotiation
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
* Commander should have neither hatred/loyalty/attitude
* Expense costs should be exponential? 
* Expense changes should have personnel costs (luxury cost, maybe tied to tags)?
* Renewal should update terms, not 'same terms'
* Fix/eliminate smear campaign event
* Implement planet blacklist for head hunting
* Add re-hire bonus to attitude
* Implement CrewCooldownInterval for head-hunting