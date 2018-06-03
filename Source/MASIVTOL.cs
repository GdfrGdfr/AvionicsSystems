﻿/*****************************************************************************
 * The MIT License (MIT)
 * 
 * Copyright (c) 2018 MOARdV
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 * 
 ****************************************************************************/
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AvionicsSystems
{
    /// <summary>
    /// MASIVTOL is the MAS interface to the VTOL Manager found in
    /// the WildBlueTools mod.  It is a MoonSharp proxy class.
    /// </summary>
    /// <LuaName>vtol</LuaName>
    /// <mdDoc>
    /// MASIVTOL is the AvionicsSystems interface with the Wild Blue Industries VTOL manager.
    /// </mdDoc>
    internal class MASIVTOL
    {
        internal static readonly bool wbivtolInstalled;

        internal static readonly Type wbiVtolManager_t;

        // Initialization
        private static readonly Func<object, Vessel, object> wbiFindControllers;

        // Hover mode control
        private static readonly Func<object, bool> wbiGetHoverActive;
        private static readonly Func<object, object> wbiToggleHover;
        private static readonly Func<object, float> wbiGetVerticalSpeed;
        private static readonly Func<object, float, object> wbiDecreaseVerticalSpeed;
        private static readonly Func<object, float, object> wbiIncreaseVerticalSpeed;
        private static readonly Func<object, object> wbiKillVerticalSpeed;

        // Thrust mode
        private static readonly FieldInfo ThrustMode_t;
        //private static readonly Func<object, object> wbiGetThrustMode;
        private static readonly Func<object, object> wbiSetForwardThrust;
        private static readonly Func<object, object> wbiSetReverseThrust;
        private static readonly Func<object, object> wbiSetVTOLThrust;

        private object vtolManager;
        private Vessel vessel;

        [MoonSharpHidden]
        internal void UpdateVessel(Vessel vessel)
        {
            if (vessel != this.vessel)
            {
                this.vessel = vessel;

                if (vtolManager != null)
                {
                    wbiFindControllers(vtolManager, this.vessel);
                }
            }
        }

        private object GetVtolManager()
        {
            if (vtolManager == null && wbivtolInstalled)
            {
                vtolManager = UnityEngine.Object.FindObjectOfType(wbiVtolManager_t);
                if (vtolManager != null)
                {
                    wbiFindControllers(vtolManager, vessel);
                }
            }

            return vtolManager;
        }

        /// <summary>
        /// Indicates whether the Wild Blue Industries VTOL manager mod is available.
        /// </summary>
        /// <returns></returns>
        public double Available()
        {
            return (wbivtolInstalled) ? 1.0 : 0.0;
        }

        /// <summary>
        /// Increase (positive amount) or decrease (negative amount) the
        /// commanded vertical speed in m/s.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public double ChangeVerticalSpeed(double amount)
        {
            object manager = GetVtolManager();
            if (manager != null)
            {
                if (amount > 0.0)
                {
                    wbiIncreaseVerticalSpeed(manager, (float)amount);
                }
                else if (amount < 0.0)
                {
                    wbiDecreaseVerticalSpeed(manager, -(float)amount);
                }

                return 1.0;
            }
            return 0.0;
        }

        /// <summary>
        /// Returns 1 if the VTOL manager reports that this vessel is in hover mode.  Returns 0 otherwise.
        /// </summary>
        /// <returns></returns>
        public double GetHover()
        {
            object manager = GetVtolManager();
            if (manager != null)
            {
                return wbiGetHoverActive(manager) ? 1.0 : 0.0;
            }
            return 0.0;
        }

        /// <summary>
        /// Returns the current thrust mode for the VTOL engines.  If none are
        /// available, returns 1 (forward).
        /// 
        /// Valid return values:
        /// * -1: Reverse Thrust
        /// * 0: VTOL Thrust
        /// * 1: Forward Thrust
        /// </summary>
        /// <returns></returns>
        public double GetThrustMode()
        {
            object manager = GetVtolManager();
            if (manager != null)
            {
                object mode = ThrustMode_t.GetValue(manager);
                //object mode = wbiGetThrustMode(manager); // Direct call doesn't work...

                switch ((int)mode)
                {
                    case 0:
                        return 1.0;
                    case 1:
                        return -1.0;
                    case 2:
                        return 0.0;
                }
            }
            return 1.0;
        }

        /// <summary>
        /// Returns the commanded vertical speed in m/s.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public double GetVerticalSpeed()
        {
            object manager = GetVtolManager();
            if (manager != null)
            {
                return wbiGetVerticalSpeed(manager);
            }
            return 0.0;
        }

        /// <summary>
        /// Set the commanded vertical speed to zero.
        /// </summary>
        /// <returns></returns>
        public double KillVerticalSpeed()
        {
            object manager = GetVtolManager();
            if (manager != null)
            {
                wbiKillVerticalSpeed(manager);
                return 1.0;
            }
            return 0.0;
        }

        /// <summary>
        /// Set the VTOL manager thrust mode.  Valid settings are:
        /// * -1: Reverse Thrust
        /// * 0: VTOL Thrust
        /// * 1: Forward Thrust
        /// </summary>
        /// <param name="mode"></param>
        /// <returns>1 if the mode was changed, 0 if it was not</returns>
        public double SetThrustMode(double mode)
        {
            object manager = GetVtolManager();
            if (manager != null)
            {
                object currentModeO = ThrustMode_t.GetValue(manager);
                int currentMode = (int)currentModeO;
                int newMode = (int)mode;

                // currentMode is WBIThrustModes, which maps 0 = Forward, 1 = Reverse, 2 = VTOL
                // MAS thrust mode is 1 = Forward, -1 = Reverse, 0 = VTOL
                if (newMode == 1 && currentMode != 0)
                {
                    wbiSetForwardThrust(manager);
                    return 1.0;
                }
                else if (newMode == 0 && currentMode != 2)
                {
                    wbiSetVTOLThrust(manager);
                    return 1.0;
                }
                else if (newMode == -1 && currentMode != 1)
                {
                    wbiSetReverseThrust(manager);
                    return 1.0;
                }
            }
            return 0.0;
        }

        /// <summary>
        /// Toggles VTOL hover mode.
        /// </summary>
        /// <returns></returns>
        public double ToggleHover()
        {
            object manager = GetVtolManager();
            if (manager != null)
            {
                wbiToggleHover(manager);
                return 1.0;
            }
            return 0.0;
        }

        [MoonSharpHidden]
        static MASIVTOL()
        {
            wbiVtolManager_t = Utility.GetExportedType("KerbalActuators", "KerbalActuators.WBIVTOLManager");

            if (wbiVtolManager_t != null)
            {
                // Init
                MethodInfo FindControllers_t = wbiVtolManager_t.GetMethod("FindControllers", BindingFlags.Instance | BindingFlags.Public);
                if (FindControllers_t == null)
                {
                    Utility.LogStaticError("Didn't find FindControllers");
                    return;
                }
                wbiFindControllers = DynamicMethodFactory.CreateDynFunc<object, Vessel, object>(FindControllers_t);

                // Hover
                FieldInfo HoverActive_t = wbiVtolManager_t.GetField("hoverActive", BindingFlags.Instance | BindingFlags.Public);
                if (HoverActive_t == null)
                {
                    Utility.LogStaticError("Didn't find hoverActive");
                    return;
                }
                wbiGetHoverActive = DynamicMethodFactory.CreateGetField<object, bool>(HoverActive_t);

                MethodInfo ToggleHover_t = wbiVtolManager_t.GetMethod("ToggleHover", BindingFlags.Instance | BindingFlags.Public);
                if (ToggleHover_t == null)
                {
                    Utility.LogStaticError("Didn't find ToggleHover");
                    return;
                }
                wbiToggleHover = DynamicMethodFactory.CreateFunc<object, object>(ToggleHover_t);

                // VSpd
                MethodInfo KillVerticalSpeed_t = wbiVtolManager_t.GetMethod("KillVerticalSpeed", BindingFlags.Instance | BindingFlags.Public);
                if (KillVerticalSpeed_t == null)
                {
                    Utility.LogStaticError("Didn't find KillVerticalSpeed");
                    return;
                }
                wbiKillVerticalSpeed = DynamicMethodFactory.CreateFunc<object, object>(KillVerticalSpeed_t);

                MethodInfo IncreaseVerticalSpeed_t = wbiVtolManager_t.GetMethod("IncreaseVerticalSpeed", BindingFlags.Instance | BindingFlags.Public);
                if (IncreaseVerticalSpeed_t == null)
                {
                    Utility.LogStaticError("Didn't find IncreaseVerticalSpeed");
                    return;
                }
                wbiIncreaseVerticalSpeed = DynamicMethodFactory.CreateDynFunc<object, float, object>(IncreaseVerticalSpeed_t);

                MethodInfo DecreaseVerticalSpeed_t = wbiVtolManager_t.GetMethod("DecreaseVerticalSpeed", BindingFlags.Instance | BindingFlags.Public);
                if (DecreaseVerticalSpeed_t == null)
                {
                    Utility.LogStaticError("Didn't find DecreaseVerticalSpeed");
                    return;
                }
                wbiDecreaseVerticalSpeed = DynamicMethodFactory.CreateDynFunc<object, float, object>(DecreaseVerticalSpeed_t);

                // Thrust mode
                ThrustMode_t = wbiVtolManager_t.GetField("thrustMode", BindingFlags.Instance | BindingFlags.Public);
                if (ThrustMode_t == null)
                {
                    Utility.LogStaticError("Didn't find thrustMode");
                    return;
                }
                //wbiGetThrustMode = DynamicMethodFactory.CreateGetField<object, object>(ThrustMode_t);

                MethodInfo SetForwardThrust_t = wbiVtolManager_t.GetMethod("SetForwardThrust", BindingFlags.Instance | BindingFlags.Public);
                if (SetForwardThrust_t == null)
                {
                    Utility.LogStaticError("Didn't find SetForwardThrust");
                    return;
                }
                wbiSetForwardThrust = DynamicMethodFactory.CreateFunc<object, object>(SetForwardThrust_t);

                MethodInfo SetReverseThrust_t = wbiVtolManager_t.GetMethod("SetReverseThrust", BindingFlags.Instance | BindingFlags.Public);
                if (SetReverseThrust_t == null)
                {
                    Utility.LogStaticError("Didn't find SetReverseThrust");
                    return;
                }
                wbiSetReverseThrust = DynamicMethodFactory.CreateFunc<object, object>(SetReverseThrust_t);

                MethodInfo SetVTOLThrust_t = wbiVtolManager_t.GetMethod("SetVTOLThrust", BindingFlags.Instance | BindingFlags.Public);
                if (SetVTOLThrust_t == null)
                {
                    Utility.LogStaticError("Didn't find SetVTOLThrust");
                    return;
                }
                wbiSetVTOLThrust = DynamicMethodFactory.CreateFunc<object, object>(SetVTOLThrust_t);

                FieldInfo VerticalSpeed_t = wbiVtolManager_t.GetField("verticalSpeed", BindingFlags.Instance | BindingFlags.Public);
                if (VerticalSpeed_t == null)
                {
                    Utility.LogStaticError("Didn't find verticalSpeed");
                    return;
                }
                wbiGetVerticalSpeed = DynamicMethodFactory.CreateGetField<object, float>(VerticalSpeed_t);

                wbivtolInstalled = true;
            }
        }
    }
}