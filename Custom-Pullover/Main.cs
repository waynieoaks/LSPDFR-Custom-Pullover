using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rage;
using Rage.Native;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Reflection;

namespace Custom_Pullover
{

    using LSPD_First_Response.Mod.API;
    using Rage;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;

    internal class Main : Plugin
    {
        public Main()
        {
            Game.LogTrivial("Creating Custom Pullover.Main.");
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
            Game.LogTrivial("Done with Custom Pullover.Main.");
        }

        public override void Finally()
        {
            foreach (Vehicle veh in TrafficStopAssist.PlayerVehicles.ToArray())
            {
                if (veh.Exists())
                {
                    veh.LockStatus = VehicleLockStatus.Unlocked;
                }
            }
        }

        public override void Initialize()
        {
            //Event handler for detecting if the player goes on duty

            Game.LogTrivial("Custom Pullover " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " has been initialised.");

        }
        
        internal static string Path = "Plugins/LSPDFR/Custom Pullover.dll";


        static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            Game.LogTrivial("Custom Pullover event handler: " + onDuty.ToString());
            if (onDuty)
            {
                    CustomPulloverHandler.Initialise();
            }
        }
    }

    // Wayne added to remove dependency on Albo1125.common

    public static class IniDefaults
    {
        public static Keys PositionResetKey = Keys.NumPad5;
        public static Keys PositionForwardKey = Keys.NumPad8;
        public static Keys PositionBackwardKey = Keys.NumPad2;
        public static Keys PositionRightKey = Keys.NumPad6;
        public static Keys PositionLeftKey = Keys.NumPad4;
        public static Keys PositionUpKey = Keys.NumPad9;
        public static Keys PositionDownKey = Keys.NumPad3;
    }

        public static class ExtensionMethods
    {

        public static string GetKeyString(Keys MainKey, Keys ModifierKey)
        {
            if (ModifierKey == Keys.None)
            {
                return MainKey.ToString();
            }
            else
            {
                string strmodKey = ModifierKey.ToString();

                if (strmodKey.EndsWith("ControlKey") | strmodKey.EndsWith("ShiftKey"))
                {
                    strmodKey.Replace("Key", "");
                }

                if (strmodKey.Contains("ControlKey"))
                {
                    strmodKey = "CTRL";
                }
                else if (strmodKey.Contains("ShiftKey"))
                {
                    strmodKey = "Shift";
                }
                else if (strmodKey.Contains("Menu"))
                {
                    strmodKey = "ALT";
                }

                return string.Format("{0} + {1}", strmodKey, MainKey.ToString());
            }
        }

        public static bool IsPointOnWater(this Vector3 position)
        {
            float height;
            return NativeFunction.Natives.GET_WATER_HEIGHT<bool>(position.X, position.Y, position.Z, out height);
        }

        public static bool IsKeyDownComputerCheck(Keys KeyPressed)
        {
            if (Rage.Native.NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() != 0)
            {
                return Game.IsKeyDown(KeyPressed);
            }
            else
            {
                return false;
            }
        }

            public static bool IsKeyDownRightNowComputerCheck(Keys KeyPressed)
        {

            if (Rage.Native.NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() != 0)
            {
                return Game.IsKeyDownRightNow(KeyPressed);
            }
            else
            {
                return false;
            }
        }

        public static bool IsKeyCombinationDownComputerCheck(Keys MainKey, Keys ModifierKey)
        {
            if (MainKey != Keys.None)
            {
                return ExtensionMethods.IsKeyDownComputerCheck(MainKey) && (ExtensionMethods.IsKeyDownRightNowComputerCheck(ModifierKey)
                || (ModifierKey == Keys.None && !ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.Shift) && !ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.Control)
                && !ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.LControlKey) && !ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.LShiftKey)));
            }
            else
            {
                return false;
            }
        }
        public static bool IsKeyCombinationDownRightNowComputerCheck(Keys MainKey, Keys ModifierKey)
        {
            if (MainKey != Keys.None)
            {
                return ExtensionMethods.IsKeyDownRightNowComputerCheck(MainKey) && ((ExtensionMethods.IsKeyDownRightNowComputerCheck(ModifierKey)
                    || (ModifierKey == Keys.None && !ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.Shift) && !ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.Control)
                    && !ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.LControlKey) && !ExtensionMethods.IsKeyDownRightNowComputerCheck(Keys.LShiftKey))));
            }
            else
            {
                return false;
            }
        }
    }
}
