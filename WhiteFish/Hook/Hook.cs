/*
 * This file is part of the WhiteRain project (C) 2013 Finn Grimpe
 * Copyright 2013 Finn Grimpe, All Rights Reserved
 * 
 * Github:  https://github.com/finndev/WhiteFish/
 * Website: https://finn.lu/whitefish/
 * License: https://finn.lu/license/
 *
 */

using GreyMagic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace WhiteFish.WoW
{
    public class Hook
    {
        internal static readonly object _executeLockObject = new object();
        internal static IntPtr _addresseInjection;
        internal static IntPtr _injectedCode;
        internal static IntPtr _retnInjectionAsm;

        internal static bool Installed { get; private set; }

        internal static void InstallHook()
        {
            //DirectX EndScene Address
            IntPtr pEndScene = Memory.WoWD3D.HookPtr;

            if (Memory.WoW.IsProcessOpen)
            {
                //Check if the game is already hooked, if so, clean it.
                if (Memory.WoW.Read<byte>(pEndScene) == 0xE9 && ((uint) _injectedCode == 0 || (uint) _addresseInjection == 0))
                {
                    DisposeHooking();
                }


                Installed = false;
                _injectedCode = Memory.WoW.AllocateMemory(2048);
                _addresseInjection = Memory.WoW.AllocateMemory(0x4);
                Memory.WoW.Write<int>(_addresseInjection, 0);
                _retnInjectionAsm = Memory.WoW.AllocateMemory(0x4);
                Memory.WoW.Write<int>(_retnInjectionAsm, 0);

                Memory.WoW.Asm.Clear();

                //Save registers
                Memory.WoW.Asm.AddLine("pushad");
                Memory.WoW.Asm.AddLine("pushfd");

                //Test if we need to launch the code
                Memory.WoW.Asm.AddLine("mov eax, [" + _addresseInjection + "]");
                Memory.WoW.Asm.AddLine("test eax, eax");
                Memory.WoW.Asm.AddLine("je @out");

                //Launch the function
                Memory.WoW.Asm.AddLine("mov eax, [" + _addresseInjection + "]");
                Memory.WoW.Asm.AddLine("call eax");

                //Copy the pointer return value
                Memory.WoW.Asm.AddLine("mov [" + _retnInjectionAsm + "], eax");

                //Mov value 0 to addressPointer
                Memory.WoW.Asm.AddLine("mov edx, " + _addresseInjection);
                Memory.WoW.Asm.AddLine("mov ecx, 0");
                Memory.WoW.Asm.AddLine("mov [edx], ecx");

                //Close the function
                Memory.WoW.Asm.AddLine("@out:");

                //Load registers
                Memory.WoW.Asm.AddLine("popfd");
                Memory.WoW.Asm.AddLine("popad");


                //The code to be injected
                var sizeAsm = (uint)(Memory.WoW.Asm.Assemble().Length);
                Memory.WoW.Asm.Inject((uint) _injectedCode);

                //ASM Size jumpback. Prolly 4 on W8.
                const int sizeJumpBack = 5;

                //Copy and save original instructions
                Memory.WoW.Asm.Clear();
                Memory.WoW.Asm.AddLine("mov edi, edi");
                Memory.WoW.Asm.AddLine("push ebp");
                Memory.WoW.Asm.AddLine("mov ebp, esp");
                Memory.WoW.Asm.Inject((uint) _injectedCode + sizeAsm);

                //Create the jump back stub
                Memory.WoW.Asm.Clear();
                Memory.WoW.Asm.AddLine("jmp " + (pEndScene + sizeJumpBack));
                Memory.WoW.Asm.Inject((uint) _injectedCode + sizeAsm + sizeJumpBack);

                //Create hook jump
                Memory.WoW.Asm.Clear(); // $jmpto
                Memory.WoW.Asm.AddLine("jmp " + (_injectedCode));
                Memory.WoW.Asm.Inject((uint) pEndScene);
                Installed = true;
            }
        }

        internal static void DisposeHooking()
        {
            //DirectX EndScene Address
            IntPtr pEndScene = Memory.WoWD3D.HookPtr;

            if (Memory.WoW.Read<byte>(pEndScene) == 0xE9) // *cough* ^_^
            {
                // Restore origine endscene:
                Memory.WoW.Asm.Clear();
                Memory.WoW.Asm.AddLine("mov edi, edi");
                Memory.WoW.Asm.AddLine("push ebp");
                Memory.WoW.Asm.AddLine("mov ebp, esp");
                Memory.WoW.Asm.Inject((uint) pEndScene);
            }

            Memory.WoW.FreeMemory(_injectedCode);
            Memory.WoW.FreeMemory(_addresseInjection);
            Memory.WoW.FreeMemory(_retnInjectionAsm);
            Installed = false;
        }

        public static byte[] InjectAndExecute(IEnumerable<string> asm, int returnLength = 0)
        {
            lock (_executeLockObject)
            {
                var tempsByte = new byte[0];
                
                //Reset return value pointer from last injection
                Memory.WoW.Write<int>(_retnInjectionAsm, 0);

                if (Memory.WoW.IsProcessOpen && Installed)
                {
                    //Write the asm lines
                    Memory.WoW.Asm.Clear();
                    foreach (string tempLineAsm in asm)
                    {
                        Memory.WoW.Asm.AddLine(tempLineAsm);
                    }

                    //Allocate memory
                    IntPtr injectionAsmCodecave = Memory.WoW.AllocateMemory(Memory.WoW.Asm.Assemble().Length);

                    try
                    {
                        //Inject
                        Memory.WoW.Asm.Inject((uint) injectionAsmCodecave);
                        Memory.WoW.Write<int>(_addresseInjection, (int)injectionAsmCodecave);
                        while (Memory.WoW.Read<int>(_addresseInjection) > 0)
                        {
                            Thread.Sleep(5);
                        } //Wait till the injection pointer is "available"


                        if (returnLength > 0)
                        {
                            tempsByte = Memory.WoW.ReadBytes((IntPtr) Memory.WoW.Read<uint>(_retnInjectionAsm), returnLength);
                        }
                        else
                        {
                            var retnByte = new List<byte>();
                            IntPtr dwAddress = Memory.WoW.Read<IntPtr>(_retnInjectionAsm);
                            byte buf = Memory.WoW.Read<byte>(dwAddress);
                            while (buf != 0)
                            {
                                retnByte.Add(buf);
                                dwAddress = dwAddress + 1;
                                buf = Memory.WoW.Read<byte>(dwAddress);
                            }
                            tempsByte = retnByte.ToArray();
                        }
                    }
                    catch
                    {
                    }

                    Memory.WoW.FreeMemory(injectionAsmCodecave);
                }
                return tempsByte;
            }
        }
    }
}