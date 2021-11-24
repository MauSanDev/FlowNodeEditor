
[System.Serializable]
public class GameplayModifiersAction : FlowNodeAction
{
    public bool revertOnDismiss;
    public bool pauseTimer;

    public override string ActionName => "Gameplay Modifiers";

    public override void Execute()
    {
        ExecuteGameplayModifiers();
    }

    public override void Dismiss()
    {
        if(revertOnDismiss)
        {
            ExecuteGameplayModifiers(true);
        }
    }

    private void ExecuteGameplayModifiers(bool revert = false)
    {
        if(GameplayManager != null)
        {
            GameplayManager.PauseGameplay(revert ? !pauseTimer : pauseTimer, false);
        }
    }

    public GamePlayManager GameplayManager => GamePlayManager.instance;
}
