using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Class Manages the whole render, Obj's are pushed to render queue on update
    /// and then rendered from queue onRender event one by one starting from first to last (top layer) with highest number.
    /// 
    /// </summary>
    class Viewport : engineReference
    {
        static private List<Viewport> viewportsList = new List<Viewport>();
        static public System.IO.StreamWriter scaleLog = new System.IO.StreamWriter("scaleLog.txt",false);
        // default window sizes:
        public const int guiBase_width = 1280;
        public const int guiBase_height = 800;
        public const int guiBase_height_MENUOVERRIDE = 800;
        static int render_width = 1280;
        static int render_height = 800;
        static int screen_width;
        static int screen_height;

        bool fullscreen;
        /// <summary>
        /// This value is properly updated when resoulution is changed
        /// </summary>
        public int width 
        {
            get { return render_width;}
            set { render_width = value;}
        }
        /// <summary>
        /// This value is properly updated when resoulution is changed
        /// </summary>
        public int height
        {
            get { return render_height; }
            set { render_height = value; }
        }
       
        List<List<Obj>> rendered_objects = new List<List<Obj>>();
        List<Obj> onceRendered_objects = new List<Obj>();
        private TextManager txtMgr;
        private GameMap currentGameMap;
        public bool isFading { get { return isFadingOut | isFadingIn; } }//if viewport is fading still render it!
        private bool isFadingOut;//
        private bool isFadingIn;//
        private double fadingAlphaStep; // value that will be added/removed every frame
        public bool partialViewport; // if it's true it will render his object don't matter what
                                     // usable fe. with rendering new music track title, some gui on game screen
        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="w">window width</param>
        /// <param name="h">window heigh</param>
        /// <param name="f">fullscreen mode?</param>
        public Viewport(int w, int h,bool f)
        {
            render_height = engine.Height;
            render_width = engine.Width;
            fullscreen = f;
           // _game.setScreenMode(fullscreen);
            screen_width = DisplayDevice.Default.Width;
            screen_height = DisplayDevice.Default.Height;
            //DisplayDevice.Default.ChangeResolution(render_width, render_height, 32, 100);
            engine.Height = render_height; engine.Width = render_width;
            engine.X = 0; engine.Y = 0;
            txtMgr = engine.textManager;
            viewportsList.Add(this);            
        }
        /// <summary>
        /// takes care of Fading-IN/OUT
        /// </summary>
        public void Update()
        {
            Obj someObj=null;
            if (isFading)
            {
                someObj = getAnyRenderedObject();
                if (someObj == null) return; //no objects, no update!
            }

            if ((isFadingIn) && (rendered_objects.Count > 0))
            {
                int newAlpha = (int)(someObj.getCurrentTexAlpha() + fadingAlphaStep);
                if (newAlpha < 255)
                {
                    for (int i = 0; i < rendered_objects.Count; i++)
                        for (int j = 0; j < rendered_objects[i].Count; j++)
                            rendered_objects[i][j].setAllTexsAlpha((byte)newAlpha);
                }
                else
                {
                    for (int i = 0; i < rendered_objects.Count; i++)
                    {
                        for (int j = 0; j < rendered_objects[i].Count; j++)
                        {
                            rendered_objects[i][j].setAllTexsAlpha(255);
                            isFadingIn = false;
                        }
                    }
                }
            }
            else if ((isFadingOut) && (rendered_objects.Count > 0))
            {
                int newAlpha = (int)(someObj.getCurrentTexAlpha() - fadingAlphaStep);
                if (newAlpha > 0)
                    for (int i = 0; i < rendered_objects.Count; i++)
                        for (int j = 0; j < rendered_objects[i].Count; j++)
                            rendered_objects[i][j].setAllTexsAlpha((byte)newAlpha);
                else
                    for (int i = 0; i < rendered_objects.Count; i++)
                    {
                        for (int j = 0; j < rendered_objects[i].Count; j++)
                        {
                            isFadingOut = false;
                            Clear();//faded out, clear all objects from render queue
                        }
                    }
            }
            else
            {
                isFadingIn = isFadingOut = false; //no objects, turn it off!
            }
        }
        /// <summary>
        /// Renders all objects added to viewport(s)
        /// NEW CONCEPT:
        /// Now viewport is divided into layers, so object in layers are rendered at the end of frame in order
        /// from 0 to last one definied.
        /// after rendering, Obj property addedToViewport is changed to false, if next time it will still be false
        /// Obj will be removed from renderQueue cause it means that it wasn't prepared to render(updated) in last frame.
        /// </summary>
        static public void Render()
        {
            Camera activeCam = engine.gameCamera;


            Obj cur;
            // Rendering objects in viewports loop:
            // i - current rendering layer
            // j - current viewport from viewportsList
            // k - current Obj rendered from actual Viewport layer
            for (int i = 0; i < Layer.MAX; i++)
            {
                for (int j = 0; j < viewportsList.Count;j++ )
                {   // Some special threatment for game viewport:
                    if ((i == 0) && (viewportsList[j].partialViewport == false) && (engine.gameManager != null) && viewportsList[j] == engine.gameViewport)
                    {   
                        engine.gameManager.prepareRender();
                        engine.gameManager.Map.renderBackground();
                    }
                    
                    // check if this layer don't exceed count of layers in current viewport
                    if (viewportsList[j].rendered_objects.Count > i)
                    {   
                        for (int k = 0; k < viewportsList[j].rendered_objects[i].Count; k++)
                        {
                            cur = viewportsList[j].rendered_objects[i][k];
                            if (cur == null || cur.addedToViewport == false || cur.preparedToRender == false)
                            {  // Remove object from viewport
                                cur.addedToViewport = false; 
                                viewportsList[j].rendered_objects[i].RemoveAt(k); 
                                k--; 
                            }
                            if (cur.isGUIObject) cur.Render(0, 0);
                            else
                            {
                                cur.Render(activeCam.camOffsetX, activeCam.camOffsetY);
                                cur.preparedToRender = false; // set to false, cause if it's still false in next call of Render()
                                                              // it will means that object wasn't udpated, so it shouldn't be rendered anymore
                            }
                        }
                    }
                }
                //render text's overlaying objects:(if current viewport is global, not partial
                engine.textManager.Render(i);
                // render lights from light engine if this is lightning engine layer:
                if (i == Layer.lightningEngine)
                    engine.lightEngine.Render();
            }

            // Render once rendered objects (overlaying)
            for (int j = 0; j < viewportsList.Count; j++)
            {
                for (int i = 0; i < viewportsList[j].onceRendered_objects.Count; i++)
                {
                    viewportsList[j].onceRendered_objects[i].Render();
                    viewportsList[j].onceRendered_objects[i].Free();
                    viewportsList[j].onceRendered_objects.RemoveAt(i); i--;
                }
                viewportsList[j].onceRendered_objects.Clear();
            }

        }
        /// <summary>
        /// Removing all objects from rendered list, runing Clear for each
        /// TODO: Dispose memory used by objects!!!
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < rendered_objects.Count; i++)
                for (int j = 0; j < rendered_objects[i].Count; j++)
                rendered_objects[i][j].Free();
            
            rendered_objects.Clear();

        }
        public void addObject(Obj o)
        {
            if (o.renderOnce == true)
                onceRendered_objects.Add(o);
            else
            {   //create layers if there's less than needed:
                while (o.layer >= rendered_objects.Count)
                    rendered_objects.Add(new List<Obj>());
                // if Layer.currentlyWorkingLayer is >= 0 then override passed object layer by value stored in this global.
                int layer = Layer.currentlyWorkingLayer >= 0 ? Layer.currentlyWorkingLayer : o.layer;
                o.layer = layer;
                while (layer >= rendered_objects.Count) rendered_objects.Add(new List<Obj>());
                rendered_objects[layer].Add(o);
                o.addedToViewport = true; // FIX: In menu Obj aren't prepared to render at first, they just being added by this method
            }
        }
        /// <summary>
        /// it will remove first file founded with this path
        /// </summary>
        /// <param name="path"></param>
        public void removeObjectByFilePath(string path)
        {
            for (int i = rendered_objects.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < rendered_objects[i].Count; j++)
                {
                    if (rendered_objects[i][j].texturePath == path)
                    {
                        rendered_objects[i][j].addedToViewport = false;
                        rendered_objects[i][j].preparedToRender = false;
                        rendered_objects[i].RemoveAt(j);
                        break;
                    }
                }
            }
        }
        public void setFadeOut(double step)
        {
            isFadingOut = true;
            fadingAlphaStep = step;
        }
        public void setFadeIn(double step)
        {
            isFadingIn = true;
            fadingAlphaStep = step;
            //start from 0 alpha:
            for (int i = 0; i < rendered_objects.Count; i++)
                for (int j = 0; j < rendered_objects[i].Count; j++) 
                rendered_objects[i][j].setAllTexsAlpha(0);
        }

        /// <summary>
        /// sets current game map so it will be included in rendering process
        /// </summary>
        /// <param name="map"></param>
        public void setGameMap(GameMap map)
        {
            currentGameMap = map;
        }

        /// <summary>
        /// Rescaling & repositioning of all objects in this viewport
        /// </summary>
        /// <param name="res"></param>
        public void adaptToNewResolution()
        {
            render_width = Profile.currentProfile.config.options.graphics.resolution.Width;
            render_height = Profile.currentProfile.config.options.graphics.resolution.Height;
            for (int i = 0; i < rendered_objects.Count; i++)
                for (int j = 0; i < rendered_objects[i].Count; j++)
                rendered_objects[i][j].guiObjRescale();
        }

        public void prepareMenuViewportObjects()
        {
            if (this != engine.menuViewport) return;

            if (engine.gameState == Game.game_state.Menu)
                for (int i = 0; i < rendered_objects.Count; i++)
                    for (int j = 0; j < rendered_objects[i].Count; j++)
                        if (rendered_objects[i][j].addedToViewport == true)
                            rendered_objects[i][j].prepareRender();
        }

        private Obj getAnyRenderedObject()
        {
            Obj someObj = null; int k = 0, l = 0; //k - list<list> iterator; l - objs iterator
            while (someObj == null && k != -1)
            {
                if (rendered_objects.Count > 0 && l < rendered_objects[k].Count) l++;
                else if (k < rendered_objects.Count) { k++; l = 0; }
                else return null;
                someObj = rendered_objects[k][l];
            }
            return someObj;
        }
    }
}
