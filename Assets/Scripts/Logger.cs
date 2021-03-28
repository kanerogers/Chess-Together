using System.Text;
using UnityEngine;

public static class Logger {
    public static StringBuilder builder = new StringBuilder();
    public static void Log(string tag, params object[] list) {
        builder.Clear();
        builder.Append("[");
        builder.Append(tag);
        builder.Append("] - ");
        foreach (var obj in list) {
            builder.Append(obj.ToString());
            builder.Append(" ");
        }
        Debug.Log(builder.ToString());
    }
}