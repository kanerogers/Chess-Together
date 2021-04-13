using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class Logger {
    public static StringBuilder builder = new StringBuilder();
    public static StringBuilder fileLoggerBuilder = new StringBuilder();
    public static string logFileName = GetLogFileName();

    private static string GetLogFileName() {
        DateTime foo = DateTime.Now;
        long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();
        var fileName = $"{unixTime.ToString()}.log";
        return fileName;
    }

    private static StreamWriter GetWriter() {
#if UNITY_EDITOR
        return new StreamWriter(logFileName, append: true);
#else 
        return null;
#endif
    }

    public static void Log(string tag, params object[] list) {
        builder.Clear();

        if (list.Length != 0) {
            builder.Append("[");
            builder.Append(tag);
            builder.Append("] - ");

            foreach (var obj in list) {
                builder.Append(obj.ToString());
                builder.Append(" ");
            }
        } else {
            builder.Append(tag);
        }

        var s = builder.ToString();
        Debug.Log(s);

#if UNITY_EDITOR
        LogToFile(s);
        fileLoggerBuilder.Clear();
#endif
    }

    private static void LogToFile(string s) {
        fileLoggerBuilder.Append(DateTime.Now);
        fileLoggerBuilder.Append(" - ");
        fileLoggerBuilder.Append(s);

        using (StreamWriter file = GetWriter()) {
            file.WriteLine(fileLoggerBuilder.ToString());
        };
    }

    public static void LogError(string s) {
        Log("ERROR", s);
    }
}