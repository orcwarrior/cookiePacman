using System;
using System.Collections.Generic;

using System.Text;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Camera class handles all camera movements (by offseting objects in viewport). 
    /// when Obj being rendered, it acess camOffsetX/Y and adds it to position of rendered object.
    /// </summary>
    class Camera : engineReference
    {
        public enum eType { STATIC, FOLLOWS_PLAYER }
        public eType type = eType.FOLLOWS_PLAYER;

        public int camOffsetX { get; private set; }
        public int camOffsetY { get; private set; }
        int toleranceField = (4 * GameManager.gridSize) / 2;
        int heroLocationBooster;
        public Camera(eType t)
        {
            type = t;
        }
        public Camera()
        {
            type = eType.FOLLOWS_PLAYER;
            centrize();
        }

        public void Update()
        {
            if (type == eType.FOLLOWS_PLAYER)
            {
                //TODO: change pos by steps? less moving in time
                //cause it makes game lag teribly...
                //return;
                int UnChanged = 0;
                const int MOVE_STEP = 2;
                GameManager GM = engine.gameManager;
                Viewport VP = engine.activeViewport;
                int oldX = camOffsetX, oldY = camOffsetY;
                if (GM == null) return; //no game - no camera centering
                if (GM.PC == null) return; //no hero - no camera centering

                if (camOffsetX + toleranceField < (engine.Width / 2 - GM.PC.pX + GameManager.gridSize))
                {
                    int a = camOffsetX + engine.Width;
                    int b = (GM.Map.mapWidth - 1) * GameManager.gridSize;
                    if (camOffsetX < GameManager.gridSize)//hero moving left
                        camOffsetX += MOVE_STEP + heroLocationBooster * 10;
                    else UnChanged += 1;
                }
                else if (camOffsetX - toleranceField > (engine.Width / 2 - engine.gameManager.PC.pX - GameManager.gridSize))
                {
                    int rMapEdge = camOffsetX + (GM.Map.mapWidth + 1) * GameManager.gridSize;
                    if (rMapEdge >= engine.Width)
                        camOffsetX -= MOVE_STEP + heroLocationBooster * 10;//move right camera
                    else UnChanged += 1;
                }
                else UnChanged += 1;
                
                if (camOffsetY + toleranceField + engine.gameManager.PC.pY < engine.Height / 2)
                {
                    if (camOffsetY < -GameManager.gridSize)
                    {
                        camOffsetY += MOVE_STEP + heroLocationBooster * 10; // move UP(?)
                    }
                    else UnChanged += 1;
                }
                else if (camOffsetY - toleranceField + engine.gameManager.PC.pY > engine.Height / 2)
                {
                    int rMapEdge = camOffsetY + (GM.Map.mapHeight + 1) * GameManager.gridSize;
                    if (rMapEdge >= engine.Height)
                        camOffsetY -= MOVE_STEP + heroLocationBooster * 10; // move DOWN(?)
                    else UnChanged += 1;
                }
                else UnChanged += 1;

                if (UnChanged == 2) heroLocationBooster = 0;

                //move Lights:
                engine.lightEngine.moveLights(new OpenTK.Vector2(camOffsetX-oldX,camOffsetY-oldY));
            }
        }
        public void correctForNewLevel()
        {
            centrize();
        }

        private void centrize()
        {
            Viewport v = engine.activeViewport;
            GameMap m = engine.gameManager.Map;
            int oldX = camOffsetX, oldY = camOffsetY;
            camOffsetX = (engine.Width - m.mapWidth * GameManager.gridSize) / 2;
            camOffsetY = (engine.Height - m.mapHeight * GameManager.gridSize) / 2;
            heroLocationBooster = 1;
            //move Lights:
            engine.lightEngine.moveLights(new OpenTK.Vector2(camOffsetX - oldX, camOffsetY - oldY));
        }
        internal void resetPos()
        {
            camOffsetX = 0;
            camOffsetY = 0;
        }
        public void Move(int x, int y)
        {
            camOffsetX += x;
            camOffsetY += y;
        }
        public void SetPos(int x, int y)
        {
            camOffsetX = x;
            camOffsetY = y;
        }
    }
}