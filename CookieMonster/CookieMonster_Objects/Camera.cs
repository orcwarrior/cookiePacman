using System;
using System.Collections.Generic;

using System.Text;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{
    class Camera : engineReference
    {
        public enum eType { STATIC, FOLLOWS_PLAYER }
        public eType type = eType.FOLLOWS_PLAYER;

        int _camOffsetX = 0; public int camOffsetX { get { return _camOffsetX; } }
        int _camOffsetY = 0; public int camOffsetY { get { return _camOffsetY; } }
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
                int oldX = _camOffsetX, oldY = _camOffsetY;
                if (GM == null) return; //no game - no camera centering
                if (GM.PC == null) return; //no hero - no camera centering

                if (_camOffsetX + toleranceField < (VP.width / 2 - GM.PC.pX + GameManager.gridSize))
                {
                    int a = _camOffsetX + VP.width;
                    int b = (GM.Map.mapWidth - 1) * GameManager.gridSize;
                    if (_camOffsetX < GameManager.gridSize)//hero moving left
                        _camOffsetX += MOVE_STEP + heroLocationBooster * 10;
                    else UnChanged += 1;
                }
                else if (_camOffsetX - toleranceField > (VP.width / 2 - engine.gameManager.PC.pX - GameManager.gridSize))
                {
                    int rMapEdge = _camOffsetX + (GM.Map.mapWidth + 1) * GameManager.gridSize;
                    if (rMapEdge >= VP.width)
                        _camOffsetX -= MOVE_STEP + heroLocationBooster * 10;//move right camera
                    else UnChanged += 1;
                }
                else UnChanged += 1;

                if (_camOffsetY + toleranceField + engine.gameManager.PC.pY < VP.height / 2)
                {
                    if (_camOffsetY < -GameManager.gridSize)
                    {
                        _camOffsetY += MOVE_STEP + heroLocationBooster * 10; // move UP(?)
                    }
                    else UnChanged += 1;
                }
                else if (_camOffsetY - toleranceField + engine.gameManager.PC.pY > VP.height / 2)
                {
                    int rMapEdge = _camOffsetY + (GM.Map.mapHeight + 1) * GameManager.gridSize;
                    if (rMapEdge >= VP.height)
                        _camOffsetY -= MOVE_STEP + heroLocationBooster * 10; // move DOWN(?)
                    else UnChanged += 1;
                }
                else UnChanged += 1;

                if (UnChanged == 2) heroLocationBooster = 0;

                //move Lights:
                engine.lightEngine.moveLights(new OpenTK.Vector2(_camOffsetX-oldX,_camOffsetY-oldY));
            }
        }
        private void centrize()
        {
            Viewport v = engine.activeViewport;
            GameMap m = engine.gameManager.Map;
            int oldX = _camOffsetX, oldY = _camOffsetY;
            _camOffsetX = (v.width - m.mapWidth * GameManager.gridSize) / 2;
            _camOffsetY = (v.height - m.mapHeight * GameManager.gridSize) / 2;
            heroLocationBooster = 1;
            //move Lights:
            engine.lightEngine.moveLights(new OpenTK.Vector2(_camOffsetX-oldX,_camOffsetY-oldY));
        }
        public void correctForNewLevel()
        {
            centrize();
        }

        internal void resetPos()
        {
            _camOffsetX = 0;
            _camOffsetY = 0;
        }
        public void Move(int x, int y)
        {
            _camOffsetX += x;
            _camOffsetY += y;
        }
        public void SetPos(int x, int y)
        {
            _camOffsetX = x;
            _camOffsetY = y;
        }
    }
}