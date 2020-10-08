using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;
using Harmony;
using HBS.Extensions;
using HumanResources.Extensions;
using HumanResources.Helper;
using SVGImporter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGBarracksRosterSlot), "RefreshCostColorAndAvailability")]
    static class SGBarracksRosterSlot_RefreshCostColorAndAvailability
    {
        static void Postfix(SGBarracksRosterSlot __instance, Pilot ___pilot, 
            UIColorRefTracker ___costTextColor, GameObject ___cantBuyMRBOverlay,
            HBSTooltip ___cantBuyToolTip
            )
        {
            Mod.Log.Debug?.Write($"Refreshing availability for pilot: {___pilot.Name}");
            
            // TODO: This may need to be improved, as it's used inside a loop. Maybe write to company stats?
            CrewDetails details = ___pilot.Evaluate();
            Mod.Log.Debug?.Write($"  -- pilot requires: {details.Size} berths");

            int usedBerths = PilotHelper.UsedBerths(ModState.SimGameState.PilotRoster);
            int availableBerths = ModState.SimGameState.GetMaxMechWarriors() - usedBerths;
            Mod.Log.Debug?.Write($"AvailableBerths: {availableBerths} = max: {ModState.SimGameState.GetMaxMechWarriors()} - used: {usedBerths}");
            
            if (details.Size > availableBerths)
            {
                Mod.Log.Info?.Write($"Pilot {___pilot.Name} cannot be hired, not enough berths (needs {details.Size})");

                ___cantBuyMRBOverlay.SetActive(true);

                HBSTooltipStateData tooltipStateData = new HBSTooltipStateData();
                tooltipStateData.SetContextString($"DM.BaseDescriptionDefs[{ModConsts.Tooltip_NotEnoughBerths}]");
                ___cantBuyToolTip.SetDefaultStateData(tooltipStateData);
            }

        }
    }

    [HarmonyPatch(typeof(SGBarracksRosterSlot), "Init")]
    static class SGBarracksRosterSlot_Init
    {
        static void Prefix(SGBarracksRosterSlot __instance, Pilot ___pilot)
        {
            Mod.Log.Debug?.Write($"Initing with pilot: {___pilot?.Name}");
        }
    }

    [HarmonyPatch(typeof(SGBarracksRosterSlot), "InitNoDrag")]
    static class SGBarracksRosterSlot_InitNoDrag
    {
        static void Prefix(SGBarracksRosterSlot __instance, Pilot ___pilot)
        {
            Mod.Log.Debug?.Write($"InitNoDrag with pilot: {___pilot?.Name}");
        }
    }


    [HarmonyPatch(typeof(SGBarracksRosterSlot), "InitNoInteract")]
    static class SGBarracksRosterSlot_InitNoInteract
    {
        static void Prefix(SGBarracksRosterSlot __instance, Pilot ___pilot)
        {
            Mod.Log.Debug?.Write($"InitNoInteract with pilot: {___pilot?.Name}");
        }
    }

    [HarmonyPatch(typeof(SGBarracksRosterSlot), "Refresh")]
    static class SGBarracksRosterSlot_Refresh
    {
        static void Prefix(Pilot ___pilot)
        {
            Mod.Log.Debug?.Write($"PRE Calling refresh for pilot: {___pilot.Name}");
        }

        static void Postfix(SGBarracksRosterSlot __instance, Pilot ___pilot, 
            GameObject ___AbilitiesObject, LocalizableText ___callsign, Image ___portrait,
            SVGImage ___roninIcon, SVGImage ___veteranIcon, 
            LocalizableText ___expertise, HBSTooltip ___ExpertiseTooltip)
        {
            if (___pilot == null) return;
            Mod.Log.Debug?.Write($"POST Calling refresh for pilot: {___pilot.Name}");

            CrewDetails details = ___pilot.Evaluate();

            // Find the common GameObjects we need to manipulate
            GameObject portraitOverride = GetOrCreateProfileOverride(___portrait);
            if (details.IsMechTechCrew || details.IsMedTechCrew || details.IsVehicleCrew) portraitOverride.SetActive(true);
            else portraitOverride.SetActive(false);

            GameObject crewBlock = GetOrCreateCrewBlock(___portrait.gameObject);
            if (details.IsMechTechCrew || details.IsMedTechCrew) crewBlock.SetActive(true);
            else crewBlock.SetActive(false);

            GameObject mwStats = ___portrait.transform.parent.parent.gameObject.FindFirstChildNamed(ModConsts.GO_HBS_Profile_Stats_Block);
            if (details.IsMechTechCrew || details.IsMedTechCrew) mwStats.SetActive(false);
            else mwStats.SetActive(true);

            if (details.IsMechWarrior) return; // nothing to do

            GameObject layoutTitleGO = __instance.GameObject.FindFirstChildNamed(ModConsts.GO_HBS_Profile_Layout_Title);
            Image layoutTitleImg = layoutTitleGO.GetComponent<Image>();
            if (details.IsMechTechCrew)
            {
                layoutTitleImg.color = Mod.Config.Crew.MechTechCrewColor;

                ___portrait.gameObject.SetActive(false);
                ___AbilitiesObject.SetActive(false);
                ___roninIcon.gameObject.SetActive(false);
                ___veteranIcon.gameObject.SetActive(false);

                // Set the portrait icon
                SVGAsset icon = ModState.SimGameState.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.CrewPortrait_MechTech, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn?.Write($"ERROR READING ICON: {Mod.Config.Icons.CrewPortrait_MechTech}");
                SVGImage image = portraitOverride.GetComponentInChildren<SVGImage>();
                image.vectorGraphics = icon;

                // Set the crew size
                LocalizableText lt1 = crewBlock.GetComponentInChildren<LocalizableText>();
                string sizeText = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Size], 
                    new object[] { details.SizeLabel, details.Size }).ToString();
                lt1.SetText(sizeText);

                // Force the font size here, otherwise the right hand panel isn't correct
                lt1.fontSize = 18f;
                lt1.fontSizeMin = 18f;
                lt1.fontSizeMax = 18f;

                // Set the expertise of the crew
                ___expertise.color = Color.white;
                ___expertise.SetText(details.SkillLabel);

            }
            else if (details.IsMedTechCrew)
            {
                layoutTitleImg.color = Mod.Config.Crew.MedTechCrewColor;

                ___portrait.gameObject.SetActive(false);
                ___AbilitiesObject.SetActive(false);
                ___roninIcon.gameObject.SetActive(false);
                ___veteranIcon.gameObject.SetActive(false);

                // Set the portrait icon
                SVGAsset icon = ModState.SimGameState.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.CrewPortrait_MedTech, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn?.Write($"ERROR READING ICON: {Mod.Config.Icons.CrewPortrait_MedTech}");
                SVGImage image = portraitOverride.GetComponentInChildren<SVGImage>();
                image.vectorGraphics = icon;

                // Set the crew size
                LocalizableText lt1 = crewBlock.GetComponentInChildren<LocalizableText>();
                string sizeText = new Localize.Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Size], 
                    new object[] { details.SizeLabel, details.Size }).ToString();
                lt1.SetText(sizeText);

                // Force the font size here, otherwise the right hand panel isn't correct
                lt1.fontSize = 18f;
                lt1.fontSizeMin = 18f;
                lt1.fontSizeMax = 18f;

                // Set the expertise of the crew
                ___expertise.color = Color.white;
                ___expertise.SetText(details.SkillLabel);
            }
            else if (details.IsVehicleCrew)
            {
                layoutTitleImg.color = Mod.Config.Crew.VehicleCrewColor;

                ___portrait.gameObject.SetActive(false);
                ___roninIcon.gameObject.SetActive(false);

                ___callsign.SetText("VCREW: " + ___pilot.Callsign);

                // Set the portrait icon
                SVGAsset icon = ModState.SimGameState.DataManager.GetObjectOfType<SVGAsset>(Mod.Config.Icons.CrewPortrait_Vehicle, BattleTechResourceType.SVGAsset);
                if (icon == null) Mod.Log.Warn?.Write($"ERROR READING ICON: {Mod.Config.Icons.CrewPortrait_Vehicle}");
                SVGImage image = portraitOverride.GetComponentInChildren<SVGImage>();
                image.vectorGraphics = icon;

                ___expertise.color = Color.white;
            }
            // TODO: Aerospace

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
                layoutGO.transform.localPosition = new Vector3(-30f, 0f);

                ContentSizeFitter csf = layoutGO.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

                RectTransform rt = layoutGO.GetComponent<RectTransform>();
                Vector3 newLocalPos = rt.transform.localPosition;
                newLocalPos.x -= 34f;
                newLocalPos.y -= 0f;
                rt.transform.localPosition = newLocalPos;

                HorizontalLayoutGroup hlg = layoutGO.AddComponent<HorizontalLayoutGroup>();
                hlg.childControlWidth = true;
                hlg.childControlHeight = false;
                hlg.childForceExpandHeight = false;
                hlg.childForceExpandWidth = false;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                
                layoutGO.transform.SetAsFirstSibling();
                //Mod.Log.Debug?.Write("ADDED CONTENT FITTER BLOCK");

                GameObject textBlock1 = new GameObject();
                textBlock1.transform.parent = hlg.transform;
                textBlock1.name = ModConsts.GO_Crew_Size;
                textBlock1.SetActive(true);

                LayoutElement le1 = textBlock1.AddComponent<LayoutElement>();
                le1.preferredHeight = 60f;
                le1.preferredWidth = 180f;

                LocalizableText lt1 = textBlock1.AddComponent<LocalizableText>();
                lt1.fontSize = 18f;
                lt1.fontSizeMin = 18f;
                lt1.fontSizeMax = 18f;
                lt1.alignment = TextAlignmentOptions.Left;
                lt1.enableAutoSizing = false;
                //Mod.Log.Debug?.Write("ADDED TEXTBLOCK1");

                portraitSlot.transform.SetAsFirstSibling();
            }
            return layoutGO;
        }
    }
}
