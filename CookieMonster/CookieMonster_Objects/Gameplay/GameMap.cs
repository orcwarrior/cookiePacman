using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{

    class GameMap : engineReference
    {
        private GameManager gameMgr;
        private Bitmap _interpretationMap; // generated automatically
        private Obj[] staticMapParts;
        private Obj[,] mapRenderQueueFirst;//Darkness Objects
        private Obj[,] mapRenderQueue; //queue is sorted but need to put in movable/dynamic objects like player, enemies
        private Obj[,] torches; //Animated Torch
        private Obj[,] paths; //paths on map [and bridges too] (rendered firstly after background)
        private List<Portal> _portals = new List<Portal>(); //portale przenosza z pewnego pkt. mapy na inny;
        private Obj _bg; public Obj background { get { return _bg; } }
        public Waynet wayNetwork{get; private set;}
        static public String MapsPath = "../data/Maps/";
        public int mapWidth { get { return _interpretationMap.Width; } }
        public int mapHeight { get { return _interpretationMap.Height; } }
        public int cookiesCount { get; private set; }
        public bool waynetActive { get; private set; }
        static int level = 10;

        //internal OpenGL stuff:
        /// <summary>
        /// Static part of buffer that will be rendered always the same (in exact position
        /// and with exact same image) - used for performance optimization.
        /// </summary>
        private int[] _staticBufferTextures; private int staticTexSize = 512;
        private int staticTexCntX, staticTexCntY, realYstartPos;
        private int yStaticMapShift = -286;//282


        public GameMap(string bitPath,GameManager g)
        {
            //reset all Lights:
            engine.lightEngine.clearAllLights();
            engine.lightEngine.lightAddStrength = 0.6f;
            engine.lightEngine.lightMulStrength = 0.2f;
            level++;
            neighborReport.map = this;
            Portal.resetPortalsCount();

            _bg = new Obj(PATH_BACKGROUND_TILE, 0.5, 0.5, Obj.align.CENTER_BOTH);
            gameMgr = g;
            _interpretationMap = (Bitmap)Bitmap.FromFile(bitPath);
            cookiesCount = 0;
            if (gameMgr.statistics != null)
            {
                gameMgr.statistics.newLevel();//resets lvlPoints
            }
            wayNetwork = new Waynet(this);
            generateMap();
            generateStaticMapToTexture();
        }
        /// <summary>
        /// Function checks point passed as argument goes
        /// through portal, if that's true it will return Point with
        /// coordinates on the other side of portal.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point tryGoThroughPortal(Point entry)
        {
            Point p = entry;
            for (int i = 0; i < _portals.Count; i++)
            {
                if (_portals[i].gridPt1.Equals(p))
                {
                    p.X = _portals[i].gridPt2.X;
                    p.Y = _portals[i].gridPt2.Y;
                    return p;
                }
                else if (_portals[i].gridPt2.Equals(p))
                {
                    p.X = _portals[i].gridPt1.X;
                    p.Y = _portals[i].gridPt1.Y;
                    return p;
                }
            }
            return entry;
        }
        
        public objType getObjTypeFromPixel(int x, int y)
        {
            if ((x < 0) || (x >= _interpretationMap.Width) || (y < 0) || (y >= _interpretationMap.Height))
            {/* new DebugMsg("tried to read map pixel out of bounds!");*/ return objType.OTHER; }
            return colorIs(_interpretationMap.GetPixel(x, y));
        }
        public Color getPXColor(int x, int y)
        {
            if ((x < 0) || (x >= _interpretationMap.Width) || (y < 0) || (y >= _interpretationMap.Height))
                return Color.Transparent;
            return _interpretationMap.GetPixel(x, y);
        }
        public Portal getPortalByColor(Color c)
        {
            for (int i = 0; i < _portals.Count; i++)
                if (_portals[i].portalColor == c) return _portals[i];
            return null;
        }
        public void generateMap()
        {
            //if (level == 1) waynetActive = false;
            waynetActive = true;
            mapRenderQueue = new Obj[mapWidth, mapHeight]; // new List<Obj>();
            mapRenderQueueFirst = new Obj[mapWidth, mapHeight];
            torches = new Obj[mapWidth, mapHeight];
            //lensFlares = new List<Obj>();
            paths = new Obj[mapWidth, mapHeight];
            // TODO: reverse rendering in X axis in render method
            for (int y = 0; y < _interpretationMap.Height; y++)
                for (int x = _interpretationMap.Width-1; x >= 0; x--)//in choosen isometric view it will be look much better
                {
                    Color c = _interpretationMap.GetPixel(x, y);
                    objType t = colorIs(c);
                    AddObject(t,x,y);
                }
                wayNetwork.generateWays();
                shortestWayAlgoritm.wnet = wayNetwork;
                ShortestWaysTable.Initialize();
                shortestWayAlgoritm New;
                System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
                timer.Start();
                for (int i = 0; i < wayNetwork.Count; i++)
                    New = new shortestWayAlgoritm(wayNetwork[i], null);
                timer.Stop();
                long time = timer.ElapsedMilliseconds;
                wayNetwork.renderWaynet = false;

        }
        /// <summary>
        /// When map is fully loaded this function will create texture with rendered
        /// static part of this GameMap.
        /// </summary>
        private void generateStaticMapToTexture()
        {
            // Clear both viewports:
            engine.menuViewport.Clear();
            engine.gameViewport.Clear();
            GL.ClearColor(0,0,0,0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            int w = engine.Width, h = engine.Height;
                        //Delete old textures:
            if (_staticBufferTextures != null)
                GL.DeleteTextures(staticTexCntX * staticTexCntY, _staticBufferTextures);

            // Compute real startpoint X ("zero"):
            realYstartPos = engine.Height;
            // openGL coords was making alot of troubles with (0,0) at
            // down left corner of current rendered viewport (not the whole scene!)
            // Finding upper left corner of viewport, which (just after rendering all static Obj's
            // when camera offset is still(0,0) is at the same time upper left corner
            // of whole scene is much more handy, and as i throught - needed anyway.
            // 20:11 2012-10-27
             #region Count needed amount of textures:
            int mapW = (mapWidth-1) * GameManager.gridSize;
            int mapH = mapHeight * GameManager.gridSize;

            staticTexCntX = (int)Math.Ceiling(mapW / (float)staticTexSize);
            staticTexCntY = (int)Math.Ceiling(mapH / (float)staticTexSize);  
            #endregion
            
            // Generate image(s) for storing buffer after rendering objects:
            _staticBufferTextures = new int[staticTexCntX * staticTexCntY+1];
            GL.GenTextures(staticTexCntX * staticTexCntY, _staticBufferTextures);
            // Create array of Obj files:
            staticMapParts = new Obj[staticTexCntX * staticTexCntY + 1];


            // Creating textures from actual gameMap
            // ---
            // How this works(loop2D):
            // 1. Sets new arrayPointer (22*i,22*i)
            // 2. Set new camShift (21+shift=16*i)
            // 3. Render objects on new part of map (>1024x1024px)
            // 4. Put rendered stuff into 4 new textures
            // 5. Quit if all objects was rendered, or start from 1. again.
            
            int objToRenderX = 22;//in one loop, in one row/column
            int objToRenderY = 11;
            int objShiftX = 16;
            int objShiftY = 16;
            // "Height" loop: (only one row per loop)
            for (int i = 0; i < staticTexCntY; i++)
            {
                // 11 - number of objects per one texture:
                int texTillEnd = staticTexCntX;
                // "Width" loop:
                for (int j = 0; j < Math.Ceiling(staticTexCntX/2.0); j++)
                {
                    texTillEnd -= 2; //decrease number of textures till end by two (they're be rendered in this loop)

                    Camera cam = engine.gameCamera;
                    Point arrayPointer = new Point(j * (objToRenderX - 1), i * (objToRenderY - 1));
                    Point camShift = new Point((j * (objToRenderX - 1)) * GameManager.gridSize + j * objShiftX,
                                               (i * (objToRenderY - 1)) * GameManager.gridSize + i * objShiftY);
                    cam.SetPos(-camShift.X, -camShift.Y);
                    //new DebugMsg("Moved camera to: (" + camShift.X + "," + camShift.Y + ")");
                        
                    // render underlying objects:
                    for (int x = arrayPointer.X; (x < arrayPointer.X + objToRenderX && x < mapWidth); x++)
                    {
                        for (int y = arrayPointer.Y; (y < objToRenderY + arrayPointer.Y && y < mapHeight); y++)
                        {
                            if(mapRenderQueueFirst[x,y]!=null)
                                mapRenderQueueFirst[x, y].prepareRender();
                        }
                    } for (int x = arrayPointer.X; (x < arrayPointer.X + objToRenderX && x < mapWidth); x++)
                    {
                        for (int y = arrayPointer.Y; (y < objToRenderY + arrayPointer.Y && y < mapHeight); y++)
                        {
                            if (paths[x, y] != null)
                                paths[x, y].prepareRender();
                        }
                    } for (int x = arrayPointer.X; (x < arrayPointer.X + objToRenderX && x < mapWidth); x++)
                    {
                        for (int y = arrayPointer.Y; (y < objToRenderY + arrayPointer.Y && y < mapHeight); y++)
                        {
                            if (mapRenderQueue[x, y] != null)
                                mapRenderQueue[x, y].prepareRender();
                        }
                    }
                    engine.gameViewport.Render();
 
                    GL.ReadBuffer(ReadBufferMode.Back);
                    // Drawing textures in one main loop  
                    // there is always 2x2 textures to draw
                    // new: 2x2 wasn't working properly, reduced to 2x1
                    for (int _y = 0; (_y < 1 && _y + i < staticTexCntY); _y++)
                    {
                        for (int _x = 0; (_x < 2 && _x + 2*j < staticTexCntX); _x++) //+2 cause of decreasing texTillEnd by two at start of loop
                        { // 'magic' happens here:
                            int x_start = j*2;
                            int y_start = i;
                            /*        *  column  *    *           row           */
                            int idx = (j * 2 + _x) + (i + _y) * staticTexCntX;
                            GL.BindTexture(TextureTarget.Texture2D, _staticBufferTextures[idx]);
                            GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (_x * staticTexSize), realYstartPos - ((_y + 1) * staticTexSize), staticTexSize, staticTexSize, 0);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                            // Create Obj in array of staticMapParts
                            staticMapParts[idx] = new Obj(_staticBufferTextures[idx], (_x * staticTexSize), realYstartPos - ((_y + 1) * staticTexSize), Obj.align.LEFT);
                            
                            // [DEBUG] Save image to file:
                            // GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                            // byte[] raw_img = new byte[staticTexSize * staticTexSize * 32];
                            // unsafe
                            // {
                            //  fixed (byte* p = raw_img)
                            //  {
                            //      IntPtr ptr = (IntPtr)p;
                            //      GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, raw_img);
                            //      Bitmap tex = new Bitmap(staticTexSize, staticTexSize, staticTexSize * 32/8, System.Drawing.Imaging.PixelFormat.Format32bppArgb, ptr);
                            //      tex.Save("stat" + idx + ".png");
                            //  }
                            // }
 
                            //new DebugMsg("Created mapTEX[" + idx + "] - (" + (_x * staticTexSize) + "," + (realYstartPos - ((_y + 1) * staticTexSize)) + ")");
                        }
                    }
                    // For debuging propouses:
                    //engine.SwapBuffers();
                    //for (int z = 0; z < int.MaxValue / 8; z++) ;
                    //engine.SwapBuffers();
                    //engine.activeViewport.Render();
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                    engine.gameViewport.Clear();
                }
            }

            
            //clear back to black:
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.ClipPlane0);

            // Reshape viewport to old shape
            //engine.gameState = oldState;
            
        }
        /// <summary>
        /// Render static part of Map buffer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void _prepareRenderOfMapStaticPart(int _x,int _y)
        {
            int xoff = 0, yoff = realYstartPos - staticTexSize;
            int staticTexSizeY = staticTexSize - 20;
            for (int y = 0; y < staticTexCntY; y++)
            {
                for (int x = 0; x < staticTexCntX; x++)
                { //'magic' happens here: 
                    //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                xoff = (staticTexSize) * x;
                yoff = realYstartPos - staticTexSizeY + (staticTexSizeY) * y;

                staticMapParts[y * staticTexCntX + x].x = xoff;
                staticMapParts[y * staticTexCntX + x].y = yoff;
                staticMapParts[y * staticTexCntX + x].prepareRender();   
                
               }
            }
            //Back to old blend function:
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }
        public void renderBackground()
        {
            int w = engine.Width;
            int h = engine.Height;
            Camera c = engine.gameCamera;
            GL.BindTexture(TextureTarget.Texture2D, background.glTexID);
            GL.Begin(BeginMode.Quads);
            GL.Color4(255, 255, 255, 255);
            /* 0---2 How
             * |  /| Quad
             * | / | isy
             * 1/--3 Made */
            float x = 880;
            float y = 550;
            GL.TexCoord2(0 - c.camOffsetX / x, 1.5 + c.camOffsetY / y);
            GL.Vertex2(0,0);

            GL.TexCoord2(0 - c.camOffsetX / x, 0 + c.camOffsetY / y);
            GL.Vertex2(0,h);

            GL.TexCoord2(1.5 - c.camOffsetX / x, 0 + c.camOffsetY / y);
            GL.Vertex2(w, h);

            GL.TexCoord2(1.5 - c.camOffsetX / x, 1.5 + c.camOffsetY / y);
            GL.Vertex2(w, 0);
            GL.End(); 
        }
        /// <summary>
        /// Prepares Rendering of
        /// *torches
        /// *mobs
        /// *power-ups
        /// *player
        /// </summary>
        public void prepareRender()
        {
            // FIX: We will make sure we passing object 
            // to game Viewport not menu! [BUGFIX]
            Game.game_state oldState = engine.gameState;
            engine.gameState = Game.game_state.Game;
            int i = 0;
            bool pcRendered = false;

            //Torches:
            for (int x = 0; x  < torches.GetUpperBound(0); x++)
            {
               for (int y = 0; y  < torches.GetUpperBound(1); y++)
               {
                   if (torches[x, y] != null)
                       torches[x, y].prepareRender();
               }
            }

            //MOBS:
            for (i = 0; i < gameMgr.sortedEnemiesList.Count;i++)
            {
                if (gameMgr.sortedEnemiesList[i] != null)
                    gameMgr.sortedEnemiesList[i].prepareRender();
            }
            //POWER UPS:
            for (i = 0; i < gameMgr.sortedPowerUpList.Count;i++ )
            {
                if (gameMgr.sortedPowerUpList[i] != null)
                        gameMgr.sortedPowerUpList[i].Render();
            }
            //PLAYER:
            gameMgr.PC.prepareRender();

            //portals: overlaping most of map objects:
            for (i = 0; i < _portals.Count; i++)
                _portals[i].Render();

           // Back to old state:
            engine.gameState = oldState;
        }
        /// <summary>
        /// Method called by Game Viewport
        /// renders static parts of game map (background, static objects)
        /// </summary>
        public void prepareStaticRender()
        {            

            //Renders static parts of map:
            Camera c = engine.gameCamera;
            _prepareRenderOfMapStaticPart(c.camOffsetX, c.camOffsetY + yStaticMapShift);//(c.camOffsetX, c.camOffsetY + );

        }
        #region Creating Map objects readed from interpretationMap pixel data
        private void AddObject(objType typ, int x, int y)
        {
            //Converts x,y map to real x,y offset + adding half of grid size
            //(objects will be rendered as center) -> that will be cool just trust me ;)
            int cx, cy;//converted input x,y offset pos
            cx = x * GameManager.gridSize +(GameManager.gridSize / 2);//centerize it!
            cy = y * GameManager.gridSize +(GameManager.gridSize / 2);//centerize it!
            Color col = _interpretationMap.GetPixel(x, y);
            switch(typ)
            {
                case objType.PORTAL:
                    addPortal(cx, cy, col); break;
                case objType.PLAYER:
                    addPlayer(cx,cy); break;
                case objType.POWERUP:
                    addPowerUP(cx,cy,col); break;
                case objType.ENEMY:
                    addEnemy(cx,cy,col); break;
                case objType.DESTROYABLE|objType.DARKNESS:
                    addBridge(cx, cy,x,y); break;
                case objType.PATH:
                    addPath(cx, cy); break;
               
                default://multi-flags:
                    if (typ == (objType.COLLIDEABLE | objType.CLIFF))
                        addCliff(cx, cy);
                    else if (typ == (objType.COLLIDEABLE | objType.DARKNESS))
                        addDarkness(cx, cy);
                    else if (typ == (objType.COLLIDEABLE | objType.LIGHT_EMITING))
                        addTorch(cx, cy);
                    else if(typ == objType.COLLIDEABLE)
                        addWall(cx, cy); 
                break;
            }
        }

        private void addBridge(int x, int y,int gridx, int gridy)
        {
            if (getObjTypeFromPixel(gridx - 1, gridy) != (objType.DESTROYABLE | objType.DARKNESS))
            {
                paths[gridx, gridy] = new Obj(PATH_BRIDGE_ENDL, x, y, Obj.align.CENTER_BOTH);
                //paths.Add(new Obj(PATH_BRIDGE_ENDL, x, y, Obj.align.CENTER_BOTH));
            }
            else if (getObjTypeFromPixel(gridx + 1, gridy) != (objType.DESTROYABLE | objType.DARKNESS))
            {
                paths[gridx, gridy] = new Obj(PATH_BRIDGE_ENDR, x, y, Obj.align.CENTER_BOTH);
                //paths.Add(new Obj(PATH_BRIDGE_ENDR, x, y, Obj.align.CENTER_BOTH));
            }
            else
            {
                paths[gridx, gridy] = new Obj(PATH_BRIDGE, x, y, Obj.align.CENTER_BOTH);
                //paths.Add(new Obj(PATH_BRIDGE, x, y, Obj.align.CENTER_BOTH));
            };
            
        }

        private void addCliff(int x, int y)
        {
            int gridx = (x-(GameManager.gridSize/2))/GameManager.gridSize;
            int gridy = (y-(GameManager.gridSize/2))/GameManager.gridSize;
            Obj cliff = Cliffs.getCliffVisual(new neighborReport(gridx, gridy, objType.CLIFF));
            if (cliff == null) return;
            cliff = cliff.shallowCopy();
            //path
            //create waypoint(report)
            y -= cliff.height / 2;//FIX
            x -= cliff.width / 2;//FIX
            cliff.x = x;
            cliff.y = y;
            
            //NOTE: From lists to arrays of objects
            mapRenderQueueFirst[x,y] = cliff;
            //mapRenderQueueFirst.Add(cliff);//cliff can be threaten like path
        }

        private void addEnemy(int x, int y,Color c)
        { 
            //color range: (1-126) (BLUE)
            // NORMAL 1-40
            // WIZARD 41-60
            // THIEF 61-80
            // SHOOP 81-100
            // ASSASSIN 101-120
            addPath(x, y);

            y -= GameManager.gridSize / 2;//FIX
            x -= GameManager.gridSize / 2;//FIX
            if (c.B < 41)
                new Enemy(Enemy.enemyType.NORMAL, x, y, 50);
            else if (c.B < 61)
                new Wizard(x, y, 40);
            else if (c.B < 81)
                new Thief(x, y, 50);
            else if (c.B < 101)
                new Lazer(x, y, 55);
            else if (c.B < 121)
                new Quickie(x, y, 61);
        }

        private void addDarkness(int x,int y)
        {
            mapRenderQueueFirst[x,y] = new Obj(PATH_DARKNESS, x, y, Obj.align.CENTER_BOTH); 
            //mapRenderQueueFirst.Add(new Obj(PATH_DARKNESS, x, y, Obj.align.CENTER_BOTH));
        }
        void addPowerUP(int x, int y,Color c)
        {
            //COLORS: R0,G(1-126),B0
            addPath(x, y);
            byte g = c.G;
            if (g<=40)//cookies! yummy
            {
                cookiesCount++;
                new PowerUp(x,y,PATH_PU_COOKIE);
            }
            else if(g<=60)//power pill
            {
                new PowerUp(x, y, PATH_PU_POWERPILL);
            }
            else if(g<=70)//bomb
            {
                new PowerUp(x, y, PATH_PU_BOMB);
            }
            else if (g <= 80)//enemyslower
            {
                new PowerUp(x, y, PATH_PU_ENEMYSLOWER);
            }
            else if (g <= 90)//extra life
            {
                new PowerUp(x, y, PATH_PU_LIFE);
            }
            else if (g <= 100)//extra skill point
            {
                new PowerUp(x, y, PATH_PU_SKILLPOINT);
            };
            
            //TODO: rest
        }

        private void addPath(int x, int y)
        {
            // mapRenderQueue.Add(new Obj(PATH_, x, y, Obj.align.LEFT));
            int gridx = (x-(GameManager.gridSize/2))/GameManager.gridSize;
            int gridy = (y-(GameManager.gridSize/2))/GameManager.gridSize;
            neighborReport report = new neighborReport(gridx, gridy);
            Obj path = new Obj();// = Paths.getPathVisual(report);

            if (Profile.currentProfile.config.options.graphics.renderPaths)
            {
                Obj rp = new Obj();
                rp = Paths.getPathVisual(report);
                if (rp == null) return; // no path, break this right now!
                path = rp.shallowCopy();
                //path
                //create waypoint(report)
                y -= path.height / 2;//FIX
                x -= path.width / 2;//FIX
                path.x = x;
                path.y = y;
                paths[gridx, gridy] = path;
                //paths.Add(path);
            }
            if (report.createWaypoint == true && waynetActive)
            {
                wayNetwork.addWaypoint(new Waypoint(gridx, gridy));
            }
        }

        private void addPlayer(int x, int y)
        {
            addPath(x, y);
            y -= GameManager.gridSize / 2;//FIX
            x -= GameManager.gridSize / 2;//FIX
            gameMgr.InitPC(x, y);
            //dont add player to map render queue, he's moveable so his position in rnd queue will vary!
        }
        private void addPortal(int x,int y, Color c)
        {
            addPath(x, y);
            bool workDone = false;
            for (int i = 0; (i < _portals.Count && workDone == false); i++)
            {
                workDone = _portals[i].matchWithPortal(x, y, c, i);
            }
            if (!workDone) // so we will need to create new portal
            {
                Portal tmp = new Portal(x,y, c);
                _portals.Add(tmp);
            }
        }
         

        private void addWall(int x, int y)
        { //TODO: Unfinished
            int gridx = (x - (GameManager.gridSize / 2)) / GameManager.gridSize;
            int gridy = (y - (GameManager.gridSize / 2)) / GameManager.gridSize;
            mapRenderQueue[gridx,gridy] = new Obj(PATH_STONEWALL, x, y, Obj.align.CENTER_BOTH);
            //mapRenderQueue.Add(new Obj(PATH_STONEWALL, x, y, Obj.align.CENTER_BOTH));
        }
        private void addTorch(int x, int y)
        { //TODO: Unfinished
            int gridx = (x - (GameManager.gridSize / 2)) / GameManager.gridSize;
            int gridy = (y - (GameManager.gridSize / 2)) / GameManager.gridSize;
            addWall(x, y);
            Obj Torch = new Obj(PATH_TORCH, x-2, y-14, Obj.align.CENTER_BOTH);
            Torch.setTexAniFPS(12);
            Torch.Desynchronize();

            torches[gridx,gridy] = Torch;
            //mapRenderQueue.Add(Torch);

            //setup light:
           
            radialGradient torchGrad = new radialGradient(new Vector2(),
                                                          90f,
                                                          new Vector4(1f, 0.8f, 0.2f, 0.65f),
                                                          new Vector4(1f, 0.3f, 0f, 0f),
                                                          BlendingFactorSrc.SrcAlpha,
                                                          BlendingFactorDest.One);
            Light torch = new Light(eLightType.DYNAMIC, torchGrad);
            torch.setPos(new OpenTK.Vector2(x - 2, y - 14));
            torch.reoffsetByCamera = true;
             
            torch.colorLightAni[0] = new lightColors(Color.FromArgb(120, 255, 140, 0),
                                                             Color.FromArgb(14, 255, 20, 0));
            torch.colorLightAni[1] = new lightColors(Color.FromArgb(110, 255, 160, 0),
                                                             Color.FromArgb(9, 255, 40, 0));
            torch.colorLightAni[2] = new lightColors(Color.FromArgb(115, 255, 150, 0),
                                                             Color.FromArgb(7, 235, 30, 0));
            //torch.colorLightAni[3] = new lightColors(Color.FromArgb(140, 255, 45, 4),
            //                                                 Color.FromArgb(0, 235, 120, 0));
            //torch.colorLightAni[4] = new lightColors(Color.FromArgb(150, 255, 135, 74),
            //                                                 Color.FromArgb(0, 235, 120, 0));
            torch.colorLightAni.shuffle();
            torch.slicesNum(20);
            
            
            torch.colorLightAni.aniFps = 2;

            torch.scaleLightAni[0] = 1.0f;
            torch.scaleLightAni[1] = 1.08f;
            torch.scaleLightAni[2] = 1.03f;
            torch.scaleLightAni[3] = 0.95f;
            torch.scaleLightAni[4] = 1.12f;
            torch.scaleLightAni[5] = 1.15f;
            torch.scaleLightAni[6] = 1.14f;
            torch.scaleLightAni[7] = 1.05f;
            torch.scaleLightAni[8] = 0.92f;
            torch.scaleLightAni[9] = 1.01f;
            torch.scaleLightAni[10] = 1.07f;
            torch.scaleLightAni.aniFps = 1;
            torch.scaleLightAni.shuffle();

            //lensFlares.Add(new Obj(PATH_TORCHLIGHT, x, y, Obj.align.CENTER_BOTH));
            // torch light in another List, rendered at least
        }
#endregion

        [Flags]
        public enum objType
        {
            OTHER = 1 << 0, COLLIDEABLE = 1 << 1, DESTROYABLE = 1 << 2, LIGHT_EMITING = 1 << 3,
            POWERUP = 1 << 4, ENEMY = 1 << 5, PORTAL = 1 << 6, PLAYER = 1 << 7, CLIFF = 1 << 8, DARKNESS = 1 << 9,
            PATH = 1 << 10
        }
        private objType colorIs(Color c)
        {
            objType ret = 0;
            UInt32 argb = (UInt32)c.ToArgb();
            //PowerUp?
            UInt32 tmp = argb;
            tmp = tmp | COLOR_POWERUP_RANGE;
            if ((argb | COLOR_POWERUP_RANGE) == (COLOR_POWERUP_BASE | COLOR_POWERUP_RANGE))
            {//check if color is in range
                tmp = (argb & COLOR_POWERUP_RANGE)%0xFF;
                if ((tmp >= COLOR_POWERUP_MIN) && (tmp <= COLOR_POWERUP_MAX))
                    ret = objType.POWERUP;
            }
            //Enemy?
            else if ((argb | COLOR_ENEMY_RANGE) == (COLOR_ENEMY_BASE | COLOR_ENEMY_RANGE))
            {//check if color is in range
                tmp = (argb & COLOR_ENEMY_RANGE) % 0xFF;
                if ((tmp >= COLOR_ENEMY_MIN) && (tmp <= COLOR_ENEMY_MAX))
                    ret = objType.ENEMY;
            }
            //Portal?
            else if ((argb | COLOR_PORTAL_RANGE | COLOR_PORTAL_RANGE2) == (COLOR_PORTAL_BASE | COLOR_PORTAL_RANGE | COLOR_PORTAL_RANGE2))
            {//check if color is in range this time it's double range
                tmp = (argb & COLOR_PORTAL_RANGE) % 0xFF;
                if ((tmp >= COLOR_PORTAL_MIN) && (tmp <= COLOR_PORTAL_MAX))
                {
                    tmp = (argb & COLOR_PORTAL_RANGE2) % 0xFF;
                    if ((tmp >= COLOR_PORTAL_MIN) && (tmp <= COLOR_PORTAL_MAX))
                        return objType.PORTAL;
                }
            };
            //flags:
            if ((argb & CFLAG_COLLIDE) == CFLAG_COLLIDE)
                ret |= objType.COLLIDEABLE;
            if ((argb & CFLAG_DESTROYABLE) == CFLAG_DESTROYABLE)
                ret |= objType.DESTROYABLE;
            if ((argb & CFLAG_LIGHTEMITING) == CFLAG_LIGHTEMITING)
                ret = ret|objType.LIGHT_EMITING;

            if (argb == COLOR_PLAYER)
                return objType.PLAYER;
            if (argb == COLOR_CLIFF)
                return objType.COLLIDEABLE | objType.CLIFF;
            if (argb == COLOR_DARKNESS)
                return objType.COLLIDEABLE | objType.DARKNESS;

            if (ret == objType.DESTROYABLE)//it's a bridge:
                return ret | objType.DARKNESS;

            //at least, it's an path?
            if ((ret == 0) && ((argb & COLOR_NOTHING) != COLOR_NOTHING))
                ret |= objType.PATH;
                
            return ret;

        }

        #region Textures Paths Constants
        static private string PATH_DARKNESS               = "../Data/Textures/GAME/SOB/DARKNESS.dds";
        static private string PATH_STONEWALL              = "../Data/Textures/GAME/SOB/StoneWall_V0.dds";
        static private string PATH_BACKGROUND_TILE        = "../Data/Textures/GAME/BG_Stone_V0.png";
        static private string PATH_TORCH                  = "../Data/Textures/GAME/SOB/Torch_A0.dds";
        static private string PATH_TORCHLIGHT             = "../Data/Textures/GAME/SOB/TorchLight_V0.dds";
        static private string PATH_BRIDGE                 = "../Data/Textures/GAME/SOB/bridge_V0.dds";
        static private string PATH_BRIDGE_ENDL            = "../Data/Textures/GAME/SOB/bridge_endL_V0.dds";
        static private string PATH_BRIDGE_ENDR            = "../Data/Textures/GAME/SOB/bridge_endR_V0.dds";
        static private string PATH_BRIDGEDESTROYED        = "../Data/Textures/GAME/SOB/Bridge_Destroyed_V0.dds";
        static private string PATH_BOARDEDDOORS           = "../Data/Textures/GAME/SOB/BoardedUpDoors_V0.dds";
        static private string PATH_BOARDEDDOORSDESTROYED  = "../Data/Textures/GAME/SOB/BoardedUpDoors_Destroyed_V0.dds";

        //theese static's are public cause PowerUP class need them to recognize which kind of powerUP is being created                                          
        static public string PATH_PU_COOKIE              = "../Data/Textures/GAME/SOB/Cookie_V0.dds";
        static public string PATH_PU_POWERPILL           = "../Data/Textures/GAME/SOB/PowerPill_A0.dds";
        static public string PATH_PU_LIFE                = "../Data/Textures/GAME/SOB/Life_A0.dds";
        static public string PATH_PU_SKILLPOINT               = "../Data/Textures/GAME/SOB/STAR_A0.dds";
        static public string PATH_PU_ENEMYSLOWER         = "../Data/Textures/GAME/SOB/TIME_SLOW.dds";
        static public string PATH_PU_BOMB                = "../Data/Textures/GAME/SOB/Bomb.dds";

        //others
        static public string PATH_BOMBDECAY = "../Data/Textures/GAME/FX/Bomb_Exploded.dds";

        #endregion
        #region Colours Constants
        // POWER-UPs:
        // R0 G(1-40) B0 - Cookie
        // R0 G(41-60) B0 - Power pill
        // R0 G(61-70) B0 - Bomb
        // R0 G(71-80) B0 - EnemySlower
        // R0 G(81-90) B0 - Live
        // R0 G(91-100) B0 - Skill Point

        // Enemies: R0 G0 B(1-126)
        // NORMAL 1-40
        // WIZARD 41-60
        // THIEF 61-80
        // SHOOP 81-100
        // ASSASSIN 101-120

        // Misc:
        // BRIDGE: R0G0B192
        // (WiP)BARRICADE R192G0B192
        //Color Flags:
        static private UInt32 CFLAG_COLLIDE = 0x00C00000;//red = 192
        static private UInt32 CFLAG_LIGHTEMITING = 0x0000C000;//green = 192
        static private UInt32 CFLAG_DESTROYABLE = 0x000000C0;//blue = 192

        //Range Flags:
        // Power up: 127,(128-192),255,255
        static private UInt32 COLOR_POWERUP_BASE = 0xFF000100;//255=a,0,1,0
        static private UInt32 COLOR_POWERUP_RANGE = 0x0000FF00;//(Green)
        static private UInt32 COLOR_POWERUP_MIN = 1;
        static private UInt32 COLOR_POWERUP_MAX = 126;

        static private Color Krzak = Color.FromArgb(255, 0, 255, 0);

        // Enemies: R0 G0 B(1-126) A255
        static private UInt32 COLOR_ENEMY_BASE = 0xFF000001;//,255=a,0,0,1
        static private UInt32 COLOR_ENEMY_RANGE = 0x000000FF;//(Blue)
        static private UInt32 COLOR_ENEMY_MIN = 1;
        static private UInt32 COLOR_ENEMY_MAX = 126;

        // Portals: 127,(128-192),(128-192),255
        static private UInt32 COLOR_PORTAL_BASE = 0xFF008080;//127,0,1,1
        static private UInt32 COLOR_PORTAL_RANGE = 0x0000FF00;//(Green)
        static private UInt32 COLOR_PORTAL_RANGE2 = 0x000000FF;//(,Blue)
        static private UInt32 COLOR_PORTAL_MIN = 128;//Ranges of both color must be the same!!
        static private UInt32 COLOR_PORTAL_MAX = 192;

        //Specific Colors:(test last, don't let it intersect with flags, etc
        //Player
        static private UInt32 COLOR_PLAYER = 0xFFFFFFFF;//white
        static private UInt32 COLOR_CLIFF = 0xFF808080;//gray 50%
        static private UInt32 COLOR_DARKNESS = 0xFF000000;//black 
        static private UInt32 COLOR_NOTHING = 0xFFBFBFBF;//
        #endregion


        internal void tryDestroyObject(int x, int y)
        {
            //new DebugMsg("tryDestroy:(" + x.ToString() + ":" + y.ToString() + ")", DebugLVL.info);
            //object is a bridge:
            if (getObjTypeFromPixel(x, y) == (objType.DESTROYABLE | objType.DARKNESS))
            {
                Color oldCol = _interpretationMap.GetPixel(x, y);
                Color newColor = Color.FromArgb(oldCol.A,192, oldCol.G, oldCol.B);
                _interpretationMap.SetPixel(x, y, newColor);
                Obj bridge = getPathAt(x, y);
                if (bridge != null)
                {
                    bridge.setDestroyed();
                }               
            }
        }

        private Obj getPathAt(int x, int y)
        {
            int grid = GameManager.gridSize;
            if(paths==null) return null;

            // podany gridPos wykracza poza indeksy obecnej tablicy paths:
            if (x > paths.GetUpperBound(0) || y > paths.GetUpperBound(1))
                return null;
            return paths[x, y];
        }

        internal void addBombDecay(int x, int y)
        {
            Obj decay = new Obj(PATH_BOMBDECAY, x, y, Obj.align.LEFT);
            decay.width *=2; decay.height *=2;
            decay.x -= decay.width / 2; decay.y -= decay.height / 2;
            // TODO: sth with that! Like.. creating another on array? List should be ok too...
            // paths.Add(decay);
            
        }
    }
}
