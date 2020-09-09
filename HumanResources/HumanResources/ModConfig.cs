using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace PitCrew
{

    public class CrewScarcity
    {
        public int MechWarriors = 0;
        public int VehicleCrews = 0;
        public int MechTechs = 0;
        public int MedTechs = 0;
    }

    public class HiringHall
    {
        public float[] SkillDistribution = { 0.2f, 0.4f, 0.2f, 0.15f, 0.05f };
        public float[] SizeDistribution = { 0.1f, 0.2f, 0.4f, 0.2f, 0.1f };
        
        [JsonIgnore]
        public float rookieThreshold = 0f;
        [JsonIgnore]
        public float regularThreshold = 0f;
        [JsonIgnore]
        public float veteranThreshold = 0f;
        [JsonIgnore]        
        public float eliteThreshold = 0f;
        [JsonIgnore]
        public float legendaryThreshold = 0f;

        public bool EnableScarcity = true;
        public CrewScarcity DefaultScarcity = new CrewScarcity();
        public Dictionary<string, CrewScarcity> ScarcityByPlanetTag = new Dictionary<string, CrewScarcity>()
        {
            {  "planet_civ_innersphere",  new CrewScarcity() { MechWarriors = 2, VehicleCrews = 4, MechTechs = 1, MedTechs = 1 } },
            {  "planet_civ_periphery",  new CrewScarcity() { MechWarriors = 1, VehicleCrews = 4, MechTechs = 1, MedTechs = 1 } },
            {  "planet_civ_primitive",  new CrewScarcity() { MechWarriors = -2, VehicleCrews = 1, MechTechs = -2, MedTechs = -4 } }
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
        public float[] MechTechCrewRGB = { 0.808f, 0.71f, 0.278f };
        public Color MechTechCrewColor = Color.yellow;

        public float[] MedTechCrewRGB = { 0.871f, 0.278f, 0.216f };
        public Color MedTechCrewColor = Color.red;

        public float[] VehicleCrewRGB = { 0.486f, 0.745f, 0.525f };
        public Color VehicleCrewColor = Color.green;

        public float[] MechwarriorRGB = { 0.376f, 0.533f, 0.604f };
        public Color MechwarriorColor = Color.blue;
    }

    public class MonthlyCost
    {
        public float DefaultComponentCostMulti = 0.1f;
    }

    public class ArmorRepair
    {
        public float PointsPerTP = 6; // How many armor points count as one TP
        public int TonsPerTP = 20; // How many points to add by tonnage
        public int CBillsPerPoint = 60;
    }

    public class StructureRepair
    {
        public float PointsPerTP = 6;
        public int TonsPerTP = 5;
        public int CBillsPerPoint = 600;
    }

    public class TagModifiers
    {
        public float ArmorRepairTPMulti = 0f;
        public float ArmorRepairCBMulti = 0f;

        public float StructureRepairTPMulti = 0f;
        public float StructureRepairCBMulti = 0f;
    }

    public class Icons
    {
        public string MechWarriorButton = "pc_missile-mech";
        
        public string MechTechButton = "pc_apc";
        public string MedTechPortrait = "pc_auto-repair";
        
        public string MedTechButton = "pc_hospital-cross";
        public string MechTechPortait = "pc_auto-repair";

        public string GroupPortrait = "pc_three-friends";
    }

    public class ModConfig
    {

        public bool Debug = false;
        public bool Trace = false;

        public Icons Icons = new Icons();

        public CrewCfg Crew = new CrewCfg();
        public HiringHall HiringHall = new HiringHall();
        public Poaching Poaching = new Poaching();

        public MonthlyCost MonthlyCost = new MonthlyCost();
        public ArmorRepair ArmorRepair = new ArmorRepair();
        public StructureRepair StructureRepair = new StructureRepair();
        public Dictionary<string, TagModifiers> ChassisTagMods = new Dictionary<string, TagModifiers>();
        public Dictionary<string, TagModifiers> ComponentTagMods = new Dictionary<string, TagModifiers>();

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

            // TODO: Validate the hiring hall distributions

        }
    }
}

