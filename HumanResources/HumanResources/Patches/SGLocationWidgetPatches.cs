using BattleTech;
using BattleTech.UI;
using Harmony;
using HumanResources.Extensions;
using HumanResources.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGLocationWidget), "ReceiveButtonPress")]
    static class SGLocationWidget_ReceiveButtonPress
    {
        static bool Prepare() => Mod.Config.DebugCommands; 

        static bool Prefix(string button)
        {
            Mod.Log.Info?.Write($"RBP invoked for button: {button}");
            if ("Hiring".Equals(button, StringComparison.InvariantCultureIgnoreCase))
            {
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    Mod.Log.Info?.Write("-- Regenerating pilots in system.");
                    ModState.SimGameState.CurSystem.AvailablePilots.Clear();
                    ModState.SimGameState.CurSystem.GeneratePilots(ModState.SimGameState.Constants.Story.DefaultPilotsPerSystem);
                }
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) &&
                    Mod.Config.HeadHunting.Enabled && ModState.SimGameState.TravelState == SimGameTravelStatus.IN_SYSTEM)
                {
                    Mod.Log.Info?.Write("-- Forcing poaching on random crew.");

                    // Randomize pilots instead of sorting by skill?
                    List<Pilot> pilots = ModState.SimGameState.PilotRoster.ToList();
                    pilots.Sort((p1, p2) => {
                        CrewDetails p1cd = ModState.GetCrewDetails(p1.pilotDef);
                        CrewDetails p2cd = ModState.GetCrewDetails(p2.pilotDef);
                        return CrewDetails.CompareByValue(p1cd, p2cd);
                    });

                    int idx = Mod.Random.Next(0, pilots.Count - 1);
                    Pilot random = pilots[idx];
                    Mod.Log.Info?.Write($"--  Poached pilot: {random.Name}");
                    CrewDetails cd = ModState.GetCrewDetails(random.pilotDef);
                    SimGameEventDef newEvent = EventHelper.CreateHeadHuntingEvent(random, cd, cd.HiringBonus, cd.HiringBonus);

                    Traverse mechWarriorEventTrackerT = Traverse.Create(ModState.SimGameState).Field("mechWarriorEventTracker");
                    SimGameEventTracker mechWarriorEventTracker = mechWarriorEventTrackerT.GetValue<SimGameEventTracker>();
                    ModState.SimGameState.OnEventTriggered(newEvent, EventScope.MechWarrior, mechWarriorEventTracker);

                    return false;
                }

            }

            return true;
        }
    }
}
