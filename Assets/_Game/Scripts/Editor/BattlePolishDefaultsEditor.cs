using System.IO;
using Isekai12Realms.Audio;
using Isekai12Realms.Battle;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor
{
    public static class BattlePolishDefaultsEditor
    {
        private const string SettingsPath = "Assets/_Game/ScriptableObjects/Economy/BattleAnimationSettings.asset";

        [MenuItem("Tools/Isekai 12 Realms/Create Battle Polish Defaults")]
        public static void CreateDefaults()
        {
            EnsureFolder("Assets/_Game/Scripts/VFX");
            EnsureFolder("Assets/_Game/Scripts/Audio");
            EnsureFolder("Assets/_Game/Prefabs/VFX");
            EnsureFolder("Assets/_Game/Prefabs/UI/Battle");
            EnsureFolder("Assets/_Game/ScriptableObjects/Economy");

            BattleAnimationSettings settings = AssetDatabase.LoadAssetAtPath<BattleAnimationSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<BattleAnimationSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            string audioPrefabPath = "Assets/_Game/Prefabs/UI/Battle/AudioService.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(audioPrefabPath) == null)
            {
                GameObject go = new GameObject("AudioService", typeof(AudioService));
                PrefabUtility.SaveAsPrefabAsset(go, audioPrefabPath);
                Object.DestroyImmediate(go);
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BattlePolish] Defaults created/verified.");
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
