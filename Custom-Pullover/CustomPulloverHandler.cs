using System;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Windows.Forms;
using System.Reflection;

[assembly: Rage.Attributes.Plugin("Custom Pullover", Description = "INSTALL IN PLUGINS/LSPDFR", Author = "Waynieoaks")]
namespace Custom_Pullover
{
    public class EntryPoint
    {
        public static void Main()
        {
            Game.DisplayNotification("You have installed Custom Pullover incorrectly and in the wrong folder: you must install it in Plugins/LSPDFR. It will then be automatically loaded when going on duty - you must NOT load it yourself via RAGEPluginHook.");
            return;
        }
    }

    internal class CustomPulloverHandler
    {

        private static void LoadValuesFromIniFile()
        {
            try
            {
                TrafficStopFollowKey = (Keys)kc.ConvertFromString(GetTrafficStopFollowKey());
                TrafficStopFollowModifierKey = (Keys)kc.ConvertFromString(GetTrafficStopFollowModifierKey());
                TrafficStopMimicKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "TrafficStopMimicKey"));
                TrafficStopMimicModifierKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "TrafficStopMimicModifierKey"));
                IniDefaults.PositionUpKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "PositionUpKey", "NumPad9"));
                IniDefaults.PositionRightKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "PositionRightKey", "NumPad6"));
                IniDefaults.PositionResetKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "PositionResetKey", "NumPad5"));
                IniDefaults.PositionLeftKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "PositionLeftKey", "NumPad4"));
                IniDefaults.PositionForwardKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "PositionForwardKey", "NumPad8"));
                IniDefaults.PositionDownKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "PositionDownKey", "NumPad3"));
                IniDefaults.PositionBackwardKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "PositionBackwardKey", "NumPad2"));
                TrafficStopAssist.VehicleDoorLockDistance = InitialiseFile().ReadSingle("Features", "VehicleDoorLockDistance", 5.2f);
                TrafficStopAssist.VehicleDoorUnlockDistance = InitialiseFile().ReadSingle("Features", "VehicleDoorUnlockDistance", 3.5f);
                AutoVehicleDoorLock = InitialiseFile().ReadBoolean("Features", "AutoVehicleDoorLock", true);
                TrafficStopAIYieldEnabled = InitialiseFile().ReadBoolean("Features", "TrafficStopAIYieldEnabled", false);
                CustomPulloverLocationKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "CustomPulloverLocationKey", "W"));
                CustomPulloverLocationModifierKey = (Keys)kc.ConvertFromString(InitialiseFile().ReadString("Keybindings", "CustomPulloverLocationModifierKey", "LControlKey"));
            }
            catch (Exception e)
            {
                TrafficStopFollowKey = Keys.T;
                TrafficStopFollowModifierKey = Keys.LControlKey;
                TrafficStopMimicKey = Keys.R;
                TrafficStopMimicModifierKey = Keys.LControlKey;
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("Loading default Custom Pullover INI file - Error detected in user's INI file.");
                Game.DisplayNotification("~r~~h~Error~s~ reading Custom Pullover ini file. Default values set; replace with default INI file!");
            }

        }
        public static Keys CustomPulloverLocationKey = Keys.W;
        public static Keys CustomPulloverLocationModifierKey = Keys.LControlKey;
        public static Keys TrafficStopMimicModifierKey { get; set; }
        public static KeysConverter kc = new KeysConverter();
        public static Keys TrafficStopFollowModifierKey { get; set; }
        public static Keys TrafficStopFollowKey { get; set; }
        // private static Keys parkModifierKey { get; set; }
        public static bool IsSomeoneRunningTheLight { get; set; }
        public static bool IsSomeoneFollowing { get; set; }
        public static Keys TrafficStopMimicKey { get; set; }

        public static bool AutoVehicleDoorLock = true;
        public static bool TrafficStopAIYieldEnabled = false;

        internal static void MainLoop()
        {
            Game.LogTrivial("Custom Pullover.Mainloop started");
            
            Game.LogTrivial("Loading Custom Pullover settings...");
            LoadValuesFromIniFile();

            IsSomeoneFollowing = false;
            IsSomeoneRunningTheLight = false;

            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();
                    if (Functions.IsPlayerPerformingPullover())
                    {

                        if (ExtensionMethods.IsKeyDownRightNowComputerCheck(TrafficStopFollowModifierKey) || (TrafficStopFollowModifierKey == Keys.None))
                        {
                            if (ExtensionMethods.IsKeyDownComputerCheck(TrafficStopFollowKey))
                            {
                                if (!IsSomeoneFollowing)
                                {
                                    TrafficStopAssist.FollowMe();
                                }
                                else { IsSomeoneFollowing = false; }
                            }
                        }
                        if (ExtensionMethods.IsKeyDownRightNowComputerCheck(TrafficStopMimicModifierKey) || (TrafficStopMimicModifierKey == Keys.None))
                        {
                            if (ExtensionMethods.IsKeyDownComputerCheck(TrafficStopMimicKey))
                            {
                                if (!IsSomeoneFollowing)
                                {
                                    TrafficStopAssist.MimicMe();
                                }
                                else
                                {
                                    IsSomeoneFollowing = false;
                                }
                            }
                        }
                        if (ExtensionMethods.IsKeyDownRightNowComputerCheck(CustomPulloverLocationModifierKey) || (CustomPulloverLocationModifierKey == Keys.None))
                        {
                            if (ExtensionMethods.IsKeyDownComputerCheck(CustomPulloverLocationKey))
                            {
                                if (!IsSomeoneFollowing)
                                {
                                    TrafficStopAssist.SetCustomPulloverLocation();
                                }
                                else
                                {
                                    Game.LogTrivial("Already doing custom pullover location.");
                                }
                            }
                        }

                        if (!IsSomeoneRunningTheLight)
                        {
                            TrafficStopAssist.CheckForceRedLightRun();
                        }
                    }
                }
            });

            while (true)
            {
                GameFiber.Yield();
                if (TrafficStopAIYieldEnabled)
                {
                    TrafficStopAssist.CheckForYieldDisable();
                }

                if (AutoVehicleDoorLock)
                {
                    TrafficStopAssist.LockPlayerDoors();
                }

                //VehicleDetails.CheckForTextEntry();
            }

        }
        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName(); if (an.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0) { return true; }
                }
            }
            return false;
        }

        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args) { foreach (Assembly assembly in Functions.GetAllUserPlugins()) { if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower())) { return assembly; } } return null; }

        public static InitializationFile InitialiseFile()
        {
            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/Custom Pullover.ini");
            ini.Create();
           // Game.LogTrivial("Custom Pullover: Reading INI file.");
            return ini;
        
        }

        private static string GetTrafficStopFollowKey()
        {
            InitializationFile ini = InitialiseFile();
            string key = ini.ReadString("Keybindings", "TrafficStopFollowKey", "T");
            return key;
        }
        private static string GetTrafficStopFollowModifierKey()
        {
            InitializationFile ini = InitialiseFile();
            string key = ini.ReadString("Keybindings", "TrafficStopFollowModifierKey", "LControlKey");
            return key;
        }
        
        internal static void Initialise()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
            
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("Custom Pullover has been loaded successfully! [CustomPulloverHandler.cs-201]");
                GameFiber.Wait(6000);
                Game.DisplayNotification("~b~Custom Pullover~b~ " + Assembly.GetExecutingAssembly().GetName().Version.ToString() );
                MainLoop();
                
            });
        }
    }
}
