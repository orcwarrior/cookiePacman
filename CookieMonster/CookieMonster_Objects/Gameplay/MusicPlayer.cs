using System;
using System.Collections.Generic;

using System.Text;
using EngineApp;
using System.IO;
using QuickFont;
using System.Drawing;
using OpenTK.Graphics;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Class used for playing music, randoming next track, any slides, fades etc.
    /// it had to be inited only, rest will be handled by itself.
    /// </summary>
    class MusicPlayer : engineReference
    {
        // evry day imma shufflin' ;D
        static Random shuffler = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        const string tracksFolder = "../data/Music/";
        const string trackInfosBG = "../data/Textures/MENU/TRACK_BG.dds";
        QFont trackTag_Font = TextManager.newQFont("KOMIKAX.ttf", 20, System.Drawing.FontStyle.Regular, true);
        String curTrackPath;
        Sound currentMusic;
        string s_curTrackTitle, s_curTrackArtist;
        Text curTrack_Title, curTrack_Artist;

        Viewport playerViewport;
        Timer trackTagsDisplayDuration;
        public MusicPlayer()
        {
            Color4 cc = new Color4(73, 160, 255, 255);
            trackTag_Font.Options.Colour = cc;
            playerViewport = new Viewport(engine.Width, engine.Height,true);
            playerViewport.partialViewport = true; // it will prevent viewport from rendering game map, texts etc.
            trackTagsDisplayDuration = new Timer(Timer.eUnits.MSEC, 9000, 0, true, false);
            curTrackPath = randomTrackName();
            playTrack();
        }
        public void Update()
        {
            //remove displayed track infos:
            if (trackTagsDisplayDuration.enabled == true)
            {
                curTrack_Title.Update();
                curTrack_Artist.Update();
            }
            if (currentMusic.state==Sound.eSoundState.STOP)
            {
                string oldPath = curTrackPath;
                for (int i = 0; ((i < 5) && curTrackPath == oldPath); i++)
                    curTrackPath = randomTrackName();
                playTrack();
            }
            playerViewport.Update();
        }
        public void prepareRender()
        {
           
          //playerViewport.Render();
        }
        public void Free()
        {
            TextManager txtMan = engine.textManager;
            txtMan.removeText(s_curTrackArtist);
            txtMan.removeText(s_curTrackTitle);
            trackTagsDisplayDuration.Dispose();
            currentMusic.Free();
        }

        #region private methods
        private string randomTrackName()
        {
            string[] tracksOGG = Directory.GetFiles(tracksFolder, "*.ogg");
            string[] tracksMP3 = Directory.GetFiles(tracksFolder, "*.mp3");
            tracksMP3.CopyTo(tracksOGG,0);
            
            int n = shuffler.Next(tracksOGG.Length);
            return tracksOGG[n];
        }
        private void playTrack()
        {
            if (currentMusic != null)
                currentMusic.Free();
            currentMusic = new Sound(Sound.eSndType.MUSIC, curTrackPath, false, true);
            currentMusic.volume = 0.80;
            //filenames need to be in format of "ARTIST - TITLE" to be correctly displayed
            String filename = curTrackPath.Substring(curTrackPath.LastIndexOf("/") + 1);
            filename = filename.Substring(0, filename.LastIndexOf("."));

            try
            {
                s_curTrackArtist = filename.Substring(0, filename.LastIndexOf("-"));
                s_curTrackTitle = filename.Substring(filename.LastIndexOf("-") + 2);
            }
            catch (Exception e)
            {   // Nazwa pliku nie byla w standardowym formacie :(
                s_curTrackArtist = filename;
                s_curTrackTitle = "";
            }
            trackTagsDisplayDuration.start();
            putTrackInfosToViewport();
        }
        private void putTrackInfosToViewport()
        {
            int screenH = engine.Height;
            //Obj bg = new Obj(trackInfosBG, 5, screenH-120, Obj.align.LEFT);
            //bg.isGUIObject = true;
            //bg.setCurrentTexAlpha(155);
            //playerViewport.addObject(bg);
            TextManager txtMan = engine.textManager;
            //TODO: Uncoment this is temporary
            Layer.currentlyWorkingLayer = Layer.textGUIFG;
            curTrack_Artist =  new Text(trackTag_Font, 50f, (float)screenH - 150, s_curTrackArtist);
            curTrack_Title  = new Text(trackTag_Font, 50f, (float)screenH - 120, s_curTrackTitle);
            Layer.currentlyWorkingLayer = -1;
        }
        #endregion
    }
}
