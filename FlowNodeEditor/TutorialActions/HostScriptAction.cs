using System.Collections.Generic;
using System;

[Serializable]
public class HostScriptAction : FlowNodeAction
{
    public bool allowDrag = false;
    public bool showOverlay = true;
    public bool destroyWhenFinished = true;
    [FlowNodeEditorField("Steps")] public List<HostStep> steps = new List<HostStep>();
    public override string ActionName => "Host Script Display";

    private HostScriptDismisser dismisser = new HostScriptDismisser();

    public override void Execute()
    {
        TeaHouseUtils.ShowHost(steps, dismisser.Dismiss, false, showOverlay, allowDrag);
    }

    public override void Dismiss()
    {
        if(destroyWhenFinished)
        {
            TeaHouseUtils.CloseHost();
        }
    }

    public override FlowNodeStepDismisser DependencyDismisser => dismisser;
}

[System.Serializable]
public class HostScriptDismisser : FlowNodeStepDismisser
{
    public override Type ActionTypeDependency => typeof(HostScriptAction);
}