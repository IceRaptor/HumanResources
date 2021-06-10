using BattleTech;
using BattleTech.UI;
using Harmony;
using HumanResources.Crew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(BattleTech.UI.AAR_ContractResults_Screen), "OnCompleted")]
    static class AAR_ContractResults_Screen
    {
        static void Postfix(BattleTech.UI.AAR_ContractResults_Screen __instance, Contract ___theContract)
        {
            List<Pilot> deployedPilots = ___theContract.PlayerUnitResults.Select(ur => ur.pilot).ToList();

            // Find just combat pilots (for benched mod)
            List<Pilot> allPilots = __instance.Sim.PilotRoster.ToList();

            List<Pilot> combatPilots = __instance.Sim.PilotRoster.Where(p => {
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                if (details.IsMechTechCrew || details.IsVehicleCrew)
                    return true;
                else
                    return false;
            }).ToList();

            int killedPilotsMod = Mod.Config.Attitude.PerMission.PilotKilledMod * ___theContract.KilledPilots.Count;
            Mod.Log.Debug?.Write($"Player lost {___theContract.KilledPilots.Count} pilots, applying a modifier of {killedPilotsMod} to all pilots.");

            // Calculate the contract bonus 
            int contractBonus = Mod.Config.Attitude.PerMission.ContractFailedMod;
            if (___theContract.State == Contract.ContractState.Complete)
            {
                contractBonus = Mod.Config.Attitude.PerMission.ContractSuccessMod;
                Mod.Log.Debug?.Write($"Contract was successful, applying contract modifier of: {contractBonus}");
            }
            else if (___theContract.IsGoodFaithEffort)
            {
                contractBonus = Mod.Config.Attitude.PerMission.ContractFailedGoodFaithMod;
                Mod.Log.Debug?.Write($"Contract was a good faith effort, applying contract modifier of: {contractBonus}");
            }
            else
            {
                Mod.Log.Debug?.Write($"Contract was failed, applying contract modifier of: {contractBonus}");
            }

            // Iterate pilots apply modifiers
            foreach (Pilot p in allPilots)
            {
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                
                if (details.IsPlayer) continue;

                Mod.Log.Debug?.Write($"Applying attitude modifiers to pilot: {p.Name}");

                // Check for bench - only applies to combat pilots
                if (deployedPilots.Contains(p))
                {
                    Mod.Log.Debug?.Write($" -- combat pilot was deployed, adding {Mod.Config.Attitude.PerMission.DeployedOnMissionMod} attitude");
                    details.Attitude += Mod.Config.Attitude.PerMission.DeployedOnMissionMod;
                }
                else if (combatPilots.Contains(p))
                {
                    Mod.Log.Debug?.Write($" -- combat pilot was benched, adding {Mod.Config.Attitude.PerMission.BenchedOnMissionMod} attitude");
                    details.Attitude += Mod.Config.Attitude.PerMission.BenchedOnMissionMod;
                }

                // Apply modifier for contract success
                details.Attitude += contractBonus;

                // Applied killed pilots modifier
                details.Attitude += killedPilotsMod;

                // Check favored / hated factions
                if (details.FavoredFaction > 0) 
                {
                    if (details.FavoredFaction == ___theContract.Override.employerTeam.FactionValue.ID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot favors employer faction, applying modifier: {Mod.Config.Attitude.PerMission.FavoredFactionIsEmployerMod}");
                        details.Attitude += Mod.Config.Attitude.PerMission.FavoredFactionIsEmployerMod;
                    }

                    if (details.FavoredFaction == ___theContract.Override.targetTeam.FactionValue.ID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot favors target faction, applying modifier: {Mod.Config.Attitude.PerMission.FavoredFactionIsTargetMod}");
                        details.Attitude += Mod.Config.Attitude.PerMission.FavoredFactionIsTargetMod;
                    }
                }

                if (details.HatedFaction > 0)
                {
                    if (details.HatedFaction == ___theContract.Override.employerTeam.FactionValue.ID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot hates employer faction, applying modifier: {Mod.Config.Attitude.PerMission.HatedEmployerIsEmployerMod}");
                        details.Attitude += Mod.Config.Attitude.PerMission.HatedEmployerIsEmployerMod;
                    }

                    if (details.HatedFaction == ___theContract.Override.targetTeam.FactionValue.ID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot hates target faction, applying modifier: {Mod.Config.Attitude.PerMission.HatedEmployerIsTargetMod}");
                        details.Attitude += Mod.Config.Attitude.PerMission.HatedEmployerIsTargetMod;
                    }
                }

                // Clamp values to max and min
                details.Attitude = Mathf.Clamp(details.Attitude, Mod.Config.Attitude.ThresholdMin, Mod.Config.Attitude.ThresholdMax);

                ModState.UpdateOrCreateCrewDetails(p.pilotDef, details);
            }

        }
    }
}
