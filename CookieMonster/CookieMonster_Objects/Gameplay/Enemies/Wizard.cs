using System;
using System.Collections.Generic;

using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class Wizard : enemyRanged
    {
        private Timer delayTimer = new Timer(Timer.eUnits.MSEC, 9 * 1000, 0, true, false);
        private Sound chargingSFX;
        private bool isCastingSpell;
        public Wizard(int posx, int posy, double spd) : base(Enemy.enemyType.WIZARD,posx,posy,spd)
        {
        }
        public override void setVisuals()
        {
            Obj vis; int widthMul = 1;
            string texNameBase = "wizard_0";
            if (currentVisual == null)
            {
                // attack:
                vis = new Obj("../data/Textures/GAME/MOB/" + texNameBase + "_attack_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
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

        public override void Update()
        {
            //BUGFIX: when wizard was casting spell when
            //suddenly changed state to wounded/flee => change state from attack then
            if (isCastingSpell && enemyState != eEnemyState.NORMAL)
            {
                CancelSpell();
            }
            if (state != eState.ATTACK)//is casting spell
            {
                if (enemyState == eEnemyState.NORMAL)
                {
                    if (!isCastingSpell && !delayTimer.enabled && hasHeroInLineOfSight(true,-1))
                    {
                        prepareSpell();
                    }
                    else
                        base.Update();
                }
                else
                {
                    base.Update();
                }
            }//IN attack state:
            else if (isCastingSpell == true && currentVisual.texAniFinished())//was casting spell when chargeTimer wen't
            {   // off, it means it's time to realease Projectile!
                castSpell();
                base.Update();
            }

            base.Update();//so when wizzard is casting spell it will be still moving, remove it if you don't want to
        }

        private void CancelSpell()
        {
            isCastingSpell = false;
            delayTimer.start();
            state = eState.DEFAULT;
            chargingSFX.Stop();
        }

        private void prepareSpell()
        {
            //MOB state to attack
            //chargeTimer.start();
            isCastingSpell = true;
            state = eState.ATTACK;
            chargingSFX = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_WIZARD_CHARGING.ogg", false, true, 0.72);
            
        }

        private void castSpell()
        {
            isCastingSpell = false;
            delayTimer.start();
            new Projectile(Projectile.eProjType.IceBolt, this, this.pX, this.pY, Projectile.iceBoltSpeed);
            state = eState.DEFAULT;
        }
    }
}
