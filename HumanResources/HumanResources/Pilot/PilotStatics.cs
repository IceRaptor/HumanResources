using BattleTech;
using HumanResources.Extensions;
using System;
using System.Collections.Generic;

namespace HumanResources.Helper
{

    public static class PilotHelper
    {

        public static int UsedBerths(IEnumerable<Pilot> pilots)
        {
            int used = 0;

            foreach (Pilot pilot in pilots)
            {
                CrewDetails details = pilot.pilotDef.Evaluate();
                used += details.Size;
            }

            return used;
        }

        public static int RandomContractLength(CrewOpts opts)
        {
            int rand = Mod.Random.Next(opts.MinContractDaysMulti, opts.MaxContractDaysMulti);
            int length = rand * opts.BaseContractDays;

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

    public static class PilotExtensions
    {
        public static CrewDetails Evaluate(this PilotDef pilotDef)
        {
            return new CrewDetails(pilotDef);
        }
    }
}
