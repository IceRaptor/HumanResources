using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using HBS.Extensions;
using HumanResources.Extensions;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGBarracksDossierPanel), "SetPilot")]
    static class SGBarracksDossierPanel_SetPilot
    {
        static void Postfix(SGBarracksDossierPanel __instance, Pilot p,
            LocalizableText ___healthText, List<GameObject> ___healthList, LocalizableText ___salary,
            LocalizableText ___firstName, LocalizableText ___lastName)
        {
            if (p == null) return;

            Mod.Log.Debug?.Write($"Updating Dossier for pilot: {p.Name}");
            CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsAerospaceCrew)
            {
                ___healthText.SetText("N/A");
                for (int i = 0; i < ___healthList.Count; i++)
                {
                    ___healthList[i].SetActive(false);
                }
            }

            string nameS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Name_Format],
                new object[] { p.FirstName, p.LastName }).ToString();
            ___firstName.SetText(nameS);

            // Set the firstname label to 'Name' instead of 'First Name'
            Mod.Log.Debug?.Write("Updating firstName to Name");
            GameObject firstNameGO = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_Dossier_LastName);
            GameObject firstNameLabelGO = firstNameGO.transform.parent.GetChild(0).GetChild(0).gameObject;
            LocalizableText firstNameLabel = firstNameLabelGO.GetComponentInChildren<LocalizableText>();
            string firstNameS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Contract_Term]).ToString();
            firstNameLabel.SetText(firstNameS);

            // Set the lastname label to 'Contract End' and the name value to the remaining days
            Mod.Log.Debug?.Write("Updating lastName to ContractTerm");
            GameObject lastNameGO = __instance.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Barracks_Dossier_LastName);
            GameObject lastNameLabelGO = lastNameGO.transform.parent.GetChild(0).GetChild(0).gameObject; // should be text_lastName -> parent -> layout-label -> label
            LocalizableText lastNameLabel = lastNameLabelGO.GetComponentInChildren<LocalizableText>();
            string contractTermS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Contract_Term]).ToString();
            lastNameLabel.SetText(contractTermS);

            string contractTermRemaining = "------";
            if (!details.IsPlayer && details.ContractTerm != 0)
            {
                int daysRemaining = details.ExpirationDay - ModState.SimGameState.DaysPassed;
                if (daysRemaining < 0) daysRemaining = 0;
                contractTermRemaining = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Days_Remaining],
                    new object[] { daysRemaining }).ToString();
                Mod.Log.Debug?.Write($" {daysRemaining} daysRemaining = {ModState.SimGameState.DaysPassed} daysPassed - {details.ExpirationDay} endDay");
            }

            ___lastName.SetText(contractTermRemaining);
            Mod.Log.Debug?.Write($"  -- done updating dossier for pilot: {p.Name}");

        }
    }
}
