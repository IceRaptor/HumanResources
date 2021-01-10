using BattleTech;
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
}
