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

        public string onSuccess => "";

        public void Execute()
        {
            Debug.Log("CLEAR");
        }
    }
}

