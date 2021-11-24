using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class FlowNodeEditor : ReflectiveEditor
{
    private const string FLOWNODE_ASSETS_PATH = "Assets/Resources/FlowNode";
    private const string FLOW_SCRIPTS_PATH = FLOWNODE_ASSETS_PATH + "/Scripts";
    private const string WRAPPER_PATH = FLOWNODE_ASSETS_PATH + "/FlowNodeScriptWrapper.asset";

    private FlowNodeScriptWrapper scriptWrapper = null;
    public FlowNodeScriptData selectedScript = null;
    
    private Vector2 scrollPosition = Vector2.zero;
    private Color uncollapsedStepColor = Color.magenta;

    private List<Type> availableActionTypes = new List<Type>();
    private List<Type> availableDismisserTypes = new List<Type>();
    private ElementsDropdown<Type> dismissersDropdown = null;
    
    //Editor Variables
    private bool isInGameList = false;
    private bool scriptExists = false;
    private int uncollapsedStepIndex = 0;
    private bool showInsertStepButtons = false;
    private bool showDependenciesBox = false;

    private List<FlowNodeScriptData> ScriptList => scriptWrapper.List;

    [MenuItem("Noar Utils/FlowNode Editor")]
    public static void ShowScriptEditor()
    {
        Open();
    }

    private FlowNodeScriptWrapper GetOrCreateWrapper()
    {
        FlowNodeScriptWrapper wrapper = AssetDatabase.LoadAssetAtPath<FlowNodeScriptWrapper>(WRAPPER_PATH);
        if (wrapper == null)
        {
            wrapper = CreateInstance<FlowNodeScriptWrapper>();
            AssetDatabase.CreateAsset(wrapper, WRAPPER_PATH);
        }

        return wrapper;
    }

    public static void Open(FlowNodeScriptData flowNodeScript = null)
    {
        FlowNodeEditor flowNodeWindow = GetWindow<FlowNodeEditor>("FlowNode Editor");

        if(flowNodeScript != null)
        {
            flowNodeWindow.SelectScript(flowNodeScript);
        }
        flowNodeWindow.Show();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        scriptWrapper = GetOrCreateWrapper();
        SearchInheritancesByReflection();
    }

    private void OnDisable()
    {
        EditorUtility.SetDirty(scriptWrapper);
        AssetDatabase.SaveAssets();
    }

    private void SearchInheritancesByReflection()
    {
        availableActionTypes = ReflectionUtilities.GetInheritancesOfType<FlowNodeAction>();
        availableDismisserTypes = ReflectionUtilities.GetInheritancesOfType<FlowNodeStepDismisser>();
        dismissersDropdown = new ElementsDropdown<Type>(availableDismisserTypes.ToArray());
    }

    private void ShowAddActionMenu(FlowNodeStep stepToAdd)
    {
        GenericMenu addActionMenu = new GenericMenu();
        foreach(Type actionType in availableActionTypes)
        {
            addActionMenu.AddItem(new GUIContent(actionType.ToString()), false, () => { AddActionToStep(stepToAdd, actionType); });
        }

        addActionMenu.ShowAsContext();
    }

    private void OnGUI()
    {
        DrawToolbar();
        
        EditorGUILayout.Space();

        if (selectedScript == null)
        {
            DrawSelectScriptMessage();
            return;
        }

        DrawScriptData();

        EditorGUILayout.BeginHorizontal();
        DrawSidebar();
        DrawStepsContainer();
        EditorGUILayout.EndHorizontal();
    }

    private void AddToGameList(bool isInGameList)
    {
        if (isInGameList)
        {
            if(!ScriptList.Contains(selectedScript))
            {
                ScriptList.Add(selectedScript);
            }
        }
        else
        {
            ScriptList.Remove(selectedScript);
        }
    }

    
    #region Drawers
    
    private void DrawSelectScriptMessage()
    {
        EditorGUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField("Please select or create a new Script to start editing!");
        EditorGUILayout.EndHorizontal();
    }

    private void DrawScriptData()
    {
        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Script Data:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 14 }, GUILayout.Width(position.width * 0.64f));

        EditorGUILayout.BeginHorizontal(GUILayout.Width(position.width * 0.34f));
        
        GUI.backgroundColor = scriptExists ? Color.red : Color.black;
        DrawSimpleButton($"Delete Script", () => TryDeleteScript(scriptExists));
        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = selectedScript.showOnce ? Color.green : Color.gray;
        DrawSimpleButton($"Show Once{(selectedScript.showOnce ? " ✓" : "")}", () => { selectedScript.showOnce = !selectedScript.showOnce; }, EditorStyles.miniButtonLeft);
        GUI.backgroundColor = Color.white;

        GUI.backgroundColor = isInGameList ? Color.green : Color.gray;
        DrawSimpleButton($"Add to Game{(isInGameList ? " ✓" : "")}", () => { isInGameList = !isInGameList; }, EditorStyles.miniButtonMid);
        GUI.backgroundColor = Color.white;
        
        if (scriptExists)
        {
            GUI.backgroundColor = selectedScript.WasShown ? Color.red : Color.green;
            DrawSimpleButton($"Set as {(selectedScript.WasShown ? "Unseen" : "Seen")}", () => { selectedScript.WasShown = !selectedScript.WasShown; }, EditorStyles.miniButtonRight);
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUI.backgroundColor = Color.black;
            DrawSimpleButton($"Set as Seen", () => { EditorUtility.DisplayDialog("Can't set as seen", "Save the file before trying to set this as seen.", "Okay"); }, EditorStyles.miniButtonRight);
            GUI.backgroundColor = Color.white;
        }    

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();

        selectedScript.Name = EditorGUILayout.TextField("Script Name:", selectedScript.Name);
        selectedScript.Identifier = EditorGUILayout.TextField("Identifier:", selectedScript.Identifier);
        selectedScript.Description = EditorGUILayout.TextField("Description", selectedScript.Description);

        if(!string.IsNullOrEmpty(selectedScript.GUID))
        {
            EditorGUILayout.LabelField($"GUID: {selectedScript.GUID}", new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Italic });
        }

        DrawScriptDependencies();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }
    
    private void DrawSidebar()
    {
        EditorGUILayout.BeginVertical("HelpBox", GUILayout.Width(position.width * 0.25f), GUILayout.Height(position.y));

        EditorGUILayout.BeginHorizontal();
        DrawSimpleButton("Add Step", () => AddStep(), EditorStyles.miniButtonLeft);
        DrawSimpleButton("Insert Step", () => { showInsertStepButtons = !showInsertStepButtons; }, EditorStyles.miniButtonRight);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        for (int i = 0; i < selectedScript.steps.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = i == uncollapsedStepIndex ? uncollapsedStepColor : Color.white;
            DrawSimpleButton($"Step {i + 1} : {(!string.IsNullOrEmpty(selectedScript.steps[i].stepDescription) ? selectedScript.steps[i].stepDescription : "<No Info>")}", () => UncollapseStepAtIndex(i), EditorStyles.miniButtonLeft);
            DrawSizeableButton("▲", () => MoveStep(i, i - 1), 20, EditorStyles.miniButtonMid);
            DrawSizeableButton("▼", () => MoveStep(i, i + 1), 20, EditorStyles.miniButtonMid);
            DrawSizeableButton("X", () => { RemoveStep(i); }, 20, EditorStyles.miniButtonRight);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            DrawInsertStepAtIndexButton(i);
        }
        EditorGUILayout.EndVertical();
    }
    
    private void DrawStepsContainer()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.74f));

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true);

        if (selectedScript.steps.Count == 0)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("This Script has no steps. Add the first one!");
            DrawAddStepButton();
            EditorGUILayout.EndVertical();
        }
        else
        {
            for (int i = 0; i < selectedScript.steps.Count; i++)
            {
                DrawStep(selectedScript.steps[i], i);
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawStep(FlowNodeStep step, int index)
    {
        GUI.backgroundColor = index == uncollapsedStepIndex ? uncollapsedStepColor : Color.white;

        EditorGUILayout.BeginVertical("HelpBox");
        GUI.backgroundColor = Color.white;

        EditorGUILayout.BeginHorizontal();
        
        DrawLabelButton($"Step {index + 1}", fontSize:14);
        DrawSizeableButton("X", () => { RemoveStep(index); }, 20, EditorStyles.miniButtonRight);
        EditorGUILayout.EndHorizontal();

        DrawByReflection(step, flags: BindingFlags.Instance | BindingFlags.Public);

        EditorGUILayout.BeginVertical("Box");
        
        EditorGUILayout.BeginHorizontal();
        DrawLabelButton($"Actions:", null);
        DrawSizeableButton("Clear", () => { step.Actions.Clear(); }, 50, EditorStyles.miniButtonLeft);
        DrawSizeableButton("Collapse", null, 60, EditorStyles.miniButtonMid);
        DrawSizeableButton("Add", () => { ShowAddActionMenu(step); }, 40, EditorStyles.miniButtonRight);
        EditorGUILayout.EndHorizontal();
        
        for(int i = 0; i < step.Actions.Count; i++)
        {
            DrawAction(step.Actions[i], step);
        }

        EditorGUILayout.EndVertical();
        
        DrawDismisser(step);
        
        EditorGUILayout.EndVertical();

    }

    private void DrawAction(FlowNodeAction actionToDraw, FlowNodeStep parentStep)
    {
        EditorGUILayout.BeginVertical("Box");

        EditorGUILayout.BeginHorizontal();
        DrawLabelButton(actionToDraw.ActionName, null);

        if(actionToDraw.DependencyDismisser != null)
        {
            bool isUsingDismisser = parentStep.Dismisser == actionToDraw.DependencyDismisser;
            
            GUI.backgroundColor = isUsingDismisser ? Color.green : Color.white;
            DrawSizeableButton($"Use Dismisser{(isUsingDismisser ? " ✓" : "")}", () => { parentStep.Dismisser = actionToDraw.DependencyDismisser; }, 120, EditorStyles.miniButtonLeft);
            GUI.backgroundColor = Color.white;
        }
        
        DrawSizeableButton("X", () => { parentStep.RemoveAction(actionToDraw); }, 20, EditorStyles.miniButtonRight);
        EditorGUILayout.EndHorizontal();
        
        DrawByReflection(actionToDraw, new List<Type>{typeof(FlowNodeEditorField)}, GetFieldNameParser, flags: BindingFlags.Instance | BindingFlags.Public);
        
        EditorGUILayout.EndVertical();
    }

    private string GetFieldNameParser(FieldInfo field)
    {
        FlowNodeEditorField attribute = field.GetAttributeOfType<FlowNodeEditorField>();
        return attribute != null ? attribute.Label : field.Name;
    }
    
    private void DrawAddStepButton()
    {
        DrawSizeableButton($"Add Step", () => AddStep(), 80);
    }
    
    private void DrawInsertStepAtIndexButton(int index)
    {
        if (selectedScript.steps.Count > index + 1 && showInsertStepButtons)
        {
            GUIStyle insertBtnStyle = new GUIStyle(GUI.skin.button);
            insertBtnStyle.normal.textColor = Color.gray;
            insertBtnStyle.fontStyle = FontStyle.Italic;
            GUI.backgroundColor = Color.black;
            DrawSimpleButton($"Insert between {index + 1 } and {index + 2}", () => InsertStep(index + 1), insertBtnStyle);
            GUI.backgroundColor = Color.white;
        }
    }
    
    private void DrawScriptDependencies()
    {
        GUI.backgroundColor = Color.black;
        EditorGUILayout.BeginVertical("HelpBox");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button($"{(showDependenciesBox ? "▼" : "►")} Dependencies:", EditorStyles.label))
        {
            showDependenciesBox = !showDependenciesBox;
        }

        DrawSizeableButton("Add Dependency", ShowAddDependencyMenu, 120, new GUIStyle(EditorStyles.miniButton));
        EditorGUILayout.EndHorizontal();

        if(!showDependenciesBox)
        {
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.LabelField("This Script won't show if the Dependencies weren't shown before. (Only In Game Scripts can be dependencies)", new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Italic });

        GUI.backgroundColor = Color.black;
        EditorGUILayout.BeginHorizontal("HelpBox");
        if(selectedScript.Dependencies.Count == 0)
        {
            EditorGUILayout.LabelField("No Dependencies");
        }
        else
        {
            GUI.backgroundColor = Color.white;
            for (int i = 0; i < selectedScript.Dependencies.Count; i++)
            {
                if (GUILayout.Button(selectedScript.Dependencies[i], GUILayout.Width(120)))
                {
                    selectedScript.Dependencies.RemoveAt(i);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
    }

    private void DrawDismisser(FlowNodeStep step)
    {
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("Dismiss:", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

        EditorGUI.BeginChangeCheck();
        dismissersDropdown.DrawGUILayout("Type:", availableDismisserTypes[0]);
        if (EditorGUI.EndChangeCheck())
        {
            step.Dismisser = (FlowNodeStepDismisser)Activator.CreateInstance(dismissersDropdown.SelectedElement);
        }
            
        if(!step.ContainsDismisserDependency())
        {
            DrawDismisserErrorMessage(step.Dismisser.ActionTypeDependency);
        }
        else
        {        
            DrawByReflection(step.Dismisser, flags: BindingFlags.Instance | BindingFlags.Public);
        }

        EditorGUILayout.EndVertical();
    }
    
    private void DrawDismisserErrorMessage(Type dismisserType)
    {
        GUI.backgroundColor = Color.red;
        EditorGUILayout.BeginHorizontal("HelpBox");
        EditorGUILayout.LabelField(
            $"This dismisser only works being settled by an Action of type {dismisserType}.");
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
    }
    
    #endregion

    #region Toolbar & Menues
    
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width * 0.5f));

        if (GUILayout.Button("Menu", EditorStyles.toolbarButton))
        {
            ShowOpenMenu();
        }

        if (GUILayout.Button("New", EditorStyles.toolbarButton))
        {
            TryCreateNew();
        }

        if (GUILayout.Button("Close", EditorStyles.toolbarButton))
        {
            TryCloseScript();
        }

        if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
        {
            ReloadScript();
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton))
        {
            SaveScript();
        }

        if (GUILayout.Button("Show on Inspector", EditorStyles.toolbarButton))
        {
            PingObject(selectedScript);
        }

        EditorGUILayout.EndHorizontal();
    }
    
    private void ShowOpenMenu()
    {
        GenericMenu showMenu = new GenericMenu();
        showMenu.AddItem(new GUIContent("New Script"), false, TryCreateNew);
        showMenu.AddSeparator("");

        string[] scriptsAtFolder = Directory.GetFiles(FLOW_SCRIPTS_PATH, "*.asset");
        if(scriptsAtFolder.Length > 0)
        {
            foreach (string tutoPath in Directory.GetFiles(FLOW_SCRIPTS_PATH, "*.asset"))
            {
                showMenu.AddItem(new GUIContent($"Open/{Path.GetFileName(tutoPath)}"), false, () => OpenScriptOnPath(tutoPath));
            }
        }
        else
        {
            showMenu.AddDisabledItem(new GUIContent($"Open/<No Files>"));
        }

        if(ScriptList.Count > 0)
        {
            foreach (FlowNodeScriptData inGameTuto in ScriptList)
            {
                showMenu.AddItem(new GUIContent($"In Game/{inGameTuto.name}"), false, () => SelectScript(inGameTuto));
            }
        }
        else
        {
            showMenu.AddDisabledItem(new GUIContent($"In Game/<No Files>"));
        }

        showMenu.AddSeparator("");

        if(selectedScript != null)
        {
            showMenu.AddItem(new GUIContent("Close"), false, TryCloseScript);
        }
        else
        {
            showMenu.AddDisabledItem(new GUIContent("Close"));
        }

        showMenu.ShowAsContext();
    }

    private void AddDependency(string dependencyId)
    {
        if(!selectedScript.AddDependency(dependencyId))
        {
            EditorUtility.DisplayDialog("Error", "You are adding the current Script as it's own dependency.", "Continue");
        }
    }
    
    private void ShowAddDependencyMenu()
    {
        GenericMenu showMenu = new GenericMenu();

        for (int i = 0; i < ScriptList.Count; i++)
        {
            if (ScriptList[i] == selectedScript)
            {
                continue;
            }

            string identifier = ScriptList[i].Identifier;
            showMenu.AddItem(new GUIContent(identifier), false, () => AddDependency(identifier));
        }

        showMenu.ShowAsContext();
    }

    #endregion
    
    #region Step Modifiers

    private void AddStep()
    {
        selectedScript.steps.Add(new FlowNodeStep());
        selectedScript.steps[0].Dismisser = (FlowNodeStepDismisser)Activator.CreateInstance(availableDismisserTypes[0]);
        UncollapseStepAtIndex(selectedScript.steps.Count - 1);
    }

    private void InsertStep(int index = 0)
    {
        selectedScript.steps.Insert(index, new FlowNodeStep() { stepDescription = "<Inserted>" });
        UncollapseStepAtIndex(index);
        showInsertStepButtons = false;
        UncollapseStepAtIndex(index);
    }
    
    private void RemoveStep(int index)
    {
        selectedScript.steps.RemoveAt(index);
        UncollapseStepAtIndex(selectedScript.steps.Count >= index ? index - 1 : 0);
    }
    
    private void MoveStep(int from, int to)
    {
        if(to < 0 || to > selectedScript.steps.Count - 1)
        {
            return;
        }

        FlowNodeStep toMove = selectedScript.steps[from];
        selectedScript.steps.RemoveAt(from);
        selectedScript.steps.Insert(to, toMove);
        UncollapseStepAtIndex(to);
    }
    
    private void AddActionToStep(FlowNodeStep stepToAdd, Type actionType)
    {
        FlowNodeAction created = (FlowNodeAction)Activator.CreateInstance(actionType);

        if(created.DependencyDismisser != null)
        {
            stepToAdd.Dismisser = created.DependencyDismisser;
        }

        stepToAdd.AddAction(created);
    }
    
    #endregion
    
    #region Select/Save/Load/Create/Close
    
    private void SelectScript(FlowNodeScriptData flowNodeScript)
    {
        isInGameList = ScriptList.Contains(flowNodeScript);
        scriptExists = AssetDatabase.Contains(flowNodeScript.GetInstanceID());
        selectedScript = flowNodeScript;
        selectedScript.OnAfterDeserialize();
    }

    private void CreateNewScript()
    {
        SelectScript(CreateInstance<FlowNodeScriptData>());
        AddStep();
    }
    
    private void OpenScriptOnPath(string scriptPath)
    {
        SelectScript(AssetDatabase.LoadAssetAtPath<FlowNodeScriptData>(scriptPath));
    }
    
    private void ReloadScript()
    {
        if(selectedScript != null && AssetDatabase.Contains(selectedScript.GetInstanceID()))
        {
            uncollapsedStepIndex = 0;
            selectedScript.OnAfterDeserialize();
        }
    }

    private bool SaveScript()
    {
        if (selectedScript == null)
        {
            EditorUtility.DisplayDialog("No Script Selected", "There's no Script opened or created to save.", "Continue");
            return false;
        }
        if (string.IsNullOrEmpty(selectedScript.Name))
        {
            EditorUtility.DisplayDialog("Script has no Name!", "You need to put a name to the Script before saving!", "Continue");
            return false;
        }
        
        if (string.IsNullOrEmpty(selectedScript.Identifier))
        {
            EditorUtility.DisplayDialog("Script has no Identifier!", "You need to put an Id to the Script before saving!", "Continue");
            return false;
        }

        selectedScript.OnBeforeSerialize();

        if (AssetDatabase.Contains(selectedScript.GetInstanceID()))
        {
            AddToGameList(isInGameList);
            EditorUtility.SetDirty(selectedScript);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Script Saved", "Script has been saved successfully.", "Continue");
            return true;
        }
        
        selectedScript.GUID = Guid.NewGuid().ToString();
        string ScriptFilePath = Path.Combine(FLOW_SCRIPTS_PATH, selectedScript.Name);

        if (File.Exists(ScriptFilePath))
        {
            EditorUtility.DisplayDialog("Script Exists", "A Script with this name already exists.", "Continue");
            return false;
        }

        if (!Directory.Exists(FLOW_SCRIPTS_PATH))
        {
            Directory.CreateDirectory(FLOW_SCRIPTS_PATH);
        }
        AddToGameList(isInGameList);
        AssetDatabase.CreateAsset(selectedScript, Path.Combine(FLOW_SCRIPTS_PATH, selectedScript.Name + ".asset"));
        EditorUtility.DisplayDialog("Script Created", "The new Script as been created!", "Continue");
        return true;
    }
    
    private void CloseScript()
    {
        uncollapsedStepIndex = 0;
        selectedScript = null;
    }

    #endregion

    #region Dialog Prompts
    
    private void TryCreateNew()
    {
        if (selectedScript != null)
        {
            int option = EditorUtility.DisplayDialogComplex("Create New?", 
                "There's a current Script already open. Do you want to save it before opening a new one?", 
                "Don't save", 
                "Save and Continue", 
                "Cancel");
            
            switch (option)
            {
                case 0:
                    CreateNewScript();
                    break;
                case 1:
                    if(SaveScript())
                    {
                        CreateNewScript();
                    }
                    break;
                case 2:
                    return;
            }
        }
        else
        {
            CreateNewScript();
        }
    }
    
    private void TryDeleteScript(bool fileExist)
    {
        if(!fileExist)
        {
            EditorUtility.DisplayDialog("The file doesn't exist", "This Script is already on edit mode, so the file doesn't exist yet. Create a new one to discard your changes.", "Continue");
        }
        else
        {
            int delete = EditorUtility.DisplayDialogComplex("Delete Script?", "The Script file will be erased. This action has no turning back. Do you want to continue?", "Delete", "Cancel", "");

            switch(delete)
            {
                case 0:
                    AddToGameList(false);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selectedScript.GetInstanceID()));
                    EditorUtility.DisplayDialog("Script Deleted", "The Script as been erased.", "Continue");
                    CreateNewScript();
                    break;
                case 1:
                    return;
            }
        }
    }
    
    private void TryCloseScript()
    {
        if(selectedScript == null)
        {
            return;
        }

        int beforeClose = EditorUtility.DisplayDialogComplex("Close without Saving?", "If you close it you will lose your progress. Do you want to continue?", "Close", "Cancel", "Save and Close");
        
        switch (beforeClose)
        {
            case 0:
                ScriptList.Remove(selectedScript);
                CloseScript();
                break;
            case 1:
                return;
            case 2:
                if(SaveScript())
                {
                    CloseScript();
                }
                break;
        }
    }

    #endregion

    #region Collapsables

    private void UncollapseStepAtIndex(int index)
    {
        uncollapsedStepIndex = index;
    }
    
    #endregion
}
