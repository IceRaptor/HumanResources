using BattleTech;
using BattleTech.StringInterpolation;
using BattleTech.UI;
using Harmony;
using HBS.Collections;
using HBS.Extensions;
using HumanResources.Extensions;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGBarracksServicePanel), "SetPilot")]
    static class SGBarracksServicePanel_SetPilot
    {
        static void Postfix(SGBarracksServicePanel __instance, Pilot p, 
            BattleTech.UI.TMProWrapper.LocalizableText ___biographyLabel, SimGameState ___sim, HBSTagView ___tagViewer)
        {
            if (p == null) return;

            CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

            // Disable the battle stats 
            GameObject battleStats = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_ServicePanel_BattleStats);
            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
                battleStats.SetActive(false);
            else
                battleStats.SetActive(true);

            Mod.Log.Debug?.Write($"Iterating tags for pilot: {p.Name}");
            // Filter the HR GUID tag out
            List<string> filteredTags = p.pilotDef.PilotTags.Where(t => !t.StartsWith(ModTags.Tag_GUID)).ToList();
            TagSet baseTags = new TagSet(filteredTags);
            foreach (string tag in p.pilotDef.PilotTags)
            ___tagViewer.Initialize(baseTags, ___sim.Context, ___sim.DebugMode, 4);

            Mod.Log.Debug?.Write("Updating attitude fields.");

            StringBuilder sb = new StringBuilder();

            if (!details.IsPlayer)
            {
                // Inject hazard pay, if appropriate
                if (details.HazardPay > 0)
                {
                    string hazardPayS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Hazard_Pay],
                        new object[] { SimGameState.GetCBillString(details.HazardPay) }).ToString();
                    Mod.Log.Debug?.Write($"Hazard pay is: {hazardPayS}");
                    sb.Append(hazardPayS);
                    sb.Append("\n");
                }

                // Inject attitude
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
            }

            // Add the original description
            sb.Append(Interpolator.Interpolate(p.pilotDef.Description.GetLocalizedDetails().ToString(true), ___sim.Context, true));

            string biographyS = sb.ToString();
            Mod.Log.Debug?.Write($"Biography will be: {biographyS}");
            ___biographyLabel.SetText(biographyS, Array.Empty<object>());
        }
    }
}
