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

        private static void loadValuesFromIniFile()
        {
            try
            {
                trafficStopFollowKey = (Keys)kc.ConvertFromString(getTrafficStopFollowKey());
                trafficStopFollowModifierKey = (Keys)kc.ConvertFromString(getTrafficStopFollowModifierKey());
                trafficStopMimicKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "TrafficStopMimicKey"));
                trafficStopMimicModifierKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "TrafficStopMimicModifierKey"));
                IniDefaults.PositionUpKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "PositionUpKey", "NumPad9"));
                IniDefaults.PositionRightKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "PositionRightKey", "NumPad6"));
                IniDefaults.PositionResetKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "PositionResetKey", "NumPad5"));
                IniDefaults.PositionLeftKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "PositionLeftKey", "NumPad4"));
                IniDefaults.PositionForwardKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "PositionForwardKey", "NumPad8"));
                IniDefaults.PositionDownKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "PositionDownKey", "NumPad3"));
                IniDefaults.PositionBackwardKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "PositionBackwardKey", "NumPad2"));
                TrafficStopAssist.VehicleDoorLockDistance = initialiseFile().ReadSingle("Features", "VehicleDoorLockDistance", 5.2f);
                TrafficStopAssist.VehicleDoorUnlockDistance = initialiseFile().ReadSingle("Features", "VehicleDoorUnlockDistance", 3.5f);
                AutoVehicleDoorLock = initialiseFile().ReadBoolean("Features", "AutoVehicleDoorLock", true);
                CustomPulloverLocationKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "CustomPulloverLocationKey", "W"));
                CustomPulloverLocationModifierKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Keybindings", "CustomPulloverLocationModifierKey", "LControlKey"));
            }
            catch (Exception e)
            {
                trafficStopFollowKey = Keys.T;
                trafficStopFollowModifierKey = Keys.LControlKey;
                trafficStopMimicKey = Keys.R;
                trafficStopMimicModifierKey = Keys.LControlKey;
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("Loading default Custom Pullover INI file - Error detected in user's INI file.");
                Game.DisplayNotification("~r~~h~Error~s~ reading Custom Pullover ini file. Default values set; replace with default INI file!");
            }

        }
        public static Keys CustomPulloverLocationKey = Keys.W;
        public static Keys CustomPulloverLocationModifierKey = Keys.LControlKey;
        public static Keys trafficStopMimicModifierKey { get; set; }
        public static KeysConverter kc = new KeysConverter();
        public static Keys trafficStopFollowModifierKey { get; set; }
        public static Keys trafficStopFollowKey { get; set; }
        // private static Keys parkModifierKey { get; set; }
        public static bool isSomeoneRunningTheLight { get; set; }
        public static bool isSomeoneFollowing { get; set; }
        public static Keys trafficStopMimicKey { get; set; }

        public static bool AutoVehicleDoorLock = true;

        internal static void mainLoop()
        {
            Game.LogTrivial("Custom Pullover.Mainloop started");
            
            Game.LogTrivial("Loading Custom Pullover settings...");
            loadValuesFromIniFile();

            isSomeoneFollowing = false;
            isSomeoneRunningTheLight = false;

            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Yield();
                    if (Functions.IsPlayerPerformingPullover())
                    {

                        if (ExtensionMethods.IsKeyDownRightNowComputerCheck(trafficStopFollowModifierKey) || (trafficStopFollowModifierKey == Keys.None))
                        {
                            if (ExtensionMethods.IsKeyDownComputerCheck(trafficStopFollowKey))
                            {
                                if (!isSomeoneFollowing)
                                {
                                    TrafficStopAssist.followMe();
                                }
                                else { isSomeoneFollowing = false; }
                            }
                        }
                        if (ExtensionMethods.IsKeyDownRightNowComputerCheck(trafficStopMimicModifierKey) || (trafficStopMimicModifierKey == Keys.None))
                        {
                            if (ExtensionMethods.IsKeyDownComputerCheck(trafficStopMimicKey))
                            {
                                if (!isSomeoneFollowing)
                                {
                                    TrafficStopAssist.mimicMe();
                                }
                                else
                                {
                                    isSomeoneFollowing = false;
                                }
                            }
                        }
                        if (ExtensionMethods.IsKeyDownRightNowComputerCheck(CustomPulloverLocationModifierKey) || (CustomPulloverLocationModifierKey == Keys.None))
                        {
                            if (ExtensionMethods.IsKeyDownComputerCheck(CustomPulloverLocationKey))
                            {
                                if (!isSomeoneFollowing)
                                {
                                    TrafficStopAssist.SetCustomPulloverLocation();
                                }
                                else
                                {
                                    Game.LogTrivial("Already doing custom pullover location.");
                                }
                            }
                        }

                        if (!isSomeoneRunningTheLight)
                        {
                            TrafficStopAssist.checkForceRedLightRun();
                        }
                    }
                }
            });

            while (true)
            {
                GameFiber.Yield();
                TrafficStopAssist.checkForYieldDisable();
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

        public static InitializationFile initialiseFile()
        {
            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/Custom Pullover.ini");
            ini.Create();
           // Game.LogTrivial("Custom Pullover: Reading INI file.");
            return ini;
        
        }

        private static string getTrafficStopFollowKey()
        {
            InitializationFile ini = initialiseFile();
            string key = ini.ReadString("Keybindings", "TrafficStopFollowKey", "T");
            return key;
        }
        private static string getTrafficStopFollowModifierKey()
        {
            InitializationFile ini = initialiseFile();
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
                mainLoop();
                
            });
        }
    }
}
