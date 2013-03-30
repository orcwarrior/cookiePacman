using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Class responsible for each sound created in game
    /// For proper handling of sounds they all will be automatically
    /// added to SoundManager.
    /// </summary>
    class Sound
    {
        public enum eSndType { UNKNOWN, SFX, MUSIC }
        public eSndType sndType { get; private set; }
        private static SoundManager gameSndMgr;
        private string filename; public string name { get { return filename; } }
        private int _stream; public int stream { get { return _stream; } }
        private bool looping;
        public enum eSoundState { PLAY, PAUSE, STOP }
        public eSoundState state { get; private set; }
        private double baseVolume;//volume = baseVolume * sfx/MusicVolume
        public double volume
        {
            get
            {
                if (!gameSndMgr.sndMgr_Initialized) return -1;
                float vol = 0.0f;
                DLL.Bass.BASS_ChannelGetAttribute(stream, DLL.Bass.BASS_ATTRIB_VOL, ref vol);
                return (double)vol;
            }
            set
            {
                if (!gameSndMgr.sndMgr_Initialized) return;
                baseVolume = value;
                double vol = baseVolume;
                if (sndType == eSndType.MUSIC)
                    vol *= Profile.currentProfile.config.options.sound.musicVol;
                if (sndType == eSndType.SFX)
                    vol *= Profile.currentProfile.config.options.sound.sfxVol;
                DLL.Bass.BASS_ChannelSetAttribute(stream, DLL.Bass.BASS_ATTRIB_VOL, (float)vol); 
            }
        }
        public Sound(eSndType type, string f, bool loop, bool autoplay)
        {
            if (!gameSndMgr.sndMgr_Initialized) return;
            sndType = type;
            unsafe
            {
                int error;
                filename = f;
                StringBuilder str = new StringBuilder(f);

                char[] fileChar = filename.ToCharArray();
                looping = loop;
                byte[] bytes = Encoding.ASCII.GetBytes(fileChar, 0, filename.Length - 1);
                IntPtr fPtr = Marshal.AllocHGlobal(bytes.Length);
                void* vfPtr = (void*)fPtr;
                Marshal.Copy(bytes, 0, fPtr, bytes.Length);
                if (looping)
                {
                    _stream = DLL.Bass.BASS_StreamCreateFile(false, filename, 0, 0, DLL.Bass.BASS_UNICODE | DLL.Bass.BASS_SAMPLE_LOOP);
                    gameSndMgr.addLoopingSound(this);
                }
                else
                {
                    _stream = DLL.Bass.BASS_StreamCreateFile(false, filename, 0, 0, DLL.Bass.BASS_UNICODE | DLL.Bass.BASS_STREAM_AUTOFREE);
                    gameSndMgr.addSound(this);
                }
                error = DLL.Bass.BASS_ErrorGetCode();
                if (autoplay)
                {
                    Play();
                }                
                error *= 10;
                volume = 1.0;//BUGFIX: when sound should be muted by global config it always need to be set through constructor
            }
        }
        public Sound(eSndType type, string f, bool loop, bool autoplay, double vol)
            : this(type,f, loop, autoplay)
        {   
            volume = vol;
        }
        public bool Play()
        {
            if (!gameSndMgr.sndMgr_Initialized) return false;
            return DLL.Bass.BASS_ChannelPlay(stream, looping);
        }
        public void Stop()
        {
            if (!gameSndMgr.sndMgr_Initialized) return;
            state = eSoundState.STOP;
            DLL.Bass.BASS_ChannelStop(stream);
        }

        public static void setSndMgr(SoundManager sndMgr)
        {
            gameSndMgr = sndMgr;
        }
        public void Free()
        {
            if (!gameSndMgr.sndMgr_Initialized) return;

            if(state != eSoundState.STOP)
                Stop();
            gameSndMgr.removeFromSndList(this);
            DLL.Bass.BASS_StreamFree(stream);
            //Remove from soundManager List:

        }
        public void recalculateVolume()
        {
            volume = baseVolume;//set accesor will do all of the job
        }
        public void fadeIn(float dstVol, int msecTime)
        {
            if (!gameSndMgr.sndMgr_Initialized) return;

            this.volume = dstVol;//push it through global volume of music/sfx
            dstVol = (float)this.volume;
            DLL.Bass.BASS_ChannelSetAttribute(stream, DLL.Bass.BASS_ATTRIB_VOL, 0f);
            DLL.Bass.BASS_ChannelSlideAttribute(stream, DLL.Bass.BASS_ATTRIB_VOL, dstVol, msecTime);
        }
    }
}
