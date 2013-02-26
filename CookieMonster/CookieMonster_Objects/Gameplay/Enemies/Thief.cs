using System;
using System.Collections.Generic;

using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class Thief : Enemy
    {
        public uint stolenCookies { get; private set; }
        private PowerUp lastStolenCookie;
        public Thief(int posx, int posy, double spd) : base(Enemy.enemyType.THIEF ,posx,posy,spd)
        {
            myCollision.cdEvent += new CollisionReport.CDEvtHandler(thiefDynamicCollison);
        }
        public override void setVisuals()
        {
            Obj vis; int widthMul = 1;
            string texNameBase = "thief_0";
            if (currentVisual == null)
            {
                // attack:
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_STEAL_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                vis.width *= widthMul;
                vis.setTexAniFPS(20);
                vis.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                setStateVisual(vis, "ATTACK");
                // frozen:
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_frozen_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                setStateVisual(vis, "FROZEN");
                // dust:
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_dust_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                vis.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                setStateVisual(vis, "DUST");
            }
            else
            {
                if (mobMirrored == true)
                {
                    widthMul = -1;
                }
            }
            // default:
            if (enemyState == eEnemyState.FLEE)
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_scared_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            else if (enemyState == eEnemyState.WOUNDED)
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_wounded_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            else/*enemy state DEFAULT/NORMAL*/
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_move_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            //Finnaly, set those visuals:
            vis.width *= widthMul;
            setStateVisual(vis, "DEFAULT");
        }
        private void thiefDynamicCollison(MOB src, collDynEventArgs fe)
        {
            Sound pickup = new Sound(Sound.eSndType.SFX, "../data/Sounds/PICKUP.ogg", false, false);
            pickup.volume = 0.8;

            if (fe.cdPowerUP != null)
            {
                if (fe.cdPowerUP.type == POWER_UP.COOKIE)
                {
                    stealCookie(fe);
                }
            }  
        }
        public override void Update()
        {
            if (state == eState.ATTACK && currentVisual.texAniFinished())
                stealCookieFinish();

            base.Update();
        }

        private void stealCookie(collDynEventArgs fe)
        {
            if (lastStolenCookie != null) return;
            if (state != eState.DEFAULT || enemyState != eEnemyState.NORMAL) return;
            state = eState.ATTACK;
            lastStolenCookie = fe.cdPowerUP;
        }
        private void stealCookieFinish()
        {
            if (lastStolenCookie == null) return;
            state = eState.DEFAULT;
            GameMan.removePowerUp(lastStolenCookie);
            stolenCookies++;
            lastStolenCookie = null;
        }

        internal void giveBackCookies()
        {
            GameMan.statistics.addEatenCookies(stolenCookies);
            stolenCookies = 0;
        }
    }
}
