
using BattleTech;
using System.Collections.Generic;

namespace PitCrew
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

        public static void Reset()
        {
            // Reinitialize state
            SimGameState = null;
            PilotCreate = new PilotCreateState();
        }
    }

}


