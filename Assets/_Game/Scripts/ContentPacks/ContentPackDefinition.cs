using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.ContentPacks
{
    [CreateAssetMenu(fileName = "ContentPackDefinition", menuName = "Isekai 12 Realms/Content Pack")]
    public class ContentPackDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ContentPackType packType;
        public int version = 1;
        public bool required;
        public bool includedInBuild;
        public bool downloadable;
        public long estimatedSizeBytes;
        public List<string> assetIds = new List<string>();
        public List<string> realmIds = new List<string>();
        public List<string> stageIds = new List<string>();
        public List<string> cosmeticIds = new List<string>();
    }
}
