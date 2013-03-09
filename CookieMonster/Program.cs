/**********************************
 *  TO DO LIST:
 * * Finish rescaling/repositioning when changing resolution.                   -NEED FIXES
 * * Credits                                                                    -DONE
 * * Kiling player, respawn after killing + hurt textures
 * * Kiling MOB (by another mob, or player bomb) + hurt/dead textures
 * * Database for high scores, downloading high scores
 *   and uploading player scores to database.
 * * Create gameTips class + graphics and all needed objects of it.
 * * Create rest of maps
 * * Let game difficult, level numbers affect gameplay
 * * Don't let entering illegal chars for filename when setting profile name.   -DONE
 * * GameMap: From list of rendered objects to 2D arrays of [x,y] grid          -DONE
 * * Obj Class: adding Layer property so At the end, objects of 0 layer are rendered first, then Layer 1 ...etc
 * 
 * ERRORS:
 * ERROR: Exception caught when attempting to load file ../data/Textures/GAME/GUI/SKILL_REFRESHING_100.dds. (FIXED)
 * 
 * BUGS:
 * Bug occur when exiting to main menu from game when time slow was active an music was turned down to low volume
 * on exiting to menu, add some check if timeSlow is active and then set music volume to one saved at profile file
 * Sometimes MOBs have problems with going through the portal when movement speed is high.                  (FIXED)
 * Stop laser beam when LAZER was frozen by ice bolt.
 * * * *********************************/
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;

using OpenTK; 
using OpenTK.Input;
using OpenTK.Platform;
using OpenTK.Graphics;
using CookieMonster.CookieMonster_Objects;
using System.Threading;
using TextureLoaders;

namespace EngineApp
{
    class Game : GameWindow
    {
        [Flags]
        public enum game_state { Undef = 0,Menu = 2, Game = 4 };//ingame menu = Menu|Game
        public game_state gameState = game_state.Undef;
        public Viewport gameViewport{get; private set;}
        public Viewport menuViewport { get; private set; }
        public bool activeViewportIsGame { get { return gameViewport == activeViewport; } }
        public Viewport activeViewport
        {
            get
            {
                if ((gameState & game_state.Menu) == game_state.Menu)
                    return menuViewport;
                else if ((gameState & game_state.Game) == game_state.Game)
                    return gameViewport;
                else
                    return null;
            }
        }
        public Viewport activeViewportOrAny
        {
            get
            {
                Viewport a = activeViewport;
                if (a != null) return a;
                else if (gameViewport != null) return gameViewport;
                else if (menuViewport != null) return menuViewport;
                else return null;
            }
        }
        public string[] cmdArguments { get; private set; }
        private IntPtr _winhdl;
        public IntPtr windowHandle { get { return _winhdl; } }
        Timers_Manager timeMgr = new Timers_Manager();
        public Menu_Manager menuManager { get; private set; }
        SoundManager sndMgr; public SoundManager SoundMan { get { return sndMgr; } }
        public TextManager textMenager { get; private set; }
        GameManager gameMgr; public GameManager gameManager{ get{return gameMgr;}}
        Camera gameCam; public Camera gameCamera{get{return gameCam;}}
        Debug debugger;
        public VideoPlayer videoPlayer { get; private set; }
        public lightingEngine lightEngine { get; private set; }

        // Those values need to be stored at the begining
        // Used for proper rescale of menu items etc. "GUI" type Obj's
        // Now theese values are straight from configuration class
        
        //SimpleEngine stuff:
        Engine.Core core  = new Engine.Core();
        double time;
        int frames;



        public Game(int w, int h)
            : base(w, h)
        {
            // Set's current engine to currently created object
            engineReference.setEngine(this);

            menuViewport = new Viewport(w, h, true);
            gameViewport = new Viewport(w, h, true);
            Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
            Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
            Keyboard.KeyDown += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyDown);
            Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(Keyboard_KeyUp);
            KeyPress += new EventHandler<KeyPressEventArgs>(Keyboard_KeyPress);
            Keyboard.KeyUp += new EventHandler<KeyboardKeyEventArgs>(Profile.Profile_KeyStroke);            
        }

        public void Keyboard_KeyPress(object sender, KeyPressEventArgs p)
        {
            InputManager.KeyPress(sender, p);
            
        }
        public void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(menuManager!=null)
                menuManager.setButtonState(e.Button, true);
            if (videoPlayer != null)    // skip video on mouse down evt.
                videoPlayer.keyDown(sender, new KeyboardKeyEventArgs());
        }
        public void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (menuManager != null)
            menuManager.setButtonState(e.Button, false);
        }
        public void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs k)
        {
            if(gameMgr!=null)
               gameMgr.KeyboardEvt(sender,k);
            InputManager.KeyDown(sender, k);

            //skip playing videos:
            if (videoPlayer != null)   // skip video on keystroke
            videoPlayer.keyDown(sender, k);

            if (k.Key == Key.S)
                gameCamera.Move(0, -50);
            if (k.Key == Key.W)
                gameCamera.Move(0, 50);
            if (k.Key == Key.A)
                gameCamera.Move(-50,0);
            if (k.Key == Key.D)
                gameCamera.Move(50,0);
                    
        }
        public void Keyboard_KeyUp(object sender, KeyboardKeyEventArgs k)
        {
            InputManager.KeyUp(sender, k);
        }
        public int fps { get { return (int)RenderFrequency; } }
        protected override void OnLoad(EventArgs e)
        {   //Loads profiles list + creates default start-up profile which is extremaly important
            Profile.Initialize();

            //Set proper resolution + full-screen mode:
            OpenTK.DisplayResolution filmResolution = OpenTK.DisplayDevice.Default.SelectResolution(1280, 800, 32, 0);
            OpenTK.DisplayDevice.Default.ChangeResolution(filmResolution);
            setScreenMode(true);
            //check is current runing system is winXP:
           //if(Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major == 5)
           //     EngineApp.engine.WindowState = OpenTK.WindowState.Fullscreen; //BUGFIX: Set now to fullscreen only on winXP
            
            core.Init();
            //openGL disable unecessary stuff:
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Lighting);
            //GL.Disable(EnableCap.Dither);
            GL.Disable(EnableCap.Fog);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.Multisample);
            GL.Disable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.TextureCompressionHint, HintMode.Fastest);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Fastest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
            GL.Hint(HintTarget.FogHint, HintMode.Fastest);
            GL.Hint(HintTarget.FragmentShaderDerivativeHint, HintMode.Fastest);


            textMenager = new TextManager();
            debugger = new Debug();//debug system uses text manager
            menuViewport = new Viewport(1280, 800, true);
			gameViewport = new Viewport(1280, 800, true);
			//Select language:
			new Lang(Lang.language.PL);
			menuManager = new Menu_Manager();
            gameState = game_state.Menu;//tmp for renderLoadingCaption
            lightEngine = new lightingEngine();
            renderLoadingCaption();//needs profile &txtManager
            new Menu("forStaticInitiation", null);
            gameState = game_state.Undef;

            initTextureLoaderParameters();
            videoPlayer = new VideoPlayer();
            GL.Disable(EnableCap.DepthTest);

            KeyPress += new EventHandler<KeyPressEventArgs>(menuManager.KeyPress);
            sndMgr = new SoundManager();
            new DebugMsg(this, "fps", DebugLVL.info);
            new DebugMsg(textMenager, "textsCount");
            new DebugMsg(this, "gameState");
            new DebugMsg(this, "activeViewportIsGame");
            
            gameCam = new Camera(Camera.eType.STATIC);


            //get window handle:
            IWindowInfo ii = ((OpenTK.NativeWindow)this).WindowInfo;
            object inf = ((OpenTK.NativeWindow)this).WindowInfo;
            PropertyInfo pi = (inf.GetType()).GetProperty("WindowHandle");
            _winhdl = ((IntPtr)pi.GetValue(ii, null));
            

            Sound.setSndMgr(sndMgr);

            // Play birds sound:
            Sound bg_birds = new Sound(Sound.eSndType.MUSIC, "../data/Sounds/MENU_BIRDS_BG.ogg", true, false);
            bg_birds.fadeIn(0.15f, 10 * 1000);
            bg_birds.Play();

            //hide cursor:
            System.Windows.Forms.Cursor.Hide();

            //play logo video:
            videoPlayer.playVideo("../data/Videos/logo.bik");

            gameState = game_state.Undef;
            //Open menu
            menuManager.initCursor();
            menuManager.current_menu = new Menu("MENU_PROFILE", Menu_Instances.Menu_Profile_Open, Menu_Manager.cursor);
            
         }

        private static void initTextureLoaderParameters()
        {
            TextureLoaderParameters.MagnificationFilter = TextureMagFilter.Linear;
            TextureLoaderParameters.MinificationFilter = TextureMinFilter.LinearMipmapLinear;
            TextureLoaderParameters.WrapModeS = TextureWrapMode.ClampToBorder;
            TextureLoaderParameters.WrapModeT = TextureWrapMode.ClampToBorder;
            TextureLoaderParameters.EnvMode = TextureEnvMode.Modulate;
            
        }

        protected override void OnUnload(EventArgs e)
        {
            Viewport.scaleLog.Close();
            Debug.debugLog.Close();
        }

        protected override void OnResize(EventArgs e)
        {
            core.Resize(Width, Height);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
           // if (videoPlayer.someVideoIsPlaying)
           //     return; //don't update Game stuff when video is playing 

            SoundMan.Update();
            timeMgr.Update();
            if (gameCam != null)//before viewports 'n stuff
                gameCam.Update();
            menuManager.onUpdate();
            if (((gameState & game_state.Menu) == game_state.Menu) || (menuViewport.isFading))
                menuViewport.Update();
            if (((gameState & game_state.Game) == game_state.Game) || (gameViewport.isFading))
                gameViewport.Update();
            if (gameMgr != null)
                gameMgr.Update();

            if (Keyboard[Key.Escape] && Keyboard[Key.ShiftLeft]) Exit();
             
            if ((time += e.Time) >= 1.0)
            {
                time = 0;                
                frames = 0;
            }
            if (debugger != null) debugger.Update();

            if (lightEngine != null) lightEngine.Update();


        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            frames++;
            //Video playback stuff:
            if (videoPlayer.renderVideoLoop())
                return; //video processed, so there is nothing to process of game mechanics etc.
            core.Clear();
            lightEngine.renderStaticLightmaps();
            //SwapBuffers(); return;
            GL.DepthMask(false);
            
            //render viewports:

            if (((gameState & game_state.Game) == game_state.Game)||(gameViewport.isFading))
                gameViewport.Render();

            //MENU Always overlay GAME layer!
            if (((gameState & game_state.Menu) == game_state.Menu) || (menuViewport.isFading))
                menuViewport.Render();
            menuManager.onRender();//render menu stuff overlaying viewport(bg, etc.)

            if (debugger != null) debugger.Render();
            
            // Lightning Engine will now render light overlaying everything else:
            // NOTE: If lights was rendered during GameManager.Render() method
            // this call will do nothing.
                lightEngine.prepareRender();

            SwapBuffers();
        }

        /// <summary>
        /// Sets screen mode simply changing window state
        /// </summary>
        /// <param name="fullscreen">Set to fullscreen?</param>
 
        public void setScreenMode(bool fullscreen)
        {
         if(fullscreen && Profile.currentProfile.config.commandline.windowed != true)   
            base.WindowState = WindowState.Fullscreen;
         else
            base.WindowState = WindowState.Normal;
        }
        public int getFrame()
        {
            return frames;
        }
        public void InitGameManager()
        {
            gameState = game_state.Game;
            gameMgr = new GameManager();            
        }
        public void InitCamera()
        {
            gameCam = new Camera();
        }
        /// <summary>
        /// Occurs when closing gameMgr session fe:
        /// Ending game / Quiting to main menu
        /// </summary>
        public void closeGameManagerSession()
        {
            //TODO: remove all ingame ObjAni, Timers (Obj)[TODO]
            gameMgr.Free();
            gameMgr = null;
            gameState = gameState & ~game_state.Game;
        }
        [STAThread]
        static void Main(string[] args)
        {
            using (Game game = new Game(1280, 800))
            {
                game.cmdArguments = args;
                game.VSync = VSyncMode.Adaptive;
                game.Title = "CookieMonster Pacman";
                game.Run(90);
            }
        }
        /// <summary>
        /// Initiation of sounds that will be called before calling of
        /// Game manager initation (when new game buttons been hit)
        /// </summary>
        private void initGamePreGameManager()
        {
            SoundMan.getSoundByFilename("../data/Sounds/MENU_THEME.ogg").Free();
            SoundMan.getSoundByFilename("../data/Sounds/MENU_BIRDS_BG.ogg").Free();
            lightEngine.clearAllLights();
        }
        /// <summary>
        /// Func. called after initiation of gameManager
        /// </summary>
        private void initGamePostGameManager()
        {
            //menuViewport.Clear();
        }
        /// <summary>
        /// Loads gameManager from passed
        /// savegame
        /// </summary>
        /// <param name="savegame"></param>
        internal void loadGame(Savegame savegame)
        {
            initGamePreGameManager();
            gameMgr = new GameManager(savegame);
            initGamePostGameManager();
        }
        /// <summary>
        /// Just normally starts (New)GameManager
        /// plays intro video additionally
        /// </summary>
        internal void startGame()
        {
            initGamePreGameManager();
            videoPlayer.playVideo("../data/Videos/intro.bik");            
        }
        public void afterIntroVideo()
        {
            gameMgr = new GameManager();
            initGamePostGameManager();
        }
        internal void afterLogoVideo()
        {   
            //set game state to menu:
            gameState = game_state.Menu;
            //Show "loading..." caption:

            renderLoadingCaption();

            //check is current runing system is >winXP:
            //if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major > 5)
            //    EngineApp.engine.WindowState = OpenTK.WindowState.Fullscreen; //BUGFIX: Set now to fullscreen only on winXP

        }

        public static void renderLoadingCaption()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);//Clear buffer

            String msg = "Loading...";
            float xCenter = (float)engineReference.getEngine().activeViewportOrAny.width/ 2f;
            float yCenter = (float)Profile.currentProfile.config.options.graphics.resolution.Height/ 2f;
            float xMinus = TextManager.font_default_20.Measure(msg).Width / 2;
            //Text loading = engine.textMenager.produceText(TextManager.font_default_20, msg, xCenter - xMinus, yCenter, QuickFont.QFontAlignment.Left);
            Text loading = new Text(TextManager.font_default_20, xCenter - xMinus, yCenter, msg);
            loading.Render();
            engineReference.getEngine().SwapBuffers(); //Render buffer
        }
    }
}

