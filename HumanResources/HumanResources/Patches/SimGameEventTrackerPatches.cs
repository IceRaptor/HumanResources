using BattleTech.Data;
using HBS.Collections;
using HumanResources.Crew;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HumanResources.Patches
{

    [HarmonyPatch(typeof(SimGameEventTracker), "IsEventValid")]
    static class SimGameEventTracker_IsEventValid
    {

        // This is stupidly slow, but because the way the weighted works it's not easy to get a list of the 
        //   remaining elements. So we (unfortunately) iterate through the pilots until we find a mechwarrior.
        static Pilot NextSGSMechwarrior()
        {
            Pilot mechWarrior = null;
            while (mechWarrior == null)
            {
                Pilot pilot = ModState.SimGameState.PilotRoster.GetNext(true);
                CrewDetails cd = ModState.GetCrewDetails(pilot.pilotDef);
                if (cd.IsMechWarrior)
                {
                    mechWarrior = pilot;
                }
            }
            return mechWarrior;
        }

        static List<Pilot> AllMechWarriors()
        {
            List<Pilot> allPilots = ModState.SimGameState.PilotRoster.ToList();
            return allPilots.Where((p) =>
            {
                CrewDetails cd = ModState.GetCrewDetails(p.pilotDef);
                return cd.IsMechWarrior;
            }).ToList();
        }

        static void Prefix(ref bool __runOriginal, SimGameEventTracker __instance, EventScope scope, EventDef_MDD evt, List<TagSet> reqList,
            out SimGameEventTracker.PotentialEvent goodEvent, ref bool __result)
        {
            goodEvent = null;

            if (!__runOriginal) return;

            Mod.Log.Debug?.Write($"Validating event with defId: {evt?.EventDefID}");

            // Skip any HR events and use the base validation
            if (evt != null &&
                (String.Equals(ModConsts.Event_ContractExpired, evt.EventDefID, StringComparison.InvariantCulture) ||
                String.Equals(ModConsts.Event_HeadHunting, evt.EventDefID, StringComparison.InvariantCulture))
                )
                return;

            int num = 0;
            if (scope == EventScope.Company)
            {
                num = 1;
            }
            else if (scope == EventScope.MechWarrior)
            {
                num = AllMechWarriors().Count();
            }
            else if (scope == EventScope.DeadMechWarrior)
            {
                num = __instance.sim.Graveyard.Count;
            }

            int i = 0;
            while (i < num)
            {
                Pilot pilot = null;
                if (scope == EventScope.MechWarrior)
                {
                    pilot = NextSGSMechwarrior();
                    if (pilot.pilotDef.TimeoutRemaining <= 0)
                    {
                        reqList.Clear();
                        reqList.Add(pilot.pilotDef.PilotTags);
                        __instance.PushRecord(string.Format("Event For {0}", pilot.Callsign));
                        goto IL_C7;
                    }
                }
                else
                {
                    if (scope == EventScope.DeadMechWarrior)
                    {
                        pilot = __instance.sim.Graveyard.GetNext(true);
                        reqList.Clear();
                        reqList.Add(pilot.pilotDef.PilotTags);
                        goto IL_C7;
                    }
                    goto IL_C7;
                }
            IL_65E:
                i++;
                continue;
            IL_C7:
                for (int j = 0; j < reqList.Count; j++)
                {
                    TagSet curTags = reqList[j];
                    __instance.RecordLog("Attempting Event: " + evt.EventDefID, SimGameLogLevel.VERBOSE);


                    if (!__instance.sim.DataManager.SimGameEventDefs.Exists(evt.EventDefID))
                    {
                        Mod.Log.Warn?.Write($"Event {evt.EventDefID} cannot be found in manifest. Skipping.");
                    }
                    else
                    {
                        SimGameEventDef simGameEventDef = __instance.sim.DataManager.SimGameEventDefs.Get(evt.EventDefID);
                        if (scope == EventScope.MechWarrior)
                        {
                            __instance.sim.Context.SetObject(GameContextObjectTagEnum.TargetMechWarrior, pilot);
                        }
                        StatCollection statsByScope = __instance.sim.GetStatsByScope(scope);
                       __instance.PushRecord(evt.EventDefID);
                        if (SimGameState.MeetsRequirements(simGameEventDef.Requirements, curTags, statsByScope, __instance.VerboseEntry))
                        {
                            __instance.RecordLog("Passed!", SimGameLogLevel.VERBOSE);
                            __instance.PopRecord();
                            __instance.RecordLog("Testing for additional requirements", SimGameLogLevel.VERBOSE);
                           __instance.PushRecord(simGameEventDef.Description.Id);
                            bool flag = true;
                            if (simGameEventDef.AdditionalRequirements != null)
                            {
                                __instance.RecordLog("Has additional reqs.", SimGameLogLevel.VERBOSE);
                                foreach (RequirementDef requirementDef in simGameEventDef.AdditionalRequirements)
                                {
                                    TagSet tagsByScope = __instance.sim.GetTagsByScope(requirementDef.Scope);
                                    StatCollection statsByScope2 = __instance.sim.GetStatsByScope(requirementDef.Scope);
                                    if (!SimGameState.MeetsRequirements(requirementDef, tagsByScope, statsByScope2, __instance.VerboseEntry))
                                    {
                                        flag = false;
                                        break;
                                    }
                                    if (!flag)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (!flag)
                            {
                                __instance.RecordLog("Failed additional req check.", SimGameLogLevel.VERBOSE);
                                __instance.PopRecord();
                            }
                            else
                            {
                                __instance.PopRecord();
                                __instance.RecordLog(string.Format("Selected final event: {0}. Attempting to fetch additional objects.", simGameEventDef.Description.Id), SimGameLogLevel.CRITICAL);
                                if (simGameEventDef.AdditionalObjects != null && simGameEventDef.AdditionalObjects.Length != 0)
                                {
                                    __instance.RecordLog("Event has additional objects.", SimGameLogLevel.VERBOSE);
                                }
                                goodEvent = new SimGameEventTracker.PotentialEvent(simGameEventDef, pilot);
                                if (simGameEventDef.AdditionalObjects != null)
                                {
                                    SimGameEventObject[] additionalObjects = simGameEventDef.AdditionalObjects;
                                    int k = 0;
                                    while (k < additionalObjects.Length)
                                    {
                                        SimGameEventObject simGameEventObject = additionalObjects[k];
                                        List<MechDef> list = new List<MechDef>();
                                        List<Pilot> list2 = new List<Pilot>();
                                        List<TagSet> list3 = new List<TagSet>();
                                        List<StatCollection> list4 = new List<StatCollection>();
                                       __instance.PushRecord("Additional Object");
                                        __instance.RecordLog("Object Scope: " + simGameEventObject.Scope, SimGameLogLevel.VERBOSE);
                                        switch (simGameEventObject.Scope)
                                        {
                                            case EventScope.SecondaryMechWarrior:
                                            case EventScope.TertiaryMechWarrior:
                                                goto IL_3C7;
                                            case EventScope.SecondaryMech:
                                                list.AddRange(__instance.sim.ActiveMechs.Values);
                                                list.Shuffle<MechDef>();
                                                using (List<MechDef>.Enumerator enumerator = list.GetEnumerator())
                                                {
                                                    while (enumerator.MoveNext())
                                                    {
                                                        MechDef mechDef = enumerator.Current;
                                                        list3.Add(mechDef.MechTags);
                                                        list4.Add(mechDef.Stats);
                                                    }
                                                    break;
                                                }
                                                goto IL_3C7;
                                        }
                                    IL_507:
                                        int num2 = -1;
                                        for (int l = 0; l < list3.Count; l++)
                                        {
                                            if (SimGameState.MeetsRequirements(simGameEventObject.Requirements, list3[l], list4[l], __instance.VerboseEntry))
                                            {
                                                num2 = l;
                                                break;
                                            }
                                        }
                                        if (num2 < 0)
                                        {
                                            __instance.RecordLog("Unable to find additional object that meets requirement. Skipping event.", SimGameLogLevel.CRITICAL);
                                            __instance.PopRecord();
                                            __runOriginal = false;
                                            return;
                                        }
                                        if (simGameEventObject.Scope == EventScope.SecondaryMech)
                                        {
                                            goodEvent.SecondaryMech = list[num2];
                                            __instance.RecordLog(string.Format("Mech {0} has been set as additional object", list[num2].Description.Id), SimGameLogLevel.CRITICAL);
                                        }
                                        else if (simGameEventObject.Scope == EventScope.SecondaryMechWarrior)
                                        {
                                            goodEvent.SecondaryPilot = list2[num2];
                                            __instance.RecordLog(string.Format("Pilot {0} has been set as additional object", list2[num2].Description.Id), SimGameLogLevel.CRITICAL);
                                        }
                                        else
                                        {
                                            goodEvent.TertiaryPilot = list2[num2];
                                            __instance.RecordLog(string.Format("Pilot {0} has been set as additional object", list2[num2].Description.Id), SimGameLogLevel.CRITICAL);
                                        }
                                        __instance.PopRecord();
                                        k++;
                                        continue;
                                    IL_3C7:
                                        foreach (Pilot pilot2 in AllMechWarriors())
                                        {
                                            if (pilot2.pilotDef.TimeoutRemaining > 0)
                                            {
                                                __instance.PopRecord();
                                            }
                                            else if (pilot != null && pilot2.GUID == pilot.GUID)
                                            {
                                                __instance.PopRecord();
                                            }
                                            else if (simGameEventObject.Scope == EventScope.SecondaryMechWarrior && goodEvent.TertiaryPilot != null && pilot2.GUID == goodEvent.TertiaryPilot.GUID)
                                            {
                                                __instance.PopRecord();
                                            }
                                            else if (simGameEventObject.Scope == EventScope.TertiaryMechWarrior && goodEvent.SecondaryPilot != null && pilot2.GUID == goodEvent.SecondaryPilot.GUID)
                                            {
                                                __instance.PopRecord();
                                            }
                                            else
                                            {
                                                list2.Add(pilot2);
                                            }
                                        }
                                        list2.Shuffle<Pilot>();
                                        foreach (Pilot pilot3 in list2)
                                        {
                                            list3.Add(pilot3.pilotDef.PilotTags);
                                            list4.Add(pilot3.StatCollection);
                                        }
                                        goto IL_507;
                                    }
                                }
                                if (goodEvent != null)
                                {
                                    if (scope == EventScope.MechWarrior)
                                    {
                                        __instance.PopRecord();
                                    }
                                    __result = true;
                                    __runOriginal = false;
                                }
                            }
                        }
                        else
                        {
                            __instance.RecordLog("Failed!", SimGameLogLevel.VERBOSE);
                            __instance.PopRecord();
                            __instance.PopRecord();
                        }
                    }
                }
                if (scope == EventScope.MechWarrior)
                {
                    __instance.PopRecord();
                    goto IL_65E;
                }
                goto IL_65E;
            }

            __result = false;
            __runOriginal = false;
            return;
        }
    }
}
