using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Enums;
using System;

namespace Enums
{
    public enum Level
    {
        Debug,
        Info,
        Warning,
        Error,
        Valid
    }
}


public class ZartoxEditorConsole : EditorWindow
{

    public static ZartoxEditorConsole console;

    #region Console refs
    private ToolbarButton toolbarClearBtn;
    private ToolbarButton toolbarOptionsBtn;
    private ScrollView logsScrollView;
    private VisualElement consoleVisualElement;
    private VisualElement optionsVisualElement;
    #endregion

    #region Options refs
    private Toggle logToFileToggle;
    private Toggle pauseOnWarningToggle;
    private Toggle pauseOnErrorToggle;
    private Toggle clearOnPlayToggle;
    private Toggle clearOnRecompileToggle;
    private IntegerField maxConsoleLinesIntField;
    #endregion

    private static DateTime consoleStartTime;
    private bool isOptionsOpened = false;

    #region Config
    private bool logToFile = false;
    private int maxConsoleLines = 20;
    private bool pauseOnWarning = false;
    private bool pauseOnError = false;
    private bool clearOnPlay = false;
    private bool clearOnRecompile = false;
    #endregion

    #region Init

    [MenuItem("Tools/TEST")]
    public static void Init()
    {
        ZartoxEditorConsole wnd = GetWindow<ZartoxEditorConsole>();
        wnd.titleContent = new GUIContent("ZartoxEditorConsole");
        consoleStartTime = DateTime.Now;


        if (console != null)
            return;
        else
            console = GetWindow<ZartoxEditorConsole>();
    }

    private void OnEnable()
    {
       
    }

    private void OnDisable()
    {
        
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Zartox/EditorConsole/ZartoxEditorConsole.uxml");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Zartox/EditorConsole/ZartoxEditorConsole.uss");

        // Attacher le fichier UXML à la racine + Style
        visualTree.CloneTree(root);
        root.styleSheets.Add(styleSheet);

        //Console Refs
        logsScrollView = root.Q<ScrollView>("logsScrollView");
        toolbarClearBtn = root.Q<ToolbarButton>("toolbarClearBtn");
        toolbarOptionsBtn = root.Q<ToolbarButton>("toolbarOptionsBtn");
        optionsVisualElement = root.Q<VisualElement>("optionsVisualElement");
        consoleVisualElement = root.Q<VisualElement>("consoleVisualElement");

        //Options Refs
        logToFileToggle = root.Q<Toggle>("logToFileToggle");
        pauseOnWarningToggle = root.Q<Toggle>("pauseOnWarningToggle");
        pauseOnErrorToggle = root.Q<Toggle>("pauseOnErrorToggle");
        clearOnPlayToggle = root.Q<Toggle>("clearOnPlayToggle");
        clearOnRecompileToggle = root.Q<Toggle>("clearOnRecompileToggle");
        maxConsoleLinesIntField = root.Q<IntegerField>("maxConsoleLinesIntField");

        //Events
        toolbarClearBtn.clicked += ClearConsole;
        toolbarOptionsBtn.clicked += HandleOptionsBtn;

    }

    #endregion

    #region Events
    private void HandleOptionsBtn()
    {
        //1) Afficher/cacher la console/les options
        //2) S'occupper de la logique pour le UI et mettre un bouton save qui save tout vers un fichier de config (si l'user ferme la mm)
        //3) Dans Init aller choper les valeurs dans ce fichier
        if (!isOptionsOpened)
        {
            consoleVisualElement.style.display = DisplayStyle.None;
            optionsVisualElement.style.display = DisplayStyle.Flex;
            isOptionsOpened = true;
        }
        else
        {
            consoleVisualElement.style.display = DisplayStyle.Flex;
            optionsVisualElement.style.display = DisplayStyle.None;
            isOptionsOpened = false;
        }
    }

    private void ClearConsole()
    {
        logsScrollView.Clear();
    }

    #endregion

    private void SetOptionsToConfig()
    {
        //TODO: Set le UI des options aux valeurs de la config
    }

    private void LoadConfig()
    {
        //TODO: Charge et set les vars avec la config inscrite dans le fichier
    }

    private void SaveConfig()
    {
        //TODO: Save toutes les options dans la config
    }

    public void Log(string message, Level logLevel=Level.Debug)
    {
        Label label = new Label(GetFormattedMessage(message, logLevel));
        SetLabelTextColor(label, logLevel);
        logsScrollView.Add(label);

        DeleteLogsOverLimit();

        //set to lowest item
        logsScrollView.scrollOffset = new Vector2(0, float.MaxValue);
    }

    private void DeleteLogsOverLimit()
    {
        if (logsScrollView.childCount > maxConsoleLines)
            logsScrollView.RemoveAt(0);
    }

    private string GetFormattedMessage(string message, Level logLevel)
    {
        TimeSpan elapsedTime = DateTime.Now - consoleStartTime;
        string elapsedTimeString = $"{elapsedTime.Hours:D2}:{elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}:{elapsedTime.Milliseconds / 10:D2}:{elapsedTime.Milliseconds % 10:D2}";//CHANGER LE DERNIER D2 EN D1 SI BUG

        return $"{elapsedTimeString} : [{logLevel.ToString().ToUpper()}] {message}";
    }

    private void SetLabelTextColor(Label label, Level logLevel)
    {
        switch (logLevel)
        {
            case Level.Debug:
                label.style.color = Color.white;
                break;
            case Level.Info:
                label.style.color = Color.cyan;
                break;
            case Level.Warning:
                label.style.color = Color.yellow;
                break;
            case Level.Error:
                label.style.color = Color.red;
                break;
            case Level.Valid:
                label.style.color = Color.green;
                break;
        }
    }
}