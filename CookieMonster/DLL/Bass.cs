
using System.Runtime.InteropServices;
using System;
using DWORD = System.UInt32;
using QWORD = System.UInt64;

namespace CookieMonster.DLL
{
    static class Bass
    {
        //tag types:
        public const int BASS_TAG_ID3 = 0;
        public const int BASS_TAG_OGG = 2;
        //returns
        public const int BASS_ACTIVE_STOPPED = 0;
        //atribs:
        public const DWORD BASS_ATTRIB_FREQ = 1;
        public const DWORD BASS_ATTRIB_VOL = 2;
        public const DWORD BASS_ATTRIB_PAN = 3;
        public const DWORD BASS_ATTRIB_EAXMIX = 4;
        public const DWORD BASS_ATTRIB_NOBUFFER = 5;
        public const DWORD BASS_ATTRIB_CPU = 7;
        public const DWORD BASS_ATTRIB_SRC = 8;

        //flags: 
        public const uint BASS_SAMPLE_8BITS = 1;// 8 bit
        public const uint BASS_SAMPLE_FLOAT = 256;// 32-bit floating-point
        public const uint BASS_SAMPLE_MONO = 2;// mono
        public const uint BASS_SAMPLE_LOOP = 4;// looped
        public const uint BASS_SAMPLE_3D = 8;// 3D functionality
        public const uint BASS_STREAM_AUTOFREE = 0x40000;
        public const uint BASS_UNICODE = 0x80000000;

        [DllImport("bass.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BASS_Init(int device, int freq, int flags, int win, [MarshalAs(UnmanagedType.AsAny)] object clsid);
        

        [DllImport("bass.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BASS_Free();

        [DllImport("bass.dll")]
        public static extern int BASS_StreamCreateFile(bool mem, [MarshalAs(UnmanagedType.LPWStr)] String str, QWORD offset, QWORD length, DWORD flags);

        [DllImport("bass.dll")]
        public static extern bool BASS_StreamFree(int handle);

        [DllImport("bass.dll")]
        public static extern bool BASS_ChannelPlay(int handle, bool restart);

        [DllImport("bass.dll")]
        public static extern bool BASS_ChannelGetAttribute(int handle, DWORD attrib, ref float value);

        [DllImport("bass.dll")]
        public static extern bool BASS_ChannelSetAttribute(int handle, DWORD attrib, float value);


        [DllImport("bass.dll")]
        public static extern bool BASS_ChannelStop(int handle);

        [DllImport("bass.dll")]
        public static extern int BASS_ErrorGetCode();

        [DllImport("bass.dll")]
        public static extern int BASS_ChannelIsActive(int handle);
        
        [DllImport("bass.dll")]
        public static extern string[] BASS_ChannelGetTags(int handle,int tags_type);


        [DllImport("bass.dll")]
        public static extern bool BASS_ChannelSlideAttribute(int handle,DWORD attrib,float value,int time_ms);


    }
}