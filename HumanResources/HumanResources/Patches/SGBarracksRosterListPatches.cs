using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
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
        static bool Prefix(SGBarracksRosterList __instance, LocalizableText ___mechWarriorCount)
        {
            
            int usedBerths = PilotHelper.UsedBerths(ModState.SimGameState.PilotRoster);
            Mod.Log.Debug?.Write($"Berths => used: {usedBerths}  available: {ModState.SimGameState.GetMaxMechWarriors()}");

            string text = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Berths_Used],
                new object[] { usedBerths, ModState.SimGameState.GetMaxMechWarriors() }).ToString(); 
            ___mechWarriorCount.SetText(text);

            return false;
        }
    }
}
