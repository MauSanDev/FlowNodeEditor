using System;

[Serializable]
public class WaitForEventAction : FlowNodeAction
{
    public IdentifierReference eventReference = new IdentifierReference();
    private WaitForEventDismisser dismisser = new WaitForEventDismisser();

    public override string ActionName => "Wait for Event";

    private CustomEventSystemManager Manager => CustomEventSystemManager.instance;

    public override void Execute()
    {
        Manager.AddListener(eventReference.AssignedID, dismisser.Dismiss);
    }

    public override void Dismiss()
    {
        Manager.RemoveListener(eventReference.AssignedID, dismisser.Dismiss);
    }

    public override FlowNodeStepDismisser DependencyDismisser => dismisser;
}

[System.Serializable]
public class WaitForEventDismisser : FlowNodeStepDismisser
{
    public override Type ActionTypeDependency => typeof(WaitForEventAction);
}