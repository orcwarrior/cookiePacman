using System;
using System.Collections.Generic;
using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    // static helper class for generating Cliff visuals
    // Whole class is based and very-alike class Paths
    class Cliffs
    {
        static Obj[] StraightCliff = new Obj[4];
        static Obj[] CornerCliff   = new Obj[4];
        static void Initialize()
        {
            //One Way:
            /*L*/
            StraightCliff[0] = new Obj("../data/textures/GAME/SOB/CLIFF_STRAIGHT_V0.dds", 0, 0, Obj.align.CENTER_BOTH);
            /*U*/
            StraightCliff[1] = new Obj("../data/Textures/GAME/SOB/CLIFF_STRAIGHT_V0.dds", 0, 0, Obj.align.CENTER_BOTH);
            StraightCliff[1].Rotate(90f);

            /*R*/
            StraightCliff[2] = new Obj("../data/Textures/GAME/SOB/CLIFF_STRAIGHT_V0.dds", 0, 0, Obj.align.CENTER_BOTH);
            StraightCliff[2].Rotate(180f);

            /*D*/
            StraightCliff[3] = new Obj("../data/Textures/GAME/SOB/CLIFF_STRAIGHT_V0.dds", 0, 0, Obj.align.CENTER_BOTH);
            StraightCliff[3].Rotate(270f);

            //CORNERS
            /*L*/
            CornerCliff[0] = new Obj("../data/Textures/GAME/SOB/CLIFF_CORNER_V0.dds", 0, 0, Obj.align.CENTER_BOTH);
            /*U*/
            CornerCliff[1] = new Obj("../data/Textures/GAME/SOB/CLIFF_CORNER_V0.dds", 0, 0, Obj.align.CENTER_BOTH);
            CornerCliff[1].Rotate(90f);

            /*R*/
            CornerCliff[2] = new Obj("../data/Textures/GAME/SOB/CLIFF_CORNER_V0.dds", 0, 0, Obj.align.CENTER_BOTH);
            CornerCliff[2].Rotate(180f);

            /*D*/
            CornerCliff[3] = new Obj("../data/Textures/GAME/SOB/CLIFF_CORNER_V0.dds", 0, 0, Obj.align.CENTER_BOTH);
            CornerCliff[3].Rotate(270f);
        }

        static public Obj getCliffVisual(neighborReport rep)
        {
            if (Cliffs.CornerCliff[0] == null) Cliffs.Initialize();
            Obj o = null;
            neighborReport dRep = new neighborReport(rep.x, rep.y,GameMap.objType.DARKNESS);

            // straight = always 3 neighbours marked as cliff (only in proper clifs with size min. 2 pixels
            int cliffsOnSides = 4 - rep.ways;//rep.ways keep non-cliffs neighbours
            cliffsOnSides += 4 - dRep.ways;
            if (cliffsOnSides == 3)//straight:
            {
                if ((rep.U == true)&&(dRep.U == true))//horizontonal
                {
                    o = StraightCliff[0];
                }
                else if ((rep.R == true)&&(dRep.R == true))
                {
                    o = StraightCliff[1];
                }
                else if ((rep.D == true)&&(dRep.D == true))
                {
                    o = StraightCliff[2];
                }
                else if ((rep.L == true)&&(dRep.L == true))//veritical
                {
                    o = StraightCliff[3];
                }
            }
            else if (cliffsOnSides != 1)//one cliff on sides means something went wrong
            {//it's got to be corner cliff
                int x = rep.x,y= rep.y;
                if (((neighborReport.map.getObjTypeFromPixel(x - 1, y) & GameMap.objType.CLIFF) == GameMap.objType.CLIFF)
                && ((neighborReport.map.getObjTypeFromPixel(x, y + 1) & GameMap.objType.CLIFF) == GameMap.objType.CLIFF))
                    o = CornerCliff[0];
                else if (((neighborReport.map.getObjTypeFromPixel(x, y - 1) & GameMap.objType.CLIFF) == GameMap.objType.CLIFF)
                     && ((neighborReport.map.getObjTypeFromPixel(x - 1, y) & GameMap.objType.CLIFF) == GameMap.objType.CLIFF))
                    o = CornerCliff[1];
                else if (((neighborReport.map.getObjTypeFromPixel(x, y - 1) & GameMap.objType.CLIFF) == GameMap.objType.CLIFF)
                     && ((neighborReport.map.getObjTypeFromPixel(x + 1, y) & GameMap.objType.CLIFF) == GameMap.objType.CLIFF))
                    o = CornerCliff[2];
                else if (((neighborReport.map.getObjTypeFromPixel(x, y + 1) & GameMap.objType.CLIFF) == GameMap.objType.CLIFF)
                     && ((neighborReport.map.getObjTypeFromPixel(x + 1, y) & GameMap.objType.CLIFF) == GameMap.objType.CLIFF))
                    o = CornerCliff[3];
            }
            return o;
        }
    }
}
