using System;
using System.Collections.Generic;

using System.Text;
using CookieMonster.DLL;
using CookieMonster;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace CookieMonster.CookieMonster_Objects
{
	class VideoPlayer : engineReference
	{
		public unsafe Bink.BINK*  binkRef{get; private set;}
        public unsafe Bink.BINKBUFFER* binkBufRef{get; private set;}
        private bool binkBufferReCreationAttempted; //if TRUE method recreateBinkBuffer will take no effect
        private Sound videoSoundtrack;
        private string videoBikPath;
		public BinkGL.RAD3D rad3d { get; private set;}
        public BinkGL.RAD3DIMAGE radImage { get; private set; }
        private OpenTK.DisplayResolution storedResolution;
        private OpenTK.DisplayResolution filmResolution;
        public bool someVideoIsPlaying { get; private set; }
        private bool exceptionOnOpeningBuffer;
		unsafe public void playVideo(string path)
        {
            videoBikPath = path;
            if (Profile.currentProfile.config.commandline.noVideos) { stopPlayingVideo(); return;}

            //set OpenTK rendering to black fill:
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            engine.SwapBuffers();

            someVideoIsPlaying = true;
            binkBufferReCreationAttempted = false;
            binkRef = DLL.Bink.BinkOpen(path, 0);//Bink.openFlags.BINKNOSKIP|Bink.openFlags.BINKCOPYNOSCALING);
                /*all scallings+different bliting*/
            //1-works on xp x86 and win7 windowed
            
            try
            {
                binkBufRef = DLL.Bink.BinkBufferOpen(engine.windowHandle, binkRef->Width, binkRef->Height, 0);
            }
            catch (Exception e)
            {
                new CookieMonster.CookieMonster_Objects.DebugMsg("EXCEPTION[Bink.BinkBufferOpen]: " + e.Message);
                engine.setScreenMode(false);
                exceptionOnOpeningBuffer = true;
            }
            finally
            {
                if(binkBufRef==null)
                    try
                    {
                        binkBufRef = DLL.Bink.BinkBufferOpen(engine.windowHandle, binkRef->Width, binkRef->Height, 1);
                    }
                    catch { }
            }
            //if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major > 5)
            //{
            //    engine.WindowState = OpenTK.WindowState.Fullscreen; //BUGFIX: Set now to fullscreen only on winXP
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                engine.SwapBuffers();
            //}
            BinkGL.Start_RAD_3D_frame(rad3d);
           
		}
        /// <summary>
        /// Triggered from Game.onRender
        /// </summary>
        /// <returns>True when some video was playing and frame was processed, else it will return false</returns>
        unsafe public bool renderVideoLoop()
        {
            if (someVideoIsPlaying)
            {
                if(binkRef->FrameNum<=1)
                    tryToPlaySoundTrack(videoBikPath);
                if (binkRef->FrameNum + 1 < binkRef->Frames)
                    BinkGL.ProcessFrame(this);
                else
                    stopPlayingVideo();//last frame was rendered, closing Bink
                return true;
            }
            else
                return false;
        }
        unsafe public void stopPlayingVideo()
        {
            someVideoIsPlaying = false;
            DLL.Bink.BinkBufferClose(binkBufRef);
            DLL.Bink.BinkClose(binkRef);
            if (exceptionOnOpeningBuffer) engine.setScreenMode(true);

            if (videoBikPath == "../data/Videos/logo.bik")
                engine.afterLogoVideo();
            else if (videoBikPath == "../data/Videos/intro.bik")
            {
                engine.afterIntroVideo();                
            }
        }
        public void setNewRADIMAGE3D(BinkGL.RAD3DIMAGE new_)
        {
            radImage = new_;
        }
        unsafe public void recreateBinkBuffer()
        {
            if (binkBufferReCreationAttempted) return;

            binkBufferReCreationAttempted = true;
            try
            {
                binkBufRef = DLL.Bink.BinkBufferOpen(engine.windowHandle, binkRef->Width, binkRef->Height, 1);
                BinkGL.Start_RAD_3D_frame(rad3d);
            }
            catch { }
               
        }
        private void tryToPlaySoundTrack(string path)
        {
            path = path.Substring(0, path.LastIndexOf("."));
            path = path + "_track.ogg";
            if(System.IO.File.Exists(path))
            {
                videoSoundtrack = new Sound(Sound.eSndType.UNKNOWN,path,false,true);
            }
        }
        private void setBestResolution()
        {            
            return;
            EngineApp.Game g = engine;
            storedResolution = OpenTK.DisplayDevice.Default.SelectResolution(g.Width, g.Height, OpenTK.DisplayDevice.Default.BitsPerPixel, OpenTK.DisplayDevice.Default.RefreshRate);
            double screenProportions = (double)storedResolution.Height / storedResolution.Width;
            unsafe
            {
                filmResolution = OpenTK.DisplayDevice.Default.SelectResolution((int)binkRef->Width, (int)(binkRef->Width * screenProportions), storedResolution.BitsPerPixel, storedResolution.RefreshRate);
                if (filmResolution.Width == storedResolution.Width &&
                filmResolution.Width != binkRef->Width)
                    filmResolution = OpenTK.DisplayDevice.Default.SelectResolution((int)binkRef->Width, 0, 32, 70);
                if (filmResolution.Width != binkRef->Width)
                    filmResolution = OpenTK.DisplayDevice.Default.SelectResolution((int)binkRef->Width,(int)binkRef->Height, 32, 0);

            }
            if (filmResolution.Width != storedResolution.Width)
                OpenTK.DisplayDevice.Default.ChangeResolution(filmResolution);
        }
        private void restoreResolution()
        {
            return;
            if(filmResolution.Width != storedResolution.Width)
                OpenTK.DisplayDevice.Default.ChangeResolution(storedResolution);
        }


        internal unsafe void keyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs k)
        {
            if (someVideoIsPlaying)
            {
                //Skip playing bink video:
                DLL.Bink.BinkGoto(binkRef, binkRef->Frames - 1, 1);//1=BINKGOTOQUICK;
                //Skip video soundtrack:
                if(videoSoundtrack!=null)
                videoSoundtrack.Free();
            }
        }
    }
}
