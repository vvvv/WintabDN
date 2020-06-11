///////////////////////////////////////////////////////////////////////////////
// CMemUtils.cs - memory utility functions for WintabDN
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

//#define TRACE_RAW_BYTES
// Some code requires a newer .NET version.
#define DOTNET_4_OR_LATER 

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WintabDN
{
    /// <summary>
    /// Provide utility methods for unmanaged memory management.
    /// </summary>
    public class CMemUtils
    {
        /// <summary>
        /// Allocates a pointer to unmanaged heap memory of sizeof(val_I).
        /// </summary>
        /// <param name="val_I">managed object that determines #bytes of unmanaged buf</param>
        /// <returns>Unmanaged buffer pointer.</returns>
        public static IntPtr AllocUnmanagedBuf(Object val_I)
        {
            IntPtr buf = IntPtr.Zero;

            try
            {
                int numBytes = Marshal.SizeOf(val_I);

                // First allocate a buffer of the correct size.
                buf = Marshal.AllocHGlobal(numBytes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("FAILED AllocUnmanagedBuf: " + ex.ToString());
            }

            return buf;
        }

        /// <summary>
        /// Allocates a pointer to unmanaged heap memory of given size.
        /// </summary>
        /// <param name="size_I">number of bytes to allocate</param>
        /// <returns>Unmanaged buffer pointer.</returns>
        public static IntPtr AllocUnmanagedBuf(int size_I)
        {
            IntPtr buf = IntPtr.Zero;

            try
            {
                buf = Marshal.AllocHGlobal(size_I);
            }
            catch (Exception ex)
            {
                MessageBox.Show("FAILED AllocUnmanagedBuf: " + ex.ToString());
            }

            return buf;
        }

        /// <summary>
        /// Marshals specified buf to the specified type.
        /// </summary>
        /// <typeparam name="T">type to which buf_I is marshalled</typeparam>
        /// <param name="buf_I">unmanaged heap pointer</param>
        /// <param name="size">expected size of buf_I</param>
        /// <returns>Managed object of specified type.</returns>
        public static T MarshalUnmanagedBuf<T>(IntPtr buf_I, int size)
        {
            if (buf_I == IntPtr.Zero)
            {
                throw new Exception("MarshalUnmanagedBuf has NULL buf_I");
            }
            
            // If size doesn't match type size, then return a zeroed struct.
            if (size != Marshal.SizeOf(typeof(T)))
            {
                int typeSize = Marshal.SizeOf(typeof(T));
                Byte[] byteArray = new Byte[typeSize];
                Marshal.Copy(byteArray, 0, buf_I, typeSize);
            }

            return (T)Marshal.PtrToStructure(buf_I, typeof(T));
        }

        /// <summary>
        /// Free unmanaged memory pointed to by buf_I.
        /// </summary>
        /// <param name="buf_I">pointer to unmanaged heap memory</param>
        public static void FreeUnmanagedBuf(IntPtr buf_I)
        {
            if (buf_I != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buf_I);
                buf_I = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Marshals a string from an unmanaged buffer.
        /// </summary>
        /// <param name="buf_I">pointer to unmanaged heap memory</param>
        /// <param name="size_I">size of ASCII string, includes null termination</param>
        /// <returns></returns>
        public static string MarshalUnmanagedString(IntPtr buf_I, int size_I)
        {
            string retStr = null;

            if (buf_I == IntPtr.Zero)
            {
                throw new Exception("MarshalUnmanagedString has null buffer.");
            }

            if (size_I <= 0)
            {
                throw new Exception("MarshalUnmanagedString has zero size.");
            }

            try
            {
                Byte[] byteArray = new Byte[size_I];

                Marshal.Copy(buf_I, byteArray, 0, size_I);

                System.Text.Encoding encoding = System.Text.Encoding.Unicode;
                retStr = encoding.GetString(byteArray);
            }
            catch (Exception ex)
            {
                MessageBox.Show("FAILED MarshalUnmanagedString: " + ex.ToString());
            }

            return retStr;
        }

        /// <summary>
        /// Marshal unmanaged data packets into managed WintabPacket data.
        /// </summary>
        /// <param name="numPkts_I">number of packets to marshal</param>
        /// <param name="buf_I">pointer to unmanaged heap memory containing data packets</param>
        /// <returns></returns>

        /// <summary>
        /// Marshal unmanaged data packets into managed WintabPacket data.
        /// </summary>
        /// <param name="numPkts_I">number of packets to marshal</param>
        /// <param name="buf_I">pointer to unmanaged heap memory containing data packets</param>
        /// <returns></returns>
#if DOTNET_4_OR_LATER
        public static WintabPacket[] MarshalDataPackets(UInt32 numPkts_I, IntPtr buf_I)
        {
            if (numPkts_I == 0 || buf_I == IntPtr.Zero)
            {
                return null;
            }

            WintabPacket[] packets = new WintabPacket[numPkts_I];

            int pktSize = Marshal.SizeOf(new WintabPacket());

            for (int pktsIdx = 0; pktsIdx < numPkts_I; pktsIdx++)
            {

#if TRACE_RAW_BYTES
            // Trace out the raw data bytes for each packet using a hex formatter.
            int offset = pktsIdx * pktSize;

            Debug.Write("Packet: [" + Convert.ToString(pktsIdx) + "] ");
            for (int idx = 0; idx < pktSize; idx++)
            {
                byte val = Marshal.ReadByte(buf_I + offset + idx);
                Debug.Write(String.Format( "{0,3:X2}", val));
            }
            Debug.WriteLine("");
#endif //TRACE_RAW_BYTES

                packets[pktsIdx] = (WintabPacket)Marshal.PtrToStructure(IntPtr.Add(buf_I, pktsIdx * pktSize), typeof(WintabPacket));
            }

            return packets;
        }

#else // Compile for .NET 3
        public static WintabPacket[] MarshalDataPackets(UInt32 numPkts_I, IntPtr buf_I)
        {
            if (numPkts_I == 0 || buf_I == IntPtr.Zero)
            {
                return null;
            }
        
            WintabPacket[] packets = new WintabPacket[numPkts_I];
        
            //
            // Marshal each WintabPacket in the array separately.
            // This is "necessary" because none of the other ways I tried to marshal
            // seemed to work.  It's ugly, but it works.
            //
            int pktSize = Marshal.SizeOf(new WintabPacket());
            Byte[] byteArray = new Byte[numPkts_I * pktSize];
            Marshal.Copy(buf_I, byteArray, 0, (int)numPkts_I * pktSize);
        
            Byte[] byteArray2 = new Byte[pktSize];
        
            for (int pktsIdx = 0; pktsIdx < numPkts_I; pktsIdx++)
            {
#if TRACE_RAW_BYTES
                // Trace out the raw data bytes for each packet using a hex formatter.
                Debug.Write("Packet: [" + Convert.ToString(pktsIdx) + "] ");
        
                for (int idx = 0; idx < pktSize; idx++)
                {
                    byteArray2[idx] = byteArray[(pktsIdx * pktSize) + idx];
                    Debug.Write(String.Format( "{0,3:X2}", byteArray2[idx]));
                }
                Debug.WriteLine("");
#endif //TRACE_RAW_BYTES
        
                IntPtr tmp = CMemUtils.AllocUnmanagedBuf(pktSize);
                Marshal.Copy(byteArray2, 0, tmp, pktSize);
        
                packets[pktsIdx] = CMemUtils.MarshalUnmanagedBuf<WintabPacket>(tmp, pktSize);
            }
        
            return packets;
        }
#endif //DOTNET_4_OR_LATER

        /// <summary>
        /// Marshal unmanaged Extension data packets into managed WintabPacketExt data.
        /// </summary>
        /// <param name="numPkts_I">number of packets to marshal</param>
        /// <param name="buf_I">pointer to unmanaged heap memory containing data packets</param>
        /// <returns></returns>
        public static WintabPacketExt[] MarshalDataExtPackets(UInt32 numPkts_I, IntPtr buf_I)
        {
            WintabPacketExt[] packets = new WintabPacketExt[numPkts_I];

            if (numPkts_I == 0 || buf_I == IntPtr.Zero)
            {
                return null;
            }

            // Marshal each WintabPacketExt in the array separately.
            // This is "necessary" because none of the other ways I tried to marshal
            // seemed to work.  It's ugly, but it works.
            int pktSize = Marshal.SizeOf(new WintabPacketExt());
            Byte[] byteArray = new Byte[numPkts_I * pktSize];
            Marshal.Copy(buf_I, byteArray, 0, (int)numPkts_I * pktSize);

            Byte[] byteArray2 = new Byte[pktSize];

            for (int pktsIdx = 0; pktsIdx < numPkts_I; pktsIdx++)
            {
                for (int idx = 0; idx < pktSize; idx++)
                {
                    byteArray2[idx] = byteArray[(pktsIdx * pktSize) + idx];
                }

                IntPtr tmp = CMemUtils.AllocUnmanagedBuf(pktSize);
                Marshal.Copy(byteArray2, 0, tmp, pktSize);

                packets[pktsIdx] = CMemUtils.MarshalUnmanagedBuf<WintabPacketExt>(tmp, pktSize);
            }

            return packets;
        }
    }
}
