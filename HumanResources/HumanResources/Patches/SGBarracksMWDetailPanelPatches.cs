﻿using BattleTech;
using BattleTech.StringInterpolation;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using HBS.Collections;
using HBS.Extensions;
using HumanResources.Extensions;
using Localize;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGBarracksMWDetailPanel), "DisplayPilot")]
    static class SGBarracksMWDetailPanel_DisplayPilot
    {
        static void Postfix(SGBarracksMWDetailPanel __instance, Pilot p,
            SGBarracksAdvancementPanel ___advancement, GameObject ___advancementSectionGO,
            SGBarracksDossierPanel ___dossier, 
            SGBarracksServicePanel ___servicePanel, GameObject ___serviceSectionGO,
            HBSDOTweenButton ___customizeButton)
        {
            if (p == null) return;
            CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

            GameObject skillsButton = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_Skill_Button);
            if (skillsButton == null)
                Mod.Log.Debug?.Write("SkillsButton is null!");

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                __instance.OnServiceSectionClicked();
                skillsButton.SetActive(false);
            }
            else
            {
                skillsButton.SetActive(true);
            }
        }
    }

    [HarmonyPatch(typeof(SGBarracksMWDetailPanel), "OnSkillsSectionClicked")]
    static class SGBarracksMWDetailPanel_OnSkillsSectionClicked
    {
        static void Postfix(SGBarracksMWDetailPanel __instance,
            Pilot ___curPilot,
            SGBarracksAdvancementPanel ___advancement, GameObject ___advancementSectionGO,
            SGBarracksDossierPanel ___dossier,
            SGBarracksServicePanel ___servicePanel, GameObject ___serviceSectionGO,
            HBSDOTweenButton ___customizeButton)
        {
            if (___curPilot == null) return;

            CrewDetails details = ModState.GetCrewDetails(___curPilot.pilotDef);

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                ___advancementSectionGO.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(SGBarracksServicePanel), "SetPilot")]
    static class SGBarracksServicePanel_SetPilot
    {
        static void Postfix(SGBarracksServicePanel __instance, Pilot p, LocalizableText ___biographyLabel, SimGameState ___sim, HBSTagView ___tagViewer)
        {
            if (p == null) return;

            CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

            // Disable the battle stats 
            GameObject battleStats = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_ServicePanel_BattleStats);
            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
                battleStats.SetActive(false);
            else
                battleStats.SetActive(true);

            // Reinitialize the tag viewer, stripping any tags that are HR centric
            TagSet baseTags = new TagSet();
            foreach (string tag in p.pilotDef.PilotTags)
            {
                if (!tag.StartsWith("HR_"))
                    baseTags.Add(tag);
            }
            ___tagViewer.Initialize(baseTags, ___sim.Context, ___sim.DebugMode, 4);

            Mod.Log.Debug?.Write("Updating attitude fields.");

            // Inject attitude
            StringBuilder sb = new StringBuilder();

            // TODO: Convert attitude to text label
            string attitudeValKey = ModText.LT_Crew_Attitude_Average;
            if (details.Attitude >= Mod.Config.Attitude.ThresholdBest)
                attitudeValKey = ModText.LT_Crew_Attitude_Best;
            else if (details.Attitude >= Mod.Config.Attitude.ThresholdGood)
                attitudeValKey = ModText.LT_Crew_Attitude_Good;
            else if (details.Attitude <= Mod.Config.Attitude.ThresholdWorst)
                attitudeValKey = ModText.LT_Crew_Attitude_Worst;
            else if (details.Attitude <= Mod.Config.Attitude.ThresholdPoor)
                attitudeValKey = ModText.LT_Crew_Attitude_Poor;

            string attitudeDescS = new Text(Mod.LocalizedText.Labels[attitudeValKey]).ToString();
            Mod.Log.Debug?.Write($"Attitude value is: {details.Attitude} with label: {attitudeDescS}");

            string attitudeS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Biography_Attitude], 
                new object[] { attitudeDescS, details.Attitude }).ToString();
            sb.Append(attitudeS);
            sb.Append("\n");

            // Convert favored and hated faction
            if (details.FavoredFaction > 0)
            {
                FactionValue faction = FactionEnumeration.GetFactionByID(details.FavoredFaction);
                string favoredFactionS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Biography_Faction_Favored], new object[] { faction.FriendlyName }).ToString();
                sb.Append(favoredFactionS);
                sb.Append("\n");
                Mod.Log.Debug?.Write($"  Favored Faction is: {favoredFactionS}");
            }

            if (details.HatedFaction > 0)
            {
                FactionValue faction = FactionEnumeration.GetFactionByID(details.HatedFaction);
                string hatedFactionS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Biography_Faction_Hated], new object[] { faction.FriendlyName }).ToString();
                sb.Append(hatedFactionS);
                sb.Append("\n");
                Mod.Log.Debug?.Write($"  Hated Faction is: {hatedFactionS}");
            }

            // Add the original description
            sb.Append(Interpolator.Interpolate(p.pilotDef.Description.GetLocalizedDetails().ToString(true), ___sim.Context, true));

            string biographyS = sb.ToString();
            Mod.Log.Debug?.Write($"Biography will be: {biographyS}");
            ___biographyLabel.SetText(biographyS, Array.Empty<object>());
        }
    }

    [HarmonyPatch(typeof(SGBarracksDossierPanel), "SetPilot")]
    static class SGBarracksDossierPanel_SetPilot
    {
        static void Postfix(SGBarracksDossierPanel __instance, Pilot p, 
            LocalizableText ___healthText, List<GameObject> ___healthList, LocalizableText ___salary,
            LocalizableText ___firstName, LocalizableText ___lastName)
        {
            if (p == null) return;

            Mod.Log.Debug?.Write($"Updating Dossier for pilot: {p.Name}");
            CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                ___healthText.SetText("N/A");
                for (int i = 0; i < ___healthList.Count; i++)
                {
                    ___healthList[i].SetActive(false);
                }
            }

            string nameS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Name_Format], 
                new object[] { p.FirstName, p.LastName }).ToString();
            ___firstName.SetText(nameS);

            // Set the firstname label to 'Name' instead of 'First Name'
            Mod.Log.Debug?.Write("Updating firstName to Name");
            GameObject firstNameGO = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_Dossier_LastName);
            GameObject firstNameLabelGO = firstNameGO.transform.parent.GetChild(0).GetChild(0).gameObject;
            LocalizableText firstNameLabel = firstNameLabelGO.GetComponentInChildren<LocalizableText>();
            string firstNameS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Contract_Term]).ToString();
            firstNameLabel.SetText(firstNameS);

            // Set the lastname label to 'Contract End' and the name value to the remaining days
            Mod.Log.Debug?.Write("Updating lastName to ContractTerm");
            GameObject lastNameGO = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_Dossier_LastName);
            GameObject lastNameLabelGO = lastNameGO.transform.parent.GetChild(0).GetChild(0).gameObject; // should be text_lastName -> parent -> layout-label -> label
            LocalizableText lastNameLabel = lastNameLabelGO.GetComponentInChildren<LocalizableText>();
            string contractTermS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Contract_Term]).ToString();
            lastNameLabel.SetText(contractTermS);

            GameObject skillsButton = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_Skill_Button);
            if (skillsButton == null)
                Mod.Log.Debug?.Write("SBGDP:SetPilot - SkillsButton is null!");
            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                //skillsButton.SetActive(false);
            }
            else
            {
                //skillsButton.SetActive(true);
            }

            string contractTermRemaining = "------";
            if (!details.IsPlayer && details.ContractTerm != 0)
            {
                int daysRemaining = details.ExpirationDay - ModState.SimGameState.DaysPassed;
                if (daysRemaining < 0) daysRemaining = 0;
                contractTermRemaining = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Days_Remaining],
                    new object[] { daysRemaining }).ToString();
                Mod.Log.Debug?.Write($" {daysRemaining} daysRemaining = {ModState.SimGameState.DaysPassed} daysPassed - {details.ExpirationDay} endDay");
            }
            
            ___lastName.SetText(contractTermRemaining);
            Mod.Log.Debug?.Write($"  -- done updating dossier for pilot: {p.Name}");

        }
    }

}
