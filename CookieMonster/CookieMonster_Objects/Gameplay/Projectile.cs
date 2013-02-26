using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    class Projectile : MOB
    {
        //Ice Bolt:
        static Obj iceBoltVisuals;
        static Obj iceBoltDestroy;
        static Obj iceBoltVisualsSecond;
        static Obj lastUsedIceBoltVisual;

        //Lazer:
        static Obj lazerVisuals;
        static Obj lazerVisualsSecond;
        static Obj lastUsedLazerVisual;

        //Knife:
        static Obj knifeVisuals;
        static Obj knifeVisualsSecond;
        static Obj lastUsedKnifeVisual;
        static Obj knifeDestroy;

        //Projectiles attributes:
        static public float iceBoltSpeed = 75f;

        static bool lazerStaticVisualsInited = false;
        static bool iceBoltStaticVisualsInited = false;
        public MOB owner { get; private set; }
        public enum eProjType { IceBolt, Lazer, Knife }
        public eProjType type {get; private set;}
        public bool isBeingDestroyed { get; private set; }
        #region Overriden_Properties
        public override int pX
        {
            get
            {
                Point cent = new Point(currentVisual.width / 2,
                                       currentVisual.height / 2);
                int retVal = base.pX;
                switch (direction)
                {
                    case eDir.U: return retVal += GameManager.gridSize / 2; break;
                    case eDir.D: return retVal += GameManager.gridSize / 2; break;
                    case eDir.L: return retVal += GameManager.gridSize - (cent.X + currentVisual.height / 2); break;
                    case eDir.R: return retVal += GameManager.gridSize / 2; break;
                    default: break;
                }
                //retVal -= GameManager.gridSize;
                return retVal;
            }
        }

        public override int pY
        {
            get
            {
                Point cent = new Point(+currentVisual.width / 2,
                                        +currentVisual.height / 2);
                int retVal = base.pY;
                switch (direction)
                {
                    case eDir.U: retVal += 0; break;
                    case eDir.D: retVal += GameManager.gridSize / 2; break;
                    case eDir.L: retVal += GameManager.gridSize / 2; break;
                    case eDir.R: retVal += GameManager.gridSize / 2; break;
                    default: break;
                }
                //retVal -= GameManager.gridSize/2;
                return retVal;

            }
        }
        /// <summary>
        /// Well, it sucks but object needs diferent position for rendering of Obj
        /// than Projectile actually position is. It's all because of
        /// rotating of Obj for direction aplied at initing of Projectile.
        /// </summary>
        public int renderX
        {
            get
            {
                Point cent = new Point(+currentVisual.width / 2,
                                        +currentVisual.height / 2);
                int retVal = pX;
                switch (direction)
                {
                    case eDir.U: retVal += -cent.X; break;
                    case eDir.D: retVal += -cent.X; break;
                    case eDir.L: retVal += GameManager.gridSize/2; break;
                    case eDir.R: retVal += -cent.Y - GameManager.gridSize/2; break;
                    default: break;
                }
                if (type == eProjType.Lazer)
                    switch (direction)
                    {
                        case eDir.U: retVal += 10; break;
                        case eDir.D: retVal += 0; break;
                        case eDir.L: retVal += -currentVisual.width - cent.X; break;
                        case eDir.R: retVal += GameManager.gridSize-10; break;
                        default: break;
                    }
                if (type == eProjType.Knife)
                    switch (direction)
                    {
                        case eDir.U: retVal += 0; break;
                        case eDir.D: retVal += 0; break;
                        case eDir.L: retVal += -currentVisual.width; break;
                        case eDir.R: retVal += 0; break;
                        default: break;
                    }
                return retVal;

            }
        }

        public int renderY
        {
            get
            {
                Point cent = new Point(+currentVisual.width / 2,
                                        +currentVisual.height / 2);
                int retVal = pY;
                switch (direction)
                {
                    case eDir.U: retVal += -GameManager.gridSize; break;
                    case eDir.D: retVal += -currentVisual.height + GameManager.gridSize; break;
                    case eDir.L: retVal += -cent.Y; break;
                    case eDir.R: retVal += -cent.Y; break;
                    default: break;
                }
                if (type == eProjType.Lazer)
                    switch (direction)
                    {
                        case eDir.U: retVal += -cent.X + GameManager.gridSize/2; break;
                        case eDir.D: retVal += cent.X - GameManager.gridSize/2; break;
                        case eDir.L: retVal += 10; break;
                        case eDir.R: retVal += 10; break;
                        default: break;
                    }
                return retVal;

            }
        }
        public int gridX
        {
            get
            {
                return base.gridX;
            }
        }

        public int gridY
        {
            get
            {
                return base.gridY;
            }
        }

        public override int nextGridX
        {
            get { return gridX; }
        }
        public override int nextGridY
        {
            get { return gridY; }
        }
        #endregion


        public Projectile(eProjType typ, MOB _owner, int posx, int posy, double spd)
            : base(posx , posy, spd) 
        {
            type = typ;
            owner = _owner;
            mobType = eMOBType.PROJECTILE;
            direction = owner.direction;
            base.myCollision = new CollisionReport(this);
            myCollision.cdEvent += new CollisionReport.CDEvtHandler(projectileDynamicCollision);
            if (typ == eProjType.IceBolt)
                InitIceBolt();
            else if (typ == eProjType.Lazer)
                InitLazer();
            else if (typ == eProjType.Knife)
                InitKnife();
            //add projectile to proj. list:
            GameMan.projectilesList.Add(this);
            mobType = eMOBType.PROJECTILE;
        }


        static public void projectilesVisualsInit()
        {
            //Ice Bolt
            iceBoltVisuals = new Obj("../data/Textures/Game/FX/ICEBOLT_A0.dds", 0, 0, Obj.align.CENTER_X);
            iceBoltDestroy = new Obj("../data/Textures/Game/FX/ICEBOLT_EXPLODE_A0.dds", 0, 0, Obj.align.CENTER_X);
            iceBoltVisualsSecond = new Obj("../data/Textures/Game/FX/ICEBOLT_A0.dds", 0, 0, Obj.align.CENTER_X);
        
            //Lazer:
            lazerVisuals       = new Obj("../data/Textures/Game/FX/FX_LAZER_A0.dds",0,0,Obj.align.LEFT);
            lazerVisualsSecond = new Obj("../data/Textures/Game/FX/FX_LAZER_A0.dds", 0, 0, Obj.align.LEFT);

            //Knife:
            knifeVisuals = new Obj("../data/Textures/Game/FX/THROWING_KNIFE.dds", 0, 0, Obj.align.CENTER_X);
            knifeVisualsSecond = new Obj("../data/Textures/Game/FX/THROWING_KNIFE.dds", 0, 0, Obj.align.CENTER_X);
            knifeDestroy = new Obj("../data/Textures/Game/FX/SWORD_EXPLODE_A0.dds", 0, 0, Obj.align.CENTER_X);
        }
        private void InitLazer()
        {
            if (!lazerStaticVisualsInited)
            {
                lazerVisuals.setTexAniLoopType(Obj_texAni.eLoopType.DEFAULT);
                lazerVisuals.setTexAniFPS(20);
                lazerVisualsSecond.setTexAniLoopType(Obj_texAni.eLoopType.DEFAULT);
                lazerVisualsSecond.setTexAniFPS(20);

                lazerStaticVisualsInited = true;
            }
            if (lastUsedLazerVisual != lazerVisuals)
            {
                setStateVisual(lazerVisuals, "DEFAULT");
                lastUsedLazerVisual = lazerVisuals;
            }
            else
            {
                setStateVisual(lazerVisualsSecond, "DEFAULT");
                lastUsedLazerVisual = lazerVisualsSecond;
            }
            currentVisual.setTexAniFrame(0);
            Obj vis = currentVisual;
            int visWAbs = Math.Abs(vis.width);

            //Rotate visual:
            if (direction == eDir.L)
            {
               // Move(-visWAbs / 2 - visWAbs / 3, 4, false);
                currentVisual.Rotate(180);
            }
            else if (direction == eDir.D)
            {
               // Move(-visWAbs / 3 - visWAbs / 26, +(visWAbs / 3) +visWAbs / 10, false);
                currentVisual.Rotate(90);
            }
            else if (direction == eDir.R)
            {
               // Move(+visWAbs / 14, 4, false);
                currentVisual.Rotate(0);
            }
            else if (direction == eDir.U)
            {
               // Move(-visWAbs / 3 - visWAbs/28, -(visWAbs / 3) -visWAbs/10, false);
                currentVisual.Rotate(270);
            }
            //Play cast sound:
            Sound cast = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_LAZER_SHOOT.ogg",false,true,1.0);
            //Set Bbox:
            updateBoundingBox();

            createLazorBeamLight();
            
        }

        private void createLazorBeamLight()
        {
            // Create dynamic Light for lazer:
            Light beam = new Light(eLightType.DYNAMIC, new radialGradient(new OpenTK.Vector2(0, 0), 200f,
                                  new OpenTK.Vector4(0.2f, 0.5f, 1f, 1f),
                                  new OpenTK.Vector4(0, 0f, 0.4f, 0f)));
            beam.scaleLightAni[0] = 1f;
            beam.scaleLightAni[1] = 0.65f;
            beam.scaleLightAni.aniFps = 0.6f;
            beam.scaleLightAni.aniType = eLightAniType.pingpong;
            //color:
            beam.colorLightAni[0] = new lightColors(Color.FromArgb(255, 120, 179, 255),
                                                     Color.FromArgb(22, 0, 100, 255));
            beam.colorLightAni[1] = new lightColors(Color.FromArgb(175, 120, 179, 245),
                                                     Color.FromArgb(0, 0, 50, 205));
            beam.colorLightAni[2] = new lightColors(Color.FromArgb(245, 150, 150, 255),
                                                     Color.FromArgb(9, 0, 205, 205));
            beam.colorLightAni[3] = new lightColors(Color.FromArgb(187, 100, 120, 245),
                                                     Color.FromArgb(0, 0, 20, 225));
            beam.colorLightAni.aniFps = 0.43f;
            beam.colorLightAni.shuffle();

            //position:
            int xMul = 0, yMul = 0;
            int len = (int)currentVisual.width;
            if (direction == eDir.L) xMul = -1;
            else if (direction == eDir.R) xMul = 1;
            else if (direction == eDir.U) yMul = -1;
            else if (direction == eDir.D) yMul = 1;
            beam.posLightAni[0] = new Point();
            beam.posLightAni[1] = new Point((int)(1 * xMul * len), (int)(1 * yMul * len));
            beam.posLightAni.aniFps = 0.3f;
            beam.posLightAni.aniType = eLightAniType.loopStep2First;
            attachLight(beam);

            // Light at the begining of the beam:
            beam = new Light(eLightType.DYNAMIC, new radialGradient(new OpenTK.Vector2(0, 0), 120f,
                                 new OpenTK.Vector4(0.2f, 0.5f, 1f, 1f),
                                 new OpenTK.Vector4(0, 0f, 0.4f, 0f)));
            beam.scaleLightAni[0] = 1f;
            beam.scaleLightAni[1] = 1.45f;
            beam.scaleLightAni.aniFps = 2f;
            beam.scaleLightAni.aniType = eLightAniType.pingpong;
            //color:
            beam.colorLightAni[0] = new lightColors(Color.FromArgb(255, 120, 179, 255),
                                                     Color.FromArgb(22, 0, 100, 255));
            beam.colorLightAni[1] = new lightColors(Color.FromArgb(175, 120, 179, 245),
                                                     Color.FromArgb(0, 0, 50, 205));
            beam.colorLightAni[2] = new lightColors(Color.FromArgb(245, 150, 150, 255),
                                                     Color.FromArgb(9, 0, 205, 205));
            beam.colorLightAni[3] = new lightColors(Color.FromArgb(187, 100, 120, 245),
                                                     Color.FromArgb(0, 0, 20, 225));
            beam.colorLightAni.aniFps = 1.25f;
            beam.colorLightAni.shuffle();
            beam.slicesNum(12);
            attachLight(beam);
        }
        public override void updateBoundingBox()
        {
           Rectangle res;
           if (type == Projectile.eProjType.Lazer)
           {
               res = new Rectangle(renderX, renderY, currentVisual.width, currentVisual.height);

               if (res.Height < res.Width)
               {
                   int h = res.Height;
                   res.Height -= h / 4;
                   res.Y += h / 4;
               }
               else
               {
                   int w = res.Width;
                   res.Width -= w / 4;
                   res.X += w / 4;
               }
               updateBoundingBox(res.X, res.Y, res.Width, res.Height);
           }
           else
               base.updateBoundingBox();
        }
        private void InitIceBolt()
        {
            if (!iceBoltStaticVisualsInited)
            {
                iceBoltDestroy.ScaleAbs = 3.0;
                iceBoltDestroy.setTexAniFPS(28);
                iceBoltDestroy.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                iceBoltVisuals.ScaleAbs = 2.0;
                iceBoltVisuals.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                iceBoltVisuals.setTexAniFPS(15);
                iceBoltVisualsSecond.ScaleAbs = 2.0;
                iceBoltVisualsSecond.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
                iceBoltVisualsSecond.setTexAniFPS(15);

                iceBoltStaticVisualsInited = true;
            }
            if (lastUsedIceBoltVisual != iceBoltVisuals)
            {
                setStateVisual(iceBoltVisuals, "DEFAULT");
                lastUsedIceBoltVisual = iceBoltVisuals;
            }
            else
            {
                setStateVisual(iceBoltVisualsSecond, "DEFAULT");
                lastUsedIceBoltVisual = iceBoltVisualsSecond;
            }
            currentVisual.setTexAniFrame(0);     
            if (direction == eDir.L)
            {
               // Move(-(cent.X - currentVisual.height / 2),-(cent.Y + currentVisual.width / 2),false);
                currentVisual.Rotate(270);
            }
            else if (direction == eDir.D)
            {
              //  Move(-currentVisual.width, currentVisual.height / 2, false);
                currentVisual.Rotate(180);
            }
            else if (direction == eDir.R)
            {
              //  Move(-(cent.X + currentVisual.height / 2), -(cent.Y - currentVisual.width / 2), false);
                currentVisual.Rotate(90);
            }
            else if (direction == eDir.U)
            {
                currentVisual.Rotate(0);
            }

            //Play cast sound:
            Sound cast = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_ICEBOLT.ogg", false, false);
            cast.volume = 0.8;
            cast.Play();

            // Create dynamic Light for ice bolt:
            Light magic = new Light(eLightType.DYNAMIC, new radialGradient(new OpenTK.Vector2(0, 0), 200f,
                                  new OpenTK.Vector4(0.2f, 0.5f, 1f, 1f),
                                  new OpenTK.Vector4(0, 0f, 0.4f, 0f)));
            magic.scaleLightAni[0] = 1f;
            magic.scaleLightAni[1] = 0.15f;
            magic.scaleLightAni[2] = 0.4f;
            magic.scaleLightAni[3] = 0.65f;
            magic.scaleLightAni[4] = 0.35f;
            magic.scaleLightAni[5] = 0.65f;
            magic.scaleLightAni[6] = 0.27f;
            magic.scaleLightAni[7] = 0.79f;
            magic.scaleLightAni[8] = 0.3f;
            magic.scaleLightAni.aniFps = 1.35f;
            magic.scaleLightAni.aniType = eLightAniType.pingpong;

            magic.colorLightAni[0] = new lightColors(Color.FromArgb(255, 100, 199, 255),
                                                     Color.FromArgb(22, 0, 100, 255));
            magic.colorLightAni[1] = new lightColors(Color.FromArgb(195, 80, 199, 245),
                                                     Color.FromArgb(0, 0, 50, 205));
            magic.colorLightAni[2] = new lightColors(Color.FromArgb(235, 80, 255, 255),
                                                     Color.FromArgb(9, 0, 205, 205));
            magic.colorLightAni[3] = new lightColors(Color.FromArgb(225, 80, 120, 245),
                                                     Color.FromArgb(0, 0, 20, 225));
            magic.colorLightAni.aniFps = 1.25f;
            attachLight(magic);
        }
        private void InitKnife()
        {
            knifeDestroy.setTexAniFPS(15);
            knifeDestroy.setTexAniLoopType(Obj_texAni.eLoopType.NONE);
            if (lastUsedKnifeVisual != knifeVisuals)
            {
                setStateVisual(knifeVisuals, "DEFAULT");
                lastUsedKnifeVisual = knifeVisuals;
            }
            else
            {
                setStateVisual(knifeVisualsSecond, "DEFAULT");
                lastUsedKnifeVisual = knifeVisualsSecond;
            }

            //Rotate visual:
            if (direction == eDir.L)
            {
                Move(0, -currentVisual.height / 2 + 27, false);
                currentVisual.Rotate(180);
            }
            else if (direction == eDir.D)
            {
                Move(currentVisual.width/4,0, false);
                currentVisual.Rotate(90);
            }
            else if (direction == eDir.R)
            {
                Move(currentVisual.width, -currentVisual.height / 2 + 27, false);
                currentVisual.Rotate(0);
            }
            else if (direction == eDir.U)
            {
                Move(currentVisual.width / 4,0, false);
                currentVisual.Rotate(270);
            }

            //Play knifethrow sound:
            Sound knifethrow = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_KNIFE_THROW.ogg", false, false);
            knifethrow.Play();
        }
        public static void forceProjectileVisualsReinit()
        {
            iceBoltStaticVisualsInited = false;
            iceBoltStaticVisualsInited = false;
        }

        public void Update()
        {
            if (isBeingDestroyed == true)
            { // projectile started destroying-ani
              // when last keyframe reached, delete Projectile
                if (iceBoltDestroy.texAniFinished() == true)
                {
                    GameMan.projectilesList.Remove(this);
                }
                return;
            }
            if (speed == 0.0) return; //no movement, no update
            int grid = GameManager.gridSize;

            if (myCollision.collisionStatic == true )
            {
                Destroy();
                return;
            }
            //TODO: When rotate by other value than 0 
            // correct Y-position store current Obj correction so
            // everything will be handled like it could be.
            if (direction == eDir.R)
                Move(pxMove, 0,false);
            else if (direction == eDir.L)
                Move(-pxMove, 0, false);
            else if (direction == eDir.U)
                Move(0, -pxMove, false);
            else if (direction == eDir.D)
                Move(0, pxMove, false);

            //Update bbox:
            if(type == eProjType.IceBolt || type == eProjType.Knife)
                updateBoundingBox(gridX * GameManager.gridSize, gridY * GameManager.gridSize, GameManager.gridSize, GameManager.gridSize);

        }
        /// <summary>
        /// Called when
        /// * collide with static obj (wall)
        /// * Stopping "firin da Lazor"
        /// * Freezing pc/npc
        /// </summary>
        public void Destroy()
        {
            RemoveLights();

            if (type == eProjType.IceBolt)
            {
                isBeingDestroyed = true;
                iceBoltDestroy.setTexAniFrame(0);
                iceBoltDestroy.x = gridX * GameManager.gridSize - 64;
                iceBoltDestroy.y = gridY * GameManager.gridSize - 64;
                
                //Play cast sound:
                Sound destroy = new Sound(Sound.eSndType.SFX,"../data/Sounds/GAME_ICEBOLT_DESTROY.ogg", false, false);
                destroy.volume = 1.0;
                destroy.Play();
                // Create dynamic Light for ice bolt:
                Light explosion = new Light(eLightType.DYNAMIC, new radialGradient(new OpenTK.Vector2(0, 0), 200f,
                                      new OpenTK.Vector4(0.2f, 0.5f, 1f, 1f),
                                      new OpenTK.Vector4(0, 0f, 0.4f, 0f)));
                explosion.scaleLightAni[0] = 1.8f;
                explosion.scaleLightAni[1] = 0f;
                explosion.scaleLightAni.aniFps = 1.35f;
                explosion.scaleLightAni.aniType = eLightAniType.once;

                explosion.colorLightAni[0] = new lightColors(Color.FromArgb(199, 255, 255, 255),
                                                         Color.FromArgb(0, 0, 100, 255));
                explosion.colorLightAni[1] = new lightColors(Color.FromArgb(0, 100, 200, 255),
                                                         Color.FromArgb(0, 0, 0, 205));
                explosion.colorLightAni.aniType = eLightAniType.once;
                explosion.colorLightAni.aniFps = 0.95f;
                explosion.destroyLightAtAniEnd = true;
                attachLight(explosion);
            }
            else if (type == eProjType.Knife)
            {
                destroyKnife();
            }
            else if (type == eProjType.Lazer)
            {
                GameMan.projectilesList.Remove(this);
            }
            GameMan.collisionSystem.RemoveReport(this);
        }

        public void prepareRender()
        {
           
           //Obj gridPos = new Obj("../data/Textures/HLP_GRID2.dds", pX-2, pY-2, Obj.align.LEFT);
           //gridPos.width = 5;
           //gridPos.height = 5;
           //gridPos.setRenderOnce();
           //gridPos.setCurrentTexAlpha(255)
            if (isBeingDestroyed)
            {
                if (type == eProjType.IceBolt)
                    iceBoltDestroy.prepareRender();
                else if (type == eProjType.Knife)
                    knifeDestroy.prepareRender();
                return;
            }
            // Base rendering caused problem with positioning of visual
            //base.Render();
            currentVisual.x = renderX; currentVisual.y = renderY;
            currentVisual.prepareRender();
        }
        private void projectileDynamicCollision(MOB src, collDynEventArgs fe)
        {
            if (type == eProjType.IceBolt)
            {
                if (isBeingDestroyed) return; //no cdDyn when destroying projectile!
                //projectile collide with player
                if (owner.mobType == eMOBType.ENEMY && fe.player != null)
                {
                    Sound freeze = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_ICEBOLT_FREEZE.ogg", false, false);
                    freeze.volume = 0.8;
                    freeze.Play();
                    fe.player.setFreeze();
                    Destroy();
                }
                if (fe.enemy != null && (MOB)fe.enemy != owner)
                {
                    Sound freeze = new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_ICEBOLT_FREEZE.ogg", false, false);
                    freeze.volume = 0.8;
                    freeze.Play();
                    if (fe.enemy.enemyState == Enemy.eEnemyState.FLEE)
                    {
                        GameMan.statistics.addPoints(50);
                        fe.enemy.enemyState = Enemy.eEnemyState.WOUNDED;
                    }
                    else if (fe.enemy.enemyState == Enemy.eEnemyState.NORMAL)
                    {
                        fe.enemy.setFreeze();
                    }
                    Destroy();
                }
            }
            else if (type == eProjType.Lazer)
            {
                if (fe.enemy != null && (MOB)fe.enemy != owner)
                {
                    fe.enemy.turnToDust();
                }
                if (fe.player != null && (MOB)fe.player != owner)
                {
                    fe.player.turnToDust();
                }
            }
            else if (type == eProjType.Knife)
            {
                if (fe.player != null && (MOB)fe.player != owner)
                {
                    fe.player.Kill();

                    new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_KNIFE_HIT.ogg", false, true);
                    // Don't destroy using Destroy method, it would cause call of destroyKnife method
                    // which is used to destroy knife when colliding with wall, etc.
                    GameMan.projectilesList.Remove(this);
                    GameMan.collisionSystem.RemoveReport(this);
                }
            }

        }
        private void destroyKnife()
        {
            new Sound(Sound.eSndType.SFX, "../data/Sounds/GAME_KNIFE_STONECOLLIDE.ogg", false, true);
            isBeingDestroyed = true;
            knifeDestroy.setTexAniFrame(0);
            knifeDestroy.x = currentVisual.x;
            knifeDestroy.y = currentVisual.y;
        }


    }
}
