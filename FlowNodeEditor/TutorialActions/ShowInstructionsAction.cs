[System.Serializable]
public class ShowInstructionsAction : FlowNodeAction
{
    public string instructionsScriptName = null;
    private ShowInstructionsDismisser dismisser = new ShowInstructionsDismisser();

    public override string ActionName => "Show Instructions";

    public override FlowNodeStepDismisser DependencyDismisser => dismisser;

    public override void Execute()
    {
        TeaHouseUtils.ShowInstructionsPopup(instructionsScriptName, DependencyDismisser.Dismiss);
    }

    public override void Dismiss()
    {
    }
}

[System.Serializable]
public class ShowInstructionsDismisser : FlowNodeStepDismisser
{

}
