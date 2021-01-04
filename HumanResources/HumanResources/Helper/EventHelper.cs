using BattleTech;
using HumanResources.Extensions;
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
                    SimGameEventResultSet[] newResultSets = sgeOption.ResultSets;
                    SimGameEventResult[] newResult = newResultSets[0].Results;
                    SimGameStat[] newStats = newResult[0].Stats;
                    for (int i = 0; i < newStats.Length; i++)
                    {
                        SimGameStat stat = newStats[i];
                        if (ModStats.HBS_Company_Funds.Equals(stat.name))
                        {
                            (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();
                            Mod.Log.Debug?.Write($" --- Changing funds stat from: {stat.value} to adjustedBonus: {expired.Details?.AdjustedBonus} for pilot: {expired.Pilot.Name}");
                            stat.value = $"-{expired.Details.AdjustedBonus}";
                        }

                        newStats[i] = stat;
                    }
                    newResult[0].Stats = newStats;
                    newResultSets[0].Results = newResult;
                    sgeOption.ResultSets = newResultSets;
                }
            }

            SimGameEventDef expiredEventDef = new SimGameEventDef(
                rawEventDef.PublishState, rawEventDef.EventType, rawEventDef.Scope,
                newBaseDescDef, rawEventDef.Requirements, rawEventDef.AdditionalRequirements,
                rawEventDef.AdditionalObjects, newOptions, 
                rawEventDef.Weight, rawEventDef.OneTimeEvent, rawEventDef.Tags);
            return expiredEventDef;
        }

        public static SimGameEventDef CreateHeadHuntingEvent(Pilot pilot, CrewDetails details, float retentionBonus, float buyoutPayment)
        {
            SimGameEventDef rawEventDef = ModState.SimGameState.DataManager.SimGameEventDefs.Get(ModConsts.Event_HeadHunting);

            // Change the description fields
            BaseDescriptionDef rawBaseDescDef = rawEventDef.Description;
            StringBuilder detailsSB = new StringBuilder(rawBaseDescDef.Details);
            detailsSB.Append("\n");
            detailsSB.Append("<margin=5em>\n");
            // TODO: Localize
            detailsSB.Append($" Retention Bonus: {SimGameState.GetCBillString(details.AdjustedBonus)}\n\n");
            detailsSB.Append($" Contract Buyout Payment: {SimGameState.GetCBillString(details.AdjustedSalary)}\n\n");
            detailsSB.Append("</margin>\n");
            BaseDescriptionDef newBaseDescDef = new BaseDescriptionDef(rawBaseDescDef.Id, rawBaseDescDef.Name, detailsSB.ToString(), rawBaseDescDef.Icon);

            // Change the options to have the correct pay values
            SimGameEventOption[] newOptions = rawEventDef.Options;
            foreach (SimGameEventOption sgeOption in newOptions)
            {
                if (ModConsts.Event_Option_HeadHunting_Leaves.Equals(sgeOption.Description.Id))
                {
                    // Mechwarrior leaves, company gets a payoff 
                    SimGameEventResultSet[] newResultSets = sgeOption.ResultSets;
                    SimGameEventResult[] newResult = newResultSets[0].Results;
                    SimGameStat[] newStats = newResult[0].Stats;
                    for (int i = 0; i < newStats.Length; i++)
                    {
                        SimGameStat stat = newStats[i];
                        if (ModStats.HBS_Company_Funds.Equals(stat.name))
                        {
                            Mod.Log.Debug?.Write($" --- Changing funds stat from: {stat.value} to contract_Buyout: {buyoutPayment} for pilot: {pilot.Name}");
                            stat.value = $"{buyoutPayment}";
                        }

                        newStats[i] = stat;
                    }
                    newResult[0].Stats = newStats;
                    newResultSets[0].Results = newResult;
                    sgeOption.ResultSets = newResultSets;
                }
                else if (ModConsts.Event_Option_HeadHunting_Retained.Equals(sgeOption.Description.Id))
                {
                    // Mechwarrior statys, company pays them retention 
                    SimGameEventResultSet[] newResultSets = sgeOption.ResultSets;
                    SimGameEventResult[] newResult = newResultSets[0].Results;
                    SimGameStat[] newStats = newResult[0].Stats;
                    for (int i = 0; i < newStats.Length; i++)
                    {
                        SimGameStat stat = newStats[i];
                        if (ModStats.HBS_Company_Funds.Equals(stat.name))
                        {
                            Mod.Log.Debug?.Write($" --- Changing funds stat from: {stat.value} to retention_bonus: {retentionBonus} for pilot: {pilot.Name}");
                            stat.value = $"-{retentionBonus}";
                        }

                        newStats[i] = stat;
                    }
                    newResult[0].Stats = newStats;
                    newResultSets[0].Results = newResult;
                    sgeOption.ResultSets = newResultSets;
                }
            }

            SimGameEventDef eventDef = new SimGameEventDef(
                rawEventDef.PublishState, rawEventDef.EventType, rawEventDef.Scope,
                newBaseDescDef, rawEventDef.Requirements, rawEventDef.AdditionalRequirements,
                rawEventDef.AdditionalObjects, newOptions,
                rawEventDef.Weight, rawEventDef.OneTimeEvent, rawEventDef.Tags);
            return eventDef;
        }
    }
}
