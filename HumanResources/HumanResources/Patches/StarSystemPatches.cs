using HumanResources.Crew;
using HumanResources.Helper;
using System;

namespace HumanResources.Patches
{

    [HarmonyPatch(typeof(StarSystem), "HirePilot")]
    [HarmonyBefore(new string[] { "io.github.mpstark.AbilityRealizer" })]
    static class StarSystem_HirePilot
    {

        static void Prefix(ref bool __runOriginal)
        {
            if (!__runOriginal) return;

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
        static void Prefix(ref bool __runOriginal, StarSystem __instance, int count)
        {
            if (!__runOriginal) return;

            int systemDiff = __instance.Def.GetDifficulty(SimGameState.SimGameType.CAREER);
            Mod.Log.Info?.Write($"Generating pilots for system: {__instance.Name} with difficulty: {systemDiff}");

            PlanetScarcity scarcity = __instance.GetScarcityForPlanet();

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
            if (mechWarriors > 0)
            {
                __instance.AvailablePilots.Clear();

                // Ronin DO NOT count against the system limits. Just add them, if the roll passes.
                double roninRoll = Mod.Random.NextDouble();
                Mod.Log.Debug?.Write($"Ronin roll of {roninRoll} vs roninChance: {Mod.Config.HiringHall.RoninChance}");
                if (roninRoll <= Mod.Config.HiringHall.RoninChance)
                {
                    Mod.Log.Debug?.Write($"Adding one Ronin to hiring hall.");
                    PilotDef unusedRonin = ModState.SimGameState.GetUnusedRonin();

                    if (!ModState.SimGameState.UsedRoninIDs.Contains(unusedRonin.Description.Id))
                    {
                        Mod.Log.Debug?.Write($"Added ronin: {unusedRonin.Description.DisplayName} to available pilots.");

                        PilotDef upgradedDef = CrewGenerator.UpgradeRonin(__instance, unusedRonin);
                        __instance.AvailablePilots.Add(upgradedDef);
                    }
                    else
                    {
                        Mod.Log.Debug?.Write($"Ronin: {unusedRonin.Description.DisplayName} already in use, skipping.");
                    }

                }

                for (int i = 0; i < mechWarriors; i++)
                {
                    PilotDef pDef = CrewGenerator.GenerateMechWarrior(__instance);
                    __instance.AvailablePilots.Add(pDef);
                }

            }

            if (Mod.Config.HiringHall.AerospaceWings.Enabled)
            {
                for (int i = 0; i < aerospace; i++)
                {
                    PilotDef pDef = CrewGenerator.GenerateAerospaceCrew(__instance);
                    __instance.AvailablePilots.Add(pDef);
                }
            }

            if (Mod.Config.HiringHall.MechTechCrews.Enabled)
            {
                for (int i = 0; i < mechTechs; i++)
                {
                    PilotDef pDef = CrewGenerator.GenerateMechTechCrew(__instance);
                    __instance.AvailablePilots.Add(pDef);
                }
            }

            if (Mod.Config.HiringHall.MedTechCrews.Enabled)
            {
                for (int i = 0; i < medTechs; i++)
                {
                    PilotDef pDef = CrewGenerator.GenerateMedTechCrew(__instance);
                    __instance.AvailablePilots.Add(pDef);
                }
            }

            if (Mod.Config.HiringHall.VehicleCrews.Enabled)
            {
                for (int i = 0; i < vehicleCrews; i++)
                {
                    PilotDef pDef = CrewGenerator.GenerateVehicleCrew(__instance);
                    __instance.AvailablePilots.Add(pDef);
                }
            }

            __runOriginal = false;
        }
    }
}
