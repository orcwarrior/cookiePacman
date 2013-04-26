using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;
using OpenTK;
using QuickFont;
using System.Drawing;
using OpenTK.Graphics;

namespace CookieMonster.CookieMonster_Objects
{
    static public class Menu_Instances
    {
        static Game engine { get {  return engineReference.getEngine(); } }
        public static void Menu_Nothing()
        {
            //YAAY!
        }
        public static void MENU_COOKIE_EYEBLINK()
        {
            Sound blink = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BLINK.ogg", false, true);
        }
        static public void Menu_InitializeBackground()
        {
            engine.lightEngine.setupMenuLightingParams();

            Viewport mViewport = engine.menuViewport;

            Obj Clouds = new Obj("../data/Textures/MENU/menu_clouds.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, true);
            Obj Clouds2 = new Obj("../data/Textures/MENU/menu_clouds.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, true);
            Clouds2.x -= Viewport.guiBase_width; // so it will ends where first one starts

            //Obj Sun = new Obj("../data/Textures/MENU/menu_sun.dds", 0.20, 0.34, Obj.align.CENTER_BOTH, true);
            //Obj_Animation sunAni = new Obj_Animation(Sun, Sun.x, Sun.y, 0.01);
            //sunAni.addKeyframe(Sun.x, Sun.y+100, 0.01);
            //Sun.addAni(sunAni);

            mViewport.addObject(new Obj("../data/Textures/MENU/menu_sky.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, true));
            //mViewport.addObject(Sun);
            mViewport.addObject(Clouds);
            mViewport.addObject(Clouds2);
            Clouds.addAni(new Obj_Animation(Clouds, Clouds.x + Viewport.guiBase_width, Clouds.y, 0.202, 1.0, true));
            // Clouds.addAniKeyframe(Clouds.x - Clouds.width, Clouds.y, 9999);

            Clouds2.addAni(new Obj_Animation(Clouds2, Clouds2.x + Viewport.guiBase_width, Clouds2.y, 0.202, 1.0, true));
            // Clouds2.addAniKeyframe(Clouds2.x - Clouds2.width, Clouds2.y, 9999);

            mViewport.addObject(new Obj("../data/Textures/MENU/menu_landscape.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, true));


            Obj cookie = new Obj("../data/Textures/MENU/COOKIE_MENU_A0.dds", 0.1448, 0.6074, Obj.align.CENTER_BOTH, true);
            cookie.setTexAniLoopType(Obj_texAni.eLoopType.REWIND);
            cookie.setTexAniFPS(10);
            mViewport.addObject(cookie);

            Obj tree = new Obj("../data/Textures/MENU/MENU_TREE_A0.dds", 0.58, 0.28, Obj.align.CENTER_BOTH, true);
            tree.setTexAniLoopType(Obj_texAni.eLoopType.REWIND);
            tree.setTexAniFPS(6);
            mViewport.addObject(tree);

            Obj cookieEyes = new Obj("../data/Textures/MENU/MENU_COOKIE_EYESBLINK.dds", 0.1175, 0.532, Obj.align.LEFT, true);
            Obj cookieEyesHlp = new Obj("../data/Textures/HLP_FullAlpha.dds", 0.0, 0.0, Obj.align.RIGHT, false);
            cookieEyesHlp.setIdleAni(cookieEyes, new Timer(Timer.eUnits.MSEC, 7 * 1000, 6 * 1000, true, false), new Timer(Timer.eUnits.FPS, 3, 2, true, false), Menu_Instances.MENU_COOKIE_EYEBLINK);
            mViewport.addObject(cookieEyesHlp);

            Obj fire = new Obj("../data/Textures/MENU/MENU_FIRE_A0.dds", 0.2905, 0.402, Obj.align.CENTER_BOTH, true);
            fire.setTexAniLoopType(Obj_texAni.eLoopType.DEFAULT);
            fire.setTexAniFPS(12);
            fire.ScaleAbs = 2.5;
            mViewport.addObject(fire);

            Obj fireLens = new Obj("../data/Textures/MENU/MENU_FIRE_LENS.dds", 0.285, 0.595, Obj.align.CENTER_BOTH, true);
            mViewport.addObject(fireLens);

            Obj logo = new Obj("../data/Textures/MENU/MENU_LOGO.dds", 0.960, 0.905, Obj.align.CENTER_BOTH, false);
            mViewport.addObject(logo);

            //"Sun" light:
            engine.lightEngine.clearAllLights();
            Light sun = new Light(eLightType.DYNAMIC,new radialGradient(new Vector2(170f,230f),500f,
                                         new Vector4(1f,0.5f,0.0f,0.95f),
                                         new Vector4(1f,0f,0f,0f)));

            sun.colorLightAni[0] = new lightColors(Color.FromArgb(220, 255, 125, 0),
                                                   Color.FromArgb(0, 255, 125, 0));
            sun.colorLightAni[1] = new lightColors(Color.FromArgb(255, 255, 100, 0),
                                                   Color.FromArgb(0, 255, 125, 0));
            sun.colorLightAni[2] = new lightColors(Color.FromArgb(200, 255, 125, 0),
                                                   Color.FromArgb(0, 255, 80, 0));
            sun.colorLightAni.aniFps = 0.1f;
            sun.scaleLightAni[0] = 1f;
            sun.scaleLightAni[1] = 1.6f;
            sun.scaleLightAni[2] = 0.7f;
            sun.scaleLightAni[3] = 0.8f;
            sun.scaleLightAni[4] = 0.9f;
            sun.scaleLightAni[5] = 0.8f;
            sun.scaleLightAni[6] = 1.1f;
            sun.scaleLightAni.aniFps = 0.06f;
            sun.slicesNum(15);
        }
        static public void Menu_Profile_Open()
        {
            Menu_InitializeBackground();
            Menu_StartBGMusic();

            Menu profile = engine.menuManager.getMenuByName("MENU_PROFILE");
            profile.clearMenuItems();            

            Profile.Menu_GenerateProfileMenu();
        }
        private static void Menu_StartBGMusic()
        {
            //start bg music:
            Sound bg_music = new Sound(Sound.eSndType.MUSIC, "../data/Sounds/MENU_THEME.ogg", true, true);
            bg_music.volume = 0.87;
            if (engine.SoundMan.getSoundByFilename("../data/Sounds/MENU_BIRDS_BG.ogg") == null)
            {
                Sound bg_birds = new Sound(Sound.eSndType.MUSIC, "../data/Sounds/MENU_BIRDS_BG.ogg", true, true);
                bg_birds.volume = 0.15;
            }
        }
        static public void Main_OnLoad()
        {
            // IF MENU_STATUS is already created it means that
            // Game was already started, then player went to status screen and come back to main menu
            // to recreate it so we doing it:
            Menu_Manager mgr = engine.menuManager;
            if (mgr.getMenuByName("MENU_STATUS") != null)
            {
                Menu_InitializeBackground();
                Menu_StartBGMusic();
            }
            // Turn on ability to enter "theather mode":
            mgr.canEnterIdleMode = true;

            // add menu items:
            // NOTE: Obj.align got to be LEFT!!! (Default)
            Menu main = engine.menuManager.getMenuByName("MENU_MAIN");
            main.clearMenuItems();
            Obj item,itemH,itemC;
            double hoverscale = 1.2;
            //NEW GAME:
            item = new Obj("../data/Textures/MENU/MENU_ITEM_NEWGAME.dds", 0.6, 0.3, Obj.align.LEFT, true);
            itemH = new Obj("../data/Textures/MENU/MENU_ITEM_NEWGAME_A.dds", 0.6, 0.3, Obj.align.LEFT, true);
            itemH.x -= (int)(0.05 * itemH.orginalWidth); itemH.y -= (int)(0.1 * itemH.orginalHeight); itemH.ScaleAbs = 1.2;
            //itemH.addAni(new Obj_Animation(itemH, itemH.x - 18, itemH.y - 6, 18.0, hoverscale,true));
            //itemH.getObjAnimation().setLoopType(Obj_Animation.eLoopType.None);
            main.addItem(new Menu_Item("MAIN_NEWGAME",item,itemH,null,null,null,Menu_NewGame_Click));
            
            //CONTINUE:
            item = new Obj("../data/Textures/MENU/MENU_ITEM_CONTINUE.dds", 0.6, 0.345, Obj.align.LEFT, true);
            itemH = new Obj("../data/Textures/MENU/MENU_ITEM_CONTINUE_A.dds", 0.6, 0.345, Obj.align.LEFT, true);
            itemH.x -= (int)(0.05 * itemH.orginalWidth); itemH.y -= (int)(0.1 * itemH.orginalHeight); itemH.ScaleAbs = 1.2;
            Obj itemDisabled = new Obj("../data/Textures/MENU/MENU_ITEM_CONTINUE_D.dds", 0.6, 0.345, Obj.align.LEFT, true);
            //itemH.addAni(new Obj_Animation(itemH, itemH.x - 18, itemH.y - 6, 18.0, hoverscale,true));
            //itemH.getObjAnimation().setLoopType(Obj_Animation.eLoopType.None);
            if(Profile.currentProfile.save!=null)
                main.addItem(new Menu_Item("MAIN_CONTINUE", item, itemH, null, null, null, Menu_LoadGame_Click));
            else//TODO: Disabled Continue button
                main.addItem(new Menu_Item("MAIN_CONTINUE", itemDisabled, itemDisabled, null, Menu_Nothing, null, null));

            //OPTIONS:
            item = new Obj("../data/Textures/MENU/MENU_ITEM_OPTIONS.dds", 0.6, 0.408, Obj.align.LEFT, true);
            itemH = new Obj("../data/Textures/MENU/MENU_ITEM_OPTIONS_A.dds", 0.6, 0.408, Obj.align.LEFT, true);
            itemH.x -= (int)(0.05 * itemH.orginalWidth); itemH.y -= (int)(0.1 * itemH.orginalHeight); itemH.ScaleAbs = 1.2;
            //itemH.addAni(new Obj_Animation(itemH, itemH.x - 18, itemH.y - 6, 18.0, hoverscale,true));
            //itemH.getObjAnimation().setLoopType(Obj_Animation.eLoopType.None);
            main.addItem(new Menu_Item("MAIN_OPTIONS", item, itemH, null, null, null, Menu_Options_Click));
            
            //CREDITS:
            item = new Obj("../data/Textures/MENU/MENU_ITEM_CREDITS.dds", 0.6, 0.46, Obj.align.LEFT, true);
            itemH = new Obj("../data/Textures/MENU/MENU_ITEM_CREDITS_A.dds", 0.6, 0.46, Obj.align.LEFT, true);
            itemH.x -= (int)(0.05 * itemH.orginalWidth); itemH.y -= (int)(0.1 * itemH.orginalHeight); itemH.ScaleAbs = 1.2;
            //itemH.addAni(new Obj_Animation(itemH, itemH.x - 18, itemH.y - 6, 18.0, hoverscale,true));
            //itemH.getObjAnimation().setLoopType(Obj_Animation.eLoopType.None);
            main.addItem(new Menu_Item("MAIN_CREDITS", item, itemH, null, null, null, Menu_Credits_Click));

            //QUIT:
            item = new Obj("../data/Textures/MENU/MENU_ITEM_QUIT.dds", 0.6, 0.52, Obj.align.LEFT, true);
            itemH = new Obj("../data/Textures/MENU/MENU_ITEM_QUIT_A.dds", 0.6, 0.52, Obj.align.LEFT, true);
            itemH.x -= (int)(0.05 * itemH.orginalWidth); itemH.y -= (int)(0.1 * itemH.orginalHeight); itemH.ScaleAbs = 1.2;
            //itemH.addAni(new Obj_Animation(itemH, itemH.x - 18, itemH.y - 6, 18.0, hoverscale,true));
            //itemH.getObjAnimation().setLoopType(Obj_Animation.eLoopType.None);
            main.addItem(new Menu_Item("MAIN_EXIT", item, itemH, null, null, null, Menu_Exit_Click));
        }
        public static void Menu_NewGame_Click()
        {
            Menu selectLvl = engine.menuManager.getMenuByName("MENU_NEWGAME_1");
            if (selectLvl == null) selectLvl = new Menu("MENU_NEWGAME_1", null);
            selectLvl.clearMenuItems();

            float pX = 700f, pY = 230f;
            Obj BG = new Obj("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds", 0.52, 0.22, Obj.align.LEFT, true);
            selectLvl.addItem(new Menu_Item("BG", BG, null, null, null, null, null));

            selectLvl.addItem(new Menu_Item(Lang.cur.Latwy, pX, pY, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Easy));
            selectLvl.addItem(new Menu_Item(Lang.cur.Normalny, pX, pY + 50f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Normal));
            selectLvl.addItem(new Menu_Item(Lang.cur.Trudny, pX, pY + 100f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Hard));
            selectLvl.addItem(new Menu_Item(Lang.cur.Hardcore, pX, pY + 150f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Hardcore));
            selectLvl.addItem(new Menu_Item(Lang.cur.Wroc, pX, pY + 200f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Back));

            engine.menuManager.openAsSubmenu(selectLvl);
        }
        public static void Menu_NewGame_Easy()
        {
            Profile.currentProfile.config.gameplay.level = gameplay.eDifficultyLevel.EASY;
            Menu_NewGame2();
        }
        public static void Menu_NewGame_Normal()
        {
            Profile.currentProfile.config.gameplay.level = gameplay.eDifficultyLevel.NORMAL;
            Menu_NewGame2();
        }
        public static void Menu_NewGame_Hard()
        {
            Profile.currentProfile.config.gameplay.level = gameplay.eDifficultyLevel.HARD;
            Menu_NewGame2();
        }
        public static void Menu_NewGame_Hardcore()
        {
            Profile.currentProfile.config.gameplay.level = gameplay.eDifficultyLevel.HARDCORE;
            Menu_NewGame2();
        }
        public static void Menu_NewGame2()
        {
            Menu selectMapCount = engine.menuManager.getMenuByName("MENU_NEWGAME_2");
            if (selectMapCount == null) selectMapCount = new Menu("MENU_NEWGAME_2", null);
            selectMapCount.clearMenuItems();

            float pX = 700f, pY = 180f;
            //CLose old shadow:
            Menu selectLvl = engine.menuManager.getMenuByName("MENU_NEWGAME_1");
            Menu_Item oldBG = selectLvl.getItemByName("BG");
            if (oldBG != null) selectLvl.removeMenuItem(oldBG);
            //then create new one:
            Obj BG = new Obj("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds", 0.52, 0.22, Obj.align.LEFT, true);
            selectMapCount.addItem(new Menu_Item("BG", BG, null, null, null, null, null));
            selectMapCount.addItem(new Menu_Item(Lang.cur.Ile_Poziomow_w_GrzeDK, pX, pY, Menu.font_Click, Menu.font_Click, Menu.font_Click));
            selectMapCount.addItem(new Menu_Item("10", pX, pY + 50f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Maps10));
            selectMapCount.addItem(new Menu_Item("20", pX, pY + 100f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Maps20));
            selectMapCount.addItem(new Menu_Item("30", pX, pY + 150f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Maps30));
            selectMapCount.addItem(new Menu_Item("40", pX, pY + 200f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_NewGame_Maps35));

            engine.menuManager.openAsSubSubmenu(selectMapCount);
        }
        public static void Menu_NewGame_Maps10()
        {
            Profile.currentProfile.config.gameplay.maps = 10;
            Menu_NewGame_Back();
            Menu_NewGame_StartGame();
        }
        public static void Menu_NewGame_Maps20()
        {
            Profile.currentProfile.config.gameplay.maps = 20;
            Menu_NewGame_Back();
            Menu_NewGame_StartGame();
        }
        public static void Menu_NewGame_Maps30()
        {
            Profile.currentProfile.config.gameplay.maps = 30;
            Menu_NewGame_Back();
            Menu_NewGame_StartGame();
        }
        public static void Menu_NewGame_Maps35()
        {
            Profile.currentProfile.config.gameplay.maps = 35;
            Menu_NewGame_Back();
            Menu_NewGame_StartGame();
        }
        public static void Menu_NewGame_Back()
        {
            engine.menuViewport.removeObjectByFilePath("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds");
            engine.menuManager.closeSubmenu(engine.menuManager.getMenuByName("MENU_NEWGAME_1"));
        }

        public static void Menu_NewGame_StartGame()
        {
            if (!((engine.gameState & Game.game_state.Game) == Game.game_state.Game))
            {
                engine.startGame();
            }

        }

        //------------------------------------
        //  L O A D   G A M E
        public static void Menu_LoadGame_Click()
        {

            Menu loadGame = engine.menuManager.getMenuByName("MENU_LOADGAME");
            if (loadGame == null) loadGame = new Menu("MENU_LOADGAME", null);
            loadGame.clearMenuItems();

            float pX = 700f, pY = 280f;
            Obj BG = new Obj("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds", 0.52, 0.22, Obj.align.LEFT, true);
            loadGame.addItem(new Menu_Item("BG", BG, null, null, null, null, null));
            String miscInfos = "";
            String miscInfos2 = "";
            QFont miscFont = TextManager.newQFont("CheriPL.ttf", 20, FontStyle.Regular, false, new Color4(80, 150, 230, 255));
            QFont miscFont2 = TextManager.newQFont("Rumpelstiltskin.ttf", 22, FontStyle.Regular, false,new Color4(80, 150, 230, 255));

            Savegame sav = Profile.currentProfile.save;
            miscInfos += Lang.cur.Tunel_NrDOT + (sav.maps.Count + 1) + "\n";
            if (sav.player.difficultLevel == 0)
                miscInfos += Lang.cur.Latwy;
            else if (sav.player.difficultLevel == 1)
                miscInfos += Lang.cur.Normalny;
            else if (sav.player.difficultLevel == 2)
                miscInfos += Lang.cur.Trudny;
            else if (sav.player.difficultLevel == 3)
                miscInfos += Lang.cur.Hardcore;
            else
                miscInfos += "???";
            miscInfos += "\n";
            miscInfos += Lang.cur.CzasDDSpace;
            uint sec = (sav.player.gameDuration % (60 * 1000)) / 1000;
            uint min = (sav.player.gameDuration % (3600 * 1000)) / (60 * 1000);
            uint hr =  (sav.player.gameDuration / (3600 * 1000));
            if (hr > 0) miscInfos += hr + ":";
            if (min > 0) miscInfos += min + ":";
            miscInfos += sec + "\n";
            miscInfos += "...\n";
            miscInfos += Lang.cur.PoziomDDSpace + sav.player.level + "\n";
            miscInfos += Lang.cur.PktDDSpace + sav.player.exp + "\n";
            miscInfos += Lang.cur.ZyciaDDSpace + sav.player.lives + "\n";
            miscInfos += Lang.cur.PktDTalentuDDSpace + sav.player.talentPts + "\n";

            loadGame.addItem(new Menu_Item(miscInfos, pX, pY, miscFont, miscFont, miscFont, null, null, Menu_Nothing));
            
            miscInfos2 = "(" + (sav.maps.Count * 100) / sav.player.mapsCount + "%" + ")";
            loadGame.addItem(new Menu_Item(miscInfos2, pX+ 150f, pY, miscFont2, miscFont2, miscFont2, null, null, Menu_Nothing));
            
            loadGame.addItem(new Menu_Item(Lang.cur.WczytajEXCL, pX + 250f, pY + 250f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_LoadGame_Load));
            loadGame.addItem(new Menu_Item(Lang.cur.Wroc, pX, pY + 250f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_LoadGame_Close));

            engine.menuManager.openAsSubmenu(loadGame);
        }
        public static void Menu_LoadGame_Load()
        {
            engine.menuViewport.removeObjectByFilePath("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds");
            Menu_Manager mgr = engine.menuManager;
            mgr.closeSubmenu(mgr.getMenuByName("MENU_LOADGAME"));
            engine.loadGame(Profile.currentProfile.save);
        }
        public static void Menu_LoadGame_Close()
        {
            engine.menuViewport.removeObjectByFilePath("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds");
            Menu_Manager mgr = engine.menuManager;
            mgr.closeSubmenu(mgr.getMenuByName("MENU_LOADGAME"));
        }
        //------------------------------------
        //  O P T I O N S
        public static void Menu_Options_Click()
        {
            
            Menu options = engine.menuManager.getMenuByName("MENU_OPTIONS");
            if (options == null) options = new Menu("MENU_OPTIONS", null);
            options.clearMenuItems();

            float pX = 700f, pY = 280f;
            Obj BG = new Obj("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds", 0.52, 0.22, Obj.align.LEFT, true);
            options.addItem(new Menu_Item("BG", BG, null, null, null, null, null));
            options.addItem(new Menu_Item(Lang.cur.Grafika, pX, pY, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_Options_GFX_Click));
            options.addItem(new Menu_Item(Lang.cur.Dzwiek, pX, pY + 50f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_Options_Sound_Click));
            options.addItem(new Menu_Item(Lang.cur.Wroc, pX, pY + 100f, Menu.font, Menu.font_Hover, Menu.font_Click, Menu_Options_Close));
           
            engine.menuManager.openAsSubmenu(options);
        }
        public static void Menu_Options_Sound_Click()
        {
            Menu_Manager mgr = engine.menuManager;
            Menu options_snd = mgr.getMenuByName("MENU_OPTIONS_SOUND");
            if (options_snd == null) options_snd = new Menu("MENU_OPTIONS_SOUND", null);
            //if this is already submenu, skip this operation!
            if (options_snd.Equals( engine.menuManager.subSubMenu)) return;
            options_snd.clearMenuItems();

            float pX = 700f, pY = 305f;
            //CLose old shadow:
            Menu options = engine.menuManager.getMenuByName("MENU_OPTIONS");
            Menu_Item oldBG = options.getItemByName("BG");
            if (oldBG != null) options.removeMenuItem(oldBG);
            //then create new one:
            Obj BG = new Obj("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds", 0.52, 0.22, Obj.align.LEFT, true);
            options_snd.addItem(new Menu_Item("BG", BG, null, null, null, null, null));

            
            options_snd.addItem(new Menu_Item(Lang.cur.Efekty+": ", pX, pY, Menu.fontSmall, Menu.fontSmall, Menu.fontSmall));
            options_snd.addItem(new Menu_Item(" - ", pX + 110f, pY, Menu.fontSmallAlt, Menu.fontSmallAlt_Hover, Menu.fontSmallAlt_Click, Menu_Options_Sound_SFX_Minus));
            options_snd.addItem(new Menu_Item("  +  ", pX + 165f, pY, Menu.fontSmallAlt, Menu.fontSmallAlt_Hover, Menu.fontSmallAlt_Click, Menu_Options_Sound_SFX_Add));
            options_snd.addItem(new Menu_Item(Math.Round(Profile.currentProfile.config.options.sound.sfxVol, 2).ToString(), pX + 140f, pY, Menu.fontSmall, null, null));

            pY += 30f;
            options_snd.addItem(new Menu_Item(Lang.cur.Muzyka+": ", pX, pY, Menu.fontSmall, Menu.fontSmall, Menu.fontSmall));
            options_snd.addItem(new Menu_Item(" - ", pX + 110f, pY, Menu.fontSmallAlt, Menu.fontSmallAlt_Hover, Menu.fontSmallAlt_Click, Menu_Options_Sound_Music_Minus));
            options_snd.addItem(new Menu_Item("  +  ", pX + 165f, pY, Menu.fontSmallAlt, Menu.fontSmallAlt_Hover, Menu.fontSmallAlt_Click, Menu_Options_Sound_Music_Add));
            options_snd.addItem(new Menu_Item(Math.Round(Profile.currentProfile.config.options.sound.musicVol, 2).ToString(), pX + 140f, pY, Menu.fontSmall, null, null));
 
            mgr.openAsSubSubmenu(options_snd);
            
        }
        public static void Menu_Options_Sound_SFX_Minus()
        {
            Configuration.prevDouble(ref Profile.currentProfile.config.options.sound.sfxVol);
            Menu options_snd = engine.menuManager.getMenuByName("MENU_OPTIONS_SOUND");
            options_snd.getItem(4).value = Math.Round(Profile.currentProfile.config.options.sound.sfxVol, 2).ToString();
            engine.SoundMan.recalculateSFX();
        }
        public static void Menu_Options_Sound_SFX_Add()
        {
            Configuration.nextDouble(ref Profile.currentProfile.config.options.sound.sfxVol);
            Menu options_snd = engine.menuManager.getMenuByName("MENU_OPTIONS_SOUND");
            options_snd.getItem(4).value = Math.Round(Profile.currentProfile.config.options.sound.sfxVol, 2).ToString();
            engine.SoundMan.recalculateSFX();
        }
        public static void Menu_Options_Sound_Music_Minus()
        {
            double hlp = Profile.currentProfile.config.options.sound.musicVol;
            Configuration.prevDouble(ref hlp);
            Profile.currentProfile.config.options.sound.musicVol = hlp;
            Menu options_snd = engine.menuManager.getMenuByName("MENU_OPTIONS_SOUND");
            options_snd.getItem(8).value = Math.Round(Profile.currentProfile.config.options.sound.musicVol, 2).ToString();
            engine.SoundMan.recalculateMusic();
        }
        public static void Menu_Options_Sound_Music_Add()
        {
            double hlp = Profile.currentProfile.config.options.sound.musicVol;
            Configuration.nextDouble(ref hlp);
            Profile.currentProfile.config.options.sound.musicVol = hlp;
            Menu options_snd = engine.menuManager.getMenuByName("MENU_OPTIONS_SOUND");
            options_snd.getItem(8).value = Math.Round(Profile.currentProfile.config.options.sound.musicVol, 2).ToString();
            engine.SoundMan.recalculateMusic();
        }
        public static void Menu_Options_GFX_Click()
        {
            Menu options_gfx = engine.menuManager.getMenuByName("MENU_OPTIONS_GFX");
            if (options_gfx == null) options_gfx = new Menu("MENU_OPTIONS_GFX", null);
            //if this is already submenu, skip this operation!
            //if (options_gfx.Equals(engine.menuManager.subSubMenu)) return;
            options_gfx.clearMenuItems(); 

            float pX = 700f, pY = 305f;

            //CLose old shadow:
            Menu options = engine.menuManager.getMenuByName("MENU_OPTIONS");
            Menu_Item oldBG = options.getItemByName("BG");
            if (oldBG != null) options.removeMenuItem(oldBG);
            //then create new one:
            Obj BG = new Obj("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds", 0.52, 0.22, Obj.align.LEFT, true);
            options_gfx.addItem(new Menu_Item("BG", BG, null, null, null, null, null));

            string resolutionStr = Profile.currentProfile.config.getResolutionString();
            options_gfx.addItem(new Menu_Item(Lang.cur.Rozdzieczlosc + ": " + resolutionStr, pX, pY, Menu.fontSmall, Menu.fontSmall_Hover, Menu.fontSmall_Click, Menu_Options_GFX_Resolution_Click));
            if (Profile.currentProfile.config.resolutionsAreTheSame(Profile.currentProfile.config.options.graphics.resIdx,Profile.currentProfile.config.options.graphics.newResolutionIdx))
            {
                options_gfx.addItem(new Menu_Item(Lang.cur.Ustaw_Rozdzielczosc, pX, pY + 30f, Menu.fontSmall_Disabled));
            }
            else
            {
                options_gfx.addItem(new Menu_Item(Lang.cur.Ustaw_Rozdzielczosc, pX, pY + 30f, Menu.fontSmall, Menu.fontSmall_Hover, Menu.fontSmall_Click, Menu_Options_GFX_ResolutionApply_Click));
            }
            options_gfx.addItem(new Menu_Item(Lang.cur.Renderuj_Sciezki + ": " + Configuration.boolToString(Profile.currentProfile.config.options.graphics.renderPaths), pX, pY + 60f, Menu.fontSmall, Menu.fontSmall_Hover, Menu.fontSmall_Click, Menu_Options_GFX_RPaths_Click));
          
            engine.menuManager.openAsSubSubmenu(options_gfx);
        }
        public static void Menu_Options_GFX_Resolution_Click()
        {
            Profile.currentProfile.config.getNextResolution();
            Menu_Options_GFX_Click();//rebuild whole menu
        }
        public static void Menu_Options_GFX_ResolutionApply_Click()
        {
            Profile.currentProfile.config.applyResolution();
        }
        public static void Menu_Options_GFX_RPaths_Click()
        {
            Profile.currentProfile.config.options.graphics.renderPaths = !Profile.currentProfile.config.options.graphics.renderPaths;
            engine.menuManager.getMenuByName("MENU_OPTIONS_GFX").getItem(2).value = Lang.cur.Renderuj_Sciezki+": " + Configuration.boolToString(Profile.currentProfile.config.options.graphics.renderPaths);
        }
        
        public static void Menu_Options_Close()
        {
            Profile.currentProfile.config.restoreResolutionValue();

            engine.menuViewport.removeObjectByFilePath("../data/Textures/MENU/MENU_SUBMENU_SHADOW.dds");
            Menu_Manager mgr = engine.menuManager;
            mgr.closeSubmenu(mgr.getMenuByName("MENU_OPTIONS"));
        }

        #region CREDITS
        static Timer CreditsTimer;
        static Timer FontsFadeIn, FontsFadeOut;
        static int creditsItemsAdded = 0;
        static QFont CreditsHead = TextManager.newQFont("Tepeno Sans Regular.ttf", 22f, false, new Color4(1, 1, 1, 1f));
        static QFont CreditsContent = TextManager.newQFont("Rumpelstiltskin.ttf", 24f, false, new Color4(0.1f, 0.37f, 0.56f, 1.0f));
        static Obj CreditsBG;
        static float creditsXMargin = 800f;//90%
        public static void Menu_Credits_Click()
        {
            Menu credits = engine.menuManager.getMenuByName("MENU_CREDITS");
            if (credits == null) credits = new Menu("MENU_CREDITS", null, Menu_Credits_Update, Menu_Credits_Render, Menu_Manager.cursor);
            credits.clearMenuItems();

            float pX = 700f, pY = 280f;engine.menuManager.current_menu = credits;
            engine.menuManager.canEnterIdleMode = false;
            CreditsTimer = new Timer(Timer.eUnits.MSEC, 0);
            CreditsTimer.start();
            creditsXMargin = 0.9f * engine.Width;
            //engine.videoPlayer.playVideo("../data/videos/outro.bik");

            CreditsBG = new Obj("../data/Textures/HLP_BLACK.dds", 0, 0, Obj.align.LEFT, true);
            CreditsBG.width = engine.Width * 3;
            CreditsBG.height = engine.Height * 3;
            CreditsBG.x -= CreditsBG.width / 3;
            CreditsBG.y -= CreditsBG.height / 3;
            CreditsBG.setCurrentTexAlpha(0);
            pX = creditsXMargin - Menu.fontSmall.Measure(Lang.cur.Wroc).Width;
            credits.addItem(new Menu_Item("BG", CreditsBG, null, null, null, null, null));
            credits.addItem(new Menu_Item(Lang.cur.Wroc, pX, pY + 400f, Menu.fontSmall, Menu.fontSmall_Hover, Menu.fontSmall_Click, Menu_Credits_Close));
            
        }
        private static List<Menu_Item> creditsMenuItmsOnScreen = new List<Menu_Item>();
        public static void Menu_Credits_Update()
        {
            float pX = 600f, pY = engine.Height/2;
            float ySpaces = 35;
            Menu_Manager mgr = engine.menuManager;
            Menu credits = mgr.current_menu; 
            if (CreditsTimer.currentTime > 53000)//last+5000(2s)
            {
                if (creditsItemsAdded == 9)
                {
                    creditsItemsAdded++;
                    pY -= 8f * ySpaces;
                    clearCreditsCaptions();
                    addCreditsCaption(Lang.cur.creditsFreesndUses, pY, CreditsHead); pY += ySpaces;
                    addCreditsCaption("weapGone.wav by RunnerPack"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Energy Whip 2.wav by ejfortin"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("tom.wav by josomebody"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("clik_final_processed.WAV by perlssdj"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("duppyBigBeat01_150bpm.wav by djduppy"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("MemoryMoon_shining-organ_grab-filtha.wav by suonho"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("MemoryMoon_pad-luminize.wav by suonho"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("03307 small whizzing swooshes.wav by Robinhood76"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Title_swell_02.wav by m_O_m"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("choir c.wav by ERH"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Cigarette Lighter + Sparkler.wav by aUREa"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("bombexplosion.wav by smcameron"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("MASSIVE PHASE CRASH 01.wav by sandyrb"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Slow 'whoop' bubble pop1.mp3 by CGEffex"  , pY, CreditsContent); pY += ySpaces; 
                    addCreditsCaption("Nightingale song 2.wav by reinsamba "  , pY, CreditsContent); pY += ySpaces; 
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 57000)//start+4000(this time)
                    FontsFadeOut.start();

                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else if (CreditsTimer.currentTime > 47000)//last+5000(2s)
            {
                if (creditsItemsAdded == 8)
                {
                    creditsItemsAdded++;
                    pY -= 8f * ySpaces;
                    clearCreditsCaptions();
                    addCreditsCaption(Lang.cur.creditsFreesndUses, pY, CreditsHead); pY += ySpaces;
                    addCreditsCaption("solemn opening 0O_10mi.WAV by Setuniman" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("revCrash.wav by mikobuntu" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Ultimate Subkick.wav by zilverton" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("swosh-22.flac by qubodup" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("SQUEAK2.WAV by propthis" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Click clack tick tock blip pop #2.wav by Timbre" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("blip10.flac by Corsica_S" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("CARTOON-BING-LOW.wav by kantouth" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("MASSIVE PHASE SWEEP 02.wav by sandyrb" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("shells-tinkle.WAV by BristolStories" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("klirr.wav by ehproductions" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("conxiadora.flac by galeku" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("wasserflasche15.wav by schluppipuppie" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("whoosh03.wav by FreqMan" , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("pock - 01.wav by schluppipuppie", pY, CreditsContent); pY += ySpaces;           
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 51000)//start+4000(this time)
                    FontsFadeOut.start();

                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else if (CreditsTimer.currentTime > 41000)//last+5000(2s)
            {
                if (creditsItemsAdded == 7)
                {
                    creditsItemsAdded++;
                    pY -= 8f * ySpaces;
                    clearCreditsCaptions();
                    addCreditsCaption(Lang.cur.creditsFreesndUses, pY, CreditsHead); pY += ySpaces;
                    addCreditsCaption("01878 high rooster.wav by Robinhood76"           , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("160bpm groove-.wav by djgriffin"                 , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("FrogsFLP.wav by daveincamas"                     , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Dun dun dun.wav by Simon_Lacelle"                , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("man_n_womangasp.wav by FGordon"                  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("popscreen test.wav by FreqMan"                   , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Inhale_10115a.wav by otherthings"                , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Yawning.aif by jackstrebor"                      , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("chimney fire.wav by reinsamba"                   , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("20060706.night.channel02.flac by dobroide"       , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("sword-01.wav by audione"                         , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Squelch Blood Stab .wav by Rock Savage"          , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Medium_Steel_Pipe_On_Concrete_1.wav by dheming"  , pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("Swosh Sword Swing by qubodup"                    , pY, CreditsContent); pY += ySpaces; 
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 45000)//start+4000(this time)
                    FontsFadeOut.start();

                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else if (CreditsTimer.currentTime > 30000)//last+4000
            {
                //Add item & fadeIn start
                if (creditsItemsAdded == 6)
                {
                    creditsItemsAdded++;
                    pY -= 1.5f * ySpaces;
                    clearCreditsCaptions();
                    addCreditsCaption(Lang.cur.creditsPodziekowania, pY, CreditsHead); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsPodziekowania_2, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsPodziekowania_3, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsPodziekowania_4, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsPodziekowania_5, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsPodziekowania_6, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsPodziekowania_7, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsPodziekowania_8, pY, CreditsContent); pY += ySpaces;
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 39000)//start+3000(this time)
                    FontsFadeOut.start();

                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else if (CreditsTimer.currentTime > 22000)//last+5000(2s)
            {
                if (creditsItemsAdded == 5)
                {
                    creditsItemsAdded++;
                    pY -= 7f * ySpaces;
                    clearCreditsCaptions();
                    addCreditsCaption(Lang.cur.creditsMusic, pY, CreditsHead); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsMusic_dubsective_1, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsMusic_dubsective_2, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsMusic_dubsective_3, pY, CreditsContent); pY += ySpaces*2;

                    addCreditsCaption(Lang.cur.creditsMusic_rezaloot_1, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsMusic_rezaloot_2, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsMusic_rezaloot_3, pY, CreditsContent); pY += ySpaces * 2;

                    addCreditsCaption(Lang.cur.creditsMusic_kavalsky_1, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsMusic_kavalsky_2, pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption(Lang.cur.creditsMusic_kavalsky_3, pY, CreditsContent); pY += ySpaces * 2;
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 28000)//start+4000(this time)
                    FontsFadeOut.start();

                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else if (CreditsTimer.currentTime > 17000)//last+4000
            {
                //Add item & fadeIn start
                if (creditsItemsAdded == 4)
                {
                    creditsItemsAdded++;
                    pY -= 1.5f * ySpaces;
                    clearCreditsCaptions();
                    addCreditsCaption(Lang.cur.creditsTesters, pY, CreditsHead); pY += ySpaces;
                    addCreditsCaption("Adam Ptaszek", pY, CreditsContent);       pY += ySpaces;
                    addCreditsCaption("Waldemar Laszczyk", pY, CreditsContent); pY += ySpaces;
                    addCreditsCaption("(Do uzupełnienia)", pY, CreditsContent); pY += ySpaces;
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 20000)//start+3000(this time)
                    FontsFadeOut.start();

                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else if (CreditsTimer.currentTime > 13000)//last+4000
            {
                //Add item & fadeIn start
                if (creditsItemsAdded == 3)
                {
                    creditsItemsAdded++;
                    pY -= 1 * ySpaces;
                    clearCreditsCaptions();
                    addCreditsCaption(Lang.cur.creditsMainGfxAni, pY, CreditsHead);
                    pY += ySpaces;
                    addCreditsCaption("Dariusz Kobuszewski", pY, CreditsContent);
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 15000)//start+2000
                    FontsFadeOut.start();

                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else if (CreditsTimer.currentTime > 9000)//last+4000
            {
                //Add item & fadeIn start
                if (creditsItemsAdded == 2)
                {
                    creditsItemsAdded++;
                    pY -= 1 * ySpaces;
                    //Remove old items:
                    clearCreditsCaptions();

                    addCreditsCaption(Lang.cur.creditsGlProgramista, pY, CreditsHead);
                    pY += ySpaces;
                    addCreditsCaption("Dariusz Kobuszewski", pY, CreditsContent);

                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 11000)//start+2000
                {//starts fade out
                    FontsFadeOut.start();
                }
                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else if (CreditsTimer.currentTime > 5000)//last+4000
            {
                //Add item & fadeIn start
                if (creditsItemsAdded == 1)
                {
                    creditsItemsAdded++;
                    pY -= 1 * ySpaces;
                    //Remove old items:
                    clearCreditsCaptions();
                    addCreditsCaption(Lang.cur.gra_opracowane_przez, pY, CreditsHead);
                    pY += ySpaces;
                    addCreditsCaption("Dariusz Kobuszewski", pY, CreditsContent);
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 7000)//start+2000
                {//starts fade out
                    FontsFadeOut.start();
                }
                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);         
            }
            else if (CreditsTimer.currentTime > 1000)
            {
                //Add item & fadeIn start
                if (creditsItemsAdded == 0)
                {
                    creditsItemsAdded++;
                    pY -= 1 * ySpaces;
                    engine.lightEngine.disabled = true;
                    CreditsBG = new Obj("../data/Textures/HLP_BLACK.dds", 0, 0, Obj.align.LEFT, true);
                    CreditsBG.width = engine.Width * 3;
                    CreditsBG.height = engine.Height * 3;
                    CreditsBG.x -= CreditsBG.width / 3;
                    CreditsBG.y -= CreditsBG.height / 3;
                    CreditsBG.setCurrentTexAlpha(255);
                    addCreditsCaption("COOKIE MONSTER PACMAN", pY, CreditsHead);
                    pY += ySpaces;
                    addCreditsCaption("Daro prodakszyn", pY, CreditsContent);
                    FontsFadeIn = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeOut = new Timer(Timer.eUnits.MSEC, 1000, 0, false, false);
                    FontsFadeIn.start();
                }
                else if (CreditsTimer.currentTime > 3000)
                {//starts fade out
                    FontsFadeOut.start();
                }
                CreditsHead.Options.Colour = new Color4(1, 1, 1, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
                CreditsContent.Options.Colour = new Color4(0.1f, 0.37f, 0.56f, FontsFadeIn.partDoneReverse - FontsFadeOut.partDoneReverse);
            }
            else
            {//Part: Blend in dark BG alpha:
                CreditsBG.setCurrentTexAlpha((byte)(CreditsTimer.currentTime/1000f*255));
            }
        }

        private static void clearCreditsCaptions()
        {
            Menu credits = engine.menuManager.current_menu;
            for (int i = 0; i < creditsMenuItmsOnScreen.Count; )
            {
                credits.removeMenuItem(creditsMenuItmsOnScreen[i]);
                creditsMenuItmsOnScreen.RemoveAt(i);
            }
        }
        private static void addCreditsCaption(string text,float pY,QFont fnt)
        {
          Menu credits = engine.menuManager.current_menu;
          float hlp = fnt.Measure(text).Width;
          //nasty bugfix(left align is wrong when last char is "i")
          if (text[text.Length - 1] == 'i')
              hlp -= 10;
          creditsMenuItmsOnScreen.Add(new Menu_Item(text, creditsXMargin - hlp, pY, fnt));
          credits.addItem(creditsMenuItmsOnScreen[creditsMenuItmsOnScreen.Count-1]);
        }
        public static void Menu_Credits_Render()
        {

        }
        public static void Menu_Credits_Close()
        {
            creditsItemsAdded = 0;
            CreditsTimer.stop();
            engine.menuManager.canEnterIdleMode = true;
            engine.menuManager.current_menu = engine.menuManager.getMenuByName("MENU_MAIN");
            engine.lightEngine.disabled = false;
        }
        #endregion
        public static void Menu_Exit_Click()
        {
            engine.menuManager.showConfirm(Lang.cur.areUSure, Menu_Exit_Click_Confirmed, engine.menuManager.closeConfirm);
        }
        public static void Menu_Exit_Click_Confirmed()
        {
            engine.Exit();
        }

        //---------------------------------
        // END OF MENU MAIN STUFF
        // --------------------------------

        // --------------------------------
        // S T A T U S   S C R E E N
        // --------------------------------
        public static void Status_OnLoad()
        {
            //generate all things there:
            //(render them at onRender func)
            //(store them in statusScreen class)
            engine.gameManager.statusScr.generateContents();
        }
        public static void Status_OnRender()
        {
            engine.gameManager.statusScr.renderContents();
        }
        public static void Status_ButtonOnHover()
        {
            Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_SHORT.ogg", false, false);
            beep.volume = 0.82;
            beep.Play();
        }
        public static void Status_AtrBoostClick()
        {
            if (engine.gameManager.PC.riseSpeed())
            {
                Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_LONG.ogg", false, false);
                beep.volume = 0.82;
                beep.Play();
                engine.gameManager.statusScr.generateContents();//refresh contents
            }
            else
            {
                Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_FAIL.ogg", false, false);
                beep.volume = 0.82;
                beep.Play();
            }
        }
        public static void Status_AtrMaxLivesClick()
        {
            if (engine.gameManager.PC.riseMaxLives())
            {
                Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_LONG.ogg", false, false);
                beep.volume = 0.82;
                beep.Play();
                engine.gameManager.statusScr.generateContents();//refresh contents
            }
            else
            {
                Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_FAIL.ogg", false, false);
                beep.volume = 0.82;
                beep.Play();
            }
        }
        public static void Status_SkillBoost()
        {
            if (engine.gameManager.PC.riseSkill(Skill.skillNames.Boost))
            {
                Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_LONG.ogg", false, false);
                beep.volume = 0.82;
                beep.Play();
                engine.gameManager.statusScr.generateContents();//refresh contents
            }
            else
            {
                Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_FAIL.ogg", false, false);
                beep.volume = 0.82;
                beep.Play();
            }
        }
        public static void Status_SkillIceBolt()
        {
            if (engine.gameManager.PC.riseSkill(Skill.skillNames.IceBolt))
            {
                Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_LONG.ogg", false, false);
                beep.volume = 0.82;
                beep.Play();
                engine.gameManager.statusScr.generateContents();//refresh contents
            }
            else
            {
                Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_FAIL.ogg", false, false);
                beep.volume = 0.82;
                beep.Play();
            }
        }
        
        public static void Status_exitToMenuClick()
        {
            Menu_Manager mgr = engine.menuManager;
            mgr.showConfirm(Lang.cur.endGameSessionAllChangesWillBeLost, Status_exitToMenuClick_Yes, mgr.closeConfirm);
        }

        public static void Status_exitToMenuClick_Yes()
        {
            Menu_Manager mgr = engine.menuManager;
            mgr.closeConfirm();
            Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_LONG.ogg", false, false);
            beep.volume = 0.82;
            beep.Play();
            engine.closeGameManagerSession();
            mgr.setCurrentMenu(mgr.getMenuByName("MENU_MAIN"));
        }
    }
}