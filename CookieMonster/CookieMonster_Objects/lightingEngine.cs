using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using Engine;
using EngineApp;
using OpenTK.Graphics.OpenGL;

namespace CookieMonster.CookieMonster_Objects
{
    class lightingEngine : engineReference
    {
        List<Light> activeStaticLights = new List<Light>();
        List<Light> activeDynamicLights = new List<Light>();
        Color4 ambientLightColor = new Color4(10, 10, 10, 255);
        int lightMapBuffer;
        int lightMapTexture;
        int lightMapGrayTexture;
        /// <summary>
        /// How strong shadows are rendered
        /// NOTE: Only static lights
        /// </summary>
        private float lightMulStrength   = 0.1f;
        private float lightAddStrength   = 0.75f;
        private float lightColorStrength = 0f;

        /// <summary>
        /// if value is true static Lights lightmap need to be recalculated
        /// </summary>
        public bool staticLightsChanged { get; private set; }
        /// <summary>
        /// Flag is true when Render() method was already called
        /// in this frame.
        /// (It will be set to false at every rendering of static Lightmaps
        ///  - which is called at the begining of rendering frame)
        /// </summary>
        public bool lightsRendered { get; private set; }
        public void renderStaticLightmaps()
        {
            // No changes in static lightning - no re-rendering
            if (!staticLightsChanged) return;

            if (activeStaticLights.Count == 0 && staticLightsChanged==false) return; // no Lights, no ligthmaps!
            //Clear buffer to ambientLightColor:
            GL.ClearColor  (ambientLightColor.R, 
                            ambientLightColor.G, 
                            ambientLightColor.B, 
                            ambientLightColor.A);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            //Render all lightmaps:
            for (int i = 0; i < activeStaticLights.Count; i++)
                activeStaticLights[i].Render();

            //Store rendered Lightmap to texture:
            //GL.Viewport(0, 0, 1024, 1024);
            //GL.DrawBuffer(DrawBufferMode.Back);
            GL.ReadBuffer(ReadBufferMode.Back);

            //Create Color lightmap:
            if(lightMapTexture == 0)
                GL.GenTextures(1,out lightMapTexture);
            GL.BindTexture(TextureTarget.Texture2D, lightMapTexture);
            //GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 0, 0, 1024, 1024,0);
            GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, engine.Width, engine.Height, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Create Inverted Grayscale Lightmap:
            if (lightMapGrayTexture== 0)
                GL.GenTextures(1, out lightMapGrayTexture);

            // save buffer to texture:
            GL.BindTexture(TextureTarget.Texture2D, lightMapGrayTexture);
            GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Luminance,0, 0, engine.Width, engine.Height, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            
            // Clear back to black:
            GL.ClearColor(0,0,0,1);
            GL.Clear(ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);

            // Rendered lightmap is most actual so we changing staticLightsChanged bool to false:
            staticLightsChanged = false;
        }
        public void clearAllLights()
        {
            activeStaticLights.Clear();
            staticLightsChanged = true;

            for (int i = 0; i < activeDynamicLights.Count; i++)
            {
                if(activeDynamicLights[i].parent!=null)
                    activeDynamicLights[i].parent.RemoveLight(activeDynamicLights[i]);
                activeDynamicLights[i] = null;
            }
            activeDynamicLights = new List<Light>();
        }
        internal void addLight(Light light)
        {
            if (light.type == eLightType.STATIC)
            {
                activeStaticLights.Add(light);
                staticLightsChanged = true;
            }
            else if (light.type == eLightType.DYNAMIC)
            {
                activeDynamicLights.Add(light);
            }
        }
        internal Light removeLight(Light light)
        {
            if (light.type == eLightType.STATIC)
            {
                for (int i = 0; i < activeStaticLights.Count; i++)
                {
                    if (activeStaticLights[i] == light)
                    {
                        staticLightsChanged = true;
                        activeStaticLights.RemoveAt(i);
                        return light;
                    }
                }
            }
            else if (light.type == eLightType.DYNAMIC)
            {
                for (int i = 0; i < activeDynamicLights.Count; i++)
                {
                    if (activeDynamicLights[i] == light)
                    {
                        activeDynamicLights.RemoveAt(i);
                        return light;
                    }
                }
            }
            return null;
        }
        internal void Update()
        {
            for (int i = 0; i < activeDynamicLights.Count; i++)
                activeDynamicLights[i].Update();
            lightsRendered = false;
        }
        internal void Render()
        {
            // Light are render once per frame
            //if (lightsRendered==true) return;
            lightsRendered = true;

            if (disabled) return;

            if (activeStaticLights.Count >= 0) // no Lights, no ligthmaps!
            {
                //BLENDING "ADD" Lightmaps rendering:
                if (lightAddStrength > 0f)
                    renderLightMapsTexture(lightColorStrength, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

                //BLENDING "MULTIPLY" Lightmaps rendering:
                if (lightMulStrength > 0f)
                    renderLightMapsTextureMultiply(lightMulStrength);

                //BLENDING "Color" Lightmaps rendering:
                if (lightColorStrength > 0f)
                    renderLightMapsTexture(lightColorStrength, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            }

            // Then render dynamic lights over static "ADD" lights:
            for (int i = 0; i < activeDynamicLights.Count; i++)
                activeDynamicLights[i].Render();

            // Back to old blend function:
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


        }
        public void moveLights(OpenTK.Vector2 pos)
        {
            if (pos.Length > 0)//we really have some movement?
            {
                for (int i = 0; i < activeStaticLights.Count; i++)
                    activeStaticLights[i].move(pos);
            }
        }

        private void renderLightMapsTexture(float opacity, BlendingFactorSrc src, BlendingFactorDest dst)
        {
            int width = engine.Width, height = engine.Height;
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(src, dst);
            GL.Color4(opacity, opacity, opacity, opacity);
            GL.BindTexture(TextureTarget.Texture2D, lightMapTexture);
            GL.BlendColor(0f, 0f, 0f, 1f);
            GL.Begin(BeginMode.Quads);
                GL.TexCoord2(0, 1);
                GL.Vertex2(0, 0);
                GL.TexCoord2(1, 1);
                GL.Vertex2(width, 0);
                GL.TexCoord2(1, 0);
                GL.Vertex2(width, height);
                GL.TexCoord2(0, 0);
                GL.Vertex2(0, height);
            GL.End();
            //GL.Disable(EnableCap.Blend);
        }
        private void renderLightMapsTextureMultiply(float opacity)
        {
            int width = engine.Width, height = engine.Height;
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Color4(opacity, opacity, opacity, opacity);
            GL.BindTexture(TextureTarget.Texture2D, lightMapGrayTexture);
            GL.BlendColor(0f, 0f, 0f, 1f);
            GL.Begin(BeginMode.Quads);
                GL.TexCoord2(0, 1);
                GL.Vertex2(0, 0);
                GL.TexCoord2(1, 1);
                GL.Vertex2(width, 0);
                GL.TexCoord2(1, 0);
                GL.Vertex2(width, height);
                GL.TexCoord2(0, 0);
                GL.Vertex2(0, height);
            GL.End();
            //GL.Disable(EnableCap.Blend);
        }

        public void setupMenuLightingParams()
        {
            lightMulStrength = 0.1f;
            lightAddStrength = 0.75f;
            lightColorStrength = 0f;
        }
        public void setupGameLightingParams()
        {
            lightAddStrength = 0.6f;
            lightMulStrength = 0.2f;
        }
        /// <summary>
        /// If it's true, all lights stored in lighting engine
        /// willn't render.
        /// </summary>
        public bool disabled { get; set; }
    }
}
