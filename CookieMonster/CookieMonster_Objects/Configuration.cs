using System;
using System.Collections.Generic;

using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    public class commandline
    {
        public bool windowed = false;
        public bool noSound = false;
        public bool noMusic;
    }
    public class gameplay
    {
        public enum eDifficultyLevel { EASY, NORMAL, HARD, HARDCORE }
        public eDifficultyLevel level { get; set; }
        public int maps { get; set; }
    }
    public class options
    {
        public graphics graphics { get; private set; }
        public sound sound {get; private set;}
        public options()
        {
            graphics = new graphics();
            sound = new sound();
        }
    }
    public class graphics
    {
        /// <summary>
        /// current resolution
        /// </summary>
        public int resIdx;
        /// <summary>
        /// resolution that will be set when user hits "Apply" in menu
        /// </summary>
        public int newResolutionIdx;
        public OpenTK.DisplayResolution resolution;
        /// <summary>
        /// previous used resolution -> for rescaling of objects after new resolution applying
        /// </summary>
        public OpenTK.DisplayResolution oldResolution;
        public bool renderPaths;
    }
    public class sound
    {
        public double sfxVol = 0.8;

        private double _musicVol = 0;
        public double musicVol { get { return _musicVol; } set { _musicVol = value; if (Profile.currentProfile.config.commandline.noMusic) _musicVol = 0; } } 
    }

    class Configuration : engineReference
    {
        public static string boolToString(bool b)
        {
            if(b)
                return "Tak";
            else
                return "Nie";
        }
        public commandline commandline { get; private set; }
        public gameplay gameplay { get; private set; }
        public options options { get; private set; }
        public static void nextDouble(ref double dob)
        {
            dob += 0.05; if (dob > 1.0) dob = 1.0;
        }
        public static void prevDouble(ref double dob)
        {
            dob -= 0.05; if (dob < 0.0) dob = 0.0;
        }
        public Configuration()
        {
            //commandline stuff:
            commandline = new CookieMonster_Objects.commandline();
            foreach (string arg in engine.cmdArguments)
            {
                switch (arg)
                {
                    case "-windowed": commandline.windowed = true; break;
                    case "-nosound": commandline.noSound = true; break;
                    case "-nomusic": commandline.noMusic = true; break;
                }
            }
            //Load default:
            gameplay = new gameplay();
            gameplay.level = gameplay.eDifficultyLevel.NORMAL;
            gameplay.maps = 40;
            options = new options();
            
            options.graphics.resolution = OpenTK.DisplayDevice.Default.SelectResolution(1280, 800, 32, 0f);
            options.graphics.oldResolution = options.graphics.resolution;
            options.graphics.resIdx = OpenTK.DisplayDevice.Default.AvailableResolutions.IndexOf(options.graphics.resolution);
            options.graphics.newResolutionIdx = options.graphics.resIdx;
            options.graphics.renderPaths = true;
        }
        public void applyResolution()
        {
            options.graphics.oldResolution = options.graphics.resolution;
            options.graphics.resolution = OpenTK.DisplayDevice.Default.AvailableResolutions[options.graphics.newResolutionIdx];
            OpenTK.DisplayDevice.Default.ChangeResolution(options.graphics.resolution);
            options.graphics.resIdx = options.graphics.newResolutionIdx;
            Viewport.scaleLog.WriteLine("");
            Viewport.scaleLog.WriteLine("New resolution: " + options.graphics.resolution.Width.ToString() + "x" + options.graphics.resolution.Height.ToString());
            engine.activeViewport.adaptToNewResolution();

        }
        public void restoreResolutionValue()
        {
            options.graphics.newResolutionIdx = options.graphics.resIdx;
        }
        public void getNextResolution()
        {
            int lastResIdx = options.graphics.newResolutionIdx;
            options.graphics.newResolutionIdx = (options.graphics.newResolutionIdx + 1) % OpenTK.DisplayDevice.Default.AvailableResolutions.Count;
            while (OpenTK.DisplayDevice.Default.AvailableResolutions[options.graphics.newResolutionIdx].Width < OpenTK.DisplayDevice.Default.AvailableResolutions[options.graphics.newResolutionIdx].Height
                || OpenTK.DisplayDevice.Default.AvailableResolutions[options.graphics.newResolutionIdx].Width < 1024//never less than 800px in width
                || OpenTK.DisplayDevice.Default.AvailableResolutions[options.graphics.newResolutionIdx].BitsPerPixel != 32//only 32bpp
                || OpenTK.DisplayDevice.Default.AvailableResolutions[options.graphics.newResolutionIdx].RefreshRate > 100f//no more than 100Hz
                //Confront with last resolution:
                || resolutionsAreTheSame(lastResIdx,options.graphics.newResolutionIdx))
                   
                options.graphics.newResolutionIdx = (options.graphics.newResolutionIdx + 1) % OpenTK.DisplayDevice.Default.AvailableResolutions.Count; //only landscape-proportion resolutions count
        }
        public bool resolutionsAreTheSame(int idx1, int idx2)
        {
            return (OpenTK.DisplayDevice.Default.AvailableResolutions[idx1].Width == OpenTK.DisplayDevice.Default.AvailableResolutions[idx2].Width
                   && OpenTK.DisplayDevice.Default.AvailableResolutions[idx1].Height == OpenTK.DisplayDevice.Default.AvailableResolutions[idx2].Height
                   && OpenTK.DisplayDevice.Default.AvailableResolutions[idx1].RefreshRate == OpenTK.DisplayDevice.Default.AvailableResolutions[idx2].RefreshRate
                   && OpenTK.DisplayDevice.Default.AvailableResolutions[idx1].BitsPerPixel == OpenTK.DisplayDevice.Default.AvailableResolutions[idx2].BitsPerPixel
                   );
        }
        public string getResolutionString()
        {
            try
            {
                OpenTK.DisplayResolution hlp = OpenTK.DisplayDevice.Default.AvailableResolutions[options.graphics.newResolutionIdx];
                return hlp.Width + "x" + hlp.Height + "x" + hlp.BitsPerPixel + " " + hlp.RefreshRate + "Hz";
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
        }
        public string encrypt()
        {
            // those infos are stored in savegame playerData
            // so we simply don't give a fuck
            //Int32 cGameplay = 0;
            //cGameplay = 1 << 30;
            //cGameplay += (int)(gameplay.level);//max is 3 so 16(4bits) should be ok anyway
            //cGameplay += gameplay.maps << 4;
            string buffer = "";// cGameplay.ToString("X");
            const string sep = "x";

            Int32 cOptG = 1 << 31;
            cOptG += options.graphics.resIdx;
            cOptG += ((options.graphics.renderPaths==true)? 1 : 0) << 10;//spare 1024 for resIdx
            buffer += cOptG.ToString("X");
            buffer += sep;

            Int32 cOptS = 0;// 1 << 29;//0x2..
            cOptS += (int)(options.sound.sfxVol/0.05);//max val 20 so 32 (5)shift
            cOptS += (int)(options.sound.musicVol/0.05)<<5;// ----,,----
            buffer += cOptS.ToString("X");
            return buffer;
        }

        internal void decrypt(string conf)
        {
            string hlp = conf.Substring(0,conf.IndexOf("x"));
            int x = Convert.ToInt32(hlp, 16);
            //graphics:
            int resIdx = x & 0x3FF;//1023(1024)
            //bugfix - when changing runing computer specs, graphic modes array can differ in size 26.02
            if (OpenTK.DisplayDevice.Default.AvailableResolutions.Count >= resIdx)
                resIdx = 0;
            int renderPaths = (x >> 10) & 1;
            options.graphics.resIdx = resIdx;
            options.graphics.renderPaths = renderPaths == 1;
            //sound:
            hlp = conf.Substring(conf.IndexOf("x") + 1);
            x = Convert.ToInt32(hlp, 16);
            int sfx = x & 0x1F;//5bits
            int music = (x >> 5) & 0x1F;
            options.sound.sfxVol = sfx * 0.05;
            options.sound.musicVol = music * 0.05;
        }
    }
}