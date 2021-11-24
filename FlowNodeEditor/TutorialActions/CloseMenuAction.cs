[System.Serializable]
public class CloseMenuAction : FlowNodeAction
{
    public bool closeMenues;
    public bool closePopups;

    public override string ActionName => "Close Menu Action";

    public override void Execute()
    {
        if(closeMenues)
        {
            MenuManager.instance.CloseMenu();
        }
        
        if(closePopups)
        {
            PopUpManager.instance.ClosePopUpCanvas();
        }
    }

    public override void Dismiss()
    {
    }
}
