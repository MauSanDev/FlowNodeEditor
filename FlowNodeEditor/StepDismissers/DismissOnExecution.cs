[System.Serializable]
public class DismissOnExecution : FlowNodeStepDismisser
{
    public override void ListenUpdates()
    {
        Dismiss();
    }
}
