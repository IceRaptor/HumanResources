using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using Harmony;
using HBS;
using HumanResources.Extensions;
using Localize;
using System;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SG_HiringHall_Screen), "OnPilotSelected")]
    static class SG_HiringHall_Screen_OnPilotSelected
    {
        static bool Prefix(SG_HiringHall_Screen __instance, Pilot p,
            ref Pilot ___selectedPilot, GameObject ___DescriptionAreaObject, 
            SG_HiringHall_DetailPanel ___DetailPanel, SG_HiringHall_MWSelectedPanel ___MWSelectedPanel)
        {
            ___selectedPilot = p;
            ___DescriptionAreaObject.SetActive(value: true);
            ___DetailPanel.SetPilot(___selectedPilot);
            ___MWSelectedPanel.SetPilot(___selectedPilot);
            __instance.WarningsCheck();
            __instance.UpdateMoneySpot();

            // Performance tweak; skip immediate refresh
            //__instance.ForceRefreshImmediate();

            return false;
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_Screen), "UpdateMoneySpot")]
    static class SG_HiringHall_Screen_UpdateMoneySpot
    {
        static void Postfix(SG_HiringHall_Screen __instance,
            Pilot ___selectedPilot, LocalizableText ___MWInitialCostText, UIColorRefTracker ___MWCostColor, 
            HBSDOTweenButton ___HireButton)
        {
            Mod.Log.Debug?.Write("Updating UpdateMoneySpot");

            if (___selectedPilot != null)
            {
                Mod.Log.Debug?.Write(" -- pilot is selected");

                // Account for the salary 
                CrewDetails details = ModState.GetCrewDetails(___selectedPilot.pilotDef);
                int modifiedBonus = (int)Mathf.RoundToInt(details.AdjustedBonus);
                string bonus = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Bonus_Label],
                    new string[] { SimGameState.GetCBillString(Mathf.RoundToInt(modifiedBonus)) })
                    .ToString();
                Mod.Log.Debug?.Write($"  -- bonus will be: {bonus}");

                ___MWInitialCostText.SetText(bonus);

                if (modifiedBonus > ModState.SimGameState.Funds)
                {
                    Mod.Log.Debug?.Write(" -- Disabling hire.");
                    ___MWCostColor.SetUIColor(UIColor.Red);
                    ___HireButton.SetState(ButtonState.Disabled);
                }
                else
                {
                    Mod.Log.Debug?.Write(" -- Enabling hire.");
                    ___MWCostColor.SetUIColor(UIColor.White);
                    ___HireButton.SetState(ButtonState.Enabled);
                }

            }
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_Screen), "ReceiveButtonPress")]
    static class SG_HiringHall_Screen_ReceiveButtonPress
    {
        static bool Prefix(SG_HiringHall_Screen __instance, Pilot ___selectedPilot, string button)
        {
            Mod.Log.Debug?.Write("Updating Dialog");

            if (___selectedPilot != null && 
                "Hire".Equals(button, StringComparison.InvariantCultureIgnoreCase) &&
                __instance.HireButtonValid())
            {
                Mod.Log.Debug?.Write(" -- pilot is selected");

                CrewDetails details = ModState.GetCrewDetails(___selectedPilot.pilotDef);
                int modifiedBonus = (int)Mathf.RoundToInt(details.AdjustedBonus);
                string salaryS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Hire_Button], 
                    new string[] { SimGameState.GetCBillString(Mathf.RoundToInt(modifiedBonus)) })
                    .ToString();
                Mod.Log.Debug?.Write($"  -- bonus will be: {salaryS}");

                GenericPopupBuilder.Create("Confirm?", salaryS)
                    .AddButton("Cancel")
                    .AddButton("Accept", __instance.HireCurrentPilot)
                    .CancelOnEscape()
                    .AddFader(LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PopupBackfill)
                    .Render();

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_MWSelectedPanel), "DisplayPilot")]
    static class SG_HiringHall_MWSelectedPanel_DisplayPilot
    {
        static void Postfix(SG_HiringHall_MWSelectedPanel __instance, Pilot p, LocalizableText ___BaseSalaryText)
        {
            if (p != null && ___BaseSalaryText != null)
            {
                Mod.Log.Debug?.Write($"Updating MWSelectedPanel for pilot: {p.Name}");

                // Account for the salary 
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                int modifiedSalary = (int)Mathf.RoundToInt(details.AdjustedSalary);
                string modifiedSalaryS = SimGameState.GetCBillString(modifiedSalary);
                Mod.Log.Debug?.Write($"  -- salary will be: {modifiedSalaryS}");

                string salaryS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Salary_Label], new string[] { modifiedSalaryS })
                    .ToString();
                ___BaseSalaryText.SetText(salaryS);
            }
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_Screen), "WarningsCheck")]
    static class SG_HiringHall_Screen_WarningsCheck
    {
        static void Postfix(SG_HiringHall_Screen __instance, GameObject ___WarningAreaObject,
            HBSDOTweenButton ___HireButton, LocalizableText ___WarningText,
            HBSTooltip ___NoHireTooltip, Pilot ___selectedPilot)
        {
            Mod.Log.Debug?.Write("Updating MWSelectedPanel:WarningsCheck");

            // Use the warnings area to display the contract length terms
            if (___selectedPilot != null && ___HireButton.State == ButtonState.Enabled)
            {
                ___WarningAreaObject.SetActive(true);

                CrewDetails details = ModState.GetCrewDetails(___selectedPilot.pilotDef);

                string contractTermS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Contract_Term],
                    new object[] { details.ContractTerm }
                    ).ToString();
                ___WarningText.SetText(contractTermS);
            }
        }
    }

}
