
using BattleTech;
using System;
using System.Collections.Generic;
using us.frostraptor.modUtils.Redzen;

namespace HumanResources
{

    public class PilotCreateState
    {
        public PilotNameGenerator NameGenerator = null;

        public List<LifepathNodeDef> LifePaths = null;
        public List<LifepathNodeDef> StartingPaths = null;
        public List<LifepathNodeDef> AdvancePaths = null;

        public GenderedOptionsListDef Voices = null;

        public List<int> GenderWeights = null;
        public readonly Gender[] Genders = { Gender.Female, Gender.Male, Gender.NonBinary };
    }

    public static class ModState
    {
        public static PilotCreateState PilotCreate = new PilotCreateState();
        public static SimGameState SimGameState = null;

        // Gaussian probabilities
        public static double[] SkillCheckResults = new double[ModConsts.GaussianResultsPerGeneration];
        public static int SkillCheckIdx = 0;
        public static double[] SizeCheckResults = new double[ModConsts.GaussianResultsPerGeneration];
        public static int SizeCheckIdx = 0;

        public static void Reset()
        {
            // Reinitialize state
            SimGameState = null;
            PilotCreate = new PilotCreateState();

            SkillCheckResults = new double[ModConsts.GaussianResultsPerGeneration];
            SkillCheckIdx = 0;

            SizeCheckResults = new double[ModConsts.GaussianResultsPerGeneration];
            SizeCheckIdx = 0;
        }

        // --- Methods manipulating CheckResults
        public static void InitializeCheckResults()
        {
            Mod.Log.Info?.Write($"Initializing a new random buffer of size: {ModConsts.GaussianResultsPerGeneration} ");
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double mean = Mod.Config.HiringHall.SkillDistribution.Mu;
            double stdDev = Mod.Config.HiringHall.SkillDistribution.Sigma;
            ZigguratGaussian.Sample(rng, mean, stdDev, SkillCheckResults);
            SkillCheckIdx = 0;
        }

        public static int GetCheckResult(bool skillCheck)
        {
            if (SkillCheckIdx < 0 || SkillCheckIdx >= ModConsts.GaussianResultsPerGeneration)
            {
                Mod.Log.Info?.Write($"ERROR: CheckResultIdx of {SkillCheckIdx} is out of bounds! THIS SHOULD NOT HAPPEN!");
            }

            double result;
            if (skillCheck)
            {
                result = SkillCheckResults[SkillCheckIdx];
                SkillCheckIdx++;
            }
            else
            {
                result = SizeCheckResults[SizeCheckIdx];
                SizeCheckIdx++;
            }

            // Normalize floats to integer buckets for easier comparison
            if (result > 0)
            {
                result = Math.Floor(result);
            }
            else if (result < 0)
            {
                result = Math.Ceiling(result);
            }

            return (int)result;
        }
    }

}


