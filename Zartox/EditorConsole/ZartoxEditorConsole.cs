using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.IO;
using System.Collections.Generic;

using Enums;
using Commands;

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
{//TODO: Ajouter un context lorsque l'on survole un des labels dans le scroll view, et ajouter un tooltip à ces labels donc créer une nvl fonc Log
    //TODO: Créer un objet log à chaque ligne écrite dans la console... Stocker un ID pour pouvoir les reconnaittre dans une liste et lorsque l'user
    //clique sur un label afficher les infos et afficher le contexte (surbillance de l'objet qui appele) 
//TODO: peut etre ajouter richText aux labels pour ajouter des couleurs (param dans Label)

    public static ZartoxEditorConsole console;

    #region Console refs
    private ToolbarButton toolbarClearBtn;
    private ToolbarButton toolbarOptionsBtn;
    private ScrollView logsScrollView;
    private VisualElement consoleVisualElement;
    private VisualElement optionsVisualElement;
    private TextField userCommandTextField;
    private Button sendCommandBtn;
    private Label logDescriptionLabel;
    #endregion

    #region Options refs
    private Toggle logToFileToggle;
    private Toggle pauseOnWarningToggle;
    private Toggle pauseOnErrorToggle;
    private Toggle clearOnPlayToggle;
    private Toggle clearOnRecompileToggle;
    private IntegerField maxConsoleLinesIntField;
    private Button saveBtn;
    #endregion

    private static DateTime consoleStartTime;
    private bool isOptionsOpened = false;
    private static string configDirPath;
    private static string configPath;
    private Dictionary<string, ICommand> commands;

    #region Config
    private bool logToFile = false;
    private int maxConsoleLines = 20;
    private bool pauseOnWarning = false;
    private bool pauseOnError = true;
    private bool clearOnPlay = false;
    private bool clearOnRecompile = true;

    //TODO: Ajouter les vars en dessous au fichier etc...
    private int consoleInputTextSize;
    private Color consoleInputColor;
    #endregion

    #region Init

    [MenuItem("Tools/TEST")]
    public static void Init()
    {
        ZartoxEditorConsole wnd = GetWindow<ZartoxEditorConsole>();
        wnd.titleContent = new GUIContent("ZartoxEditorConsole");
    }

    private void OnEnable()
    {
        consoleStartTime = DateTime.Now;

        configDirPath = Application.dataPath + "/Zartox/EditorConsole/Config";
        configPath = configDirPath + "/config.txt";

        if (console == null)
            console = GetWindow<ZartoxEditorConsole>();

        LoadConfig();
        RegisterCommands();
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
        userCommandTextField = root.Q<TextField>("userCommandTextField");
        sendCommandBtn = root.Q<Button>("sendCommandBtn");
        logDescriptionLabel = root.Q<Label>("logDescriptionLabel");

        //Options Refs
        logToFileToggle = root.Q<Toggle>("logToFileToggle");
        pauseOnWarningToggle = root.Q<Toggle>("pauseOnWarningToggle");
        pauseOnErrorToggle = root.Q<Toggle>("pauseOnErrorToggle");
        clearOnPlayToggle = root.Q<Toggle>("clearOnPlayToggle");
        clearOnRecompileToggle = root.Q<Toggle>("clearOnRecompileToggle");
        maxConsoleLinesIntField = root.Q<IntegerField>("maxConsoleLinesIntField");
        saveBtn = root.Q<Button>("saveBtn");

        //Events
        toolbarClearBtn.clicked += ClearConsole;
        toolbarOptionsBtn.clicked += HandleOptionsBtn;
        saveBtn.clicked += SaveConfig;
        sendCommandBtn.clicked += ExecuteUserCommand;


        userCommandTextField.label = Application.productName + ": ";
    }

    private void LoadConfig()
    {
        if (File.Exists(configPath))
        {
            Dictionary<string, string> configValues = new Dictionary<string, string>();
            string[] configFileLines = File.ReadAllLines(configPath);

            foreach (string line in configFileLines)
            {
                string[] keyValue = line.Split('=');

                if (keyValue.Length == 2)
                {
                    configValues[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }

            if (configValues.ContainsKey("logToFile"))
                logToFile = bool.Parse(configValues["logToFile"]);

            if (configValues.ContainsKey("pauseOnWarning"))
                pauseOnWarning = bool.Parse(configValues["pauseOnWarning"]);

            if (configValues.ContainsKey("pauseOnError"))
                pauseOnError = bool.Parse(configValues["pauseOnError"]);

            if (configValues.ContainsKey("clearOnPlay"))
                clearOnPlay = bool.Parse(configValues["clearOnPlay"]);

            if (configValues.ContainsKey("clearOnRecompile"))
                clearOnRecompile = bool.Parse(configValues["clearOnRecompile"]);

            if (configValues.ContainsKey("maxConsoleLines"))
                maxConsoleLines = int.Parse(configValues["maxConsoleLines"]);

            Log("Configuration loaded...", Level.Valid);

        }
        else
        {
            Log("Config not found... Please generate config file first...", Level.Error);
            GenerateDefaultConfigFile();
            LoadConfig();
        }
    }

    private void RegisterCommands()
    {
        commands = new Dictionary<string, ICommand>();

        //Add your commands here:
        commands["clear"] = new ClearCommand();
    }

    #endregion

    private void HandleOptionsBtn()
    {
        //1) Afficher/cacher la console/les options V
        //2) S'occupper de la logique pour le UI et mettre un bouton save qui save tout vers un fichier de config (si l'user ferme la mm) V
        //3) Dans Init aller choper les valeurs dans ce fichier
        if (!isOptionsOpened)
        {
            consoleVisualElement.style.display = DisplayStyle.None;
            optionsVisualElement.style.display = DisplayStyle.Flex;
            isOptionsOpened = true;
            SetOptionsToConfig();
        }
        else
        {
            consoleVisualElement.style.display = DisplayStyle.Flex;
            optionsVisualElement.style.display = DisplayStyle.None;
            isOptionsOpened = false;
        }
    }

    public void ClearConsole() { logsScrollView.Clear(); }

    private void SetOptionsToConfig()
    {
        //TODO: Set le UI des options aux valeurs de la config

            logToFileToggle.value = logToFile;
            pauseOnWarningToggle.value = pauseOnWarning;
            pauseOnErrorToggle.value = pauseOnError;
            clearOnPlayToggle.value = clearOnPlay;
            clearOnRecompileToggle.value = clearOnRecompile;
            maxConsoleLinesIntField.value = maxConsoleLines;
}

    private void SaveConfig()
    {
        //TODO: Save toutes les options dans la config

        logToFile = logToFileToggle.value;
        pauseOnWarning = pauseOnWarningToggle.value;
        pauseOnError = pauseOnErrorToggle.value;
        clearOnPlay = clearOnPlayToggle.value;
        clearOnRecompile = clearOnRecompileToggle.value;
        maxConsoleLines = maxConsoleLinesIntField.value;

        if (!Directory.Exists(configDirPath))
            Directory.CreateDirectory(configDirPath);

        using (StreamWriter sw = File.CreateText(configPath))
        {
            sw.WriteLine($"logToFile={logToFile}");
            sw.WriteLine($"pauseOnWarning={pauseOnWarning}");
            sw.WriteLine($"pauseOnError={pauseOnError}");
            sw.WriteLine($"clearOnPlay={clearOnPlay}");
            sw.WriteLine($"clearOnRecompile={clearOnRecompile}");
            sw.WriteLine($"maxConsoleLines={maxConsoleLines}");
        }

        AssetDatabase.Refresh();
        HandleOptionsBtn();
        Log("Config Saved...", Level.Valid);
    }

    public void Log(string message, Level logLevel=Level.Debug)
    {
        Label label = new Label(GetFormattedMessage(message, logLevel));
        label.focusable = true;
        label.AddToClassList("logLabel");
        SetLabelTextColor(label, logLevel);
        logsScrollView.Add(label);

        DeleteLogsOverLimit();

        logsScrollView.scrollOffset = new Vector2(0, float.MaxValue);
    }
    
    public void Log(string message, GameObject contextObject, Level logLevel=Level.Debug)
    {
        Label label = new Label(GetFormattedMessage(message, logLevel));
        label.focusable = true;
        label.AddToClassList("logLabel");
        SetLabelTextColor(label, logLevel);
        logsScrollView.Add(label);

        DeleteLogsOverLimit();

        logsScrollView.scrollOffset = new Vector2(0, float.MaxValue);

        label.RegisterCallback<FocusInEvent>(evt =>
        {
            HighlightObjectInHierarchy(contextObject);
        });
    }
    
    public void Log(string message, string description, Level logLevel = Level.Debug)
    {
        Label label = new Label(GetFormattedMessage(message, logLevel));
        label.focusable = true;
        label.AddToClassList("logLabel");
        SetLabelTextColor(label, logLevel);
        logsScrollView.Add(label);

        DeleteLogsOverLimit();

        logsScrollView.scrollOffset = new Vector2(0, float.MaxValue);

        label.RegisterCallback<FocusInEvent>(evt =>
        {
            logDescriptionLabel.text = description;
        });

        label.RegisterCallback<FocusOutEvent>(evt =>
        {
            logDescriptionLabel.text = "";
        });

    }
    
    public void Log(string message, string description, GameObject contextObject, Level logLevel = Level.Debug)
    {
        Label label = new Label(GetFormattedMessage(message, logLevel));
        label.focusable = true;
        label.AddToClassList("logLabel");
        SetLabelTextColor(label, logLevel);
        logsScrollView.Add(label);

        DeleteLogsOverLimit();

        logsScrollView.scrollOffset = new Vector2(0, float.MaxValue);

        label.RegisterCallback<FocusInEvent>(evt =>
        {
            logDescriptionLabel.text = description;
            HighlightObjectInHierarchy(contextObject);
        });

        label.RegisterCallback<FocusOutEvent>(evt =>
        {
            logDescriptionLabel.text = "";
        });
    }

    private void HighlightObjectInHierarchy(GameObject gameObject)
    {
        if (gameObject != null)
            EditorGUIUtility.PingObject(gameObject);
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

    #region Menu

    [MenuItem("Tools/Generate new config")]
    private static void GenerateDefaultConfigFile()
    {

        if (!Directory.Exists(configDirPath))
            Directory.CreateDirectory(configDirPath);

        using (StreamWriter sw = File.CreateText(configPath))
        {
            sw.WriteLine($"logToFile=False");
            sw.WriteLine($"pauseOnWarning=False");
            sw.WriteLine($"pauseOnError=True");
            sw.WriteLine($"clearOnPlay=False");
            sw.WriteLine($"clearOnRecompile=True");
            sw.WriteLine($"maxConsoleLines=50");
        }

        AssetDatabase.Refresh();
        console.Log("Generated a fresh config file with defaults values...", Level.Valid);
    }

    [MenuItem("Tools/Delete config file")]
    private static void DeleteConfigFile()
    {
        string metaPath = configPath + ".meta";

        if (File.Exists(configPath))
        {
            File.Delete(configPath);
            File.Delete(metaPath);
            AssetDatabase.Refresh();
            console.Log("Config file deleted...", Level.Valid);
        }
        else
        {
            console.Log("There is no config file to delete...", Level.Warning);
        }

    }

    #endregion

    #region Commands

    private void ExecuteUserCommand()
    {
        string command = userCommandTextField.value;

        if (commands.TryGetValue(command, out ICommand commandObject))
        {
            commandObject.Execute();
        }
        else
        {
            Log("Invalid command", Level.Error);
        }

        // Effacer le champ de texte après l'exécution
        userCommandTextField.value = "";
    }

    #endregion
}