[System.Serializable]
public class SpawnHamsterAction : FlowNodeAction
{
    public override string ActionName => "Spawn Hamster";

    public override void Execute()
    {
        if(TeaHouseManager != null)
        {
            TeaHouseManager.ForceSpawn();
        }
    }

    public override void Dismiss() { }

    private TeaHouseManager TeaHouseManager => TeaHouseManager.instance;
}
