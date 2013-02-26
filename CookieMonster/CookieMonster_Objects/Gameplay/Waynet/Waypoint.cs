using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    class Waypoint
    {
        static public int lastid = 0;
        static public Waynet wnet;
        // position: (x,y are grid-scaled coords)
        int _x, _y;
        public int x { get { return _x; } }
        public int y { get { return _y; } }

        //Connected ways:
        private Way U, R, D, L;
        public Way Up    { get { return U; } set { if (U == null)U = value; } }
        public Way Right { get { return R; } set { if (R == null)R = value; } }
        public Way Down  { get { return D; } set { if (D == null)D = value; } }
        public Way Left  { get { return L; } set { if (L == null)L = value; } }

        //Connected waypoints (on other side of way)
        public Waypoint upWP    { get { if(U != null){if(U.begin==this)return U.end; else return U.begin;} return null; } }
        public Waypoint rightWP { get { if(R != null){if(R.begin==this)return R.end; else return R.begin;} return null; } }
        public Waypoint downWP  { get { if(D != null){if(D.begin==this)return D.end; else return D.begin;} return null; } }
        public Waypoint leftWP  { get { if(L != null){if(L.begin==this)return L.end; else return L.begin;} return null; } }

        private int id; public int _id { get { return id; } }
        public Waypoint(int x, int y)
        {
            id = lastid++;
            _x = x; _y = y;
        }
        public void generateWays()
        {
            GameMap map = wnet.map;
            uint steps = 0; int lastStepOnPortal = -1;
            Point curPos = new Point(x, y);

            if ((x == 21) && (y == 5))
            {
                bool doAfuckingBreakpointWitAFlip = true;
                if (doAfuckingBreakpointWitAFlip == true)
                {
                    doAfuckingBreakpointWitAFlip=false; //lol will it work now?
                }
            }

            //Direction: UP
            if(Up==null) //there is no way connected to up of waypoint
                while ( (map.getObjTypeFromPixel(curPos.X, curPos.Y) & GameMap.objType.COLLIDEABLE) != GameMap.objType.COLLIDEABLE)
                {
                    if (steps > 0)//don't check in first loop cuz it will be same wp as begining
                    {
                        //is there an waypoint?
                        Waypoint wp = wnet.getWPAt((uint)curPos.X, (uint)curPos.Y);
                        if (wp != null) { new Way(this, wp, steps, Way.eWayDir.UD); break; }
                    }
                    //not found an waypoint go to futher pos
                    if ((steps>lastStepOnPortal+1)&&((map.getObjTypeFromPixel(curPos.X, curPos.Y) & GameMap.objType.PORTAL) == GameMap.objType.PORTAL))
                    {//current point is passing through portal, find other side pos:
                        Point newPos = map.tryGoThroughPortal(curPos);
                        if (newPos == curPos) curPos.Y -= 1;
                        else
                        {
                            lastStepOnPortal = (int)steps;
                            curPos = newPos;
                        }
                    }
                    else if ((curPos.X >= 0) && (curPos.Y >= 0) && (curPos.Y < map.mapHeight) && (curPos.X < map.mapWidth))
                        curPos.Y -= 1;
                    else break;//end of map reached
                    steps=steps+1;
                }

            //Direction: Down
            steps = 0; lastStepOnPortal = -1; //comeback to wp Pos
            curPos.X = x; curPos.Y = y;
            if (Down == null) //there is no way connected to up of waypoint
                while ((map.getObjTypeFromPixel(curPos.X, curPos.Y) & GameMap.objType.COLLIDEABLE) != GameMap.objType.COLLIDEABLE)
                {
                    if (steps > 0)//don't check in first loop cuz it will be same wp as begining
                    {
                        //is there an waypoint?
                        Waypoint wp = wnet.getWPAt((uint)curPos.X, (uint)curPos.Y);
                        if (wp != null) { new Way(this, wp, steps, Way.eWayDir.DU); break; }
                    }
                    //not found an waypoint go to futher pos
                    if ((steps > lastStepOnPortal + 1) && ((map.getObjTypeFromPixel(curPos.X, curPos.Y) & GameMap.objType.PORTAL) == GameMap.objType.PORTAL))
                    {//current point is passing through portal, find other side pos:
                        Point newPos = map.tryGoThroughPortal(curPos);
                        if (newPos == curPos) curPos.Y += 1;
                        else
                        {
                            lastStepOnPortal = (int)steps;
                            curPos = newPos;
                        }
                    }
                    else if ((curPos.X >= 0) && (curPos.Y >= 0) && (curPos.Y < map.mapHeight) && (curPos.X < map.mapWidth))
                        curPos.Y += 1;
                    else break;//end of map reached
                    steps++;
                }

            //Direction: Left
            steps = 0; lastStepOnPortal = -1; //comeback to wp Pos
            curPos.X = x; curPos.Y = y;
            if (Left == null) //there is no way connected to up of waypoint
                while (!((map.getObjTypeFromPixel(curPos.X, curPos.Y) & GameMap.objType.COLLIDEABLE) == GameMap.objType.COLLIDEABLE))
                {
                    if (steps > 0)//don't check in first loop cuz it will be same wp as begining
                    {
                        //is there an waypoint?
                        Waypoint wp = wnet.getWPAt((uint)curPos.X, (uint)curPos.Y);
                        if (wp != null) { new Way(this, wp, steps, Way.eWayDir.LR); break; }
                    }
                    //not found an waypoint go to futher pos
                    if ((steps > lastStepOnPortal + 1) && ((map.getObjTypeFromPixel(curPos.X, curPos.Y) & GameMap.objType.PORTAL) == GameMap.objType.PORTAL))
                    {//current point is passing through portal, find other side pos:
                        Point newPos = map.tryGoThroughPortal(curPos);
                        if (newPos == curPos) curPos.X -= 1;
                        else
                        {
                            lastStepOnPortal = (int)steps;
                            curPos = newPos;
                        }
                    }
                    else if ((curPos.X >= 0) && (curPos.Y >= 0) && (curPos.Y < map.mapHeight) && (curPos.X < map.mapWidth))
                        curPos.X -= 1;
                    else break;//end of map reached
                    steps++;
                }

            //Direction: Right
            steps = 0; lastStepOnPortal = -1;//comeback to wp Pos
            curPos.X = x; curPos.Y = y;
            if (Right == null) //there is no way connected to up of waypoint
                while (!((map.getObjTypeFromPixel(curPos.X, curPos.Y) & GameMap.objType.COLLIDEABLE) == GameMap.objType.COLLIDEABLE))
                {
                    if (steps > 0)//don't check in first loop cuz it will be same wp as begining
                    {
                        //is there an waypoint?
                        Waypoint wp = wnet.getWPAt((uint)curPos.X, (uint)curPos.Y);
                        if (wp != null) { new Way(this, wp, steps, Way.eWayDir.RL); break; }
                    }
                    //not found an waypoint go to futher pos
                    if ((steps > lastStepOnPortal + 1) && ((map.getObjTypeFromPixel(curPos.X, curPos.Y) & GameMap.objType.PORTAL) == GameMap.objType.PORTAL))
                    {//current point is passing through portal, find other side pos:
                        Point newPos = map.tryGoThroughPortal(curPos);
                        if (newPos == curPos) curPos.X += 1;//nope, this wasn't a portal
                        else
                        {
                            lastStepOnPortal = (int)steps;
                            curPos = newPos;
                        }
                    }
                    else if ((curPos.X >= 0) && (curPos.Y >= 0) && (curPos.Y < map.mapHeight) && (curPos.X < map.mapWidth))
                        curPos.X += 1;
                    else break;//end of map reached
                    steps++;
                }

        }

        public Waypoint getWPAtDirection(MOB.eDir dir)
        {
            if (dir == MOB.eDir.U)
                return upWP;
            else if (dir == MOB.eDir.R)
                return rightWP;
            else if (dir == MOB.eDir.D)
                return downWP;
            else
                return leftWP;
        }
        public override string ToString()
        {
            return "WP" + _id.ToString() + "(" + x.ToString() + "," + y.ToString() + ")";
        }
    }
}
