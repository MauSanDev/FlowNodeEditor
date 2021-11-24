using UnityEngine;

[System.Serializable]
public class TriggerLogAction : FlowNodeAction
{
    public override string ActionName => "Trigger Log Action";
    [SerializeField] public string onExecuteLog = "";
    [SerializeField] public string onDismissLog = "";

    public override void Execute()
    {
        Debug.Log($"{GetType()} :: EXECUTE :: {onExecuteLog}"); 
    }

    public override void Dismiss()
    {
        Debug.Log($"{GetType()} :: DISMISS :: {onDismissLog}");
    }
}
