using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LogikUI.Interop
{
    struct Scene
    {
        public long Length;
        public unsafe Component* Components;
    }

    struct Component
    {
        public int Type;
        public int Length;
        public unsafe Property* Properties;
    }

    enum PropertyType : int
    {
        Byte,
        Short,
        Int,
        CString,
        VoidPtr,
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Property
    {
        public PropertyType Type;
        public unsafe fixed byte Data[8];

        public unsafe byte Byte => Type == PropertyType.Byte ? Data[0] : throw new InvalidOperationException();
        public unsafe short Short => Type == PropertyType.Short ? Unsafe.As<byte, short>(ref Data[0]) : throw new InvalidOperationException();

        public unsafe int Int => Type == PropertyType.Int ? Unsafe.As<byte, int>(ref Data[0]) : throw new InvalidOperationException();
        public unsafe void* VoidPtr => Type == PropertyType.VoidPtr ? Unsafe.AsPointer(ref Data[0]) : throw new InvalidOperationException();
        public unsafe IntPtr IntPtr => Type == PropertyType.VoidPtr ? (IntPtr)Unsafe.AsPointer(ref Data[0]) : throw new InvalidOperationException();
        public unsafe string String => Type == PropertyType.CString ? Marshal.PtrToStringAuto(IntPtr) : throw new InvalidOperationException();
    }

    static class Logic
    {
        const string Lib = "Native/logik_simulation";
        const CallingConvention CallingConv = CallingConvention.Cdecl;

        [DllImport(Lib, EntryPoint = "init", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Init();

        [DllImport(Lib, EntryPoint = "exit", ExactSpelling = true, CallingConvention = CallingConv)]
        public static extern void Exit();
    }
}
