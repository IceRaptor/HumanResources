using BattleTech;
using HumanResources.Crew;
using Localize;
using System;
using System.Text;

namespace HumanResources.Helper
{
    public static class EventHelper
    {
        public static SimGameEventDef ModifyContractExpirationEventForPilot(Pilot pilot, CrewDetails details)
        {
            SimGameEventDef rawEventDef = ModState.SimGameState.DataManager.SimGameEventDefs.Get(ModConsts.Event_ContractExpired);

            // Change the description fields
            BaseDescriptionDef rawBaseDescDef = rawEventDef.Description;
            StringBuilder detailsSB = new StringBuilder(rawBaseDescDef.Details);
            detailsSB.Append("\n");
            detailsSB.Append("<margin=5em>\n");
            // TODO: Localize
            detailsSB.Append($" Hiring Bonus: {SimGameState.GetCBillString(details.AdjustedBonus)}\n\n");
            detailsSB.Append($" Monthly Salary: {SimGameState.GetCBillString(details.AdjustedSalary)}\n\n");
            detailsSB.Append("</margin>\n");
            BaseDescriptionDef newBaseDescDef = new BaseDescriptionDef(rawBaseDescDef.Id, rawBaseDescDef.Name, detailsSB.ToString(), rawBaseDescDef.Icon);

            // Change the options to have the correct pay values
            SimGameEventOption[] newOptions = rawEventDef.Options;
            foreach (SimGameEventOption sgeOption in newOptions)
            {
                if (ModConsts.Event_Option_ContractExpired_Hire_Bonus.Equals(sgeOption.Description.Id))
                {
                    (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();
                    UpdateFundsStat(pilot, expired.Details.AdjustedBonus, sgeOption);
                }
            }

            SimGameEventDef expiredEventDef = new SimGameEventDef(
                rawEventDef.PublishState, rawEventDef.EventType, rawEventDef.Scope,
                newBaseDescDef, rawEventDef.Requirements, rawEventDef.AdditionalRequirements,
                rawEventDef.AdditionalObjects, newOptions, 
                rawEventDef.Weight, rawEventDef.OneTimeEvent, rawEventDef.Tags);
            return expiredEventDef;
        }

        public static SimGameEventDef CreateHeadHuntingEvent(Pilot pilot, CrewDetails details, float retentionCost, float buyoutPayment)
        {
            SimGameEventDef rawEventDef = ModState.SimGameState.DataManager.SimGameEventDefs.Get(ModConsts.Event_HeadHunting);

            int counterOffer = SalaryHelper.CalcCounterOffer(details) * -1;
            int buyout = details.AdjustedBonus;
            Mod.Log.Info?.Write($"For headhunting event, counterOffer: {counterOffer}  buyout: {buyout}");
            
            // Change the description fields
            BaseDescriptionDef rawBaseDescDef = rawEventDef.Description;
            StringBuilder detailsSB = new StringBuilder(rawBaseDescDef.Details);
            detailsSB.Append("\n\n");
            detailsSB.Append("<margin=5em>\n");
            detailsSB.Append(
                new Text(Mod.LocalizedText.Events[ModText.ET_HeadHunted_Retention], 
                new object[] { SimGameState.GetCBillString(counterOffer) }).ToString()
                );
            detailsSB.Append(
                new Text(Mod.LocalizedText.Events[ModText.ET_HeadHunted_Buyout],
                new object[] { SimGameState.GetCBillString(buyout) }).ToString()
                );
            detailsSB.Append("</margin>\n");
            BaseDescriptionDef newBaseDescDef = new BaseDescriptionDef(rawBaseDescDef.Id, rawBaseDescDef.Name, detailsSB.ToString(), rawBaseDescDef.Icon);

            // Change the options to have the correct pay values
            SimGameEventOption[] newOptions = rawEventDef.Options;
            foreach (SimGameEventOption sgeOption in newOptions)
            {
                if (ModConsts.Event_Option_HeadHunting_Leaves.Equals(sgeOption.Description.Id))
                {
                    // Mechwarrior leaves, company gets a payoff 
                    UpdateFundsStat(pilot, buyout, sgeOption);
                }
                else if (ModConsts.Event_Option_HeadHunting_Retained.Equals(sgeOption.Description.Id))
                {
                    // Mechwarrior statys, company pays them retention 
                    UpdateFundsStat(pilot, counterOffer, sgeOption);                    
                }
            }

            SimGameEventDef eventDef = new SimGameEventDef(
                rawEventDef.PublishState, rawEventDef.EventType, rawEventDef.Scope,
                newBaseDescDef, rawEventDef.Requirements, rawEventDef.AdditionalRequirements,
                rawEventDef.AdditionalObjects, newOptions,
                rawEventDef.Weight, rawEventDef.OneTimeEvent, rawEventDef.Tags);
            return eventDef;
        }

        // Update the funds stat to the new vaslue
        private static void UpdateFundsStat(Pilot pilot, float newValue, SimGameEventOption sgeOption)
        {
            SimGameEventResultSet[] newResultSets = sgeOption.ResultSets;
            SimGameEventResult[] newResult = newResultSets[0].Results;
            SimGameStat[] newStats = newResult[0].Stats;
            for (int i = 0; i < newStats.Length; i++)
            {
                SimGameStat stat = newStats[i];
                if (ModStats.HBS_Company_Funds.Equals(stat.name))
                {
                    Mod.Log.Debug?.Write($" --- Changing funds stat from: {stat.value} to value: {newValue} for pilot: {pilot.Name}");
                    stat.value = $"{newValue}";
                }

                newStats[i] = stat;
            }
            newResult[0].Stats = newStats;
            newResultSets[0].Results = newResult;
            sgeOption.ResultSets = newResultSets;
        }
    }
}
