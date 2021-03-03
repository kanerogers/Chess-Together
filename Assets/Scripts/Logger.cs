using UnityEngine;

public class Logger {
    public static void Log(string tag, string message) {
        Debug.Log($"[{tag}] - {message}");
    }
}