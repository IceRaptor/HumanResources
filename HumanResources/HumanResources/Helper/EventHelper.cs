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
            detailsSB.Append($" Hiring Bonus: {SimGameState.GetCBillString(details.AdjustedBonus)}\n\n");
            detailsSB.Append($" Monthly Salary: {SimGameState.GetCBillString(details.AdjustedSalary)}\n\n");
            detailsSB.Append("</margin>\n");
            BaseDescriptionDef newBaseDescDef = new BaseDescriptionDef(rawBaseDescDef.Id, rawBaseDescDef.Name, detailsSB.ToString(), rawBaseDescDef.Icon);

            // Change the options to have the correct pay values
            SimGameEventOption[] newOptions = rawEventDef.Options;
            foreach (SimGameEventOption sgeOption in newOptions)
            {
                if (ModConsts.Event_ContractExpired_Option_Hire_Bonus.Equals(sgeOption.Description.Id))
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
    }
}
