using BattleTech;
using BattleTech.UI;
using Harmony;
using SVGImporter;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HumanResources.Patches
{

    [HarmonyPatch(typeof(SGBarracksRosterList), "AddPilot")]
    static class SGBarracksRosterList_AddPilot
    {
        static bool Prefix(SGBarracksRosterList __instance, 
            Pilot pilot, UnityAction<SGBarracksRosterSlot> pilotSelectedOnClick, bool isInHireHall,
            Dictionary<string, SGBarracksRosterSlot> ___currentRoster)
        {
            Mod.Log.Debug?.Write($"Adding pilot {pilot.Callsign} to roster list.");

            if (!___currentRoster.ContainsKey(pilot.GUID))
            {
                SGBarracksRosterSlot slot = __instance.GetSlot();
                if (isInHireHall)
                {
                    slot.InitNoDrag(pilot, ModState.SimGameState, pilotSelectedOnClick, showTheCost: true);
                    slot.SetDraggable(isDraggable: false);
                }
                else
                {
                    slot.Init(pilot, __instance, pilotSelectedOnClick);
                }
                ___currentRoster.Add(pilot.GUID, slot);
                slot.AddToRadioSet(__instance.listRadioSet);
                //ForceRefreshImmediate();
            }

            return false;
        }

    }

    [HarmonyPatch(typeof(SGBarracksRosterList), "ApplySort")]
    static class SGBarracksRosterList_ApplySort
    {
        static bool Prefix(SGBarracksRosterList __instance, List<SGBarracksRosterSlot> inventory)
        {
            // TODO: Apply a logical sort here
            return true;
        }

    }

    [HarmonyPatch(typeof(SGBarracksRosterList), "SetRosterBerthText")]
    static class SGBarracksRosterList_SetRosterBerthText
    {
        static bool Prefix(SGBarracksRosterList __instance)
        {
            Mod.Log.Debug?.Write("Parsing berths consumed by current pilots.");

            int berthsUsed = 0;
            foreach (Pilot pilot in __instance.Sim.PilotRoster)
            {
                bool foundSizeTag = false;
                foreach (string tag in pilot.pilotDef.PilotTags)
                {
                    if (tag.StartsWith(ModTags.Tag_CrewSize_Prefix))
                    {
                        foundSizeTag = true;

                        string crewSizeS = tag.Substring(ModTags.Tag_CrewSize_Prefix.Length);
                        try 
                        {
                            int crewSize = Int32.Parse(crewSizeS);
                            Mod.Log.Debug?.Write($"Crew '{pilot.Callsign}' consumes {crewSize} berths.");
                            berthsUsed += crewSize;
                            
                            
                        }
                        catch (Exception e)
                        {
                            Mod.Log.Info?.Write(e, $"Failed to parse crew size tag: {tag}, defaulting to 1 berth.");
                            berthsUsed++;
                        }
                        break;
                    }
                }

                if (!foundSizeTag) berthsUsed++;
            }

            return false;
        }
    }
}
