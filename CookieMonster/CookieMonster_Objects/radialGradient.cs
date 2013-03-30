using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
namespace CookieMonster.CookieMonster_Objects
{
    class radialGradient
    {

        //                                     //(src)x(dst)
        public enum blendingMode   {NORMAL,    // One x One
                                    ADD,       // SrcAlpha x One
                                    MULTIPLY}; // One x One (equation: multiply)
         
        
        static Vector4 defaultInnerColor = new Vector4(1f, 1f, 1f, 1f);
        static Vector4 defaultOuterColor = new Vector4(0f, 0f, 0f, 1f);
        static int defaultSlices = 9;
        // Size,Positon,shape
        private Vector2 cPos;
        public Vector2 centerPos { get { return cPos; } }
        public float radius { get; private set; }
        float orgRadius;//save it for scaling purposes
        int     slices; // start looking good from 8 slices or more

        // colors
        Vector4 innerColor, outerColor;

        // blendings
        public blendingMode blendMode { get; private set; }
        BlendingFactorSrc gradSrcBlending;
        BlendingFactorDest gradDestBlending;

        #region Constructors
        /// <summary>
        /// Creates radial gradient with default Colors(white in center, black on edges)
        /// and default blending and slices number(8)
        /// </summary>
        /// <param name="cP">Location of the midde of the gradient on screen</param>
        /// <param name="rad">Radius of the "circle" (in pixels)</param>
        public radialGradient(Vector2 cP,float rad)
        {
            cPos = cP;
            orgRadius = radius = rad;
            slices = defaultSlices;

            //pick default colors #FFFF in, #000F out:
            innerColor = defaultInnerColor;
            outerColor = defaultOuterColor;

            //pick default blending:
            gradSrcBlending  = BlendingFactorSrc.SrcAlpha;
            gradDestBlending = BlendingFactorDest.One;//currently buffer
        }
        /// <summary>
        /// Creates radial gradient with default blending and slices number(8)
        /// </summary>
        /// <param name="cP">Location of the midde of the gradient on screen</param>
        /// <param name="rad">Radius of the "circle" (in pixels)</param>
        /// <param name="iC">inner Color of gradient(0-1f)</param>
        /// <param name="oC">outter Color of gradient(0-1f)</param>
        public radialGradient(Vector2 cP, float rad, Vector4 iC, Vector4 oC) : this(cP,rad)
        {
            innerColor = iC; outerColor = oC;
        }
        /// <summary>
        /// Creates radial gradient with default slices number(8)
        /// </summary>
        /// <param name="cP">Location of the midde of the gradient on screen</param>
        /// <param name="rad">Radius of the "circle" (in pixels)</param>
        /// <param name="iC">inner Color of gradient(0-1f)</param>
        /// <param name="oC">outter Color of gradient(0-1f)</param>
        /// <param name="d">Destination blend func</param>
        /// <param name="s">Source blend func</param>
        public radialGradient(Vector2 cP, float rad, Vector4 iC, Vector4 oC,BlendingFactorSrc s,BlendingFactorDest d)
            : this(cP, rad,iC,oC)
        {
            gradSrcBlending = s; gradDestBlending = d;
        }
        /// <summary>
        /// Creates radial gradient.
        /// </summary>
        /// <param name="cP">Location of the midde of the gradient on screen</param>
        /// <param name="rad">Radius of the "circle" (in pixels)</param>
        /// <param name="iC">inner Color of gradient(0-1f)</param>
        /// <param name="oC">outter Color of gradient(0-1f)</param>
        /// <param name="d">Destination blend func</param>
        /// <param name="s">Source blend func</param>
        /// <param name="sl">Vert Fan slices count(more smoother it looks)</param>
        public radialGradient(Vector2 cP, float rad, Vector4 iC, Vector4 oC, BlendingFactorSrc s, BlendingFactorDest d,int sl)
            : this(cP, rad, iC, oC,s,d)
        {
            slices = sl;
        }
        #endregion
        /// <summary>
        /// Changes blending functions to those
        /// passed as parameters.
        /// </summary>
        /// <param name="s">New Source blending</param>
        /// <param name="d">New Destination blending</param>
        public void changeBlendFunc(BlendingFactorSrc s, BlendingFactorDest d)
        { gradSrcBlending = s; gradDestBlending = d; }
        /// <summary>
        /// Sets gradient colors to those passed as
        /// parameters
        /// </summary>
        /// <param name="iColor">inner</param>
        /// <param name="oColor">outer</param>
        public void changeColors(Vector4 iColor, Vector4 oColor)
        { innerColor = iColor; outerColor = oColor; }
        public void changeColors(System.Drawing.Color iColor, System.Drawing.Color oColor)
        {
            innerColor = new Vector4(iColor.R / 255f, iColor.G / 255f, iColor.B / 255f, iColor.A / 255f);
            outerColor = new Vector4(oColor.R / 255f, oColor.G / 255f, oColor.B / 255f, oColor.A / 255f); 
        }

        /// <summary>
        /// Scale gradien size(radius) by multipiler
        /// passed as argument (by orgRadius!!!)
        /// </summary>
        public void scale(float scale)
        {
            radius = orgRadius * scale;
        }
        /// <summary>
        /// Move center of gradient to new position
        /// </summary>
        /// <param name="newPos"></param>
        public void Move(Vector2 newPos)
        {
            cPos = newPos;
        }
        /// <summary>
        /// Move center of gradient to new position
        /// </summary>
        /// <param name="newPos"></param>
        public void moveRelatively(Vector2 newPos)
        {
            cPos = Vector2.Add(cPos, newPos);
        }
        /// <summary>
        /// Draws this radialGradient To current openGL buffer.
        /// </summary>
        public void drawGradientToBuffer()
        {
            float incr = (float)(2 * Math.PI / slices);

            GL.BindTexture(TextureTarget.Texture2D, 0);//Unbind Texture2d

            //GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(gradSrcBlending, gradDestBlending);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
                GL.Begin(BeginMode.TriangleFan);
                GL.Color4(innerColor);
                GL.Vertex2(centerPos);
                GL.Color4(outerColor);

                // Generating fan verts:
                for (int i = 0; i < slices; i++)
                {
                    float angle = incr * i;
                    float x = (float)Math.Cos(angle) * radius;
                    float y = (float)Math.Sin(angle) * radius;
                    GL.Vertex2(centerPos.X + x, centerPos.Y + y);
                }

                GL.Vertex2(centerPos.X + radius, centerPos.Y);
            GL.End();
            //Back to old blend function:
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ShadeModel(ShadingModel.Flat);
        }
        public void setSlicesCount(int s)
        {
            slices = s;
        }

    }
}
