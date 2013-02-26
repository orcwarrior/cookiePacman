using System;
using System.Collections.Generic;
using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class enemyRanged : Enemy
    {
        public enemyRanged(Enemy.enemyType et, int posx, int posy, double spd)
            : base(et, posx, posy, spd)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="checkCollisions">Check if there is some collideable objects on line of sight</param>
        /// <param name="range">Max. distance to player in gridpoints.</param>
        /// <returns></returns>
        protected bool hasHeroInLineOfSight(bool checkCollisions,int range)
        {
            if (gridY == GameMan.PC.gridY)
            { // in line horizontaly (left right)
                if (direction == eDir.L && gridX >= GameMan.PC.gridX)
                {   // hero is in front of object, but we need to check if there is no obstacles between them.
                    if(gridX+range < GameMan.PC.gridX) return false; // PC out of range
                    if(!checkCollisions) return true;                // don't check for colliding object.
                    for (int i = gridX - 1; i > GameMan.PC.gridX; i--)
                    {
                        if ((GameMan.Map.getObjTypeFromPixel(i, gridY) & GameMap.objType.COLLIDEABLE) == GameMap.objType.COLLIDEABLE)
                            return false; // obstacle on way, can't see player
                    }
                    return true;
                }
                else if (direction == eDir.R && gridX <= GameMan.PC.gridX)
                {
                    if (gridX + range > GameMan.PC.gridX) return false; // PC out of range
                    if (!checkCollisions) return true;                  // don't check for colliding object.
                    for (int i = gridX + 1; i < GameMan.PC.gridX; i++)
                    {
                        if (((GameMan.Map.getObjTypeFromPixel(i, gridY) & GameMap.objType.COLLIDEABLE) == GameMap.objType.COLLIDEABLE))
                            return false; // obstacle on way, can't see player
                    }
                    return true;
                }
            }
            else if (gridX == GameMan.PC.gridX)
            {// in line verticaly
                // in line horizontaly (left right)
                if (direction == eDir.U && gridY >= GameMan.PC.gridY)
                {   // hero is in front of wizard, but we need to check if there is no obstacles between them.
                    if (gridY + range < GameMan.PC.gridX) return false; // PC out of range
                    if (!checkCollisions) return true;                  // don't check for colliding object.
                    for (int i = gridY - 1; i > GameMan.PC.gridY; i--)
                    {
                        if ((GameMan.Map.getObjTypeFromPixel(gridX, i) & GameMap.objType.COLLIDEABLE) == GameMap.objType.COLLIDEABLE)
                            return false; // obstacle on way, can't see player
                    }
                    return true;
                }
                else if (direction == eDir.D && gridY <= GameMan.PC.gridY)
                {
                    if (gridY + range > GameMan.PC.gridX) return false; // PC out of range
                    if (!checkCollisions) return true;                  // don't check for colliding object.
                    for (int i = gridY + 1; i < GameMan.PC.gridX; i++)
                    {
                        if ((GameMan.Map.getObjTypeFromPixel(gridX, i) & GameMap.objType.COLLIDEABLE) == GameMap.objType.COLLIDEABLE)
                            return false; // obstacle on way, can't see player
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
