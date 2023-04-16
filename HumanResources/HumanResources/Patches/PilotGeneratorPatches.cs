using System;
using System.Collections.Generic;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(PilotGenerator), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(SimGameState), typeof(List<LifepathNodeDef>), typeof(List<SimGameStringList>) })]
    static class PilotGenerator_Ctor
    {
        static void Postfix(SimGameState sim, PilotGenerator __instance)
        {
            ModState.CrewCreateState.NameGenerator = new PilotNameGenerator();

            ModState.CrewCreateState.LifePaths = __instance.lifepaths;
            ModState.CrewCreateState.StartingPaths = __instance.startingPaths;
            ModState.CrewCreateState.AdvancePaths = __instance.advanceStartingPaths;
            ModState.CrewCreateState.Voices = __instance.voiceList;

            ModState.CrewCreateState.GenderWeights = new List<int>()
            {
                sim.Constants.Pilot.FemaleGenerationWeight,
                sim.Constants.Pilot.MaleGenerationWeight,
                sim.Constants.Pilot.NonBinaryGenerationWeight
            };

            ModState.CrewCreateState.HBSPilotGenerator = __instance;

            Mod.Log.Info?.Write("Initialized Pilot Creation shared state.");
        }
    }
}
