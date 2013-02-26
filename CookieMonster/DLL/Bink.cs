using System;
using System.Collections.Generic;

using System.Text;
using System.Runtime.InteropServices;

namespace CookieMonster.DLL
{
    static class Bink
    {
        #region flagClass

        #region BINK_OPEN_FLAGS
        public class openFlags
        {
            public static UInt32 BINKYAINVERT = 0x00000800; // Reverse Y and A planes when blitting (for debugging)
            public static UInt32 BINKFRAMERATE = 0x00001000; // Override fr (call BinkFrameRate first)
            public static UInt32 BINKPRELOADALL = 0x00002000; // Preload the entire animation
            public static UInt32 BINKSNDTRACK = 0x00004000; // Set the track number to play
            public static UInt32 BINKOLDFRAMEFORMAT = 0x00008000; // using the old Bink frame format (internal use only)
            public static UInt32 BINKRBINVERT = 0x00010000; // use reversed R and B planes (internal use only)
            public static UInt32 BINKGRAYSCALE = 0x00020000; // Force Bink to use grayscale
            public static UInt32 BINKNOMMX = 0x00040000; // Don't use MMX
            public static UInt32 BINKNOSKIP = 0x00080000; // Don't skip frames if falling behind
            public static UInt32 BINKALPHA = 0x00100000; // Decompress alpha plane (if present)
            public static UInt32 BINKNOFILLIOBUF = 0x00200000; // Fill the IO buffer in SmackOpen
            public static UInt32 BINKSIMULATE = 0x00400000; // Simulate the speed (call BinkSim first)
            public static UInt32 BINKFILEHANDLE = 0x00800000; // Use when passing in a file handle
            public static UInt32 BINKIOSIZE = 0x01000000; // Set an io size (call BinkIOSize first)
            public static UInt32 BINKIOPROCESSOR = 0x02000000; // Set an io processor (call BinkIO first)
            public static UInt32 BINKFROMMEMORY = 0x04000000; // Use when passing in a pointer to the file
            public static UInt32 BINKNOTHREADEDIO = 0x08000000; // Don't use a background thread for IO
            public static UInt32 BINKCOPYNOSCALING = 0x70000000;
        }
        #endregion
        #endregion
        #region structs
        struct BINKRECT
        {
            Int32 Left;
            Int32 Top;
            Int32 Width;
            Int32 Height;
        };
        public struct BINK
        {
            public UInt32 Width;
            public UInt32 Height;
            public UInt32 Frames;
            public UInt32 FrameNum;
            public UInt32 FrameRate;
            public UInt32 FrameRateDiv;
            public UInt32 ReadError;
            public UInt64  OpenFlags;
            public unsafe fixed Int32 FrameRects[8*4];// Dirty rects from BinkGetRects // should be BINKRECT struct
            public UInt32 NumRects;
            public UInt32 NumTracks;
        };
        public struct BINKBUFFER
        {
            public UInt32 Width;
            public UInt32 Height;
            public UInt32 WindowWidth;
            public UInt32 WindowHeight;
            public UInt32 SurfaceType;
            public unsafe void* Buffer;
            public UInt32 BufferPitch;
            public Int32 ClientOffsetX;
            public Int32 ClientOffsetY;
            public UInt32 ScreenWidth;
            public UInt32 ScreenHeight;
            public UInt32 ScreenDepth;
            public UInt32 ExtraWindowWidth;
            public UInt32 ExtraWindowHeight;
            public UInt32 ScaleFlags;
            public UInt32 StretchWidth;
            public UInt32 StretchHeight;
            
            public Int32 surface;
            public unsafe void* ddsurface;
            public unsafe void* ddclipper;
            public Int32 destx, desty;
            public UInt32 HWND;
            public Int32 ddoverlay;
            public Int32 ddoffscreen;
            public Int32 lastovershow;

            public Int32 issoftcur;
            public UInt32 cursorcount;
            public unsafe void* buffertop;
            public UInt32 type;
            public Int32 noclipping; 


        };
        
        #endregion

        //  [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("binkw32.dll", EntryPoint = "_BinkOpen@8")]
        unsafe public static extern Bink.BINK* BinkOpen(string file, UInt32 openFlags);

        [DllImport("binkw32.dll", EntryPoint = "_BinkWait@4")]
        unsafe public static extern Int32 BinkWait(Bink.BINK* bink);

        [DllImport("binkw32.dll", EntryPoint = "_BinkOpenDirectSound@4")]
        unsafe public static extern IntPtr BinkOpenDirectSound(UInt32 arg1);

        [DllImport("binkw32.dll", EntryPoint = "_BinkSetSoundSystem@8")]
        unsafe public static extern UInt32 BinkSetSoundSystem(IntPtr DSnd, IntPtr arg2);

        [DllImport("binkw32.dll", EntryPoint = "_BinkDoFrame@4")]
        unsafe public static extern UInt32 BinkDoFrame(Bink.BINK* bnk);

        [DllImport("binkw32.dll", EntryPoint = "_BinkNextFrame@4")]
        unsafe public static extern void BinkNextFrame(Bink.BINK* bnk);
        [DllImport("binkw32.dll", EntryPoint = "_BinkCopyToBuffer@28")]
        unsafe public static extern void BinkCopyToBuffer(Bink.BINK* bnk, void* buf ,UInt32 destpitch,UInt32 destheight,UInt32 destx,UInt32 desty,UInt32 copy_flags);

        [DllImport("binkw32.dll", EntryPoint = "_BinkClose@4")]
        unsafe public static extern void BinkClose(Bink.BINK* bnk);

        [DllImport("binkw32.dll", EntryPoint = "_BinkGoto@12")]
        unsafe public static extern Bink.BINKBUFFER* BinkGoto(Bink.BINK* bnk, UInt32 frameNum, UInt32 BINK_GOTO_FLAGS);
        /*
            #define BINKGOTOQUICK          1
            #define BINKGOTOQUICKSOUND     2
         */


        //BinkBuffer
        [DllImport("binkw32.dll", EntryPoint = "_BinkBufferOpen@16")]
        unsafe public static extern Bink.BINKBUFFER* BinkBufferOpen(IntPtr wnd, UInt32 width,UInt32 height,UInt32 open_flags);

        [DllImport("binkw32.dll", EntryPoint = "_BinkBufferLock@4")]
        unsafe public static extern Int32 BinkBufferLock(BINKBUFFER* buf);

        [DllImport("binkw32.dll", EntryPoint = "_BinkBufferUnlock@4")]
        unsafe public static extern void BinkBufferUnlock(BINKBUFFER* buf);

        [DllImport("binkw32.dll", EntryPoint = "_BinkBufferClose@4")]
        unsafe public static extern void BinkBufferClose(BINKBUFFER* buf);

        [DllImport("binkw32.dll", EntryPoint = "_BinkBufferBlit@12")]
        unsafe public static extern void BinkBufferBlit(BINKBUFFER* buf, Int32* frameRects, UInt32 num_rects);
        
        [DllImport("binkw32.dll", EntryPoint = "_BinkGetRects@8")]
        unsafe public static extern UInt32 BinkGetRects (BINK* bink, UInt32 getrects_flags);

        [DllImport("binkw32.dll", EntryPoint = "_BinkBufferSetScale@12")]
        unsafe public static extern UInt32 BinkBufferSetScale(BINKBUFFER* buf,UInt32 width, UInt32 height);

        [DllImport("binkw32.dll", EntryPoint = "_BinkBufferSetResolution@12")]
        unsafe public static extern UInt32 BinkBufferSetResolution(UInt32 width, UInt32 height,UInt32 colorDepth);

        [DllImport("binkw32.dll", EntryPoint = "_BinkBufferSetOffset@12")]
        unsafe public static extern UInt32 BinkBufferSetOffset(BINKBUFFER* buf, UInt32 x, UInt32 y);
 
        
    }
}
