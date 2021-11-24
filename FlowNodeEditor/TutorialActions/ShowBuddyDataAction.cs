using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShowBuddyDataAction : FlowNodeAction
{
    public override string ActionName => "Show Buddy Data";

    public override void Execute()
    {
        TeaHamster buddy = Manager.TeaHamsterData.GetHamsterByID(Manager.PlayerState.HamFriends.BestHamFriend);
        buddy.ShowOnMenu();
    }

    public override void Dismiss() { }

    private ServiceManager Manager => ServiceManager.instance;
}
