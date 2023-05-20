using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ZartoxEditorConsole;

public class TESTMONO : MonoBehaviour
{

    private void Start()
    {
        Init();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //TestEveryLog();
            LogRandomNumber();
        }
    }

    private void LogRandomNumber()
    {
        console.Log(Random.Range(0, 100).ToString(), (Enums.Level)Random.Range(0, 5));
    }

    private void TestEveryLog()
    {
        console.Log("Hello from MonoBehaviour!");
        console.Log("Hello from MonoBehaviour!", Enums.Level.Info);
        console.Log("Hello from MonoBehaviour!", Enums.Level.Warning);
        console.Log("Hello from MonoBehaviour!", Enums.Level.Error);
        console.Log("Hello from MonoBehaviour!", Enums.Level.Valid);
    }
}
