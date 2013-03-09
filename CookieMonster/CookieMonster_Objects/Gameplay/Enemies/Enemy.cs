using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    class Enemy : MOB
    {
        //Right now variations only work and are needed with default kind of enemy
        static int visualVariationizer = 0;
        private int visualsCount;
        private String texNameBase;
        //NOTE: Even if WIZARD is threaten as child class of Enemy it's type
        // still need to exist in here cause it needs to set proper visuals, etc.
        public enum enemyType { NORMAL, ASSASSIN, THIEF, SHOOPDAWOOP, WIZARD }
        public enum eEnemyState
        {
            NORMAL,     // chasing player
            FLEE,       // player eaten powerPill run for your life!
            WOUNDED,    // just eaten by hero runing quickly to the startpoint
            UNCONSCIOUS // x,X i need some rest, 10s should be ok ;)
        }
        private Timer woundedTimer;
        public enemyType type { get; private set; }
        private eEnemyState _enemyState;
        public eEnemyState enemyState
        {
            get { return _enemyState; }
            set
            {
                if (_enemyState == eEnemyState.FLEE && value == eEnemyState.UNCONSCIOUS || value == eEnemyState.WOUNDED)
                    onHit();

                _enemyState = value;
            }
        }
        private eEnemyState oldstate;
        private double baseSpeed;

        Waypoint startpoint; // when enemy was "eated" by player enemy will set route to startpoint
        ShortestWay chaseRoute; // shortest route from currentWP to player.currentWP
        public string routeList { get { if (chaseRoute != null) return chaseRoute.ToString(); else return "NULL"; } }

        // for checking if enemy stucked, SAD but needed :( ...yeah, I suck ;(
        private Timer unstuckTimer = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
        private Point unstuckLastPt;
        public Enemy(enemyType typ, int posx, int posy, double spd)
            : base(posx, posy, spd)
        {
            type = typ;
            baseSpeed = spd;
            oldstate = _enemyState = eEnemyState.NORMAL;
            mobType = eMOBType.ENEMY;
            base.myCollision = new CollisionReport(this);
            if (typ != enemyType.THIEF) //thief needs collision for stealing cookies
                myCollision.disableDynamicCollision(); // disable collisions with cookies & mobs
            GameMan.enemiesList.Add(this);
            setVisuals();
            woundedTimer = new Timer(Timer.eUnits.MSEC, 15 * 1000, 0, true, false);

            //new DebugMsg(this, "enemyState", DebugLVL.info);
            //new DebugMsg(this, "lastWPid");
            //new DebugMsg(this, "nxtWPid"); 
            //new DebugMsg(this, "routeList", DebugLVL.info);
            mobType = eMOBType.ENEMY;
            unstuckTimer.start();
        }

        /// <summary>
        /// function sets or updates proper visuals for enemy type 
        /// (default type has some variations, see visualVariationizer)
        /// funcion updates visuals when enemy.eEnemyState was changed
        /// </summary>
        public virtual void setVisuals()
        {
            Obj vis; int widthMul = 1;
            if (texNameBase == null)
            {
                visualsCount = 3;
                texNameBase = "ghost_" + visualVariationizer.ToString();
                visualVariationizer = (visualVariationizer + 1) % visualsCount;

                // frozen:
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_frozen_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                setStateVisual(vis, "FROZEN");

                // dust:
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_dust_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                vis.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                setStateVisual(vis, "DUST");
            }
            else
            {//so we already have some visualDefault grab pos:
                if (mobMirrored == true)
                {
                    widthMul = -1;
                }
            }
            if (enemyState == eEnemyState.FLEE)
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_scared_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            else if (enemyState == eEnemyState.WOUNDED)
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_wounded_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            else
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_move_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            vis.width *= widthMul;
            setStateVisual(vis, "DEFAULT");
            //todo: idle, etc.
        }
        public virtual void Update()
        {
            // initialize startpoint:
            if (startpoint == null) startpoint = lastWP;

            updateState();

            base.Update();
            if (chaseRoute == null) recalculateShortestWay();
            Waypoint wp;
            wp = GameMan.Map.wayNetwork.getWPAt((uint)gridX, (uint)gridY);

            if ((enemyState == eEnemyState.FLEE) && isOnGrid && wp != null)
            {
                dirBuffer = direction = runawayDirection(wp);
            }
            else if ((enemyState == eEnemyState.WOUNDED) && wp != null && wp != startpoint)
            {
                if (isOnGrid && chaseRoute != null && (chaseRoute.route.dirList.Count != 0))
                    dirBuffer = direction = chaseRoute.Pop();
                if (chaseRoute == null || (chaseRoute.route.dirList.Count == 0))
                {
                    setRouteToStartpoint();
                }
            }
            else if (wp != null)
            {
                if (isOnFirstWPFromStack(wp))// || nextDirectionOnStackIsOpposite())
                {
                    if (chaseRoute.route.dirList.Count > 0)
                    {
                        dirBuffer = chaseRoute.Pop();
                    }
                    //new DebugMsg("POPed dir from chaseRoute!");
                }
            }
            //mob needs unstuck?
            if (unstuckTimer.enabled == false)
            {
                if (pX == unstuckLastPt.X && pY == unstuckLastPt.Y && state != eState.FROZEN && enemyState == eEnemyState.NORMAL)
                {
                    new DebugMsg("unstuck enemy!");
                    enemyState = eEnemyState.FLEE;
                }
                unstuckTimer.start();
                unstuckLastPt.X = pX; unstuckLastPt.Y = pY;
            }
            oldstate = enemyState;
        }

        public void prepareRender()
        {
            base.prepareRender();
        }
        public override void Kill()
        {
            base.Kill();
            enemyState = eEnemyState.FLEE;
            setRouteToStartpoint();
        }
        private eDir runawayDirection(Waypoint curWP)
        {
            ShortestWay[] s = new ShortestWay[4];
            eDir[] sDir = new eDir[4];
            int i = 0;
            if (curWP.downWP != null)
            {
                sDir[i] = eDir.D;
                if (GameMan.PC.lastWPid != -1)
                    s[i] = ShortestWaysTable.getCopy(curWP.downWP._id, GameMan.PC.lastWPid);
                else if (GameMan.PC.nxtWPid != -1)
                    s[i] = ShortestWaysTable.getCopy(curWP.downWP._id, GameMan.PC.nxtWPid);
                if (s[i] != null) { s[i] = s[i].Copy(); i++; }
            }
            if (curWP.leftWP != null)
            {
                sDir[i] = eDir.L;
                if (GameMan.PC.lastWPid != -1)
                    s[i] = ShortestWaysTable.getCopy(curWP.leftWP._id, GameMan.PC.lastWPid);
                else if (GameMan.PC.nxtWPid != -1)
                    s[i] = ShortestWaysTable.getCopy(curWP.leftWP._id, GameMan.PC.nxtWPid);
                if (s[i] != null) { s[i] = s[i].Copy(); i++; }
            }
            if (curWP.rightWP != null)
            {
                sDir[i] = eDir.R; //TODO: idx out of range (lastWPid)
                if (GameMan.PC.lastWPid != -1)
                    s[i] = ShortestWaysTable.getCopy(curWP.rightWP._id, GameMan.PC.lastWPid);
                else if (GameMan.PC.nxtWPid != -1)
                    s[i] = ShortestWaysTable.getCopy(curWP.rightWP._id, GameMan.PC.nxtWPid);

                if (s[i] != null) { s[i] = s[i].Copy(); i++; }
            }
            if (curWP.upWP != null)
            {
                sDir[i] = eDir.U;
                if (GameMan.PC.lastWPid != -1)
                    s[i] = ShortestWaysTable.getCopy(curWP.upWP._id, GameMan.PC.lastWPid);
                else if (GameMan.PC.nxtWPid != -1)
                    s[i] = ShortestWaysTable.getCopy(curWP.upWP._id, GameMan.PC.nxtWPid);
                if (s[i] != null) { s[i] = s[i].Copy(); i++; }
            }
            int biggestWeight = 0; int biggestWeight_i = 0;
            for (i--; i >= 0; i--)
            {
                if (s[i].weight > biggestWeight)
                    biggestWeight_i = i;
            }
            return sDir[biggestWeight_i];
        }
        private bool wasFrozen;
        private void updateState()
        {
            // enemy frozen? -> set FLEE visuals:
            if ((isFrozen) && (enemyState == eEnemyState.NORMAL))
            {
                wasFrozen = true;
                enemyState = eEnemyState.FLEE;
            }
            if ((wasFrozen) && (!isFrozen) && (enemyState == eEnemyState.FLEE))
            {
                wasFrozen = false;
                if (GameMan.powerPillActive())
                    enemyState = eEnemyState.FLEE;
                else
                    enemyState = eEnemyState.NORMAL;
            }
            // new enemyState aplied
            if (enemyState != oldstate)
            {
                if (enemyState == eEnemyState.NORMAL)
                {
                    setVisuals();
                    speed = baseSpeed;
                    recalculateShortestWay();
                }
                else if (enemyState == eEnemyState.FLEE)
                {
                    setVisuals();//new visual need to be set
                    speed = GameManager.gridSize / 3;
                    direction = dirBuffer = inverseDirection(direction);//first, we need to comeback to lastWP
                }
                else if (enemyState == eEnemyState.WOUNDED)
                {
                    doActionsOnWounded();
                    setVisuals();//new visual need to be set
                    speed = GameManager.gridSize * 3;
                    setRouteToStartpoint();
                    woundedTimer.restart();
                }
                else if (enemyState == eEnemyState.UNCONSCIOUS)
                {
                    speed = 0;
                }
            }
            else if ((enemyState == eEnemyState.WOUNDED) && (woundedTimer.enabled == false))
            {
                //set normal enemyState
                enemyState = eEnemyState.NORMAL;
                setVisuals();
                speed = baseSpeed;
            }

        }

        private void doActionsOnWounded()
        {
            if (type == enemyType.THIEF)
            {
                ((Thief)this).giveBackCookies();
            }
        }


        private void setRouteToStartpoint()
        {

            if (nextWP != null)
                chaseRoute = ShortestWaysTable.getCopy(nextWP._id, startpoint._id);
            else if (lastWP != null)
            {
                chaseRoute = ShortestWaysTable.getCopy(lastWP._id, startpoint._id);
                Waypoint wp;
                wp = GameMan.Map.wayNetwork.getWPAt((uint)gridX, (uint)gridY);
                if (((wp != null) && (chaseRoute != null) && (chaseRoute.wpList.Count > 0) && (wp._id != chaseRoute.wpList[0]._id))
                        || wp == null)
                {
                    direction = inverseDirection(direction);//first, we need to comeback to lastWP
                    dirBuffer = direction;
                }
            }
        }

        private bool isOnFirstWPFromStack(Waypoint wp)
        {
            return ((wp != null) && isOnGrid && (chaseRoute != null) && (wp == chaseRoute.wpList[0]));
        }
        private bool nextDirectionOnStackIsOpposite()
        {
            if (isOnGrid) return false;
            if (chaseRoute == null) return false;
            else if (chaseRoute.route == null) return false;
            else if (chaseRoute.route.dirList.Count > 0)
            {
                // I LOL'D ;D
                return (((direction == eDir.R) && (chaseRoute.route.dirList[0] == eDir.L))
                || ((direction == eDir.L) && (chaseRoute.route.dirList[0] == eDir.R))
                || ((direction == eDir.D) && (chaseRoute.route.dirList[0] == eDir.U))
                || ((direction == eDir.U) && (chaseRoute.route.dirList[0] == eDir.D)));
            }
            else return false;
        }
        public void recalculateShortestWay()
        {
            if (!tryToSetRoute(nextWP))
                if (!tryToSetRoute(lastWP))
                    if (!tryToSetRoute(currentWP))
                        new DebugMsg("Updating chase Route went wrong!");

        }

        private bool tryToSetRoute(Waypoint WP)
        {
            if (WP != null)
            {
                ShortestWay s;
                int nxtWPRouteWeight = int.MaxValue, lastWPRouteWeight = int.MaxValue;
                if ((GameMan.PC.nextWP != null) && (WP._id != GameMan.PC.nextWP._id))
                { // at first enemy will try to go to next player WP
                    s = ShortestWaysTable.getCopy(WP._id, GameMan.PC.nextWP._id);
                    if (s != null)
                    {
                        chaseRoute = s;
                        //new DebugMsg("Enemy has new chase route (" + WP._id + "->" + GameMan.PC.nextWP._id + ")");
                        nxtWPRouteWeight = (int)s.weight;
                        return true;
                    }
                }
                else if ((GameMan.PC.lastWP != null) && (WP._id != GameMan.PC.lastWP._id))
                { //if there is no next WP at last try to go to the last hero WP
                    s = ShortestWaysTable.getCopy(WP._id, GameMan.PC.lastWP._id);
                    if (s != null)
                    {
                        lastWPRouteWeight = (int)s.weight;
                        if (lastWPRouteWeight < nxtWPRouteWeight)
                        {
                            chaseRoute = s;
                            //new DebugMsg("Enemy has new chase route (" + WP._id + "->" + GameMan.PC.lastWP._id + ")");
                        }
                        return true;
                    }
                }
                else
                    return false;
            }
            return false;
        }
    }
}
