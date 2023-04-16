using HumanResources.Helper;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGTravelManager), "OnArrivedAtPlanet")]
    static class SGTravelManager_OnArrivedAtPlanet
    {
        static void Postfix(SGTravelManager __instance)
        {
            HeadHuntingHelper.UpdateNextDayOnSystemEntry();
        }
    }
}
