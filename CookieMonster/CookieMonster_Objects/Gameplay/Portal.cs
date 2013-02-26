using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    class Portal
    {
        static private int portalsCount = 0;
        static private string PATH_PORTAL_PREFIX = "../Data/Textures/GAME/SOB/PORTAL_";
        static int maxPortalsImages = 3;//Portal_1-3_A0-n.dds (thatswhatimtalkinbout)
        private Color _portalColor;
        private bool _finished = false; // when its finished new Portal on list will be created
        private Obj[] _portalOBJ = new Obj[2];
        public Obj portalObj1 { get { return _portalOBJ[0]; } }
        public Obj portalObj2 { get { return _portalOBJ[1]; } }

        public Point gridPt1
        {
            get
            {
                if (portalObj1 == null) return new Point(-1,-1);
                return new Point((portalObj1.x + (GameManager.gridSize / 2)) / GameManager.gridSize,
                                 (portalObj1.y + (GameManager.gridSize / 2)) / GameManager.gridSize);
            }
        }
        public Point gridPt2
        {
            get
            {
                if (portalObj2 == null) return new Point(-1, -1);
                return new Point((portalObj2.x + (GameManager.gridSize / 2)) / GameManager.gridSize,
                                 (portalObj2.y + (GameManager.gridSize / 2)) / GameManager.gridSize);
            }
        }
        
        public Portal(int x,int y, Color c)
        {
            _portalColor = c;
            _portalOBJ[0] = new Obj(PATH_PORTAL_PREFIX + portalsCount % maxPortalsImages + "_A0.dds", x, y, Obj.align.CENTER_BOTH);
            
            //visual stuff:
            _portalOBJ[0].setTexAniFPS(12);

            createPortalLight(x, y,portalsCount);

            portalsCount++;
            
        }
        
        public Color portalColor
        {
            get { return _portalColor; }
        }
        public bool matchWithPortal(int x, int y, Color c, int inListIdx)
        {
            if ( (_portalColor == c) && (_portalOBJ[1]==null) )
            {
                _portalOBJ[1] = new Obj(PATH_PORTAL_PREFIX + inListIdx % maxPortalsImages + "_A0.dds", x, y, Obj.align.CENTER_BOTH);
                //visual stuff:
                _portalOBJ[1].setTexAniFPS(12);
                //_portalOBJ[1].width = 60; _portalOBJ[1].height = 60;
                //_portalOBJ[1].x -= 6; _portalOBJ[1].y -= 6;
                createPortalLight(x, y,inListIdx);
                _finished = true;
                return true;
            }
            else
                return false;
        }
        public bool portalFinished()
        { return _finished; }
        

        // public Point teleportPoint(Point startPoint)
        // {
        //     if (startPoint.Equals(_pos[0]))
        //         return _pos[1];
        //     else
        //         return _pos[0];
        // }
        public void Render()
        {
            // if portal is unfinished do not render!
            if (_portalOBJ[1] == null) return;
            _portalOBJ[0].prepareRender();
            _portalOBJ[1].prepareRender();

        }
        public static void resetPortalsCount()
        {
            portalsCount = 0;
        }
        private static void createPortalLight(int x, int y, int portalN)
        {
            //Create light for portal:
            Light portal = new Light(eLightType.DYNAMIC, new radialGradient(new OpenTK.Vector2(x, y), 115f,
                                      new OpenTK.Vector4(0.2f, 0.5f, 1f, 1f),
                                      new OpenTK.Vector4(0, 0f, 0.4f, 0f)));
            portal.scaleLightAni[0] = 0.8f;
            portal.scaleLightAni[1] = 1.3f;
            portal.scaleLightAni[2] = 1.1f;
            portal.scaleLightAni[3] = 0.9f;
            portal.scaleLightAni[4] = 1.4f;
            portal.scaleLightAni[5] = 1.15f;
            portal.scaleLightAni[6] = 1.6f;
            portal.scaleLightAni.aniFps = 2.00f;
            portal.scaleLightAni.aniType = eLightAniType.loop;
            portal.scaleLightAni.shuffle();

            portal.colorLightAni.aniType = eLightAniType.loop;
            portal.colorLightAni.aniFps = 0.45f;
            //portal color specyfics:
            switch (portalN & maxPortalsImages)
            {
                case 0: //blue
                    portal.colorLightAni[0] = new lightColors(Color.FromArgb(60, 80, 200, 255),
                                                             Color.FromArgb(0, 0, 100, 255));
                    portal.colorLightAni[1] = new lightColors(Color.FromArgb(75, 0, 120, 255),
                                                             Color.FromArgb(0, 0, 100, 255));
                    portal.colorLightAni[2] = new lightColors(Color.FromArgb(66, 0, 180, 255),
                                                             Color.FromArgb(0, 0, 120, 255));
                    break;
                case 1: // green                    
                    portal.colorLightAni[0] = new lightColors(Color.FromArgb(58, 100, 255, 120),
                                                             Color.FromArgb(0, 100, 255, 20));
                    portal.colorLightAni[1] = new lightColors(Color.FromArgb(65, 150, 255, 150),
                                                             Color.FromArgb(0, 100, 255, 50));
                    portal.colorLightAni[2] = new lightColors(Color.FromArgb(60, 120, 200, 140),
                                                             Color.FromArgb(0, 100, 255, 20));
                    break;
                case 2:  // red
                    portal.colorLightAni[0] = new lightColors(Color.FromArgb(60, 255, 170, 170),
                                                             Color.FromArgb(0, 255, 100, 100));
                    portal.colorLightAni[1] = new lightColors(Color.FromArgb(70, 255, 140, 140),
                                                             Color.FromArgb(0, 255, 100, 100));
                    portal.colorLightAni[2] = new lightColors(Color.FromArgb(59, 255, 160, 150),
                                                             Color.FromArgb(0, 255, 100, 100));
                    break;
                default: break;
            }
            portal.colorLightAni.shuffle();
        }
    }
}
