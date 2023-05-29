# Zartox_Custom_editor_console
A custom console made with UiToolkit in Unity.

TAKING A BREAK FROM THIS PROJECT, IT'S NOT FINISHED BUT FEEL FREE TO MODIFY IT :)
I'll add informations on how to use it once it's done.

## Changelog:
---------------------------------------
### 0.0.1:
        - Display lines in the console via Log function
        - 5 different log levels 
        - Clear the console
        - Set max number of lines in the console
        - Added Timestamps
---------------------------------------
### 0.0.2:
        - Decided that design is not the priority...
        - Added an option menu
        - Added a config file to save settings
        - Added a menu button to Generate a default config file
        - Added a menu button to Delete the actual config file
---------------------------------------
### 0.0.3:
        - Added interface to send commands to the console (early)
        - Added a test command (clear, but that does not clear the console)
        - Added a way to create simple custom commands
---------------------------------------
### 0.0.4:
        - Added a clear command
        - Added a command success message
        - Added a command failed message
        - Added a description to logs (optional, made to have more infos without making a mess inside the console)
        - Added focus on logs to select them
        - Added a context for logs (when clicked: highlight gameObject in hierarchy)
---------------------------------------
### 0.0.5:
        - Added play mode pause on error (optional)
        - Added play mode pause on warning (optional)
        - Added clear console on play mode enter (optional)
        - Added clear console on recompile (optional)
        - Added a simple way to create help messages for every command you created
        - Added a file that keep trace of every line in the console (optional)
        - Added a delete every log menu button
        - Added a delete every log file created today...
        - Every log file created is now named the date it was created
        - Added a mark in the log file for playmode start and playmode end 
---------------------------------------
### 0.0.6:
        - Enabled RichText in the console (you can now use <b>test</b>, etc...)
---------------------------------------


## Known bugs:
---------------------------------------
        - Opening 2 instances of the console [Temporary fix: reset yout UI to default]: FIXED
---------------------------------------
