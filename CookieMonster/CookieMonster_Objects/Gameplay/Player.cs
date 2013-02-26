using System;
using System.Collections.Generic;

using System.Text;
using OpenTK.Input;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    class Skill
    {
        static string[] names = { "Lodowy pocisk", "Przyspieszenie" };

        public enum skillNames { IceBolt, Boost};
        string name;
        public skillNames type { get; private set; }
        public int level{get; private set;}
        public int cooldown { get; private set; }
        public Timer cooldownTimer { get; private set; }
        public int baseTalentPointCost { get; private set; }
        public int talentPointCost { get; private set; }
        public Skill(skillNames s)
        {
            level = 1;
            if (s == skillNames.Boost)
            {
                cooldown = 30;
            }
            else
            {
                cooldown = 20;
            }
            cooldownTimer = new Timer(Timer.eUnits.MSEC, cooldown * 1000, 0, true, false);
            type = s;
            name = names[(int)type];
            baseTalentPointCost = 2;
            talentPointCost = baseTalentPointCost + 2 * level;
        }
        public void riseLevel()
        {
            level++;
            if (type == skillNames.Boost)
            {
                cooldown -= 5;//30/25/20/15
            }
            else
            {
                cooldown /= 2;//20/10/5/2,5..
            }
            cooldownTimer = new Timer(Timer.eUnits.MSEC, cooldown * 1000, 0, true, false);
            name = names[(int)type] + " " + level.ToString();
            talentPointCost = baseTalentPointCost + 2 * level;
        }
    }
    class Player : MOB
    {
        //statics:
        //statistics:
        public int level {get; private set;}
        public int exp{get; private set;}
        public int exp_next{get; private set;}
        public int talentPoints { get; private set; }

        //Skills:
        List<Skill> earnedSkills = new List<Skill>();
        Timer skillBoostTimer; bool skillBoostEnabled;
        private Obj boostFXCurrent;
        private Obj boostFXHorizontal;
        private Obj boostFXUp;
        private Obj boostFXDown;
        //private List<Obj> boostFXSteps;
        //Timer boostFXTimer = new Timer(Timer.eUnits.MSEC, 200, 0, true, false);
        //attributes:
        //double speed => in base class MOB
        public int maxLives { get; private set; }
        int _lives; public int lives { get { return _lives; } set { if (value <= maxLives)_lives = value; } }
        
        private POWER_UP _powerUpsInventory; public POWER_UP powerUpsInventory { get { return _powerUpsInventory; } }
        public int modX { get { return pX % GameManager.gridSize; } }
        public int modY { get { return pY % GameManager.gridSize; } }
       
        private Obj powerPillAura;   
        //Misc:
        //TODO: temporary stuff:
        public Player(int posx, int posy, double spd)
            : base(posx, posy, spd)
        {
            //new DebugMsg("Stworzono obiekt gracza", DebugLVL.info);
            //new DebugMsg("...jest bardzo ladny, naprawde", DebugLVL.info);
            //new DebugMsg(this, "direction", DebugLVL.info);
            //new DebugMsg(this, "dirBuffer", DebugLVL.info);
            //new DebugMsg(this, "pX");
            //new DebugMsg(this, "pY");
            //new DebugMsg(this, "modX");
            //new DebugMsg(this, "modY");
            level = 0; exp = 0; exp_next = 100;
            _lives = 2;
            maxLives = 3;
            mobType = eMOBType.PLAYER;
            skillBoostTimer = new Timer(Timer.eUnits.MSEC, 10 * 1000, 0, true, false);
            //collision:
            base.myCollision = new CollisionReport(this);
            myCollision.cdEvent += new CollisionReport.CDEvtHandler(playerDynamicCollison);
            talentPoints = 0;
            initPowerPillVisuals();
            //now baseSpeed is property of MOB class and this value will be set to speed at MOB constructor
            //baseSpeed = speed;
            //boostFXSteps = new List<Obj>();
            boostFXHorizontal = new Obj("../data/Textures/Game/MOB/COOKIE_BOOST_HORIZONTAL_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            boostFXUp = new Obj("../data/Textures/Game/MOB/COOKIE_BOOST_UP_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            boostFXDown = new Obj("../data/Textures/Game/MOB/COOKIE_BOOST_DOWN_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            boostFXCurrent = boostFXHorizontal;
            _powerUpsInventory = POWER_UP.ENEMY_SLOWER;
        }

        private void initPowerPillVisuals()
        {
            //power pill aura visuals:
            
            powerPillAura = new Obj("../data/Textures/Game/FX/POWERPILL_AURA_A0.dds", 0, 0, Obj.align.CENTER_BOTH);
            powerPillAura.setTexAniFPS(15);
            powerPillAura.ScaleAbs = 1.0;
            Obj_Animation ani = new Obj_Animation(powerPillAura);
            ani.addKeyframe(0, 0, 1.0, 5.0, new Timer(Timer.eUnits.MSEC, 15 * 1000));
            ani.addKeyframe(0, 0, 1.0, 0.9, new Timer(Timer.eUnits.MSEC, 15 * 1000));
            ani.setLoopType(Obj_Animation.eLoopType.None);
            ani.affectionFlags &= ~Obj_Animation.eAffectionFlags.affectPos;
            powerPillAura.addAni(ani);

        }

        public void Update()
        {       
            Waypoint oldLastWP = lastWP;
            base.Update();
            if (oldLastWP != lastWP)
            {
                GameMan.heroHasNewWP();
            }
            if (skillBoostEnabled)
            {
                if (skillBoostTimer.enabled == false)
                {
                    resetMovementSpeed();
                    skillBoostEnabled = false;
                }
            }
        }

        private void UpdateBoostFX()
        {
            if (direction == eDir.R || direction == eDir.L)
            {
                if (mobMirrored)
                    boostFXHorizontal.x = currentVisual.x + 10;
                else
                    boostFXHorizontal.x = currentVisual.x- 10;
                boostFXHorizontal.y = currentVisual.y;
                if ((boostFXHorizontal.width > 0 && mobMirrored)
                || (boostFXHorizontal.width < 0 && !mobMirrored)) boostFXHorizontal.width *= -1;
                boostFXCurrent = boostFXHorizontal;
            }
            else if (direction == eDir.U)
            {
                boostFXUp.y = currentVisual.y+10;
                boostFXUp.x = currentVisual.x;
                if ((boostFXUp.width > 0 && mobMirrored)
                || (boostFXUp.width < 0 && !mobMirrored)) boostFXUp.width *= -1;
                boostFXCurrent = boostFXUp;
            }
            else if (direction == eDir.D)
            {
                boostFXDown.y = currentVisual.y-10;
                boostFXDown.x = currentVisual.x;
                if ((boostFXDown.width > 0 && mobMirrored)
                || (boostFXDown.width < 0 && !mobMirrored)) boostFXDown.width *= -1;
                boostFXCurrent = boostFXDown;
            }
        }
        public void Render()
        {
            if (GameMan.powerPillActive())
            {
                powerPillAura.x = pX + Math.Abs(currentVisual.width)/2;
                powerPillAura.y = pY + currentVisual.height/2;
                powerPillAura.applyAlignCorrection();
                powerPillAura.prepareRender();
            }
            base.Render();
            RenderBoostFX();
        }

        private void RenderBoostFX()
        {
            if (skillBoostEnabled && myCollision.collisionStatic != true && state != eState.FROZEN)
            {
                UpdateBoostFX();
                boostFXCurrent.prepareRender();
            }
        }
        
        /// <summary>
        /// checks if player has skill, if not
        /// method will return null
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Pointer to this skill type in earnedSkills list</returns>
        public Skill hasSkill(Skill.skillNames type)
        {
            Skill s = null;
            for (int i = 0; i < earnedSkills.Count; i++)
            {
                if (earnedSkills[i].type == type)
                    s = earnedSkills[i];
            }
            return s;
        }

        /// <summary>
        /// Rises a Skill
        /// </summary>
        /// <param name="type">Type of the skill</param>
        /// <param name="costTP">true if rise will cost any TalentPts.</param>
        /// <returns>Returns true when skill was risen, false when player doesn't have enough TP</returns>
        public bool riseSkill(Skill.skillNames type,bool costTP)
        {
            Skill s = hasSkill(type);
            bool skillCreated = false;
            int cost = -1;
            if (s == null)
            {
                skillCreated = true;
                s = new Skill(type);
                cost = s.baseTalentPointCost;
            }
            if (cost == -1) cost = s.talentPointCost;//skill wasn't just created so take value from TP cost
            if(talentPoints>=cost && costTP)
                talentPoints = talentPoints-cost;//decreasing talent pts by skill cost
            else if(costTP)
                return false; //no talent points, no skill!!!

            if (skillCreated)
                earnedSkills.Add(s);
            else
                s.riseLevel();
            return true;
        }
        public bool riseSkill(Skill.skillNames type)
        {
            return riseSkill(type, true);
        }
        public bool riseMaxLives()
        {
            if (talentPoints > 0)
            {
                maxLives++;
                talentPoints--;
                return true;
            }
            return false;
        }
        public bool riseSpeed()
        {
            if (talentPoints > 0)
            {
                speed += 5; //current speed, actually can be different than baseSpeed
                increaseBaseSpeed(5);//so we need to update both (when player is boosted)
                talentPoints--;
                return true;
            }
            return false;
        }
        private void nextLevel()
        {
            //on first level show new tip:
            if (level == 0)
                new tipWindow("../data/Textures/GAME/TIPS/tipScreen_IMG2.dds", Lang.cur.tip2_title, Lang.cur.tip2_contents).MoveImage(-32, 0);

            level++;
            talentPoints++;
            exp_next = generateExpNext();
        }

        private int generateExpNext()
        {
            int ret = level * 150 + level * level * 15;
            ret = ret + (100- (ret%100));//round up by 100;
            return ret;
        }
        /// <summary>
        /// Add's exp for player
        /// </summary>
        /// <param name="val"></param>
        /// <returns>TRUE when hero has reached new level</returns>
        public bool addExp(int val)
        {
            exp += val;
            if (exp >= exp_next)
            {//TODO: some jingle to play?
                nextLevel();
                return true;
            }
            return false;
        }

        internal void KeyboardEvt(object sender, KeyboardKeyEventArgs k)
        {
            switch (k.Key)
            {
                case Key.Left:
                    dirBuffer = eDir.L; break;
                case Key.Right:
                    dirBuffer = eDir.R; break;
                case Key.Up:
                    dirBuffer = eDir.U; break;
                case Key.Down:
                    dirBuffer = eDir.D; break;
                case Key.Z:
                    if ((powerUpsInventory & POWER_UP.BOMB) == POWER_UP.BOMB)
                    {
                        _powerUpsInventory = powerUpsInventory & ~POWER_UP.BOMB;
                        new Bomb(pX, pY);
                    }
                    break;
                case Key.X:
                    if ((powerUpsInventory & POWER_UP.ENEMY_SLOWER) == POWER_UP.ENEMY_SLOWER)
                    {
                        _powerUpsInventory = powerUpsInventory & ~POWER_UP.ENEMY_SLOWER;
                        GameMan.slowEnemies();
                    }
                    break;
                case Key.C:
                    useSkill(Skill.skillNames.Boost); break;
                case Key.V:
                    useSkill(Skill.skillNames.IceBolt); break;
                case Key.Enter:
                    if (GameMan.canStartNextLevel) GameMan.nextLevel(); break;
                


            }
        }
        private void useSkill(Skill.skillNames type)
        {
            if (state == eState.FROZEN) return; //no using skill when frozen!
            Skill s = hasSkill(type);
            if (s == null)
                return; // can't use skill u don't have
            if (s.cooldownTimer.enabled) 
                return; //can't use skill that's on cooldown
            s.cooldownTimer.start();
            if (type == Skill.skillNames.Boost)
            {
                skillBoostTimer.restart();
                //resetMovementSpeed();
                speed = baseSpeed + 35.0 + 15.0*s.level;
                skillBoostEnabled = true;
            }
            else if (type == Skill.skillNames.IceBolt)
            {
                new Projectile(Projectile.eProjType.IceBolt, this, this.pX, this.pY, Projectile.iceBoltSpeed);
            }
        }
        /// <summary>
        /// Sets skills cooldown to 0
        /// </summary>
        public void renewSkills()
        {
            for (int i = 0; i < earnedSkills.Count; i++)
            {
                if (earnedSkills[i].cooldownTimer != null)
                {
                    earnedSkills[i].cooldownTimer.restart();
                    earnedSkills[i].cooldownTimer.stop();
                }
            }

        }
        public override void Kill()
        {
            // Can't kill invincible player!
            if (invincible == true) return;

            base.Kill();
            lives--;
            if (lives < 0) GameMan.GameOver();
            createShield();
        }
        private void playerDynamicCollison(MOB src, collDynEventArgs fe)
        {
            Sound pickup = new Sound(Sound.eSndType.SFX, "../data/Sounds/PICKUP.ogg", false, false);
            pickup.volume = 0.8;

            if (fe.cdPowerUP != null)
            {
                if (fe.cdPowerUP.type == POWER_UP.COOKIE)
                    src.eatCookie(fe.cdPowerUP);
                else if (fe.cdPowerUP.type == POWER_UP.BOMB)
                {
                    _powerUpsInventory |= POWER_UP.BOMB;
                    GameMan.removePowerUp(fe.cdPowerUP);
                    pickup.Play();
                }
                else if (fe.cdPowerUP.type == POWER_UP.ENEMY_SLOWER)
                {
                    _powerUpsInventory |= POWER_UP.ENEMY_SLOWER;
                    GameMan.removePowerUp(fe.cdPowerUP);
                    pickup.Play();
                }
                else if (fe.cdPowerUP.type == POWER_UP.POWER_PELLET)
                {
                    initPowerPillVisuals();
                    GameMan.PlayerTakePowerPill();
                    GameMan.removePowerUp(fe.cdPowerUP);
                    pickup.Play();
                    setupPowerPillLight();
                }
                else if (fe.cdPowerUP.type == POWER_UP.LIFE)
                {
                    int oldLives = lives;
                    lives++;
                    if (lives != oldLives)
                    {
                        pickup.Play();
                        GameMan.removePowerUp(fe.cdPowerUP);
                    }
                    else
                    {
                        GameMan.playFailSound();
                    }
                }
                else if (fe.cdPowerUP.type == POWER_UP.SKILL_POINT)
                {
                    talentPoints++;
                    pickup.Play();
                    GameMan.removePowerUp(fe.cdPowerUP);
                }
            }
            if (fe.enemy != null)
            {
                if (fe.enemy.enemyState == Enemy.eEnemyState.FLEE)
                {
                    switch (fe.enemy.type)
                    {
                        case Enemy.enemyType.NORMAL: GameMan.statistics.addPoints(50); break;
                        case Enemy.enemyType.THIEF: GameMan.statistics.addPoints(20); break;
                        case Enemy.enemyType.WIZARD: GameMan.statistics.addPoints(70); break;
                        case Enemy.enemyType.SHOOPDAWOOP: GameMan.statistics.addPoints(80); break;
                        case Enemy.enemyType.ASSASSIN: GameMan.statistics.addPoints(100); break;
                    }
                    fe.enemy.enemyState = Enemy.eEnemyState.WOUNDED;
                    
                }
                else if (fe.enemy.enemyState == Enemy.eEnemyState.NORMAL && fe.enemy.state == eState.DEFAULT)
                {
                    //TODO: Later, it will means it's time to start attack
                    onHit();
                    Kill();
                }
            }
            //  
        }

        private void setupPowerPillLight()
        {
            //Start-up dynamic light:
            radialGradient grad = new radialGradient(new Vector2(),
                                                          320f,
                                                          new Vector4(0f, 0.8f, 0.2f, 0.65f),
                                                          new Vector4(0f, 0.3f, 0f, 0f),
                                                          BlendingFactorSrc.SrcAlpha,
                                                          BlendingFactorDest.One);
            Light lightFX = new Light(eLightType.DYNAMIC, grad);

            lightFX.colorLightAni[0] = new lightColors(Color.FromArgb(160, 120, 190, 255),
                                                       Color.FromArgb(0, 0, 0, 255));
            lightFX.colorLightAni[1] = new lightColors(Color.FromArgb(110, 0, 90, 255),
                                                       Color.FromArgb(0, 0, 0, 255));
            lightFX.colorLightAni.aniFps = 0.5f;

            lightFX.scaleLightAni[0] = 1f;
            lightFX.scaleLightAni[1] = 0f;
            lightFX.scaleLightAni.aniFps = 0.066f;
            lightFX.scaleLightAni.aniType = eLightAniType.once;
            lightFX.destroyLightAtAniEnd = true;
            lightFX.setParentMOB(this);
            lightFX.posLightAni[0] = new Point(GameManager.gridSize / 2, GameManager.gridSize / 2);
        }


        /// <summary>
        /// Restore player attributes & skills
        /// based on savegame playerData
        /// </summary>
        /// <param name="sav">passed Savegame</param>
        internal void restoreFromSave(Savegame sav)
        {
            speed = sav.player.movementSpeed;
            increaseBaseSpeed((int)(speed - GameManager.initPCSpeed));
            level = (int)sav.player.level;
            exp  = (int)sav.player.exp;
            _lives = (int)sav.player.lives;
            maxLives = (int)sav.player.maxLives;
            talentPoints = (int)sav.player.talentPts;
            //inventory:
            if (sav.player.invHasBomb > 0)
                _powerUpsInventory |= POWER_UP.BOMB;
            if (sav.player.invHasTimeSlow > 0)
                _powerUpsInventory |= POWER_UP.ENEMY_SLOWER;
            //skills:
            for (int i = 0; i < sav.player.speedBoostLVL; i++)
                riseSkill(Skill.skillNames.Boost, false);

            for (int i = 0; i < sav.player.iceBoltLVL; i++)
                riseSkill(Skill.skillNames.IceBolt, false);
            
            exp_next = generateExpNext();
        }
    }
}
