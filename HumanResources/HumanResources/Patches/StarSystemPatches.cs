using BattleTech;
using Harmony;
using HumanResources.Extensions;
using HumanResources.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HumanResources.Patches
{

    [HarmonyPatch(typeof(StarSystem), "HirePilot")]
    [HarmonyBefore(new string[] { "io.github.mpstark.AbilityRealizer" })]
    static class StarSystem_HirePilot
    {

        static void Prefix()
        {
            Mod.Log.Debug?.Write("ENABLING HIRING FLOW FLAG");
            ModState.IsHiringFlow = false;
        }

        static void Postfix()
        {
            Mod.Log.Debug?.Write("DISABLING HIRING FLOW FLAG");
            ModState.IsHiringFlow = false;
        }
    }

    [HarmonyPatch(typeof(StarSystem), "GeneratePilots")]
    static class StarSystem_GeneratePilots
    {
        static bool Prepare() => Mod.Config.HiringHall.Scarcity.Enabled;

        // TODO: Manipulate # of pilots by planet tags
        // TODO: Manipulate # of ronin by planet tags
        // TODO: Check for Allow flags by each type
        static bool Prefix(StarSystem __instance, int count)
        {
            int systemDiff = __instance.Def.GetDifficulty(SimGameState.SimGameType.CAREER);
            Mod.Log.Info?.Write($"Generating pilots for system: {__instance.Name} with difficulty: {systemDiff}");

            PilotScarcity scarcity = PilotHelper.GetScarcityForPlanet(__instance);

            int aerospace = scarcity.Aerospace.Upper > 0 ?
                Math.Max(0, Mod.Random.Next(scarcity.Aerospace.Lower, scarcity.Aerospace.Upper)) : 0;
            int mechTechs = scarcity.MechTechs.Upper > 0 ?
                Math.Max(0, Mod.Random.Next(scarcity.MechTechs.Lower, scarcity.MechTechs.Upper)) : 0;
            int mechWarriors = scarcity.MechWarriors.Upper > 0 ? 
                Math.Max(0, Mod.Random.Next(scarcity.MechWarriors.Lower, scarcity.MechWarriors.Upper)) : 0;
            int medTechs = scarcity.MedTechs.Upper > 0 ?
                Math.Max(0, Mod.Random.Next(scarcity.MedTechs.Lower, scarcity.MedTechs.Upper)) : 0;
            int vehicleCrews = scarcity.Vehicles.Upper > 0 ?
                Math.Max(0, Mod.Random.Next(scarcity.Vehicles.Lower, scarcity.Vehicles.Upper)) : 0;

            Mod.Log.Debug?.Write($"Generated mechwarriors: {mechWarriors}  vehicleCrews: {vehicleCrews}  " +
                $"mechTechs: {mechTechs}  medTechs: {medTechs}  aerospace: {aerospace}");

            // Generate pilots and crews

            // Direct copy of StarSystem.GeneratePilots()
            if (mechWarriors > 0)
            {
                __instance.AvailablePilots.Clear();
                float roninChance = __instance.Def.UseSystemRoninHiringChance ? 
                    __instance.Def.RoninHiringChance : __instance.Sim.Constants.Story.DefaultRoninHiringChance;
                List<PilotDef> roninList;
                List<PilotDef> collection = 
                    __instance.Sim.PilotGenerator.GeneratePilots(count, systemDiff, roninChance, out roninList);

                // For each pilot, set a contract length (handled for crews elsewhere)
                foreach (PilotDef def in collection)
                {
                    // Determine contract length
                    CrewDetails details = new CrewDetails(def, CrewType.MechWarrior);
                    ModState.UpdateOrCreateCrewDetails(def, details);
                }
                
                // Remove Ronins that have already been used
                for (int num = roninList.Count - 1; num >= 0; num--)
                {
                    if (__instance.Sim.UsedRoninIDs.Contains(roninList[num].Description.Id))
                    {
                        roninList.RemoveAt(num);
                    }
                }

                for (int num2 = __instance.PermanentRonin.Count - 1; num2 >= 0; num2--)
                {
                    if (__instance.Sim.UsedRoninIDs.Contains(__instance.PermanentRonin[num2].Description.Id))
                    {
                        __instance.PermanentRonin.RemoveAt(num2);
                    }
                }

                // Add the pilot to the collection
                __instance.AvailablePilots.AddRange(collection);

                // Update the ronin tracking
                if (roninList.Count > 0)
                {
                    __instance.AvailablePilots.AddRange(roninList);
                }
                if (__instance.PermanentRonin.Count > 0)
                {
                    __instance.AvailablePilots.AddRange(__instance.PermanentRonin);
                }
            }

            if (Mod.Config.HiringHall.AerospaceWings.Enabled)
            {
                for (int i = 0; i < aerospace; i++)
                {
                    PilotDef pDef = PilotGen.GenerateAerospaceCrew();
                    __instance.AvailablePilots.Add(pDef);
                }
            }

            if (Mod.Config.HiringHall.MechTechCrews.Enabled)
            {
                for (int i = 0; i < mechTechs; i++)
                {
                    PilotDef pDef = PilotGen.GenerateMechTechCrew();
                    __instance.AvailablePilots.Add(pDef);
                }
            }

            if (Mod.Config.HiringHall.MedTechCrews.Enabled)
            {
                for (int i = 0; i < medTechs; i++)
                {
                    PilotDef pDef = PilotGen.GenerateMedTechCrew();
                    __instance.AvailablePilots.Add(pDef);
                }
            }

            if (Mod.Config.HiringHall.VehicleCrews.Enabled)
            {
                for (int i = 0; i < vehicleCrews; i++)
                {
                    PilotDef pDef = PilotGen.GenerateVehicleCrew(systemDiff);

                    Mod.Log.Debug?.Write($"CREATED VEHICLE CREW");
                    // Before returning, initialize the cache value
                    CrewDetails details = new CrewDetails(pDef, CrewType.VehicleCrew);
                    ModState.UpdateOrCreateCrewDetails(pDef, details);

                    __instance.AvailablePilots.Add(pDef);
                }
            }
            return false;
        }
    }
}
