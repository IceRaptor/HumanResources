using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
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
                CrewDetails details = ___selectedPilot.Evaluate();
                float salaryExpense = details.Bonus *
                    ModState.SimGameState.GetExpenditureCostModifier(ModState.SimGameState.ExpenditureLevel);
                int modifiedSalary = (int)Mathf.RoundToInt(salaryExpense);
                string salaryS = Strings.T("{0} Bonus", SimGameState.GetCBillString(Mathf.RoundToInt(modifiedSalary)));
                Mod.Log.Debug?.Write($"  -- bonus will be: {salaryS}");

                ___MWInitialCostText.SetText(salaryS);

                if (modifiedSalary > ModState.SimGameState.Funds)
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

    [HarmonyPatch(typeof(SG_HiringHall_MWSelectedPanel), "DisplayPilot")]
    static class SG_HiringHall_MWSelectedPanel_DisplayPilot
    {
        static void Postfix(SG_HiringHall_MWSelectedPanel __instance, Pilot p, LocalizableText ___BaseSalaryText)
        {
            Mod.Log.Debug?.Write("Updating MWSelectedPanel");

            // Account for the salary 
            CrewDetails details = p.Evaluate();
            float salaryExpense = details.Salary * 
                ModState.SimGameState.GetExpenditureCostModifier(ModState.SimGameState.ExpenditureLevel);
            int modifiedSalary = (int)Mathf.RoundToInt(salaryExpense);
            string salaryS = Strings.T("{0} / Mo", SimGameState.GetCBillString(Mathf.RoundToInt(modifiedSalary)));
            Mod.Log.Debug?.Write($"  -- salary will be: {salaryS}");

            ___BaseSalaryText.SetText(salaryS);
        }
    }

}
