using BattleTech;
using BattleTech.UI;
using Harmony;
using HumanResources.Crew;
using Localize;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(BattleTech.UI.AAR_ContractResults_Screen), "OnCompleted")]
    static class AAR_ContractResults_Screen
    {
        static void Postfix(BattleTech.UI.AAR_ContractResults_Screen __instance, Contract ___theContract)
        {

            StringBuilder bodySB = new StringBuilder();
            bodySB.Append(new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Body], new object[] { }).ToString());            

            int killedPilotsMod = Mod.Config.Attitude.PerMission.PilotKilledMod * ___theContract.KilledPilots.Count;
            if (___theContract.KilledPilots.Count > 0)
            {
                Mod.Log.Debug?.Write($"Player lost {___theContract.KilledPilots.Count} pilots, applying a modifier of {killedPilotsMod} to all pilots.");
                bodySB.Append(
                     new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Pilot_Killed],
                     new object[] { killedPilotsMod }).ToString()
                    );
            }

            // Add the monospace tag
            bodySB.Append(new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Font], new object[] { }).ToString());

            // Calculate the contract bonus 
            int contractBonus = Mod.Config.Attitude.PerMission.ContractFailedMod;
            if (___theContract.State == Contract.ContractState.Complete)
            {
                contractBonus = Mod.Config.Attitude.PerMission.ContractSuccessMod;
                Mod.Log.Debug?.Write($"Contract was successful, applying contract modifier of: {contractBonus}");
                bodySB.Append(
                     new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Contract_Success],
                     new object[] { contractBonus }).ToString()
                    );

            }
            else if (___theContract.IsGoodFaithEffort)
            {
                contractBonus = Mod.Config.Attitude.PerMission.ContractFailedGoodFaithMod;
                Mod.Log.Debug?.Write($"Contract was a good faith effort, applying contract modifier of: {contractBonus}");
                bodySB.Append(
                    new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Contract_GoodFaith],
                    new object[] { contractBonus }).ToString()
                   );
            }
            else
            {
                Mod.Log.Debug?.Write($"Contract was failed, applying contract modifier of: {contractBonus}");
                bodySB.Append(
                    new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Contract_Failed],
                    new object[] { contractBonus }).ToString()
                   );
            }
            bodySB.Append("\n");

            // Find just combat pilots (for benched mod)
            List<Pilot> allPilots = __instance.Sim.PilotRoster.ToList();
            List<Pilot> combatPilots = allPilots.Where(p =>
            {
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                return details.IsCombatCrew;
            }).ToList();
            Mod.Log.Debug?.Write($"All Combat pilots are: {string.Join(", ", combatPilots.Select(p => p.Name).ToList())}");

            List<Pilot> deployedPilots = ___theContract.PlayerUnitResults.Select(ur => ur.pilot).ToList();
            Mod.Log.Debug?.Write($"Deployed pilots were: {string.Join(", ", deployedPilots.Select(p => p.Name).ToList())}");

            // Iterate pilots apply modifiers
            foreach (Pilot p in allPilots)
            {
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

                if (details.IsPlayer) continue;

                int startingAttitude = details.Attitude;

                Mod.Log.Debug?.Write($"Applying attitude modifiers to pilot: {p.Name}");

                // Apply modifier for contract success
                details.Attitude += contractBonus;

                // Applied killed pilots modifier
                details.Attitude += killedPilotsMod;

                List<string> detailDescs = new List<string>();
                // Check for bench - only applies to combat pilots
                if (deployedPilots.Contains(p))
                {
                    Mod.Log.Debug?.Write($" -- combat pilot was deployed, adding {Mod.Config.Attitude.PerMission.DeployedOnMissionMod} attitude");
                    details.Attitude += Mod.Config.Attitude.PerMission.DeployedOnMissionMod;
                    detailDescs.Add(
                        new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Deployed],
                        new object[] { Mod.Config.Attitude.PerMission.DeployedOnMissionMod }).ToString()
                       );
                }
                else if (combatPilots.Contains(p))
                {
                    Mod.Log.Debug?.Write($" -- combat pilot was benched, adding {Mod.Config.Attitude.PerMission.BenchedOnMissionMod} attitude");
                    details.Attitude += Mod.Config.Attitude.PerMission.BenchedOnMissionMod;
                    detailDescs.Add(
                        new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Benched],
                        new object[] { Mod.Config.Attitude.PerMission.BenchedOnMissionMod }).ToString()
                       );
                }

                // Check favored / hated factions
                if (details.FavoredFactionId > 0)
                {
                    if (details.FavoredFactionId == ___theContract.Override.employerTeam.FactionValue.FactionID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot favors employer faction, applying modifier: {Mod.Config.Attitude.PerMission.FavoredFactionIsEmployerMod}");
                        details.Attitude += Mod.Config.Attitude.PerMission.FavoredFactionIsEmployerMod;
                        detailDescs.Add(
                                new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Favored_Employer],
                            new object[] { Mod.Config.Attitude.PerMission.FavoredFactionIsEmployerMod }).ToString()
                           );
                    }

                    if (details.FavoredFactionId == ___theContract.Override.targetTeam.FactionValue.FactionID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot favors target faction, applying modifier: {Mod.Config.Attitude.PerMission.FavoredFactionIsTargetMod}");
                        details.Attitude += Mod.Config.Attitude.PerMission.FavoredFactionIsTargetMod;
                        detailDescs.Add(
                                new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Favored_Target],
                            new object[] { Mod.Config.Attitude.PerMission.FavoredFactionIsTargetMod }).ToString()
                           );
                    }
                }

                if (details.HatedFactionId > 0)
                {
                    if (details.HatedFactionId == ___theContract.Override.employerTeam.FactionValue.FactionID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot hates employer faction, applying modifier: {Mod.Config.Attitude.PerMission.HatedFactionIsEmployerMod}");
                        details.Attitude += Mod.Config.Attitude.PerMission.HatedFactionIsEmployerMod;
                        detailDescs.Add(
                                new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Hated_Employer],
                            new object[] { Mod.Config.Attitude.PerMission.HatedFactionIsEmployerMod }).ToString()
                           );
                    }

                    if (details.HatedFactionId == ___theContract.Override.targetTeam.FactionValue.FactionID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot hates target faction, applying modifier: {Mod.Config.Attitude.PerMission.HatedFactionIsTargetMod}");
                        details.Attitude += Mod.Config.Attitude.PerMission.HatedFactionIsTargetMod;
                        detailDescs.Add(
                                new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Mod_Hated_Target],
                            new object[] { Mod.Config.Attitude.PerMission.HatedFactionIsTargetMod }).ToString()
                           );
                    }
                }

                // Clamp values to max and min
                details.Attitude = Mathf.Clamp(details.Attitude, Mod.Config.Attitude.ThresholdMin, Mod.Config.Attitude.ThresholdMax);

                string detailsS = string.Join(", ", detailDescs);
                bodySB.Append(
                    new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Pilot_Line],
                    new object[] {details.Attitude, p.Callsign, detailsS }).ToString()
                    );

                ModState.UpdateOrCreateCrewDetails(p.pilotDef, details);
            }

            // Close the mspace tag
            bodySB.Append("</mspace>");

            // Display a dialog with changes
            string localDialogTitle = new Text(Mod.LocalizedText.Dialogs[ModText.DIAT_AAR_Title]).ToString();
            string localDialogText = bodySB.ToString();
            GenericPopup gp = GenericPopupBuilder.Create(localDialogTitle, localDialogText)
                .Render();

            TextMeshProUGUI contentText = (TextMeshProUGUI)Traverse.Create(gp).Field("_contentText").GetValue();
            contentText.alignment = TextAlignmentOptions.Left;
        }
    }
}
