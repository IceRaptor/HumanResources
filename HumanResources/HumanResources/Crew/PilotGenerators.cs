﻿using BattleTech;
using BattleTech.Data;
using BattleTech.Portraits;
using HBS.Collections;
using HumanResources.Helper;
using HumanResources.Lifepath;
using IRBTModUtils;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HumanResources.Crew
{

    public static class CrewGenerator
    {



        public static PilotDef GenerateMechTechCrew(StarSystem starSystem)
        {
            int callsignIdx = Mod.Random.Next(0, Mod.CrewNames.MechTech.Count - 1);
            string newCallsign = Mod.CrewNames.MechTech[callsignIdx];

            PilotDef def = GenerateSkillessCrew(starSystem, newCallsign, out int crewSize, out int crewSkill);

            // Before returning, initialize the cache value
            CrewDetails details = new CrewDetails(def, CrewType.MechTechCrew, crewSize, crewSkill);
            ModState.UpdateOrCreateCrewDetails(def, details);

            return def;
        }

        public static PilotDef GenerateMedTechCrew(StarSystem starSystem)
        {
            int callsignIdx = Mod.Random.Next(0, Mod.CrewNames.MedTech.Count - 1);
            string newCallsign = Mod.CrewNames.MedTech[callsignIdx];

            PilotDef def = GenerateSkillessCrew(starSystem, newCallsign, out int crewSize, out int crewSkill);

            // Before returning, initialize the cache value
            CrewDetails details = new CrewDetails(def, CrewType.MedTechCrew, crewSize, crewSkill);
            ModState.UpdateOrCreateCrewDetails(def, details);

            return def;
        }

        public static PilotDef GenerateAerospaceCrew(StarSystem starSystem)
        {
            int callsignIdx = Mod.Random.Next(0, Mod.CrewNames.Aerospace.Count - 1);
            string newCallsign = Mod.CrewNames.Aerospace[callsignIdx];

            PilotDef def = GenerateSkillessCrew(starSystem, newCallsign, out int crewSize, out int crewSkill);

            // Before returning, initialize the cache value
            CrewDetails details = new CrewDetails(def, CrewType.AerospaceWing, crewSize, crewSkill);
            ModState.UpdateOrCreateCrewDetails(def, details);

            return def;
        }
        public static PilotDef GenerateSkillessCrew(StarSystem starSystem, string callsign, out int crewSize, out int crewSkill)
        {
            // Determine crew size and skill
            crewSize = GaussianHelper.RandomCrewSize(0, 0);
            crewSkill = GaussianHelper.RandomCrewSkill(0, 0);
            Mod.Log.Debug?.Write($" - random crewSize: {crewSize}  crewSkill: {crewSkill}");

            PilotDef pilotDef = new PilotDef(new HumanDescriptionDef(), 1, 1, 1, 1, 1, 1, lethalInjury: false, 1, "", new List<string>(), AIPersonality.Undefined, 0, 0, 0);

            return GenerateCrew(starSystem, callsign, pilotDef);
        }

        public static PilotDef GenerateSkilledCrew(StarSystem starSystem, bool isMechWarrior)
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

            // TODO: Randomize ability selections

            PilotDef pilotDef = new PilotDef(new HumanDescriptionDef(), gunnery, piloting, guts, tactics, injuries: 0, health, 
                lethalInjury: false, morale: 1, voice: "", new List<string>(), AIPersonality.Undefined, 0, 0, 0);

            return GenerateCrew(starSystem, callsign, pilotDef);
        }

        public static PilotDef GenerateCrew(StarSystem starSystem, string callsign, PilotDef pilotDef)
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
            pilotDef2.ForceRefreshAbilityDefs();
            ModState.SimGameState.pilotGenCallsignDiscardPile.Add(pilotDef2.Description.Callsign);

            return pilotDef2;
        }


        // Piloting = crew size
        // Gunnery = crew skill
        // Morale =  crew loyalty    
        // Adapted from BattleTech.PilotGenerator.GenerateRandomPilot
        //public static PilotDef GenerateVehicleCrew(int systemDifficulty)
        //{
        //    int initialAge = ModState.SimGameState.Constants.Pilot.MinimumPilotAge + ModState.SimGameState.NetworkRandom.Int(1, ModState.SimGameState.Constants.Pilot.StartingAgeRange + 1);

        //    Gender newGender = RandomGender();
        //    string newFirstName = ModState.CrewCreateState.NameGenerator.GetFirstName(newGender);
        //    string newLastName = ModState.CrewCreateState.NameGenerator.GetLastName();
        //    string newCallsign = RandomUnusedCallsign();
        //    Mod.Log.Debug?.Write($"Generating vehicleCrew with callsign: {newCallsign}");

        //    int currentAge;
        //    PilotDef pilotDef;
        //    TagSet tagSet;
        //    StringBuilder lifepathDescParagraphs;
        //    GenAndWalkLifePath(systemDifficulty, initialAge, out currentAge, out pilotDef, out tagSet, out lifepathDescParagraphs);

        //    string id = GenerateID();
        //    Gender voiceGender = newGender;
        //    if (voiceGender == Gender.NonBinary)
        //    {
        //        voiceGender = ((!(ModState.SimGameState.NetworkRandom.Float() < 0.5f)) ? Gender.Female : Gender.Male);
        //    }
        //    string voice = RandomUnusedVoice(voiceGender);
        //    HumanDescriptionDef description = new HumanDescriptionDef(id, newCallsign, newFirstName, newLastName, newCallsign, newGender, FactionEnumeration.GetNoFactionValue(), currentAge, lifepathDescParagraphs.ToString(), null);

        //    StatCollection statCollection = pilotDef.GetStats();
        //    int spentXPPilot = GetSpentXPPilot(statCollection);

        //    List<string> alreadyAssignedPortraits = new List<string>();
        //    if (ModState.SimGameState.Commander != null && ModState.SimGameState.Commander.pilotDef.PortraitSettings != null)
        //    {
        //        alreadyAssignedPortraits.Add(ModState.SimGameState.Commander.pilotDef.PortraitSettings.Description.Id);
        //    }
        //    foreach (Pilot activePilot in ModState.SimGameState.PilotRoster)
        //    {
        //        if (activePilot.pilotDef.PortraitSettings != null)
        //        {
        //            alreadyAssignedPortraits.Add(activePilot.pilotDef.PortraitSettings.Description.Id);
        //        }
        //    }

        //    PilotDef pilotDef2 = new PilotDef(description, pilotDef.BaseGunnery, pilotDef.BasePiloting, pilotDef.BaseGuts, pilotDef.BaseTactics, 0,
        //        ModState.SimGameState.CombatConstants.PilotingConstants.DefaultMaxInjuries, lethalInjury: false, 0, voice, pilotDef.abilityDefNames,
        //        AIPersonality.Undefined, 0, tagSet, spentXPPilot, 0)
        //    {
        //        DataManager = ModState.SimGameState.DataManager,
        //        PortraitSettings = GetPortraitForGenderAndAge(voiceGender, currentAge, alreadyAssignedPortraits)
        //    };
        //    pilotDef2.ForceRefreshAbilityDefs();
        //    ModState.SimGameState.pilotGenCallsignDiscardPile.Add(pilotDef2.Description.Callsign);
        //    return pilotDef2;
        //}

        // A fairly complex method adapted from HBS code. In short it attempts to walk a node tree and find conditions
        //   that would end the creation of the character.
        private static void GenAndWalkLifePath(int systemDifficulty, int initialAge, out int currentAge, out PilotDef pilotDef, out TagSet tagSet, out StringBuilder lifepathDescParagraphs)
        {

            currentAge = initialAge;
            pilotDef = new PilotDef(new HumanDescriptionDef(), 1, 1, 1, 1, 1, 1, lethalInjury: false, 1, "", new List<string>(), AIPersonality.Undefined, 0, 0, 0);
            tagSet = new TagSet();

            List<LifepathNodeDef> walkedNodes = new List<LifepathNodeDef>();
            List<EndingPair> endings = new List<EndingPair>();
            List<SimGameEventResultSet> nodeEventResults = new List<SimGameEventResultSet>();

            LifepathNodeDef lifepathNodeDef = GetStartingNode(systemDifficulty);
            while (lifepathNodeDef != null)
            {
                walkedNodes.Add(lifepathNodeDef);
                currentAge += lifepathNodeDef.Duration;
                SimGameEventResultSet resultSet = ModState.SimGameState.GetResultSet(lifepathNodeDef.ResultSets);
                SimGameEventResult[] results = resultSet.Results;
                foreach (SimGameEventResult simGameEventResult in results)
                {
                    if (simGameEventResult.AddedTags != null)
                    {
                        tagSet.AddRange(simGameEventResult.AddedTags);
                    }
                    if (simGameEventResult.RemovedTags != null)
                    {
                        tagSet.RemoveRange(simGameEventResult.RemovedTags);
                    }
                    if (simGameEventResult.Stats == null)
                    {
                        continue;
                    }
                    for (int j = 0; j < simGameEventResult.Stats.Length; j++)
                    {
                        SimGameStat simGameStat = simGameEventResult.Stats[j];
                        float f = simGameStat.ToSingle();
                        SkillType skillType = SkillStringToType(simGameStat.name);
                        if (skillType != 0)
                        {
                            int num3 = Mathf.RoundToInt(f);
                            int baseSkill = pilotDef.GetBaseSkill(skillType);
                            pilotDef.AddBaseSkill(skillType, num3);
                            for (int k = baseSkill + 1; k <= baseSkill + num3; k++)
                            {
                                SetPilotAbilities(pilotDef, simGameStat.name, k);
                            }
                        }
                    }
                }

                nodeEventResults.Add(resultSet);
                List<LifepathNodeEnding> endingNodes = new List<LifepathNodeEnding>();
                for (int l = 0; l < lifepathNodeDef.Endings.Length; l++)
                {
                    RequirementDef requirements = lifepathNodeDef.Endings[l].Requirements;
                    if (requirements == null || SimGameState.MeetsRequirements(requirements.RequirementTags, requirements.ExclusionTags, requirements.RequirementComparisons, tagSet, pilotDef.GetStats()))
                    {
                        endingNodes.Add(lifepathNodeDef.Endings[l]);
                    }
                }

                LifepathNodeDef nextLifePathNode = null;
                if (endingNodes.Count > 0)
                {
                    List<int> list6 = new List<int>();
                    for (int m = 0; m < endingNodes.Count; m++)
                    {
                        list6.Add(endingNodes[m].Weight);
                    }
                    int weightedResult = SimGameState.GetWeightedResult(list6, ModState.SimGameState.NetworkRandom.Float());
                    LifepathNodeEnding lifepathNodeEnding = endingNodes[weightedResult];
                    EndingPair item = default(EndingPair);
                    item.ending = lifepathNodeEnding;
                    TagSet nextNodeTags = lifepathNodeEnding.NextNodeTags;
                    float num4 = (float)currentAge * ModState.SimGameState.Constants.Pilot.AgeEndingModifier;
                    bool flag = false;
                    if ((float)ModState.SimGameState.NetworkRandom.Int() < num4 && !lifepathNodeDef.ForceNode)
                    {
                        flag = true;
                    }
                    if (!lifepathNodeEnding.EndNode && !flag)
                    {
                        List<LifepathNodeDef> list7 = new List<LifepathNodeDef>();
                        for (int n = 0; n < ModState.CrewCreateState.LifePaths.Count; n++)
                        {
                            if (nextNodeTags != null && !ModState.CrewCreateState.LifePaths[n].NodeTags.ContainsAll(nextNodeTags))
                            {
                                continue;
                            }
                            RequirementDef requirements2 = ModState.CrewCreateState.LifePaths[n].Requirements;
                            if (requirements2 != null)
                            {
                                TagSet requirementTags = requirements2.RequirementTags;
                                TagSet exclusionTags = requirements2.ExclusionTags;
                                List<ComparisonDef> requirementComparisons = requirements2.RequirementComparisons;
                                if (!SimGameState.MeetsRequirements(requirementTags, exclusionTags, requirementComparisons, tagSet, pilotDef.GetStats()))
                                {
                                    continue;
                                }
                            }
                            list7.Add(ModState.CrewCreateState.LifePaths[n]);
                        }
                        if (list7.Count > 0)
                        {
                            nextLifePathNode = (item.nextNode = list7[ModState.SimGameState.NetworkRandom.Int(0, list7.Count)]);
                        }
                        else
                        {
                            Debug.LogWarning("Unable to find new node from ending: " + lifepathNodeEnding.Description.Id + " | " + lifepathNodeDef.Description.Id);
                        }
                    }
                    endings.Add(item);
                }
                lifepathNodeDef = nextLifePathNode;
            }

            lifepathDescParagraphs = new StringBuilder();
            foreach (SimGameEventResultSet item2 in nodeEventResults)
            {
                if (item2.Description.Name != "")
                {
                    lifepathDescParagraphs.Append(string.Format("<b>{1}:</b> {0}\n\n", item2.Description.Details, item2.Description.Name));
                }
            }
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

        // Adapted from BattleTech.PilotGenerator.GetStartingNode for clarity
        private static LifepathNodeDef GetStartingNode(int systemDifficulty)
        {

            float advancedPilotChance = ModState.SimGameState.Constants.Pilot.AdvancedPilotBaseChance + (float)systemDifficulty * ModState.SimGameState.Constants.Pilot.AdvancedPilotDifficultyStep;
            Mod.Log.Debug?.Write($"Chance for advanced pilot => base: {ModState.SimGameState.Constants.Pilot.AdvancedPilotBaseChance} + systemDifficulty: {systemDifficulty} x difficultyStep: {ModState.SimGameState.Constants.Pilot.AdvancedPilotDifficultyStep}");
            float roll = (float)Mod.Random.NextDouble();

            bool isAdvancedPilot = advancedPilotChance > roll;
            List<LifepathNodeDef> list = isAdvancedPilot ? ModState.CrewCreateState.AdvancePaths : ModState.CrewCreateState.StartingPaths;

            int randomIdx = Mod.Random.Next(0, list.Count);
            LifepathNodeDef lifepathNode = list[randomIdx];
            Mod.Log.Debug?.Write($"Randomly determined lifePathNode: {lifepathNode?.Description?.Name}");

            return lifepathNode;
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

        // Direct copy of BattleTech.PilotGenerator.SetPilotAbilities
        private static void SetPilotAbilities(PilotDef pilot, string type, int value)
        {
            value--;
            if (value < 0 || 
                !ModState.SimGameState.AbilityTree.ContainsKey(type) || 
                ModState.SimGameState.AbilityTree[type].Count <= value)
            {
                return;
            }

            List<AbilityDef> list = ModState.SimGameState.AbilityTree[type][value];
            pilot.DataManager = ModState.SimGameState.DataManager;
            pilot.ForceRefreshAbilityDefs();
            for (int i = 0; i < list.Count; i++)
            {
                if (ModState.SimGameState.CanPilotTakeAbility(pilot, list[i]))
                {
                    pilot.abilityDefNames.Add(list[i].Description.Id);
                }
            }

            pilot.ForceRefreshAbilityDefs();
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

        // Direct copy of BattleTech.PilotGenerator
        private static SkillType SkillStringToType(string s)
        {
            switch (s)
            {
                case "Gunnery":
                    return SkillType.Gunnery;
                case "Guts":
                    return SkillType.Guts;
                case "Tactics":
                    return SkillType.Tactics;
                case "Piloting":
                    return SkillType.Piloting;
                default:
                    return SkillType.NotSet;
            }
        }

       
    }
}
