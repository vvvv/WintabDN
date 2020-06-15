///////////////////////////////////////////////////////////////////////////////
// CWintabInfo.cs - Wintab information access for WintabDN
//
// Copyright (c) 2010, Wacom Technology Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Windows.Forms;

namespace WintabDN
{
    /// <summary>
    /// Class to access Wintab interface data.
    /// </summary>
    public class CWintabInfo
    {
        private const int MAX_STRING_SIZE = 256;

        /// <summary>
        /// Returns TRUE if Wintab service is running and responsive.
        /// </summary>
        /// <returns></returns>
        public static bool IsWintabAvailable()
        {
            IntPtr buf = IntPtr.Zero;
            bool status = false;

            status = (CWintabFuncs.WTInfo(0, 0, buf) > 0);

            return status;
        }

        /// <summary>
        /// Returns a string containing device name.
        /// </summary>
        /// <returns></returns>
        public static String GetDeviceInfo()
        {
            string devInfo = null;
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(MAX_STRING_SIZE);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)EWTICategoryIndex.WTI_DEVICES, 
                    (uint)EWTIDevicesIndex.DVC_NAME, buf);

                if (size < 1)
                {
                    throw new Exception("GetDeviceInfo returned empty string.");
                }

                // Strip off final null character before marshalling.
                devInfo = CMemUtils.MarshalUnmanagedString(buf, size-2);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return devInfo;
        }

        /// <summary>
        /// Returns the default digitizing context, with useful context overrides. 
        /// </summary>
        /// <param name="options_I">caller's options; OR'd into context options</param>
        /// <returns>A valid context object or null on error.</returns>
        public static CWintabContext GetDefaultDigitizingContext(ECTXOptionValues options_I = 0)
        {
            // Send all possible data bits (not including extended data).
            // This is redundant with CWintabContext initialization, which
            // also inits with PK_PKTBITS_ALL.
            uint PACKETDATA = (uint)EWintabPacketBit.PK_PKTBITS_ALL;  // The Full Monty
            uint PACKETMODE = (uint)EWintabPacketBit.PK_BUTTONS;

            CWintabContext context = GetDefaultContext(EWTICategoryIndex.WTI_DEFCONTEXT);

            if ( context != null )
            {
                // Add digitizer-specific context tweaks.
                context.PktMode = 0;        // all data in absolute mode (set EWintabPacketBit bit(s) for relative mode)
                context.SysMode = false;    // system cursor tracks in absolute mode (zero)

                // Add caller's options.
                context.Options |= (uint)options_I;

                // Set the context data bits.
                context.PktData = PACKETDATA;
                context.PktMode = PACKETMODE;
                context.MoveMask = PACKETDATA;
                context.BtnUpMask = context.BtnDnMask;
            }

            return context;
        }

        /// <summary>
        /// Returns the default system context, with useful context overrides.
        /// </summary>
        /// <param name="options_I">caller's options; OR'd into context options</param>
        /// <returns>A valid context object or null on error.</returns>
        public static CWintabContext GetDefaultSystemContext(ECTXOptionValues options_I = 0)
        {
            // Send all possible data bits (not including extended data).
            // This is redundant with CWintabContext initialization, which
            // also inits with PK_PKTBITS_ALL.
            uint PACKETDATA = (uint)EWintabPacketBit.PK_PKTBITS_ALL;  // The Full Monty
            uint PACKETMODE = (uint)EWintabPacketBit.PK_BUTTONS;

            CWintabContext context = GetDefaultContext(EWTICategoryIndex.WTI_DEFSYSCTX);

            if (context != null)
            {
                // TODO: Add system-specific context tweaks.

                // Add caller's options.
                context.Options |= (uint)options_I;

                // Make sure we get data packet messages.
                context.Options |= (uint) ECTXOptionValues.CXO_MESSAGES;

                // Set the context data bits.
                context.PktData = PACKETDATA;
                context.PktMode = PACKETMODE;
                context.MoveMask = PACKETDATA;
                context.BtnUpMask = context.BtnDnMask;
            }

            return context;
        }

        /// <summary>
        /// Helper function to get digitizing or system default context.
        /// </summary>
        /// <param name="contextType_I">Use WTI_DEFCONTEXT for digital context or WTI_DEFSYSCTX for system context</param>
        /// <returns>Returns the default context or null on error.</returns>
        private static CWintabContext GetDefaultContext(EWTICategoryIndex contextIndex_I)        
        {
            CWintabContext context = new CWintabContext();
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(context.LogContext);

            try
            {
                int size = (int)CWintabFuncs.WTInfo((uint)contextIndex_I, 0, buf);

                context.LogContext = CMemUtils.MarshalUnmanagedBuf<WintabLogContext>(buf, size);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return context;
        }

        /// <summary>
        /// Returns the default device.  If this value is -1, then it also known as a "virtual device".
        /// </summary>
        /// <returns></returns>
        public static Int32 GetDefaultDeviceIndex()
        {
            Int32 devIndex = 0;
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(devIndex);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)EWTICategoryIndex.WTI_DEFCONTEXT, 
                    (uint)EWTIContextIndex.CTX_DEVICE, buf);

                devIndex = CMemUtils.MarshalUnmanagedBuf<Int32>(buf, size);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return devIndex;
        }

        /// <summary>
        /// Returns the WintabAxis object for specified device and dimension.
        /// </summary>
        /// <param name="devIndex_I">Device index (-1 = virtual device)</param>
        /// <param name="dim_I">Dimension: AXIS_X, AXIS_Y or AXIS_Z</param>
        /// <returns></returns>
        public static WintabAxis GetDeviceAxis(Int32 devIndex_I, EAxisDimension dim_I)
        {
            WintabAxis axis = new WintabAxis();
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(axis);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)(EWTICategoryIndex.WTI_DEVICES + devIndex_I), 
                    (uint)dim_I, buf);

                // If size == 0, then returns a zeroed struct.
                axis = CMemUtils.MarshalUnmanagedBuf<WintabAxis>(buf, size);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return axis;
        }

        /// <summary>
        /// Returns a 3-element array describing the tablet's orientation range and resolution capabilities.
        /// </summary>
        /// <returns></returns>
        public static WintabAxisArray GetDeviceOrientation( out bool tiltSupported_O )
        {
            WintabAxisArray axisArray = new WintabAxisArray();
            tiltSupported_O = false;
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(axisArray);          

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)EWTICategoryIndex.WTI_DEVICES, 
                    (uint)EWTIDevicesIndex.DVC_ORIENTATION, buf);

                 // If size == 0, then returns a zeroed struct.
                axisArray = CMemUtils.MarshalUnmanagedBuf<WintabAxisArray>(buf, size);
                tiltSupported_O = (axisArray.array[0].axResolution != 0 && axisArray.array[1].axResolution != 0);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return axisArray;
        }

        /// <summary>
        /// Returns a 3-element array describing the tablet's rotation range and resolution capabilities
        /// </summary>
        /// <returns></returns>
        public static WintabAxisArray GetDeviceRotation(out bool rotationSupported_O)
        {
            WintabAxisArray axisArray = new WintabAxisArray();
            rotationSupported_O = false;
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(axisArray);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)EWTICategoryIndex.WTI_DEVICES, 
                    (uint)EWTIDevicesIndex.DVC_ROTATION, buf);

                // If size == 0, then returns a zeroed struct.
                axisArray = CMemUtils.MarshalUnmanagedBuf<WintabAxisArray>(buf, size);
                rotationSupported_O = (axisArray.array[0].axResolution != 0 && axisArray.array[1].axResolution != 0);                
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return axisArray;
        }

        /// <summary>
        /// Returns the number of devices connected.
        /// </summary>
        /// <returns></returns>
        public static UInt32 GetNumberOfDevices()
        {
            UInt32 numDevices = 0;
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(numDevices);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)EWTICategoryIndex.WTI_INTERFACE, 
                    (uint)EWTIInterfaceIndex.IFC_NDEVICES, buf);

                numDevices = CMemUtils.MarshalUnmanagedBuf<UInt32>(buf, size);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return numDevices;
        }

        /// <summary>
        /// Returns whether a stylus is currently connected to the active cursor.
        /// </summary>
        /// <returns></returns>
        public static bool IsStylusActive()
        {
            bool isStylusActive = false;
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(isStylusActive);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)EWTICategoryIndex.WTI_INTERFACE,
                    (uint)EWTIInterfaceIndex.IFC_NDEVICES, buf);

                isStylusActive = CMemUtils.MarshalUnmanagedBuf<bool>(buf, size);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return isStylusActive;
        }

        /// <summary>
        /// Returns a string containing the name of the selected stylus. 
        /// </summary>
        /// <param name="index_I">indicates stylus type</param>
        /// <returns></returns>
        public static string GetStylusName(EWTICursorNameIndex index_I)
        {
            string stylusName = null;
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(MAX_STRING_SIZE);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)index_I,
                    (uint)EWTICursorsIndex.CSR_NAME, buf);

                if (size < 1)
                {
                    throw new Exception("GetStylusName returned empty string.");
                }

                // Strip off final null character before marshalling.
                stylusName = CMemUtils.MarshalUnmanagedString(buf, size-2);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return stylusName;
        }

        /// <summary>
        /// Return max normal pressure supported by tablet.
        /// </summary>
        /// <param name="getNormalPressure_I">TRUE=> normal pressure; 
        /// FALSE=> tangential pressure (not supported on all tablets)</param>
        /// <returns>maximum pressure value or zero on error</returns>
        public static Int32 GetMaxPressure(bool getNormalPressure_I = true)
        {
            WintabAxis pressureAxis = new WintabAxis();
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(pressureAxis);

            EWTIDevicesIndex devIdx = (getNormalPressure_I ?
                EWTIDevicesIndex.DVC_NPRESSURE : EWTIDevicesIndex.DVC_TPRESSURE);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)EWTICategoryIndex.WTI_DEVICES,
                    (uint)devIdx, buf);

                pressureAxis = CMemUtils.MarshalUnmanagedBuf<WintabAxis>(buf, size);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return pressureAxis.axMax;
        }

        /// <summary>
        /// Return the WintabAxis object for the specified dimension.
        /// </summary>
        /// <param name="dimension_I">Dimension to fetch (eg: x, y)</param>
        /// <returns></returns>
        public static WintabAxis GetTabletAxis(EAxisDimension dimension_I)
        {
            WintabAxis axis = new WintabAxis();
            IntPtr buf = CMemUtils.AllocUnmanagedBuf(axis);

            try
            {
                int size = (int)CWintabFuncs.WTInfo(
                    (uint)EWTICategoryIndex.WTI_DEVICES,
                    (uint)dimension_I, buf);

                axis = CMemUtils.MarshalUnmanagedBuf<WintabAxis>(buf, size);
            }
            finally
            {
                CMemUtils.FreeUnmanagedBuf(buf);
            }

            return axis;
        }
    }
}
