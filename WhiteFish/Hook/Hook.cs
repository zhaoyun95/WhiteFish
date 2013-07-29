/*
 * This file is part of the Zeenox Project (C) 2013 Finn Grimpe
 * Copyright 2013 ZeenoxBots, All Rights Reserved
 * 
 * Website: http://zeenoxbots.com
 * License: http://zeenoxbots.com/about/license
 * 
 * $Author: $
 * $Revision: 29 $
 * $Date: 2013-03-31 05:38:05 +0200 (So, 31 Mrz 2013) $
 * $Id: Hook.cs 29 2013-03-31 03:38:05Z  $
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
            // Get address of EndScene
            IntPtr pEndScene = Memory.WoWD3D.HookPtr;

            if (Memory.WoW.IsProcessOpen)
            {
                // check if game is already hooked and dispose Hook
                if (Memory.WoW.Read<byte>(pEndScene) == 0xE9 &&
                    ((uint) _injectedCode == 0 || (uint) _addresseInjection == 0))
                {
                    DisposeHooking();
                }
                // skip check since bots sometimes won't clean up after themselves
                //if (_memory.ReadByte(pEndScene) != 0xE9) // check if game is already hooked
                //{
                Installed = false;
                // allocate memory to store injected code:
                _injectedCode = Memory.WoW.AllocateMemory(2048);
                // allocate memory the new injection code pointer:
                _addresseInjection = Memory.WoW.AllocateMemory(0x4);
                Memory.WoW.Write<int>(_addresseInjection, 0);
                // allocate memory the pointer return value:
                _retnInjectionAsm = Memory.WoW.AllocateMemory(0x4);
                Memory.WoW.Write<int>(_retnInjectionAsm, 0);

                // Generate the STUB to be injected
                Memory.WoW.Asm.Clear();

                // save regs
                Memory.WoW.Asm.AddLine("pushad");
                Memory.WoW.Asm.AddLine("pushfd");

                // Test if you need launch injected code:
                Memory.WoW.Asm.AddLine("mov eax, [" + _addresseInjection + "]");
                Memory.WoW.Asm.AddLine("test eax, eax");
                Memory.WoW.Asm.AddLine("je @out");

                // Launch Fonction:
                Memory.WoW.Asm.AddLine("mov eax, [" + _addresseInjection + "]");
                Memory.WoW.Asm.AddLine("call eax");

                // Copy pointer return value:
                Memory.WoW.Asm.AddLine("mov [" + _retnInjectionAsm + "], eax");

                // Enter value 0 of addresse func inject
                Memory.WoW.Asm.AddLine("mov edx, " + _addresseInjection);
                Memory.WoW.Asm.AddLine("mov ecx, 0");
                Memory.WoW.Asm.AddLine("mov [edx], ecx");

                // Close func
                Memory.WoW.Asm.AddLine("@out:");

                // load reg
                Memory.WoW.Asm.AddLine("popfd");
                Memory.WoW.Asm.AddLine("popad");


                // injected code
                var sizeAsm = (uint)(Memory.WoW.Asm.Assemble().Length);
                Memory.WoW.Asm.Inject((uint) _injectedCode);

                // Size asm jumpback
                const int sizeJumpBack = 5;

                // copy and save original instructions
                Memory.WoW.Asm.Clear();
                Memory.WoW.Asm.AddLine("mov edi, edi");
                Memory.WoW.Asm.AddLine("push ebp");
                Memory.WoW.Asm.AddLine("mov ebp, esp");
                Memory.WoW.Asm.Inject((uint) _injectedCode + sizeAsm);

                // create jump back stub
                Memory.WoW.Asm.Clear();
                Memory.WoW.Asm.AddLine("jmp " + (pEndScene + sizeJumpBack));
                Memory.WoW.Asm.Inject((uint) _injectedCode + sizeAsm + sizeJumpBack);

                // create hook jump
                Memory.WoW.Asm.Clear(); // $jmpto
                Memory.WoW.Asm.AddLine("jmp " + (_injectedCode));
                Memory.WoW.Asm.Inject((uint) pEndScene);
                //}
                Installed = true;
            }
        }

        internal static void DisposeHooking()
        {
            try
            {
                // Get address of EndScene
                IntPtr pEndScene = Memory.WoWD3D.HookPtr;

                if (Memory.WoW.Read<byte>(pEndScene) == 0xE9) // check if wow is already hooked and dispose Hook
                {
                    // Restore origine endscene:
                    Memory.WoW.Asm.Clear();
                    Memory.WoW.Asm.AddLine("mov edi, edi");
                    Memory.WoW.Asm.AddLine("push ebp");
                    Memory.WoW.Asm.AddLine("mov ebp, esp");
                    Memory.WoW.Asm.Inject((uint) pEndScene);
                }

                // free memory:
                Memory.WoW.FreeMemory(_injectedCode);
                Memory.WoW.FreeMemory(_addresseInjection);
                Memory.WoW.FreeMemory(_retnInjectionAsm);
                Installed = false;
            }
            catch
            {
            }
        }

        public static byte[] InjectAndExecute(IEnumerable<string> asm, int returnLength = 0)
        {
            lock (_executeLockObject)
            {
                var tempsByte = new byte[0];
                // reset return value pointer
                Memory.WoW.Write<int>(_retnInjectionAsm, 0);

                if (Memory.WoW.IsProcessOpen && Installed)
                {
                    // Write the asm stuff
                    Memory.WoW.Asm.Clear();
                    foreach (string tempLineAsm in asm)
                    {
                        Memory.WoW.Asm.AddLine(tempLineAsm);
                    }

                    // Allocation Memory
                    IntPtr injectionAsmCodecave = Memory.WoW.AllocateMemory(Memory.WoW.Asm.Assemble().Length);

                    try
                    {
                        // Inject
                        Memory.WoW.Asm.Inject((uint) injectionAsmCodecave);
                        Memory.WoW.Write<int>(_addresseInjection, (int)injectionAsmCodecave);
                        while (Memory.WoW.Read<int>(_addresseInjection) > 0)
                        {
                            Thread.Sleep(5);
                        } // Wait to launch code


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

                    // Free memory allocated 
                    Memory.WoW.FreeMemory(injectionAsmCodecave);
                }
                // return
                return tempsByte;
            }
        }
    }
}