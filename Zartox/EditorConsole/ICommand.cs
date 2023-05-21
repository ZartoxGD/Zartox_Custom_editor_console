using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    string name { get; } //Name of the command (the word the user will enter in the console)
    string description { get; } //Describe the effect of this command
    string help { get; } //Sentence to show when the user made a mistake with the args
    string onSuccess { get; } //Sentence to show in the console if the command is a success (if == "" don't send this to console)

    //TODO: faire retourner une bool � la fin de la commande... Si �a a fonctionn� true sinon false et envoyer le log onSuccess ou onFail (� cr�er) � la console

    public void Execute();
}
