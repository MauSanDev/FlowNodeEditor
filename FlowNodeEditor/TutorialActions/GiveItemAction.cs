[System.Serializable]
public class GiveItemAction : FlowNodeAction
{
    public TeaHamsterIDs.ItemId item;
    public int amount = 1;

    public override string ActionName => "Give Item Action";

    public override void Execute()
    {
        PlayerItems.AddItem(item, amount);
    }

    public override void Dismiss() {}

    private PlayerStateItems PlayerItems => ServiceManager.instance.PlayerState.Inventory;
}
