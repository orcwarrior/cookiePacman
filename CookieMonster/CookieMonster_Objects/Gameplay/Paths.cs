using System;
using System.Collections.Generic;

using System.Text;

namespace CookieMonster.CookieMonster_Objects
{

    //static class for generating proper Paths visuals:
    class Paths
    {
        static Obj[] OneWay = new Obj[4];
        static Obj[] TwoWayStraight = new Obj[4];
        static Obj[] TwoWayTurn = new Obj[4];
        static Obj[] ThreeWay = new Obj[4];
        public static Obj   FourWay;
        static void Initialize()
        {
            //One Way:
            /*L*/OneWay[0] = new Obj("../data/textures/GAME/PATHS/PATH_1_L.dds", 0, 0, Obj.align.CENTER_BOTH);
            /*U*/OneWay[1] = new Obj("../data/Textures/GAME/PATHS/PATH_1_L.dds", 0, 0, Obj.align.CENTER_BOTH);
                 OneWay[1].Rotate(90f);
            
            /*R*/OneWay[2] = new Obj("../data/Textures/GAME/PATHS/PATH_1_L.dds", 0, 0, Obj.align.CENTER_BOTH);
                 OneWay[2].Rotate(180f);

            /*D*/OneWay[3] = new Obj("../data/Textures/GAME/PATHS/PATH_1_L.dds", 0, 0, Obj.align.CENTER_BOTH);
                 OneWay[3].Rotate(270f);

            //Two ways:
            /*L*/TwoWayStraight[0] = new Obj("../data/Textures/GAME/PATHS/PATH_2_LR.dds", 0, 0, Obj.align.CENTER_BOTH);
            /*U*/TwoWayStraight[1] = new Obj("../data/Textures/GAME/PATHS/PATH_2_LR.dds", 0, 0, Obj.align.CENTER_BOTH);
                 TwoWayStraight[1].Rotate(90f);

            /*R*/TwoWayStraight[2] = new Obj("../data/Textures/GAME/PATHS/PATH_2_LR.dds", 0, 0, Obj.align.CENTER_BOTH);
                 TwoWayStraight[2].Rotate(180f);

            /*D*/TwoWayStraight[3] = new Obj("../data/Textures/GAME/PATHS/PATH_2_LR.dds", 0, 0, Obj.align.CENTER_BOTH);
                 TwoWayStraight[3].Rotate(270f);

            //Two ways with turn:
            /*L*/TwoWayTurn[0] = new Obj("../data/Textures/GAME/PATHS/PATH_2_LU.dds", 0, 0, Obj.align.CENTER_BOTH);
            /*U*/TwoWayTurn[1] = new Obj("../data/Textures/GAME/PATHS/PATH_2_LU.dds", 0, 0, Obj.align.CENTER_BOTH);
                 TwoWayTurn[1].Rotate(90f);

            /*R*/TwoWayTurn[2] = new Obj("../data/Textures/GAME/PATHS/PATH_2_LU.dds", 0, 0, Obj.align.CENTER_BOTH);
                 TwoWayTurn[2].Rotate(180f);

            /*D*/TwoWayTurn[3] = new Obj("../data/Textures/GAME/PATHS/PATH_2_LU.dds", 0, 0, Obj.align.CENTER_BOTH);
                 TwoWayTurn[3].Rotate(270f);
            
            //Three Ways
            /*L*/ThreeWay[0] = new Obj("../data/Textures/GAME/PATHS/PATH_3_LUR.dds", 0, 0, Obj.align.CENTER_BOTH);
            /*U*/ThreeWay[1] = new Obj("../data/Textures/GAME/PATHS/PATH_3_LUR.dds", 0, 0, Obj.align.CENTER_BOTH);
                 ThreeWay[1].Rotate(90f);

            /*R*/ThreeWay[2] = new Obj("../data/Textures/GAME/PATHS/PATH_3_LUR.dds", 0, 0, Obj.align.CENTER_BOTH);
                 ThreeWay[2].Rotate(180f);

            /*D*/ThreeWay[3] = new Obj("../data/Textures/GAME/PATHS/PATH_3_LUR.dds", 0, 0, Obj.align.CENTER_BOTH);
                 ThreeWay[3].Rotate(270f);

            //Four Ways (only one)
                 FourWay = new Obj("../data/Textures/GAME/PATHS/PATH_4_LURD.dds", 0, 0, Obj.align.CENTER_BOTH);
        } 
        static public Obj getPathVisual(neighborReport rep)
        {
            if (Paths.FourWay == null) Paths.Initialize();
            if (rep.dontCreatePath == true) return null;//dont create any path!
            string result = "../data/Textures/GAME/PATHS/PATH_";
            result += rep.ways.ToString()+"_";
            if (rep.L) result += "L";
            if (rep.U) result += "U";
            if (rep.R) result += "R";
            if (rep.D) result += "D";
            result += ".dds"; 
            //ok, filename generated, now it's time to apply rotations,etc.
            Obj o = null;
            if (rep.ways == 4)
                o = FourWay;
            else if (rep.ways == 3)
            {
                if (rep.L)
                    if (rep.U)
                        if (rep.R)
                            o = ThreeWay[0];
                        else
                            o = ThreeWay[3];
                    else
                        o = ThreeWay[2];
                else
                    o = ThreeWay[1];
            }
            else if (rep.ways == 2)
            {
                if (rep.L)
                {
                    if (rep.R)//todo: random? 0 and 3
                    { o = TwoWayStraight[0]; }
                    else
                        if (rep.U)
                            o = TwoWayTurn[0];
                        else
                            o = TwoWayTurn[3];
                }
                else//L-false
                {
                    if(rep.R)
                    {
                        if(rep.U)
                            o = TwoWayTurn[1];
                        else
                            o = TwoWayTurn[2];
                    }
                    else o = TwoWayStraight[1];//todo: random?
                }
            }
            else
            {
                     if (rep.L)
                    o = OneWay[0];
                else if (rep.U)
                    o = OneWay[1];
                else if (rep.R)
                    o = OneWay[2];
                else
                    o = OneWay[3];
            }
            if (o == null) o = OneWay[0];
            return o;
        }
    }
}
