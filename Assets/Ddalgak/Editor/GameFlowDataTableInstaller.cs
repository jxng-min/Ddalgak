#if UNITY_EDITOR
using System;
using System.Threading.Tasks;
using JxModule.DataTable;
using UnityEditor;
using UnityEngine;

namespace Ddalgak.Editor
{
    public static class GameFlowDataTableInstaller
    {
        private const string Root = "Assets/Resources/GameFlowDataTables";
        private const string InstallLog = "[GameFlow] DataTable assets updated.";
        private const int ExpectedTableCount = 4;

        [InitializeOnLoadMethod]
        private static void ScheduleInstall()
        {
            EditorApplication.delayCall += Install;
        }

        [MenuItem("Tools/Ddalgak/Update GameFlow DataTables")]
        public static async void Install()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            try
            {
                await UpdateTable<EventDataTableRow>("DT_Event");
                await UpdateTable<ChoiceDataTableRow>("DT_Choice");
                await UpdateTable<ResultDataTableRow>("DT_Result");
                await UpdateTable<ButtonActionDataTableRow>("DT_ButtonAction");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"{InstallLog} Count: {ExpectedTableCount}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameFlow] DataTable installation failed: {exception}");
            }
        }

        private static async Task UpdateTable<TRow>(string tableName)
            where TRow : DataTableRowBase
        {
            string csvPath = $"{Root}/{tableName}.csv";
            string assetPath = $"{Root}/{tableName}.asset";
            TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);
            if (csv == null)
            {
                throw new InvalidOperationException($"CSV not found: {csvPath}");
            }

            DataTable table = AssetDatabase.LoadAssetAtPath<DataTable>(assetPath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<DataTable>();
                AssetDatabase.CreateAsset(table, assetPath);
            }

            TRow temporaryRow = ScriptableObject.CreateInstance<TRow>();
            MonoScript rowScript = MonoScript.FromScriptableObject(temporaryRow);
            UnityEngine.Object.DestroyImmediate(temporaryRow);

            table.dataTableCsv = csv;
            table.dataTableRowScript = rowScript;
            table.name = tableName;
            EditorUtility.SetDirty(table);
            await table.UpdateData(csv, null);
        }
    }
}
#endif
