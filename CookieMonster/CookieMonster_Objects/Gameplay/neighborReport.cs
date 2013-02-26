using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    class neighborReport
    {
        static public GameMap map;
        public GameMap.objType Up, Right, Down, Left;
        public bool U, R, D, L;//if true there is a way at this side
        public int ways;
        public bool dontCreatePath;
        public bool createWaypoint;
        public GameMap.objType checkedType;
        private int _x, _y; public int x { get { return _x; } }
                            public int y { get { return _y; } }
        public neighborReport(int x, int y)
            : this(x, y, GameMap.objType.COLLIDEABLE)
        {
            //collideable is threaten as default report type
        }
        public neighborReport(int x, int y, GameMap.objType reportedType)//x,y are grid(bitmap) pos!
        {
            _x = x; _y = y;
            Point pU = new Point(x, y - 1), pR = new Point(x + 1, y), pD, pL;
            dontCreatePath = false;
            ways = 0;
            int gs = GameManager.gridSize;
            Up = map.getObjTypeFromPixel(x, y - 1);
            Right = map.getObjTypeFromPixel(x + 1, y);
            Down = map.getObjTypeFromPixel(x, y + 1);
            Left = map.getObjTypeFromPixel(x - 1, y);
            if ((Up & reportedType) != reportedType)
            { U = true; ways++; }
            if ((Right & reportedType) != reportedType)
            { R = true; ways++; }
            if ((Down & reportedType) != reportedType)
            { D = true; ways++; }
            if ((Left & reportedType) != reportedType)
            { L = true; ways++; }

            // Create waypoint there?
            // bugfix: don't insert waypoint on portal pixels:
            if ((map.getObjTypeFromPixel(x, y) & GameMap.objType.PORTAL) == GameMap.objType.PORTAL)
                createWaypoint = false;
            else if ((ways == 2) && (((R == true) && (L == true)) || ((U == true) && (D == true))))
            {
                createWaypoint = false;
            }
            else
                createWaypoint = true;
            if (ways == 4)//check if there is nothing surrounding this pixel, if true, just don't create any path tex!
            {
                /* So we got X's:  O X O  We need "O"'s too, let's grab em
                 *                 X . X
                 *                 O X O 
                 */
                int additionalWays = 0;
                if ((map.getObjTypeFromPixel(x - 1, y - 1) & reportedType) != reportedType)
                    additionalWays++;
                if ((map.getObjTypeFromPixel(x + 1, y - 1) & reportedType) != reportedType)
                    additionalWays++;
                if ((map.getObjTypeFromPixel(x - 1, y + 1) & reportedType) != reportedType)
                    additionalWays++;
                if ((map.getObjTypeFromPixel(x + 1, y + 1) & reportedType) != reportedType)
                    additionalWays++;
                if (additionalWays > 0)
                    dontCreatePath = true;
            }
            else if (ways == 3)//need special threatment:
            {//we will check two point (laying between paths) what i mean by this:
                /* O X O  So we will check both O's if they're uncollidable
                 * X . X  we won't render path, elsewhere path need to be rendered
                 * - - -
                 */
                int pointsOk = 0;
                int px = -1, py = -1;
                if (R == true)
                    px = x + 1;
                if (U == true)
                    py = y - 1;
                if ((px != -1) && (py != -1))//already one point to chceck
                    if ((map.getObjTypeFromPixel(px, py) & reportedType) != reportedType)
                        pointsOk++;
                    else
                        return;//one of point's is collideable, futher checking is waste of CPU
                if (L == true)
                {
                    if (pointsOk == 1)//R was true too, one point already checked this will be second and last
                    {
                        px = x - 1;
                        if ((map.getObjTypeFromPixel(px, py) & reportedType) != reportedType)
                        { pointsOk++; dontCreatePath = true; }
                        else
                            return;//unfortunately secont point isn't uncollideable, but hey, we have new path :)
                    }
                    else///so U was false => D got2be true checking R & D and L & D then:
                    {
                        if (D == false) return; //safety is always welcome ;)
                        py = y + 1;
                        if ((map.getObjTypeFromPixel(px, py) & reportedType) != reportedType)
                            pointsOk++;
                        else
                            return;//one of point's is collideable, futher checking is waste of CPU
                        px = x - 1;
                        if ((map.getObjTypeFromPixel(px, py) & reportedType) != reportedType)
                        { pointsOk++; dontCreatePath = true; }
                        else
                            return;//unfortunately secont point isn't uncollideable, but hey, we have new path :)
                    }
                    if (pointsOk == 2) dontCreatePath = true;
                    return;
                }
                if (D == true)
                {// we are here so L was false,then R and U are true and checked, so right now 
                    // we need to make last check and see what we got here:
                    py = y + 1;
                    if ((map.getObjTypeFromPixel(px, py) & reportedType) != reportedType)
                    { pointsOk++; dontCreatePath = true; }
                    else
                        return;//unfortunately secont point isn't uncollideable, but hey, we have new path :)
                }
            }
            else if (ways == 2)//need special threatment:
            {//we will check only one point, but first we need to make sure which one ...
                int px = -1, py = -1;
                if (R == true)
                    px = x + 1;
                else if (L == true)
                    px = x - 1;
                if (U == true)
                    py = y - 1;
                else if (D == true)
                    py = y + 1;
                if ((px == -1) || (py == -1))
                    return;//it's an straight path not curved!!!
                if ((map.getObjTypeFromPixel(px, py) & reportedType) != reportedType)
                    dontCreatePath = true;
            }

        }
    }
}
