using BattleTech.UI;
using HBS.Extensions;
using HumanResources.Crew;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGBarracksMWDetailPanel), "DisplayPilot")]
    static class SGBarracksMWDetailPanel_DisplayPilot
    {
        static void Postfix(SGBarracksMWDetailPanel __instance, Pilot p)
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
        static void Postfix(SGBarracksMWDetailPanel __instance)
        {
            if (__instance.curPilot == null) return;

            CrewDetails details = ModState.GetCrewDetails(__instance.curPilot.pilotDef);

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                __instance.advancementSectionGO.SetActive(false);
            }
        }
    }
}
