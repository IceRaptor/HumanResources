using BattleTech;
using BattleTech.UI;
using Harmony;
using System.Collections.Generic;

namespace HumanResources.Patches.MechTechs
{

    // NOTE: Barracks used for both lance configuration as well as hiring hall
    //[HarmonyPatch(typeof(SGBarracksRosterList), "PopulateRosterAsync")]
    //public static class SGBarracksRosterList_PopulateRosterAsync
    //{
    //    public static void Postfix(SGBarracksRosterList __instance)
    //    {
    //        Mod.Log.Debug?.Write("SGBRL:PRA entered.");
    //    }
    //}

    [HarmonyPatch(typeof(SG_HiringHall_Screen), "AddPeople")]
    public static class SG_HiringHall_Screen_AddPeople
    {
        public static void Postfix(SG_HiringHall_Screen __instance)
        {
            Mod.Log.Debug?.Write("SG_HH_S:AP entered.");

            //int bobNum = 0;
            //List<Pilot> mechTechsAsPilots = new List<Pilot>();
            //foreach (TechDef mechTech in ModState.GetSimGameState().CurSystem.AvailableMechTechs)
            //{
            //    Mod.Log.Debug?.Write($"Found techDef with desc: {mechTech.Description} skill: {mechTech.Skill}");
            //    PilotDef mechTechPD = new PilotDef();
            //    HumanDescriptionDef mechTechPDDef = mechTechPD.Description;
            //    mechTechPD.Description.SetFirstName($"Bob");
            //    mechTechPD.Description.SetLastName($"{bobNum}");

            //    Pilot mechTechAsPilot = new Pilot(new PilotDef(), mechTechPD.Description.FullName(), true);
            //}
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_Screen), "InitData")]
    public static class SG_HiringHall_Screen_InitData
    {
        public static void Postfix(SG_HiringHall_Screen __instance, SimGameState sim)
        {
            Mod.Log.Debug?.Write("SG_HH_S:ID entered.");

            // TODO: Mech/Med Techs aren't initialized per system, so we'll need to serialize them
            if (sim.TravelState == 0)
            {
                //// We aren't traveling, so allow hiring
                //Mod.Log.Info?.Write($"Default techs for a system => mech: {sim.Constants.Story.DefaultMechTechsPerSystem} med: {sim.Constants.Story.DefaultMedTechsPerSystem}");

                //// TODO: Check planet tags for # of techs in both cases
                //Mod.Log.Info?.Write($" == Generating Mech Techs");
                //List<TechDef> mechTechs = new List<TechDef>();
                //mechTechs.AddRange(sim.PilotGenerator.GenerateTech(3, sim.CurSystem.Def.GetDifficulty(sim.SimGameMode)));
                //foreach (TechDef def in mechTechs)
                //{
                //    Mod.Log.Info?.Write($" - Tech {def.Description.FirstName} {def.Description.LastName} of faction: {def.Description.FactionValue} with skill: {def.Skill}");
                //}

                //Mod.Log.Info?.Write($" == Generating Med Techs");
                //List<TechDef> medTechs = new List<TechDef>();
                //medTechs.AddRange(sim.PilotGenerator.GenerateTech(3, sim.CurSystem.Def.GetDifficulty(sim.SimGameMode)));
                //foreach (TechDef def in medTechs)
                //{
                //    Mod.Log.Info?.Write($" - Tech {def.Description.FirstName} {def.Description.LastName} of faction: {def.Description.FactionValue} with skill: {def.Skill}");
                //}

            }

            //int bobNum = 0;
            //List<Pilot> mechTechsAsPilots = new List<Pilot>();
            //foreach (TechDef mechTech in ModState.GetSimGameState().CurSystem.AvailableMechTechs)
            //{
            //    Mod.Log.Debug?.Write($"Found techDef with desc: {mechTech.Description} skill: {mechTech.Skill}");
            //    PilotDef mechTechPD = new PilotDef();
            //    HumanDescriptionDef mechTechPDDef = mechTechPD.Description;
            //    mechTechPD.Description.SetFirstName($"Bob");
            //    mechTechPD.Description.SetLastName($"{bobNum}");

            //    Pilot mechTechAsPilot = new Pilot(new PilotDef(), mechTechPD.Description.FullName(), true);
            //}
        }
    }
}
