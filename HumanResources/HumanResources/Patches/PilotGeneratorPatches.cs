using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;

namespace PitCrew.Patches
{
    [HarmonyPatch(typeof(PilotGenerator), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(SimGameState), typeof(List<LifepathNodeDef>), typeof(List<SimGameStringList>) })]
    static class PilotGenerator_Ctor
    {
        static void Postfix(SimGameState sim, List<LifepathNodeDef> ___lifepaths, List<LifepathNodeDef> ___startingPaths,
            List<LifepathNodeDef> ___advanceStartingPaths, GenderedOptionsListDef ___voiceList)
        {
            ModState.PilotCreate.NameGenerator = new PilotNameGenerator();

            ModState.PilotCreate.LifePaths = ___lifepaths;
            ModState.PilotCreate.StartingPaths = ___startingPaths;
            ModState.PilotCreate.AdvancePaths = ___advanceStartingPaths;
            ModState.PilotCreate.Voices = ___voiceList;

            ModState.PilotCreate.GenderWeights = new List<int>()
            {
                sim.Constants.Pilot.FemaleGenerationWeight,
                sim.Constants.Pilot.MaleGenerationWeight,
                sim.Constants.Pilot.NonBinaryGenerationWeight
            };

            Mod.Log.Info?.Write("Initialized Pilot Creation shared state.");
        }
    }
}
