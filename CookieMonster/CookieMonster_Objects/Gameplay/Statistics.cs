﻿using System;
using System.Collections.Generic;

using System.Drawing;
using System.Text;
using QuickFont;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{
    class Statistics
    {
        static public uint ptsPerCookie = 10;
        public uint points { get; private set; }
        public uint lvlPoints { get; private set; } //only current level points

        private uint _eatenCookies; public uint eatenCookies { get { return _eatenCookies; } }

        QFont ptFont = TextManager.newQFont("Rumpelstiltskin.ttf", 20, true);
        
        public Statistics()
        {
            points = 0;
            lvlPoints = 0;
            _eatenCookies=0;
        }

        public Statistics(Savegame sav)
        {
            lvlPoints = 0;
            points = sav.player.score;
            //TODO: Well, this sucks :(
            _eatenCookies = 0;
        }
        public void newLevel() { lvlPoints = 0; }
        public void addPoints(uint v)
        {
            Camera curCam = Game.self.gameCamera;
            points += v;
            lvlPoints += v;
            ptFont.Options.Colour = new OpenTK.Graphics.Color4(255, 255, 255, 200);
            //give exp pts to hero:
            bool newLevel = Game.self.gameManager.PC.addExp((int)v);
            string pipe = "+" + v.ToString() + "pts.";
            if (newLevel) pipe+= "\nNOWY POZIOM!";

            //Text msg = Game.self.textMenager.produceText(ptFont, pipe , (float)(Game.self.gameManager.PC.pX + 30 + curCam.camOffsetX), (float)(Game.self.gameManager.PC.pY + curCam.camOffsetY + 10));
            //Text msg = new Text(ptFont,  (float)(Game.self.gameManager.PC.pX + 30 + curCam.camOffsetX), (float)(Game.self.gameManager.PC.pY + curCam.camOffsetY + 10), "+" + v.ToString() + "pts.");
            Text msg = new Text(ptFont, (float)(Game.self.gameManager.PC.pX + 30 + curCam.camOffsetX), (float)(Game.self.gameManager.PC.pY + curCam.camOffsetY + 10), pipe);
            msg.setLifeTime(600);
            msg.setAnimationMove(new Point(0, -3));
            //Game.self.textMenager.addText(msg);
            
            
        }
        public void addEatenCookies(uint v)
        {
            _eatenCookies += v;
            addPoints(v * ptsPerCookie); //every cookie is 10 pts.
        }

    }
}