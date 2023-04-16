using HumanResources.Helper;
using HumanResources.Lifepath;
using IRBTModUtils.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace HumanResources
{
    public static class Mod
    {

        public const string HarmonyPackage = "us.frostraptor.HumanResources";
        public const string LogName = "human_resources";
        public const string LogSuffixDossier = "dossier";

        public static DeferringLogger Log;
        public static DeferringLogger DossierLog;

        public static string ModDir;
        public static ModConfig Config;
        public static ModCrewNames CrewNames;
        public static ModText LocalizedText;

        public static Dictionary<string, LifePathFamily> LifePathFamilies = new Dictionary<string, LifePathFamily>();
        public static Dictionary<string, LifePath> LifePaths = new Dictionary<string, LifePath>();

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON)
        {
            ModDir = modDirectory;

            Exception settingsE = null;
            try
            {
                Mod.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            }
            catch (Exception e)
            {
                settingsE = e;
                Mod.Config = new ModConfig();
            }
            Mod.Config.Init(); // Initialize color conversion & defaults

            Log = new DeferringLogger(modDirectory, LogName, "HR", Mod.Config.Debug, Mod.Config.Trace);
            DossierLog = new DeferringLogger(modDirectory, $"{LogName}.{LogSuffixDossier}", "HR", Mod.Config.Debug, Mod.Config.Trace);

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info?.Write($"Assembly version: {fvi.ProductVersion}");

            // Read config
            Log.Debug?.Write($"ModDir is:{modDirectory}");
            Log.Debug?.Write($"mod.json settings are:({settingsJSON})");
            Mod.Config.LogConfig();
            if (settingsE != null)
                Log.Info?.Write($"ERROR reading settings file! Error was: {settingsE}");
            else
                Log.Info?.Write($"INFO: No errors reading settings file.");

            // Read crew name dictionary
            string namesPath = Path.Combine(ModDir, "./mod_names.json");
            try
            {
                string jsonS = File.ReadAllText(namesPath);
                Mod.CrewNames = JsonConvert.DeserializeObject<ModCrewNames>(jsonS);
                Log.Info?.Write($"Successfully read:" +
                    $" {Mod.CrewNames.Aerospace?.Count} aerospace crew names" +
                    $" {Mod.CrewNames.MechTech?.Count} mechtech crew names" +
                    $" {Mod.CrewNames.MedTech?.Count} medtech crew names" +
                    $" {Mod.CrewNames.Vehicle?.Count} vehicle crew names"
                    );
            }
            catch (Exception e)
            {
                Mod.CrewNames = new ModCrewNames();
                Log.Error?.Write(e, $"Failed to read names from: {namesPath} due to error!");
            }

            // Read localization
            string localizationPath = Path.Combine(ModDir, "./mod_localized_text.json");
            try
            {
                string jsonS = File.ReadAllText(localizationPath);
                Mod.LocalizedText = JsonConvert.DeserializeObject<ModText>(jsonS);
                Log.Info?.Write("Successfully read mod localization files.");
            }
            catch (Exception e)
            {
                Mod.LocalizedText = new ModText();
                Log.Error?.Write(e, $"Failed to read localizations from: {localizationPath} due to error!");
            }

            // Read lifepaths
            string lifepathsPath = Path.Combine(ModDir, "./lifepaths.json");
            try
            {
                string jsonS = File.ReadAllText(lifepathsPath);
                Mod.LifePathFamilies = JsonConvert.DeserializeObject<Dictionary<string, LifePathFamily>>(jsonS);
                Log.Info?.Write($"Successfully read {Mod.LifePaths?.Count} lifepath families.");

                LifePathHelper.InitAtModLoad();
            }
            catch (Exception e)
            {
                Mod.LocalizedText = new ModText();
                Log.Error?.Write(e, $"Failed to read lifepaths from: {lifepathsPath} due to error!");
            }

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), HarmonyPackage);
        }

    }
}
