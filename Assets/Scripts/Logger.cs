using System.Text;
using UnityEngine;

public static class Logger {
    public static StringBuilder builder = new StringBuilder();
    public static bool AT_CORRECT_TURN = false;
    public static bool SPECIAL_DEBUG = false;
    public static void Log(string tag, params object[] list) {
        if (!SPECIAL_DEBUG) {
            if (tag == "SPECIAL_DEBUG") return;
            if (tag == "UPDATE EP") return;
            if (tag == "UNDO EP") return;
        }
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