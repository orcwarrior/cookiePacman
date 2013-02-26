using System;
using System.Collections.Generic;
using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class Lazer : enemyRanged
    {
        private bool charginMyLazor,firinMyLazor;
        private Timer lazorCooldown = new Timer(Timer.eUnits.MSEC, 10 * 1000, 0, true, false);
        private Timer lazorHold = new Timer(Timer.eUnits.MSEC, 3 * 1000 + 400, 0, true, false);
        private Sound chargingSFX;
        private Projectile myLazor;
        public Lazer(int posx, int posy, double spd)
            : base(Enemy.enemyType.SHOOPDAWOOP, posx, posy, spd)
        {

        }
        
        public override void setVisuals()
        {
            Obj vis; int widthMul = 1;
            string texNameBase = "LAZER_0";
            if (currentVisual == null)
            {
                // attack:
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_attack_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
                vis.width *= widthMul;
                vis.setTexAniFPS(12);
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
            vis.setTexAniFPS(20);
            setStateVisual(vis, "DEFAULT");
        }

        public override void Update()
        {
            //BUGFIX: when wizard was casting spell when
            //suddenly changed state to wounded/flee => change state from attack then
            if (charginMyLazor && enemyState != eEnemyState.NORMAL)
            {
                stopLazor();
            }
            if (state != eState.ATTACK)//is casting spell
            {
                if (enemyState == eEnemyState.NORMAL)
                {
                    if (!charginMyLazor && !lazorCooldown.enabled && hasHeroInLineOfSight(false,9))
                    {
                        chargeLazor();
                    }
                    else
                        base.Update();
                }
                else
                {
                    base.Update();
                }
            }//IN attack state:
            else if (charginMyLazor == true && currentVisual.texAniFinished())//was casting spell when chargeTimer wen't
            {   // off, it means it's time to realease Projectile!
                fireDaLazor();
                //base.Update();
            }
            else if (firinMyLazor == true && lazorHold.enabled == false)
            {
                stopFirinDaLazor();
            }
            //When lazor is chargin shoop da whoop stands still
            //base.Update();
        }

        private void stopLazor()
        {
            charginMyLazor = false;
            lazorCooldown.start();
            state = eState.DEFAULT;
            chargingSFX.Stop();
        }

        private void chargeLazor()
        {
            //MOB state to attack
            //chargeTimer.start();
            charginMyLazor = true;
            state = eState.ATTACK;
            chargingSFX = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_LAZER_PRE.ogg", false, true, 0.95);
        }

        private void fireDaLazor()
        {
            charginMyLazor = false;
            firinMyLazor = true;
            lazorHold.start();
            myLazor = new Projectile(Projectile.eProjType.Lazer, this, this.pX, this.pY, 0.0);
            //state = eState.DEFAULT;
        }
        private void stopFirinDaLazor()
        {
            state = eState.DEFAULT;
            firinMyLazor = false;
            lazorCooldown.start();
            myLazor.Destroy();
        }
    }
}
