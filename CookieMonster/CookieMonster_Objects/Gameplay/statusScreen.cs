﻿using System;
using System.Collections.Generic;

using System.Text;
using EngineApp;
using System.Drawing;
using QuickFont;

namespace CookieMonster.CookieMonster_Objects
{
    class statusScreen : engineReference
    {
        public bool active { get; private set; }
        private Obj background;
        private Obj cookie; private int cookieDstXPos, cookieMoveWidth;
        private Obj pause;
        private Timer easeInTimer;
        private Timer easeOutTimer;

        //menu stuff:
        private Menu menu_status;
        private bool menuCreated;
        public Rectangle menuArea { get; private set; }
        public QFont FontStatus_BIG { get; private set; }
        public QFont FontStatus_Small { get; private set; }
        public QFont FontStatus_Little { get; private set; }

        #region menu contents
        //menu texts: (generated in generateContents method, rendered by onRender function by MENU_STATUS)
        public Text Level_initial { get; private set; }
        public Text Level         { get; private set; }

        public Text TalentPts_initial   { get; private set; }
        public Text TalentPts_initial2  { get; private set; }
        public Text TalentPts           { get; private set; }
        public List<Obj> TalentPtsStars { get; private set; }

        public Text Atributes_initial       { get; private set; }
        public Text Atributes               { get; private set; }
        public Text Atributes_speed         { get; private set; }
        public Text Atributes_maxLives      { get; private set; }
        public Text Atributes_speed_val     { get; private set; }
        public Text Atributes_maxLives_val  { get; private set; }
        public Text Atributes_speed_plus    { get; private set; } 
        public Text Atributes_maxLives_plus { get; private set; }
        public Obj  Atributes_speed_add     { get; private set; } //there? rather as menu item
        public Obj  Atributes_maxLives_add  { get; private set; }
        
        public Text Skills_initial       { get; private set; }
        public Text Skills               { get; private set; }
        public Text Skills_boost         { get; private set; }
        public Text Skills_icebolt       { get; private set; }
#endregion

        public statusScreen()
        {
            easeInTimer = new Timer(Timer.eUnits.MSEC, 400, 0, true, false);
            easeOutTimer = new Timer(Timer.eUnits.MSEC, 200, 0, true, false);
            background = new Obj("../data/Textures/GAME/GUI/STATUSSCREEN_BG.dds", 0.5, 0.45, Obj.align.CENTER_BOTH);
            background.isGUIObjectButUnscaled = true;
            menuArea = new Rectangle(background.x + 53, background.y + 216, 500, 750);

            cookie = new Obj("../data/Textures/GAME/GUI/STATUSSCREEN_COOKIE.dds", 1.05, 1.09, Obj.align.RIGHT);
            cookie.isGUIObjectButUnscaled = true;
            cookieDstXPos = cookie.x;// +cookie.width;
            cookieMoveWidth = 950;
            cookie.x += cookieMoveWidth;

            pause = new Obj("../data/Textures/GAME/GUI/STATUSSCREEN_PAUSE.dds", 1.0, 0.85, Obj.align.RIGHT);
            pause.isGUIObjectButUnscaled = true; pause.x -= 200;


            FontStatus_BIG = TextManager.newQFont("Rumpelstiltskin.ttf", 37,FontStyle.Regular,true);
            FontStatus_Small = TextManager.newQFont("Rumpelstiltskin.ttf", 28, FontStyle.Regular, true);
            FontStatus_Little = TextManager.newQFont("Rumpelstiltskin.ttf", 25, FontStyle.Regular, true);
            TalentPtsStars = new List<Obj>();
        }
        public void Show()
        {
            active = true;
            easeOutTimer.restart(); easeOutTimer.stop();
            easeInTimer.start();
            menuCreated = false;
            cookie.setCurrentTexAlpha(255);
            cookie.x = cookieDstXPos + cookieMoveWidth;
        }
        public void Hide()
        {
            easeInTimer.restart(); easeInTimer.stop();
            easeOutTimer.start();
            engine.gameState &= ~Game.game_state.Menu;
        }
        public void Update()
        {
            if (easeInTimer.enabled)
            { // menu is easing in
                double multi = ((easeInTimer.currentTime - 150.0) / easeInTimer.totalTime);
                if (multi < 0.0) multi = 0.0;
                cookie.x = cookieDstXPos + (int)(cookieMoveWidth * multi);
                multi = 1.0 - (easeInTimer.currentTime / 300.0);
                if (multi < 0.0) multi = 0.0;
                background.setCurrentTexAlpha((byte)(255 * multi));
            }
            else if (easeOutTimer.enabled)
            { // menu is easing out
                double multi = (easeOutTimer.currentTime*1.0 / easeOutTimer.totalTime);
                if (multi > 1.0) multi = 1.0;
                background.setCurrentTexAlpha((byte)(255.0 * multi));
                cookie.setCurrentTexAlpha((byte)(255.0 * multi));

            }
            else if (active && !menuCreated)
            {// menu just eased in -> time to create menu
                menuCreated = true;
                menu_status = new Menu("MENU_STATUS", Menu_Instances.Status_OnLoad, null, Menu_Instances.Status_OnRender, Menu_Manager.cursor);
                engine.menuManager.setCurrentMenu(menu_status);
                engine.gameState |= Game.game_state.Menu;
            }
            else if (!easeOutTimer.enabled && active && !((engine.gameState & Game.game_state.Menu) == Game.game_state.Menu))
            { // status menu just has easedout
                active = false;
            }
        }
        public void prepareRender()
        {
            if (active)
            {
                background.prepareRender();
                cookie.prepareRender();
            }
        }

        public void generateContents()
        {
            GameManager gm = engine.gameManager;
            TextManager txtMan = engine.textMenager;
            int pX, pY;
            const int lMargin = 20;


            Text.currentWorkingLayer = Layer.textGUIBG;
            menu_status.clearMenuItems();
            //Level X:
            pX = menuArea.Left + lMargin;
            pY = menuArea.Top + (int)(menuArea.Height * 0.085);//8%
            
            Level_initial = new Text(FontStatus_BIG,  pX, pY - 10, Lang.cur.POZIOM_FIRST+"     " + gm.PC.level + "(" + gm.PC.exp+"/"+gm.PC.exp_next+")");
            Level = new Text(FontStatus_Small,  pX + 4, pY, " " + Lang.cur.POZIOM_REST);
           
            //Talent Pts
            pX = menuArea.Right+90;
            pY = menuArea.Top + (int)(menuArea.Height * 0.085);//8%
            TalentPts_initial2 = new Text(FontStatus_BIG, pX - 15, pY - 10, Lang.cur.PKT_TALENTU__P + "        ", QFontAlignment.Right, 300);
            TalentPts_initial = new Text(FontStatus_BIG, pX + 2, pY - 10, Lang.cur.PKT_TALENTU__T + "     ", QFontAlignment.Right, 300);
            TalentPts = new Text(FontStatus_Small, pX + 4, pY, " " + Lang.cur.KT_ALENTU, QFontAlignment.Right, 300);
            //TalentPts_initial2 = txtMan.produceText(FontStatus_BIG, Lang.cur.PKT_TALENTU__P + "        ", pX - 15, pY - 10, QFontAlignment.Right);
            //TalentPts_initial = txtMan.produceText(FontStatus_BIG, Lang.cur.PKT_TALENTU__T + "     ", pX + 2, pY - 10, QFontAlignment.Right);
            //TalentPts        = txtMan.produceText(FontStatus_Small, " "+Lang.cur.KT_ALENTU, pX + 4, pY, QFontAlignment.Right);
            //Talent Stars:
            TalentPtsStars.Clear();
            int starXStep = 40;
            pY += (int)(menuArea.Height * 0.12); pX += 20;
            for (int i = 0; i < engine.gameManager.PC.talentPoints; i++)
            {
                Obj star = new Obj("../data/Textures/GAME/GUI/STAR_COLOR.dds", pX - starXStep * i, pY, Obj.align.RIGHT);
                star.isGUIObjectButUnscaled = true;
                TalentPtsStars.Add(star);
            }

            //Atributes:
            pX = menuArea.Left + lMargin;
            pY = menuArea.Top + (int)(menuArea.Height * 0.25);//25%

            Atributes_initial = new Text(FontStatus_BIG,  pX, pY - 10, Lang.cur.ATRYBUTY__A);
            Atributes = new Text(FontStatus_Small,  pX + 5, pY, " "+Lang.cur.TRYBUTY);

            pY += (int)(menuArea.Height * 0.07);
            Atributes_speed = new Text(FontStatus_Little,  pX, pY, Lang.cur.SZYBKOSC+":");
            pX += (int)(menuArea.Width * 0.4);
            int speed = (int)gm.PC.speed;
            Atributes_speed_val = new Text(FontStatus_Little,  pX, pY, speed.ToString());

            pX += (int)(menuArea.Width * 0.15);
            pY -= (int)(menuArea.Height * 0.02);
            Obj add = new Obj("../data/Textures/GAME/GUI/STAR_GRAY.dds", pX, pY, Obj.align.CENTER_X);
            add.isGUIObjectButUnscaled = true;
            Obj addOn = new Obj("../data/Textures/GAME/GUI/STAR_COLOR.dds", pX, pY, Obj.align.CENTER_X);
            addOn.isGUIObjectButUnscaled = true;
            Obj addClick = new Obj("../data/Textures/GAME/GUI/STAR_COLOR_BIG.dds", pX, pY, Obj.align.CENTER_X);
            addClick.isGUIObjectButUnscaled = true;
            menu_status.addItem(new Menu_Item("raiseSpeed", add, addOn, addClick, Menu_Instances.Status_ButtonOnHover,null, Menu_Instances.Status_AtrBoostClick));

            pY -= (int)(menuArea.Height * 0.006);
            pX += 4;
            Atributes_speed_plus = new Text(FontStatus_Small, pX, pY, "+5");

            //Max lives number:

            pX = menuArea.Left + lMargin;
            pY = menuArea.Top + (int)(menuArea.Height * 0.39);//39%
            Atributes_maxLives = new Text(FontStatus_Little,  pX, pY, Lang.cur.MAXDOT_ILDOT_ZYC+":");
            pX += (int)(menuArea.Width * 0.4);
            Atributes_maxLives_val = new Text(FontStatus_Little,  pX, pY, gm.PC.maxLives.ToString());

            pX += (int)(menuArea.Width * 0.15);
            pY -= (int)(menuArea.Height * 0.02);
            add = new Obj("../data/Textures/GAME/GUI/STAR_GRAY.dds", pX, pY, Obj.align.CENTER_X);
            add.isGUIObjectButUnscaled = true;
            addOn = new Obj("../data/Textures/GAME/GUI/STAR_COLOR.dds", pX, pY, Obj.align.CENTER_X);
            addOn.isGUIObjectButUnscaled = true;
            addClick = new Obj("../data/Textures/GAME/GUI/STAR_COLOR_BIG.dds", pX, pY, Obj.align.CENTER_X);
            addClick.isGUIObjectButUnscaled = true;
            menu_status.addItem(new Menu_Item("raiseMaxLives", add, addOn, addClick, Menu_Instances.Status_ButtonOnHover, null, Menu_Instances.Status_AtrMaxLivesClick));

            pY -= (int)(menuArea.Height * 0.006);
            pX += 4;
            Atributes_maxLives_plus = new Text(FontStatus_Small, pX, pY, "+1");

            //Skills:
            pX = menuArea.Left + lMargin;
            pY = menuArea.Top + (int)(menuArea.Height * 0.50);//50%

            Skills_initial = new Text(FontStatus_BIG,  pX, pY - 10, Lang.cur.UMIEJETNOSCI__U);
            Skills = new Text(FontStatus_Small,  pX + 7, pY, " "+Lang.cur.MIEJETNOSCI);

            //Skill Boost:
            pY += (int)(menuArea.Height * 0.07);
            Skill s = gm.PC.hasSkill(Skill.skillNames.Boost);
            int lvl = s == null ? 1 : s.level + 1;
            Skills_boost = new Text(FontStatus_Little,  pX, pY, Lang.cur.TRAMPY_MOCY+"("+lvl+")");
            
            pX += (int)(menuArea.Width * 0.55);
            pY -= (int)(menuArea.Height * 0.02);

            Obj root = new Obj("../data/Textures/GAME/GUI/STAR_GRAY.dds", pX, pY, Obj.align.CENTER_X); root.isGUIObjectButUnscaled = true;
            Obj rootOn = new Obj("../data/Textures/GAME/GUI/STAR_COLOR.dds", pX, pY, Obj.align.CENTER_X); rootOn.isGUIObjectButUnscaled = true;
            Obj rootClick = new Obj("../data/Textures/GAME/GUI/STAR_COLOR_BIG.dds", pX, pY, Obj.align.CENTER_X); rootClick.isGUIObjectButUnscaled = true;

            pX = 32; // absolute pre-positioning of whole group of other stars
            int tpCost;
            if (s == null) tpCost = new Skill(Skill.skillNames.Boost).baseTalentPointCost;
            else tpCost = s.talentPointCost;
            for (int i = 1; i < tpCost; i++)
            { // generate rest of 'needed' stars as child of "main" star
                pX +=starXStep; pY = 0;
                add = new Obj("../data/Textures/GAME/GUI/STAR_GRAY.dds", pX, pY, Obj.align.CENTER_X);
                add.isGUIObjectButUnscaled = true; root.addChildObj(add);
                addOn = new Obj("../data/Textures/GAME/GUI/STAR_COLOR.dds", pX, pY, Obj.align.CENTER_X);
                addOn.isGUIObjectButUnscaled = true;  rootOn.addChildObj(addOn);
                addClick = new Obj("../data/Textures/GAME/GUI/STAR_COLOR_BIG.dds", pX, pY, Obj.align.CENTER_X);
                addClick.isGUIObjectButUnscaled = true; rootClick.addChildObj(addClick);         
            }
            menu_status.addItem(new Menu_Item("raiseBoost", root, rootOn, rootClick, Menu_Instances.Status_ButtonOnHover, null, Menu_Instances.Status_SkillBoost));

            //Skill IceBolt:
            pY = menuArea.Top + (int)(menuArea.Height * 0.64);//64%
            pX = menuArea.Left + lMargin;
            s = gm.PC.hasSkill(Skill.skillNames.IceBolt);
            lvl = s == null ? 1 : s.level + 1;
            Skills_icebolt = new Text(FontStatus_Little,  pX, pY, Lang.cur.LODOWY_POCISK+"(" + lvl + ")");

            pX += (int)(menuArea.Width * 0.55);
            pY -= (int)(menuArea.Height * 0.02);

            root = new Obj("../data/Textures/GAME/GUI/STAR_GRAY.dds", pX, pY, Obj.align.CENTER_X); root.isGUIObjectButUnscaled = true;
            rootOn = new Obj("../data/Textures/GAME/GUI/STAR_COLOR.dds", pX, pY, Obj.align.CENTER_X); rootOn.isGUIObjectButUnscaled = true;
            rootClick = new Obj("../data/Textures/GAME/GUI/STAR_COLOR_BIG.dds", pX, pY, Obj.align.CENTER_X); rootClick.isGUIObjectButUnscaled = true;

            pX = 32; // absolute pre-positioning of whole group of other stars

            if (s == null) tpCost = new Skill(Skill.skillNames.IceBolt).baseTalentPointCost;
            else tpCost = s.talentPointCost;
            for (int i = 1; i < tpCost; i++)
            { // generate rest of 'needed' start as child of "main" star
                pX += starXStep; pY = 0;
                add = new Obj("../data/Textures/GAME/GUI/STAR_GRAY.dds", pX, pY, Obj.align.CENTER_X);
                add.isGUIObjectButUnscaled = true; root.addChildObj(add);
                addOn = new Obj("../data/Textures/GAME/GUI/STAR_COLOR.dds", pX, pY, Obj.align.CENTER_X);
                addOn.isGUIObjectButUnscaled = true; rootOn.addChildObj(addOn);
                addClick = new Obj("../data/Textures/GAME/GUI/STAR_COLOR_BIG.dds", pX, pY, Obj.align.CENTER_X);
                addClick.isGUIObjectButUnscaled = true; rootClick.addChildObj(addClick);
            }
            menu_status.addItem(new Menu_Item("raiseBoost", root, rootOn, rootClick, Menu_Instances.Status_ButtonOnHover, null, Menu_Instances.Status_SkillIceBolt));

            //TO MENU BUTTON:
            pX = menuArea.Left+35; pY = menuArea.Bottom-210;
            Obj toMenu = new Obj("../data/Textures/GAME/GUI/STATUSCREEN_TOMENU.dds", pX, pY, Obj.align.CENTER_X);
            toMenu.isGUIObjectButUnscaled = true;
            Obj toMenuHover = new Obj("../data/Textures/GAME/GUI/STATUSCREEN_TOMENU_ACTIVE.dds", pX, pY, Obj.align.CENTER_X);
            toMenuHover.isGUIObjectButUnscaled = true;
            menu_status.addItem(new Menu_Item("toMenu", toMenu, toMenuHover, toMenuHover, Menu_Instances.Status_ButtonOnHover, null, Menu_Instances.Status_exitToMenuClick));

            // Set Text working layer back to default:
            Text.currentWorkingLayer = -1;
        }
        public void renderContents()
        {
            if (Level_initial == null) return;
            //level:
            Level_initial.Update();
            Level.Update();
            //talent pts
            TalentPts_initial.Update();
            TalentPts_initial2.Update();
            TalentPts.Update();
            for (int i = 0; i < TalentPtsStars.Count; i++)
                TalentPtsStars[i].prepareRender();
            //atributes:
            Atributes_initial.Update();
            Atributes.Update();
            Atributes_speed.Update();
            Atributes_speed_val.Update();
            Atributes_speed_plus.Update();

            Atributes_maxLives.Update();
            Atributes_maxLives_val.Update();
            Atributes_maxLives_plus.Update();

            Skills_initial.Update();
            Skills.Update();
            Skills_boost.Update();
            Skills_icebolt.Update();

            pause.prepareRender();
        }
    }
}
