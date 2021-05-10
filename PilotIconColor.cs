using BattleTech;
using BattleTech.UI;
using Harmony;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace PilotIconColor
{
    public class PilotIconColor
    {
        internal static ModSettings settings;
        internal static string ModDirectory;

        public static void Init(string directory, string modSettings)
        {
            ModDirectory = directory;
            try
            {
                settings = JsonConvert.DeserializeObject<ModSettings>(modSettings);
            }
            catch (Exception e)
            {
                Logger.NewLog();
                Logger.LogError(e);
            }
            var harmony = HarmonyInstance.Create("sqparadox.PilotIconColor");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        [HarmonyAfter(new string[] { "sqparadox.NoKickstarterIcon" })]
        [HarmonyPatch(typeof(SimGameState), "GetPilotTypeColor")]
        public static class NoKickstarterIcon_GetPilotTypeColor_Patch
        {  
            public static UIColor PilotColorFromString(string color, UIColor originalColor)
            {
                UIColor IconColor;
                try
                {
                    IconColor = (UIColor)Enum.Parse(typeof(UIColor), color);
                }
                catch (Exception)
                {
                    if (color.ToLower() == "commander")
                        IconColor = UIColor.SimPilotCommander;
                    else if (color.ToLower() == "backer")
                        IconColor = UIColor.SimPilotBacker;
                    else if (color.ToLower() == "ronin")
                        IconColor = UIColor.SimPilotRonin;
                    else
                    {
                        Logger.LogLine($"Failed to parse color {color}, using incoming color of {originalColor}");
                        IconColor = originalColor;
                    }
                }
                return IconColor;
            }
            public static bool Prepare()
            {
                return settings.BackerIconColor != "backer" || settings.RoninIconColor != "ronin" || settings.CommanderIconColor != "commander";
            }
            public static void Postfix(SimGameState __instance, Pilot p, ref UIColor __result, Pilot ___commander)
            {
                if (p.pilotDef.Description.Id == ___commander.Description.Id)
                    __result = PilotColorFromString(settings.CommanderIconColor, __result);
                else if (p.pilotDef.IsVanguard)
                    __result = PilotColorFromString(settings.BackerIconColor, __result);
                else if (p.pilotDef.IsRonin)
                    __result = PilotColorFromString(settings.RoninIconColor, __result);
                else
                    __result = UIColor.SimPilotStandard;
            }
        }

        public class Logger
        {
            static readonly string filePath = $"{ModDirectory}\\Log.txt";
            public static void NewLog()
            {
                using (StreamWriter streamWriter = new StreamWriter(filePath, false))
                {
                    streamWriter.WriteLine("");
                }
            }
            public static void LogError(Exception ex)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

            public static void LogLine(String line)
            {
                using (StreamWriter streamWriter = new StreamWriter(filePath, true))
                {
                    streamWriter.WriteLine(DateTime.Now.ToString() + Environment.NewLine + line + Environment.NewLine);
                }
            }
        }
        internal class ModSettings
        {
            public string BackerIconColor = "backer";
            public string RoninIconColor = "ronin";
            public string CommanderIconColor = "commander";
        }
    }
}
