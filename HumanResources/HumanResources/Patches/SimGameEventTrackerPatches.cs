using BattleTech;
using BattleTech.Data;
using Harmony;
using HBS.Collections;
using HumanResources.Extensions;
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
			//___sim.PilotRoster.GetNext(true);
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
			return allPilots.Where((p) => {
				CrewDetails cd = ModState.GetCrewDetails(p.pilotDef);
				return cd.IsMechWarrior;
			}).ToList();
		}

		static bool Prefix(SimGameEventTracker __instance, EventScope scope, EventDef_MDD evt, List<TagSet> reqList, 
            out SimGameEventTracker.PotentialEvent goodEvent, ref bool __result,
			SimGameState ___sim, SimGameReport.ReportEntry ___VerboseEntry)
        {
			goodEvent = null;

			Mod.Log.Debug?.Write($"Validating event with defId: {evt?.EventDefID}");
			
			// Skip any HR events and use the base validation
			if (evt != null &&
				(String.Equals(ModConsts.Event_ContractExpired, evt.EventDefID, StringComparison.InvariantCulture) ||
				String.Equals(ModConsts.Event_HeadHunting, evt.EventDefID, StringComparison.InvariantCulture))
				)
				return true;

			Traverse pushRecordT = Traverse.Create(__instance).Method("PushRecord", new Type[] { typeof(string) });
			Traverse popRecordT = Traverse.Create(__instance).Method("PopRecord");

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
				num = ___sim.Graveyard.Count;
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
						pushRecordT.GetValue(new object[] { string.Format("Event For {0}", pilot.Callsign) });
						goto IL_C7;
					}
				}
				else
				{
					if (scope == EventScope.DeadMechWarrior)
					{
						pilot = ___sim.Graveyard.GetNext(true);
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
	

					if (!___sim.DataManager.SimGameEventDefs.Exists(evt.EventDefID))
					{
						Mod.Log.Warn?.Write($"Event {evt.EventDefID} cannot be found in manifest. Skipping.");
					}
					else
					{
						SimGameEventDef simGameEventDef = ___sim.DataManager.SimGameEventDefs.Get(evt.EventDefID);
						if (scope == EventScope.MechWarrior)
						{
							___sim.Context.SetObject(GameContextObjectTagEnum.TargetMechWarrior, pilot);
						}
						StatCollection statsByScope = ___sim.GetStatsByScope(scope);
						pushRecordT.GetValue(new object[] { evt.EventDefID });
						if (SimGameState.MeetsRequirements(simGameEventDef.Requirements, curTags, statsByScope, ___VerboseEntry))
						{
							__instance.RecordLog("Passed!", SimGameLogLevel.VERBOSE);
							popRecordT.GetValue();
							__instance.RecordLog("Testing for additional requirements", SimGameLogLevel.VERBOSE);
							pushRecordT.GetValue(new object[] { simGameEventDef.Description.Id });
							bool flag = true;
							if (simGameEventDef.AdditionalRequirements != null)
							{
								__instance.RecordLog("Has additional reqs.", SimGameLogLevel.VERBOSE);
								foreach (RequirementDef requirementDef in simGameEventDef.AdditionalRequirements)
								{
									TagSet tagsByScope = ___sim.GetTagsByScope(requirementDef.Scope);
									StatCollection statsByScope2 = ___sim.GetStatsByScope(requirementDef.Scope);
									if (!SimGameState.MeetsRequirements(requirementDef, tagsByScope, statsByScope2, ___VerboseEntry))
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
								popRecordT.GetValue();
							}
							else
							{
								popRecordT.GetValue();
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
										pushRecordT.GetValue(new object[] { "Additional Object" });
										__instance.RecordLog("Object Scope: " + simGameEventObject.Scope, SimGameLogLevel.VERBOSE);
										switch (simGameEventObject.Scope)
										{
											case EventScope.SecondaryMechWarrior:
											case EventScope.TertiaryMechWarrior:
												goto IL_3C7;
											case EventScope.SecondaryMech:
												list.AddRange(___sim.ActiveMechs.Values);
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
											if (SimGameState.MeetsRequirements(simGameEventObject.Requirements, list3[l], list4[l], ___VerboseEntry))
											{
												num2 = l;
												break;
											}
										}
										if (num2 < 0)
										{
											__instance.RecordLog("Unable to find additional object that meets requirement. Skipping event.", SimGameLogLevel.CRITICAL);
											popRecordT.GetValue();
											return false;
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
										popRecordT.GetValue();
										k++;
										continue;
									IL_3C7:
										foreach (Pilot pilot2 in AllMechWarriors())
										{
											if (pilot2.pilotDef.TimeoutRemaining > 0)
											{
												popRecordT.GetValue();
											}
											else if (pilot != null && pilot2.GUID == pilot.GUID)
											{
												popRecordT.GetValue();
											}
											else if (simGameEventObject.Scope == EventScope.SecondaryMechWarrior && goodEvent.TertiaryPilot != null && pilot2.GUID == goodEvent.TertiaryPilot.GUID)
											{
												popRecordT.GetValue();
											}
											else if (simGameEventObject.Scope == EventScope.TertiaryMechWarrior && goodEvent.SecondaryPilot != null && pilot2.GUID == goodEvent.SecondaryPilot.GUID)
											{
												popRecordT.GetValue();
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
										popRecordT.GetValue();
									}
									__result = true;
									return false;
								}
							}
						}
						else
						{
							__instance.RecordLog("Failed!", SimGameLogLevel.VERBOSE);
							popRecordT.GetValue();
							popRecordT.GetValue();
						}
					}
				}
				if (scope == EventScope.MechWarrior)
				{
					popRecordT.GetValue();
					goto IL_65E;
				}
				goto IL_65E;
			}

			__result = false;
			return false;
        }
    }
}
