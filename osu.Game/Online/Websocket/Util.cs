// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace osu.Game.Online.Websocket
{
    public static class Util
    {
        public static byte[] GetBytes(this Object objectToBytes)
        {
            int size = Marshal.SizeOf(objectToBytes);
            var data = new byte[size];

            IntPtr pointer = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(objectToBytes, pointer, true);
            Marshal.Copy(pointer, data, 0, size);
            Marshal.FreeHGlobal(pointer);

            return data;
        }

        public static T ToObject<T>(this byte[] data)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr pointer = Marshal.AllocHGlobal(size);

            Marshal.Copy(data, 0, pointer, size);

            var objectFromBytes = Activator.CreateInstance<T>();

            objectFromBytes = (T)Marshal.PtrToStructure(pointer, objectFromBytes.GetType());
            Marshal.FreeHGlobal(pointer);

            return objectFromBytes;
        }

        public static string ToHexString(this byte[] data)
        {
            StringBuilder hex = new StringBuilder(data.Length * 2);
            foreach (byte b in data)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
