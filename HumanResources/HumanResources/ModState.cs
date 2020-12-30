
using BattleTech;
using HumanResources.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using us.frostraptor.modUtils.Redzen;

namespace HumanResources
{

    public class PilotCreateState
    {
        public PilotNameGenerator NameGenerator = null;

        public List<LifepathNodeDef> LifePaths = null;
        public List<LifepathNodeDef> StartingPaths = null;
        public List<LifepathNodeDef> AdvancePaths = null;

        public GenderedOptionsListDef Voices = null;

        public List<int> GenderWeights = null;
        public readonly Gender[] Genders = { Gender.Female, Gender.Male, Gender.NonBinary };
    }

    public static class ModState
    {
        public static PilotCreateState PilotCreate = new PilotCreateState();
        public static SimGameState SimGameState = null;
        public static Queue<(Pilot Pilot, CrewDetails Details)> ExpiredContracts = 
            new Queue<(Pilot, CrewDetails)>();
        public static bool IsHiringFlow = false;

        private static Dictionary<string, CrewDetails> crewDetailsCache = new Dictionary<string, CrewDetails>();

        public static void Reset()
        {
            // Reinitialize state
            Mod.Log.Info?.Write("CLEARING ALL MOD STATE DATA");
            SimGameState = null;
            PilotCreate = new PilotCreateState();
            ExpiredContracts.Clear();
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
            //Mod.Log.Debug?.Write($"Checking pilot tags for GUID in Get");
            foreach (string tag in pilotDef.PilotTags)
            {
                if (tag.StartsWith(ModTags.Tag_GUID))
                {
                    guid = tag.Substring(ModTags.Tag_GUID.Length);
                }
            }

            // If we cannot find a GUID, assume we're a vanilla MechWarrior and generate a new CrewDetails
            if (guid == null)
            {
                CrewDetails newDetails = new CrewDetails(pilotDef, CrewType.MechWarrior);
                crewDetailsCache[newDetails.GUID] = newDetails;
                Mod.Log.Warn?.Write($"Failed to find GUID for pilotDef: {pilotDef} - creating a new default one for mechwarrior");
                UpdateOrCreateCrewDetails(pilotDef, newDetails);
                return newDetails;
            }
            //else
            //{
            //    Mod.Log.Debug?.Write($"Reading crew details using GUID from tag: {guid}");
            //}

            CrewDetails details;
            bool hasKey = crewDetailsCache.TryGetValue(guid, out details);
            if (!hasKey)
            {
                // Doesn't exist in cache, read from the company stat
                string companyStatName = ModStats.Company_CrewDetail_Prefix + guid;
                //Mod.Log.Debug?.Write($"Trying to read companyStatName: {companyStatName}");

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
                //Mod.Log.Debug?.Write($"Read companyStat value as: {statVal}");

                try
                {
                    details = JsonConvert.DeserializeObject<CrewDetails>(statVal);
                    //Mod.Log.Debug?.Write($"Fetched details from companyStats serialization: {details}.");
                }
                catch (Exception e)
                {
                    Mod.Log.Error?.Write(e, "Failed to deserialize json from companyStat");
                }

                // Add to cache
                crewDetailsCache[guid] = details;
            }
            //else
            //{
            //    Mod.Log.Debug?.Write($"Found cached details value: {details}");
            //}

            return details;
        }

        public static CrewDetails UpdateOrCreateCrewDetails(PilotDef pilotDef, CrewDetails newDetails)
        {
            if (pilotDef == null || newDetails == null) return null;

            string guid = null;
            // Check the pilotDef for an existing guid
            Mod.Log.Debug?.Write($"Checking pilot tags for GUID in UpdateOrCreate");
            foreach (string tag in pilotDef.PilotTags)
            {
                if (tag.StartsWith(ModTags.Tag_GUID))
                {
                    guid = tag.Substring(ModTags.Tag_GUID.Length);
                    break;
                }
            }

            // Generate a new guid if not present, and add it as a tag
            if (guid == null)
            {
                Mod.Log.Debug?.Write($" -- no GUID found for pilotDef, adding new details GUID: {newDetails.GUID}");
                pilotDef.PilotTags.Add($"{ModTags.Tag_GUID}{newDetails.GUID}");
            }
            else
            {
                Mod.Log.Debug?.Write($" -- pilot has existing details GUID: {guid}");
            }

            // TODO: Check for GUID mismatch in tags / details

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

    }

}


