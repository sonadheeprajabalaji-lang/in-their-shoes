using System;
using System.IO;
using UnityEngine;

// Writes one anonymous line per response to a CSV file.
// No names or emails are stored, only a random session ID.
public class ResponseLogger : MonoBehaviour
{
    // Will be controlled by the consent screen later.
    // If the player declines, this is set to false and nothing is saved.
    public bool loggingEnabled = true;

    string sessionId;
    string filePath;

    void Awake()
    {
        sessionId = Guid.NewGuid().ToString().Substring(0, 8);
        filePath = Path.Combine(Application.persistentDataPath, "response_log.csv");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "session_id,chapter,scene,choice,response,ms\n");
        }

        Debug.Log("Response log file: " + filePath);
    }

    public void Log(int chapter, string scene, string choice, string response, int ms)
    {
        if (!loggingEnabled) return;

        string line = sessionId + "," +
                      chapter + "," +
                      Clean(scene) + "," +
                      Clean(choice) + "," +
                      Clean(response) + "," +
                      ms;

        File.AppendAllText(filePath, line + "\n");
    }

    // Wraps text in quotes if it contains a comma, so the CSV stays valid
    static string Clean(string text)
    {
        if (text.Contains(",") || text.Contains("\""))
        {
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
        return text;
    }
}