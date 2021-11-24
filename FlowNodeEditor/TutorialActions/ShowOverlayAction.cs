using System;

[Serializable]
public class ShowOverlayAction : FlowNodeAction
{
    public OverlayStatus status = OverlayStatus.Show;

    public enum OverlayStatus
    {
        Show,
        Hide
    }

    public override string ActionName => "Show Overlay";

    public override void Execute()
    {
        OverlayWidget.Instance.SetOverlay(status == OverlayStatus.Show);
    }

    public override void Dismiss() { }
}
