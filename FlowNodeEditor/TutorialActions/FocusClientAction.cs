[System.Serializable]
public class FocusClientAction : FlowNodeAction
{
    //This action is only used by Buddy Tutorial
    public override string ActionName => "Focus Spawn";

    public override void Execute()
    {
        TeaHouseManager.instance.FocusLastSpawn();
    }

    public override void Dismiss() { }
}
