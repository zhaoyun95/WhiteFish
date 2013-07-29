/*
 * This file is part of the Zeenox Project (C) 2013 Finn Grimpe
 * Copyright 2013 ZeenoxBots, All Rights Reserved
 * 
 * Website: http://zeenoxbots.com
 * License: http://zeenoxbots.com/about/license
 * 
 * $Author: $
 * $Revision: 28 $
 * $Date: 2013-03-31 04:43:30 +0200 (So, 31 Mrz 2013) $
 * $Id: Dirext3D.cs 28 2013-03-31 02:43:30Z  $
 * 
 */

using System;
using System.Diagnostics;
using System.Linq;

namespace WhiteFish.WoW.DirectX
{
    internal class Dirext3D
    {
        public Dirext3D(Process targetProc)
        {
            TargetProcess = targetProc;

            UsingDirectX11 = TargetProcess.Modules.Cast<ProcessModule>().Any(m => m.ModuleName == "d3d11.dll");

            using (D3DDevice d3D = UsingDirectX11
                                       ? (D3DDevice)new D3D11Device(targetProc)
                                       : new D3D9Device(targetProc))
            {
                HookPtr = UsingDirectX11 ? ((D3D11Device)d3D).GetSwapVTableFuncAbsoluteAddress(d3D.PresentVtableIndex) : d3D.GetDeviceVTableFuncAbsoluteAddress(d3D.EndSceneVtableIndex);
            }
        }

        public Process TargetProcess { get; private set; }
        public bool UsingDirectX11 { get; private set; }
        public IntPtr HookPtr { get; private set; }
    }
}