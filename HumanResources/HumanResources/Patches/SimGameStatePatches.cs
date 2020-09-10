using BattleTech;
using BattleTech.Data;
using Harmony;
using SVGImporter;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SimGameState), "_OnInit")]
    static class SimGameState__OnInit
    {
        static void Postfix(SimGameState __instance, GameInstance game, SimGameDifficulty difficulty)
        {
            if (Mod.Log.IsTrace) Mod.Log.Trace?.Write("SGS:_OI entered.");

            DataManager dm = UnityGameInstance.BattleTechGame.DataManager;
            LoadRequest loadRequest = dm.CreateLoadRequest();

            // Need to load each unique icon
            Mod.Log.Info?.Write("-- Loading HUD icons");
            
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.MechWarriorButton, null);
            
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.MechTechButton, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.MechTechPortait, null);

            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.MedTechButton, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.MedTechPortrait, null);

            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.GroupPortrait, null);

            loadRequest.ProcessRequests();
            Mod.Log.Info?.Write("--  Done!");

            ModState.SimGameState = __instance;
        }
    }

    [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
    static class SimGameState_OnDayPassed
    {
        static void Postfix(SimGameState __instance, int timeLapse)
        {
            Mod.Log.Debug?.Write($"OnDayPassed called with timeLapse: {timeLapse}");
        }
    }


}
