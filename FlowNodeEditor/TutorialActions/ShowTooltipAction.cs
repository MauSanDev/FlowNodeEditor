using UnityEngine;
using System;

[System.Serializable]
public class ShowTooltipAction : FlowNodeAction
{
    [SerializeField] public string textToShow = "";
    [SerializeField] public TooltipWidget.Anchor anchor = TooltipWidget.Anchor.Center;
    [SerializeField] public float yPercent = 0f;
    [SerializeField] public bool showButton = false;
    [SerializeField] public float dismissAfterTime = 0f;
    public override string ActionName => "Show Tooltip";

    private ShowTooltipDismisser dismisser = new ShowTooltipDismisser();

    public override void Execute()
    {
        float positionY = (Camera.main.orthographicSize * 2) * (yPercent * 0.1f);
        TooltipWidget.ShowTooltip(textToShow, anchor, positionY, showButton, DependencyDismisser.Dismiss, dismissAfterTime);
    }

    public override void Dismiss()
    {
        if(dismissAfterTime == 0 && !showButton) //This means that the button is not being shown and the tooltip won't be closed by time, so it closes the current tooltip.
        {
            TooltipWidget.CloseCurrentTooltip();
        }
    }

    public override FlowNodeStepDismisser DependencyDismisser => dismisser; 
}

[System.Serializable]
public class ShowTooltipDismisser : FlowNodeStepDismisser
{
    public override Type ActionTypeDependency => typeof(ShowTooltipAction);
}


