/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using E8Tools.V8Unpack.Exceptions;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace E8Tools.V8Unpack
{
    internal static class Utils
    {
        
        public static DateTime FromFileDate(UInt64 serializedDate)
        {
            return new DateTime((long)serializedDate * 1000);
        }

        public static T Read<T>(Stream stream) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var buffer = new byte[size];
            var bytesRead = stream.Read(buffer, 0, size);
            if (bytesRead == size)
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    var ptr = handle.AddrOfPinnedObject();
                    var data = (T)Marshal.PtrToStructure(ptr, typeof(T));
                    return data;
                }
                finally
                {
                    handle.Free();
                }
            }
            throw new FileFormatException();
        }

        public static bool ReadCertainChar(Stream stream, byte character)
        {
            return stream.ReadByte() == character;
        }

        public static UInt64 ReadUIntFromHexString<T>(Stream stream)
        {
            var size = Marshal.SizeOf(typeof(T)) * 2;
            var buffer = new byte[size];
            var bytesRead = stream.Read(buffer, 0, size);
            if (bytesRead == size)
            {
                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    var ptr = handle.AddrOfPinnedObject();
                    var data = (T)Marshal.PtrToStructure(ptr, typeof(T));
                    UInt64 result = 0;
                    for (int i = 0; i < size; i++)
                    {
                        result = (result << 4) | (byte)FromHexDigit(buffer[i]);
                    }
                    return result;
                }
                finally
                {
                    handle.Free();
                }
            }
            throw new FileFormatException();
        }

        public static int FromHexDigit(byte hexByte)
        {
            if (hexByte >= (byte)'0' && hexByte <= (byte)'9') return hexByte - (byte)'0';
            if (hexByte >= (byte)'a' && hexByte <= (byte)'f') return hexByte - (byte)'a' + 10;
            if (hexByte >= (byte)'A' && hexByte <= (byte)'F') return hexByte - (byte)'A' + 10;
            throw new FileFormatException();
        }

    }
}
