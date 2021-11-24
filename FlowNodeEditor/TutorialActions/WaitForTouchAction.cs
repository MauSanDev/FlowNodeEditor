using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaitForTouchAction : FlowNodeAction
{
    private WaitForTouchDismisser dismisser = new WaitForTouchDismisser();

    public override string ActionName => "Wait for Touch";

    public override FlowNodeStepDismisser DependencyDismisser => dismisser;

    public override void Execute()
    {
        InputManager.Instance.OnTouchStarted += (x) => dismisser.Dismiss();
    }

    public override void Dismiss()
    {
        InputManager.Instance.OnTouchStarted -= (x) => dismisser.Dismiss();
    }
}

[System.Serializable]
public class WaitForTouchDismisser : FlowNodeStepDismisser
{ 
}
