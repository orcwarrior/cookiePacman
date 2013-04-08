using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using EngineApp;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace CookieMonster.CookieMonster_Objects
{

    /// <summary>
    /// Game Manager is the main class that is responsible for game session
    /// It Manges root parts of game logic, and have control over other isolated parts of game
    /// logic (gameMap,musicPlayer, etc.).
    /// </summary>
    class GameManager : engineReference
    {
        static public Random variantizer = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        static private int _gridSize;
        static public double initPCSpeed = 60f;

        public Player PC { get { return _pc; } }
        public List<Enemy> enemiesList { get; private set; }
        public List<Enemy> sortedEnemiesList { get; private set; }//recreated on every gameMgr update
        public List<Projectile> projectilesList { get; private set; }
        public List<PowerUp> sortedPowerUpList { get { return _sortedPowerUpList; } }
        public statusScreen statusScr { get; private set; } 
        public int level { get; private set; }
        public bool gamePaused { get; private set; }
        public Viewport activeView { get { return engine.activeViewport; } }
        public GameMap Map { get { return _Map; } }
        public CollisionSystem collisionSystem { get { return collSys; } }
        public Timer gameDuration { get; private set; }
        public Timer levelDuration { get; private set; }
        public Timer powerPillTimer { get; private set; }
       
        private GameMap _Map;
        private CollisionSystem collSys; 
        private MusicPlayer mPlayer;
        private GUI gui;
        private Statistics stats; public Statistics statistics { get { return stats; } }
        private Player _pc;
        private List<Bomb> _plantedBombs; 
        private List<PowerUp> _sortedPowerUpList; 
        private Timer failureTimer = new Timer(Timer.eUnits.MSEC, 1 * 1000, 0, true, false);
        private Timer slowdownTimer = new Timer(Timer.eUnits.MSEC, 30 * 1000, 0, true, false);
        private bool slowdownActive;

        public static int gridSize
        {
            get { return _gridSize; }
        }
        //returns true when player can proceed to next level;
        //TODO: Back to normal (commented)
        //public bool canStartNextLevel { get { return statistics.lvlPoints >= Map.cookiesCount * Statistics.ptsPerCookie; } }
        public bool canStartNextLevel { get { return true; } }
        public GameManager()
        {
            engine.gameState = Game.game_state.Game;

            initLoadScreen();
            updateLoadingInfos("..."+Lang.cur.Inicjuje_Game_Managera+"...");
            //init other clases static fields:
            PowerUp.GameMan = this;
            MOB.GameMan = this;
            _gridSize = 48;//in pixels
            level = 1;
            enemiesList = new List<Enemy>();
            sortedEnemiesList = new List<Enemy>();
            _sortedPowerUpList = new List<PowerUp>();
            _plantedBombs = new List<Bomb>();
            projectilesList = new List<Projectile>();
            //collision system init:
            updateLoadingInfos("..."+Lang.cur.Inicjuje_system_kolizji+"...");
            collSys = new CollisionSystem(this);
            CollisionReport.collSystem = collSys;
            powerPillTimer = new Timer(Timer.eUnits.MSEC,1000*15,0,true,false);

            updateLoadingInfos("..."+Lang.cur.wczytuje_poziom+"...");
            //InitGameMap:
            initGameMap();

            //create gui:
            updateLoadingInfos("..."+Lang.cur.tworze_GUI+"...");
            gui = new GUI();
            //music player init
            statusScr = new statusScreen();
            updateLoadingInfos("..."+Lang.cur.inicjuje_odtwarzacz_muzyki+"...");
            mPlayer = new MusicPlayer();

            //create statistics:
            stats = new Statistics();

            updateLoadingInfos("..."+Lang.cur.inicjalizuje_obiekty_poziomu+"...");
            initializeMOBs();
            updateLoadingInfos("..."+Lang.cur.alokuje_pamiec_tekstur+"...");
            Projectile.projectilesVisualsInit();
            InitTeleportFX();
            startGameDurationTimer();
            startNewLevelTimer();
        }

        /// <summary>
        /// create game manager based on passed savegame
        /// </summary>
        /// <param name="sav"></param>

        public GameManager(Savegame sav)
        {
            initLoadScreen();
            updateLoadingInfos("..." + Lang.cur.Inicjuje_Game_Managera + "...");
            //init other clases static fields:
            PowerUp.GameMan = this;
            MOB.GameMan = this;
            _gridSize = 48;//in pixels
            level = (int)sav.maps[sav.maps.Count-1].level+1;//we starting from next map from last-saved one
            enemiesList = new List<Enemy>();
            sortedEnemiesList = new List<Enemy>();
            _sortedPowerUpList = new List<PowerUp>();
            _plantedBombs = new List<Bomb>();
            projectilesList = new List<Projectile>();
            //collision system init:
            updateLoadingInfos("..." + Lang.cur.Inicjuje_system_kolizji + "...");
            collSys = new CollisionSystem(this);
            CollisionReport.collSystem = collSys;
            powerPillTimer = new Timer(Timer.eUnits.MSEC, 1000 * 15, 0, true, false);

            updateLoadingInfos("..." + Lang.cur.wczytuje_poziom + "...");
            //InitGameMap:
            initGameMap();//after this op. PC will be created
            //We need to preinit PC:
            PC.restoreFromSave(sav);

            //create gui:
            updateLoadingInfos("..." + Lang.cur.tworze_GUI + "...");
            gui = new GUI();
            //music player init
            statusScr = new statusScreen();
            updateLoadingInfos("..." + Lang.cur.inicjuje_odtwarzacz_muzyki + "...");
            mPlayer = new MusicPlayer();

            //create statistics:
            stats = new Statistics(sav);
            
            updateLoadingInfos("..." + Lang.cur.inicjalizuje_obiekty_poziomu + "...");
            initializeMOBs();
            updateLoadingInfos("..." + Lang.cur.alokuje_pamiec_tekstur + "...");
            Projectile.projectilesVisualsInit();
            InitTeleportFX();

            startGameDurationTimer();
            //todo set current time to old time
            gameDuration.currentTime = sav.player.gameDuration;
            startNewLevelTimer();
        }

        public void nextLevel()
        {
            // bugfix: reinit of projectile visuals is needed
            // cause sometimes problems with visals scale occurs.
            Projectile.forceProjectileVisualsReinit();

            pauseGameDurationTimer();
            //update saveGame
            Profile.currentProfile.autoSave();

            updateLoadingInfos("...dealokuje pamiec...");
            powerPillTimer.restart();
            powerPillTimer.stop();
            enemiesList = new List<Enemy>();
            sortedEnemiesList = new List<Enemy>();
            _sortedPowerUpList = new List<PowerUp>();
            _plantedBombs = new List<Bomb>();
            projectilesList = new List<Projectile>();
            
            level++;

            updateLoadingInfos("...wczytuje poziom...");
            initGameMap();

            updateLoadingInfos("...inicjalizuje obiekty poziomu...");
            initializeMOBs();
            PC.renewSkills();
            engine.gameCamera.correctForNewLevel();

            //start/continue timers:
            continueGameDurationTimer();
            startNewLevelTimer();

        }

        public void InitPC(int x, int y)
        {
            if (PC == null)//pc still not created:
            {
                Obj oPCdef = new Obj("../data/Textures/GAME/MOB/cookie_move_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                Obj oPCidle = new Obj("../data/Textures/GAME/MOB/cookie_idle_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                Obj oPCeat = new Obj("../data/Textures/GAME/MOB/cookie_attack_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                Obj oPCfrozen = new Obj("../data/Textures/GAME/MOB/cookie_frozen_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                Obj oPCdust = new Obj("../data/Textures/GAME/MOB/cookie_dust_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                oPCdef.setTexAniFPS(12);
                oPCidle.setTexAniFPS(20);
                oPCeat.setTexAniFPS(18);
                oPCeat.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                oPCidle.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                oPCdust.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                _pc = new Player(x, y, initPCSpeed);//60
                _pc.setStateVisual(oPCdef, "DEFAULT");
                _pc.setStateVisual(oPCidle, "IDLE");
                _pc.setStateVisual(oPCeat, "ATTACK");
                _pc.setStateVisual(oPCfrozen, "FROZEN");
                _pc.setStateVisual(oPCdust, "DUST");
            }
            else
            {   //only move hero to new starting position:
                PC.Move(x - PC.pX, y - PC.pY, false);
            };
            
            //mobList.Add(_pc);
        }

        private bool afterFirstUpdate;
        public void Update()
        {
            bool pauseGame;//for later pausing-unpauseing of the game
            // Some very ugly bugfix:
            if (!afterFirstUpdate)
            {
                afterFirstUpdate = true;
                firstUpdate();
            }
            // tipWindow(s) update:
            pauseGame = tipsManager.Update();
            //update StatusScreen(this needed only when he's easing-out
            statusScr.Update();
            if (!gamePaused)
            { // Update of objects placed on map, only when game isn't paused.
                slowDownUpdate();
                powerPillUpdate();
                //update collisions:
                collSys.Update();
                //update music player:
                mPlayer.Update();
                //re-sort MOB list: & update them
                updateMOBs();

                //Updates powerUPs:
                for (int i = 0; i < sortedPowerUpList.Count; i++)
                    sortedPowerUpList[i].Update();

                //Updates planted bombs: (change keyframes checking if they're ready to explode
                for (int i = 0; i < _plantedBombs.Count; i++)
                    _plantedBombs[i].Update();

                //update Projectiles:
                for (int i = 0; i < projectilesList.Count; i++)
                    projectilesList[i].Update();
            }

            //If tipsManager decided to pause game, now it will take effect:
            //BUGFIX: if statuscreen is active, don't care about tipManager statement about activePause or not
            if (statusScr.active) return;
            gamePaused = pauseGame;
        }

        public void prepareRender()
        {//handles all object render during Game
            // FIX: We will make sure we passing object 
            // to game Viewport not menu! [BUGFIX]
            Game.game_state oldState = engine.gameState;
            engine.gameState = Game.game_state.Game;

            if (_Map != null)
            {
                _Map.prepareStaticRender();
                _Map.prepareRender();
            }
            //renders bomb(overlaying all other objects:)
            for (int i = 0; i < _plantedBombs.Count; i++)
                _plantedBombs[i].prepareRender();

            //render Projectiles:
            for (int i = 0; i < projectilesList.Count; i++)
                projectilesList[i].prepareRender();

            // Render lightmaps:
            engine.lightEngine.Render();

            //music player render:
            mPlayer.prepareRender();

            //gui render:
            gui.prepareRender();
            statusScr.prepareRender();
            tipsManager.prepareRender();

            engine.gameState = oldState;
        }

        internal void KeyboardEvt(object sender, OpenTK.Input.KeyboardKeyEventArgs k)
        {
            if (_pc != null)
                _pc.KeyboardEvt(sender, k);
            if( k.Key == OpenTK.Input.Key.Escape)
                    ESCClicked(); 
        }
        /// <summary>
        /// ads to list of power ups, in sorted order
        /// sort by Y then X position
        /// </summary>
        /// <param name="p"></param>
        public void addPowerUP(PowerUp p)
        {
            sortedPowerUpList.Add(p);
        }
        public void removePowerUp(PowerUp p)
        {
            sortedPowerUpList.Remove(p);
        }


        #region loading_screen
        private Text loadScreenTxtMsg;
        private Obj loadScreenBG;
        private Obj loadScreenAni;
        private void updateLoadingInfos(string msg)
        {
            float xCenter = (float)engine.Width / 2f;
            float yCenter = (float)engine.Height / 2f;

            float xMinus = TextManager.font_default_20.Measure(msg).Width / 2;
            loadScreenTxtMsg = new Text(TextManager.font_default_20, xCenter - xMinus, yCenter, msg);
            __renderLoadScreen();
            //engine.textManager.onRender();         
        }

        private void initLoadScreen()
        {
            //FIX: remove all previous texts
            engine.textManager.clearAll();
            loadScreenBG = new Obj("../data/Textures/MENU/LOADING_BG.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, false);
            loadScreenAni = new Obj("../data/Textures/MENU/LOADING_ANI_A0.dds", 0.5, 0.42, Obj.align.CENTER_BOTH, false);
            loadScreenBG.isGUIObjectButUnscaled = true;
            loadScreenAni.isGUIObjectButUnscaled = true;
        }
        private void __renderLoadScreen()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            loadScreenBG.Render();  // Using render there is required because there is
            loadScreenAni.Render(); // no active viewport and it's not rendered right now!!!
            if (loadScreenTxtMsg != null)
            {
                loadScreenTxtMsg.Update();
                loadScreenTxtMsg.Render();
            }
            engine.SwapBuffers();
        }
        #endregion
        private void startGameDurationTimer()
        {
            gameDuration = new Timer(Timer.eUnits.MSEC, -1);
            gameDuration.start();
        }
        private void pauseGameDurationTimer()
        {
            gameDuration.stop();
        }
        private void continueGameDurationTimer()
        {
            gameDuration.start();
        }
        private void startNewLevelTimer()
        {
            //Start new level timer:
            levelDuration = new Timer(Timer.eUnits.MSEC, -1);
            levelDuration.start();
        }
        private void initializeMOBs()
        {
            PC.Initialize();
            for (int i = 0; i < enemiesList.Count; i++)
                enemiesList[i].Initialize();
        }
        private void initGameMap()
        {
            engine.gameCamera.resetPos();
            String mapFile = "LEVEL_" + level.ToString() + ".pn" + "g";//workaround to force it always as dds
            mapFile = GameMap.MapsPath + mapFile;
            _Map = new GameMap(mapFile, this);
        }

        private void firstUpdate()
        {
            //Chceck if there is new tip to show:
            tipsManager.newLevel(level);
            engine.InitCamera();
        }

        private void slowDownUpdate()
        {
            if (slowdownActive && slowdownTimer.enabled == false)
            {
                slowdownActive = false;
                Profile.currentProfile.config.options.sound.musicVol *= 10.0;
                engine.SoundMan.recalculateMusic();
                for (int i = 0; i < enemiesList.Count; i++)
                {
                    enemiesList[i].resetMovementSpeed();
                }
            }
        }

        private void updateMOBs()
        {
            sortedEnemiesList.Clear();
            _pc.Update();
            for (int i = 0; i < enemiesList.Count; i++)
            {
                enemiesList[i].Update();//update mob
                int j = 0;
                for (; j < sortedEnemiesList.Count; j++)
                {
                    if (enemiesList[i].pY < sortedEnemiesList[j].pY)
                        break;
                    else if (enemiesList[i].pX < sortedEnemiesList[j].pX)
                        break;
                }
                sortedEnemiesList.Insert(j, enemiesList[i]);
                j++;
            }
        }

        //Teleport Stuff:
        private Obj _teleportFX; public Obj teleportFX{get {return _teleportFX;}}
        private void InitTeleportFX()
        {
            _teleportFX = new Obj("../data/Textures/Game/FX/TELEPORT_A0.dds", 128, 128, Obj.align.CENTER_BOTH);
            _teleportFX.setTexAniFPS(30);
            _teleportFX.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
        }
        public void ReinitTeleportFX(int px, int py)
        {
            _teleportFX.x = px; _teleportFX.y = py;
            _teleportFX.restartTexAni();
        }

        internal void addBomb(Bomb bomb)
        {
            _plantedBombs.Add(bomb);
        }
        internal void removeBomb(Bomb bomb)
        {
            _plantedBombs.Remove(bomb);
        }
        
        internal void bombExplode(Bomb bomb)
        {
            int y = bomb.gridY;
            for (int x = bomb.gridX - 1; x <= bomb.gridX + 1; x++)
            {
                if (x == bomb.gridX)
                {
                    for (y = bomb.gridY - 1; y <= bomb.gridY + 1; y++)
                    {
                        Map.tryDestroyObject(x, y);
                    }
                    y = bomb.gridY;
                }
                else
                {
                    Map.tryDestroyObject(x, y);
                }
            }
            Map.addBombDecay(bomb.pX, bomb.pY);
        }

        internal void heroHasNewWP()
        {
            for (int i = 0; i < enemiesList.Count; i++)
            {
                if(enemiesList[i].enemyState == Enemy.eEnemyState.NORMAL)
                enemiesList[i].recalculateShortestWay();
            }
        }

        private void powerPillUpdate()
        {
            if (!powerPillActive())
            {
                for (int i = 0; i < enemiesList.Count; i++)
                {
                    if (enemiesList[i].enemyState == Enemy.eEnemyState.FLEE)
                        enemiesList[i].enemyState = Enemy.eEnemyState.NORMAL;
                }
            }
        }

        public bool powerPillActive()
        {
            return powerPillTimer.enabled;
        }
        internal void PlayerTakePowerPill()
        {
            powerPillTimer.restart();
            for (int i = 0; i < sortedEnemiesList.Count; i++)
            {
                sortedEnemiesList[i].enemyState = Enemy.eEnemyState.FLEE;
            }
        }

        internal void slowEnemies()
        {
            Sound slowdown = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_TIMESLOW.ogg", false, false);
            slowdown.volume = 0.8;
            //Mute music when slowdown is active:
            Profile.currentProfile.config.options.sound.musicVol *= 0.1;
            engine.SoundMan.recalculateMusic();
            slowdown.Play();
            slowdownTimer.start();
            slowdownActive = true;
            for (int i = 0; i < enemiesList.Count; i++)
            {
                enemiesList[i].setSlow();
            }

        }
        /// <summary>
        /// Get's an grid pos of passed Object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Point getObjGridPos(Obj obj)
        {
            Point ret = new Point((obj.x + obj.width / 2) / GameManager.gridSize,
                                  (obj.y + obj.height / 2) / GameManager.gridSize);
            return ret;
        }

        public void playFailSound()
        {
            if (!failureTimer.enabled)
            {
                failureTimer.start();
                Sound failure = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_DISABLED.ogg", false, true, 0.8);
            }
        }

        public void Pause()
        {
            gamePaused = true;
        }
        public void UnPause()
        {
            gamePaused = false;
        }
        public void toggleIngameMenu()
        {
            Pause();
            statusScr.Show();
        }
        public void untoggleIngameMenu()
        {
            UnPause();
            statusScr.Hide();
        }

        internal void ESCClicked()
        {
           if(!statusScr.active)
           {
               toggleIngameMenu();
           }
           else
           {
               untoggleIngameMenu();
           }
        }
        public void Free()
        {
            mPlayer.Free();
        }

        internal void GameOver()
        {
            highscoresManager.addNewHighscore(this);
            //throw new NotImplementedException();
        }
    }
}
