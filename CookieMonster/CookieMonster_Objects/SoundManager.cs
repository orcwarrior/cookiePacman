using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;
using CookieMonster.DLL;

namespace CookieMonster.CookieMonster_Objects
{
    class SoundManager : engineReference
    {
        List<Sound> Music = new List<Sound>();
        List<Sound> loopingMusic = new List<Sound>();
        List<Sound> SFX = new List<Sound>();
        List<Sound> loopingSFX = new List<Sound>();
        public bool sndMgr_Initialized { get; private set; }
        public SoundManager()
        {
            //No sound? don't init bass.dll!!!
            if (Profile.currentProfile.config.commandline.noSound) return;

            try
            {
                sndMgr_Initialized = DLL.Bass.BASS_Init(-1, 44100, 0, 0, (object)null);
            }
            catch (Exception e)
            {
                engine.menuManager.showAlert("Nieudana inicjalizacja Bass.dll:\n"+e.Message+"\n ...ponowna inicjalizacja");
                try
                {
                    DLL.Bass.BASS_Free();
                    sndMgr_Initialized = DLL.Bass.BASS_Init(-1, 44100, 0, 0, (object)null);
                }
                catch (Exception dllExcptn)
                {
                    engine.menuManager.showAlert("Nieudana inicjalizacja Bass.dll:\n" + dllExcptn.Message + "\n ...ponowna inicjalizacja");
                }
            }
        }
        public void addLoopingSound(Sound s)
        {
            if (s.sndType == Sound.eSndType.MUSIC)
                loopingMusic.Add(s);
            else if (s.sndType == Sound.eSndType.SFX)
                loopingSFX.Add(s);
        }
        public void addSound(Sound s)
        {
            if (s.sndType == Sound.eSndType.MUSIC)
                Music.Add(s);
            else if (s.sndType == Sound.eSndType.SFX)
                SFX.Add(s);
        }
        public Sound getSoundByFilename(string fname)
        {
            for (int i = 0; i < SFX.Count; i++)
                if (SFX[i].name == fname) return SFX[i];
            for (int i = 0; i < loopingMusic.Count; i++)
                if (loopingMusic[i].name == fname) return loopingMusic[i];
            for (int i = 0; i < loopingSFX.Count; i++)
                if (loopingSFX[i].name == fname) return loopingSFX[i];
            for (int i = 0; i < Music.Count; i++)
                if (Music[i].name == fname) return Music[i];
            return null;
        }
        /// <ummary>
        /// Checks for Sounds that finished playing
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < SFX.Count; i++)
                if (SFX[i].state != Sound.eSoundState.STOP)
                    if (Bass.BASS_ChannelIsActive(SFX[i].stream) == Bass.BASS_ACTIVE_STOPPED)
                    {
                        SFX[i].Free();//free resources when sound stops playing
                    }

            for (          int i = 0; i < Music.Count; i++)
                if (Music[i].state != Sound.eSoundState.STOP)
                    if (Bass.BASS_ChannelIsActive(Music[i].stream) == Bass.BASS_ACTIVE_STOPPED)
                    {
                        Music[i].Stop();//free resources when sound stops playing
                    }  
        }
        public bool removeFromSndList(Sound s)
        {
            if (s.sndType == Sound.eSndType.MUSIC)
            {
                for(int i=0;i<Music.Count;i++)
                    if(Music[i].Equals(s))
                    {
                        Music.RemoveAt(i);
                        return true;
                    }
                for(int i=0;i<loopingMusic.Count;i++)
                    if(loopingMusic[i].Equals(s))
                    {
                        loopingMusic.RemoveAt(i);
                        return true;
                    }
            }
            else if(s.sndType == Sound.eSndType.SFX)
            {

                for (int i = 0; i < SFX.Count; i++)
                    if (SFX[i].Equals(s))
                    {
                        SFX.RemoveAt(i);
                        return true;
                    }
                for (int i = 0; i < loopingSFX.Count; i++)
                    if (loopingSFX[i].Equals(s))
                    {
                        loopingSFX.RemoveAt(i);
                        return true;
                    }
            }
            return false;//sound wasn't found
        }
        public void recalculateSFX()
        {
            for (int i = 0; i < SFX.Count; i++)
                SFX[i].recalculateVolume();
            for (int i = 0; i < loopingSFX.Count; i++)
                loopingSFX[i].recalculateVolume();
        }
        public void recalculateMusic()
        {
            for (int i = 0; i < loopingMusic.Count; i++)
                loopingMusic[i].recalculateVolume();
            for (int i = 0; i < Music.Count; i++)
                Music[i].recalculateVolume();
        }
        public void removeAllMusic()
        {
            loopingMusic.Clear();
            Music.Clear();
        }
    }
}
