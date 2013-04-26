using System;
using System.Collections.Generic;

using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    [Flags]
    enum POWER_UP
    {
        COOKIE,//just some cookie to eat
        POWER_PELLET,//classic pac-man powerup
        LIFE,
        BOMB,
        ENEMY_SLOWER,
        SKILL_POINT,
        INSTANT_STONE
    }
    class PowerUp : MOB
    {
        static public GameManager GameMan;
        static private int yShift = +6;

        private POWER_UP _type; public POWER_UP type { get { return _type; } }
        public PowerUp(int posx, int posy,String visual)
            : base(posx, posy + yShift, 0)
        { 
            base.setVisualsLayer(Layer.imgFG2);
            base.setStateVisual(new Obj(visual, posx, posy,Obj.align.CENTER_BOTH), "DEFAULT");
            
            base.Move(- base.currentVisual.width / 2,-base.currentVisual.height / 2,false); 
            GameMan.addPowerUP(this);
            base.updateBoundingBox();//for objects without movement, so in future bbox willn't be 
            //calculated anymore
            if(visual==GameMap.PATH_PU_COOKIE)
                    _type = POWER_UP.COOKIE;
            else if(visual==GameMap.PATH_PU_POWERPILL)
                    _type = POWER_UP.POWER_PELLET; 
            else if(visual==GameMap.PATH_PU_BOMB)
                    _type = POWER_UP.BOMB;
            else if (visual == GameMap.PATH_PU_LIFE)
            {
                _type = POWER_UP.LIFE;
                currentVisual.setTexAniFPS(15);
            }
            else if (visual == GameMap.PATH_PU_SKILLPOINT)
            {
                _type = POWER_UP.SKILL_POINT;
                currentVisual.setTexAniFPS(10);
            }
            else if (visual == GameMap.PATH_PU_ENEMYSLOWER)
                _type = POWER_UP.ENEMY_SLOWER;

            mobType = eMOBType.POWERUP;
        }
        public void Update()
        {
           // base.Update();
        }
        public void Render()
        {
            base.prepareRender();
        }
        public void Remove()
        {
            GameMan.removePowerUp(this);
        }
    }
}
