using BattleTech;

namespace HumanResources.Extensions
{
    public static class PilotExtensions
    {
        public static CrewDetails Evaluate(this PilotDef pilotDef)
        {
            return new CrewDetails(pilotDef);
        }
    }
}
