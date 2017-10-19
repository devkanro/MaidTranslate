﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityInjector.ConsoleUtil;

namespace CM3D2.YATranslator.Plugin.Utils
{
    public enum DumpType
    {
        Strings = 0,
        Textures = 1,
        Assets = 2,
        TexSprites = 3,
        Voices = 4
    }

    public class LogLevel
    {
        public static readonly LogLevel Error = new LogLevel(ConsoleColor.Red);
        public static readonly LogLevel Minor = new LogLevel(ConsoleColor.DarkGray);
        public static readonly LogLevel Normal = new LogLevel(ConsoleColor.Gray);
        public static readonly LogLevel Warning = new LogLevel(ConsoleColor.Yellow);

        public LogLevel(ConsoleColor color)
        {
            Color = color;
        }

        public ConsoleColor Color { get; }
    }

    public static class Logger
    {
        private static readonly HashSet<DumpType> AllowedDumpTypes;
        private static HashSet<string> cachedDumps;
        private const string DUMP_FILENAME = "TRANSLATION_DUMP";
        private static bool dumpFileLoaded;

        private static readonly string[] DumpFolderNames =
        {
            "Strings",
            "Textures",
            "Assets",
            "Textures_Sprites",
            "Audio"
        };

        private static TextWriter dumpStream;

        static Logger()
        {
            AllowedDumpTypes = new HashSet<DumpType>();
        }

        public static bool DumpEnabled => AllowedDumpTypes.Count != 0 && !string.IsNullOrEmpty(DumpPath);

        public static string DumpPath { get; set; }

        public static DumpType[] DumpTypes
        {
            set
            {
                AllowedDumpTypes.Clear();
                foreach (DumpType dumpType in value)
                    AllowedDumpTypes.Add(dumpType);
            }
        }

        public static ResourceType Verbosity { get; set; }

        public static bool CanDump(DumpType type) => AllowedDumpTypes.Contains(type);

        public static bool InitDump()
        {
            if (!DumpEnabled)
                return false;
            if (dumpFileLoaded)
                return true;
            WriteLine("Trying to initialize dumping");
            try
            {
                if (!Directory.Exists(DumpPath))
                    Directory.CreateDirectory(DumpPath);
                for (int index = 0; index < DumpFolderNames.Length; index++)
                {
                    string folderPath = Path.Combine(DumpPath, DumpFolderNames[index]);
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    DumpFolderNames[index] = folderPath;
                }
            }
            catch (Exception e)
            {
                WriteLine(LogLevel.Error, $"Failed to create dump directory because {e.Message}");
                return false;
            }
            string dumpFilePath = Path.Combine(GetDumpFolderName(DumpType.Strings),
                                               $"{DUMP_FILENAME}.{DateTime.Now:yyyy-MM-dd-HHmmss}.txt");
            try
            {
                dumpStream = File.CreateText(dumpFilePath);
                dumpFileLoaded = true;
                cachedDumps = new HashSet<string>();
            }
            catch (Exception e)
            {
                WriteLine(LogLevel.Error, $"Failed to create dump to {dumpFilePath}. Reason: {e.Message}");
                return false;
            }
            return true;
        }

        public static void Dispose()
        {
            if (!dumpFileLoaded)
                return;

            dumpStream.Flush();
            dumpStream.Dispose();
        }

        public static void DumpLine(string line, int level, DumpType dumpType = DumpType.Strings)
        {
            if (!CanDump(dumpType) || !InitDump())
                return;
            if (cachedDumps.Contains(line))
                return;
            cachedDumps.Add(line);
            lock (dumpStream)
            {
                dumpStream.WriteLine($"[{dumpType}][LEVEL {level}] {line}");
                dumpStream.Flush();
            }
        }

        public static void WriteLine(ResourceType verbosity, LogLevel logLevel, string message)
        {
            if (IsLogging(verbosity))
                WriteLine(logLevel, message);
        }

        public static void WriteLine(ResourceType verbosity, string message)
        {
            WriteLine(verbosity, LogLevel.Normal, message);
        }

        public static bool IsLogging(ResourceType verbosity) => (verbosity & Verbosity) != 0;

        public static void WriteLine(LogLevel logLevel, string message)
        {
            ConsoleColor oldColor = SafeConsole.ForegroundColor;
            SafeConsole.ForegroundColor = logLevel.Color;
            Console.WriteLine(message);
            SafeConsole.ForegroundColor = oldColor;
        }

        public static void WriteLine(string message)
        {
            WriteLine(LogLevel.Normal, message);
        }

        public static void DumpTexture(DumpType dumpType, string name, Texture2D texture, bool duplicate)
        {
            if (!CanDump(dumpType) || texture == null)
                return;
            if (!InitDump())
                return;

            if (cachedDumps.Contains(name))
                return;
            cachedDumps.Add(name);

            if (name.StartsWith("!"))
                return;

            string path = Path.Combine(GetDumpFolderName(dumpType), $"{name}.png");
            if (File.Exists(path))
                return;

            Texture2D tex = duplicate ? Duplicate(texture) : texture;
            WriteLine($"Translation::Dumping {name}.png");
            using (FileStream fs = File.Create(path))
            {
                byte[] pngData = tex.EncodeToPNG();
                fs.Write(pngData, 0, pngData.Length);
                fs.Flush();
            }
        }

        public static void DumpVoice(string name, AudioClip clip)
        {
            if (!CanDump(DumpType.Voices) || clip == null || !InitDump())
                return;

            if (cachedDumps.Contains(name))
                return;
            cachedDumps.Add(name);

            string path = Path.Combine(GetDumpFolderName(DumpType.Voices), $"{name}.wav");
            if (File.Exists(path))
                return;

            WriteLine($"Translation::Dumping {name}.wav");

            using (FileStream fs = File.Create(path))
            {
                byte[] wavData = clip.ToWavData();
                fs.Write(wavData, 0, wavData.Length);
                fs.Flush();
            }
        }

        private static string GetDumpFolderName(DumpType dumpType) => DumpFolderNames[(int) dumpType];

        private static Texture2D Duplicate(Texture texture)
        {
            RenderTexture render = RenderTexture.GetTemporary(texture.width,
                                                              texture.height,
                                                              0,
                                                              RenderTextureFormat.Default,
                                                              RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, render);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = render;
            Texture2D result = new Texture2D(texture.width, texture.height);
            result.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
            result.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(render);
            return result;
        }
    }
}