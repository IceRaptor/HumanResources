using BattleTech.StringInterpolation;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using HBS;
using HumanResources.Crew;
using Localize;
using System;
using System.Text;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SG_HiringHall_Screen), "OnPilotSelected")]
    static class SG_HiringHall_Screen_OnPilotSelected
    {
        static void Prefix(ref bool __runOriginal, SG_HiringHall_Screen __instance, Pilot p)
        {
            if (!__runOriginal) return;

            __instance.selectedPilot = p;
            __instance.DescriptionAreaObject.SetActive(value: true);
            __instance.DetailPanel.SetPilot(__instance.selectedPilot);
            __instance.MWSelectedPanel.SetPilot(__instance.selectedPilot);
            __instance.WarningsCheck();
            __instance.UpdateMoneySpot();

            // Performance tweak; skip immediate refresh
            __instance.ForceRefreshImmediate();

            __runOriginal = false;
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_Screen), "UpdateMoneySpot")]
    static class SG_HiringHall_Screen_UpdateMoneySpot
    {
        static void Postfix(SG_HiringHall_Screen __instance)
        {
            Mod.Log.Debug?.Write("Updating UpdateMoneySpot");

            if (__instance.selectedPilot != null)
            {
                Mod.Log.Debug?.Write(" -- pilot is selected");

                // Account for the salary 
                CrewDetails details = ModState.GetCrewDetails(__instance.selectedPilot.pilotDef);
                int modifiedBonus = (int)Mathf.RoundToInt(details.AdjustedBonus);
                string bonus = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Bonus_Label],
                    new string[] { SimGameState.GetCBillString(Mathf.RoundToInt(modifiedBonus)) })
                    .ToString();
                Mod.Log.Debug?.Write($"  -- bonus will be: {bonus}");

                __instance.MWInitialCostText.SetText(bonus);

                if (modifiedBonus > ModState.SimGameState.Funds)
                {
                    Mod.Log.Debug?.Write(" -- Disabling hire.");
                    __instance.MWCostColor.SetUIColor(UIColor.Red);
                    __instance.HireButton.SetState(ButtonState.Disabled);
                }
                else
                {
                    Mod.Log.Debug?.Write(" -- Enabling hire.");
                    __instance.MWCostColor.SetUIColor(UIColor.White);
                    __instance.HireButton.SetState(ButtonState.Enabled);
                }

            }
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_Screen), "ReceiveButtonPress")]
    static class SG_HiringHall_Screen_ReceiveButtonPress
    {
        static void Prefix(ref bool __runOriginal, SG_HiringHall_Screen __instance, string button)
        {
            if (!__runOriginal) return;

            Mod.Log.Debug?.Write("Updating Dialog");

            if (__instance.selectedPilot != null &&
                "Hire".Equals(button, StringComparison.InvariantCultureIgnoreCase) &&
                __instance.HireButtonValid())
            {
                Mod.Log.Debug?.Write(" -- pilot is selected");

                CrewDetails details = ModState.GetCrewDetails(__instance.selectedPilot.pilotDef);
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

                __runOriginal = false;
            }
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_MWSelectedPanel), "DisplayPilot")]
    static class SG_HiringHall_MWSelectedPanel_DisplayPilot
    {
        static void Postfix(SG_HiringHall_MWSelectedPanel __instance, Pilot p)
        {
            if (p != null && __instance.BaseSalaryText != null)
            {
                Mod.Log.Debug?.Write($"Updating MWSelectedPanel for pilot: {p.Name}");

                // Account for the salary 
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                int modifiedSalary = (int)Mathf.RoundToInt(details.AdjustedSalary);
                string modifiedSalaryS = SimGameState.GetCBillString(modifiedSalary);
                Mod.Log.Debug?.Write($"  -- salary will be: {modifiedSalaryS}");

                string salaryS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Salary_Label], new string[] { modifiedSalaryS })
                    .ToString();
                __instance.BaseSalaryText.SetText(salaryS);
            }
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_Screen), "WarningsCheck")]
    static class SG_HiringHall_Screen_WarningsCheck
    {
        static void Postfix(SG_HiringHall_Screen __instance)
        {
            Mod.Log.Debug?.Write("Updating MWSelectedPanel:WarningsCheck");

            // Use the warnings area to display the contract length terms
            if (__instance.selectedPilot != null && __instance.HireButton.State == ButtonState.Enabled)
            {
                __instance.WarningAreaObject.SetActive(true);

                CrewDetails details = ModState.GetCrewDetails(__instance.selectedPilot.pilotDef);

                string contractTermS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Contract_Term],
                    new object[] { details.ContractTerm }
                    ).ToString();
                __instance.WarningText.SetText(contractTermS);
            }
        }
    }

    [HarmonyPatch(typeof(SG_HiringHall_DetailPanel), "DisplayPilot")]
    static class SG_HiringHall_DetailPanel_DisplayPilot
    {
        static void Postfix(SG_HiringHall_DetailPanel __instance, Pilot p)
        {
            CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

            StringBuilder sb = new StringBuilder();

            // Check hazard pay
            if (details.HazardPay > 0)
            {
                string hazardPayS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Hazard_Pay],
                    new object[] { SimGameState.GetCBillString(details.HazardPay) }).ToString();
                Mod.Log.Debug?.Write($"Hazard pay is: {hazardPayS}");
                sb.Append(hazardPayS);
                sb.Append("\n\n");
            }

            // Convert favored and hated faction
            if (details.FavoredFactionId > 0)
            {
                FactionValue faction = FactionEnumeration.GetFactionByID(details.FavoredFactionId);
                string favoredFactionS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Biography_Faction_Favored],
                    new object[] { faction.FactionDef.CapitalizedName }).ToString();
                sb.Append(favoredFactionS);
                sb.Append("\n\n");
                Mod.Log.Debug?.Write($"  Favored Faction is: {favoredFactionS}");
            }

            if (details.HatedFactionId > 0)
            {
                FactionValue faction = FactionEnumeration.GetFactionByID(details.HatedFactionId);
                string hatedFactionS = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Dossier_Biography_Faction_Hated],
                    new object[] { faction.FactionDef.CapitalizedName }).ToString();
                sb.Append(hatedFactionS);
                sb.Append("\n\n");
                Mod.Log.Debug?.Write($"  Hated Faction is: {hatedFactionS}");
            }

            sb.Append(Interpolator.Interpolate(p.pilotDef.Description.GetLocalizedDetails().ToString(true), ModState.SimGameState.Context, true));

            __instance.DescriptionText.SetText(sb.ToString());
        }

    }
}
