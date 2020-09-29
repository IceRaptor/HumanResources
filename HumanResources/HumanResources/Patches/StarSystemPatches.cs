using BattleTech;
using Harmony;
using HumanResources.Helper;
using System;
using System.Collections.Generic;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(StarSystem), "GeneratePilots")]
    static class StarSystem_GeneratePilots
    {
        static bool Prepare() => Mod.Config.HiringHall.EnableScarcity;

        // TODO: Manipulate # of pilots by planet tags
        // TODO: Manipulate # of ronin by planet tags
        // TODO: Check for Allow flags by each type
        static bool Prefix(StarSystem __instance, int count)
        {
            int systemDiff = __instance.Def.GetDifficulty(SimGameState.SimGameType.CAREER);
            Mod.Log.Info?.Write($"Generating pilots for system: {__instance.Name} with difficulty: {systemDiff}");

            int mechWarriorsUpperBound = 0;
            int vehicleCrewsUpperBound = 0;
            int mechTechsUpperBound = 0;
            int medTechsUpperBound = 0;
            int aerospaceUpperBound = 0;

            foreach (string tag in __instance.Tags)
            {
                Mod.Config.HiringHall.ScarcityByPlanetTag.TryGetValue(tag, out CrewScarcity scarcity);
                if (scarcity != null)
                {
                    Mod.Log.Debug?.Write($" tag: {tag} has scarcity =>  " +
                        $"mechwarriors: {scarcity.MechWarriors}  mechTechs: {scarcity.MechTechs}  medTechs: {scarcity.MedTechs}  vehicleCrews: {scarcity.VehicleCrews}");
                    mechWarriorsUpperBound += scarcity.MechWarriors;
                    mechTechsUpperBound += scarcity.MechTechs;
                    medTechsUpperBound += scarcity.MedTechs;
                    vehicleCrewsUpperBound += scarcity.VehicleCrews;
                    aerospaceUpperBound += scarcity.Aerospace;
                }
            }
            Mod.Log.Debug?.Write($"Final scarcity for planet {__instance.Name} => " +
                $"mechwarriors: {mechWarriorsUpperBound}  mechTechs: {mechTechsUpperBound}  medTechs: {medTechsUpperBound}  vehicleCrews: {vehicleCrewsUpperBound}");

            int mechWarriorsLowerBound = Math.Max(0, mechWarriorsUpperBound / 2);
            Mod.Log.Debug?.Write($"  MechWarriors lowerBound: {mechWarriorsLowerBound}  upperBound: {mechWarriorsUpperBound}");
            int vehicleCrewsLowerBound = Math.Max(0, vehicleCrewsUpperBound / 2);
            Mod.Log.Debug?.Write($"  VehicleCrews lowerBound: {vehicleCrewsLowerBound}  upperBound: {vehicleCrewsUpperBound}");
            int mechTechLowerBound = Math.Max(0, mechTechsUpperBound / 2);
            Mod.Log.Debug?.Write($"  MechTechs lowerBound: {mechTechLowerBound}  upperBound: {mechTechsUpperBound}");
            int medTechsLowerBound = Math.Max(0, medTechsUpperBound / 2);
            Mod.Log.Debug?.Write($"  MedTechs lowerBound: {medTechsLowerBound}  upperBound: {medTechsUpperBound}");
            int aerospaceLowerBound = Math.Max(0, aerospaceUpperBound / 2);
            Mod.Log.Debug?.Write($"  Aerospace lowerBound: {aerospaceLowerBound}  upperBound: {aerospaceUpperBound}");

            int mechWarriors = mechWarriorsUpperBound > 0 ? 
                Math.Max(0, Mod.Random.Next(mechWarriorsLowerBound, mechWarriorsUpperBound)) : 0;
            int vehicleCrews = vehicleCrewsUpperBound > 0 ? 
                Math.Max(0, Mod.Random.Next(vehicleCrewsLowerBound, vehicleCrewsUpperBound)) : 0;
            int mechTechs = mechTechsUpperBound > 0 ? 
                Math.Max(0, Mod.Random.Next(mechTechLowerBound, mechTechsUpperBound)) : 0;
            int medTechs = medTechsUpperBound > 0 ? 
                Math.Max(0, Mod.Random.Next(medTechsLowerBound, medTechsUpperBound)) : 0;
            int aerospace = aerospaceUpperBound > 0 ?
                Math.Max(0, Mod.Random.Next(aerospaceLowerBound, aerospaceUpperBound)) : 0;

            Mod.Log.Debug?.Write($"Generated  {mechWarriors} mechWarriors  {vehicleCrews} vehicleCrews  " +
                $"{mechTechs} mechTechs  {medTechs} medTechs  {aerospace} aerospace");

            // Generate pilots and crews

            // Direct copy of StarSystem.GeneratePilots()
            if (mechWarriors > 0)
            {
                __instance.AvailablePilots.Clear();
                List<PilotDef> roninList = new List<PilotDef>();
                float roninChance = __instance.Def.UseSystemRoninHiringChance ? 
                    __instance.Def.RoninHiringChance : __instance.Sim.Constants.Story.DefaultRoninHiringChance;
                List<PilotDef> collection = 
                    __instance.Sim.PilotGenerator.GeneratePilots(count, __instance.Def.GetDifficulty(__instance.Sim.SimGameMode), roninChance, out roninList);
                
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

            for (int i = 0; i < vehicleCrews; i++)
            {
                PilotDef pDef = PilotHelper.GenerateVehicleCrew(systemDiff);
                pDef.PilotTags.Add(ModTags.Tag_CrewType_Vehicle);
                __instance.AvailablePilots.Add(pDef);
            }

            for (int i = 0; i < mechTechs; i++)
            {
                PilotDef pDef = PilotHelper.GenerateTechs(3, true);
                __instance.AvailablePilots.Add(pDef);
            }

            for (int i = 0; i < medTechs; i++)
            {
                PilotDef pDef = PilotHelper.GenerateTechs(3, false);
                __instance.AvailablePilots.Add(pDef);
            }

            // TODO: Add aerospace

            return false;
        }
    }
}
