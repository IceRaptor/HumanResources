using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(PilotGenerator), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(SimGameState), typeof(List<LifepathNodeDef>), typeof(List<SimGameStringList>) })]
    static class PilotGenerator_Ctor
    {
        static void Postfix(SimGameState sim, List<LifepathNodeDef> ___lifepaths, List<LifepathNodeDef> ___startingPaths,
            List<LifepathNodeDef> ___advanceStartingPaths, GenderedOptionsListDef ___voiceList)
        {
            ModState.CrewCreateState.NameGenerator = new PilotNameGenerator();

            ModState.CrewCreateState.LifePaths = ___lifepaths;
            ModState.CrewCreateState.StartingPaths = ___startingPaths;
            ModState.CrewCreateState.AdvancePaths = ___advanceStartingPaths;
            ModState.CrewCreateState.Voices = ___voiceList;

            ModState.CrewCreateState.GenderWeights = new List<int>()
            {
                sim.Constants.Pilot.FemaleGenerationWeight,
                sim.Constants.Pilot.MaleGenerationWeight,
                sim.Constants.Pilot.NonBinaryGenerationWeight
            };

            Mod.Log.Info?.Write("Initialized Pilot Creation shared state.");
        }
    }
}
