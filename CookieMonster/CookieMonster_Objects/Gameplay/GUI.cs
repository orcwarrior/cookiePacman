using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;
using QuickFont;

namespace CookieMonster.CookieMonster_Objects
{
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
            const int textFromDownLine = 70;
            const int textKeyHints = 85;
            Viewport act = engine.activeViewport;
            TextManager txtMan = engine.textMenager;
            double y_bg = 0.8575;
            double y_obj = 0.90;
            background = new Obj("../data/Textures/GAME/GUI/BG.dds", 0.5, y_bg, Obj.align.CENTER_X);
            background.isGUIObject = true;//so it won't move with camera

            livesFont.Options.Colour = new OpenTK.Graphics.Color4(200,  230, 250, 255);
            //Power ups:
            PUBackgrounds = new List<Obj>();
            Obj tmp = new Obj("../data/Textures/GAME/GUI/GUI_PU_BG.dds", 0.1565, y_obj, Obj.align.CENTER_X, false);
            tmp.isGUIObjectButUnscaled = true;
            PUBackgrounds.Add(tmp);
            tmp = new Obj("../data/Textures/GAME/GUI/GUI_PU_BG.dds", 0.2175, y_obj, Obj.align.CENTER_X, false);
            tmp.isGUIObjectButUnscaled = true;
            PUBackgrounds.Add(tmp);


            PUBomb = new Obj("../data/Textures/GAME/SOB/Bomb.dds", 0.1565, y_obj, Obj.align.CENTER_X, false);
            PUBomb.isGUIObjectButUnscaled = true;
            PUTimeSlow = new Obj("../data/Textures/GAME/SOB/TIME_SLOW.dds", 0.2175, y_obj, Obj.align.CENTER_X, false);
            PUTimeSlow.isGUIObjectButUnscaled = true;
            lives = new Obj("../data/Textures/GAME/GUI/LIVE.dds", 0.18, y_obj, Obj.align.CENTER_X, false);
            lives.isGUIObjectButUnscaled = true;

            powerUpTxt = new Text(guiFont,  28, (float)(act.height - textFromDownLine),Lang.cur.Dopalacze);
            PUControllLetters = new List<Text>();
            controllsFont.Options.Colour = new OpenTK.Graphics.Color4(255, 255, 255, 192);
            PUControllLetters.Add(new Text(controllsFont, 187, (float)(act.height - textKeyHints), "Z"));
            PUControllLetters.Add(new Text(controllsFont, 265, (float)(act.height - textKeyHints), "X"));


            SkillsBackgrounds = new List<Obj>();
            tmp = new Obj("../data/Textures/GAME/GUI/GUI_SKILL_BG.dds", 0.895, y_obj, Obj.align.CENTER_X, false);
            tmp.isGUIObjectButUnscaled = true;
            SkillsBackgrounds.Add(tmp);
            tmp = new Obj("../data/Textures/GAME/GUI/GUI_SKILL_BG.dds", 0.955, y_obj, Obj.align.CENTER_X, false);
            tmp.isGUIObjectButUnscaled = true;
            SkillsBackgrounds.Add(tmp);
            SkillsTxt = new Text(guiFont, act.width - 335, (float)(act.height - textFromDownLine), Lang.cur.umiejetnosci);
            SkillsControllLetters = new List<Text>();
            SkillsControllLetters.Add(new Text(controllsFont, act.width - 148, (float)(act.height - textKeyHints), "C"));
            SkillsControllLetters.Add(new Text(controllsFont, act.width - 72, (float)(act.height - textKeyHints), "V"));

            SkillBoost = new Obj("../data/Textures/GAME/SKILLS/SKILL_BOOST.dds", 0.895, y_obj, Obj.align.CENTER_X, false);
            SkillBoost.isGUIObjectButUnscaled = true;
            SkillIceBolt = new Obj("../data/Textures/GAME/SKILLS/SKILL_ICEBOLT.dds", 0.955, y_obj, Obj.align.CENTER_X, true);
            SkillIceBolt.isGUIObjectButUnscaled = true;

            DisabledSkillBoost = new Obj("../data/Textures/GAME/SKILLS/SKILL_BOOST_DISABLED.dds", 0.895, y_obj, Obj.align.CENTER_X, true);
            DisabledSkillBoost.isGUIObjectButUnscaled = true;
            DisabledSkillIceBolt = new Obj("../data/Textures/GAME/SKILLS/SKILL_ICEBOLT_DISABLED.dds", 0.955, y_obj, Obj.align.CENTER_X, true);
            DisabledSkillIceBolt.isGUIObjectButUnscaled = true;

            Points = new Text(guiFont,  act.width / 2 - 310, (float)(act.height - textFromDownLine),Lang.cur.punkty_0);

            enterButtonToNextLevel = new Obj("../data/Textures/GAME/GUI/enter_button.dds", 0.5, 0.0, Obj.align.CENTER_X, true);

            // Init skill refreshing tex's:
            skillRefreshingTexs = new Obj[100];
            for(int i=0;i<100;i++)
                skillRefreshingTexs[i] = new Obj("../data/Textures/GAME/GUI/SKILL_REFRESHING_" + i + ".dds", 0.895, y_obj, Obj.align.CENTER_X, false);
        }
        public void prepareRender()
        {
            engine.activeViewport.currentAddingLayer = Layer.imgFG;
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
                {
                    double part = 1.0 - (boost.cooldownTimer.currentTime / (double)boost.cooldownTimer.totalTime);
                    part = Math.Max(0f,Math.Min(0.99f,Math.Round(part, 2)));// 100% safe etc. :P
                    part *= 100;
                    int p = (int)part;
                    Obj tmp = skillRefreshingTexs[p];
                    tmp.vx = 0.870;
                    //new Obj("../data/Textures/GAME/GUI/SKILL_REFRESHING_" + p + ".dds", 0.895, y_obj, Obj.align.CENTER_X, false);
                    tmp.isGUIObjectButUnscaled = true;
                    /*tmp.setRenderOnce();*/ tmp.prepareRender();
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
                    double part = 1.0 - (iceBolt.cooldownTimer.currentTime / (double)iceBolt.cooldownTimer.totalTime);
                    part = Math.Max(0f, Math.Min(0.99f, Math.Round(part, 2)));// 100% safe etc. :P
                    part *= 100;
                    int p = (int)part;
                    if (p > 99) p = 99; else if (p < 0) p = 0;
                    Obj tmp = skillRefreshingTexs[p];
                    tmp.vx = 0.930;
                    //new Obj("../data/Textures/GAME/GUI/SKILL_REFRESHING_" + p + ".dds", 0.955, y_obj, Obj.align.CENTER_X, false);
                    tmp.isGUIObjectButUnscaled = true;
                    /*tmp.setRenderOnce(); */tmp.prepareRender();
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
            int l; int startX = (int)(600.0 / (double)Viewport.guiBase_width * gm.activeView.width); 
            int stepX = 50;
            for (l = 0; ((l + 1 < gm.PC.lives)&&(l<maxLives)); l++)
            {
                lives.x = startX + l * stepX;
                lives.prepareRender();
            }
            if (gm.PC.lives - l - 1 > 1)
            {
                TextManager txtMan = engine.textMenager;
                Viewport act = engine.activeViewport;
                if (restLives == null)
                    restLives = new Text(livesFont, (float)(startX + 10 + l * stepX), (float)(act.height - 70), "+" + (gm.PC.lives - maxLives - 2).ToString());
                else restLives.changeText("+" + (gm.PC.lives - maxLives - 2));
                //Text restLives = txtMan.produceText(livesFont, "+" + (gm.PC.lives - maxLives - 2).ToString(), (float)(startX + 10 + l * stepX), (float)(act.height - 70));
                restLives.Update();
            }

            Points.msg = Lang.cur.punkty + engine.gameManager.statistics.lvlPoints.ToString() + " / " + (engine.gameManager.Map.cookiesCount * Statistics.ptsPerCookie).ToString();
            Points.Update();

            if (engine.gameManager.canStartNextLevel)
            {
                enterButtonToNextLevel.prepareRender();
            }
            engine.activeViewport.currentAddingLayer = -1;
        }
    }
}
