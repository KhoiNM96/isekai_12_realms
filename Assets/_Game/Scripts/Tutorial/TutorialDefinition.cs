using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Tutorial
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Tutorial Definition")]
    public class TutorialDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public List<TutorialStepData> steps = new List<TutorialStepData>();
        public bool skippable = true;
        public bool autoStart = true;
    }
}
