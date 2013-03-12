using System;
using System.Collections.Generic;
using System.Text;
using QuickFont;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// This class is created for making use of
    /// tipWindow class easier to handle by
    /// Game Manager
    /// </summary>
    class tipsManager
    {
        static private List<tipWindow> activeTipWindows = new List<tipWindow>();

        static public void Add(tipWindow t)
        {
            activeTipWindows.Add(t);
        }
        
        /// <summary>
        /// Update actual tips, if there's
        /// any.
        /// </summary>
        /// <returns>returns true if there was some Tip and if was updated, otherwise false.</returns>
        static public bool Update()
        {
            if (activeTipWindows.Count == 0) return false;
            activeTipWindows[activeTipWindows.Count - 1].Update();

            if (activeTipWindows[activeTipWindows.Count - 1].isClosed)
            {
                activeTipWindows.RemoveAt(activeTipWindows.Count - 1);
                return false;
            }
            return activeTipWindows[activeTipWindows.Count-1].pauseGame;
        }
        static public void prepareRender()
        {
            if (activeTipWindows.Count == 0) return;
            if(activeTipWindows[activeTipWindows.Count - 1] !=null) activeTipWindows[activeTipWindows.Count - 1].Render();
        }

        internal static void newLevel(int level)
        {
            switch (level)
            {
                case 1: // [CRASH] tipWindow - wrong initializer
                    try
                    {
                        tipWindow wnd = new tipWindow("../data/Textures/GAME/TIPS/tipScreen_IMG1.dds", Lang.cur.tip1_title, Lang.cur.tip1_contents);
                        if (wnd != null) wnd.MoveImage(-64, 0);
                    }
                    catch (Exception e) { new DebugMsg("[EXCEPTION]" + e.Message); }
                break;
            }
        }
    }
    /// <summary>
    /// Class is fully self-operating it means you
    /// don't have to hold created tipWindows somewhere,
    /// they're holded in static memory of class an all you have
    /// to do is call Render and Update methods (static) from
    /// gameManager
    /// </summary>
    class tipWindow : engineReference
    {
        private static Obj background = new Obj("../data/Textures/GAME/TIPS/tipScreen_BG.dds", 0.5, 0.5, Obj.align.CENTER_BOTH);
        private static QFont titleFont = TextManager.newQFont("CheriPL.ttf", 31);
        private static QFont descFont = TextManager.newQFont("Rumpelstiltskin.ttf",22,false,new OpenTK.Graphics.Color4(130,146,231,255));

        private Timer easeInTimer;
        private Timer easeOutTimer;

        private Obj image;
        private Text titleText, descriptionText;
        /// <summary>
        /// When bool is true, tipManager will know it's time to throw away
        /// this instance of tipWindow. (happens after clicking "OK")
        /// </summary>
        public bool isClosed { get; private set; }
        // True just after hitting "OK".
        private bool isClosing;
        // True after creating class object, till end of easing in ani
        private bool isOpening;
        private bool isOpened;
        private bool isInited;
        public bool pauseGame{ get; private set;}

        static tipWindow()
        {
            background.isGUIObjectButUnscaled = true;
        }
        /// <summary>
        /// Creates and inits (puts on screen)
        /// tip screen object with choosen:
        /// </summary>
        /// <param name="imgPath">Image on left to display (new monster/object, etc.)</param>
        /// <param name="title">Title of tip, like: "Portals"</param>
        /// <param name="desc">Description showed under title on tipScreen</param>
        public tipWindow(String imgPath, String title, String desc)
        {
            float x, y;

            Layer.currentlyWorkingLayer = Layer.textGUIFG;
            // Image:
            image = new Obj(imgPath,0.5,0.5,Obj.align.CENTER_Y);
            image.x -= 732/2 + 100;//732 - width of background
            image.isGUIObjectButUnscaled = true;
            // Setup proper layers for tipWindow Obj's:
            background.layer = Layer.imgGUI;
            image.layer = Layer.imgGUI;

            // Title:
            x = engine.Width / 2 + 130f - titleFont.Measure(title).Width / 2f;
            y = engine.Height/2 - 130f;
            titleText = new Text(titleFont, x, y, title);
            // Contents: 
            desc = TextManager.breakTextWithNewlines(desc, 38);
            x = engine.Width / 2 + 125f - descFont.Measure(desc).Width / 2f;
            y = engine.Height / 2 -descFont.Measure(desc).Height / 2f + 10;
            descriptionText = new Text(descFont,  x, y, desc);
            // Timers:
            easeInTimer = new Timer(Timer.eUnits.MSEC, 400, 0, true, false);
            easeOutTimer = new Timer(Timer.eUnits.MSEC, 200, 0, true, false);
            // Add to Tips Manager:
            tipsManager.Add(this);
            Layer.currentlyWorkingLayer = -1;
        }

        public void Update()
        {
            engine.gameState |= Game.game_state.Menu;
            if (!isInited)
            {
                easeInTimer.start();
                isInited = true;
                isOpening = true;
            }
            // Still easing in:
            else if (easeInTimer.enabled)
            {
                background.setCurrentTexAlpha((byte)(easeInTimer.partDoneReverse * 255.0));
                image.setCurrentTexAlpha((byte)(easeInTimer.partDoneReverse * 255.0));
                 }
            else if (isOpening)
            {          
                isOpening = false;
                Open();
            }
            //Easing out:
            else if (easeOutTimer.enabled)
            {
                background.setCurrentTexAlpha((byte)(easeOutTimer.partDone * 255));
                image.setCurrentTexAlpha((byte)(easeOutTimer.partDone * 255));
            }
            //Eased out, finalize close
            else if (isClosing)
            {
                Destroy();
                isClosing = false;
                isClosed = true;
            }
        }

        private void Open()
        {
            // Create menu, "OK" item, show fonts
            isOpened = true;
            pauseGame = true;
            engine.gameState |= Game.game_state.Menu;
            Menu_Manager mgr = engine.menuManager;
            mgr.current_menu = new Menu("MENU_TIP", null,Menu_Manager.cursor);
            float x, y;
            x = engine.Width / 2 + 235f;
            y = engine.Height / 2 + 95f;

            Layer.currentlyWorkingLayer = Layer.textGUIFG;
            Menu_Item OK = new Menu_Item("OK", x, y, Menu.font,
                                                    Menu.font_Hover,
                                                    Menu.font_Click,
                                                    this.Close);
            mgr.current_menu.addItem(OK);

            Layer.currentlyWorkingLayer = -1;
        }

        public void Render()
        {
            background.prepareRender();
            image.prepareRender();
            if (isOpened)
            {
                titleText.Update();
                descriptionText.Update();
            }
        }
        public void Close()
        {
            engine.menuManager.current_menu = null;
            isOpened = false;
            isClosing = true;
            easeOutTimer.start();
        }
        private void Destroy()
        {
            pauseGame = false;
            background.addedToViewport = image.addedToViewport = false;
            engine.gameState &= ~Game.game_state.Menu;
        }
        public void MoveImage(int x, int y)
        {
            image.x += x;
            image.y += y;
        }
    }
}
