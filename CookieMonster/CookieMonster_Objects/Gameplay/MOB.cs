using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// MOB... Moveable OBject
    /// </summary>
    class MOB
    {
        //Enumerators
        public enum eMOBType { MOB, POWERUP, ENEMY, PLAYER, PROJECTILE }
        public enum eState   { DEFAULT, // Default MOB state
                                  IDLE, // MOB is idle, ex. Player hit wall, waiting for new direction
                                ATTACK, // MOB is an enemy and collided with player, started ATTACK ani
                            ATTACK_END, // Just when ATTACK ani ends MOB will set state to ATTACK_END
                                        // if hero is still in collision with MOB, hero will be Kill()ed
                                  HURT, // Hurted by player, that x_x face ;)
                                FROZEN, // MOB frozen by ice bolt
                                DUST }; // MOB was 'touched' by Lazer beam
        public enum eDir     { L, R, U, D, UNDEF };
        
        #region fields
        static public GameManager GameMan;

        private eMOBType _mobType = eMOBType.MOB;
        // movement stuff:
        public eDir dirBuffer;//keystorke is first inserted into buffer, when mob is on ideal grid pos then dir is set to dirBuffer
        private eDir dir;
        public bool mobMirrored { get; private set; }
        
        // speed of movement:
        private Timer moveGap;//in ms
        public int   pxMove {get; private set;}
        public double baseSpeed { get; private set; }//value will be set when object being constructed 
        private double _speed;
        
        private int posX,posY;//in pixels, on screen

        // collision stuff:
        private Rectangle bbox;//bounding box of MOB -> for intersecting with map collision
        private CollisionReport _myCollision; 

        //Waynet stuff:
        public Waypoint currentWP { get; private set; } // it's not null only when enemy is EXACTLY on wp pos
        public Waypoint lastWP { get; private set; } // last currentWP
        public Waypoint nextWP { get; private set; } // wp readed from lastWP.Way(in direciton of MOB is heading)
        public Waypoint spawnWP { get; private set; } //WP which in MOB was spawned
        // Visuals:
        private List<Obj> visuals = new List<Obj>();//list of created visuals
        private Obj visualDefault;//moving
        private Obj visualIdle; //stand?(it's even possible?) or blocked by obstacles
        private Obj visualAttack;//near player(enemy) or about to eat cookie (player)
        private Obj visualHurt;// hmmm...
        private Obj visualFrozen;
        private Obj visualDust;

        private eState _state;
        public bool isOnGrid { get; private set; }
        public bool invincible { get; private set; }
        private Timer freezeTimer = new Timer(Timer.eUnits.MSEC, (10 * 1000), 0, true, false);
        private Timer dustTimer   = new Timer(Timer.eUnits.MSEC, (5 * 1000), 0, true, false);
        private Timer shieldTimer = new Timer(Timer.eUnits.MSEC, (5 * 1000), 0, true, false);

        private Obj iceCubeVisual = new Obj("../data/Textures/GAME/SOB/icecube.dds", 0, 0, Obj.align.CENTER_BOTH);
        private Obj shieldVisual = new Obj("../data/Textures/GAME/FX/FX_SHIELD_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
        private List<Light> attachedLights;

        private bool lastCollsionStatic;
        private PowerUp takenPowerUP; private int cookieEatenHoldFrames;
        private bool isIgnitOfCurrentTeleportFX;
        #endregion

        #region properties
        public double speed
        {//in pixels per second:
            // finding proper value by:
            // 1 timer dur: 20-50(ms) per "move" frame
            // 2 px per "move" frame: 1..n
            get { return _speed; }
            set
            {
                _speed = value;
                double px_Move = pxMove;
                if (value == 0.0) { px_Move = 0; moveGap = new Timer(Timer.eUnits.MSEC, 100000, 0, false, false); return; }
                double ms_min = 20.0; double ms_max = 1000.00;
                for (px_Move = 1; px_Move * (1000.0 / 50.0) <= value; px_Move++) ;
                //pxMove--;//pxMove founded, now timer interlace
                double step = ms_max - ms_min;
                step /= 2.0;
                while (step > 1.0)
                {
                    if (1000.0 / (ms_min + step) / px_Move < value)
                        ms_max -= step;
                    else
                        ms_min += step;
                    step /= 2.0;
                }
                if (moveGap != null) moveGap.Dispose();
                moveGap = new Timer(Timer.eUnits.MSEC, (int)ms_max, 0, true, false);
                moveGap.start();
                pxMove = (int)px_Move;
            }
        }
        public CollisionReport myCollision
        {
            get
            {
                return _myCollision;
            }
            set
            {
                if (_myCollision == null)//it can be set only once
                    _myCollision = value;
            }
        }
        public Obj currentVisual
        {
            get
            {
                Obj ret = visualDefault;
                if ((state == eState.IDLE) && (visualIdle != null))
                    ret = visualIdle;
                else if ((state == eState.ATTACK) && (visualAttack != null))
                    ret = visualAttack;
                else if ((state == eState.HURT) && (visualHurt != null))
                    ret = visualHurt;
                else if ((state == eState.FROZEN) && (visualFrozen != null))
                    ret = visualFrozen;
                else if ((state == eState.DUST) && (visualDust != null))
                    ret = visualDust;
                return ret;
            }
        }
        public virtual int pX
        {
            get
            {
                if (!mobMirrored)
                    return posX;
                else
                return posX + currentVisual.width;
            }
            private set
            {
                if (!mobMirrored)
                    posX = value;
                else
                    posX = value - currentVisual.width;
            }
        }
        public virtual int pY
        {
            get { return posY; }
        }
        /// <summary>
        /// returns on grid X position of MOB
        /// </summary>
        public int gridX
        {
            get
            {
                if (direction == eDir.R)
                    return pX / GameManager.gridSize;
                else
                    return (pX) / GameManager.gridSize;
                    // return (pX + GameManager.gridSize/2+1) / GameManager.gridSize;
            }
        }
        /// <summary>
        /// returns on grid Y position of MOB
        /// </summary>
        public int gridY
        {
            get 
            {
                if (direction == eDir.U)
                    return (pY) / GameManager.gridSize;
                else
                    return (pY) / GameManager.gridSize;
            }
        }
        /// <summary>
        /// return's next predicted grid X pos
        /// </summary>
        public virtual int nextGridX
        {
            get
            {
                int grid = GameManager.gridSize;
                
                if (dir == eDir.R)
                    return (pX + pxMove + GameManager.gridSize) / GameManager.gridSize;
                else if (dir == eDir.L)
                    return (pX - pxMove) / GameManager.gridSize;
                else
                    return gridX;
            }
        }
        /// <summary>
        /// return's next predicted grid Y pos
        /// </summary>
        public virtual int nextGridY
        {
            get
            {
                if (dir == eDir.U)
                    return (pY - pxMove) / GameManager.gridSize;
                else if (dir == eDir.D)
                    return (pY + pxMove + GameManager.gridSize) / GameManager.gridSize;
                else
                    return gridY;
            }
        }

        public bool isFrozen { get { return state == eState.FROZEN; } }
        public eState state { set { _state = value; currentVisual.setTexAniFrame(0); } get { return _state; } }
        //TODO: temporary stuff, remove when not needed
        public int lastWPid { get { if (lastWP != null)return lastWP._id; else return -1; } }
        public int nxtWPid { get { if (nextWP != null)return nextWP._id; else return -1; } }

        public Rectangle boundingBox { get { return bbox; } }
        public eDir direction
        {
            set
            {
                if (dir == value) return; //same key as previous? do nothing!
                //if ((((value == eDir.L) || (value == eDir.D)) && ((dir == eDir.R) || (dir == eDir.R)))
                //|| (((dir == eDir.L) || (dir == eDir.D)) && ((value == eDir.R) || (value == eDir.R))))
                if (((value == eDir.L) && (!mobMirrored)) || ((value == eDir.R) && (mobMirrored)))
                {
                    mobMirrored = !mobMirrored;
                    if (visualDefault != null)
                    {
                        for (int i = 0; i < visuals.Count; i++)
                        {
                            visuals[i].width *= -1;
                        }
                        posX -= currentVisual.width;//(int)(visualDefault.width * 4.8);

                    }
                }
                if ((movingHorizontal && ((value == eDir.L) || (value == eDir.R)))
                || (movingVertical && ((value == eDir.U) || (value == eDir.D))))
                {
                    Waypoint wp = nextWP;
                    nextWP = lastWP;
                    lastWP = wp;
                    if ((wp != null) && (lastWP == null)) lastWP = wp.getWPAtDirection(inverseDirection(value));
                }
                dir = value;
            }
            get { return dir; }
        }
        public eMOBType mobType { get { return _mobType; } set { if (_mobType == eMOBType.MOB)_mobType = value; } }
        public bool movingHorizontal { get { return (dir == eDir.L) || (dir == eDir.R); } }
        public bool movingVertical { get { return !movingHorizontal; } }
       
        #endregion


        public MOB(int posx, int posy, double spd)
        {
            mobMirrored = false;
            direction = eDir.R;//by default
            posX = posx; posY = posy;
            pxMove = 1;
            baseSpeed = speed = spd;
            mobType = eMOBType.MOB;//value will be overwritten if object only inherits from MOB
            
        }
        /// <summary>
        /// this method should be called when all objects creations ended
        /// especially waynet is fully generated cause it sets last/nextWP proper values
        /// </summary>
        public void Initialize()
        {
            if (!GameMan.Map.waynetActive) return;
            spawnWP = GameMan.Map.wayNetwork.getWPAt((uint)gridX, (uint)gridY);
            lastWP = currentWP = GameMan.Map.wayNetwork.getWPAt((uint)gridX, (uint)gridY);
            if (lastWP != null)
            {
                nextWP = lastWP.getWPAtDirection(direction);
                if (nextWP == null) nextWP = lastWP;
            }

        }
        /// <summary>
        /// handled states:
        /// "DEFAULT" "IDLE" "ATTACK" "RUNAWAY" (new 21:50 2012-08-12)"FROZEN"
        /// (case insensitive)
        /// </summary>
        /// <param name="vis"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public void setStateVisual(Obj vis, string type)
        {
            string typ = type.ToUpper();
            visuals.Add(vis);
            switch (typ)
            {
                case "IDLE":
                    visualIdle = vis; break;
                case "ATTACK":
                    visualAttack = vis; break;
                case "HURT":
                    visualHurt = vis; break;
                case "FROZEN":
                    visualFrozen = vis; break;
                case "DUST":
                    visualDust = vis; break;
                default:
                    visualDefault = vis; break;
            }
        }

        internal void Render()
        {
           currentVisual.x = posX; currentVisual.y = posY;
           currentVisual.prepareRender();

            //Freeze FX:
           if (state == eState.FROZEN)
           {
               iceCubeVisual.x = pX; iceCubeVisual.y = pY;
               iceCubeVisual.prepareRender();
           }

            //Teleport FX:
           if (isIgnitOfCurrentTeleportFX)
               if (!GameMan.teleportFX.texAniFinished())
                   GameMan.teleportFX.prepareRender();
               else
                   isIgnitOfCurrentTeleportFX = false;

            //Shield FX:
           if (shieldTimer.enabled)
           {
               shieldVisual.x = pX - 32;
               shieldVisual.y = pY - 32;
               int alpha = (int)(255 * (shieldTimer.partDone / 0.2f));
               new DebugMsg("new shield-alpha: " + alpha + " ("+shieldTimer.partDone+")");
               if(alpha>255)alpha=255;
               shieldVisual.setCurrentTexAlpha((byte)alpha);
               shieldVisual.prepareRender();
           }
           else if (invincible == true)
               invincible = false;//shield ended, set invincible to false.

            //Cartoon "BAM" FX:
           if (fxComicBoom != null)
           {
               if (fxComicBoom.objAnimation.isFinished) fxComicBoom = null;
               else 
               fxComicBoom.prepareRender();
           }
        }
        public void Update()
        {            
            //for cookies eating
            updateTakenPowerUP();
            if (state == eState.FROZEN)
            {
                if (freezeTimer.enabled == false)
                {
                    state = eState.DEFAULT;
                    freezeTimer.restart();
                    freezeTimer.stop();
                }
                return;
            }
            else if (state == eState.DUST)
            {
                if (dustTimer.enabled == false)
                {
                    Kill();
                    dustTimer.restart();
                    dustTimer.stop();
                    state = eState.DEFAULT;
                }
                return;
            }
            //check if its time to change dir (MOB is ideally on grid)
            if (speed == 0.0) return; //no movement, no update
            int grid = GameManager.gridSize;
            int x = posX, y = posY;
            
            // if player want to inverse direction: just do it even if mob isn't on grid point:            
            if(moveGap.enabled==false)//timer ended, next move
            {
                checkDirection();
                moveGap.start();
                updateBoundingBox();
                //speed = speed;//reinit timer ??? not sure if it will work
                if (myCollision!=null && myCollision.collisionStatic == true)
                { //in collision with static obj! don't move!
                    if (lastCollsionStatic == false)//I just hit wall
                    {
                        if (state != eState.ATTACK)
                            state = eState.IDLE;
                        //make move (snap to grid)
                        if(movingHorizontal)
                            Move(pxMove - ((pX + pxMove) % GameManager.gridSize), 0,false);
                        else //if (dir == eDir.D)
                            Move(0,pxMove - ((pY + pxMove) % GameManager.gridSize),false);

                    }
                    lastCollsionStatic = myCollision.collisionStatic;
                    return;
                }
                else if(state == eState.IDLE)
                    state = eState.DEFAULT;
                if (direction == eDir.R)
                        Move(pxMove, 0);
                else if (direction == eDir.L)
                        Move(-pxMove, 0);
                else if (direction == eDir.U)
                        Move(0, -pxMove);
                else if (direction == eDir.D)
                        Move(0, pxMove);

                if(myCollision!=null)
                    lastCollsionStatic = myCollision.collisionStatic;

                //Waypoint's stuff
                if (!GameMan.Map.waynetActive) return;
                Waypoint wp;
                wp = GameMan.Map.wayNetwork.getWPAt((uint)gridX, (uint)gridY);
                currentWP = wp; // do it, even if wp is null
                if (currentWP != null)
                { //MOB reached new wp:
                    lastWP = currentWP;
                    //enemy part
                    //direction = chaseRoute.Pop();
                    nextWP = lastWP.getWPAtDirection(direction);
                }
            }
        }
        public void Move(int _x, int _y)
        {
            Move(_x, _y, true);
        }
        public void Move(int _x, int _y,bool snapToGrid)
        {
            int grid = GameManager.gridSize;
            int oldX = posX, oldY = posY;
            if(snapToGrid)
            {
                if (movingHorizontal)
                    snapToGridX(_x);
                if (movingVertical)
                    snapToGridY(_y);  
            }
            else
            {
                posX+=_x; posY+=_y;
            }
            
            if ((pX % grid == 0) && (pY % grid == 0))
                isOnGrid = true;
            else
                isOnGrid = false;
            
            currentVisual.x = posX;
            currentVisual.y = posY;
        }/// <summary>
        /// update BoundingBox by setting rectangle passed as
        /// the parameters.
        /// </summary>
        public void updateBoundingBox(int x, int y, int width, int height)
        {
            bbox = new Rectangle(x, y, width, height);
        }
        public virtual void updateBoundingBox()
        {
            
                Point startPt;
                int wLoss = currentVisual.width / 4, hLoss = currentVisual.height / 4 - 4;
                wLoss = Math.Abs(wLoss); // when MOB is mirrored it will be negative value
                int bboxWidth = GameManager.gridSize;
                if (currentVisual.width < bboxWidth) bboxWidth = Math.Abs(currentVisual.width);
                else wLoss = bboxWidth / 4;

                int bboxHeight = GameManager.gridSize;
                if (currentVisual.height < bboxHeight) bboxHeight = currentVisual.height;
                else hLoss = bboxHeight / 3 - 4;
                //if (currentVisual.width>0)
                //    startPt = new Point(pX - GameManager.gridSize, pY);
                //else
                startPt = new Point(pX, pY);
                //Obj gridPos = new Obj("../data/Textures/HLP_GRID.dds", startPt.X + wLoss, startPt.Y + hLoss, Obj.align.LEFT);
                //gridPos.width = Math.Abs(bboxWidth) - (2 * wLoss);
                //gridPos.height = bboxHeight - (2 * hLoss);
                //gridPos.setRenderOnce();
                //gridPos.setCurrentTexAlpha(190);
                bbox = new Rectangle(startPt.X + wLoss, startPt.Y + hLoss, Math.Abs(bboxWidth) - (2 * wLoss), bboxHeight - (2 * hLoss));
            
        }
        public void Teleport(Obj dst)
        {

            Point dstPt = GameManager.getObjGridPos(dst);
            int sizeDiff = Math.Abs(currentVisual.width) - GameManager.gridSize;
            posX = (dstPt.X + 1) * GameManager.gridSize + sizeDiff;
            if (!mobMirrored)
                posX -= currentVisual.width;

            posY = dstPt.Y * GameManager.gridSize;

            new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_TELEPORT.ogg", false, true);
            //FX:
            GameMan.ReinitTeleportFX((int)(dst.x - GameManager.gridSize * 2.25), (int)(dst.y - GameManager.gridSize * 2.25));
            isIgnitOfCurrentTeleportFX = true;
        }

        public eDir inverseDirection(MOB.eDir dir)
        {
            if (dir == eDir.L) return eDir.R;
            if (dir == eDir.R) return eDir.L;
            if (dir == eDir.D) return eDir.U;
            if (dir == eDir.U) return eDir.D;
            return eDir.UNDEF;
        }
        public void setSlow()
        {
            const int SLOW_TIME_MULTIPILER = 7;//7x slower movement? ugh i think so
            moveGap = new Timer(Timer.eUnits.MSEC, (int)moveGap.totalTime * SLOW_TIME_MULTIPILER, 0, true, false);
            moveGap.start();
        }
        public virtual void setFreeze()
        {
            if (invincible == true) return;
            iceCubeVisual.setCurrentTexAlpha(180);
            state = eState.FROZEN;
            freezeTimer.restart();
        }

        public void turnToDust()
        {
            if (invincible == true) return;
            state = eState.DUST;
            dustTimer.start();
        }
        public void resetMovementSpeed()
        {
            speed = baseSpeed;
        }
        public void increaseBaseSpeed(int val)
        {
            baseSpeed += val;
        }
        /// <summary>
        /// Killing MOB object will move
        /// all kinds of mob to spawn point position
        /// rest depends on child class implementation.
        /// </summary>
        public virtual void Kill()
        {
            state = eState.DEFAULT;
            freezeTimer.restart();
            freezeTimer.stop();
            dustTimer.restart();
            dustTimer.stop();
            moveToSpawnPoint();
        }
        /// <summary>
        /// Attached light will be changing positons just like mob it's attached to.
        /// </summary>
        /// <param name="light"></param>
        public void attachLight(Light light)
        {
            if (attachedLights == null) attachedLights = new List<Light>();
            attachedLights.Add(light);
            light.setParentMOB(this);
        }
        public void eatCookie(PowerUp cookie)
        {
            if (takenPowerUP == cookie) return;
            //another cookie still eaten, speed it up
            if (takenPowerUP != null)
                if (takenPowerUP.type == POWER_UP.COOKIE)
                {
                    GameMan.removePowerUp(takenPowerUP);
                    state = eState.DEFAULT;
                    takenPowerUP = null;
                }
            state = eState.ATTACK;
            currentVisual.restartTexAni();
            cookieEatenHoldFrames = 1;
            takenPowerUP = cookie;
            Sound cookieVaccum;
            int rnd = GameManager.variantizer.Next(20);
            if (rnd < 9)
                cookieVaccum = new Sound(Sound.eSndType.SFX, "../data/Sounds/COOKIE_EAT1.ogg", false, false, 0.33);
            else if (rnd < 18)
                cookieVaccum = new Sound(Sound.eSndType.SFX, "../data/Sounds/COOKIE_EAT2.ogg", false, false, 0.33);
            else if (rnd < 19)
                cookieVaccum = new Sound(Sound.eSndType.SFX, "../data/Sounds/COOKIE_EAT3.ogg", false, false, 0.95);
            else
                cookieVaccum = new Sound(Sound.eSndType.SFX, "../data/Sounds/COOKIE_EAT4.ogg", false, false, 0.7);

            cookieVaccum.Play();
            GameMan.statistics.addEatenCookies(1);
            GameMan.removePowerUp(takenPowerUP);
        }
        /// <summary>
        /// Creates shield by starting shield Timer
        /// and setting invincible to true
        /// </summary>
        public void createShield()
        {
            // when shield timer is enabled, shield visuals will be rendered overlay MOB
            shieldTimer.start();
            invincible = true;
        }

        //
        // Private methods
        private void snapToGridY(int _y)
        {
            int grid = GameManager.gridSize;
            if ((direction == eDir.D) && ((posY % grid) > ((posY + _y) % grid)))
                posY = (posY + _y) - ((posY + _y) % grid);
            else if ((direction == eDir.U) && (posY % grid != 0) && ((posY % grid) < ((posY + _y) % grid)))
                posY = (posY + _y) + grid - ((posY + _y) % grid);
            else
                posY += _y;
        }
        private void snapToGridX(int _x)
        {
            int grid = GameManager.gridSize;
            if ((direction == eDir.R) && (posX % grid > (posX + _x) % grid))
                posX = (posX + _x) - ((posX + _x) % grid);
            else if ((direction == eDir.L) && (pX % grid != 0) && (pX % grid < (pX + _x) % grid))
            {
                pX = (pX + _x) + (grid-((pX + _x) % grid));
            }
            else
                posX += _x;
        }

        private void checkDirection()
        {
            // direction will change in the same orientation -> then just do it and don't care 'bout nothing
            if (((dirBuffer == eDir.L) && (direction == eDir.R)) || ((direction == eDir.L) && (dirBuffer == eDir.R))
            || ((dirBuffer == eDir.U) && (direction == eDir.D)) || ((direction == eDir.U) && (dirBuffer == eDir.D)))
            {
                direction = dirBuffer;
            }
            // MOB is on grid pos -> we trying to change direction
            if (GameMan.Map.waynetActive)
            {
                Waypoint wp = Waypoint.wnet.getWPAt((uint)gridX, (uint)gridY);
                if ((direction != dirBuffer) && (isOnGrid) && wp != null)
                {
                    // mob it's on grid, now check if grid on direction which he want to head
                    // is not collideable
                    eDir oldDir = direction;
                    direction = dirBuffer;
                    int grid = GameManager.gridSize;

                    if ((direction == eDir.D) && (wp.downWP == null)) direction = oldDir;
                    else if ((direction == eDir.L) && (wp.leftWP == null)) direction = oldDir;
                    else if ((direction == eDir.R) && (wp.rightWP == null)) direction = oldDir;
                    else if ((direction == eDir.U) && (wp.upWP == null)) direction = oldDir;

                }
            }
            else if (!GameMan.Map.waynetActive && isOnGrid && direction != dirBuffer)
            {
                    // mob it's on grid, now check if grid on direction which he want to head
                    // is not collideable
                    eDir oldDir = direction;
                    direction = dirBuffer;
                    myCollision.Update();
                    if (myCollision.collisionStatic)
                        direction = oldDir;
                    myCollision.Update();
            }
        }

        private void updateTakenPowerUP()
        {
            if (takenPowerUP == null) return;
            //cookie:
            if (takenPowerUP.type == POWER_UP.COOKIE)
            {
                if (currentVisual.texAniFinished())
                {
                    if (cookieEatenHoldFrames > 0) cookieEatenHoldFrames--;
                    else
                    {
                        //GameMan.removePowerUp(takenPowerUP);
                        state = eState.DEFAULT;
                        takenPowerUP = null;
                    }
                }
                //else if (state != eState.ATTACK)//something strange occured, but still we need to remove cookie:
                //{
                //    new DebugMsg("state was changed during eating cookie\n but still disposing cookie from map...", DebugLVL.warn);
                //   GameMan.removePowerUp(takenPowerUP);
                //    takenPowerUP = null;
                //}
            }
        }

        /// <summary>
        /// Moves MOB back to his spawnPoint on map
        /// </summary>
        private void moveToSpawnPoint()
        {
            Point dest = new Point(spawnWP.x * GameManager.gridSize, spawnWP.y * GameManager.gridSize);
            pX = spawnWP.x * GameManager.gridSize;
            posY = spawnWP.y * GameManager.gridSize;
            //Move(dest.X - posX, dest.Y - posY,true);
        }
        internal void RemoveLight(Light light)
        {
            if (attachedLights == null) return;
            if (EngineApp.Game.self.lightEngine == null) return;
            attachedLights.Remove(light);
        }

        internal void RemoveLights()
        {
            if (attachedLights == null) return;
            if (EngineApp.Game.self.lightEngine == null) return;
            for (int i = 0; i < attachedLights.Count; i++)
            {
                EngineApp.Game.self.lightEngine.removeLight(attachedLights[i]);
                attachedLights[i] = null;
            }
        }

        /// <summary>
        /// Sets "BAM"/"POOF" etc. fx in cartoon style over mob.
        /// </summary>
        private Obj fxComicBoom;
        protected void bamFX()
        {
            if (fxComicBoom != null) return;
            fxComicBoom = new Obj("../data/Textures/GAME/FX/HIT_V0.dds", pX + GameManager.gridSize / 2, pY + GameManager.gridSize / 2, Obj.align.CENTER_BOTH);
            fxComicBoom.addAni(new Obj_Animation(fxComicBoom));
            fxComicBoom.objAnimation.affectionFlags = Obj_Animation.eAffectionFlags.NONE;
            fxComicBoom.objAnimation.setRelativeMovement();
            fxComicBoom.objAnimation.addKeyframe(64, 64, 0, 1.0, new Timer(Timer.eUnits.FPS, 0));
            fxComicBoom.objAnimation.addKeyframe(0, 0, 0, 1.25, new Timer(Timer.eUnits.FPS, 5));
            fxComicBoom.objAnimation.addKeyframe(0, 0, 0, 1.25, new Timer(Timer.eUnits.MSEC, 100));
            fxComicBoom.objAnimation.setLoopType(Obj_Animation.eLoopType.None);
            fxComicBoom.layer = Layer.imgFG;
        }
        /// <summary>
        /// Method called when player/enemy was hit
        /// </summary>
        protected void onHit()
        {
            if( mobType == eMOBType.ENEMY || mobType == eMOBType.PLAYER) 
            bamFX();
        }
    }
}
