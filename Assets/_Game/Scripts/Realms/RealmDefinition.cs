using System.Collections.Generic;
using Isekai12Realms.Stages;
using UnityEngine;

namespace Isekai12Realms.Realms
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Realm Definition")]
    public class RealmDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
        public int order;
        public string backgroundAssetId;
        public List<StageDefinition> stages = new List<StageDefinition>();
    }
}
