using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace Engine
{
    /// <summary>
    /// Part of SimpleEngine, functionality is limited to initing
    /// openGL stuff when starting aplication.
    /// </summary>
    class Core
    {
        /// <summary>
        /// Initializes 2D mode.
        /// </summary>
        public void Init()
        {
            int[] viewPort = new int[4];

            GL.GetInteger(GetPName.Viewport, viewPort);

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Ortho(viewPort[0], viewPort[0] + viewPort[2], viewPort[1] + viewPort[3], viewPort[1], -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
            //GL.Translate(0.375, 0.375, 0.0);
            GL.Translate(0.0, 0.0, 0.0);

            GL.PushAttrib(AttribMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);
            
            GL.Enable(EnableCap.Texture2D);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }
        
        /// <summary>
        /// Clears the background.
        /// </summary>
        public void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
