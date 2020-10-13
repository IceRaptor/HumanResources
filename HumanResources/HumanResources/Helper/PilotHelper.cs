using BattleTech;
using BattleTech.Portraits;
using HBS.Collections;
using HumanResources.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HumanResources.Helper
{

    public static class PilotHelper
    {

        public static int UsedBerths(IEnumerable<Pilot> pilots)
        {
            int used = 0;

            foreach (Pilot pilot in pilots)
            {
                CrewDetails details = pilot.pilotDef.Evaluate();
                used += details.Size;
            }

            return used;
        }

        public static PilotDef GenerateTechs(int systemDifficulty, bool isMechTech)
        {

            int initialAge = ModState.SimGameState.Constants.Pilot.MinimumPilotAge + ModState.SimGameState.NetworkRandom.Int(1, ModState.SimGameState.Constants.Pilot.StartingAgeRange + 1);

            Gender newGender = RandomGender();
            string newFirstName = ModState.PilotCreate.NameGenerator.GetFirstName(newGender);
            string newLastName = ModState.PilotCreate.NameGenerator.GetLastName();
            //string newCallsign = RandomUnusedCallsign();
            int callsignIdx = isMechTech ? Mod.Random.Next(0, Mod.CrewNames.MechTech.Count) : Mod.Random.Next(0, Mod.CrewNames.MedTech.Count);
            string newCallsign = isMechTech ? Mod.CrewNames.MechTech[callsignIdx] : Mod.CrewNames.MedTech[callsignIdx];
            Mod.Log.Debug?.Write($"Generating mechTech: {isMechTech} with callsign: {newCallsign}");

            int currentAge = Mod.Random.Next(initialAge, 70);
            PilotDef pilotDef = new PilotDef(new HumanDescriptionDef(), 1, 1, 1, 1, 1, 1, lethalInjury: false, 1, "", new List<string>(), AIPersonality.Undefined, 0, 0, 0);
            
            TagSet tagSet = new TagSet();
            if (isMechTech) tagSet.Add(ModTags.Tag_Crew_Type_MechTech);
            else tagSet.Add(ModTags.Tag_Crew_Type_MedTech);

            // TODO: Determine crew size
            string sizeTag = GaussianHelper.GetCrewSizeTag(0, 0);
            Mod.Log.Info?.Write($" Adding sizeTag: {sizeTag}");
            tagSet.Add(sizeTag);

            // TODO: Determine crew skill
            string skillTag = GaussianHelper.GetCrewSkillTag(0, 0);
            Mod.Log.Info?.Write($" Adding skillTag: {skillTag}");
            tagSet.Add(skillTag);

            // TODO: Randomize N factions
            // TODO: Build jibberish history
            // TODO: Add crew size, loyalty, etc to description
            StringBuilder lifepathDescParagraphs = new StringBuilder();

            // DETAILS string is EXTREMELY picky, see HumanDescriptionDef.GetLocalizedDetails
            lifepathDescParagraphs.Append($"{Environment.NewLine}<b>Crew:</b>CREW IS MECHTECH: {isMechTech}\n\n");

            string id = GenerateID();
            Gender voiceGender = newGender;
            if (voiceGender == Gender.NonBinary)
            {
                voiceGender = ((!(ModState.SimGameState.NetworkRandom.Float() < 0.5f)) ? Gender.Female : Gender.Male);
            }
            string voice = RandomUnusedVoice(voiceGender);
            HumanDescriptionDef description = new HumanDescriptionDef(id, newCallsign, newFirstName, newLastName, newCallsign, newGender, FactionEnumeration.GetNoFactionValue(), currentAge, lifepathDescParagraphs.ToString(), null);

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

            PilotDef pilotDef2 = new PilotDef(description, pilotDef.BaseGunnery, pilotDef.BasePiloting, pilotDef.BaseGuts, pilotDef.BaseTactics, 0,
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
        public static PilotDef GenerateVehicleCrew(int systemDifficulty)
        {
            int initialAge = ModState.SimGameState.Constants.Pilot.MinimumPilotAge + ModState.SimGameState.NetworkRandom.Int(1, ModState.SimGameState.Constants.Pilot.StartingAgeRange + 1);

            Gender newGender = RandomGender();
            string newFirstName = ModState.PilotCreate.NameGenerator.GetFirstName(newGender);
            string newLastName = ModState.PilotCreate.NameGenerator.GetLastName();
            string newCallsign = RandomUnusedCallsign();
            Mod.Log.Debug?.Write($"Generating vehicleCrew with callsign: {newCallsign}");

            int currentAge;
            PilotDef pilotDef;
            TagSet tagSet;
            StringBuilder lifepathDescParagraphs;
            GenAndWalkLifePath(systemDifficulty, initialAge, out currentAge, out pilotDef, out tagSet, out lifepathDescParagraphs);

            string id = GenerateID();
            Gender voiceGender = newGender;
            if (voiceGender == Gender.NonBinary)
            {
                voiceGender = ((!(ModState.SimGameState.NetworkRandom.Float() < 0.5f)) ? Gender.Female : Gender.Male);
            }
            string voice = RandomUnusedVoice(voiceGender);
            HumanDescriptionDef description = new HumanDescriptionDef(id, newCallsign, newFirstName, newLastName, newCallsign, newGender, FactionEnumeration.GetNoFactionValue(), currentAge, lifepathDescParagraphs.ToString(), null);

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

            PilotDef pilotDef2 = new PilotDef(description, pilotDef.BaseGunnery, pilotDef.BasePiloting, pilotDef.BaseGuts, pilotDef.BaseTactics, 0,
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
                        for (int n = 0; n < ModState.PilotCreate.LifePaths.Count; n++)
                        {
                            if (nextNodeTags != null && !ModState.PilotCreate.LifePaths[n].NodeTags.ContainsAll(nextNodeTags))
                            {
                                continue;
                            }
                            RequirementDef requirements2 = ModState.PilotCreate.LifePaths[n].Requirements;
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
                            list7.Add(ModState.PilotCreate.LifePaths[n]);
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
            List<string> availableCallsigns = ModState.PilotCreate.NameGenerator.GetAllCallsigns();
            
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
            int weightedResult = SimGameState.GetWeightedResult(ModState.PilotCreate.GenderWeights, (float)Mod.Random.NextDouble());
            Gender gender = ModState.PilotCreate.Genders[weightedResult];
            return gender;
        }

        // Adapted from BattleTech.PilotGenerator.GetStartingNode for clarity
        private static LifepathNodeDef GetStartingNode(int systemDifficulty)
        {

            float advancedPilotChance = ModState.SimGameState.Constants.Pilot.AdvancedPilotBaseChance + (float)systemDifficulty * ModState.SimGameState.Constants.Pilot.AdvancedPilotDifficultyStep;
            Mod.Log.Debug?.Write($"Chance for advanced pilot => base: {ModState.SimGameState.Constants.Pilot.AdvancedPilotBaseChance} + systemDifficulty: {systemDifficulty} x difficultyStep: {ModState.SimGameState.Constants.Pilot.AdvancedPilotDifficultyStep}");
            float roll = (float)Mod.Random.NextDouble();

            bool isAdvancedPilot = advancedPilotChance > roll;
            List<LifepathNodeDef> list = isAdvancedPilot ? ModState.PilotCreate.AdvancePaths : ModState.PilotCreate.StartingPaths;

            int randomIdx = Mod.Random.Next(0, list.Count);
            LifepathNodeDef lifepathNode = list[randomIdx];
            Mod.Log.Debug?.Write($"Randomly determined lifePathNode: {lifepathNode?.Description?.Name}");

            return lifepathNode;
        }

        // Direct copy from BattleTech.PilotGenerator.GenerateVoiceForGender
        private static string RandomUnusedVoice(Gender gender)
        {
            List<string> allOptionsForGender = ModState.PilotCreate.Voices.GetAllOptionsForGender(gender);
            
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
