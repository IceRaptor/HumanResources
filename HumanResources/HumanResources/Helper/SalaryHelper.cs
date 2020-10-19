using System;

namespace HumanResources.Helper
{
    public static class SalaryHelper
    {
        public static void CalculateSalary(int value, CrewOpts config, out int salary, out int bonus)
        {
            int salaryByValue = (int)Math.Floor(config.SalaryMulti * (float)Math.Pow(config.SalaryExponent, value));
            Mod.Log.Debug?.Write($" -- salaryByValue: {salaryByValue}");

            // Determine the final salary by adding variance
            float maxSalary = salaryByValue * config.SalaryVariance;
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
    }
}
