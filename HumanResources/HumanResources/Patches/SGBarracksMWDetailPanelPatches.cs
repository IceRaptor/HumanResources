using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using HBS.Extensions;
using HumanResources.Extensions;
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

    [HarmonyPatch(typeof(SGBarracksMWDetailPanel), "OnServiceSectionClicked")]
    static class SGBarracksMWDetailPanel_OnServiceSectionClicked
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
                // TODO:
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

            GameObject battleStats = __instance.gameObject.FindFirstChildNamed("obj-BattleStats");
            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                // TODO:
                battleStats.SetActive(false);
            }
            else
            {
                battleStats.SetActive(true);
            }
        }
    }

    [HarmonyPatch(typeof(SGBarracksDossierPanel), "SetPilot")]
    static class SGBarracksDossierPanel_SetPilot
    {
        static void Postfix(SGBarracksDossierPanel __instance, Pilot p, 
            LocalizableText ___healthText, List<GameObject> ___healthList, LocalizableText ___salary)
        {
            if (p == null) return;

            CrewDetails details = new CrewDetails(p.pilotDef);

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                // TODO:
                ___healthText.SetText("N/A");
                for (int i = 0; i < ___healthList.Count; i++)
                {
                    ___healthList[i].SetActive(false);
                }
            }
     
        }
    }

}
