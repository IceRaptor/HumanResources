﻿using HoudiniEngineUnity;
using System.Collections.Generic;
using UnityEngine;

namespace HumanResources
{

    public class DistributionOpts
    {
        // Defines how broad the grouping is, which influences the height of the curve.
        public int Sigma = 1;
        // Where on the number line the curve is centered, i.e. the tallest point of the curve
        public int Mu = 0;

        // Breakpoints on the PDF (Probability distribution function), from worst to best. Must be 4 values.
        public float[] Breakpoints = new float[] { -1.0f, 1.0f, 2.0f, 3.0f };
    }

    public class CrewOpts
    {
        public bool Enabled = true;

        public int BaseDaysInContract = 15;
        public int MinContractDaysMulti = 6;
        public int MaxContractDaysMulti = 12;

        // ab^x where a = multiplier, b = exponent, x = skill level
        public int SalaryMulti = 30000;
        public float SalaryExponent = 1.1f;
        
        // Salary variance is up or down; if salary is 10,000 variance will calculate from 9,500 to 10,500
        public float SalaryVariance = 0.05f;
        // Bonus variance is always above the salary range; 10,000 salary will be between 10,000 and 13,000
        public float BonusVariance = 1.3f;

        // -1 indicates no limit
        public float MaxOfType = -1;

        // Change that the mercenary has a specific factional loyalty
        public float FactionLoyaltyChance = 0.3f;
    }

    public class CrewScarcity
    {
        public float Aerospace = 0f;
        public float MechTechs = 0f;
        public float MechWarriors = 0f;
        public float MedTechs = 0f;
        public float VehicleCrews = 0f;
    }

    public class ScarcityOps
    {
        public bool Enabled = true;

        public CrewScarcity Defaults = new CrewScarcity();

        public Dictionary<string, CrewScarcity> PlanetTagModifiers = new Dictionary<string, CrewScarcity>()
        {
            {  "planet_civ_innersphere",
                new CrewScarcity() { MechWarriors = 1f, VehicleCrews = 2f, MechTechs = 1f, MedTechs = 1f, Aerospace = 1f } },
            {  "planet_civ_periphery",
                new CrewScarcity() { MechWarriors = 1f, VehicleCrews = 1f, MechTechs = 0f, MedTechs = 0f, Aerospace = 1f } },
            {  "planet_civ_primitive",
                new CrewScarcity() { MechWarriors = -2f, VehicleCrews = 1f, MechTechs = -2f, MedTechs = -4f, Aerospace = -4f } }
        };
    }

    public class HiringHall
    {
        public DistributionOpts SkillDistribution = new DistributionOpts() 
        {
            // Rookie, Regular, Veteran, Elite, Legendary
            Breakpoints = new float[] { -1.5f, 1.5f, 2f, 2.5f  }
        };
        
        public DistributionOpts SizeDistribution = new DistributionOpts()
        {
            // Tiny, Small, Medium, Large, Huge
            Breakpoints = new float[] { -1f, 0.5f, 1.25f, 2f }
        };

        public ScarcityOps Scarcity = new ScarcityOps();
        
        public PointsBySkillAndSizeOpts PointsBySkillAndSize = new PointsBySkillAndSizeOpts();

        public CrewOpts AerospaceWings = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = 1,
            FactionLoyaltyChance = 0.3f
        };

        public CrewOpts MechTechCrews = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = -1,
            FactionLoyaltyChance = 0.3f
        };

        public CrewOpts MedTechCrews = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = 2,
            FactionLoyaltyChance = 0.3f
        };

        public CrewOpts MechWarriors = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = -1,
            FactionLoyaltyChance = 0.3f
        };

        public CrewOpts VehicleCrews = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = -1,
            FactionLoyaltyChance = 0.3f
        };

    }

    public class PointsBySkillAndSizeOpts
    {

        public int[][] Aerospace = new int[][]
{
            new int[] { 1, 2, 3, 5, 8 },
            new int[] { 2, 4, 6, 10, 16 },
            new int[] { 3, 6, 9, 15, 24 },
            new int[] { 4, 8, 12, 20, 32 },
            new int[] { 5, 10, 15, 25, 40 }
        };

        public int[][] MechTech = new int[][]
        {
            new int[] { 1, 2, 3, 5, 8 },
            new int[] { 2, 4, 6, 10, 16 },
            new int[] { 3, 6, 9, 15, 24 },
            new int[] { 4, 8, 12, 20, 32 },
            new int[] { 5, 10, 15, 25, 40 }
        };

        public int[][] MedTech = new int[][]
        {
            new int[] { 1, 2, 3, 5, 8 },
            new int[] { 2, 4, 6, 10, 16 },
            new int[] { 3, 6, 9, 15, 24 },
            new int[] { 4, 8, 12, 20, 32 },
            new int[] { 5, 10, 15, 25, 40 }
        };
    }

    public class Poaching
    {
        public bool EnablePoaching = true;

        public float[] LifeStyleMods = { 0.35f, 0.2f, 0f, -0.2f, -0.35f };
        public float[] PoachingChance = { 0.05f, 0.1f, 0.15f, 0.3f, 0.6f };
    }

    public class CrewCfg
    {
        public float[] AerospaceRGB = { 0.376f, 0.533f, 0.604f };
        public Color AerospaceColor = Color.cyan;

        public float[] MechTechCrewRGB = { 0.808f, 0.71f, 0.278f };
        public Color MechTechCrewColor = Color.yellow;

        public float[] MedTechCrewRGB = { 0.871f, 0.278f, 0.216f };
        public Color MedTechCrewColor = Color.red;

        public float[] VehicleCrewRGB = { 0.486f, 0.745f, 0.525f };
        public Color VehicleCrewColor = Color.green;

        public float[] MechwarriorRGB = { 0.376f, 0.533f, 0.604f };
        public Color MechwarriorColor = Color.blue;

    }

    public class Icons
    {
        public string CrewPortrait_Aerospace = "hr_jet-fighter";
        public string CrewPortrait_MedTech = "hr_hospital-cross";
        public string CrewPortrait_MechTech = "hr_auto-repair";
        public string CrewPortrait_Vehicle = "hr_apc";
    }

    public class ModConfig
    {
        public bool Debug = false;
        public bool Trace = false;

        public bool DebugCommands = true;

        public Icons Icons = new Icons();

        public CrewCfg Crew = new CrewCfg();
        public HiringHall HiringHall = new HiringHall();
        public Poaching Poaching = new Poaching();

        public void LogConfig()
        {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG: {this.Debug} Trace: {this.Trace}");
            Mod.Log.Info?.Write("=== MOD CONFIG END ===");
        }

        public void Init()
        {
            if (this.Crew.MechTechCrewRGB != null && this.Crew.MechTechCrewRGB.Length == 3)
            {
                this.Crew.MechTechCrewColor = new Color(this.Crew.MechTechCrewRGB[0], this.Crew.MechTechCrewRGB[1], this.Crew.MechTechCrewRGB[2]);
            }
            if (this.Crew.MedTechCrewRGB != null && this.Crew.MedTechCrewRGB.Length == 3)
            {
                this.Crew.MedTechCrewColor = new Color(this.Crew.MedTechCrewRGB[0], this.Crew.MedTechCrewRGB[1], this.Crew.MedTechCrewRGB[2]);
            }
            if (this.Crew.VehicleCrewRGB != null && this.Crew.VehicleCrewRGB.Length == 3)
            {
                this.Crew.VehicleCrewColor = new Color(this.Crew.VehicleCrewRGB[0], this.Crew.VehicleCrewRGB[1], this.Crew.VehicleCrewRGB[2]);
            }
            if (this.Crew.MechwarriorRGB != null && this.Crew.MechwarriorRGB.Length == 3)
            {
                this.Crew.MechwarriorColor = new Color(this.Crew.MechwarriorRGB[0], this.Crew.MechwarriorRGB[1], this.Crew.MechwarriorRGB[2]);
            }
            if (this.Crew.AerospaceRGB!= null && this.Crew.AerospaceRGB.Length == 3)
            {
                this.Crew.AerospaceColor = new Color(this.Crew.AerospaceRGB[0], this.Crew.AerospaceRGB[1], this.Crew.AerospaceRGB[2]);
            }

            // TODO: Validate the hiring hall distributions

        }
    }
}

