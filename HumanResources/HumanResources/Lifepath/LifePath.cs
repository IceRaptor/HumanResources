using Localize;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HumanResources.Lifepath
{
    public class LifePathDescription
    {
        public string Title;
        public string Description;
    }

    public class LifePathFamily
    {
        public int Weight = 0;
        [JsonIgnore]
        public (int, int) WeightBounds = (0, 0);
        [JsonIgnore]
        public int WeightTotal = 0;
        
        public Dictionary<string, LifePath> Lifepaths;
    }

    public class LifePath
    {
        public int Weight = 0;
        [JsonIgnore]
        public (int, int) WeightBounds = (0, 0);

        public string DescriptionKey;
        public List<string> RequiredTags = new List<string>();
        public List<string> RandomTags = new List<string>();
        public LifePathSkillMod SkillMod;

        public LifePathDescription Description
        {
            get
            {
                LifePathDescription desc;
                Mod.LocalizedText.LifePathDescriptions.TryGetValue(DescriptionKey, out desc);
                if (desc == null)
                {
                    desc = new LifePathDescription
                    {
                        Title = "KEY_NOT_FOUND",
                        Description = "ERROR - REPORT!"
                    };
                }

                return desc;
            }
        }

    }

    public class LifePathSkillMod
    {
        public float MechWarrior = 0f;
        public float Vehicle = 0f;
        public float MechTech = 0f;
        public float MedTech = 0f;
    }
}
