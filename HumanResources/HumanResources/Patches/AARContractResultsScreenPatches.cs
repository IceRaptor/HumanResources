using BattleTech;
using BattleTech.UI;
using Harmony;
using HumanResources.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(BattleTech.UI.AAR_ContractResults_Screen), "OnCompleted")]
    static class AAR_ContractResults_Screen
    {
        static void Postfix(BattleTech.UI.AAR_ContractResults_Screen __instance, Contract ___theContract)
        {
            List<Pilot> deployedPilots = ___theContract.PlayerUnitResults.Select(ur => ur.pilot).ToList();

            // Find all pilots
            List<Pilot> combatPilots = __instance.Sim.PilotRoster.Where(p => {
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                if (details.IsMechTechCrew || details.IsVehicleCrew)
                    return true;
                else
                    return false;
            }).ToList();

            int killedPilotsMod = Mod.Config.Attitude.PilotKilledMod * ___theContract.KilledPilots.Count;
            Mod.Log.Debug?.Write($"Player lost {___theContract.KilledPilots.Count} pilots, applying a modifier of {killedPilotsMod} to all pilots.");

            // Apply the contract bonus 
            int contractBonus = Mod.Config.Attitude.ContractFailedMod;
            if (___theContract.State == Contract.ContractState.Complete)
            {
                contractBonus = Mod.Config.Attitude.ContractSuccessMod;
                Mod.Log.Debug?.Write($"Contract was successful, applying contract modifier of: {contractBonus}");
            }
            else if (___theContract.IsGoodFaithEffort)
            {
                contractBonus = Mod.Config.Attitude.ContractFailedGoodFaithMod;
                Mod.Log.Debug?.Write($"Contract was a good faith effort, applying contract modifier of: {contractBonus}");
            }
            else
            {
                Mod.Log.Debug?.Write($"Contract was failed, applying contract modifier of: {contractBonus}");
            }

            // Iterate pilots apply modifiers
            foreach (Pilot p in combatPilots)
            {
                Mod.Log.Debug?.Write($"Applying attitude modifiers to pilot: {p.Name}");
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

                // Check for bench
                if (deployedPilots.Contains(p))
                {
                    Mod.Log.Debug?.Write($" -- pilot was deployed, adding {Mod.Config.Attitude.DeployedOnMissionMod} attitude");
                    details.Attitude += Mod.Config.Attitude.DeployedOnMissionMod;
                    
                    details.Attitude += contractBonus;
                }
                else
                {
                    Mod.Log.Debug?.Write($" -- pilot was benched, adding {Mod.Config.Attitude.BenchedOnMissionMod} attitude");
                    details.Attitude += Mod.Config.Attitude.BenchedOnMissionMod;
                }

                // Applied killed pilots modifier
                details.Attitude += killedPilotsMod;

                // Check favored / hated factions
                if (details.FavoredFaction > 0) 
                {
                    if (details.FavoredFaction == ___theContract.Override.employerTeam.FactionValue.ID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot favors employer faction, applying modifier: {Mod.Config.Attitude.FavoredFactionIsEmployerMod}");
                    }

                    if (details.FavoredFaction == ___theContract.Override.targetTeam.FactionValue.ID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot favors target faction, applying modifier: {Mod.Config.Attitude.FavoredFactionIsTargetMod}");
                    }
                }

                if (details.HatedFaction > 0)
                {
                    if (details.HatedFaction == ___theContract.Override.employerTeam.FactionValue.ID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot hates employer faction, applying modifier: {Mod.Config.Attitude.HatedEmployerIsEmployerMod}");
                    }

                    if (details.HatedFaction == ___theContract.Override.targetTeam.FactionValue.ID)
                    {
                        Mod.Log.Debug?.Write($" -- pilot hates target faction, applying modifier: {Mod.Config.Attitude.HatedEmployerIsTargetMod}");
                    }
                }

                ModState.UpdateOrCreateCrewDetails(p.pilotDef, details);
            }

        }
    }
}
