using UnityEngine;
using System.Collections.Generic;

public class FlowNodeManager : MonoBehaviour
{
    public static FlowNodeManager Instance { get; set; }

    [SerializeField] private FlowNodeScriptWrapper scriptList = null;
    
    private FlowNodeScriptData currentScript = null;
    private Queue<string> scriptQueue = new Queue<string>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }


    private void Update()
    {
        currentScript?.ListenUpdates();
    }

    /// <summary>
    /// Trigger the flow of a tutorial by it's name. If another tutorial is running, it will not trigger, but can be enqueued.
    /// </summary>
    /// <param name="tutorialKey">Defines the name of the tutorial to try trigger.</param>
    /// <param name="enqueue">If a tutorial is running and this value is true, the tutorial will be enqueued to play after the current one.</param>
    public void ShowTutorial(string tutorialKey, bool enqueue = true)
    {
#if FLOWNODE_DISABLED
        return;
#endif
        if(currentScript != null && currentScript.IsRunning)
        {
            if(enqueue)
            {
                scriptQueue.Enqueue(tutorialKey);
            }
            else
            {
                Debug.LogError($"{GetType()} :: Cannot try trigger {tutorialKey} because {currentScript.Name} is running. Try again or enqueue it.");
            }
            return;
        }

        if(scriptList.TryGetScript(tutorialKey, out currentScript))
        {
            if(currentScript.CanShow)
            {
                currentScript.OnFinish += FinishScript;
                currentScript.Start();
            }
            else
            {
                Debug.LogError($"{GetType()} :: Tutorial of key {currentScript.Name} was already shown.");
                ExecuteNextOnQueue();
            }
        }
        else
        {
            Debug.LogError($"{GetType()} :: Tutorial of key {tutorialKey} doesn't exist.");
            ExecuteNextOnQueue();
        }
    }

    private void ExecuteNextOnQueue()
    {
        if(scriptQueue.Count > 0)
        {
            ShowTutorial(scriptQueue.Dequeue());
        }
    }

    private void FinishScript()
    {
        currentScript.OnFinish -= FinishScript;
        currentScript = null;
        ExecuteNextOnQueue();
    }
}
