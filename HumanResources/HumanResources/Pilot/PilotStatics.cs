using BattleTech;
using HumanResources.Extensions;
using System;
using System.Collections.Generic;

namespace HumanResources.Helper
{

    
    public static class PilotHelper
    {

        public static PilotScarcity GetScarcityForPlanet(StarSystem currentSystem)
        {
            float aerospaceUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.Aerospace;
            float mechTechsUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.MechTechs;
            float mechWarriorsUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.MechWarriors;
            float medTechsUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.MedTechs;
            float vehicleCrewsUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.VehicleCrews;

            foreach (string tag in currentSystem.Tags)
            {
                Mod.Config.HiringHall.Scarcity.PlanetTagModifiers.TryGetValue(tag, out CrewScarcity scarcity);
                if (scarcity != null)
                {
                    Mod.Log.Debug?.Write($"  tag: {tag} has scarcity =>  " +
                        $"aerospace: {scarcity.Aerospace}  mechTechs: {scarcity.MechTechs}  mechwarriors: {scarcity.MechWarriors}  medTechs: {scarcity.MedTechs}  vehicleCrews: {scarcity.VehicleCrews}");
                    aerospaceUpperBound += scarcity.Aerospace;
                    mechWarriorsUpperBound += scarcity.MechWarriors;
                    mechTechsUpperBound += scarcity.MechTechs;
                    medTechsUpperBound += scarcity.MedTechs;
                    vehicleCrewsUpperBound += scarcity.VehicleCrews;
                }
            }

            // Ceiling everything
            aerospaceUpperBound = (float)Math.Ceiling(aerospaceUpperBound);
            mechTechsUpperBound = (float)Math.Ceiling(mechTechsUpperBound);
            mechWarriorsUpperBound = (float)Math.Ceiling(mechWarriorsUpperBound);
            medTechsUpperBound = (float)Math.Ceiling(medTechsUpperBound);
            vehicleCrewsUpperBound = (float)Math.Ceiling(vehicleCrewsUpperBound);

            PilotScarcity pilotScarcity = new PilotScarcity();

            if (aerospaceUpperBound > 0)
            {
                int aerospaceLowerBound = (int)Math.Max(0, aerospaceUpperBound / 2);
                pilotScarcity.Aerospace = (aerospaceLowerBound, (int)aerospaceUpperBound);
            }

            if (mechTechsUpperBound > 0)
            {
                int mechTechLowerBound = (int)Math.Max(0, mechTechsUpperBound / 2);
                pilotScarcity.MechTechs = (mechTechLowerBound, (int)mechTechsUpperBound);
            }

            if (mechWarriorsUpperBound > 0)
            {
                int mechWarriorsLowerBound = (int)Math.Max(0, mechWarriorsUpperBound / 2);
                pilotScarcity.MechWarriors = (mechWarriorsLowerBound, (int)mechWarriorsUpperBound);
            }

            if (medTechsUpperBound > 0)
            {
                int medTechsLowerBound = (int)Math.Max(0, medTechsUpperBound / 2);
                pilotScarcity.MedTechs = (medTechsLowerBound, (int)medTechsUpperBound);
            }

            if (vehicleCrewsUpperBound > 0)
            {
                int vehicleCrewsLowerBound = (int)Math.Max(0, vehicleCrewsUpperBound / 2);
                pilotScarcity.Vehicles = (vehicleCrewsLowerBound, (int)vehicleCrewsUpperBound);
            }

            Mod.Log.Debug?.Write($"Planet: {currentSystem.Name} has final bounds:");
            Mod.Log.Debug?.Write($"  mechwarriors:  {pilotScarcity.MechWarriors.Lower} to {pilotScarcity.MechWarriors.Upper}");
            Mod.Log.Debug?.Write($"  vehicles:      {pilotScarcity.Vehicles.Lower} to {pilotScarcity.Vehicles.Upper}");
            Mod.Log.Debug?.Write($"  aerospace:     {pilotScarcity.Aerospace.Lower} to {pilotScarcity.Aerospace.Upper}");
            Mod.Log.Debug?.Write($"  mechTechs:     {pilotScarcity.MechTechs.Lower} to {pilotScarcity.MechTechs.Upper}");
            Mod.Log.Debug?.Write($"  medTechs:      {pilotScarcity.MedTechs.Lower} to {pilotScarcity.MedTechs.Upper}");

            return pilotScarcity;
        }

        public static int UsedBerths(IEnumerable<Pilot> pilots)
        {
            int used = 0;

            foreach (Pilot pilot in pilots)
            {
                CrewDetails details = ModState.GetCrewDetails(pilot.pilotDef);
                used += details.Size;
            }

            return used;
        }

        public static int RandomContractLength(CrewOpts opts)
        {
            int rand = Mod.Random.Next(opts.MinContractDaysMulti, opts.MaxContractDaysMulti);
            int length = rand * opts.BaseDaysInContract;

            Mod.Log.Debug?.Write($"Generated random contract length of: {length} days.");

            return length;
        }

        public static bool CanHireMoreCrewOfType(CrewDetails details)
        {
            CrewOpts crewOpt = null;
            Statistic crewStat = null;
            if (details.IsAerospaceCrew)
            {
                crewOpt = Mod.Config.HiringHall.AerospaceWings;
                crewStat = ModState.SimGameState.CompanyStats.GetStatistic(ModStats.CrewCount_Aerospace);
            }
            if (details.IsMechTechCrew)
            {
                crewOpt = Mod.Config.HiringHall.MechTechCrews;
                crewStat = ModState.SimGameState.CompanyStats.GetStatistic(ModStats.CrewCount_MechTechs);
            }
            if (details.IsMechWarrior)
            {
                crewOpt = Mod.Config.HiringHall.MechWarriors;
                crewStat = ModState.SimGameState.CompanyStats.GetStatistic(ModStats.CrewCount_MechWarriors);
            }
            if (details.IsMedTechCrew)
            {
                crewOpt = Mod.Config.HiringHall.MedTechCrews;
                crewStat = ModState.SimGameState.CompanyStats.GetStatistic(ModStats.CrewCount_MedTechs);
            }
            if (details.IsVehicleCrew)
            {
                crewOpt = Mod.Config.HiringHall.VehicleCrews;
                crewStat = ModState.SimGameState.CompanyStats.GetStatistic(ModStats.CrewCount_VehicleCrews);
            }

            int countOfType = crewStat == null ? 0 : crewStat.Value<int>();
            Mod.Log.Debug?.Write($"Comparing countOfType: {countOfType} vs. limit: {crewOpt.MaxOfType}");

            return crewOpt.MaxOfType == -1 || countOfType < crewOpt.MaxOfType;
        }
    }

    public class PilotScarcity
    {
        public (int Lower, int Upper) Aerospace = (0, 0);
        public (int Lower, int Upper) MechWarriors = (0, 0);
        public (int Lower, int Upper) MechTechs = (0, 0);
        public (int Lower, int Upper) MedTechs = (0, 0);
        public (int Lower, int Upper) Vehicles = (0, 0);
    }

}
