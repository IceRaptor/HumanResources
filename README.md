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

# Hiring 

All crews are available to be hired from the HiringHall. Mechwarriors are untouched, but all other crews are represented with different icons:

* TODO: Add icon images

A crew's base salary is driven by an exponential function `ab^x` where a = `SalaryMulti`, b = `SalaryExponent` and x = the value of the pilot, rounded down to the nearest integer value. A pilot's value is defined as the number of points they contribute (Aerospace, MedTech, MechTech) or the sum of all their skills (MechWarriors or Vehicle Crews). 

> Example: For a SalaryMulti of 30,000 and SalaryExponent of 1.1:
>
> A MechWarrior with gunnery 2, guts 3, piloting 2, and tactics 2 would have a base salary of (30,000) x (1.1 ^ (2 + 3 + 2 + 2)) = 70,738.43 = 70,738.
>
> A Vehicle crew with gunnery 7, guts 8, piloting 9, and tactics 7 would have a base salary of (30,000) x (1.1 ^ (7 + 8 + 9 + 7)) = 575,830.274 = 575,830.
>
> A MedTech crew with MechTech points 12 would have a base salary of (30,000) x (1.1 ^ 12) = 94,152.85 = 94,152.

Once the base salary is calculated, a variance is applied to randomize the amount. This is controlled by the `SalaryVariance` multiplier.  A random value between the base salary and the salary x `SalaryVariance` will be used as the final salary the mercenary asks for.

Each mercenary asks for a hiring bonus when their contract is renewed. This bonus is calculated using the same formula as the salary, but used `BonusVariance` multiplier instead. 

> Example: A mercenary with base salary 95,000 has `SalaryVariance` = 1.1 and `BonusVariance` = 1.5. 
>
> Their salary will be randomly chosen between 95,000 and 104,500 (i.e. 95,000 x 1.1)
>
> Their bonus will be randomly chosen between 95,000 and 142,500 (i.e. 95,000 x 1.5)



### AeroSpace Points

Aerospace points are applied to the statistic `HR_AerospaceSkill`. It's not used directly by any mechanic in the HBS game, but may be used by other mods.

| Crew Size | Rookie | Regular | Veteran | Elite | Legendary |
| --------- | ------ | ------- | ------- | ----- | --------- |
| Tiny      | 1      | 2       | 3       | 5     | 8         |
| Small     | 2      | 4       | 6       | 10    | 16        |
| Medium    | 3      | 6       | 9       | 15    | 24        |
| Large     | 4      | 8       | 12      | 20    | 32        |
| Huge      | 5      | 10      | 15      | 25    | 40        |

### MechTech Points

MechTech points are applied to the CompanyStat `MechTechSkill`. This value determines how quickly the Mech workqueue is resolved. 

| Crew Size | Rookie | Regular | Veteran | Elite | Legendary |
| --------- | ------ | ------- | ------- | ----- | --------- |
| Tiny      | 1      | 2       | 3       | 5     | 8         |
| Small     | 2      | 4       | 6       | 10    | 16        |
| Medium    | 3      | 6       | 9       | 15    | 24        |
| Large     | 4      | 8       | 12      | 20    | 32        |
| Huge      | 5      | 10      | 15      | 25    | 40        |

### MedTech Points

MedTech points are applied to the CompanyStat `MedTechSkill`. This value determines how quickly the injuries work queue is resolved.

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
* Add Aerospace Wings
* Hiring cost varies by planet tag *not* faction owner
  * Faction reputation influences the cost (if they are faction affiliated and you are hated, more expensive)
* Firing events takes forever according to t-bone; profile when firing the event
* skill distribution mu and sigma modified by planet tags
* size distribution mu and sigma modified by planet tags
* MechWarrior skill distribution to reflect our distribution
* Mercenary loyalty
  * Low morale decays their loyalty to your crew
  * Fighting against their faction has a slight reduction
  * Pilot losses impact their loyalty
  * Morale increases their loyalty slightly
  * Company reputation; go with options that make you hated and it becomes harder to hire
  * After AAR report, will complain if you work against their faction
  * Poaching - sometimes they will want to break a contract (for more pay) and you have to decide what to do
* Max injuries should be hidden on hiring screen
* BUG: Colors not updating when you first look at screen
* BUG: Colors not updating for Mechs in list
* Contract expiration not decrementing company funds on renewal
* Contract expiration descriptions are weird
* Contract desc / pilot isn't getting reset between events

