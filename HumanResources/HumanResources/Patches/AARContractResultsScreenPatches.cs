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
    [HarmonyPatch(typeof(AAR_ContractResults_Screen), "OnCompleted")]
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

            //if (this.State == Contract.ContractState.Complete)
            //{
            //    num14 = simulation.Constants.Story.EmployerRepSuccessMod;
            //    num15 = simulation.Constants.Story.TargetRepSuccessMod;
            //    num16 = simulation.Constants.Story.MRBSuccessMod;
            //}
            //else if (this.IsGoodFaithEffort)
            //{
            //    num14 = simulation.Constants.Story.EmployerRepGoodFaithMod;
            //    num15 = simulation.Constants.Story.TargetRepGoodFaithMod;
            //    num16 = simulation.Constants.Story.MRBGoodFaithMod;
            //}
            //else
            //{
            //    num14 = simulation.Constants.Story.EmployerRepBadFaithMod;
            //    num15 = simulation.Constants.Story.TargetRepBadFaithMod;
            //    num16 = simulation.Constants.Story.MRBFailureMod;
            //}

            foreach (Pilot p in combatPilots)
            {
                Mod.Log.Debug?.Write($"Applying attitude modifiers to pilot: {p.Name}");
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

                // Check for bench
                if (deployedPilots.Contains(p))
                {
                    Mod.Log.Debug?.Write($" -- pilot was deployed, adding {Mod.Config.Attitude.DeployedOnMissionMod} attitude");
                    details.Attitude += Mod.Config.Attitude.DeployedOnMissionMod;
                }
                else
                {
                    Mod.Log.Debug?.Write($" -- pilot was benched, adding {Mod.Config.Attitude.BenchedOnMissionMod} attitude");
                    details.Attitude += Mod.Config.Attitude.BenchedOnMissionMod;
                }

 


                ModState.UpdateOrCreateCrewDetails(p.pilotDef, details);
            }

            //public int PilotLossMod = -10;
            //public int ContractLossMod = -10;

            //public int FavoredEmployerAlliedMonthlyMod = 6;
            //public int FavoredEmployerPerMissionMod = 1;

            //public int HatedEmployerAlliedMonthlyMod = -30;
            //public int HatedEmployerPerMissionMod = -3;

    }
    }
}
