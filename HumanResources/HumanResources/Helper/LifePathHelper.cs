using HumanResources.Lifepath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResources.Helper
{
    public static class LifePathHelper
    {
        private static int TotalFamiliesWeight = 0;

        public static LifePath GetRandomLifePath()
        {
            Mod.Log.Info?.Write("Selecting random lifepath:");

            // Determine the family
            int familyIdx = Mod.Random.Next(0, TotalFamiliesWeight);
            foreach (KeyValuePair<string, LifePathFamily> familyKVP in Mod.LifePathFamilies)
            {
                if (familyIdx >= familyKVP.Value.WeightBounds.Item1 && familyIdx <= familyKVP.Value.WeightBounds.Item2)
                {
                    Mod.Log.Info?.Write($" - Family index: {familyIdx} matched bounds {familyKVP.Value.WeightBounds} for family: {familyKVP.Key}");

                    // Iterate the lifepaths
                    int pathIdx = Mod.Random.Next(0, familyKVP.Value.WeightTotal);
                    foreach (KeyValuePair<string, LifePath> pathKVP in familyKVP.Value.Lifepaths)
                    {
                        if (pathIdx >= pathKVP.Value.WeightBounds.Item1 && pathIdx <= pathKVP.Value.WeightBounds.Item2)
                        {
                            Mod.Log.Info?.Write($" -- Path index: {pathIdx} matched bounds {pathKVP.Value.WeightBounds} for path: {pathKVP.Key}, returning lifepath.");
                            return pathKVP.Value;
                        }
                    }
                }
            }
            Mod.Log.Warn?.Write($"No lifepath or family could be found! This will likely error. Let frost know!");

            return null;
        }


        // Walk the loaded list of lifepaths and families and assign weights based upon iteration order.
        //   This *should* be stable as in C# dictionary key traverse order is fixed
        public static void InitAtModLoad()
        {
            Mod.Log.Info?.Write($"Calculating bounds for LifePath Weights");
            // Weight families
            foreach (KeyValuePair<string, LifePathFamily> familyKVP in Mod.LifePathFamilies)
            {
                familyKVP.Value.WeightBounds = (TotalFamiliesWeight, TotalFamiliesWeight + familyKVP.Value.Weight - 1);
                TotalFamiliesWeight += familyKVP.Value.Weight;

                Mod.Log.Info?.Write($" - LifePathFamily: {familyKVP.Key} assigned bounds: {familyKVP.Value.WeightBounds}");

                // For each LifePath, weight as well.
                int totalPathsWeight = 0;
                foreach (KeyValuePair<string, LifePath> pathKVP in familyKVP.Value.Lifepaths)
                {
                    pathKVP.Value.WeightBounds = (totalPathsWeight, totalPathsWeight + pathKVP.Value.Weight - 1);
                    totalPathsWeight += pathKVP.Value.Weight;
                    Mod.Log.Info?.Write($" -- LifePath: {pathKVP.Key} assigned bounds: {pathKVP.Value.WeightBounds}");

                    // Generate a compound key for the lifepath and populate the LifePaths dictionary with it for fast lookups
                    string pathCompoundKey = $"{familyKVP.Key}_{pathKVP.Key}";
                    if (!Mod.LifePaths.TryGetValue(pathCompoundKey, out LifePath value))
                    {
                        Mod.LifePaths.Add(pathCompoundKey, pathKVP.Value);
                    }
                }
                familyKVP.Value.WeightTotal = totalPathsWeight;

            }



        }
    }
}
