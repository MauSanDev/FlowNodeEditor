using System;

[Serializable]
public abstract class FlowNodeStepDismisser : IFlowNodeStepDismisser
{
    public virtual Type ActionTypeDependency { get; }

    [field: NonSerializedAttribute()] public event Action OnDismiss;
    /// <summary>
    /// Checks on the TutorialManager Update method to trigger specific actions.
    /// </summary>
    public virtual void ListenUpdates()
    {
    }

    public void Dismiss()
    {
        OnDismiss?.Invoke();
    }

}

public interface IFlowNodeStepDismisser
{
    void ListenUpdates();
    void Dismiss();
}
