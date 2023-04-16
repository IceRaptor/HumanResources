using HumanResources.Crew;
using System;
using System.Collections.Generic;

namespace HumanResources.Helper
{
    public static class PlanetExtensions
    {
        public static CrewTagModifier GetPlanetSkillDistModifier(this StarSystem currentSystem)
        {
            return GetPlanetSkillModifier(currentSystem, Mod.Config.HiringHall.SkillDistribution.PlanetTagModifiers);
        }
        public static CrewTagModifier GetPlanetSizeDistModifier(this StarSystem currentSystem)
        {
            return GetPlanetSkillModifier(currentSystem, Mod.Config.HiringHall.SizeDistribution.PlanetTagModifiers);
        }

        private static CrewTagModifier GetPlanetSkillModifier(StarSystem currentSystem, Dictionary<string, CrewTagModifier> modifiers)
        {
            CrewTagModifier tagModifiers = new CrewTagModifier();
            foreach (string tag in currentSystem.Tags)
            {
                modifiers.TryGetValue(tag, out CrewTagModifier modifier);
                if (modifier != null)
                {
                    Mod.Log.Debug?.Write($"  tag: {tag} has scarcity =>  " +
                        $"aerospace: {modifier.Aerospace}  mechTechs: {modifier.MechTechs}  mechwarriors: {modifier.MechWarriors}  medTechs: {modifier.MedTechs}  vehicleCrews: {modifier.VehicleCrews}");
                    tagModifiers.Aerospace += modifier.Aerospace;
                    tagModifiers.MechWarriors += modifier.MechWarriors;
                    tagModifiers.MechTechs += modifier.MechTechs;
                    tagModifiers.MedTechs += modifier.MedTechs;
                    tagModifiers.VehicleCrews += modifier.VehicleCrews;
                }
            }

            return tagModifiers;
        }

        public static PlanetScarcity GetScarcityForPlanet(this StarSystem currentSystem)
        {
            float aerospaceUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.Aerospace;
            float mechTechsUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.MechTechs;
            float mechWarriorsUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.MechWarriors;
            float medTechsUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.MedTechs;
            float vehicleCrewsUpperBound = Mod.Config.HiringHall.Scarcity.Defaults.VehicleCrews;

            foreach (string tag in currentSystem.Tags)
            {
                Mod.Config.HiringHall.Scarcity.PlanetTagModifiers.TryGetValue(tag, out CrewTagModifier modifier);
                if (modifier != null)
                {
                    Mod.Log.Debug?.Write($"  tag: {tag} has scarcity =>  " +
                        $"aerospace: {modifier.Aerospace}  mechTechs: {modifier.MechTechs}  mechwarriors: {modifier.MechWarriors}  medTechs: {modifier.MedTechs}  vehicleCrews: {modifier.VehicleCrews}");
                    aerospaceUpperBound += modifier.Aerospace;
                    mechWarriorsUpperBound += modifier.MechWarriors;
                    mechTechsUpperBound += modifier.MechTechs;
                    medTechsUpperBound += modifier.MedTechs;
                    vehicleCrewsUpperBound += modifier.VehicleCrews;
                }
            }

            // Ceiling everything
            aerospaceUpperBound = (float)Math.Ceiling(aerospaceUpperBound);
            mechTechsUpperBound = (float)Math.Ceiling(mechTechsUpperBound);
            mechWarriorsUpperBound = (float)Math.Ceiling(mechWarriorsUpperBound);
            medTechsUpperBound = (float)Math.Ceiling(medTechsUpperBound);
            vehicleCrewsUpperBound = (float)Math.Ceiling(vehicleCrewsUpperBound);

            PlanetScarcity pilotScarcity = new PlanetScarcity();

            if (aerospaceUpperBound > 0)
            {
                int aerospaceLowerBound = (int)Math.Max(0, aerospaceUpperBound / 2);
                pilotScarcity.Aerospace = (aerospaceLowerBound, (int)aerospaceUpperBound);
            }

            if (mechTechsUpperBound > 0)
            {
                int mechTechLowerBound = (int)Math.Max(0, mechTechsUpperBound / 2);
                pilotScarcity.MechTechs = (mechTechLowerBound, (int)mechTechsUpperBound);
            }

            if (mechWarriorsUpperBound > 0)
            {
                int mechWarriorsLowerBound = (int)Math.Max(0, mechWarriorsUpperBound / 2);
                pilotScarcity.MechWarriors = (mechWarriorsLowerBound, (int)mechWarriorsUpperBound);
            }

            if (medTechsUpperBound > 0)
            {
                int medTechsLowerBound = (int)Math.Max(0, medTechsUpperBound / 2);
                pilotScarcity.MedTechs = (medTechsLowerBound, (int)medTechsUpperBound);
            }

            if (vehicleCrewsUpperBound > 0)
            {
                int vehicleCrewsLowerBound = (int)Math.Max(0, vehicleCrewsUpperBound / 2);
                pilotScarcity.Vehicles = (vehicleCrewsLowerBound, (int)vehicleCrewsUpperBound);
            }

            Mod.Log.Debug?.Write($"Planet: {currentSystem.Name} has final bounds:");
            Mod.Log.Debug?.Write($"  mechwarriors:  {pilotScarcity.MechWarriors.Lower} to {pilotScarcity.MechWarriors.Upper}");
            Mod.Log.Debug?.Write($"  vehicles:      {pilotScarcity.Vehicles.Lower} to {pilotScarcity.Vehicles.Upper}");
            Mod.Log.Debug?.Write($"  aerospace:     {pilotScarcity.Aerospace.Lower} to {pilotScarcity.Aerospace.Upper}");
            Mod.Log.Debug?.Write($"  mechTechs:     {pilotScarcity.MechTechs.Lower} to {pilotScarcity.MechTechs.Upper}");
            Mod.Log.Debug?.Write($"  medTechs:      {pilotScarcity.MedTechs.Lower} to {pilotScarcity.MedTechs.Upper}");

            return pilotScarcity;
        }
    }
}
