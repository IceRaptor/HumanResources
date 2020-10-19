﻿using BattleTech;
using BattleTech.UI;
using Harmony;
using HumanResources.Extensions;
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
        public static void Postfix(SGCaptainsQuartersStatusScreen __instance, 
            EconomyScale expenditureLevel, bool showMoraleChange,
            Transform ___SectionTwoExpensesList, TextMeshProUGUI ___SectionTwoExpensesField,
            SimGameState ___simState)
        {

            // Redo all the mechwarrior cost
            float expenditureCostModifier = ___simState.GetExpenditureCostModifier(expenditureLevel);
            ClearListLineItems(___SectionTwoExpensesList, ___simState);
            int ongoingMechWariorCosts = 0;

            //int oldCosts = 0;
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
            foreach (Pilot item in ___simState.PilotRoster)
            {
                string key = item.pilotDef.Description.DisplayName;
                
                CrewDetails details = ModState.GetCrewDetails(item.pilotDef);
                //oldCosts += Mathf.CeilToInt(expenditureCostModifier * (float)___simState.GetMechWarriorValue(item.pilotDef));

                Mod.Log.Debug?.Write($" Pilot: {item.Name} has salary: {details.AdjustedSalary}");
                list.Add(new KeyValuePair<string, int>(key, details.AdjustedSalary));
            }

            // Sort by most expensive
            list.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => b.Value.CompareTo(a.Value));

            // Create a new line item for each
            list.ForEach(delegate (KeyValuePair<string, int> entry)
            {
                ongoingMechWariorCosts += entry.Value;
                AddListLineItem(___SectionTwoExpensesList, ___simState, entry.Key, 
                    SimGameState.GetCBillString(entry.Value));
            });

            ___SectionTwoExpensesField.SetText(SimGameState.GetCBillString(ongoingMechWariorCosts));

            // Rectify the salary field

            //SimGameState simGameState = UnityGameInstance.BattleTechGame.Simulation;
            //if (__instance == null || ___SectionOneExpensesList == null || ___SectionOneExpensesField == null || simGameState == null)
            //{
            //    Mod.Log.Debug?.Write($"SGCQSS:RD - skipping");
            //    return;
            //}

            //// TODO: Add this to mech parts maybe?
            ////float expenditureCostModifier = simGameState.GetExpenditureCostModifier(expenditureLevel);

            //// Determine the level of aerospace support
            ////Statistic aerospaceAssets = simGameState.CompanyStats.GetStatistic("AerospaceAssets");
            ////int aerospaceSupport = aerospaceAssets != null ? aerospaceAssets.Value<int>() : 0;

            //Mod.Log.Info?.Write($"SGCQSS:RD - entered. Parsing current keys.");

            //List<KeyValuePair<string, int>> currentKeys = GetCurrentKeys(___SectionOneExpensesList, ___simState);
            //// Extract the active mechs from the list, then re-add the updated price
            //List<KeyValuePair<string, int>> filteredKeys = FilterActiveMechs(currentKeys, ___simState);
            //List<KeyValuePair<string, int>> activeMechs = GetUpkeepLabels(___simState);
            //filteredKeys.AddRange(activeMechs);

            //// Add the new costs
            //int newActiveMechCosts = MonthlyCostCalcs.SumMonthlyMechCosts(___simState);

            //filteredKeys.Sort(new ExpensesSorter());

            //Mod.Log.Info?.Write($"SGCQSS:RD - Clearing items");
            //ClearListLineItems(___SectionOneExpensesList, ___simState);

            //Mod.Log.Info?.Write($"SGCQSS:RD - Adding listLineItems");
            //int totalCost = 0;
            //try
            //{
            //    foreach (KeyValuePair<string, int> kvp in filteredKeys)
            //    {
            //        Mod.Log.Info?.Write($"SGCQSS:RD - Adding key:{kvp.Key} value:{kvp.Value}");
            //        totalCost += kvp.Value;
            //        AddListLineItem(___SectionOneExpensesList, ___simState, kvp.Key, SimGameState.GetCBillString(kvp.Value));
            //    }

            //}
            //catch (Exception e)
            //{
            //    Mod.Log.Info?.Write($"SGCQSS:RD - failed to add lineItemParts due to: {e.Message}");
            //}

            //// Update summary costs
            //int newCosts = totalCost;
            //string newCostsS = SimGameState.GetCBillString(newCosts);
            //Mod.Log.Debug?.Write($"SGCQSS:RD - total:{newCosts} = activeMechs:{newActiveMechCosts}");

            //try
            //{
            //    ___SectionOneExpensesField.SetText(SimGameState.GetCBillString(newCosts));
            //    Mod.Log.Debug?.Write($"SGCQSS:RD - updated ");
            //}
            //catch (Exception e)
            //{
            //    Mod.Log.Info?.Write($"SGCQSS:RD - failed to update summary costs section due to: {e.Message}");
            //}
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
                    Traverse keyT = Traverse.Create(component).Field("Key");
                    TextMeshProUGUI keyText = (TextMeshProUGUI)keyT.GetValue();
                    string key = keyText.text;
                    Mod.Log.Debug?.Write($"SGCQSS:RD - key found as: {key}");

                    Traverse valueT = Traverse.Create(component).Field("Value");
                    TextMeshProUGUI valueText = (TextMeshProUGUI)valueT.GetValue();
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
