using System.Collections.Generic;
using JxModule.DataTable;
using UnityEngine;

namespace Ddalgak
{
    public sealed class EventRepository : EventRepositoryBase
    {
        private readonly List<EventData> _events = new();
        private bool _isLoaded;

        public override IReadOnlyList<EventData> GetAllEvents()
        {
            if (!_isLoaded)
            {
                LoadDataTables();
            }

            return _events;
        }

        [ContextMenu("Reload DataTables")]
        public void ReloadDataTables()
        {
            _isLoaded = false;
            _events.Clear();
            LoadDataTables();
        }

        private void LoadDataTables()
        {
            _events.Clear();

            Dictionary<string, ResultDataTableRow> results = LoadResults();
            Dictionary<string, ButtonAction> actions = LoadActions();
            Dictionary<string, ChoiceData> choices = LoadChoices(results, actions);
            List<EventDataTableRow> eventRows = DataTableManager.FindAllRows<EventDataTableRow>();

            foreach (EventDataTableRow row in eventRows)
            {
                if (row == null || !row.isEnable)
                {
                    continue;
                }

                _events.Add(row.ToRuntimeData(choices, actions, results));
            }

            _isLoaded = true;
            Debug.Log($"[GameFlow] Loaded {_events.Count} events from DataTable.");
        }

        private static Dictionary<string, ButtonAction> LoadActions()
        {
            Dictionary<string, ButtonAction> result = new();
            foreach (ButtonActionDataTableRow row in DataTableManager.FindAllRows<ButtonActionDataTableRow>())
            {
                if (row != null && row.isEnable)
                {
                    result[row.rowID] = row.ToRuntimeData();
                }
            }

            return result;
        }

        private static Dictionary<string, ResultDataTableRow> LoadResults()
        {
            Dictionary<string, ResultDataTableRow> result = new();
            foreach (ResultDataTableRow row in DataTableManager.FindAllRows<ResultDataTableRow>())
            {
                if (row != null && row.isEnable)
                {
                    result[row.rowID] = row;
                }
            }

            return result;
        }

        private static Dictionary<string, ChoiceData> LoadChoices(
            IReadOnlyDictionary<string, ResultDataTableRow> results,
            IReadOnlyDictionary<string, ButtonAction> actions)
        {
            Dictionary<string, ChoiceData> result = new();
            foreach (ChoiceDataTableRow row in DataTableManager.FindAllRows<ChoiceDataTableRow>())
            {
                if (row != null && row.isEnable)
                {
                    result[row.rowID] = row.ToRuntimeData(results, actions);
                }
            }

            return result;
        }
    }
}
