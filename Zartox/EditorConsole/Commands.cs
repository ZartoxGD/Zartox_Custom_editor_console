using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Commands
{ 
    public class Commands : MonoBehaviour
    {

    }

    public class ClearCommand : ICommand
    {
        public string name => "clear";

        public string description => "Clear the console of any log";

        public string help => "clear";

        public string onSuccessMessage => "";

        public string onFailMessage => "Command execution failed";

        public void Execute()
        {
            try
            {
                ZartoxEditorConsole.console.ClearConsole();

                if (!string.IsNullOrEmpty(onSuccessMessage))
                {
                    ZartoxEditorConsole.console.Log(onSuccessMessage, Enums.Level.Valid);
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(onFailMessage))
                {
                    ZartoxEditorConsole.console.Log($"{onFailMessage}");
                    ZartoxEditorConsole.console.Log($"{e.Message}", Enums.Level.Error);
                }
            }
        }
    }
}

