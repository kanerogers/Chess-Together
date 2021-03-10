using UnityEngine;

public class Logger {
    public static void Log(string tag, string message) {
        Debug.Log($"[{tag}] - {message}");
    }

    public static void Log(string message) {
        Debug.Log($"[LAZY] - {message}");
    }
}