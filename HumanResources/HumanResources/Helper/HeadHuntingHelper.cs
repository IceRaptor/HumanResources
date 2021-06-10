using BattleTech;
using HumanResources.Crew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResources.Helper
{
    public static class HeadHuntingHelper
    {
        public static bool ShouldCheckHeadHunting()
        {
            if (!Mod.Config.HeadHunting.Enabled) return false;
            Statistic nextDayToTestStat = ModState.SimGameState.CompanyStats.GetStatistic(ModStats.Company_HeadHunting_TestOnDay);

            bool shouldCheck;
            if (nextDayToTestStat == null)
            {
                ModState.SimGameState.CompanyStats.AddStatistic<int>(ModStats.Company_HeadHunting_TestOnDay, 0);
                ModState.SimGameState.CompanyStats.Set<int>(ModStats.Company_HeadHunting_TestOnDay, ModState.SimGameState.DaysPassed);
                Mod.Log.Debug?.Write($"Should headhunt as statistic didn't exist; initializing to daysPassed:{ModState.SimGameState.DaysPassed}");
                shouldCheck = true;
            }
            else if (nextDayToTestStat.Value<int>() <= ModState.SimGameState.DaysPassed)
            {
                Mod.Log.Info?.Write($"Can headhunt as next HeadHunting day is: {nextDayToTestStat.Value<int>()} <= daysPassed:{ModState.SimGameState.DaysPassed} ");
                shouldCheck = true;
            }
            else
            {
                Mod.Log.Debug?.Write($"Should not headhunt as next HeadHunting day is: {nextDayToTestStat.Value<int>()} > daysPassed:{ModState.SimGameState.DaysPassed}");
                shouldCheck = false;
            }

            return shouldCheck;
        }

        public static Pilot TestAllCrews()
        {

            // Determine our headhunting roll
            float randomRoll = (float)Mod.Random.NextDouble();
            float econMod = Mod.Config.HeadHunting.EconMods[(int)ModState.SimGameState.ExpenditureLevel + 2];
            float modifiedRoll = randomRoll + econMod;
            Mod.Log.Info?.Write($"HeadHunting roll: {modifiedRoll} = randomRoll: {randomRoll} + econMod: {econMod}");
            if (econMod >= 100 || econMod <= 0)
            {
                Mod.Log.Info?.Write($"  outside 0-100 bounds, failing roll.");
                return null;
            }

            // Determine a random order for the list
            List<Pilot> pilots = ModState.SimGameState.PilotRoster.ToList();
            pilots.Shuffle<Pilot>();

            Pilot headHuntedPilot = null;
            foreach (Pilot pilot in pilots)
            {
                CrewDetails cd = ModState.GetCrewDetails(pilot.pilotDef);
                Mod.Log.Debug?.Write($"Checking pilot: {pilot.Name} for poaching for expertise: {cd.Expertise}");
                float crewChance = Mod.Config.HeadHunting.ChanceBySkill[cd.Expertise];
                Mod.Log.Debug?.Write($"  -- chance for crew: {crewChance} vs. modifiedRoll: {modifiedRoll}");
                if (crewChance <= modifiedRoll)
                {
                    Mod.Log.Info?.Write($"Pilot: {pilot.Name} should be headhunted.");
                    headHuntedPilot = pilot;
                    break;
                }
            }

            return headHuntedPilot;

        }

        public static void UpdateNextDayOnSystemEntry()
        {
            if (!Mod.Config.HeadHunting.Enabled) return;

            // Set the next head-hunting day to some time between the error cases, which should be lower
            int daysToWait = Mod.Random.Next(Mod.Config.HeadHunting.FailedCooldownIntervalMin, Mod.Config.HeadHunting.FailedCooldownIntervalMax);
            int nextDay = ModState.SimGameState.DaysPassed + daysToWait;

            Mod.Log.Info?.Write($"Updating nextDay for HeadHunting to: {nextDay} onSystemEntry");
            ModState.SimGameState.CompanyStats.Set<int>(ModStats.Company_HeadHunting_TestOnDay, nextDay);
        }

        public static void UpdateNextDayOnFailure()
        {
            if (!Mod.Config.HeadHunting.Enabled) return;

            // Set the next head-hunting day to some time between the error cases, which should be lower
            int daysToWait = Mod.Random.Next(Mod.Config.HeadHunting.FailedCooldownIntervalMin, Mod.Config.HeadHunting.FailedCooldownIntervalMax);
            int nextDay = ModState.SimGameState.DaysPassed + daysToWait;

            Mod.Log.Info?.Write($"Updating nextDay for HeadHunting to: {nextDay} onFailure");
            ModState.SimGameState.CompanyStats.Set<int>(ModStats.Company_HeadHunting_TestOnDay, nextDay);

        }

        public static void UpdateNextDayOnSuccess()
        {
            if (!Mod.Config.HeadHunting.Enabled) return;

            // Set the next head-hunting day to some time between the error cases, which should be lower
            int daysToWait = Mod.Random.Next(Mod.Config.HeadHunting.SuccessCooldownIntervalMin, Mod.Config.HeadHunting.SuccessCooldownIntervalMax);
            int nextDay = ModState.SimGameState.DaysPassed + daysToWait;

            Mod.Log.Info?.Write($"Updating nextDay for HeadHunting to: {nextDay} OnSuccess");
            ModState.SimGameState.CompanyStats.Set<int>(ModStats.Company_HeadHunting_TestOnDay, nextDay);

        }
    }
}
