﻿using BattleTech;
using Harmony;
using HumanResources.Extensions;
using HumanResources.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HumanResources.Patches
{

    [HarmonyPatch(typeof(StarSystem), "HirePilot")]
    [HarmonyBefore("io.github.mpstark.AbilityRealizer")]
    static class StartSystem_HirePilot
    {
        // Contains a duplicate of https://github.com/BattletechModders/AbilityRealizer/blob/5fdbb5aa1576f18056d2b2a6236891f41fe44c26/AbilityRealizer/Patches/StarSystem.cs#L14
        //   plus extensions 
        static bool Prefix(StarSystem __instance, PilotDef def)
        {

            //		if (!__instance.AvailablePilots.Contains(def))
            if (__instance.AvailablePilots.Any(x => x.Description.Id == def.Description.Id))
            {
                __instance.AvailablePilots.Remove(def);
                if (__instance.PermanentRonin.Contains(def))
                {
                    __instance.PermanentRonin.Remove(def);
                    __instance.Sim.UsedRoninIDs.Add(def.Description.Id);
                }
                def.SetDayOfHire(__instance.Sim.DaysPassed);
                __instance.Sim.AddPilotToRoster(def, true, false);

                CrewDetails details = def.Evaluate();

                // Apply their hiring bonus
                __instance.Sim.AddFunds(-details.AdjustedBonus, null, true, true);

                // Add any mechtech, medtech, or aerospace points
                if (details.IsAerospaceCrew)
                {
                    // Track our skill points
                    Statistic aerospaceSkill = __instance.Sim.CompanyStats.GetStatistic(ModStats.Aerospace_Skill);
                        __instance.Sim.CompanyStats.AddStatistic<int>(ModStats.Aerospace_Skill, 0);

                    __instance.Sim.CompanyStats.ModifyStat<int>(null, -1,
                         ModStats.Aerospace_Skill,
                         StatCollection.StatOperation.Int_Add, details.AerospacePoints);

                    // Track the crew count
                    Statistic crewCount = __instance.Sim.CompanyStats.GetStatistic(ModStats.CrewCount_Aerospace);
                    __instance.Sim.CompanyStats.AddStatistic<int>(ModStats.CrewCount_Aerospace, 0);

                    __instance.Sim.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_Aerospace,
                        StatCollection.StatOperation.Int_Add, 1);
                }
                if (details.IsMechTechCrew)
                {
                    __instance.Sim.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.HBS_Company_MechTech_Skill,
                        StatCollection.StatOperation.Int_Add, details.MechTechPoints);

                    // Track the crew count
                    Statistic crewCount = __instance.Sim.CompanyStats.GetStatistic(ModStats.CrewCount_MechTechs);
                    __instance.Sim.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechTechs, 0);

                    __instance.Sim.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MechTechs,
                        StatCollection.StatOperation.Int_Add, 1);
                }
                else if (details.IsMechWarrior)
                {
                    // Track the crew count
                    Statistic crewCount = __instance.Sim.CompanyStats.GetStatistic(ModStats.CrewCount_MechWarriors);
                    __instance.Sim.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechWarriors, 0);

                    __instance.Sim.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MechWarriors,
                        StatCollection.StatOperation.Int_Add, 1);
                }
                else if (details.IsMedTechCrew)
                {
                    __instance.Sim.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.HBS_Company_MedTech_Skill,
                        StatCollection.StatOperation.Int_Add, details.MedTechPoints);

                    // Track the crew count
                    Statistic crewCount = __instance.Sim.CompanyStats.GetStatistic(ModStats.CrewCount_MedTechs);
                    __instance.Sim.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MedTechs, 0);

                    __instance.Sim.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MedTechs,
                        StatCollection.StatOperation.Int_Add, 1);
                }
                else if (details.IsVehicleCrew)
                {
                    // Track the crew count
                    Statistic crewCount = __instance.Sim.CompanyStats.GetStatistic(ModStats.CrewCount_VehicleCrews);
                    __instance.Sim.CompanyStats.AddStatistic<int>(ModStats.CrewCount_VehicleCrews, 0);

                    __instance.Sim.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_VehicleCrews,
                        StatCollection.StatOperation.Int_Add, 1);
                }

                __instance.Sim.RoomManager.RefreshTimeline(false);
                __instance.Sim.RoomManager.RefreshDisplay();
            }

            return false;
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
                    int contractLength = PilotHelper.RandomContractLength(Mod.Config.HiringHall.MechWarriors);
                    def.PilotTags.Add($"{ModTags.Tag_Crew_ContractTerm_Prefix}{contractLength}");
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

            for (int i = 0; i < aerospace; i++)
            {
                PilotDef pDef = PilotGen.GenerateAerospaceCrew();
                __instance.AvailablePilots.Add(pDef);
            }

            for (int i = 0; i < mechTechs; i++)
            {
                PilotDef pDef = PilotGen.GenerateMechTechCrew();
                __instance.AvailablePilots.Add(pDef);
            }

            for (int i = 0; i < medTechs; i++)
            {
                PilotDef pDef = PilotGen.GenerateMedTechCrew();
                __instance.AvailablePilots.Add(pDef);
            }

            for (int i = 0; i < vehicleCrews; i++)
            {
                PilotDef pDef = PilotGen.GenerateVehicleCrew(systemDiff);
                pDef.PilotTags.Add(ModTags.Tag_Crew_Type_Vehicle);
                pDef.PilotTags.Add(ModTags.Tag_CU_NoMech_Crew);
                pDef.PilotTags.Add(ModTags.Tag_CU_Vehicle_Crew);
                __instance.AvailablePilots.Add(pDef);
            }


            return false;
        }
    }
}
