
using BattleTech;
using HumanResources.Crew;
using HumanResources.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using us.frostraptor.modUtils.Redzen;

namespace HumanResources
{

    public class CrewCreateState
    {
        public PilotNameGenerator NameGenerator = null;

        public List<LifepathNodeDef> LifePaths = null;
        public List<LifepathNodeDef> StartingPaths = null;
        public List<LifepathNodeDef> AdvancePaths = null;

        // public static Dictionary<string, LifePathFamily> LifePaths = new Dictionary<string, LifePathFamily>();
        public Dictionary<string, (int, int)> LifePathFamilyBounds = new Dictionary<string, (int, int)>();
        public Dictionary<string, Dictionary<string, (int, int)>> LifePathBounds = new Dictionary<string, Dictionary<string, (int, int)>>();

        public GenderedOptionsListDef Voices = null;

        public List<int> GenderWeights = null;
        public readonly Gender[] Genders = { Gender.Female, Gender.Male, Gender.NonBinary };

        public PilotGenerator HBSPilotGenerator;
    }

    public static class ModState
    {
        public static CrewCreateState CrewCreateState = new CrewCreateState();
        public static SimGameState SimGameState = null;
        public static bool IsHiringFlow = false;

        // Event-based objects
        public static Queue<(Pilot Pilot, CrewDetails Details)> ExpiredContracts = new Queue<(Pilot, CrewDetails)>();
        public static Pilot HeadHuntedPilot = null;

        private static Dictionary<string, CrewDetails> crewDetailsCache = new Dictionary<string, CrewDetails>();

        public static void Reset()
        {
            // Reinitialize state
            Mod.Log.Info?.Write("CLEARING ALL MOD STATE DATA");
            SimGameState = null;
            CrewCreateState = new CrewCreateState();
            
            ExpiredContracts.Clear();
            HeadHuntedPilot = null;

            IsHiringFlow = false;
        }

        // --- Methods manipulating CheckResults
        public static double[] InitializeCheckResults(double mean, double stdDev, int arraySize)
        {
            Mod.Log.Debug?.Write($"Initializing a new random buffer of size: {arraySize} ");
            Xoshiro256PlusRandomBuilder builder = new Xoshiro256PlusRandomBuilder();
            IRandomSource rng = builder.Create();
            double[] GaussianResults = new double[arraySize];
            ZigguratGaussian.Sample(rng, mean, stdDev, GaussianResults);
            Mod.Log.Debug?.Write("Initialization complete.");
            return GaussianResults;
        }

        public static CrewDetails GetCrewDetails(PilotDef pilotDef)
        {
            if (pilotDef == null) return null;

            string guid = null;
            // Check the pilotDef for an existing guid
            Mod.Log.Trace?.Write($"===== GetCrewDetails: Checking pilot tags for GUID");
            foreach (string tag in pilotDef.PilotTags)
            {
                if (tag.StartsWith(ModTags.Tag_GUID))
                {
                    guid = tag.Substring(ModTags.Tag_GUID.Length);
                    Mod.Log.Trace?.Write($" -- found GUID: {guid} from tag.");
                }
            }

            // If we cannot find a GUID, assume we're a vanilla MechWarrior and generate a new CrewDetails
            if (guid == null)
            {
                Mod.Log.Warn?.Write($"Failed to find crew details for pilotDef - creating a new one.");
                CrewDetails newDetails = CrewGenerator.GenerateDetailsForVanillaMechwarrior(pilotDef);
                crewDetailsCache[newDetails.GUID] = newDetails;
                return newDetails;
            }
            else
            {
                Mod.Log.Trace?.Write($"Reading crew details using GUID from tag: {guid}");
            }

            bool hasKey = crewDetailsCache.TryGetValue(guid, out CrewDetails details);
            if (!hasKey)
            {
                // Doesn't exist in cache, read from the company stat
                string companyStatName = ModStats.Company_CrewDetail_Prefix + guid;
                Mod.Log.Trace?.Write($"Trying to read companyStatName: {companyStatName}");

                Statistic detailsCompanyStat = ModState.SimGameState.CompanyStats.GetStatistic(companyStatName);
                if (detailsCompanyStat == null)
                {
                    Mod.Log.Warn?.Write($"Failed to read GUID {guid} from companyStat: {companyStatName}. This should never happen!");
                    Mod.Log.Warn?.Write($"Iterating company crew stats");
                    foreach (KeyValuePair<string, Statistic> kvp in ModState.SimGameState.CompanyStats)
                    {
                        if (kvp.Key.StartsWith(ModStats.Company_CrewDetail_Prefix))
                        {
                            Mod.Log.Debug?.Write($"-- Found stat: {kvp.Key}");
                            Mod.Log.Debug?.Write($"--       '{kvp.Value.Value<string>()}'");
                        }
                    }
                    return null;
                }

                string statVal = detailsCompanyStat.Value<string>();
                Mod.Log.Trace?.Write($"Read companyStat value as: {statVal}");

                try
                {
                    details = JsonConvert.DeserializeObject<CrewDetails>(statVal);
                    Mod.Log.Trace?.Write($"Fetched details from companyStats serialization: {details}.");
                }
                catch (Exception e)
                {
                    Mod.Log.Error?.Write(e, "Failed to deserialize json from companyStat");
                }

                // Add to cache
                crewDetailsCache[guid] = details;
            }
            else
            {
                Mod.Log.Trace?.Write($"Found cached details value: {details}");
            }

            // Check for an empty SalaryCfg; if empty, default to the mod settings
            if (details.SalaryConfig == null || details.SalaryConfig.IsDefault())
                details.SalaryConfig = SalaryConfig.FromModConfig(details.Type);

            return details;
        }

        public static CrewDetails UpdateOrCreateCrewDetails(PilotDef pilotDef, CrewDetails newDetails)
        {
            if (pilotDef == null || newDetails == null) return null;

            string guid = null;
            // Check the pilotDef for an existing guid
            Mod.Log.Trace?.Write($"===== UpdateOrCreate: Checking pilot tags for GUID");
            foreach (string tag in pilotDef.PilotTags)
            {
                if (tag.StartsWith(ModTags.Tag_GUID))
                {
                    guid = tag.Substring(ModTags.Tag_GUID.Length);
                    Mod.Log.Trace?.Write($" -- found GUID: {guid} from tag");
                    break;
                }
            }

            // PilotDef has no existing GUID, so we need to create a new one.
            if (guid != null && String.IsNullOrEmpty(newDetails.GUID))
            {
                newDetails.GUID = guid;
                Mod.Log.Debug?.Write($" -- no GUID found in updated details, setting guid from tag: {guid}");
            }
            else if (guid != null && !String.Equals(guid, newDetails.GUID, StringComparison.InvariantCultureIgnoreCase))
            {
                // Something is seriously fucked up; fail fast and make a mess
                Mod.Log.Error?.Write($"Pilot has GUID: {guid} but asked to be updated with GUID: {newDetails.GUID}. " +
                    $"Can't process this inconsisteny, failing!");
                return null;
            }
            else if (guid == null && String.IsNullOrEmpty(newDetails.GUID))
            {
                newDetails.GUID = Guid.NewGuid().ToString();
                guid = newDetails.GUID;
                Mod.Log.Debug?.Write($" -- no GUID found for pilotDef in tag or details, adding new details GUID: {newDetails.GUID}");
            }
            if (guid == null && !String.IsNullOrEmpty(newDetails.GUID))
            {               
                guid = newDetails.GUID;
                Mod.Log.Debug?.Write($" -- no GUID found for pilotDef in tags, using new details GUID: {newDetails.GUID}");
            }
            else
            {
                Mod.Log.Trace?.Write($" -- pilot has existing details GUID: {guid}");
            }

            // Make sure the pilotDef gets the linking tag
            if (!pilotDef.PilotTags.Contains(guid))
                pilotDef.PilotTags.Add($"{ModTags.Tag_GUID}{guid}");

            // Update attitude tags on pilot
            newDetails.UpdateAttitudeTags(pilotDef);

            // Check for an empty SalaryCfg; if empty, default to the mod settings
            if (newDetails.SalaryConfig == null || newDetails.SalaryConfig.IsDefault())
                newDetails.SalaryConfig = SalaryConfig.FromModConfig(newDetails.Type);

            // Write the new data to the company stats for it
            string companyStatName = ModStats.Company_CrewDetail_Prefix + guid;
            //Mod.Log.Debug?.Write($"Trying to read companyStat: {companyStatName}");
            Statistic detailsCompanyStat = ModState.SimGameState.CompanyStats.GetStatistic(companyStatName);
            if (detailsCompanyStat == null)
                detailsCompanyStat = ModState.SimGameState.CompanyStats.AddStatistic<string>(companyStatName, "");

            string serializedDetails = JsonConvert.SerializeObject(newDetails);
            detailsCompanyStat.SetValue<string>(serializedDetails);
            //Mod.Log.Debug?.Write($"CompanyStat: {companyStatName} updated with new CrewDetails: {serializedDetails}.");

            // Update the cache
            crewDetailsCache[guid] = newDetails;

            return newDetails;
        }

        public static void RemoveCrewDetails(PilotDef pilotDef, CrewDetails newDetails)
        {
            if (pilotDef == null || newDetails == null) return;

            Mod.Log.Info?.Write($"Deleting crew details for pilotDef with guid: {pilotDef?.GUID}");

            string guid = null;
            // Check the pilotDef for an existing guid
            Mod.Log.Debug?.Write($"Deleting  pilot tags for GUID in UpdateOrCreate");
            foreach (string tag in pilotDef.PilotTags)
            {
                if (tag.StartsWith(ModTags.Tag_GUID))
                {
                    guid = tag.Substring(ModTags.Tag_GUID.Length);
                    break;
                }
            }

            if (guid != null)
            {
                Mod.Log.Info?.Write($" -- removing GUID: {guid}");
                string companyStatName = ModStats.Company_CrewDetail_Prefix + guid;
                Statistic detailsCompanyStat = ModState.SimGameState.CompanyStats.GetStatistic(companyStatName);
                if (detailsCompanyStat != null)
                    ModState.SimGameState.CompanyStats.RemoveStatistic(companyStatName);
                pilotDef.PilotTags.Remove($"{ModTags.Tag_GUID}{newDetails.GUID}");
            }

            newDetails.UpdateAttitudeTags(pilotDef);

            // Delete all tags in the mod
            pilotDef.PilotTags.RemoveRange(ModTags.Tags_All);

        }

    }

}


