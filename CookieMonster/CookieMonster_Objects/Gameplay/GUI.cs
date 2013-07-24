using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;
using QuickFont;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Class is responsible for graphic user interface in-game
    /// (If I have more time for refractoring, I would made it singleton)
    /// It's responsible for drawing of whole bar on bottom of screen
    /// (with power-up's and skills icons, points etc.)
    /// </summary>
    class GUI : engineReference
    {
        QFont guiFont = TextManager.newQFont("Rumpelstiltskin.ttf", 25);
        QFont livesFont = TextManager.newQFont("Rumpelstiltskin.ttf", 30);
        QFont controllsFont = TextManager.newQFont("KOMIKAX.ttf", 25,true);
        Obj background;
        List<Obj> PUBackgrounds;
        List<Obj> SkillsBackgrounds;
        Obj PUBomb; Obj PUTimeSlow;
        Obj SkillBoost; Obj SkillIceBolt;
        Obj Cooldown_Boost, Cooldown_IceBolt;

        Obj DisabledSkillBoost; Obj DisabledSkillIceBolt;
        Obj[] skillRefreshingTexs;
        Obj lives;
        Text powerUpTxt;
        Text SkillsTxt;
        Text Points;
        List<Text> PUControllLetters;
        List<Text> SkillsControllLetters;
        Text restLives;
        Obj enterButtonToNextLevel;
        public GUI()
        {
            const int textFromDownLine = 48;
            const int textKeyHints = 62;
            Viewport act = engine.activeViewport;
            TextManager txtMan = engine.textManager;
            double y_bg = 0.8575;
            double y_obj = 0.90;

            new DebugMsg("Creating GUI bar background...");
            background = new Obj("../data/Textures/GAME/GUI/BG.dds", 0.5, y_bg, Obj.align.CENTER_X);
            background.isGUIObject = true;//so it won't move with camera

            new DebugMsg("Creating livesFont...");
            livesFont.Options.Colour = new OpenTK.Graphics.Color4(200, 230, 250, 255);
            //Power ups:
            new DebugMsg("Creating pUPs BGs...");
            PUBackgrounds = new List<Obj>();
            Obj tmp = new Obj("../data/Textures/GAME/GUI/GUI_PU_BG.dds", 0.1595, y_obj, Obj.align.CENTER_X, false);
            tmp.ignoreCameraOffset = true;
            PUBackgrounds.Add(tmp);
            tmp = new Obj("../data/Textures/GAME/GUI/GUI_PU_BG.dds", 0.2225, y_obj, Obj.align.CENTER_X, false);
            tmp.ignoreCameraOffset = true;
            PUBackgrounds.Add(tmp);


            new DebugMsg("Creating pUPs Items TEXs...");
            PUBomb = new Obj("../data/Textures/GAME/SOB/Bomb.dds", 0.1595, y_obj, Obj.align.CENTER_X, false);
            PUBomb.ignoreCameraOffset = true;
            PUTimeSlow = new Obj("../data/Textures/GAME/SOB/TIME_SLOW.dds", 0.2225, y_obj, Obj.align.CENTER_X, false);
            PUTimeSlow.ignoreCameraOffset = true;
            new DebugMsg("Creating pUPs lives TEXs...");
            lives = new Obj("../data/Textures/GAME/GUI/LIVE.dds", 0.18, y_obj, Obj.align.CENTER_X, false);
            lives.ignoreCameraOffset = true;

            new DebugMsg("Creating pUPs triggerKeys Texts...");
            powerUpTxt = new Text(guiFont, 28, (float)(engine.Height - textFromDownLine), Lang.cur.Dopalacze);
            PUControllLetters = new List<Text>();
            controllsFont.Options.Colour = new OpenTK.Graphics.Color4(255, 255, 255, 192);
            PUControllLetters.Add(new Text(controllsFont, 192, (float)(engine.Height - textKeyHints), "Z"));
            PUControllLetters.Add(new Text(controllsFont, 268, (float)(engine.Height - textKeyHints), "X"));
            //Correct layer of letters:
            foreach (Text t in PUControllLetters)
                t.layer = Layer.textGUIFG;

            new DebugMsg("Creating skills BGs...");
            SkillsBackgrounds = new List<Obj>();
            tmp = new Obj("../data/Textures/GAME/GUI/GUI_SKILL_BG.dds", 0.904, y_obj, Obj.align.CENTER_X, false);
            tmp.ignoreCameraOffset = true;
            SkillsBackgrounds.Add(tmp);
            tmp = new Obj("../data/Textures/GAME/GUI/GUI_SKILL_BG.dds", 0.9645, y_obj, Obj.align.CENTER_X, false);
            tmp.ignoreCameraOffset = true;
            SkillsBackgrounds.Add(tmp);

            new DebugMsg("Creating skills triggerKeys Texts...");
            SkillsTxt = new Text(guiFont, engine.Width - 335, (float)(engine.Height - textFromDownLine), Lang.cur.umiejetnosci);
            SkillsControllLetters = new List<Text>();
            SkillsControllLetters.Add(new Text(controllsFont, engine.Width - 134, (float)(engine.Height - textKeyHints), "C"));
            SkillsControllLetters.Add(new Text(controllsFont, engine.Width - 62, (float)(engine.Height - textKeyHints), "V"));
            //Correct layer of letters:
            foreach (Text t in SkillsControllLetters)
                t.layer = Layer.textGUIFG;

            new DebugMsg("Creating skills TEXs...");
            SkillBoost = new Obj("../data/Textures/GAME/SKILLS/SKILL_BOOST.dds", 0.905, y_obj, Obj.align.CENTER_X, false);
            SkillBoost.ignoreCameraOffset = true;
            SkillIceBolt = new Obj("../data/Textures/GAME/SKILLS/SKILL_ICEBOLT.dds", 0.964, y_obj, Obj.align.CENTER_X, true);
            SkillIceBolt.ignoreCameraOffset = true;

            new DebugMsg("Creating skills disabled TEXs...");
            DisabledSkillBoost = new Obj("../data/Textures/GAME/SKILLS/SKILL_BOOST_DISABLED.dds", 0.905, y_obj, Obj.align.CENTER_X, true);
            DisabledSkillBoost.ignoreCameraOffset = true;
            DisabledSkillIceBolt = new Obj("../data/Textures/GAME/SKILLS/SKILL_ICEBOLT_DISABLED.dds", 0.964, y_obj, Obj.align.CENTER_X, true);
            DisabledSkillIceBolt.ignoreCameraOffset = true;

            new DebugMsg("Creating skills pts Text...");
            Points = new Text(guiFont, engine.Width / 2 - 310, (float)(engine.Height - textFromDownLine), Lang.cur.punkty_0);

            new DebugMsg("Creating enter button Obj...");
            enterButtonToNextLevel = new Obj("../data/Textures/GAME/GUI/enter_button.dds", 0.5, 0.0, Obj.align.CENTER_X, true);
            enterButtonToNextLevel.layer = Layer.imgGUIFG;
            // Init skill refreshing tex's:
            new DebugMsg("Initing skill Refreshing OBJs...");
            skillRefreshingTexs = new Obj[100];
            for (int i = 0; i < 100; i++)
            {
                skillRefreshingTexs[i] = new Obj("../data/Textures/GAME/GUI/SKILL_REFRESHING_" + i + ".dds", 0.905, y_obj, Obj.align.CENTER_X, false);
                skillRefreshingTexs[i].layer = Layer.imgGUI;
                // this should fix 'blinking' bug:
                skillRefreshingTexs[i].prepareRender();
            }
            new DebugMsg("GUI Constructor END...");
        }
        public void prepareRender()
        {
            GameManager gm = engine.gameManager;
            double y_obj = 0.90;
            background.prepareRender();
            for (int i = 0; i < PUBackgrounds.Count; i++)
                PUBackgrounds[i].prepareRender();
            for (int i = 0; i < SkillsBackgrounds.Count; i++)
                SkillsBackgrounds[i].prepareRender();
            powerUpTxt.Update();
            SkillsTxt.Update();

            //Print powerUps tiles:
            if ((gm.PC.powerUpsInventory & POWER_UP.BOMB) == POWER_UP.BOMB)//hero has bomb:
                PUBomb.prepareRender();
            if ((gm.PC.powerUpsInventory & POWER_UP.ENEMY_SLOWER) == POWER_UP.ENEMY_SLOWER)
                PUTimeSlow.prepareRender();

            //print skills:

            if (gm.PC.hasSkill(Skill.skillNames.Boost) != null)
            {
                Skill boost = gm.PC.hasSkill(Skill.skillNames.Boost);
                if (boost.cooldownTimer.enabled)
                { // Show cooldown timer:
                    double part = boost.cooldownTimer.partDoneReverse;
                    
                    part = Math.Max(0f,Math.Min(0.99f,Math.Round(part, 2)));// 100% safe etc. :P
                    part *= 100;
                    int p = (int)part;

                    // bugfix: LOL ITS REALLY SOME KIND
                    // OF MAGIC AND STUFF, BUT THIS REALLY HELPS:
                    // (rendering both current cooldown value 
                    //  tex, and succesor tex for cooldown)
                    int p_succesor = (p + 1) % 100;
                    skillRefreshingTexs[p].vx = 1.05; // let it be rendered off-screen
                    skillRefreshingTexs[p].prepareRender();

                    // prevent using same tex for both cooldowns:
                    if (skillRefreshingTexs[p] == Cooldown_Boost) p = (p == 0) ? p + 1 : p - 1;

                    //skillRefreshingTexs[p_succesor] = skillRefreshingTexs[p];
                    skillRefreshingTexs[p_succesor].vx = 0.878;
                    skillRefreshingTexs[p_succesor].layer = Layer.imgGUI;
                    skillRefreshingTexs[p_succesor].ignoreCameraOffset = true;
                    skillRefreshingTexs[p_succesor].prepareRender();
                    // END OF MAGICAL BUGFIX
                    DisabledSkillBoost.prepareRender();
                }
                else
                    SkillBoost.prepareRender();
            }
            else
                DisabledSkillBoost.prepareRender();

            if (gm.PC.hasSkill(Skill.skillNames.IceBolt) != null)
            {
                Skill iceBolt = gm.PC.hasSkill(Skill.skillNames.IceBolt);
                if (iceBolt.cooldownTimer.enabled)
                {
                    double part = iceBolt.cooldownTimer.partDoneReverse;
                    part = Math.Max(0f, Math.Min(0.99f, Math.Round(part, 2)));// 100% safe etc. :P
                    part *= 100;
                    //part = part - (part % 10);
                    int p = (int)part;
                    if (p > 99) p = 99; else if (p < 0) p = 0;

                    // bugfix: LOL ITS REALLY SOME KIND
                    // OF MAGIC AND STUFF, BUT THIS REALLY HELPS:
                    // (rendering both current cooldown value 
                    //  tex, and succesor tex for cooldown)
                    int p_succesor = (p + 1) % 100;
                    skillRefreshingTexs[p].vx = 1.05; // let it be rendered off-screen
                    skillRefreshingTexs[p].prepareRender();

                    // prevent using same tex for both cooldowns:
                    if (skillRefreshingTexs[p] == Cooldown_Boost) p = (p == 0) ? p + 1 : p - 1;

                    //skillRefreshingTexs[p_succesor] = skillRefreshingTexs[p];
                    skillRefreshingTexs[p_succesor].vx = 0.9385;
                    skillRefreshingTexs[p_succesor].layer = Layer.imgGUI;
                    skillRefreshingTexs[p_succesor].ignoreCameraOffset = true;
                    skillRefreshingTexs[p_succesor].prepareRender();
                    // END OF MAGICAL BUGFIX

                    DisabledSkillIceBolt.prepareRender();
                }
                else
                    SkillIceBolt.prepareRender();
            }
            else
                DisabledSkillIceBolt.prepareRender();

           
            for (int i = 0; i < PUControllLetters.Count; i++)
                PUControllLetters[i].Update();
            for (int i = 0; i < SkillsControllLetters.Count; i++)
                SkillsControllLetters[i].Update();

            //Lives:
            const int maxLives = 5;
            int l; int startX = (int)(600.0 / (double)Viewport.guiBase_width * engine.Width); 
            int stepX = 50;
            for (l = 0; ((l + 1 < gm.PC.lives)&&(l<maxLives)); l++)
            {
                lives.x = startX + l * stepX;
                lives.prepareRender();
            }
            if (gm.PC.lives - l - 1 > 1)
            {
                TextManager txtMan = engine.textManager;
                Viewport act = engine.activeViewport;
                if (restLives == null)
                    restLives = new Text(livesFont, (float)(startX + 10 + l * stepX), (float)(engine.Height - 70), "+" + (gm.PC.lives - maxLives - 2).ToString());
                else restLives.changeText("+" + (gm.PC.lives - maxLives - 2));
                //Text restLives = txtMan.produceText(livesFont, "+" + (gm.PC.lives - maxLives - 2).ToString(), (float)(startX + 10 + l * stepX), (float)(act.height - 70));
                restLives.Update();
            }

            Points.msg = Lang.cur.punkty + engine.gameManager.statistics.lvlPoints.ToString() + " / " + (engine.gameManager.Map.cookiesCount * Statistics.ptsPerCookie).ToString();
            Points.Update();

            if (engine.gameManager.canStartNextLevel)
            {
                enterButtonToNextLevel.layer = Layer.imgGUIFG;
                enterButtonToNextLevel.prepareRender();
            }

        }

        public void Free()
        {
            foreach (Obj o in PUBackgrounds)     o.Free();
            foreach (Obj o in SkillsBackgrounds) o.Free();
            PUBomb.Free();                PUTimeSlow.Free();
            SkillBoost.Free();            SkillIceBolt.Free();
            DisabledSkillBoost.Free(); DisabledSkillIceBolt.Free();
            foreach (Obj o in skillRefreshingTexs) o.Free();
            lives.Free();
            enterButtonToNextLevel.Free();
            background.Free();
        }
    }
}
