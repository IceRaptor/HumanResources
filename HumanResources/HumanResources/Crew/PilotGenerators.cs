using BattleTech;
using BattleTech.Data;
using BattleTech.Portraits;
using Harmony;
using HBS.Collections;
using HumanResources.Helper;
using HumanResources.Lifepath;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HumanResources.Crew
{

    public static class CrewGenerator
    {
        // === SUPPORT CREWS ===
        public static PilotDef GenerateMechTechCrew(StarSystem starSystem)
        {
            int callsignIdx = Mod.Random.Next(0, Mod.CrewNames.MechTech.Count - 1);
            string newCallsign = Mod.CrewNames.MechTech[callsignIdx];

            PilotDef def = GenerateSkillessCrew(starSystem, newCallsign, out int crewSize, out int crewSkill);

            // Before returning, initialize the cache value
            (FactionValue favored, FactionValue hated) = GenerateCrewFactions(starSystem);
            CrewDetails details = new CrewDetails(def, CrewType.MechTechCrew, favored, hated, crewSize, crewSkill);
            ModState.UpdateOrCreateCrewDetails(def, details);

            return def;
        }

        public static PilotDef GenerateMedTechCrew(StarSystem starSystem)
        {
            int callsignIdx = Mod.Random.Next(0, Mod.CrewNames.MedTech.Count - 1);
            string newCallsign = Mod.CrewNames.MedTech[callsignIdx];

            PilotDef def = GenerateSkillessCrew(starSystem, newCallsign, out int crewSize, out int crewSkill);

            // Before returning, initialize the cache value
            (FactionValue favored, FactionValue hated) = GenerateCrewFactions(starSystem);
            CrewDetails details = new CrewDetails(def, CrewType.MedTechCrew, favored, hated, crewSize, crewSkill);
            ModState.UpdateOrCreateCrewDetails(def, details);

            return def;
        }

        public static PilotDef GenerateAerospaceCrew(StarSystem starSystem)
        {
            int callsignIdx = Mod.Random.Next(0, Mod.CrewNames.Aerospace.Count - 1);
            string newCallsign = Mod.CrewNames.Aerospace[callsignIdx];

            PilotDef def = GenerateSkillessCrew(starSystem, newCallsign, out int crewSize, out int crewSkill);

            // Before returning, initialize the cache value
            (FactionValue favored, FactionValue hated) = GenerateCrewFactions(starSystem);
            CrewDetails details = new CrewDetails(def, CrewType.AerospaceWing, favored, hated, crewSize, crewSkill);
            ModState.UpdateOrCreateCrewDetails(def, details);

            return def;
        }

        // === COMBAT CREWS ===
        public static PilotDef GenerateMechWarrior(StarSystem starSystem)
        {
            PilotDef crew = GenerateSkilledCrew(starSystem, true);
            Mod.Log.Debug?.Write($"CREATED MECHWARRIOR CREW");

            (FactionValue favored, FactionValue hated) = GenerateCrewFactions(starSystem);
            CrewDetails details = new CrewDetails(crew, CrewType.MechWarrior, favored, hated);
            ModState.UpdateOrCreateCrewDetails(crew, details);
            return crew;
        }

        public static PilotDef UpgradeRonin(StarSystem starSystem, PilotDef baseRoninDef)
        {
            CrewDetails details = ReadRoninTags(baseRoninDef);
            ModState.UpdateOrCreateCrewDetails(baseRoninDef, details);
            return baseRoninDef;
        }

        public static CrewDetails GenerateDetailsForVanillaMechwarrior(PilotDef basePilotDef, bool isFounder=false)
        {
            (FactionValue favored, FactionValue hated) = GenerateCrewFactions(null);
            CrewDetails details = new CrewDetails(basePilotDef, CrewType.MechWarrior, favored, hated);
            ModState.UpdateOrCreateCrewDetails(basePilotDef, details);
            Mod.Log.Info?.Write($" -- pilotDef associated with GUID: {details.GUID}");

            return details;
        }

        public static PilotDef GenerateVehicleCrew(StarSystem starSystem)
        {
            PilotDef crew = GenerateSkilledCrew(starSystem, false);
            Mod.Log.Debug?.Write($"CREATED VEHICLE CREW");

            (FactionValue favored, FactionValue hated) = GenerateCrewFactions(starSystem);
            CrewDetails details = new CrewDetails(crew, CrewType.VehicleCrew, favored, hated);
            ModState.UpdateOrCreateCrewDetails(crew, details);

            return crew;
        }

        private static (FactionValue, FactionValue) GenerateCrewFactions(StarSystem starSystem)
        {
            // Check for favored / hated faction
            FactionValue favoredFaction = null;
            if (Mod.Config.Attitude.FavoredFactionCandidates.Count > 0)
            {
                double favoredRoll = Mod.Random.NextDouble();
                if (favoredRoll < Mod.Config.Attitude.FavoredFactionChance)
                {
                    int idx = Mod.Random.Next(Mod.Config.Attitude.FavoredFactionCandidates.Count - 1);
                    favoredFaction = Mod.Config.Attitude.FavoredFactionCandidates[idx];
                    Mod.Log.Info?.Write($"Roll {favoredRoll} < {Mod.Config.Attitude.FavoredFactionChance}, adding {favoredFaction} as favored faction");
                }
            }

            FactionValue hatedFaction = null;
            double hatedRoll = Mod.Random.NextDouble();
            if (Mod.Config.Attitude.HatedFactionCandidates.Count > 0)
            {
                if (hatedRoll < Mod.Config.Attitude.HatedFactionChance)
                {
                    List<FactionValue> candidates = new List<FactionValue>(Mod.Config.Attitude.HatedFactionCandidates);
                    if (favoredFaction != null)
                        candidates.Remove(favoredFaction);

                    int idx = Mod.Random.Next(candidates.Count - 1);
                    hatedFaction = candidates[idx];
                    Mod.Log.Info?.Write($"Roll {hatedRoll} < {Mod.Config.Attitude.HatedFactionChance}, adding {hatedFaction} as hated faction");
                }
            }

            return (favoredFaction, hatedFaction);
        }

        private static CrewDetails ReadRoninTags(PilotDef roninDef)
        {
            //CrewDetails crewDetails = new CrewDetails(); BAD, SERIALIZATION ONLY!
            Mod.Log.Debug?.Write($"Building CrewDetails for Ronin: {roninDef.Description.Name}_{roninDef.GUID}");

            CrewType type = CrewType.MechWarrior;
            FactionValue favored = null, hated = null;
            int skillIdx = 0, sizeIdx = 0;
            int salaryMulti ;
            float salaryExp, salaryVariance, bonusVariance;
            bool isFounder = false;
            foreach (string tag in roninDef.PilotTags)
            {
                // Check types
                if (tag.Equals(ModTags.Tag_CrewType_Aerospace, StringComparison.InvariantCultureIgnoreCase))
                {
                    Mod.Log.Debug?.Write($" -- found type == Aerospace");
                    type = CrewType.AerospaceWing;
                }
                if (tag.Equals(ModTags.Tag_CrewType_MechTech, StringComparison.InvariantCultureIgnoreCase))
                {
                    Mod.Log.Debug?.Write($" -- found type == MechTech");
                    type = CrewType.MechTechCrew;
                }
                if (tag.Equals(ModTags.Tag_CrewType_MedTech, StringComparison.InvariantCultureIgnoreCase))
                {
                    Mod.Log.Debug?.Write($" -- found type == MedTech");
                    type = CrewType.MedTechCrew;
                }
                if (tag.Equals(ModTags.Tag_CU_Vehicle_Crew, StringComparison.InvariantCultureIgnoreCase))
                {
                    Mod.Log.Debug?.Write($" -- found type == Vehicle");
                    type = CrewType.VehicleCrew;
                }

                // Check factions
                if (tag.StartsWith(ModTags.Tag_Prefix_Ronin_Faction_Favored, StringComparison.InvariantCultureIgnoreCase))
                {
                    string factionDefId = tag.Substring(ModTags.Tag_Prefix_Ronin_Faction_Favored.Length);
                    Mod.Log.Debug?.Write($" -- found favored faction defId: {factionDefId}");

                    foreach (FactionValue factionVal in Mod.Config.Attitude.FavoredFactionCandidates)
                    {
                        if (factionVal.FactionDefID.Equals(factionDefId, StringComparison.InvariantCultureIgnoreCase))
                            favored = factionVal;
                    }

                    if (favored == null)
                        Mod.Log.Warn?.Write($"Could not map favored factionDefId: {factionDefId} to a configured faction! Skipping!");
                }

                if (tag.StartsWith(ModTags.Tag_Prefix_Ronin_Faction_Hated, StringComparison.InvariantCultureIgnoreCase))
                {
                    string factionDefId = tag.Substring(ModTags.Tag_Prefix_Ronin_Faction_Hated.Length);
                    Mod.Log.Debug?.Write($" -- found hated faction defId: {factionDefId}");

                    foreach (FactionValue factionVal in Mod.Config.Attitude.HatedFactionCandidates)
                    {
                        if (factionVal.FactionDefID.Equals(factionDefId, StringComparison.InvariantCultureIgnoreCase))
                            hated = factionVal;
                    }

                    if (hated == null)
                        Mod.Log.Warn?.Write($"Could not map hated factionDefId: {factionDefId} to a configured faction! Skipping!");
                }

                // Check salary & bonus values
                if (tag.StartsWith(ModTags.Tag_Prefix_Ronin_Salary_Multi, StringComparison.InvariantCultureIgnoreCase))
                {
                    string tagS = tag.Substring(ModTags.Tag_Prefix_Ronin_Salary_Multi.Length);
                    try
                    {
                        salaryMulti =  Int32.Parse(tagS, CultureInfo.InvariantCulture);
                        Mod.Log.Debug?.Write($" -- found salaryMulti: {salaryMulti}");
                    }
                    catch (Exception e)
                    {
                        Mod.Log.Warn?.Write(e, $"Failed to read salaryMulti: {tagS} as an integer value, skipping!");
                    }
                }

                if (tag.StartsWith(ModTags.Tag_Prefix_Ronin_Salary_Exp, StringComparison.InvariantCultureIgnoreCase))
                {
                    string tagS = tag.Substring(ModTags.Tag_Prefix_Ronin_Salary_Exp.Length);
                    try
                    {
                        salaryExp = float.Parse(tagS, CultureInfo.InvariantCulture);
                        Mod.Log.Debug?.Write($" -- found salaryExp: {salaryExp}");
                    }
                    catch (Exception e)
                    {
                        Mod.Log.Warn?.Write(e, $"Failed to read salaryExp: {tagS} as an integer value, skipping!");
                    }
                }

                if (tag.StartsWith(ModTags.Tag_Prefix_Ronin_Salary_Variance, StringComparison.InvariantCultureIgnoreCase))
                {
                    string tagS = tag.Substring(ModTags.Tag_Prefix_Ronin_Salary_Variance.Length);
                    try
                    {
                        salaryVariance = float.Parse(tagS, CultureInfo.InvariantCulture);
                        Mod.Log.Debug?.Write($" -- found salaryVariance: {salaryVariance}");
                    }
                    catch (Exception e)
                    {
                        Mod.Log.Warn?.Write(e, $"Failed to read salaryVariance: {tagS} as an integer value, skipping!");
                    }
                }

                if (tag.StartsWith(ModTags.Tag_Prefix_Ronin_Bonus_Variance, StringComparison.InvariantCultureIgnoreCase))
                {
                    string tagS = tag.Substring(ModTags.Tag_Prefix_Ronin_Bonus_Variance.Length);
                    try
                    {
                        bonusVariance = float.Parse(tagS, CultureInfo.InvariantCulture);
                        Mod.Log.Debug?.Write($" -- found bonusVariance: {bonusVariance}");
                    }
                    catch (Exception e)
                    {
                        Mod.Log.Warn?.Write(e, $"Failed to read bonusVariance: {tagS} as an integer value, skipping!");
                    }
                }


                // Check founder
                if (tag.Equals(ModTags.Tag_Founder, StringComparison.InvariantCultureIgnoreCase))
                {
                    Mod.Log.Debug?.Write($" -- found founder tag");
                    isFounder = true;
                }

            }

            CrewDetails details = new CrewDetails(roninDef, type, favored, hated, sizeIdx, skillIdx, isFounder);

            return details;
        }

        private static PilotDef GenerateSkillessCrew(StarSystem starSystem, string callsign, out int crewSize, out int crewSkill)
        {
            // Determine crew size and skill
            crewSize = GaussianHelper.RandomCrewSize(0, 0);
            crewSkill = GaussianHelper.RandomCrewSkill(0, 0);
            Mod.Log.Debug?.Write($" - random crewSize: {crewSize}  crewSkill: {crewSkill}");

            PilotDef pilotDef = new PilotDef(new HumanDescriptionDef(), 1, 1, 1, 1, 1, 1, lethalInjury: false, 1, "", new List<string>(), AIPersonality.Undefined, 0, 0, 0);

            return GenerateCrew(starSystem, callsign, pilotDef);
        }

        private static PilotDef GenerateSkilledCrew(StarSystem starSystem, bool isMechWarrior)
        {
            string callsign = RandomUnusedCallsign();

            CrewTagModifier planetModifiers = starSystem.GetPlanetSkillDistModifier();
            float planetMod = isMechWarrior ? planetModifiers.MechWarriors : planetModifiers.VehicleCrews;
            int skillIdx = GaussianHelper.RandomCrewSkill(planetMod, 0);
            Mod.Log.Debug?.Write($" - generated skillIdx: {skillIdx} using planetModifier: {planetMod}");

            // Divide skill points by 4 to generate a median for each skill
            float totalSkillPoints = isMechWarrior ?
                Mod.Config.HiringHall.MechWarriors.SkillToExpertiseThresholds[skillIdx] :
                Mod.Config.HiringHall.VehicleCrews.SkillToExpertiseThresholds[skillIdx];
            // Normalize to the basic 10/10/10/10 max if we pull the highest index
            if (totalSkillPoints > 40) totalSkillPoints = 40;
            float medianSkillPoints = totalSkillPoints / 4;
            // Reduce the median by -2 to allow for random add below
            float adjustedMedian = (float)Math.Max(0, medianSkillPoints - 2);
            Mod.Log.Debug?.Write($" - skillPoints =>  total: {totalSkillPoints} median: {medianSkillPoints}  adjusted: {adjustedMedian}");

            // Each skill can vary +2.0 from adjusted, then rounded to an integer.
            int piloting = (int)Math.Round(adjustedMedian + (Mod.Random.NextDouble() * 2));
            int gunnery = (int)Math.Round(adjustedMedian + (Mod.Random.NextDouble() * 2));
            int guts = (int)Math.Round(adjustedMedian + (Mod.Random.NextDouble() * 2));
            int tactics = (int)Math.Round(adjustedMedian + (Mod.Random.NextDouble() * 2));
            Mod.Log.Debug?.Write($" - skills =>  piloting: {piloting}  gunnery: {gunnery}  guts: {guts}  tactics: {tactics}");

            // TODO: randomize health +/- 1?
            int health = 3;

            PilotDef pilotDef = new PilotDef(new HumanDescriptionDef(), gunnery, piloting, guts, tactics, injuries: 0, health,
                lethalInjury: false, morale: 1, voice: "", new List<string>(), AIPersonality.Undefined, 0, 0, 0);
            PilotDef generatedDef = GenerateCrew(starSystem, callsign, pilotDef);

            // Add the necessary tags here *in addition* to in CrewDetails to support Abilifier (it needs the tag before
            //   the call to SetPilotAbilities)
            if (!isMechWarrior)
            {
                generatedDef.PilotTags.Add(ModTags.Tag_CU_NoMech_Crew);
                generatedDef.PilotTags.Add(ModTags.Tag_CU_Vehicle_Crew);
            }
            else
            {
                // TODO: Are there any mechwarrior tags?
            }


            // Abilities must be set AFTER the pilotDef is created, to allow Abilifier a chance to hook into them
            // TODO: Randomize ability selections
            GenerateAbilityDefs(generatedDef, new Dictionary<string, int>()
                {
                    { ModConsts.Skill_Gunnery, gunnery },
                    { ModConsts.Skill_Guts, guts},
                    { ModConsts.Skill_Piloting, piloting },
                    { ModConsts.Skill_Tactics, tactics },
                },
                isMechWarrior ? Mod.Config.HiringHall.MechWarriors : Mod.Config.HiringHall.VehicleCrews
                );

            generatedDef.ForceRefreshAbilityDefs();

            return generatedDef;
        }

        private static PilotDef GenerateCrew(StarSystem starSystem, string callsign, PilotDef pilotDef)
        {
            Mod.Log.Debug?.Write($"Generating support crew with callsign: {callsign}");

            // Generate the lifepath we'll use
            LifePath lifePath = LifePathHelper.GetRandomLifePath();

            int initialAge = ModState.SimGameState.Constants.Pilot.MinimumPilotAge +
                ModState.SimGameState.NetworkRandom.Int(1, ModState.SimGameState.Constants.Pilot.StartingAgeRange + 1);
            int currentAge = Mod.Random.Next(initialAge, 70);
            Mod.Log.Debug?.Write($" - currentAge: {currentAge}");

            Gender newGender = RandomGender();
            Gender voiceGender = newGender;
            if (voiceGender == Gender.NonBinary)
            {
                voiceGender = ((!(ModState.SimGameState.NetworkRandom.Float() < 0.5f)) ? Gender.Female : Gender.Male);
            }
            string voice = RandomUnusedVoice(voiceGender);
            string newFirstName = ModState.CrewCreateState.NameGenerator.GetFirstName(newGender);
            string newLastName = ModState.CrewCreateState.NameGenerator.GetLastName();
            Mod.Log.Debug?.Write($" - gender: {newGender}  voiceGender: {voiceGender}  firstName: {newFirstName}  lastName: {newLastName}");
            pilotDef.SetVoice(voice);

            StringBuilder lifepathDescParagraphs = new StringBuilder();
            string backgroundTitle = new Text(lifePath.Description.Title).ToString();
            string backgroundDesc = new Text(lifePath.Description.Description).ToString();
            // DETAILS string is EXTREMELY picky, see HumanDescriptionDef.GetLocalizedDetails. There format must be followed *exactly*
            string formattedBackground = $"{Environment.NewLine}<b>{backgroundTitle}:</b>  {backgroundDesc}";
            Mod.Log.Debug?.Write($" - Background: {formattedBackground}");
            lifepathDescParagraphs.Append(formattedBackground);

            // Add tags from the lifepath
            TagSet tagSet = new TagSet();
            tagSet.AddRange(lifePath.RequiredTags);

            foreach (string tag in lifePath.RandomTags)
            {
                double tagRoll = Mod.Random.NextDouble();
                if (tagRoll <= Mod.Config.HiringHall.LifePath.RandomTagChance)
                {
                    tagSet.Add(tag);
                }
            }
            Mod.Log.Debug?.Write($" - Tags: {String.Join(", ", tagSet)}");

            // Add tags to the background
            foreach (string tagId in tagSet)
            {
                Tag_MDD tagIfExists = MetadataDatabase.Instance.GetTagIfExists(tagId);
                if (tagIfExists != null)
                {
                    TagDataStruct tagStruct = new TagDataStruct(tagId, tagIfExists.PlayerVisible, tagIfExists.Important, tagIfExists.Name, tagIfExists.FriendlyName, tagIfExists.Description);
                    string formattedTag = $"<b><color=#ff8c00>{tagStruct.FriendlyName}:</b>  <color=#ffffff>{tagStruct.DescriptionTag}";
                    lifepathDescParagraphs.Append(formattedTag);
                }
            }

            string id = GenerateID();
            HumanDescriptionDef descriptionDef = new HumanDescriptionDef(id, callsign, newFirstName, newLastName, callsign, newGender,
                FactionEnumeration.GetNoFactionValue(), currentAge, lifepathDescParagraphs.ToString(), null);

            StatCollection statCollection = pilotDef.GetStats();
            int spentXPPilot = GetSpentXPPilot(statCollection);

            List<string> alreadyAssignedPortraits = new List<string>();
            if (ModState.SimGameState.Commander != null && ModState.SimGameState.Commander.pilotDef.PortraitSettings != null)
            {
                alreadyAssignedPortraits.Add(ModState.SimGameState.Commander.pilotDef.PortraitSettings.Description.Id);
            }
            foreach (Pilot activePilot in ModState.SimGameState.PilotRoster)
            {
                if (activePilot.pilotDef.PortraitSettings != null)
                {
                    alreadyAssignedPortraits.Add(activePilot.pilotDef.PortraitSettings.Description.Id);
                }
            }

            PilotDef pilotDef2 = new PilotDef(descriptionDef, pilotDef.BaseGunnery, pilotDef.BasePiloting, pilotDef.BaseGuts, pilotDef.BaseTactics, 0,
                ModState.SimGameState.CombatConstants.PilotingConstants.DefaultMaxInjuries, lethalInjury: false, 0, voice, pilotDef.abilityDefNames,
                AIPersonality.Undefined, 0, tagSet, spentXPPilot, 0)
            {
                DataManager = ModState.SimGameState.DataManager,
                PortraitSettings = GetPortraitForGenderAndAge(voiceGender, currentAge, alreadyAssignedPortraits)
            };
            ModState.SimGameState.pilotGenCallsignDiscardPile.Add(pilotDef2.Description.Callsign);

            return pilotDef2;
        }

        private static Traverse SetPilotAbilitiesT;

        private static void GenerateAbilityDefs(PilotDef pilotDef, Dictionary<string, int> skillLevels, CrewOpts crewOpts)
        {
            //Mod.Log.Info?.Write($" Pilot skills => gunnery: {skillLevels[ModConsts.Skill_Gunnery]}  guts: {skillLevels[ModConsts.Skill_Guts]}  " +
            //    $"piloting: {skillLevels[ModConsts.Skill_Piloting]}  tactics: {skillLevels[ModConsts.Skill_Tactics]}");
            //Mod.Log.Info?.Write($" Pilot base skills => gunnery: {pilotDef.BaseGunnery}  guts: {pilotDef.BaseGuts}  " +
            //    $"piloting: {pilotDef.BasePiloting}  tactics: {pilotDef.BaseTactics}");
            //Mod.Log.Info?.Write($"Initial abilityDef names are: {string.Join(",", pilotDef.abilityDefNames)}");

            //Mod.Log.Info?.Write($"-- ITERATING ABILITY TREE");
            //foreach (KeyValuePair<string, Dictionary<int, List<AbilityDef>>> kvp in ModState.SimGameState.AbilityTree)
            //{
            //    Mod.Log.Info?.Write($"-- {kvp.Key}");
            //    foreach (KeyValuePair<int, List<AbilityDef>> kvp2 in kvp.Value)
            //    {
            //        Mod.Log.Info?.Write($" ---- {string.Join(",", kvp2.Value.Select(x => x.Id))}");
            //    }
            //}

            // Sort by skill skill
            string primarySkill = SelectHighestSkill(skillLevels);

            Dictionary<string, int> secondarySkills = new Dictionary<string, int>(skillLevels);
            secondarySkills.Remove(primarySkill);
            string secondarySkill = SelectHighestSkill(secondarySkills);

            try
            {
                Mod.Log.Info?.Write($"Generating pilot with skills => primary: {primarySkill}@{skillLevels[primarySkill]} " +
                    $"secondary: {secondarySkill}@{skillLevels[secondarySkill]}");

                // Filter for primary defs - sort by weight
                if (SetPilotAbilitiesT == null)
                {
                    SetPilotAbilitiesT = Traverse
                        .Create(ModState.CrewCreateState.HBSPilotGenerator)
                        .Method("SetPilotAbilities", new Type[] { typeof(PilotDef), typeof(string), typeof(int) });
                }

                // invokeType - PilotDef pilot, string type, int value
                Mod.Log.Info?.Write($" -- setting primary skill: {primarySkill}");
                for (int i = 1; i < skillLevels[primarySkill] + 1; i++)
                {
                    SetPilotAbilitiesT.GetValue(new object[] { pilotDef, primarySkill, i });
                }

                Mod.Log.Info?.Write($" -- setting secondary skill: {secondarySkill}");
                for (int i = 1; i < skillLevels[secondarySkill] + 1; i++)
                {
                    SetPilotAbilitiesT.GetValue(new object[] { pilotDef, secondarySkill, i });
                }

            }
            catch (Exception e)
            {
                Mod.Log.Warn?.Write(e, " Failed to set skill defs!");
            }

            Mod.Log.Info?.Write($"Final ability def names are: {string.Join(",", pilotDef.abilityDefNames)}");
            Mod.Log.Info?.Write($"Final abilityDefs are:");
            foreach (AbilityDef abilityDef in pilotDef.AbilityDefs)
            {
                Mod.Log.Info?.Write($" -- {abilityDef.Description.Id}");
            }
        }

        private static string SelectHighestSkill(Dictionary<string, int> skillLevels)
        {
            var sortedLevels = skillLevels.OrderByDescending(x => x.Value);

            int level = sortedLevels.FirstOrDefault().Value;
            List<KeyValuePair<string, int>> skills = sortedLevels.Where(x => x.Value == level).ToList();
            if (skills.Count == 1)
                return skills[0].Key;

            if (skills.Count == 0)
                return null;

            int randIdx = Mod.Random.Next(skills.Count);
            return skills[randIdx].Key;
        }

        private static string RandomUnusedCallsign()
        {
            List<string> availableCallsigns = ModState.CrewCreateState.NameGenerator.GetAllCallsigns();

            List<string> previouslyUsedCallsigns = new List<string>();
            for (int idx = availableCallsigns.Count - 1; idx >= 0; idx--)
            {
                if (ModState.SimGameState.pilotGenCallsignDiscardPile.Contains(availableCallsigns[idx]))
                {
                    previouslyUsedCallsigns.Add(availableCallsigns[idx]);
                    availableCallsigns.RemoveAt(idx);
                }
            }

            // If we've already used enough callsigns that we're at risk of running out, re-inject the discarded
            //   ones and allow duplicates
            if ((float)previouslyUsedCallsigns.Count >= (float)availableCallsigns.Count * ModState.SimGameState.Constants.Story.DiscardPileToActiveRatio)
            {
                Mod.Log.Info?.Write("Too many callsigns have been discarded; refreshing list to allow duplicates.");
                availableCallsigns.AddRange(previouslyUsedCallsigns);
                ModState.SimGameState.pilotGenCallsignDiscardPile.Clear();
            }

            // TODO: why are we shuffling each and every time? This is pretty inefficient...
            availableCallsigns.Shuffle();
            string callSign = availableCallsigns[0];
            Mod.Log.Debug?.Write($"Returning callsign: {callSign}");

            return callSign;
        }

        private static Gender RandomGender()
        {
            int weightedResult = SimGameState.GetWeightedResult(ModState.CrewCreateState.GenderWeights, (float)Mod.Random.NextDouble());
            Gender gender = ModState.CrewCreateState.Genders[weightedResult];
            return gender;
        }

        // Direct copy from BattleTech.PilotGenerator.GenerateVoiceForGender
        private static string RandomUnusedVoice(Gender gender)
        {
            List<string> allOptionsForGender = ModState.CrewCreateState.Voices.GetAllOptionsForGender(gender);

            List<string> alreadyUsedVoices = new List<string>();
            for (int num = allOptionsForGender.Count - 1; num >= 0; num--)
            {
                if (ModState.SimGameState.pilotGenVoiceDiscardPile.Contains(allOptionsForGender[num]))
                {
                    alreadyUsedVoices.Add(allOptionsForGender[num]);
                    allOptionsForGender.RemoveAt(num);
                }
            }

            // If we've in danger of running out of voices, reinject the voices. This will cause duplicates.
            if ((float)alreadyUsedVoices.Count >= (float)allOptionsForGender.Count * ModState.SimGameState.Constants.Story.DiscardPileToActiveRatio)
            {
                allOptionsForGender.AddRange(alreadyUsedVoices);
                ModState.SimGameState.pilotGenVoiceDiscardPile.Clear();
            }

            // TODO: why are we shuffling each and every time? This is pretty inefficient...
            allOptionsForGender.Shuffle();
            return allOptionsForGender[0];
        }

        // Direct copy of BattleTech.PilotGenerator.GetPortraitForGenderAndAge
        private static PortraitSettings GetPortraitForGenderAndAge(Gender gender, int age, List<string> blackListedIDs)
        {
            List<PortraitSettings> list = new List<PortraitSettings>();
            VersionManifestEntry[] array = ModState.SimGameState.DataManager.ResourceLocator.AllEntriesOfResource(BattleTechResourceType.PortraitSettings);
            foreach (VersionManifestEntry versionManifestEntry in array)
            {
                if (!blackListedIDs.Contains(versionManifestEntry.Id))
                {
                    list.Add(ModState.SimGameState.DataManager.PortraitSettings.Get(versionManifestEntry.Id));
                }
            }
            List<PortraitSettings> list2 = list.FindAll((PortraitSettings x) => (gender == Gender.NonBinary || x.Gender == gender) && x.MatchesAge(age));
            if (list2 != null && list2.Count > 0)
            {
                list = list2;
            }
            else
            {
                Debug.LogWarning($"Cannot find any portrait settings to match Gender {gender} and Age {age}");
            }
            List<PortraitSettings> list3 = new List<PortraitSettings>();
            for (int num = list.Count - 1; num >= 0; num--)
            {
                if (ModState.SimGameState.pilotGenPortraitDiscardPile.Contains(list[num].Description.Id))
                {
                    list3.Add(list[num]);
                    list.RemoveAt(num);
                }
            }
            if ((float)list3.Count >= (float)list.Count * ModState.SimGameState.Constants.Story.DiscardPileToActiveRatio)
            {
                list.AddRange(list3);
                ModState.SimGameState.pilotGenPortraitDiscardPile.Clear();
            }
            PortraitSettings portraitSettings = list[Mod.Random.Next(0, list.Count)];
            ModState.SimGameState.pilotGenPortraitDiscardPile.Add(portraitSettings.Description.Id);
            return portraitSettings;
        }

        // Direct copy of BattleTech.PilotGenerator.GetSpentXPPilot
        private static int GetSpentXPPilot(StatCollection stats)
        {
            return GetSpentXPPilot(stats.GetValue<int>("Gunnery")) + GetSpentXPPilot(stats.GetValue<int>("Piloting")) +
                GetSpentXPPilot(stats.GetValue<int>("Guts")) + GetSpentXPPilot(stats.GetValue<int>("Tactics"));
        }

        // Direct copy of BattleTech.PilotGenerator.GetSpentXPPilot
        private static int GetSpentXPPilot(int baseLevel)
        {
            int num = 0;
            for (int num2 = baseLevel; num2 > 1; num2--)
            {
                num += ModState.SimGameState.GetLevelCost(num2);
            }
            return num;
        }

        // Direct copy of BattleTech.PilotGenerator.GenerateID
        private static string GenerateID()
        {
            return string.Format("{0}{1}", "PilotGen_", ModState.SimGameState.GenerateSimGameUID());
        }

    }
}
