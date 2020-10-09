using System;

namespace HumanResources.Helper
{
    public static class GaussianHelper
    {
        public static string GetCrewSkillTag(double muMod, double sigmaMod)
        {
            float[] breakpoints = Mod.Config.HiringHall.SkillDistribution.Breakpoints;

            double baseMu = Mod.Config.HiringHall.SkillDistribution.Mu;
            double mu = baseMu + muMod;

            double baseSigma = Mod.Config.HiringHall.SkillDistribution.Sigma;
            double sigma = baseSigma + sigmaMod;

            int size = GetIndex(mu, sigma, breakpoints);
            return String.Format(ModTags.Tag_Crew_Skill_Template, new object[] { size });
        }

        public static string GetCrewSizeTag(double muMod, double sigmaMod)
        {
            float[] breakpoints = Mod.Config.HiringHall.SizeDistribution.Breakpoints;

            double baseMu = Mod.Config.HiringHall.SizeDistribution.Mu;
            double mu = baseMu + muMod;

            double baseSigma = Mod.Config.HiringHall.SizeDistribution.Sigma;
            double sigma = baseSigma + sigmaMod;

            int size = GetIndex(mu, sigma, breakpoints);
            return String.Format(ModTags.Tag_Crew_Size_Template, new object[] { size });
        }

        private static int GetIndex(double mean, double stdDev, float[] breakpoints)
        {
            double[] gaussianResults = ModState.InitializeCheckResults(mean, stdDev, 1024);
            double result = gaussianResults[0];
            int index = 0;
            foreach (double breakpoint in breakpoints)
            {
                if (result < breakpoint)
                {
                    Mod.Log.Info?.Write($" result: {result} < breakpoint: {breakpoint}");
                    break;
                }
                else
                {
                    index++;
                }
            }

            return index + 1;
        }

    }
}
