using HumanResources.Crew;
using System;

namespace HumanResources.Helper
{
    public class SalaryConfig
    {
        public int Multi = 0;
        public float Exponent = 0f;
        public float Variance = 0f;
        public float BonusVariance = 0f;

        public SalaryConfig()
        {
        }
        public static SalaryConfig FromModConfig(CrewOpts config)
        {
            return new SalaryConfig()
            {
                Multi = config.SalaryMulti,
                Exponent = config.SalaryExponent,
                Variance = config.SalaryVariance,
                BonusVariance = config.BonusVariance
            };
        }

        public static SalaryConfig FromModConfig(CrewType type)
        {
            if (type == CrewType.AerospaceWing)
                return FromModConfig(Mod.Config.HiringHall.AerospaceWings);
            else if (type == CrewType.MechTechCrew)
                return FromModConfig(Mod.Config.HiringHall.MechTechCrews);
            else if (type == CrewType.MechWarrior)
                return FromModConfig(Mod.Config.HiringHall.MechWarriors);
            else if (type == CrewType.MedTechCrew)
                return FromModConfig(Mod.Config.HiringHall.MedTechCrews);
            else if (type == CrewType.VehicleCrew)
                return FromModConfig(Mod.Config.HiringHall.VehicleCrews);
            else
                return FromModConfig(Mod.Config.HiringHall.MechWarriors);
        }

        public bool IsDefault()
        {
            return Multi == 0 && Exponent == 0f && Variance == 0f && BonusVariance == 0f;
        }
    }

    public static class SalaryHelper
    {
        public static void CalcSalary(int value, SalaryConfig config, out int salary, out int bonus)
        {
            int salaryByValue = (int)Math.Floor(config.Multi * (float)Math.Pow(config.Exponent, value));
            Mod.Log.Debug?.Write($" -- salaryByValue: {salaryByValue}");

            // Determine the final salary by adding variance
            float maxSalary = salaryByValue * config.Variance;
            float salaryDelta = maxSalary - salaryByValue;
            float salaryVariance = salaryDelta * (float)Mod.Random.NextDouble();
            Mod.Log.Debug?.Write($" -- baseSalary: {salaryByValue} + salaryVariance: {salaryVariance}");
            int rawFinalSalary = (int)Math.Ceiling(salaryByValue + salaryVariance);
            salary = (rawFinalSalary / 10) * 10; // Round to nearest 10 cbills
            Mod.Log.Debug?.Write($" -- salary: {salary}");

            // Determine the bonus by adding variance to the salary
            float maxBonus = salaryByValue * config.BonusVariance;
            float bonusDelta = maxBonus - salaryByValue;
            float bonusVariance = bonusDelta * (float)Mod.Random.NextDouble();
            Mod.Log.Debug?.Write($" -- baseSalary: {salaryByValue} + bonusVariance: {bonusVariance}");
            int rawFinalBonus = (int)Math.Ceiling(salaryByValue + bonusVariance);
            bonus = (rawFinalBonus / 10) * 10; // Round to nearest 10 cbills
            Mod.Log.Debug?.Write($" -- bonus: {bonus}");

            return;
        }

        public static int CalcCounterOffer(CrewDetails details)
        {
            float counterOfferBounds = details.AdjustedBonus * Mod.Config.HeadHunting.CounterOfferVariance;
            float delta = counterOfferBounds - details.AdjustedBonus;
            float variance = delta * (float)Mod.Random.NextDouble();
            Mod.Log.Debug?.Write($"Counter offer => adjustedBonus: {details.AdjustedBonus} + variance: {variance}");

            int rawCounterOffer = (int)Math.Ceiling(details.AdjustedBonus + variance);
            int counterOffer = (rawCounterOffer / 10) * 10; // Round to nearest 10 cbills
            Mod.Log.Debug?.Write($" -- counterOffer: {counterOffer}");

            return counterOffer;
        }
    }
}
