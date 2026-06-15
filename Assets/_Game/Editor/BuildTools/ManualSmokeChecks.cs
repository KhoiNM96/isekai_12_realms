using Isekai12Realms.Character;
using Isekai12Realms.Data;
using Isekai12Realms.Equipment;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor.BuildTools
{
    public static class ManualSmokeChecks
    {
        [MenuItem("Tools/Isekai 12 Realms/QA/Run Manual Smoke Checks")]
        public static void RunManualSmokeChecks()
        {
            int pass = 0;
            int fail = 0;
            Check("Build validation has no critical errors", !BuildValidator.Validate(false).HasCriticalErrors, ref pass, ref fail);
            SaveService saveService = new SaveService();
            saveService.LoadOrCreateSave();
            Check("SaveService creates save", saveService.CurrentSave != null, ref pass, ref fail);
            int gold = saveService.CurrentSave != null ? saveService.CurrentSave.gold : 0;
            if (saveService.CurrentSave != null) { saveService.CurrentSave.gold += 10; saveService.SaveNow(); }
            Check("Player gains gold", saveService.CurrentSave != null && saveService.CurrentSave.gold >= gold + 10, ref pass, ref fail);
            GameContentDatabase db = AssetDatabase.LoadAssetAtPath<GameContentDatabase>(BuildValidator.DatabasePath);
            Check("Stage 1-1 exists", db != null && db.GetStageById("stage_01_01") != null, ref pass, ref fail);
            Check("Stage 1-2 exists", db != null && db.GetStageById("stage_01_02") != null, ref pass, ref fail);
            Check("Prototype skills exist", db != null && db.skills != null && db.skills.Count > 0, ref pass, ref fail);
            Check("Prototype equipment exists", db != null && db.equipmentDefinitions != null && db.equipmentDefinitions.Count > 0, ref pass, ref fail);
            Check("Prototype shop exists", db != null && db.shops != null && db.shops.Count > 0, ref pass, ref fail);
            Debug.Log($"[QA] Manual smoke checks complete. PASS={pass} FAIL={fail}");
        }

        private static void Check(string label, bool condition, ref int pass, ref int fail)
        {
            if (condition) { pass++; Debug.Log("[QA] PASS: " + label); }
            else { fail++; Debug.LogError("[QA] FAIL: " + label); }
        }
    }
}
