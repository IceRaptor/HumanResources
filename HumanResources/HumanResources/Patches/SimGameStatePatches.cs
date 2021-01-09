using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using HumanResources.Extensions;
using HumanResources.Helper;
using Localize;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SimGameState), "_OnInit")]
    static class SimGameState__OnInit
    {
        static void Postfix(SimGameState __instance, GameInstance game, SimGameDifficulty difficulty)
        {
            DataManager dm = UnityGameInstance.BattleTechGame.DataManager;
            LoadRequest loadRequest = dm.CreateLoadRequest();

            // Need to load each unique icon
            Mod.Log.Info?.Write("-- Loading HUD icons");

            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_Aerospace, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_MechTech, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_MedTech, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_Vehicle, null);

            loadRequest.ProcessRequests();
            Mod.Log.Info?.Write("--  Done!");

            // Reinitialize state
            ModState.Reset();
            ModState.SimGameState = __instance;
        }
    }

    // Initialize all the stats we need. This is invoked after both the load-from-save and new game paths.
    [HarmonyPatch(typeof(SimGameState), "InitCompanyStatValidators")]
    static class SimGameState_InitCompanyStatValidators
    {
        static void Postfix(SimGameState __instance)
        {
            // Initialize company stats if they aren't present
            Statistic aeroSkillStat = __instance.CompanyStats.GetStatistic(ModStats.Aerospace_Skill);
            if (aeroSkillStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.Aerospace_Skill, 0); }

            Statistic companyRepStat = __instance.CompanyStats.GetStatistic(ModStats.Company_Reputation);
            if (companyRepStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.Company_Reputation, 0); }

            Statistic crewCountAerospaceStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_Aerospace);
            if (crewCountAerospaceStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_Aerospace, 0); }

            Statistic crewCountMechWarriorsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechWarriors);
            if (crewCountMechWarriorsStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechWarriors, 0); }

            Statistic crewCountMechTechsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechTechs);
            if (crewCountMechTechsStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechTechs, 0); }

            Statistic crewCountMedTechsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MedTechs);
            if (crewCountMedTechsStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MedTechs, 0); }

            Statistic crewCountVehicleCrewsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_VehicleCrews);
            if (crewCountVehicleCrewsStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_VehicleCrews, 0); }
        }
    }

    // ==== PILOT CHANGES =====
    [HarmonyPatch(typeof(SimGameState), "AddPilotToRoster")]
    [HarmonyPatch(new Type[] { typeof(PilotDef), typeof(bool), typeof(bool) })]
    [HarmonyAfter(new string[] { "us.tbone.TisButAScratch" })]
    static class SimGameState_AddPilotToRoster
    {
        static void Postfix(SimGameState __instance, PilotDef def)
        {

            Mod.Log.Info?.Write($"Adding new pilot def: {def} to roster.");
            CrewDetails details = ModState.GetCrewDetails(def);

            // Add any mechtech, medtech, or aerospace points
            if (details.IsAerospaceCrew)
            {
                // Track our skill points
                Mod.Log.Info?.Write($"  -- crew is aerospace, adding: {details.Value} aerospace skill");
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                     ModStats.Aerospace_Skill,
                     StatCollection.StatOperation.Int_Add, details.Value);

                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_Aerospace,
                    StatCollection.StatOperation.Int_Add, 1);
            }
            else if (details.IsMechTechCrew)
            {
                Mod.Log.Info?.Write($"  -- crew is mechtech, adding: {details.Value} mechtech skill");
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MechTech_Skill,
                    StatCollection.StatOperation.Int_Add, details.Value);

                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_MechTechs,
                    StatCollection.StatOperation.Int_Add, 1);
            }
            else if (details.IsMechWarrior)
            {
                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_MechWarriors,
                    StatCollection.StatOperation.Int_Add, 1);
            }
            else if (details.IsMedTechCrew)
            {
                Mod.Log.Info?.Write($"  -- crew is medtech, adding: {details.Value} medtech skill");
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MedTech_Skill,
                    StatCollection.StatOperation.Int_Add, details.Value);

                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_MedTechs,
                    StatCollection.StatOperation.Int_Add, 1);
            }
            else if (details.IsVehicleCrew)
            {
                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_VehicleCrews,
                    StatCollection.StatOperation.Int_Add, 1);
            }

            if (ModState.IsHiringFlow)
            {
                // We are a real hire, not from initial spawn
                Mod.Log.Debug?.Write("  -- performing hiring checks.");

                // An actual hire versus start-of-game initialization. Revert the costs previously applied in HirePilot
                int purchaseCostAfterReputationModifier = __instance.CurSystem.GetPurchaseCostAfterReputationModifier(__instance.GetMechWarriorHiringCost(def));
                __instance.AddFunds(purchaseCostAfterReputationModifier, null, true, true);

                // Apply their hiring bonus
                Mod.Log.Info?.Write($"Hiring crew for bonus: {details.AdjustedBonus}");
                __instance.AddFunds(-details.AdjustedBonus, null, true, true);

                // Only refresh if we're actually in game
                __instance.RoomManager.RefreshTimeline(false);
                __instance.RoomManager.RefreshDisplay();
            }

            // DEBUG
            Mod.Log.Debug?.Write($"ITERATING TAGS ON DEF");
            foreach (string tag in def.PilotTags)
            {
                Mod.Log.Debug?.Write($" -- tag: {tag}");
            }

        }
    }

    [HarmonyPatch(typeof(SimGameState), "DismissPilot")]
    [HarmonyPatch(new Type[] { typeof(Pilot) })]
    static class SimGameState_DismissPilot
    {
        static void Prefix(SimGameState __instance, Pilot p)
        {
            if (p == null || !__instance.PilotRoster.Contains(p)) return;

            Mod.Log.Debug?.Write($"Removing pilot: {p.Name} from company.");

            CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

            // Remove any mechtech, medtech, or aerospace points
            if (details.IsAerospaceCrew)
            {
                // Track our skill points
                Statistic aerospaceSkill = __instance.CompanyStats.GetStatistic(ModStats.Aerospace_Skill);
                if (aerospaceSkill == null)
                    aerospaceSkill = __instance.CompanyStats.AddStatistic<int>(ModStats.Aerospace_Skill, 0);

                if (aerospaceSkill.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                            ModStats.Aerospace_Skill,
                            StatCollection.StatOperation.Int_Subtract, details.Value);

                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_Aerospace);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_Aerospace, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_Aerospace,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }
            else if (details.IsMechTechCrew)
            {
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MechTech_Skill,
                    StatCollection.StatOperation.Int_Subtract, details.Value);

                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechTechs);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechTechs, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MechTechs,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }
            else if (details.IsMechWarrior)
            {
                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechWarriors);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechWarriors, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MechWarriors,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }
            else if (details.IsMedTechCrew)
            {
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MedTech_Skill,
                    StatCollection.StatOperation.Int_Subtract, details.Value);

                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MedTechs);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MedTechs, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MedTechs,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }
            else if (details.IsVehicleCrew)
            {
                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_VehicleCrews);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_VehicleCrews, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_VehicleCrews,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }

            __instance.RoomManager.RefreshTimeline(false);
            __instance.RoomManager.RefreshDisplay();
        }
    }

    // Upgrade all the existing pilots w/ a contract time
    [HarmonyPatch(typeof(SimGameState), "OnCareerModeCharacterCreationComplete")]
    static class SimGameState_OnCareerModeCharacterCreationComplete
    {
        static void Postfix(SimGameState __instance)
        {
            foreach (Pilot pilot in __instance.PilotRoster)
            {
                if (pilot.pilotDef.IsFree && pilot.pilotDef.IsImmortal) continue; // player character, skip

                // Initialize new details for the pre-generated pilots
                CrewDetails details = new CrewDetails(pilot.pilotDef, CrewType.MechWarrior);
                ModState.UpdateOrCreateCrewDetails(pilot.pilotDef, details);
            }
        }
    }

    // ==== EVENTS =====

    [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
    static class SimGameState_OnDayPassed
    {
        static void Postfix(SimGameState __instance, int timeLapse, SimGameInterruptManager ___interruptQueue,
            SimGameEventTracker ___companyEventTracker, SimGameEventTracker ___mechWarriorEventTracker,
            SimGameEventTracker ___deadEventTracker, SimGameEventTracker ___moraleEventTracker)
        {
            Mod.Log.Debug?.Write($"OnDayPassed called with timeLapse: {timeLapse}");

            // Only check for events if another event isn't firing, and we're in orbit around a system
            if (!___interruptQueue.IsOpen && __instance.TravelState == SimGameTravelStatus.IN_SYSTEM)
            {
                foreach (Pilot pilot in __instance.PilotRoster)
                {
                    CrewDetails details = ModState.GetCrewDetails(pilot.pilotDef);
                    if (details.ExpirationDay <= __instance.DaysPassed)
                    {
                        Mod.Log.Debug?.Write($"CONTRACT FOR PILOT: {pilot.Name} HAS ELAPSED, FIRING EVENT");
                        ModState.ExpiredContracts.Enqueue((pilot, details));
                    }
                }

                if (ModState.ExpiredContracts.Count > 0)
                {
                    // Fire the first event, if there are more they will be fired from OnEventDismissed{
                    Mod.Log.Info?.Write($"Contract expiration event fired.");
                    (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();
                    SimGameEventDef newEvent = EventHelper.ModifyContractExpirationEventForPilot(expired.Pilot, expired.Details);
                    ModState.SimGameState.OnEventTriggered(newEvent, EventScope.MechWarrior, ___mechWarriorEventTracker);
                }
                else if (Mod.Config.HeadHunting.Enabled && __instance.TravelState == SimGameTravelStatus.IN_SYSTEM)
                {
                    // TODO: Only do check if not at planet with blacklisted tags
                    if (HeadHuntingHelper.ShouldCheckHeadHunting())
                    {
                        Pilot headHuntedCrew = HeadHuntingHelper.TestAllCrews();

                        if (headHuntedCrew != null)
                        {
                            CrewDetails cd = ModState.GetCrewDetails(headHuntedCrew.pilotDef);
                            SimGameEventDef newEvent = EventHelper.CreateHeadHuntingEvent(headHuntedCrew, cd, cd.HiringBonus, cd.HiringBonus);
                            ModState.HeadHuntedPilot = headHuntedCrew;
                            ModState.SimGameState.OnEventTriggered(newEvent, EventScope.MechWarrior, ___mechWarriorEventTracker);

                            HeadHuntingHelper.UpdateNextDayOnSuccess();
                        }
                        else
                        {
                            HeadHuntingHelper.UpdateNextDayOnFailure();
                        }
                    }
                }
            }
        }
    }

    // Select an expired pilot and populate the fields with their data
    [HarmonyPatch(typeof(SGEventPanel), "SetEvent")]
    static class SGEventPanel_SetEvent
    {
        // Populate the target mechwarrior before the event
        static void Prefix(SGEventPanel __instance, SimGameEventDef evt)
        {
            Mod.Log.Debug?.Write("SGEP:SetEvent:PRE");
            if (ModConsts.Event_ContractExpired.Equals(evt.Description.Id) && ModState.ExpiredContracts.Count > 0)
            {
                (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();
                Mod.Log.Trace?.Write($"SGEventPanel details setting targetMechwarrior: {expired.Pilot.Name}");
                ModState.SimGameState.Context.SetObject(GameContextObjectTagEnum.TargetMechWarrior, expired.Pilot);
            }
            else if (ModConsts.Event_HeadHunting.Equals(evt.Description.Id) && ModState.HeadHuntedPilot != null)
            {
                Mod.Log.Trace?.Write($"SGEventPanel details setting targetMechwarrior: {ModState.HeadHuntedPilot.Name}");
                ModState.SimGameState.Context.SetObject(GameContextObjectTagEnum.TargetMechWarrior, ModState.HeadHuntedPilot);
            }
        }
    }

    [HarmonyPatch(typeof(SGEventPanel), "OnOptionSelected")]
    static class SimGameState_OnOptionSelected
    {
        static void Postfix(SGEventPanel __instance, SimGameEventOption option,
            SimGameInterruptManager.EventPopupEntry ___thisEntry)
        {

            if (___thisEntry != null && ___thisEntry.parameters != null &&
                ___thisEntry.parameters[0] is SimGameEventDef eventDef)
            {

                if (ModConsts.Event_ContractExpired.Equals(eventDef.Description.Id))
                {
                    // Handle updating the contract length in the def; funds are handled by the event.
                    if (ModConsts.Event_Option_ContractExpired_Hire_NoBonus.Equals(option.Description.Id) ||
                        ModConsts.Event_Option_ContractExpired_Hire_Bonus.Equals(option.Description.Id))
                    {
                        (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();

                        Mod.Log.Debug?.Write($"Pilot {expired.Pilot.Name} was re-hired w/o a bonus");
                        expired.Details.ExpirationDay = ModState.SimGameState.DaysPassed + expired.Details.ContractTerm;
                        ModState.UpdateOrCreateCrewDetails(expired.Pilot.pilotDef, expired.Details);
                    }
                }
                else if (ModConsts.Event_HeadHunting.Equals(eventDef.Description.Id))
                {
                    // TODO: Anything?
                }

            }                
        }
    }

    // Check for any other events that need to be fired.
    [HarmonyPatch(typeof(SimGameState), "OnEventDismissed")]
    static class SimGameState_OnEventDismissed
    {
        static void Postfix(SimGameInterruptManager.EventPopupEntry entry, SimGameEventTracker ___mechWarriorEventTracker)
        {
            SimGameEventDef evt = entry != null && entry.parameters[0] is SimGameEventDef ? (SimGameEventDef)entry.parameters[0] : null;
            if (String.Equals(ModConsts.Event_ContractExpired, evt?.Description?.Id))
            {
                // First, dequeue to remove any existing pilot
                if (ModState.ExpiredContracts.Count > 0)
                    ModState.ExpiredContracts.Dequeue();

                // If there are any other events left after that, fire them here.
                if (ModState.ExpiredContracts.Count > 0)
                {
                    (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();
                    SimGameEventDef newEvent = EventHelper.ModifyContractExpirationEventForPilot(expired.Pilot, expired.Details);
                    Mod.Log.Info?.Write($"Contract expiration event fired.");
                    ModState.SimGameState.OnEventTriggered(newEvent, EventScope.MechWarrior, ___mechWarriorEventTracker);
                }
            }

            else if (String.Equals(ModConsts.Event_HeadHunting, evt?.Description?.Id))
            {
                ModState.HeadHuntedPilot = null;
            }

        }
    }

    // ===== COSTS =====
    [HarmonyPatch(typeof(SimGameState), "GetMechWarriorSalary")]
    static class SimGameState_GetMechWarriorSalary
    {
        static bool Prefix(SimGameState __instance, PilotDef def, ref string __result)
        {
            string salaryLabel = "------";
            if (!def.IsFree)
            {
                CrewDetails details = ModState.GetCrewDetails(def);
                string cbillString = SimGameState.GetCBillString(Mathf.RoundToInt(details.AdjustedSalary));
                salaryLabel = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Salary_Label],
                    new object[] { cbillString })
                    .ToString();
            }
            __result = salaryLabel;

            return false;
        }
    }

    [HarmonyPatch(typeof(SimGameState), "GetExpenditures")]
    [HarmonyPatch(new Type[] { typeof(EconomyScale), typeof(bool) })]
    [HarmonyAfter(new string[] { "de.morphyum.MechMaintenanceByCost", "us.frostraptor.IttyBittyLivingSpace" })]
    public static class SimGameState_GetExpenditures
    {
        public static void Postfix(SimGameState __instance, ref int __result, EconomyScale expenditureLevel, bool proRate)
        {
            Mod.Log.Trace?.Write($"SGS:GE entered with {__result}");

            // Evaluate old versus new cost
            int vanillaCosts = 0;
            int newCosts = 0;
            for (int i = 0; i < __instance.PilotRoster.Count; i++)
            {
                PilotDef def = __instance.PilotRoster[i].pilotDef;
                vanillaCosts += __instance.GetMechWarriorValue(def);

                CrewDetails details = ModState.GetCrewDetails(def);
                newCosts += details.AdjustedSalary;
            }

            // Multiply old costs by expenditure. New has that built in. Then subtract them from the running total
            float expenditureCostModifier = __instance.GetExpenditureCostModifier(expenditureLevel);
            int vanillaTotal = Mathf.CeilToInt((float)vanillaCosts * expenditureCostModifier);
            Mod.Log.Trace?.Write($"Removing {vanillaCosts} costs x {expenditureCostModifier} expenditureMulti " +
                $"= {vanillaTotal} total vanilla costs.");
            __result -= vanillaTotal;

            // Add the new costs
            __result += newCosts;
        }
    }

    [HarmonyPatch(typeof(SimGameState), "GetMechWarriorValue")]
    static class SimGameState_GetMechWarriorValue
    {
        static bool Prefix(SimGameState __instance, PilotDef def, ref int __result)
        {
            CrewDetails details = ModState.GetCrewDetails(def);
            __result = details != null ? details.Salary : 0;

            return false;
        }
    }

    [HarmonyPatch(typeof(SimGameState), "DeductQuarterlyFunds")]
    static class SimGameState_DeductQuarterlyFunds
    {
        static void Postfix(SimGameState __instance)
        {

            List<FactionValue> alliedFactions = new List<FactionValue>();
            foreach (string factionName in __instance.GetAllCareerAllies())
            {
                alliedFactions.Add(FactionEnumeration.GetFactionByName(factionName));
            }

            foreach (Pilot pilot in __instance.PilotRoster)
            {
                CrewDetails details = ModState.GetCrewDetails(pilot.pilotDef);

                // Decay positive attitude
                if (details.Attitude > 0)
                {
                    int decayAmount = (int)Math.Ceiling(details.Attitude * Mod.Config.Attitude.Monthly.Decay);
                    Mod.Log.Debug?.Write($"Decaying positive attitude from {details.Attitude} by {decayAmount}");
                    details.Attitude -= decayAmount;
                }

                // Apply the morale modifier 
                int econMod = Mod.Config.Attitude.Monthly.EconomyNormalMod;
                if (__instance.ExpenditureLevel == EconomyScale.Spartan) econMod = Mod.Config.Attitude.Monthly.EconSpartanMod;
                else if (__instance.ExpenditureLevel == EconomyScale.Restrictive) econMod = Mod.Config.Attitude.Monthly.EconRestrictiveMod;
                else if (__instance.ExpenditureLevel == EconomyScale.Generous) econMod = Mod.Config.Attitude.Monthly.EconomyGenerousMod;
                else if (__instance.ExpenditureLevel == EconomyScale.Extravagant) econMod = Mod.Config.Attitude.Monthly.EconomyExtravagantMod;
                details.Attitude += econMod;

                // Check pilots to see if they favor or hate an allied faction
                if (details.FavoredFaction > 0)
                {
                    foreach (FactionValue faction in alliedFactions)
                    {
                        if (faction.ID == details.FavoredFaction)
                        {
                            Mod.Log.Debug?.Write($"Pilot {pilot.Name} favors allied faction: {faction.FriendlyName}, " +
                                $"applying attitude mod: {Mod.Config.Attitude.Monthly.FavoredEmployerAlliedMod}");
                            details.Attitude += Mod.Config.Attitude.Monthly.FavoredEmployerAlliedMod;
                            ModState.UpdateOrCreateCrewDetails(pilot.pilotDef, details);
                        }
                    }
                }

                if (details.HatedFaction > 0)
                {
                    foreach (FactionValue faction in alliedFactions)
                    {
                        if (faction.ID == details.HatedFaction)
                        {
                            Mod.Log.Debug?.Write($"Pilot {pilot.Name} hates allied faction: {faction.FriendlyName}, " +
                                $"applying attitude mod: {Mod.Config.Attitude.Monthly.HatedEmployerAlliedMod}");
                            details.Attitude += Mod.Config.Attitude.Monthly.HatedEmployerAlliedMod;
                            ModState.UpdateOrCreateCrewDetails(pilot.pilotDef, details);
                        }
                    }
                }

                // Clamp values
                details.Attitude = Mathf.Clamp(details.Attitude, Mod.Config.Attitude.ThresholdMin, Mod.Config.Attitude.ThresholdMax);

                ModState.UpdateOrCreateCrewDetails(pilot.pilotDef, details);

            }
        }
    }



    [HarmonyPatch(typeof(SimGameState), "CanMechWarriorBeHiredAccordingToMRBRating")]
    static class SimGameState_CanMechWarriorBeHiredAccordingToMRBRating
    {
        static void Postfix(SimGameState __instance, Pilot pilot, ref bool __result)
        {
            int currentMRBLevel = __instance.GetCurrentMRBLevel();
            CrewDetails details = ModState.GetCrewDetails(pilot.pilotDef);
            __result = details.CanBeHiredAtMRBLevel(currentMRBLevel);
        }
    }

    [HarmonyPatch(typeof(SimGameState), "CanMechWarriorBeHiredAccordingToMorale")]
    static class SimGameState_CanMechWarriorBeHiredAccordingToMorale
    {
        static void Postfix(SimGameState __instance, Pilot pilot, ref bool __result)
        {
            // TODO:
            // Check morale settings here; 
            //   should elites require more morale?
            //   should high level mechwarriors require a higher morale?
            //   should nobles require a high morale rating?

            __result = true;
        }
    }

    

}
