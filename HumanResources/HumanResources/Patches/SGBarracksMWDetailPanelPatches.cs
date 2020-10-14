using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using HBS.Extensions;
using HumanResources.Extensions;
using Localize;
using System.Collections.Generic;
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
            CrewDetails details = new CrewDetails(p.pilotDef);

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                ___advancementSectionGO.SetActive(false);
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

            CrewDetails details = new CrewDetails(___curPilot.pilotDef);

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                ___advancementSectionGO.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(SGBarracksServicePanel), "SetPilot")]
    static class SGBarracksServicePanel_SetPilot
    {
        static void Postfix(SGBarracksServicePanel __instance, Pilot p)
        {
            if (p == null) return;

            CrewDetails details = new CrewDetails(p.pilotDef);

            GameObject battleStats = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_ServicePanel_BattleStats);
            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
                battleStats.SetActive(false);
            else
                battleStats.SetActive(true);
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
            CrewDetails details = new CrewDetails(p.pilotDef);

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                ___healthText.SetText("N/A");
                for (int i = 0; i < ___healthList.Count; i++)
                {
                    ___healthList[i].SetActive(false);
                }
            }

            string nameS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Name_Format],
                new object[] { p.FirstName, p.LastName }
                ).ToString();
            ___firstName.SetText(nameS);

            GameObject lastNameGO = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_Dossier_LastName);
            GameObject layoutLabelGO = lastNameGO.transform.parent.GetChild(0).GetChild(0).gameObject; // should be text_lastName -> parent -> layout-label -> label
            LocalizableText lastNameLabel = layoutLabelGO.GetComponentInChildren<LocalizableText>();
            string contractTermS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Contract_Term])
                .ToString();
            lastNameLabel.SetText(contractTermS);

            string contractTermRemaining = "------";
            if (details.ContractEndDay != 0)
            {
                int daysRemaining = details.ContractEndDay - ModState.SimGameState.DaysPassed;
                if (daysRemaining < 0) daysRemaining = 0;
                contractTermRemaining = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Days_Remaining],
                    new object[] { daysRemaining }).ToString();
                Mod.Log.Debug?.Write($" {daysRemaining} daysRemaining = {ModState.SimGameState.DaysPassed} daysPassed - {details.ContractEndDay} endDay");
            }
            
            ___lastName.SetText(contractTermRemaining);
     
        }
    }

}
