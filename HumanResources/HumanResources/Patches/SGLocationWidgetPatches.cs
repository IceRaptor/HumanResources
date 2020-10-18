using BattleTech.UI;
using Harmony;
using System;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGLocationWidget), "ReceiveButtonPress")]
    static class SGLocationWidget_ReceiveButtonPress
    {
        static bool Prepare() => Mod.Config.DebugCommands; 

        static void Prefix(string button)
        {
            Mod.Log.Info?.Write($"RBP invoked for button: {button}");
            if ("Hiring".Equals(button, StringComparison.InvariantCultureIgnoreCase) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
            {
                Mod.Log.Info?.Write("-- Regenerating pilots in system.");
                ModState.SimGameState.CurSystem.AvailablePilots.Clear();
                ModState.SimGameState.CurSystem.GeneratePilots(ModState.SimGameState.Constants.Story.DefaultPilotsPerSystem);
            }
        }
    }
}
