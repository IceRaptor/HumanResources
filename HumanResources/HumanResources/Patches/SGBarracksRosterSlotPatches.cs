﻿using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using HBS.Extensions;
using HumanResources.Crew;
using SVGImporter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGBarracksRosterSlot), "RefreshCostColorAndAvailability")]
    static class SGBarracksRosterSlot_RefreshCostColorAndAvailability
    {

        static void Postfix(SGBarracksRosterSlot __instance)
        {
            if (ModState.SimGameState == null) return; // Only patch if we're in SimGame

            Mod.Log.Debug?.Write($"Refreshing availability for pilot: {__instance.pilot.Name}");

            // TODO: This may need to be improved, as it's used inside a loop. Maybe write to company stats?
            CrewDetails details = ModState.GetCrewDetails(__instance.pilot.pilotDef);
            Mod.Log.Debug?.Write($"  -- pilot requires: {details.Size} berths");

            int usedBerths = CrewHelper.UsedBerths(ModState.SimGameState.PilotRoster);
            int availableBerths = ModState.SimGameState.GetMaxMechWarriors() - usedBerths;
            Mod.Log.Debug?.Write($"AvailableBerths: {availableBerths} = max: {ModState.SimGameState.GetMaxMechWarriors()} - used: {usedBerths}");

            // Check berths limitations
            if (details.Size > availableBerths)
            {
                Mod.Log.Info?.Write($"Pilot {__instance.pilot.Name} cannot be hired, not enough berths (needs {details.Size})");

                __instance.cantBuyMRBOverlay.SetActive(true);

                HBSTooltipStateData tooltipStateData = new HBSTooltipStateData();
                tooltipStateData.SetContextString($"DM.BaseDescriptionDefs[{ModConsts.Tooltip_NotEnoughBerths}]");
                __instance.cantBuyToolTip.SetDefaultStateData(tooltipStateData);
            }

            // Check type limitations
            if (!CrewHelper.CanHireMoreCrewOfType(details))
            {
                Mod.Log.Info?.Write($"Pilot {__instance.pilot.Name} cannot be hired, too many of type already employed.");

                __instance.cantBuyMRBOverlay.SetActive(true);

                HBSTooltipStateData tooltipStateData = new HBSTooltipStateData();
                tooltipStateData.SetContextString($"DM.BaseDescriptionDefs[{ModConsts.Tooltip_TooManyOfType}]");
                __instance.cantBuyToolTip.SetDefaultStateData(tooltipStateData);
            }
            else
            {
                Mod.Log.Debug?.Write($"Pilot {__instance.pilot.Name} can be hired, no limiations on max.");
            }

            // Set the prices
            //int purchaseCostAfterReputationModifier = ModState.SimGameState.CurSystem.GetPurchaseCostAfterReputationModifier(
            //    ModState.SimGameState.GetMechWarriorHiringCost(__instance.pilot.pilotDef)
            //    );
            // TODO: Apply system cost multiplier
            // Hiring cost is influenced by:
            //  - current morale rating
            //  - any faction alignment for units
            //  - any reputation modifiers

            __instance.costText.SetText(SimGameState.GetCBillString(details.HiringBonus));

            UIColor costColor = UIColor.Red;
            if (details.HiringBonus <= ModState.SimGameState.Funds) costColor = UIColor.White;
            __instance.costTextColor.SetUIColor(costColor);
        }
    }

    [HarmonyPatch(typeof(SGBarracksRosterSlot), "Refresh")]
    static class SGBarracksRosterSlot_Refresh
    {
        static void Prefix(ref bool __runOriginal, SGBarracksRosterSlot __instance)
        {
            if (!__runOriginal) return;

            if (ModState.SimGameState == null) return; // Only patch if we're in SimGame

            Mod.Log.Debug?.Write($"PRE Calling refresh for pilot: {__instance.pilot.Name}");
        }

        static void Postfix(SGBarracksRosterSlot __instance )
        {
            if (ModState.SimGameState == null) return; // Only patch if we're in SimGame

            if (__instance.pilot == null) return;
            Mod.Log.Debug?.Write($"POST Calling refresh for pilot: {__instance.pilot.Name}");

            CrewDetails details = ModState.GetCrewDetails(__instance.pilot.pilotDef);

            // Find the common GameObjects we need to manipulate
            GameObject portraitOverride = GetOrCreateProfileOverride(__instance.portrait);
            if (details.IsAerospaceCrew || details.IsMechTechCrew || details.IsMedTechCrew || details.IsVehicleCrew) portraitOverride.SetActive(true);
            else portraitOverride.SetActive(false);

            GameObject crewBlock = GetOrCreateCrewBlock(__instance.portrait.gameObject);
            if (details.IsAerospaceCrew || details.IsMechTechCrew || details.IsMedTechCrew) crewBlock.SetActive(true);
            else crewBlock.SetActive(false);

            GameObject mwStats = __instance.portrait.transform.parent.parent.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Profile_Stats_Block);
            if (details.IsAerospaceCrew || details.IsMechTechCrew || details.IsMedTechCrew) mwStats.SetActive(false);
            else mwStats.SetActive(true);

            GameObject layoutTitleGO = __instance.GameObject.FindFirstChildNamed(ModConsts.GO_HBS_Profile_Layout_Title);
            Image layoutTitleImg = layoutTitleGO.GetComponent<Image>();

            if (details.IsAerospaceCrew)
            {
                Mod.Log.Debug?.Write($"  -- pilot is Aerospace crew");
                layoutTitleImg.color = Mod.Config.Crew.AerospaceColor;

                __instance.portrait.gameObject.SetActive(false);
                __instance.AbilitiesObject.SetActive(false);
                __instance.roninIcon.gameObject.SetActive(false);
                __instance.veteranIcon.gameObject.SetActive(false);

                // Set the portrait icon
                SVGAsset icon = ModState.SimGameState.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.CrewPortrait_Aerospace, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn?.Write($"ERROR READING ICON: {Mod.Config.Icons.CrewPortrait_Aerospace}");
                SVGImage image = portraitOverride.GetComponentInChildren<SVGImage>();
                image.vectorGraphics = icon;

                // Set the crew size
                LocalizableText[] texts = crewBlock.GetComponentsInChildren<LocalizableText>();

                LocalizableText lt1 = texts[0];
                string sizeText = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Size],
                    new object[] { details.SizeLabel, details.Size }).ToString();
                lt1.SetText(sizeText);

                // Force the font size here, otherwise the right hand panel isn't correct
                lt1.fontSize = 16f;
                lt1.fontSizeMin = 16f;
                lt1.fontSizeMax = 16f;

                LocalizableText lt2 = texts[1];
                string skillText = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Skill_Aerospace_Points],
                    new object[] { details.Value }).ToString();
                lt2.SetText(skillText);

                // Force the font size here, otherwise the right hand panel isn't correct
                lt2.fontSize = 16f;
                lt2.fontSizeMin = 16f;
                lt2.fontSizeMax = 16f;

                // Set the expertise of the crew
                __instance.expertise.color = Color.white;
                __instance.expertise.SetText(details.ExpertiseLabel);

            }
            else if (details.IsMechTechCrew)
            {
                Mod.Log.Debug?.Write($"  -- pilot is Mechtech crew");
                layoutTitleImg.color = Mod.Config.Crew.MechTechCrewColor;

                __instance.portrait.gameObject.SetActive(false);
                __instance.AbilitiesObject.SetActive(false);
                __instance.roninIcon.gameObject.SetActive(false);
                __instance.veteranIcon.gameObject.SetActive(false);

                // Set the portrait icon
                SVGAsset icon = ModState.SimGameState.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.CrewPortrait_MechTech, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn?.Write($"ERROR READING ICON: {Mod.Config.Icons.CrewPortrait_MechTech}");
                SVGImage image = portraitOverride.GetComponentInChildren<SVGImage>();
                image.vectorGraphics = icon;

                // Set the crew size
                LocalizableText[] texts = crewBlock.GetComponentsInChildren<LocalizableText>();

                LocalizableText lt1 = texts[0];
                string sizeText = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Size],
                    new object[] { details.SizeLabel, details.Size }).ToString();
                lt1.SetText(sizeText);

                // Force the font size here, otherwise the right hand panel isn't correct
                lt1.fontSize = 16f;
                lt1.fontSizeMin = 16f;
                lt1.fontSizeMax = 16f;

                LocalizableText lt2 = texts[1];
                string skillText = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Skill_MechTech_Points],
                    new object[] { details.Value }).ToString();
                lt2.SetText(skillText);

                // Force the font size here, otherwise the right hand panel isn't correct
                lt2.fontSize = 16f;
                lt2.fontSizeMin = 16f;
                lt2.fontSizeMax = 16f;

                // Set the expertise of the crew
                __instance.expertise.color = Color.white;
                __instance.expertise.SetText(details.ExpertiseLabel);

            }
            else if (details.IsMedTechCrew)
            {
                Mod.Log.Debug?.Write($"  -- pilot is Medtech crew");
                layoutTitleImg.color = Mod.Config.Crew.MedTechCrewColor;

                __instance.portrait.gameObject.SetActive(false);
                __instance.AbilitiesObject.SetActive(false);
                __instance.roninIcon.gameObject.SetActive(false);
                __instance.veteranIcon.gameObject.SetActive(false);

                // Set the portrait icon
                SVGAsset icon = ModState.SimGameState.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.CrewPortrait_MedTech, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn?.Write($"ERROR READING ICON: {Mod.Config.Icons.CrewPortrait_MedTech}");
                SVGImage image = portraitOverride.GetComponentInChildren<SVGImage>();
                image.vectorGraphics = icon;

                // Set the crew size
                LocalizableText[] texts = crewBlock.GetComponentsInChildren<LocalizableText>();

                LocalizableText lt1 = texts[0];
                string sizeText = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Size],
                    new object[] { details.SizeLabel, details.Size }).ToString();
                lt1.SetText(sizeText);

                // Force the font size here, otherwise the right hand panel isn't correct
                lt1.fontSize = 16f;
                lt1.fontSizeMin = 16f;
                lt1.fontSizeMax = 16f;

                LocalizableText lt2 = texts[1];
                string skillText = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Skill_MedTech_Points],
                    new object[] { details.Value }).ToString();
                lt2.SetText(skillText);

                // Force the font size here, otherwise the right hand panel isn't correct
                lt2.fontSize = 16f;
                lt2.fontSizeMin = 16f;
                lt2.fontSizeMax = 16f;

                // Set the expertise of the crew
                __instance.expertise.color = Color.white;
                __instance.expertise.SetText(details.ExpertiseLabel);
            }
            else if (details.IsVehicleCrew)
            {
                Mod.Log.Debug?.Write($"  -- pilot is Vehicle crew");
                layoutTitleImg.color = Mod.Config.Crew.VehicleCrewColor;

                __instance.portrait.gameObject.SetActive(false);
                __instance.roninIcon.gameObject.SetActive(false);

                __instance.callsign.SetText("VCREW: " + __instance.pilot.Callsign);

                // Set the portrait icon
                SVGAsset icon = ModState.SimGameState.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.CrewPortrait_Vehicle, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn?.Write($"ERROR READING ICON: {Mod.Config.Icons.CrewPortrait_Vehicle}");
                SVGImage image = portraitOverride.GetComponentInChildren<SVGImage>();
                image.vectorGraphics = icon;

                __instance.expertise.color = Color.white;
            }
            else
            {
                Mod.Log.Debug?.Write($"  -- pilot is Mechwarrior");
                __instance.portrait.gameObject.SetActive(true);
            }

            Mod.Log.Debug?.Write($"LayoutTitleImg color set to: {layoutTitleImg.color}");
        }

        private static GameObject GetOrCreateProfileOverride(Image portrait)
        {
            GameObject profileOverrideGO = portrait.transform.parent.gameObject.FindFirstChildNamed(ModConsts.GO_Profile_Override);
            if (profileOverrideGO == null)
            {
                Mod.Log.Debug?.Write(" -- CREATING PORTRAIT OVERRIDE NODE");
                profileOverrideGO = new GameObject();
                profileOverrideGO.name = ModConsts.GO_Profile_Override;
                profileOverrideGO.transform.parent = portrait.transform.parent;
                profileOverrideGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

                Vector3 newPos = portrait.transform.position;
                newPos.x -= 30f;
                newPos.y += 30f;
                profileOverrideGO.transform.position = newPos;

                SVGImage image = profileOverrideGO.GetOrAddComponent<SVGImage>();
                if (image == null) Mod.Log.Warn?.Write(" -- FAILED TO LOAD IMAGE - WILL NRE!");
                image.color = Color.white;
                image.enabled = true;
            }

            return profileOverrideGO;
        }

        private static GameObject GetOrCreateCrewBlock(GameObject portrait)
        {
            // mw_Image -> mw_PortraitSlot -> layout_details
            GameObject portraitSlot = portrait.transform.parent.gameObject;
            //Mod.Log.Debug?.Write($"Is mw_PortraitSlot ? : {portraitSlot.name}");

            GameObject layoutDetails = portraitSlot.transform.parent.gameObject;
            //Mod.Log.Debug?.Write($"Is layout_details ? : {layoutDetails.name}");

            //GameObject layoutStatsOrHiringCost = layoutDetails.FindFirstChildNamed(ModConsts.GO_HBS_Profile_Layout_Stats);
            //if (layoutStatsOrHiringCost == null) Mod.Log.Warn?.Write("FAILED TO FIND layout_Stats-Or-HiringCost, will NRE!");

            GameObject layoutGO = layoutDetails.FindFirstChildNamed(ModConsts.GO_Crew_Block);
            if (layoutGO == null)
            {
                layoutGO = new GameObject();
                layoutGO.name = ModConsts.GO_Crew_Block;
                layoutGO.transform.parent = layoutDetails.transform;
                layoutGO.transform.SetAsFirstSibling();
                layoutGO.transform.localPosition = new Vector3(-20f, 0f);

                ContentSizeFitter csf = layoutGO.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

                RectTransform rt = layoutGO.GetComponent<RectTransform>();
                Vector3 newLocalPos = rt.transform.localPosition;
                newLocalPos.x -= 34f;
                newLocalPos.y -= 0f;
                rt.transform.localPosition = newLocalPos;

                VerticalLayoutGroup vlg = layoutGO.AddComponent<VerticalLayoutGroup>();
                vlg.childControlWidth = true;
                vlg.childControlHeight = true;
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = false;
                vlg.childAlignment = TextAnchor.MiddleLeft;

                layoutGO.transform.SetAsFirstSibling();
                //Mod.Log.Debug?.Write("ADDED CONTENT FITTER BLOCK");

                GameObject textBlock1 = new GameObject();
                textBlock1.transform.parent = vlg.transform;
                textBlock1.name = ModConsts.GO_Crew_Size;
                textBlock1.SetActive(true);

                LayoutElement le1 = textBlock1.AddComponent<LayoutElement>();
                le1.preferredHeight = 24f;
                le1.preferredWidth = 200f;

                LocalizableText lt1 = textBlock1.AddComponent<LocalizableText>();
                lt1.fontSize = 16f;
                lt1.fontSizeMin = 16f;
                lt1.fontSizeMax = 16f;
                lt1.alignment = TextAlignmentOptions.Left;
                lt1.enableAutoSizing = false;
                //Mod.Log.Debug?.Write("ADDED TEXTBLOCK1");

                GameObject textBlock2 = new GameObject();
                textBlock2.transform.parent = vlg.transform;
                textBlock2.name = ModConsts.GO_Crew_Skill;
                textBlock2.SetActive(true);

                LayoutElement le2 = textBlock2.AddComponent<LayoutElement>();
                le2.preferredHeight = 24f;
                le2.preferredWidth = 200f;

                LocalizableText lt2 = textBlock2.AddComponent<LocalizableText>();
                lt2.fontSize = 16f;
                lt2.fontSizeMin = 16f;
                lt2.fontSizeMax = 16f;
                lt2.alignment = TextAlignmentOptions.Left;
                lt2.enableAutoSizing = false;
                //Mod.Log.Debug?.Write("ADDED TEXTBLOCK1");

                portraitSlot.transform.SetAsFirstSibling();
            }
            return layoutGO;
        }
    }
}
