using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// 
    /// BIG FUCKING NOTE: Changing name of this class will made
    /// constructor of lightAniData detecting of passed <T> type
    /// obsolete!!!
    /// </summary>
    class lightColors
    {
        public Color innerColor{get; private set;}
        public Color outerColor { get; private set; }

        public int innerR { get { return innerColor.R; } }
        public int innerG { get { return innerColor.G; } }
        public int innerB { get { return innerColor.B; } }
        public int innerA { get { return innerColor.A; } }

        public int outerR { get { return outerColor.R; } }
        public int outerG { get { return outerColor.G; } }
        public int outerB { get { return outerColor.B; } }
        public int outerA { get { return outerColor.A; } }

        /// <summary>
        /// Creates lightColors from passed as arguments
        /// ARGB colors
        /// (use Color.FromArgb() method)
        /// </summary>
        /// <param name="inner">inner color value</param>
        /// <param name="outer">outer color value</param>
        public lightColors(Color inner, Color outer)
        {
            innerColor = inner;
            outerColor = outer;
        }
        /// <summary>
        /// Mixing this and passed colors returning
        /// mixed colors (mix both inner and outer).
        /// </summary>
        /// <param name="second">color that will be mixed with this</param>
        /// <param name="distribution">mix distribution 0 - 100% of this, 1.0 - 100% of second, etc.</param>
        /// <returns></returns>
        public lightColors mixColors(lightColors c, double distribution)
        {
            Color mixInner = Color.FromArgb( (int)(innerColor.A + (c.innerColor.A - innerColor.A) * distribution),
                                             (int)(innerColor.R + (c.innerColor.R - innerColor.R) * distribution),
                                             (int)(innerColor.G + (c.innerColor.G - innerColor.G) * distribution),
                                             (int)(innerColor.B + (c.innerColor.B - innerColor.B) * distribution));
            
            Color mixouter= Color.FromArgb(  (int)(outerColor.A + (c.outerColor.A - outerColor.A) * distribution),
                                             (int)(outerColor.R + (c.outerColor.R - outerColor.R) * distribution),
                                             (int)(outerColor.G + (c.outerColor.G - outerColor.G) * distribution),
                                             (int)(outerColor.B + (c.outerColor.B - outerColor.B) * distribution));
            return new lightColors(mixInner, mixouter);
            
        }
    }
    public enum eLightDataType { undef, pos, scale, color };
    public enum eLightAniType { once, loop,loopStep2First, pingpong };

    // This class contains only a raw data that will be fully
    // processed by Light class objects itself.
    /// <summary> 
    /// After redesigning of class (to generic) I decided
    /// that this class will calculate everything by itself
    /// and return "ready to use" data to Light.
    /// </summary>
    class lightAniData<T> //where T : Point,lightColors,
    {
        // if this value/object was returned, getting went wrong
        public T obsolete;
        static public Point pointObsolete = Point.Empty;
        static public lightColors colorsObsolete = null;
        static public float scaleObsolete = 0f;

        private List<T> keyframeData;

        private float _aniFps;
        public float aniFps { get { return _aniFps; } set { if (value > 0)_aniFps = value; } }
        private int lastFrame;
        private int nxtFrame;
        private bool lastTimeSwitchedToNextFrame;
        /// <summary>
        /// 0.0 - lastFrame was just reached
        /// 1.0 - next frame is reaching
        /// (now private)
        /// </summary>
        private double frameMixDone
        { 
            get
            {
                double hlp;
                if (lastTimeSwitchedToNextFrame)
                {
                    timeMsAtInit = (System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond);
                    hlp = 0f;
                    lastTimeSwitchedToNextFrame = false;
                }
                else
                {
                    hlp = (System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond);
                    if (timeMsAtInit > hlp) hlp += 60000 - timeMsAtInit;
                    else hlp -= timeMsAtInit;
                    hlp = hlp / (1000.0 / aniFps);
                    //hlp /= 1000.0;
                    hlp = Math.Max(0, Math.Min(1, hlp));
                    if (hlp == 1f) { startNewKeyframe(); hlp = 0f;}
                }
                return hlp;// / (1.0 / aniFps);
            }
        }
        private double timeMsAtInit;

        /// <summary>
        /// Returns current Frame position calculated from last and next
        /// frame and current frameMixDone
        /// </summary>
        public Point currentPos{ get
        {
            if (type == eLightDataType.pos && Count>0)
            {
                double pD = frameMixDone;
                // LOL, casting is fun ^_^
                Point p = (Point)((object)(keyframeData[lastFrame]));
                Point p2 = (Point)((object)(keyframeData[nxtFrame]));
                int x = (int)((1.0 - pD) * p.X + pD * p2.X);
                int y = (int)((1.0 - pD) * p.Y + pD * p2.Y);
                return new Point(x, y);
            }
            return pointObsolete;
        }}
        /// <summary>
        /// Returns current Frame position calculated from last and next
        /// frame and current frameMixDone
        /// </summary>
        public float currentScale
        {
            get
            {
                if (type == eLightDataType.scale && Count>0)
                {
                    double pD = frameMixDone;
                    // yeye more fun...
                    float s = (float)((object)(keyframeData[lastFrame]));
                    float s2 = (float)((object)(keyframeData[nxtFrame]));
                    return (float)((1.0 - pD) * s + pD * s2);
                }
                return scaleObsolete;
            }
        }
        /// <summary>
        /// Returns current Frame position calculated from last and next
        /// frame and current frameMixDone
        /// </summary>
        public lightColors currentColor
        {
            get
            {
                if (type == eLightDataType.color)
                {
                    double pD = frameMixDone;
                    // ... and another one
                    lightColors c = (lightColors)((object)(keyframeData[lastFrame]));
                    lightColors c2 = (lightColors)((object)(keyframeData[nxtFrame]));
                    return c.mixColors(c2, pD);
                }
                return colorsObsolete;
            }
        }

        public eLightAniType aniType;
        public eLightDataType type { get; private set; }

        /// <summary>
        /// If light got a flag destroyAtAniEnd set
        /// to true and all of its aniData has aniHasEnded set to true
        /// light will be destroyed.
        /// NOTE: It can be set to true only when aniLoopType is ONCE
        /// or when the count of ani keyframes is 0.
        /// </summary>
        public bool aniHasEnded { get { if (Count == 0)return true; return _aniHasEnded; } private set { _aniHasEnded = value; } }
        private bool _aniHasEnded;
        /// <summary>
        /// Returns number of keyframes in this Data
        /// </summary>
        public int Count
        {
            get { if (keyframeData != null)return keyframeData.Count; return 0; }
        }
        /// <summary>
        /// If lightData in type which it was
        /// Constructed or set on passed argument
        /// (in T type)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {           
            get
            {
                if (index < Count && index >= 0)
                    return keyframeData[index];
                else
                {
                    throw new ArgumentOutOfRangeException();
                    return obsolete;
                }
            }
            set
            {
                if (index == Count) keyframeData.Add(value);
                else if (index < Count && index >= 0) keyframeData[index] = value;
                else return;

                // At initiation there is no any keyframes at all, so 
                // when we creating keyframes is nice to update nxtFrame
                // value.
                if (lastFrame == nxtFrame && index > 0) nxtFrame++;
            }
        }
        /// <summary>
        /// Class Constructor
        /// Auto detecting of generic passed class works
        /// good with Point and float, but lightColors need
        /// some lil' more threatment, + changing name of this class
        /// will cause failure of auto-detecting type of lightAniData
        /// </summary>
        public lightAniData()
        {            
            if (obsolete is float)
                type = eLightDataType.scale;
            else if (obsolete is Point)
                type = eLightDataType.pos;
            else if (typeof(T).Name.Equals("lightColors"))//(obsolete is lightColors)
                type = eLightDataType.color;
            else
                type = eLightDataType.undef;

            aniType = eLightAniType.loop;
            keyframeData = new List<T>();

            // For initiation, it's perfectly good:
            // (we will change it when setting this[idx] eventually)
            lastFrame = nxtFrame = 0;

            //For initializaton, well it's better when frame start from 0% of done ;p
            timeMsAtInit = (System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond);

        }
        public lightAniData(eLightDataType t)
            : this()
        {
            type = t;
            aniType = eLightAniType.loop;
        }
        public lightAniData(eLightDataType t, eLightAniType a)
            : this(t)
        {
            aniType = a;
        }

        private void startNewKeyframe()
        {
            lastTimeSwitchedToNextFrame = true;
            if (aniType == eLightAniType.loop)
            {
                lastFrame = nxtFrame;
                nxtFrame = (nxtFrame + 1) % Count;
            }
            else if (aniType == eLightAniType.loopStep2First)
            {
                lastFrame = nxtFrame;
                nxtFrame++;
                if(nxtFrame>=Count)
                {
                    lastFrame = 0;
                    nxtFrame = (nxtFrame + 1) % Count;
                }
            }
            else if (aniType == eLightAniType.once)
            {
                lastFrame = nxtFrame;
                if (nxtFrame + 1 < Count) nxtFrame++;
                else aniHasEnded = true;
            }
            else if (aniType == eLightAniType.pingpong)
            {
                if (nxtFrame == Count - 1 || nxtFrame <= lastFrame && nxtFrame != 0) { lastFrame = nxtFrame; nxtFrame = Math.Max(nxtFrame - 1, 0); }
                else if (nxtFrame == 0 || nxtFrame > lastFrame) { lastFrame = nxtFrame; nxtFrame = Math.Min(nxtFrame + 1, Count - 1); }
            }
        }

        /// <summary>
        /// Method will shufle keyframeData Array
        /// Algorithm is based on:
        /// http://blog.thijssen.ch/2010/02/when-random-is-too-consistent.html
        /// </summary>
        internal void shuffle()
        {
            var provider = new System.Security.Cryptography.RNGCryptoServiceProvider();
            int n = Count;
            while (n > 1)
            {
                var box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                var k = (box[0] % n);
                n--;
                var value = keyframeData[k];
                keyframeData[k] = keyframeData[n];
                keyframeData[n] = value;
            }
        }
    }

    public enum eLightType { STATIC, DYNAMIC };
    class Light
    {
        /// <summary>
        /// List of gradients that make up this Light
        /// </summary>
        List<radialGradient> gradsComposition;

        /// <summary>
        /// If Light has some parent(only if dynamic!!!)
        /// it will "follows" position of paren object.
        /// (actually it will copy Pos of parrent in 
        /// update method)
        /// </summary>
        public MOB parent{get; private set;}
        // Size,Positon,shape
        /// <summary>
        /// AbsolutePos - for determining pos of "parent" 
        /// object(like MOB.pos), or pos passed as gradient 
        /// pos in constructor which position is set base on
        /// 
        /// RelativePost - for determining shift from AbsPos (light Animation, etc.)
        /// Move() method change relPos and pass absPos+relPos
        /// </summary>
        OpenTK.Vector2 absPos, relPos;

        float size;

        public lightAniData<Point> posLightAni;
        public lightAniData<float> scaleLightAni;
        public lightAniData<lightColors> colorLightAni;

        public eLightType type { get; private set; }
        /// <summary>
        /// If this bool is set to true and
        /// all of lightAniData obj has aniHasEnded bools
        /// set to true the light will be Destroyed.
        /// </summary>
        public bool destroyLightAtAniEnd;
        /// <summary>
        /// if object is not attached to any MOB
        /// but it's an map-orientet object eg. Portal
        /// DEFAULT: TRUE!!!
        /// </summary>
        public bool reoffsetByCamera = true; 
        /// <summary>
        /// Creates Light base on passed radialGradient
        /// (then add it to the List of gradsComposition)
        /// </summary>
        /// <param name="grad"></param>
        public Light(eLightType typ, radialGradient grad)
        {
            type = typ;
            addLightToLightingEngine();
            size = grad.radius;
            absPos = grad.centerPos;

            gradsComposition = new List<radialGradient>();
            gradsComposition.Add(grad);

            //correct position by current position of camera:
            move(new Vector2(EngineApp.Game.self.gameCamera.camOffsetX, EngineApp.Game.self.gameCamera.camOffsetY));

            // if light is dynamic, create lightAniData(s):
            if (type == eLightType.DYNAMIC)
            {
                scaleLightAni = new lightAniData<float>();
                posLightAni = new lightAniData<Point>();
                colorLightAni = new lightAniData<lightColors>();
            }
        }
        /// <summary>
        /// Creates Light base on passed radialGradient
        /// (then add it to the List of gradsComposition)
        /// </summary>
        /// <param name="grad"></param>
        /// <param name="pos">Light center position</param>
        public Light(eLightType typ, radialGradient grad, Vector2 pos)
            : this(typ, grad)
        {
            absPos = pos;
            grad.Move(pos);//move gradient to pos passed as parameter
        }


        /// <summary>
        /// Creates Light based on list of passed radialGradients
        /// and for sure, adding them to composition of grads in this light.
        /// </summary>
        /// <param name="gradList"></param>
        public Light(eLightType typ, List<radialGradient> gradList)
        {
            type = typ;
            addLightToLightingEngine();
            if (gradList == null)
                throw new ArgumentNullException("radialGradient List passed to Light constructor is null!");
            if (gradList.Count == 0)
                throw new Exception("radialGradient List passed to Light doesn't contain any radialGradients!");
            size = gradList[0].radius;
            absPos = gradList[0].centerPos;

            //correct position by current position of camera:
            move(new Vector2(EngineApp.Game.self.gameCamera.camOffsetX, EngineApp.Game.self.gameCamera.camOffsetY));

            // if light is dynamic, create lightAniData(s):
            if (type == eLightType.DYNAMIC)
            {
                scaleLightAni = new lightAniData<float>();
                posLightAni = new lightAniData<Point>();
                colorLightAni = new lightAniData<lightColors>();
            }
        }
        /// <summary>
        /// Creates Light based on list of passed radialGradients
        /// and for sure, adding them to composition of grads in this light.
        /// </summary>
        /// <param name="gradList"></param>
        public Light(eLightType typ, List<radialGradient> gradList, Vector2 pos)
            : this(typ, gradList)
        {
            absPos = pos;
            for (int i = 0; i < gradList.Count; i++)
                gradList[i].Move(pos);
        }
        /// <summary>
        /// Rendering a light to the buffer
        /// (it just render all radialGradients on list...
        /// </summary>
        public void Render()
        {
            for (int i = 0; i < gradsComposition.Count; i++)
                gradsComposition[i].drawGradientToBuffer();
        }
        public void slicesNum(int slices)
        {
            for (int i = 0; i < gradsComposition.Count; i++)
                gradsComposition[i].setSlicesCount(slices);
        }
        public void setPos(Vector2 pos)
        {
            absPos = pos;
            for (int i = 0; i < gradsComposition.Count; i++)
                gradsComposition[i].Move(pos);
        }
        public void move(Vector2 pos)
        {
            relPos.Add(pos);
            for (int i = 0; i < gradsComposition.Count; i++)
                gradsComposition[i].moveRelatively(pos);
        }
        /// <summary>
        /// Refres pos of gradients
        /// new position will be set based on:
        /// absPos + relPos + camOffset
        /// </summary> 
        private void refreshPos()
        {
            Vector2 camOff = new Vector2(EngineApp.Game.self.gameCamera.camOffsetX, EngineApp.Game.self.gameCamera.camOffsetY);
            for (int i = 0; i < gradsComposition.Count; i++)
            {
                gradsComposition[i].Move(absPos + relPos);
                if(reoffsetByCamera)
                    gradsComposition[i].moveRelatively(camOff);
            }
        }

        /// <summary>
        /// It will set radius of light gradient based on this formula:
        /// size * scale - simple as that :)
        /// NOTE:
        /// size - taken from radialGradient radius property
        /// scale - taken from currentScale of lightDataAni
        /// </summary>
        private void refreshSize()
        {
            for (int i = 0; i < gradsComposition.Count; i++)
                gradsComposition[i].scale(scaleLightAni.currentScale);
        }

        private void refreshColor()
        {
            if (colorLightAni.Count == 0) return;
            lightColors l = colorLightAni.currentColor;
            for (int i = 0; i < gradsComposition.Count; i++)
                gradsComposition[i].changeColors(l.innerColor, l.outerColor);
        }

        private void addLightToLightingEngine()
        {
            if (EngineApp.Game.self.lightEngine == null)
                throw new Exception("Light Engine is null!");
            EngineApp.Game.self.lightEngine.addLight(this);
        }

        /// <summary>
        /// Sets parent object of light which position in map will
        /// be the absolute position (absPos) of this light.
        /// </summary>
        /// <param name="p"></param>
        public void setParentMOB(MOB p)
        {
            parent = p;
        }
        public void Update()
        {
            // Don't update static lights!
            if (type == eLightType.STATIC) return;

            if (parent != null)
                absPos = new Vector2(parent.pX, parent.pY);
            relPos = new Vector2(posLightAni.currentPos.X,posLightAni.currentPos.Y);
            refreshPos();
            refreshSize();
            refreshColor();
            if (posLightAni.aniHasEnded && colorLightAni.aniHasEnded && scaleLightAni.aniHasEnded
            && destroyLightAtAniEnd)
                Destroy();
        }
        /// <summary>
        /// Method will destroy light
        /// </summary>
        private void Destroy()
        {
            if (parent!=null)
                parent.RemoveLight(this);
            if (EngineApp.Game.self.lightEngine == null) return;
            EngineApp.Game.self.lightEngine.removeLight(this);
        }

    }
}
