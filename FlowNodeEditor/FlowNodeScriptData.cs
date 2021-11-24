using System.Collections.Generic;
using UnityEngine;
using System;

public class FlowNodeScriptData : ScriptableObject, ISerializationCallbackReceiver
{
    private const string SCRIPT_SEEN_PREF = "FlowNodeScript_";
    
    [SerializeField] private string scriptName = null;
    [SerializeField] private string identifier = null;
    [SerializeField] private string description = null;
    [SerializeField] private List<string> dependencies = new List<string>();
    [HideInInspector] public string guid = null;
    public List<FlowNodeStep> steps = new List<FlowNodeStep>();
    public bool showOnce = false;

    private int currentStepIndex = 0;
    private bool isRunning;

    public event Action OnFinish;
    public event Action OnStepChange;
    public event Action OnStart;


    public string Name
    {
        get => scriptName;
        set => scriptName = value;
    }
    public string Description
    {
        get =>description;
        set => description = value;
    }
    

    public string GUID
    {
        get => guid;
        set => guid = value;
    }

    public string Identifier
    {
        get => identifier;
        set => identifier = value;
    }

    public List<string> Dependencies => dependencies;
    public bool CanShow => !showOnce || showOnce && !WasShown;
    public bool IsRunning => isRunning;
    
    public bool WasShown
    {
        get
        {
            if(string.IsNullOrEmpty(GUID))
            {
                return false;
            }
            return PlayerPrefs.GetInt(SCRIPT_SEEN_PREF + GUID) == 1;
        }
        set
        {
            if(!value)
            {
                PlayerPrefs.DeleteKey(SCRIPT_SEEN_PREF + GUID);
            }
            else
            {
                PlayerPrefs.SetInt(SCRIPT_SEEN_PREF + GUID, 1);
            }
        }
    }
    
    private FlowNodeStep CurrentStep => steps[currentStepIndex];

    public void Start()
    {
        isRunning = true;
        currentStepIndex = 0;
        OnStart?.Invoke();
        GoToNextStep();
    }

    public void GoToNextStep()
    {
        if (CurrentStep.IsExecuting)
        {
            CurrentStep.DismissStep();
            CurrentStep.OnDismiss -= GoToNextStep;
            currentStepIndex++;
        }

        OnStepChange?.Invoke();

        if(currentStepIndex < steps.Count)
        {
            steps[currentStepIndex].ExecuteStep();
            steps[currentStepIndex].OnDismiss += GoToNextStep;
        }
        else
        {
            Finish();
        }
    }

    private void Finish()
    {
        WasShown = true;
        isRunning = false;
        OnFinish?.Invoke();
    }

    public void ListenUpdates()
    {
        if(steps.Count > currentStepIndex)
        {
            steps[currentStepIndex]?.ListenUpdates();
        }
    }

    public bool AddDependency(string dependencyId)
    {
        if(dependencyId == Identifier)
        { 
            return false;
        }

        if(!dependencies.Contains(dependencyId))
        {
            dependencies.Add(dependencyId);
        }

        return true;
    }
    
    
    #region Serialization

    public void OnBeforeSerialize()
    {
        foreach (FlowNodeStep step in steps)
        {
            step.OnBeforeSerialize();
        }
    }

    public void OnAfterDeserialize()
    {
        foreach (FlowNodeStep step in steps)
        {
            step.OnAfterDeserialize();
        }
    }

    #endregion
}
