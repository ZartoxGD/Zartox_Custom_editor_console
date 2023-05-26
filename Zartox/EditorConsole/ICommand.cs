using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    string name { get; } //Name of the command (the word the user will enter in the console)
    string help { get; } //Sentence to show when the user made a mistake with the args
    string onSuccessMessage { get; } //Sentence to show in the console if the command is a success (if == "" don't send this to console)
    string onFailMessage { get; } //Sentence to show if the command fails

    public void Execute();
}
