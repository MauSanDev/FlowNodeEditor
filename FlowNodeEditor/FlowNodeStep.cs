using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FlowNodeStep : ISerializationCallbackReceiver
{
    [SerializeField] public string stepDescription = "";
    [SerializeField] private FlowNodeStepDismisser dismisser = new DismissOnExecution();
    [SerializeField] private List<string> serializedActions = new List<string>();
    private List<FlowNodeAction> actions = new List<FlowNodeAction>();
    
    [field: NonSerializedAttribute()] private bool isExecuting = false;

    public List<FlowNodeAction> Actions => actions;
    public FlowNodeStepDismisser Dismisser
    {
        get => dismisser;
        set => dismisser = value;
    }

    public bool IsExecuting => isExecuting;

    public void ExecuteStep()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].Execute();
        }

        isExecuting = true;
    }
    public void DismissStep()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].Dismiss();
        }

        isExecuting = false;
    }

    public void ListenUpdates()
    {
        dismisser?.ListenUpdates();
    }

    public event Action OnDismiss
    {
        add
        {
            dismisser.OnDismiss += value;
        }
        remove
        {
            dismisser.OnDismiss -= value;
        }
    }
    
    public void OnBeforeSerialize()
    {
        serializedActions.Clear();
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].OnAfterDeserialize();
            serializedActions.Add(actions[i].GetJSON());
        }
    }

    public void OnAfterDeserialize()
    {
        actions.Clear();
        for (int i = 0; i < serializedActions.Count; i++)
        {
            FlowNodeAction deserializedAction = FlowNodeAction.ConstructFromJSON(serializedActions[i]);
            deserializedAction.OnAfterDeserialize();
            actions.Add(deserializedAction);
        }
    }

    public void AddAction(FlowNodeAction action)
    {
        if (!actions.Contains(action))
        {
            actions.Add(action);
        }
    }

    public bool RemoveAction(FlowNodeAction action) => actions.Remove(action);

    public bool ContainsDismisserDependency()
    {
        if(dismisser != null && dismisser.ActionTypeDependency != null)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].GetType() == dismisser.ActionTypeDependency)
                {
                    return true;
                }
            }
            return false;
        }

        return true;
    }
}
