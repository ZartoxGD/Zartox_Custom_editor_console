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
{
    //TODO: Créer un objet log à chaque ligne écrite dans la console... Stocker un ID pour pouvoir les reconnaitre dans une liste et lorsque l'user
//TODO: peut etre ajouter richText aux labels pour ajouter des couleurs (param dans Label ui builder)
//TODO: dans le fichier texte log à la date d'aujourdhui si logToFile == true alors à chaque fois que
//l'on quitte le mode play rajouter une ligne à la fin de ce fichier pour signifier une test 

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

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

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

    private void OnAfterAssemblyReload()
    {
        if(clearOnRecompile)
            ClearConsole();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        string fileName = Application.dataPath + "/Logs/" + DateTime.Now.ToString("yyyy-MM-dd") + "_ConsoleLogs.txt";
        //TODO: Si le fichier n'éxiste pas et que logtofile ==true alors le créer et rajouter la ligne start

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            if (clearOnPlay)
                ClearConsole();

            if (File.Exists(fileName) && logToFile)
            {
                File.AppendAllText(fileName, "\n");
                File.AppendAllText(fileName, "-------------SESSION START-------------\n");
            }
        }
        else if (state == PlayModeStateChange.ExitingPlayMode && logToFile)
        {
            if (File.Exists(fileName))
            {
                File.AppendAllText(fileName, "-------------SESSION END-------------\n");
                File.AppendAllText(fileName, "\n");
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {

        }
        else if (state == PlayModeStateChange.ExitingEditMode)
        {

        }
    }

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

            logToFileToggle.value = logToFile;
            pauseOnWarningToggle.value = pauseOnWarning;
            pauseOnErrorToggle.value = pauseOnError;
            clearOnPlayToggle.value = clearOnPlay;
            clearOnRecompileToggle.value = clearOnRecompile;
            maxConsoleLinesIntField.value = maxConsoleLines;
}

    private void SaveConfig()
    {

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

        LogToFile(message, logLevel);
        PausePlayMode(logLevel);
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

        LogToFile(message, logLevel);
        PausePlayMode(logLevel);
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

        LogToFile(message, logLevel);
        PausePlayMode(logLevel);
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

        LogToFile(message, logLevel);
        PausePlayMode(logLevel);
    }

    private void LogToFile(string message, Level level = Level.Debug)
    {
        if (!logToFile)
            return;

        string directoryPath = Application.dataPath + "/Logs";

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "_ConsoleLogs.txt";
        string filePath = directoryPath + "/" + fileName;

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            writer.WriteLine($"[{timestamp}] [{level.ToString().ToUpper()}] : {message}");
        }
    }

    private void HighlightObjectInHierarchy(GameObject gameObject)
    {
        if (gameObject != null)
            EditorGUIUtility.PingObject(gameObject);
        else
            Log("The object you passed for context as probably been destroyed... Can't highlight it in hierarchy.", Level.Warning);
    }

    private void DeleteLogsOverLimit()
    {
        if (logsScrollView.childCount > maxConsoleLines)
            logsScrollView.RemoveAt(0);
    }

    private void PausePlayMode(Level logLevel)
    {
        if (logLevel == Level.Warning && pauseOnWarning)
            EditorApplication.isPaused = true;
        
        if (logLevel == Level.Error && pauseOnError)
            EditorApplication.isPaused = true;
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

    [MenuItem("Tools/Delete today's Logs")]
    public static void DeleteTodaysLogs()
    {
        string logsDir = Application.dataPath + "/Logs";
        string[] logFiles = Directory.GetFiles(logsDir);
        int counter = 0;

        foreach (string logFile in logFiles)
        {
            FileInfo fileInfo = new FileInfo(logFile);

            if (fileInfo.CreationTime.Date == DateTime.Today)
            {
                File.Delete(logFile);
                counter++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[Zartox Log] : Deleted today's logs ({counter / 2} files) in /Logs...");
    }

    [MenuItem("Tools/Delete every Log")]
    public static void DeleteEveryLogs()
    {
        string[] filePaths = Directory.GetFiles(Application.dataPath + "/Logs");
        int counter = 0;

        foreach (string filePath in filePaths)
        {
            File.Delete(filePath);
            counter++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[Zartox Log] : Deleted every logs ({counter / 2} files)...");
    }

    #endregion

    #region Commands

    private void ExecuteUserCommand()
    {
        string completeCommand = userCommandTextField.value;
        string[] parts = completeCommand.Split(' ');
        string command = parts[0];

        if (commands.TryGetValue(completeCommand, out ICommand commandObject))
        {
            commandObject.Execute();
        }
        else if (parts[1] == "help" && commands.TryGetValue(command, out ICommand obj))
        {
            Log(obj.help, $"Help message for the {obj.name} command...", Level.Info);
        }
        else
        {
            Log("Invalid command", Level.Warning);
        }

        userCommandTextField.value = "";
    }

    #endregion
}