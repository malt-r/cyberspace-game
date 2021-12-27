using System.Diagnostics;
using UnityEngine;
using UnityEditor;

public class MenuItems
{
    [MenuItem("Tools/Generate Voice Lines")]
    private static void GenerateVoiceLines()
    {
        string pathToExe = Application.dataPath.Replace(@"/", @"\") + "\\..\\Tools\\Text2Speech\\";
        string pathToLines = Application.dataPath.Replace(@"/", @"\") + "\\AudioClips\\Speaker\\lines.csv";
        string output = Application.dataPath.Replace(@"/", @"\") + "\\AudioClips\\Speaker\\";
        string command = pathToExe + "SpeechTest.exe " + pathToLines + " " + output;
        //command = "-NoExit -Command " + command;

        Process proc = Process.Start("powershell.exe", command);
        proc.WaitForExit();
        proc.Close();
    }
}