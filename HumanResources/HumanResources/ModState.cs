
using BattleTech;
using HumanResources.Extensions;
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
        public static bool HasLoadedAssets = false;

        public static PilotCreateState PilotCreate = new PilotCreateState();
        public static SimGameState SimGameState = null;
        public static Queue<(Pilot Pilot, CrewDetails Details)> ExpiredContracts = 
            new Queue<(Pilot, CrewDetails)>();

        public static void Reset()
        {
            // Reinitialize state
            Mod.Log.Info?.Write("CLEARING ALL MOD STATE DATA");
            SimGameState = null;
            PilotCreate = new PilotCreateState();
            ExpiredContracts.Clear();
        }

        // --- Methods manipulating CheckResults
        public static double[] InitializeCheckResults(double mean, double stdDev, int arraySize)
        {
            Mod.Log.Info?.Write($"Initializing a new random buffer of size: {arraySize} ");
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double[] GaussianResults = new double[arraySize];
            ZigguratGaussian.Sample(rng, mean, stdDev, GaussianResults);
            Mod.Log.Info?.Write("Initialization complete.");
            return GaussianResults;
        }

    }

}


