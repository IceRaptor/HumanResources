﻿using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using HumanResources.Comparable;
using HumanResources.Crew;
using HumanResources.Helper;
using System.Collections.Generic;
using UnityEngine.Events;

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

                    // Update the pilot's contract end date
                    CrewDetails details = ModState.GetCrewDetails(pilot.pilotDef);
                    details.ExpirationDay = ModState.SimGameState.DaysPassed + details.ContractTerm;
                    Mod.Log.Debug?.Write($"  - pilot's contract ends on day: {details.ExpirationDay}");
                    ModState.UpdateOrCreateCrewDetails(pilot.pilotDef, details);
                }
                else
                {
                    slot.Init(pilot, __instance, pilotSelectedOnClick);
                }
                ___currentRoster.Add(pilot.GUID, slot);
                slot.AddToRadioSet(__instance.listRadioSet);

                // Performance tweak; skip
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
            Mod.Log.Info?.Write($"Sorting {inventory?.Count} pilot slots");

            List<SGBarracksRosterSlot> sortedSlots = new List<SGBarracksRosterSlot>(inventory);
            sortedSlots.Sort(SGBarracksRosterSlotComparisons.CompareByCrewTypeAndValue);

            int index = 0;
            foreach (SGBarracksRosterSlot slot in sortedSlots)
            {
                Mod.Log.Debug?.Write($" -- pilot: {slot.Pilot.Name} has index: {index}");
                slot.gameObject.transform.SetSiblingIndex(index);
                index++;
            }

            __instance.ForceRefreshImmediate();

            return false;
        }

    }

    [HarmonyPatch(typeof(SGBarracksRosterList), "SetRosterBerthText")]
    static class SGBarracksRosterList_SetRosterBerthText
    {
        static bool Prefix(SGBarracksRosterList __instance, LocalizableText ___mechWarriorCount)
        {
            
            int usedBerths = CrewHelper.UsedBerths(ModState.SimGameState.PilotRoster);
            Mod.Log.Debug?.Write($"Berths => used: {usedBerths}  available: {ModState.SimGameState.GetMaxMechWarriors()}");

            string text = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Berths_Used],
                new object[] { usedBerths, ModState.SimGameState.GetMaxMechWarriors() }).ToString(); 
            ___mechWarriorCount.SetText(text);

            return false;
        }
    }
}
