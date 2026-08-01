using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ddalgak
{
    // 실제 EventPresenter/DataTable 없이 ChoiceGroupView 연출만 확인하기 위한 임시 테스트용 컴포넌트.
    public sealed class ChoiceGroupViewTestHarness : MonoBehaviour
    {
        [SerializeField] private ChoiceGroupView choiceGroupView;
        [SerializeField] private bool runOnStart = true;

        private IEnumerator Start()
        {
            if (runOnStart)
            {
                yield return RunOnce();
            }
        }

        [ContextMenu("Run Choice Test")]
        private void RunFromMenu()
        {
            StartCoroutine(RunOnce());
        }

        private IEnumerator RunOnce()
        {
            List<ChoiceData> choices = new()
            {
                CreateChoice("TEST_P", KeyCode.P, "P. 올해 축제는 취소한다"),
                CreateChoice("TEST_Q", KeyCode.Q, "Q. 일주일 동안 대축제를 연다"),
                CreateChoice("TEST_SPACE", KeyCode.Space, "Space. 왕궁 앞에서 하루만 축하한다"),
            };

            ChoiceData selected = null;
            yield return choiceGroupView.ShowChoices(choices, choice => selected = choice);

            Debug.Log($"[ChoiceGroupViewTestHarness] Selected: {selected?.choiceId}");

            yield return new WaitForSeconds(1f);
            yield return choiceGroupView.HideChoices();
        }

        private static ChoiceData CreateChoice(string id, KeyCode key, string description)
        {
            return new ChoiceData { choiceId = id, inputKey = key, description = description };
        }
    }
}