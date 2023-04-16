using BattleTech.UI;
using HumanResources.Crew;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGCaptainsQuartersStatusScreen), "RefreshData")]
    [HarmonyAfter(new string[] { "dZ.Zappo.MonthlyTechAdjustment", "us.frostraptor.IttyBittyLivingSpace" })]
    public static class SGCaptainsQuartersStatusScreen_RefreshData
    {
        public static void Postfix(SGCaptainsQuartersStatusScreen __instance, EconomyScale expenditureLevel, bool showMoraleChange)
        {

            // Redo all the mechwarrior cost
            float expenditureCostModifier = __instance.simState.GetExpenditureCostModifier(expenditureLevel);
            ClearListLineItems(__instance.SectionTwoExpensesList, __instance.simState);
            int ongoingMechWariorCosts = 0;

            //int oldCosts = 0;
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
            foreach (Pilot item in __instance.simState.PilotRoster)
            {
                string key = item.pilotDef.Description.DisplayName;

                CrewDetails details = ModState.GetCrewDetails(item.pilotDef);
                //oldCosts += Mathf.CeilToInt(expenditureCostModifier * (float)__instance.simState.GetMechWarriorValue(item.pilotDef));

                Mod.Log.Debug?.Write($" Pilot: {item.Name} has salary: {details.AdjustedSalary}");
                list.Add(new KeyValuePair<string, int>(key, details.AdjustedSalary));
            }

            // Sort by most expensive
            list.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => b.Value.CompareTo(a.Value));

            // Create a new line item for each
            list.ForEach(delegate (KeyValuePair<string, int> entry)
            {
                ongoingMechWariorCosts += entry.Value;
                AddListLineItem(__instance.SectionTwoExpensesList, __instance.simState, entry.Key,
                    SimGameState.GetCBillString(entry.Value));
            });

            __instance.SectionTwoExpensesField.SetText(SimGameState.GetCBillString(ongoingMechWariorCosts));
        }

        public static List<KeyValuePair<string, int>> GetCurrentKeys(Transform container, SimGameState sgs)
        {

            List<KeyValuePair<string, int>> currentKeys = new List<KeyValuePair<string, int>>();
            IEnumerator enumerator = container.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    Transform transform = (Transform)obj;
                    SGKeyValueView component = transform.gameObject.GetComponent<SGKeyValueView>();

                    Mod.Log.Debug?.Write($"SGCQSS:RD - Reading key from component:{component.name}.");
                    TextMeshProUGUI keyText = component.Key;
                    string key = keyText.text;
                    Mod.Log.Debug?.Write($"SGCQSS:RD - key found as: {key}");

                    TextMeshProUGUI valueText = component.Value;
                    string valueS = valueText.text;
                    string digits = Regex.Replace(valueS, @"[^\d]", "");
                    Mod.Log.Debug?.Write($"SGCQSS:RD - rawValue:{valueS} digits:{digits}");
                    int value = Int32.Parse(digits);

                    Mod.Log.Debug?.Write($"SGCQSS:RD - found existing pair: {key} / {value}");
                    KeyValuePair<string, int> kvp = new KeyValuePair<string, int>(key, value);
                    currentKeys.Add(kvp);

                }
            }
            catch (Exception e)
            {
                Mod.Log.Info?.Write($"Failed to get key-value pairs: {e.Message}");
            }

            return currentKeys;
        }

        private static void AddListLineItem(Transform list, SimGameState sgs, string key, string value)
        {
            GameObject gameObject = sgs.DataManager.PooledInstantiate("uixPrfPanl_captainsQuarters_quarterlyReportLineItem-element",
                BattleTechResourceType.UIModulePrefabs, null, null, list);
            SGKeyValueView component = gameObject.GetComponent<SGKeyValueView>();
            gameObject.transform.localScale = Vector3.one;
            component.SetData(key, value);
        }

        private static void ClearListLineItems(Transform container, SimGameState sgs)
        {
            List<GameObject> list = new List<GameObject>();
            IEnumerator enumerator = container.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object obj = enumerator.Current;
                    Transform transform = (Transform)obj;
                    list.Add(transform.gameObject);
                }
            }
            finally
            {
                IDisposable disposable;
                if ((disposable = (enumerator as IDisposable)) != null)
                {
                    disposable.Dispose();
                }
            }
            while (list.Count > 0)
            {
                GameObject gameObject = list[0];
                sgs.DataManager.PoolGameObject("uixPrfPanl_captainsQuarters_quarterlyReportLineItem-element", gameObject);
                list.Remove(gameObject);
            }
        }

    }
}
