using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{
    //arguments of dynamic collision event:
    class collDynEventArgs : EventArgs
    {
        public Enemy enemy { get; private set; }
        private PowerUp _powerup; public PowerUp cdPowerUP { get { return _powerup; } }
        public Player player { get; private set; }
        public collDynEventArgs(Enemy m,PowerUp p,Player pc)
        { 
            this.enemy = m;
            this._powerup = p;
            this.player = pc;
        }
    }
    class CollisionReport
    {
        static public CollisionSystem collSystem;
        public CollisionReport(MOB m)
        {
            _collisionStatic = false;
            srcMob = m;
            collSystem.AddReport(this);
            this.lastType = GameMap.objType.OTHER;
        }
        public CollisionReport(Projectile p)
        {
            _collisionStatic = false;
            sourceProjectile = p;
            collSystem.AddReport(this);
            this.lastType = GameMap.objType.OTHER;
        }
        MOB srcMob; public MOB sourceMob { get { return srcMob; } }
        public Projectile sourceProjectile { get; private set; }
        bool _collisionStatic; public bool collisionStatic { get { return _collisionStatic; } }
        public bool collisionDynamicDisabled{get; private set;}
        public GameMap.objType lastType;

        public void setStaticCollision(bool v)
        {
            _collisionStatic = v;
        }
        //
        // dynamic collision (handled with event:)
        public delegate void CDEvtHandler(MOB src, collDynEventArgs fe);

        // dynamic collision event:
        public event CDEvtHandler cdEvent;

        public void triggerCDEvent(MOB src, collDynEventArgs fe)
        {
            if(cdEvent!=null)//only when mob has some thing of reaction methood to dynamic collision
                cdEvent(src,fe);
        }
        public void disableDynamicCollision()
        {
            collisionDynamicDisabled = true;
        }
        public int gridX { get { if (srcMob != null) { return srcMob.gridX; } else if (sourceProjectile != null) { return sourceProjectile.gridX; } else return 0; } }
        public int gridY { get { if (srcMob != null) { return srcMob.gridY; } else if (sourceProjectile != null) { return sourceProjectile.gridY; } else return 0; } }
        public int nextGridX { get { if (srcMob != null) { return srcMob.nextGridX; } else if (sourceProjectile != null) { return sourceProjectile.nextGridX; } else return 0; } }
        public int nextGridY { get { if (srcMob != null) { return srcMob.nextGridY; } else if (sourceProjectile != null) { return sourceProjectile.nextGridY; } else return 0; } }
        public int pX { get { if (srcMob != null) { return srcMob.pX; } else if (sourceProjectile != null) { return sourceProjectile.pX; } else return 0; } }
        public int pY { get { if (srcMob != null) { return srcMob.pY; } else if (sourceProjectile != null) { return sourceProjectile.pY; } else return 0; } }
        public Rectangle boundingBox { 
        get { 
            if (srcMob != null) { return srcMob.boundingBox; } 
            else if (sourceProjectile != null) {
                return sourceProjectile.boundingBox;
            } 
            else return new Rectangle(); 
            } 
        }

        public void Update()
        {
            GameManager gm = engineReference.getEngine().gameManager;
            //Static collison:
            int xx = gridX;
            int yy = gridY;
            int px = nextGridX;
            int py = nextGridY;
            int grid = GameManager.gridSize;
            lastType = gm.Map.getObjTypeFromPixel(px, py);
            /*Obj gridPos = new Obj("../data/Textures/HLP_GRID.dds", px * 48, py * 48, Obj.align.LEFT);
            gridPos.setRenderOnce();
            gridPos.setCurrentTexAlpha(150);
            Obj gridPos2 = new Obj("../data/Textures/HLP_GRID2.dds", xx * 48, yy * 48, Obj.align.LEFT);
            gridPos2.setRenderOnce();
            gridPos2.setCurrentTexAlpha(100);
            */
            if ((lastType & GameMap.objType.COLLIDEABLE) == GameMap.objType.COLLIDEABLE)
            {
                if( (xx != px || yy != py) && sourceMob.isOnGrid &&
                    (gm.Map.getObjTypeFromPixel(xx, yy) & GameMap.objType.PORTAL) == GameMap.objType.PORTAL)
                {
                    // When we get there, it means that MOB should be teleported through portal but
                    // it goes so quick that MOB wasn't even steped in "teleporting" part of portal
                    // and MOB already hit the wall.
                    Portal p = gm.Map.getPortalByColor(gm.Map.getPXColor(xx, yy));
                    int posx = pX, posy = pY;

                    Point p1grid;
                    p1grid = new Point((p.portalObj1.x + p.portalObj1.width / 2) / grid,
                                             (p.portalObj1.y + p.portalObj1.height / 2) / grid);

                    Point p2grid;
                    p2grid = new Point((p.portalObj2.x + p.portalObj2.width / 2) / grid,
                                             (p.portalObj2.y + p.portalObj2.height / 2) / grid);

                    if ((p2grid.X == xx) && (p2grid.Y == yy))
                    {//mob is on portal 2:
                            sourceMob.Teleport(p.portalObj1);
                    }
                    else if ((p1grid.X == xx) && (p1grid.Y == yy))
                    {//mob is on portal 1
                            sourceMob.Teleport(p.portalObj2);
                    }
                    
                }
                setStaticCollision(true);
            }
            else
            {
                if ((sourceMob != null) && ((lastType & GameMap.objType.PORTAL) == GameMap.objType.PORTAL))
                {
                    const int tolerance = 12;
                    GameMap map = gm.Map;
                    Portal p = map.getPortalByColor(gm.Map.getPXColor(px, py));
                    int posx = pX, posy = pY;


                    Point p1grid;
                    p1grid = new Point((p.portalObj1.x + p.portalObj1.width / 2) / grid,
                                             (p.portalObj1.y + p.portalObj1.height / 2) / grid);

                    Point p2grid;
                    p2grid = new Point((p.portalObj2.x + p.portalObj2.width / 2) / grid,
                                             (p.portalObj2.y + p.portalObj2.height / 2) / grid);

                    if (((p2grid.X == xx) || (p2grid.X == xx + 1)) && ((p2grid.Y == yy) || (p2grid.Y == yy + 1)))
                    {//mob is on portal 2:
                        if (((posx + tolerance >= xx * grid) && (posx - tolerance <= xx * grid))
                        && ((posy + tolerance >= yy * grid) && (posy - tolerance <= yy * grid)))
                        {// let's teleport!
                            sourceMob.Teleport(p.portalObj1);
                        }
                    }
                    else if (((p1grid.X == xx) || (p1grid.X == xx + 1)) && ((p1grid.Y == yy) || (p1grid.Y == yy + 1)))
                    {//mob is on portal 1
                        if (((posx + tolerance >= xx * grid) && (posx - tolerance <= xx * grid))
                        && ((posy + tolerance >= yy * grid) && (posy - tolerance <= yy * grid)))
                        {// let's teleport!
                            sourceMob.Teleport(p.portalObj2);
                        }
                    }
                }
                setStaticCollision(false);
            }

            // dynamic collision:
            if (!collisionDynamicDisabled)
            {
                //(PowerUPs)
                if (sourceMob != null)
                {
                    int max = gm.sortedPowerUpList.Count;
                    int min = 0;
                    int step = (max - min) / 2;
                    while (min < max)
                    {
                        if (min + step >= gm.sortedPowerUpList.Count)
                        {
                            max = min; break;
                        }//TODO: sometimes min+step gets > arraySize (min=count-1;step=1)
                        else if (gm.sortedPowerUpList[min + step].pY < pY)
                        {
                            min += step;
                        }
                        else
                        {
                            break;
                            max -= step;
                        }
                        step = (max - min) / 2; if (step < 1) step = 1;
                    }
                    if (min > 0) min--;
                    for (int j = min; (j < gm.sortedPowerUpList.Count); j++)
                    {
                        if (gm.sortedPowerUpList[j].gridY - 1 > gridY) break; //different y grid-break loop
                        if (gm.sortedPowerUpList[j].boundingBox.IntersectsWith(boundingBox))
                        {
                            triggerCDEvent(sourceMob, new collDynEventArgs(null, gm.sortedPowerUpList[j], null));
                        }
                    }
                }
                //dynamic Collision Enemies:
                for (int j = 0; j < gm.enemiesList.Count; j++)
                {
                    if (gm.enemiesList[j].boundingBox.IntersectsWith(boundingBox))
                    {
                        if (sourceProjectile != null)
                        {
                            triggerCDEvent(sourceProjectile, new collDynEventArgs(gm.enemiesList[j], null, null));
                        }
                        else if (sourceMob != null)
                        {
                            triggerCDEvent(sourceMob, new collDynEventArgs(gm.enemiesList[j], null, null));
                        }
                    }
                }
                //dynamic Collision with player:
                if (sourceMob == null || sourceMob.mobType != MOB.eMOBType.PLAYER)
                {
                    if (gm.PC.boundingBox.IntersectsWith(boundingBox))
                    {
                        if (sourceProjectile != null)
                        {
                            triggerCDEvent(sourceProjectile, new collDynEventArgs(null, null, gm.PC));
                        }
                        else if (sourceMob != null)
                        {
                            triggerCDEvent(sourceMob, new collDynEventArgs(null, null, gm.PC));
                        }
                    }
                }
            }

        }
       
    }
    class CollisionSystem
    {
        GameManager gm;
        List<CollisionReport> reportsList;

        /// <summary>
        /// Updates all CollisionReports on list
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < reportsList.Count; i++)//go through list of all objects with collision report
            {
                reportsList[i].Update();
            }
        }
        public CollisionSystem(GameManager th)
        {
            reportsList = new List<CollisionReport>();
            gm = th;
        }
        public void AddReport(CollisionReport r)
        {
            reportsList.Add(r);
        }
        public void RemoveReport(Projectile p)
        {
            for(int i=0;i<reportsList.Count;i++)
                if (reportsList[i].sourceProjectile == p)
                {
                    reportsList.RemoveAt(i);
                    break;
                }
        }
        public void RemoveReport(MOB m)
        {
            for (int i = 0; i < reportsList.Count; i++)
                if (reportsList[i].sourceMob == m)
                {
                    reportsList.RemoveAt(i);
                    break;
                }
        }
    }
    
}
