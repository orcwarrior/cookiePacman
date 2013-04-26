using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{

    /// <summary>
    /// Each object of this class is responsible for the object displayed 
    /// on the screen, it is quite a complicated structure as the object can 
    /// be extended to be able to do idle animations (idleAni), animation texture 
    /// which is another complex class (Obj_texAni) and animations of 
    /// position, scale, and transparency (Obj_Animation).
    /// </summary>
    class Obj : engineReference
    {
        #region Obj_fields
        static Random variatonizer = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        static private UInt32 id_ctr = 1;

        public UInt32 id { get; private set; }
        public int layer { get; set; }//layer (in Viewport) of texture first 0 layer is drawn, then 1, then..
        public bool addedToViewport { get; set; }
        public bool preparedToRender { get; set; }
        public bool hasAnimatedTexture { get { return texAni.Count != 0; } }

        public double orginalWidth { get; private set; }
        public double orginalHeight { get; private set; }

        public enum align { LEFT, CENTER_X, CENTER_Y, CENTER_BOTH, RIGHT };
        public align objAlign { get; private set; }

        public Obj parentObj { get; private set; } //parent obj that this obj will be relatively positioned to
        public List<Obj> childObjs { get; private set; } //u can add child objects there that will be rendered next to this main
        //object, note that they pos will be threaten relatively, not absolute
        public Obj_Animation objAnimation { get { return ani; } }

        public string texturePath { get { return tex.bitmapPath; } }

        private double scaleX = 1.0, scaleY = 1.0;
        private Obj_texAni texAni = null;
        private Obj_Animation ani = null;

        private Engine.Image tex;// Engine.Image of this object (it's texture)

        private double vposx, vposy;//0-0px 1.0-x/y size of screen
        private int posx, posy; //actual position on screen

        private Viewport _myViewport;
        public Viewport myViewport
        {
            get { return _myViewport; }
            set
            {
                if (_myViewport == value) return; // it will don't change anything
                if (_myViewport != null) _myViewport.removeObject(this);
                _myViewport = value; addedToViewport = false;
            }
        }
        /// <summary>
        /// If it's set to false, object will not render.
        /// </summary>
        public bool visible { get; set; }

        #region IdleAni struct
        idleAni idleAnimation;
        public delegate void funcOnIdle();
        private class idleAni
        {
            public bool objInIdleAni;
            public Obj idleObj;
            public Timer delayTimer;   // time that will need to be passed before idle obj will be rendered
            public Timer idleAniTimer; // duration of idleAni
            public funcOnIdle runOnIdle;
        }
        #endregion
        #endregion

        #region Obj_Get/Set
        public double vx
        {
            get { return vposx; }
            set { vposx = value; posx = (int)((double)engine.Width * value); }
        }
        public double vy
        {
            get { return vposy; }
            set { vposy = value; posy = (int)((double)engine.Height * value); }
        }
        public int x
        {
            get { return posx; }
            set { posx = value; if (value == 0)vposx = 0.0; else vposx = (double)(engine.Width / value); }
        }
        public int y
        {
            get { return posy; }
            set { posy = value; if (value == 0)vposy = 0.0; else vposy = (double)(engine.Height / value); }
        }
        //positions used when creating object:
        public int orgX { get; private set; }
        public int orgY { get; private set; }
        public int width
        {
            get
            {
                if (!texAni.enabled) return tex.w;
                else return texAni[texAni.currentFrame].w;
            }
            set
            {
                int oldW = width;
                //re-centerize Obj by moving positon by proper value:
                if ((objAlign == align.CENTER_BOTH) || (objAlign == align.CENTER_X))
                {
                    x -= (value - oldW) / 2;
                }
                else if (objAlign == align.RIGHT)
                {
                    x -= (value - oldW);
                }
                if (!texAni.enabled)
                {

                    tex.SetSize(value, tex.h);
                }
                else
                {
                    for (int i = 0; i < texAni.Count; i++)
                    {
                        texAni[i].SetSize(value, texAni[i].h);
                    }
                }
            }
        }
        public int height
        {
            get
            {
                if (!texAni.enabled) return tex.h;
                else return texAni[texAni.currentFrame].h;
            }
            set
            {
                int oldH = height;
                //re-centerize Obj by moving positon by proper value:
                if ((objAlign == align.CENTER_BOTH) || (objAlign == align.CENTER_Y))
                {
                    y -= (value - oldH) / 2;
                }

                if (!texAni.enabled)
                {
                    tex.SetSize(tex.w, value);
                }
                else
                {
                    for (int i = 0; i < texAni.Count; i++)
                    {
                        texAni[i].SetSize(texAni[i].w, value);
                    }
                }
            }
        }
        private double[] _scale;
        public double[] scale
        {
            get
            {
                if (_scale == null)
                {
                    _scale = new double[2];
                    _scale[0] = _scale[1] = 1.0;
                }
                return _scale;
            }
            set
            {
                if (_scale == null) _scale = new double[2];
                _scale = value;
                width = (int)(orginalWidth * value[0]);
                height = (int)(orginalHeight * value[1]);
                scaleX = value[0]; scaleY = value[1];
            }
        }
        /// <summary>
        /// This scale is based on scale[2]
        /// so f.e.: Scale = 2.0 is the same as
        /// scale = {2.0,2.0} (pseudo)
        /// ---
        /// Scale will be set Relatively (multipiled by current scale value)
        /// </summary>
        public double ScaleRel
        {
            get { return Math.Max(scale[0], scale[1]); }
            set { double[] s = new double[2]; s[0] = scale[0] * value; s[1] = scale[1] * value; scale = s; }
        }
        /// <summary>
        /// This scale is based on scale[2]
        /// so f.e.: Scale = 2.0 is the same as
        /// scale = {2.0,2.0} (pseudo)
        /// ---
        /// Scale will be set Absolute (multipiled by orginal width/height values)
        /// </summary>
        public double ScaleAbs
        {
            get { return Math.Max(scale[0], scale[1]); }
            set { double[] s = new double[2]; s[0] = value; s[1] = value; scale = s; }
        }
        /// <summary>
        /// Returns id of currently used openGL texture (for binding and other usesful stuff)
        /// </summary>
        public int glTexID
        {
            get { return (int)tex.texture; }
        }
        #endregion

        #region Obj_Constructors
        /// <summary>
        /// blank constructor
        /// </summary>
        public Obj()
        {
            visible = true;
            id = id_ctr;
            id_ctr++;
            layer = Layer.objDefaultLayer;
            texAni = new Obj_texAni(this, "");
            // Add object to current viewport
            _myViewport = engine.activeViewportOrAny;
        }
        /// <summary>
        /// Create Obj base on existing openGL Texture.
        /// </summary>
        /// <param name="glTexID">openGL Texture ID</param>
        /// <param name="_x">x position</param>
        /// <param name="_y">y position</param>
        /// <param name="algn">align of object</param>
        public Obj(int glTexID, int _x, int _y, Obj.align algn)
            : this()
        {
            tex = new Engine.Image(glTexID);
            tex.SetBlending();

            x = _x; y = _y;
            orgX = x; orgY = y;
            texAni = new Obj_texAni(this, "");

            objAlign = algn;
            applyAlignCorrection();
            orginalWidth = width;
            orginalHeight = height;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipath">Path to image</param>
        /// <param name="vx">virtual x (0.0-1.0) of screen</param>
        /// <param name="vy">virtual y</param>
        /// <param name="algn">align of object</param>
        public Obj(string ipath, double _vx, double _vy, align algn)
            : this()
        {
            tex = new Engine.Image();
            tex.Load(ipath);
            tex.SetBlending();

            vx = _vx; vy = _vy;
            orgX = x; orgY = y;
            texAni = new Obj_texAni(this, ipath);

            objAlign = algn;
            applyAlignCorrection();
            orginalWidth = width;
            orginalHeight = height;
        }
        public Obj(string ipath, double _vx, double _vy, align algn, bool isGUIObj)
            : this(ipath, _vx, _vy, algn)
        {
            _isGUIObject = isGUIObj;
            guiObjRescale();
        }
        public Obj(string ipath, int _x, int _y, align algn)
            : this()
        {
            ipath = variableTexture(ipath);//if texture has "V0" in name it will random name from V0-Vn
            tex = new Engine.Image();
            tex.Load(ipath);
            tex.SetBlending();

            x = _x; y = _y;
            orgX = x; orgY = y;
            texAni = new Obj_texAni(this, ipath);

            objAlign = algn;
            applyAlignCorrection();
            orginalWidth = tex.w;
            orginalHeight = tex.h;
        }
        public Obj(string ipath, int _x, int _y, align algn, bool isGUIObj)
            : this(ipath, _x, _y, algn)
        {
            _isGUIObject = isGUIObj;
            guiObjRescale();
        }
        #endregion

        public void setVisibleWithChilds(bool val)
        {
            visible = val;
            if (childObjs == null) return;
            foreach (Obj o in childObjs)
                o.visible = val;
        }
        public void addChildObj(Obj child)
        {
            if (childObjs == null)
                childObjs = new List<Obj>();
            childObjs.Add(child);
            child.setParent(this);
        }
        public void setParent(Obj parent)
        {
            parentObj = parent;
        }
        /// <summary>
        /// Method will prepare render for current viewport (apply transformations, step into new texture frame, etc.)
        /// </summary>
        public void prepareRender()
        {
            if (!visible) {preparedToRender=false; return;}

            if ( (parentObj != null)&&(!parentObj.preparedToRender) ) {preparedToRender=false; return;}
            
            preparedToRender = true;
            if (!addedToViewport)
            {   //add object to viewport if it's not already in.
                if (myViewport != null) myViewport.addObject(this);
                else    engine.activeViewportOrAny.addObject(this);
                addedToViewport = true;
            }

            int oldX = x, oldY = y;
            //if(!objInViewport(activeCam)) return;
            //compute animation frame:
            if (ani != null && (engine.gameManager == null || !engine.gameManager.gamePaused || !ani.isIngameAnimation))
                ani.computeFrame();

            if (!texAni.prepareRender(x, y))
            {
                if (idleAnimation != null)
                {
                    if (((idleAnimation.delayTimer <= 0) || (!idleAnimation.delayTimer.enabled)) && (!idleAnimation.objInIdleAni))
                    {// object has to be turned into idle ani:
                        idleAnimation.idleAniTimer.start();
                        idleAnimation.objInIdleAni = true;
                        idleAnimation.idleObj.prepareRender();

                        if (idleAnimation.runOnIdle != null)
                            idleAnimation.runOnIdle();
                    }
                    else if (idleAnimation.objInIdleAni)
                    {
                        if ((idleAnimation.idleAniTimer > 0) && (idleAnimation.idleAniTimer.enabled))
                        {
                            idleAnimation.idleObj.prepareRender();
                        }
                        else // goes back to default ani + reset timer:
                        {
                            idleAnimation.delayTimer.start();
                            idleAnimation.objInIdleAni = false;
                            idleAnimation.idleObj.preparedToRender = false;
                            idleAnimation.idleObj.addedToViewport = false;
                        }
                    }
                }
            }
            // Prepares render of childs if there is some of them
            if (childObjs != null)
                for (int i = 0; i < childObjs.Count; i++)
                    childObjs[i].prepareRender();
            x = oldX; y = oldY;
        }
        /// <summary>
        /// Draws object, CALL ONLY BY VIEWPORT CLASS!!!
        /// </summary>
        public void Render(int offX, int offY)
        {
            if (hasAnimatedTexture)
                tex = texAni[texAni.currentFrame];

            if (parentObj != null)
                tex.Draw(x + offX + parentObj.x, y + offY + parentObj.y);
            else
                tex.Draw(x + offX, y + offY);

            //Draw child objects (if present):
            // if (childObjs != null)
            //     for (int i = 0; i < childObjs.Count; i++)
            //     {
            //         childObjs[i].prepareRender();
            //         childObjs[i].Render();
            //     }
        }

        private bool objInViewport(Camera activeCam)
        {
            if ((x + activeCam.camOffsetX > engine.Width)
            && (x + width + activeCam.camOffsetX > engine.Width))
                return false;
            if ((y + activeCam.camOffsetY > engine.Height)
            && (y + height + activeCam.camOffsetY > engine.Height))
                return false;
            return true;
        }
        public void Free()
        {
            addedToViewport = false;
            //TODO: Reimplement it, fix
            //if (tex != null)
            //    tex.Free();
            //if (texAni != null)
            //    for (int i = 0; i < texAni.Count; i++)
            //        texAni[i].Free();
        }

        // Animation of manipulations of pos and scale TODO: opacity manipulations
        #region Ani methods
        public void addAni(Obj_Animation a)
        {
            ani = a;
        }
        public void addAniKeyframe(int tX, int tY, double tS)
        {
            ani.addKeyframe(tX, tY, tS);
        }
        #endregion

        // Classic animation of changing textures (gif-alike) generated automatically 
        // if there is _A0 as name prefix
        #region texAni methods
        public void setTexAniLoopType(Obj_texAni.eLoopType typ)
        {
            if (texAni != null)
                texAni.loopType = typ;
        }
        public void setTexAniFPS(int fps)
        {
            if (texAni != null)
                texAni.FPS = fps;
        }

        public bool texAniFinished()
        {
            if ((texAni.loopType == Obj_texAni.eLoopType.NONE) && (texAni.currentFrame == texAni.Count - 1))
                return true;
            else
                return false;
        }
        /// <summary>
        /// setting current frame of animation to first(0)
        /// </summary>
        public void restartTexAni()
        {
            texAni.currentFrame = 0;
        }
        public void setTexAniFrame(int frame)
        {
            if (frame < texAni.Count)
                texAni.currentFrame = frame;
        }
        public int getTexAniFrame()
        {
            return texAni.currentFrame;
        }
        public void setTexAniControlledExternal()
        {
            texAni.isControlledExternal = true; // so it willn't calculate frames by self, but render current frame
            // till it's value will change externally.
        }
        #endregion
        public void setIdleAni(Obj iObj, Timer idleAniAfter, Timer idleAniTime, funcOnIdle runOnIdle)
        {
            idleAnimation = new idleAni();
            idleAnimation.objInIdleAni = false;
            idleAnimation.idleObj = iObj;
            idleAnimation.delayTimer = idleAniAfter;
            idleAnimation.idleAniTimer = idleAniTime;
            idleAnimation.delayTimer.start();
            idleAnimation.runOnIdle = runOnIdle;
        }
        /// <summary>
        /// Swaps current image with new one, returining old
        /// </summary>
        /// <param name="_new"></param>
        /// <returns></returns>
        public Engine.Image swapTexture(string _newPath)
        {
            Engine.Image _new = new Engine.Image();
            Engine.Image old = tex;
            tex = _new;
            tex.Load(_newPath);
            tex.SetBlending();

            texAni = new Obj_texAni(this, _newPath);
            return old;
        }
        public void afterResize()
        {

        }
        public void setCurrentTexAlpha(byte alpha)
        {
            if (!texAni.enabled)
            {
                tex.SetBlending(alpha);
            }
            else
            {
                texAni[texAni.currentFrame].SetBlending(alpha);
            }
        }
        public byte getCurrentTexAlpha()
        {
            if (!texAni.enabled)
            {
                return tex.a;
            }
            else
            {
                return texAni[texAni.currentFrame].a;
            }
        }
        public void setAllTexsAlpha(byte alpha)
        {
            if (!texAni.enabled)
            {
                tex.SetBlending(alpha);
            }
            else
            {
                for (int i = 0; i < texAni.Count; i++)
                    texAni[i].SetBlending(alpha);
            }
        }
        public override string ToString()
        {
            if (tex.bitmapPath == null) return "GL Texture ID: " + tex.texture + "(" + x + "," + y + ")";

            string filename = tex.bitmapPath.Substring(tex.bitmapPath.LastIndexOfAny(new char[] { '/', '\\' }));
            return filename + "(" + x + "," + y + ")";
        }
        private string variableTexture(string path)
        {
            List<string> varNames = new List<string>();
            string hlp = "";
            hlp = (string)path.Clone();
            //NO A0 in name? so this got to be static textureg
            if (hlp.LastIndexOf("V0") <= 0)
                return path;
            else
            {
                int idx = hlp.LastIndexOf("V0");
                varNames.Add(hlp);//adds V0
                string hlp2 = "";
                hlp2 = hlp.Remove(idx + 1, 1); hlp2 = hlp2.Insert(idx + 1, "1");
                int i = 1;
                while (System.IO.File.Exists(hlp2))
                {
                    i++;
                    varNames.Add(hlp2);
                    hlp2 = hlp.Remove(idx + 1, 1); hlp2 = hlp2.Insert(idx + 1, i.ToString());

                };
                //get random name from list:
                return varNames[variatonizer.Next(i)];
            }
        }

        // if flag is true, after rendering it will be removed from viewport
        // + it will be renderedFrom separate onceRenderedObj List (to clear after each render
        private bool _renderOnce; public bool renderOnce { get { return _renderOnce; } }
        /// <summary>
        /// It's automatically add's Obj to viewport render queue
        /// </summary>
        public void setRenderOnce()
        {
            _renderOnce = true;
            engine.activeViewport.addObject(this);
        }
        /// <summary>
        /// Changes current frame of animation to some random number from ani range
        /// </summary>
        public void Desynchronize()
        {
            if (texAni != null)
                texAni.currentFrame = variatonizer.Next(texAni.Count);
        }

        //GUI Objects: Dont give a fuck of camera position caues it's always on screen at exact position

        private bool _isGUIObject;
        public void guiObjRescale()
        {
            if (!_isGUIObject) return;
            //store old resolution
            int oResX = Profile.currentProfile.config.options.graphics.oldResolution.Width;
            int oResY = Profile.currentProfile.config.options.graphics.oldResolution.Height;
            //get new resolution
            int nResX = Profile.currentProfile.config.options.graphics.resolution.Width;
            int nResY = Profile.currentProfile.config.options.graphics.resolution.Height;

            // Scaling:
            //first, scale to multipiler
            ScaleAbs = ScaleAbs; //it's based on orginalHeight/Width and' yep this should do the job ;) trust me!
            //set scale to resolution
            int w = width, h = height;
            width = (int)(width * ((double)nResX / Viewport.guiBase_width));
            if (engine.activeViewport.GetHashCode() == engine.menuViewport.GetHashCode())
                height = (int)(height * ((double)nResY / Viewport.guiBase_height_MENUOVERRIDE));
            else
                height = (int)(height * ((double)nResY / Viewport.guiBase_height));
            //repositioning:
            posx = (int)((float)orgX / Viewport.guiBase_width * nResX);
            posy = (int)((float)orgY / Viewport.guiBase_width * nResX);
            applyAlignCorrection();

            string line = "Obj:" + tex.bitmapPath.Substring(17) + ":: Scale(" + w.ToString() + "x" + h.ToString() + ")>>(" + width.ToString() + "," + height.ToString() + ")";
            line += " || Pos(" + orgX + "," + orgY + ")>>(" + posx + "," + posy + ")";
            Viewport.scaleLog.WriteLine(line);
        }
        public bool isGUIObject
        {
            get { return _isGUIObject; }
            set
            {
                if ((_isGUIObject == false) && (value == true))
                {
                    guiObjRescale();
                }
                _isGUIObject = value;
            }
        }
        public bool isGUIObjectButUnscaled
        {
            set { _isGUIObject = value; }
        }

        public void Rotate(float deg)
        {
            //tex.setRotationOrgin(width / 2, height / 2);
            tex.rotation = deg;

            for (int i = 0; i < texAni.Count; i++)
                texAni[i].rotation = deg;
        }
        public void Rotate(float deg, Point orgin)
        {
            tex.setRotationOrgin(orgin.X, orgin.Y);
            tex.rotation = deg;
            for (int i = 0; i < texAni.Count; i++)
            {
                texAni[i].setRotationOrgin(orgin.X, orgin.Y);
                texAni[i].rotation = deg;
            }

        }
        public Obj shallowCopy()
        {
            return (Obj)this.MemberwiseClone();
        }
        public void changeVisual(string newPath)
        {
            Engine.Image img = new Engine.Image();
            img.Load(newPath);
            if (img != null)
            {
                img.SetBlending(255);
                tex.Free();
                tex = img;
            }
        }
        /// <summary>
        /// Used when creating object, when need to move object depend of position
        /// *RIGHT: (the "first" pixel will be the last so move whole object by (-width,-height)
        /// *CENTER(X/Y) (the "first" pixel will be in the middle, so move whole object by (-width/2,-height/2)
        /// </summary>
        public void applyAlignCorrection()
        {

            //correct position from align:
            if ((objAlign == align.CENTER_BOTH) || (objAlign == align.CENTER_X))
                x -= (width / 2);
            else if (objAlign == align.RIGHT)
                x -= width;

            if ((objAlign == align.CENTER_BOTH) || (objAlign == align.CENTER_Y))
                y -= (height / 2);
            else if (objAlign == align.RIGHT)
                y -= height;
        }
        /// <summary>
        /// try to sets destroyed visual(for object like bridges, etc
        /// </summary>
        /// <returns>return true if destroyed visual was found and set</returns>
        public bool setDestroyed()
        {
            String destPath = new String(tex.bitmapPath.ToCharArray());
            int insert = destPath.LastIndexOf(".");
            destPath = destPath.Insert(insert, "_DESTROYED");
            if (System.IO.File.Exists(destPath) == true)
            {
                changeVisual(destPath);
                return true;
            }
            return false;
        }

        internal void Render()
        {
            Render(0, 0);
        }

        internal void OverrideTexcoords(float u1, float u2, float v1, float v2)
        {
            tex.OverrideTexcoords(u1, u2, v1, v2);
            // Build texcoordinates of all animation
            // textures too:
            if (hasAnimatedTexture)
            {
                for (int i = 0; i < texAni.Count; i++)
                {
                    texAni[i].OverrideTexcoords(u1, u2, v1, v2);
                    //texAni[i].rebuild = false;
                }
            }
            //tex.rebuild = false; // prevent VBO from rebuilding see docu. on rebuild
        }
    }


    /// <summary>
    /// Class object is created when there is found texture ani at creation of Obj
    /// (texture has *_A0* in name, plus there was found next animation frames (A1,A2...))
    /// </summary>
    class Obj_texAni : engineReference
                     // this class is more like a struct by public fields
    {                // but whateva' if texAni field in Obj class is private ;)
        private List<Engine.Image> textures;
        public enum eLoopType { NONE, DEFAULT, REWIND, DISPOSE };
        public bool enabled = false;
        public int _fps = 5;
        public int currentFrame = 0;
        public eLoopType loopType = eLoopType.DEFAULT;
        public bool isRewinding = false;
        public bool isControlledExternal;
        public int FPS
        {
            get { return _fps; }
            set { if (value > 0) _fps = value; }
        }
        public int Count
        {
            get { if (textures == null)return 0; return textures.Count; }
        }
        private Obj owner = null;
        public Obj_texAni(Obj o)
        {
            owner = o;
        }
        public Obj_texAni(Obj o, string texPath)
        {
            owner = o;
            FPS = 5;
            List<string> texNames = extractTextureAniFrames(texPath);
            if (texNames != null)
            {
                textures = new List<Engine.Image>();
                currentFrame = 0;
                enabled = true;
                for (int i = 0; i < texNames.Count; i++)
                {
                    textures.Add(new Engine.Image());
                    textures[i].Load(texNames[i]);
                    textures[i].SetBlending();
                }
            };
        }
        public bool prepareRender(int px, int py)
        {
            if (enabled)
            {
                int x, y;
                Camera activeCam = engine.gameCamera;
                if (owner.isGUIObject) activeCam = new Camera(Camera.eType.STATIC);

                    x = px + activeCam.camOffsetX;
                    y = py + activeCam.camOffsetY;
                    if ((textures[0].w != textures[currentFrame].w) || (textures[0].h != textures[currentFrame].h))
                    {
                        if ((owner.objAlign == Obj.align.CENTER_BOTH) || (owner.objAlign == Obj.align.CENTER_X))
                        {   //centering 9                       obj by first frame is evil, recalculate offset always!
                            x = px + (int)owner.orginalWidth / 2 + activeCam.camOffsetX - textures[currentFrame].w / 2;
                        }
                        else if (owner.objAlign == Obj.align.RIGHT)
                        {
                            x = px + (int)owner.orginalWidth + activeCam.camOffsetX - textures[currentFrame].w;
                        }
                        if ((owner.objAlign == Obj.align.CENTER_BOTH) || (owner.objAlign == Obj.align.CENTER_Y))
                        {
                            y = py + (int)owner.orginalHeight / 2 + activeCam.camOffsetY - textures[currentFrame].h / 2;
                        }
                    }
                // Calculate next tex frame to render:
                if (isControlledExternal == false)
                {
                    int curframe = engine.frames;
                    int gameFPS = (int)engine.RenderFrequency;//TODO: PRECISE ..hmm.. (int)engine.RenderFrequency;
                    this._fps = FPS;
                    int hlp = gameFPS / FPS; if (hlp <= 0) hlp = 1;
                    //todo: correct !!!
                    if (curframe % hlp == 0)//time to draw further tex frame
                    {
                        if (currentFrame + 1 == textures.Count) //last frame reached, time to do sth
                        {
                            if (loopType == eLoopType.DEFAULT)
                                currentFrame = 0;
                            else if (loopType == eLoopType.DISPOSE)
                                owner.setRenderOnce();//<- this simple trick does job of removing whole obj
                            else if (loopType == eLoopType.REWIND)
                            {
                                isRewinding = true;
                                currentFrame--;
                            }
                        }
                        else if (!isRewinding) currentFrame++;
                        if (isRewinding) // texAni is rewinding back:
                        {
                            if (currentFrame > 0) // still not first frame, continue rewinding
                                currentFrame--;
                            else // first frame reached, stop rewinding
                            {
                                isRewinding = false;
                                currentFrame++;
                            }
                        } // texAni goes forward as usual:
                        // else currentFrame = (currentFrame + 1) % textures.Count;
                    }
                }
                //TODO: !!! DRAW !!!
                //textures[currentFrame].Draw(x, y);
                return true;
            };
            return false;
        }
        public Engine.Image this[int index]
        {
            get { return textures[index]; }
        }

        public void reScale(double w, double h)
        {
                for (int i = 0; i < textures.Count; i++)
                {
                    textures[i].scaleX *= (float)w;
                    textures[i].scaleY *= (float)h;
                }
        }
        // private methods:
        private List<string> extractTextureAniFrames(string f)
        {
            List<string> ret = new List<string>();
            string hlp = "";
            hlp = (string)f.Clone();
            //NO A0 in name? so this got to be static textureg
            if (hlp.LastIndexOf("_A0") <= 0)
                return null;
            else
            {
                int idx = hlp.LastIndexOf("_A0");
                ret.Add(hlp);
                string hlp2 = "";
                hlp2 = hlp.Remove(idx + 2, 1); hlp2 = hlp2.Insert(idx + 2, "1");
                int i = 2;
                while (System.IO.File.Exists(hlp2))
                {
                    ret.Add(hlp2);
                    hlp2 = hlp.Remove(idx + 2, 1); hlp2 = hlp2.Insert(idx + 2, i.ToString());
                    i++;
                };
                return ret;
            };

        }
    }
}
