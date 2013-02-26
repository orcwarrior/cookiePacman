using System;
using System.Collections.Generic;
using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class Quickie : enemyRanged
    {

        private Timer delayTimer = new Timer(Timer.eUnits.MSEC, 6 * 1000, 0, true, false);
        private bool isThrowingKnife;
        public Quickie(int posx, int posy, double spd)
            : base(Enemy.enemyType.ASSASSIN, posx, posy, spd)
        {
        }
        
        public override void setVisuals()
        {
            Obj vis; int widthMul = 1;
            string texNameBase = "QUICKIE_0";
            if (currentVisual == null)
            {
                // attack:
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_attack_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                vis.width *= widthMul;
                vis.setTexAniFPS(25);
                vis.setTexAniLoopType(Obj_texAni.eLoopType.DEFAULT);
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
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_MOVE_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            //Finnaly, set those visuals:
            vis.width *= widthMul;
            vis.setTexAniFPS(16);
            setStateVisual(vis, "DEFAULT");
        }


        public override void Update()
        {
            //BUGFIX: when wizard was casting spell when
            //suddenly changed state to wounded/flee => change state from attack then
            if (isThrowingKnife && enemyState != eEnemyState.NORMAL)
            {
                stopThrowing();
            }
            if (state != eState.ATTACK)//is casting spell
            {
                if (enemyState == eEnemyState.NORMAL)
                {
                    if (!isThrowingKnife && !delayTimer.enabled && hasHeroInLineOfSight(true,12))
                    {
                        doKnifeSwing();//put "ATTACK" state on, wait till it hit last frame
                    }
                    else
                        base.Update();
                }
                else
                {
                    base.Update();
                }
            }//IN attack state:
            else if (isThrowingKnife == true && currentVisual.texAniFinished())//was casting spell when chargeTimer wen't
            {   // off, it means it's time to realease Projectile!
                throwKnife();
                base.Update();
            }

            base.Update();//so when wizzard is casting spell it will be still moving, remove it if you don't want to
        }

        private void stopThrowing()
        {
            isThrowingKnife = false;
            delayTimer.start();
            state = eState.DEFAULT;
        }

        private void doKnifeSwing()
        {
            isThrowingKnife = true;
            state = eState.ATTACK;
            currentVisual.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
        }

        private void throwKnife()
        {
            isThrowingKnife = false;
            delayTimer.start();
            new Projectile(Projectile.eProjType.Knife, this, this.pX, this.pY, 70.0);
            currentVisual.setTexAniLoopType(Obj_texAni.eLoopType.DEFAULT);
            state = eState.DEFAULT;
        }

    }
}
