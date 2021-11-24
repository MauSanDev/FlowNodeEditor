using System;
using System.Reflection;

[Serializable]
public class OpenMenuAction : FlowNodeAction
{
    public string popupPresenter = null;
    public bool closeOnDismiss = false;

    public override string ActionName => "Open Menu";

    public override void Execute()
    {
        Type presenterType = System.Type.GetType(popupPresenter);

        MethodInfo method = typeof(MenuManager).GetMethod(nameof(MenuManager.ShowMenu));
        MethodInfo generic = method.MakeGenericMethod(presenterType);
        object[] parameters = { null, null, false };
        generic.Invoke(MenuManager, parameters);
    }

    public override void Dismiss()
    {
        if(closeOnDismiss)
        {
            MenuManager.CloseMenu();
        }
    }

    public MenuManager MenuManager => MenuManager.instance;
}
