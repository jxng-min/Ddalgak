using JxModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : LocalSingleton<EventManager>
{
    [Header("연동 컴포넌트")]
    public ButtonActionSequence sequencer;

    [Header("현재 상태")]
    public EGameState currentState = EGameState.Inactive;

    //public GameEventData currentEvent;
    // 현재 실행 중인 데이터


    public void StartEvent(/*GameEventData eventData*/)
    {
        /*currentEvent = eventData;

        switch (eventData.eventType)
        {
            case EEventType.NormalChoice:
            case EEventType.ExecutableChoice:
                EnterChoicePhase();
                break;
            case EEventType.SurpriseAction:
                StartCoroutine(SurpriseEventRoutine());
                break;
        }*/
    }

    private void EnterChoicePhase()
    {
        currentState = EGameState.ChoicePhase;
    }

    private void Update()
    {
        if (currentState == EGameState.ChoicePhase)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                //OnSelectOption(currentEvent.optionQ);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                //OnSelectOption(currentEvent.optionSpace);
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                //OnSelectOption(currentEvent.optionP);
            }
        }
    }

    /*private void OnSelectOption(ChoiceOption option)
    {
        //selectedOption = option;

        *//*if (currentEvent.eventType == EEventType.NormalChoice)
        {
            EndEvent();
        }
        else if (currentEvent.eventType == EEventType.ExecutableChoice)
        {
            EnterActionPhase(selectedOption.actionSequence);
        }*//*
    }*/

    private void EnterActionPhase(List<ButtonAction> sequence)
    {
        currentState = EGameState.ActionPhase;
        sequencer.actionSequence = sequence;
        sequencer.onSequenceSuccess.RemoveAllListeners();
        sequencer.onSequenceFailed.RemoveAllListeners();

        sequencer.onSequenceSuccess.AddListener(OnActionSuccess);
        sequencer.onSequenceFailed.AddListener(OnActionFailed);

        sequencer.StartSequence();
    }

    private IEnumerator SurpriseEventRoutine()
    {
        //경고 표시
        yield return new WaitForSeconds(3.0f);
        //EnterActionPhase(currentEvent.surpriseActionSequence);
    }

    private void OnActionSuccess()
    {
        //액션성공
        /*if (currentEvent.eventType == EEventType.ExecutableChoice)
        {
            ApplyStats(selectedOption.baseResult);
        }
        else if (currentEvent.eventType == EEventType.SurpriseAction)
        {
            ApplyStats(currentEvent.surpriseSuccessResult);
        }*/

        EndEvent();
    }

    private void OnActionFailed()
    {
        //액션실패
        /*if (currentEvent.eventType == EEventType.ExecutableChoice)
        {
            KingdomStats combinedResult = AddStats(selectedOption.baseResult, selectedOption.actionFailurePenalty);
            ApplyStats(combinedResult);
        }
        else if (currentEvent.eventType == EEventType.SurpriseAction)
        {
            ApplyStats(currentEvent.surpriseFailureResult);

            if (currentEvent.isGameOverOnFailure)
            {
                TriggerGameOver();
                return;
            }
        }*/

        EndEvent();
    }

    private void EndEvent()
    {
        currentState = EGameState.Inactive;
    }

    private void TriggerGameOver()
    {
        currentState = EGameState.Inactive;
    }
}
