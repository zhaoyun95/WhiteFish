/*
 * This file is part of the WhiteRain project (C) 2013 Finn Grimpe
 * Copyright 2013 Finn Grimpe, All Rights Reserved
 * 
 * Github:  https://github.com/finndev/WhiteFish/
 * Website: https://finn.lu/whitefish/
 * License: https://finn.lu/license/
 *
 */

using System;
using System.Text;

namespace WhiteFish.WoW
{
    class Lua
    {
        internal static string GetLocalizedText(string localVar)
        {
            if (localVar.Length == 0) throw new Exception("Invalid localized variable");
            if (!Hook.Installed)
            {
                Hook.InstallHook();
                if (!Hook.Installed) //Still not installed?
                {
                    Debug.Log("Error: Failed to hook DirectX.");
                    throw new Exception("Failed to hook DirectX");
                }
            }

            IntPtr Lua_GetLocalizedText_Space = Memory.WoW.AllocateMemory(Encoding.UTF8.GetBytes(localVar).Length + 1);

            IntPtr ClntObjMgrGetActivePlayerObj = Memory.WoW.ImageBase + 0x2D17;
            IntPtr FrameScript__GetLocalizedText = Memory.WoW.ImageBase + 0x3DD663;

            Memory.WoW.WriteBytes(Lua_GetLocalizedText_Space, Encoding.UTF8.GetBytes(localVar));

            String[] asm = new String[] 
                {
                    "call " + (uint) ClntObjMgrGetActivePlayerObj,
                    "mov ecx, eax",
                    "push -1",
                    "mov edx, " + Lua_GetLocalizedText_Space + "",
                    "push edx",
                    "call " + (uint) FrameScript__GetLocalizedText,
                    "retn",
                };

            string sResult = Encoding.UTF8.GetString(WoW.Hook.InjectAndExecute(asm));

            Memory.WoW.FreeMemory(Lua_GetLocalizedText_Space);
            return sResult;
        }

        internal static void DoString(string Lua)
        {
            if (Lua.Length == 0) return;
            if (!Hook.Installed)
            {
                Hook.InstallHook();
                if (!Hook.Installed) //Still not installed?
                {
                    Debug.Log("Error: Failed to hook DirectX.");
                    return;
                }
            }

            IntPtr DoStringArg_Codecave = Memory.WoW.AllocateMemory(Encoding.UTF8.GetBytes(Lua).Length + 1);

            uint FrameScript__Execute = 0x54EE6;
            Memory.WoW.WriteBytes(DoStringArg_Codecave, Encoding.UTF8.GetBytes(Lua));

            String[] ASM = new String[] 
                {
                    "mov eax, " + DoStringArg_Codecave,
                    "push 0",
                    "push eax",
                
                    "push eax",
                    "mov eax, " + ((uint)Memory.WoW.ImageBase + FrameScript__Execute),
                
                    "call eax",
                    "add esp, 0xC",
                    "retn",    
                };

            WoW.Hook.InjectAndExecute(ASM);
            Memory.WoW.FreeMemory(DoStringArg_Codecave);
        }
    }
}
