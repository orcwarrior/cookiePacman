using System;
using System.Collections.Generic;

using System.Text;
using System.Diagnostics;

namespace CookieMonster.CookieMonster_Objects
{
    class Bomb : MOB
    {
        static Obj bombVisual = new Obj("../data/textures/game/sob/Bomb_Explosion_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
        Stopwatch tickTock;
        const int frameMSTime = 100;
        Timer lastFrameTimer;

        public Bomb(int x,int y) : base(x,y,0)
        {
            GameMan.addBomb(this);
            base.setStateVisual(bombVisual, "DEFAULT");
            base.currentVisual.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
            base.currentVisual.x = x;
            base.currentVisual.y = y;

            tickTock = new Stopwatch();
            tickTock.Start();
            currentVisual.setTexAniControlledExternal();

            Sound bomb = new Sound(Sound.eSndType.SFX, "../data/Sounds/BOMB.ogg", false, false);
            bomb.Play();
        }
        public void Update()
        {
            int frame = (int)(tickTock.ElapsedMilliseconds / frameMSTime);
            currentVisual.setTexAniFrame(frame);
            if ((frame == 51)&&(lastFrameTimer==null))
            {
                lastFrameTimer = new Timer(Timer.eUnits.MSEC, 300, 0);
                lastFrameTimer.start();
            }
            if(lastFrameTimer!=null)
                if (lastFrameTimer.enabled == false)
                {
                    lastFrameTimer.Dispose();
                    Explode();
                }
            
        }
        public void prepareRender()
        {
            base.Render();
        }
        public void Explode()
        { //TODO: Implement rest (destructing near objects)
            GameMan.bombExplode(this);
            GameMan.removeBomb(this);
        }
    }
}
