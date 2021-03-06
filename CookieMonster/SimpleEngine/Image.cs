using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics;
using CookieMonster.CookieMonster_Objects;
using CookieMonster.TextureLoaders;
using CookieMonster;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    class Image : IProperties
    {
        public Bitmap bitmap;          // Used to load image
        public uint texture;            // Holds image data (openGL texID)
        
        public VBO vbo = new VBO();

        /// <summary>
        /// If flag is set to TRUE VBO (Vertex positions/UV Mapping)
        /// will be rebuilded (by BuildTexchord).
        /// </summary>
        public bool rebuild = true;

        private bool VBOinited;
        private string _bitmapPath; public string bitmapPath { get { return _bitmapPath; } }

        //performance debug
        static int imgCreated = 0; public int imgs { get { return imgCreated; } }
        static bool debugAdded = false;

        /// <summary>
        /// Creates 4 vertices and texcoords for quad.
        /// </summary>
        public Image()
        {
            vbo.vertices = new Vertex[4];    // Create 4 vertices for quad
            vbo.texcoords = new TexCoord[4]; // Texture coordinates for quad
            imgCreated++; //NOTE: To remove
            if (!debugAdded)
            {
             //   new DebugMsg(this, "imgs", DebugLVL.info); debugAdded = true; 
            }
        }
        /// <summary>
        /// Construct Image object based on existing openGL Texture ID
        /// </summary>
        /// <param name="texID"></param>
        public Image(int texID)
        {
            vbo.vertices = new Vertex[4];    // Create 4 vertices for quad
            vbo.texcoords = new TexCoord[4]; // Texture coordinates for quad
            // Hook texture with passed ID:
            // get needed texture parameters:
            texture = (uint)texID;
            correctTexDimensions();
        }

        /// <summary>
        /// Loads image from harddisk into memory.
        /// </summary>
        /// <param name="path">Image path.</param>
        public void Load(string path)
        {
            _bitmapPath = path;
            if (path.Substring(path.LastIndexOf(".")) == ".dds")
            {
                //this method was shity and won't helped at all
                //TexPoolItem t = TexturesPool.getImage(path);
                TextureTarget texTarget = TextureTarget.Texture2D;
                //Set parrameters for call in new thread:
                _loadTextureWorker_pTexTarget = texTarget;
                _loadTextureWorker_pPath = path;
                //[TODO] Temporary disabled, fix it!!!
                //new System.Threading.Thread(_loadTextureWorker).Start();
                ImageDDS.LoadFromDisk(path, out texture, out texTarget);
                w = ImageDDS.Width;  h = ImageDDS.Height;
            }
            else
            {
                // Load image
                bitmap = new Bitmap(bitmapPath);

                // Generate texture
                GL.GenTextures(1, out texture);
                GL.BindTexture(TextureTarget.Texture2D, texture);

                // Store texture size
                w = bitmap.Width;
                h = bitmap.Height;

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data);

                // Setup filtering
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
         }

        private int _loadTextureWorker_attempts=0;
        private string _loadTextureWorker_pPath;
        private TextureTarget _loadTextureWorker_pTexTarget;
        private void _loadTextureWorker()
        {
            if (_loadTextureWorker_attempts > 3) return;
            try
            {
                _loadTextureWorker_attempts++;
                ImageDDS.LoadFromDisk(_loadTextureWorker_pPath, out texture, out _loadTextureWorker_pTexTarget);
                correctTexDimensions();
                System.Threading.Thread.CurrentThread.Abort();
            }
            catch (System.Exception e) { System.Threading.Thread.Sleep(25); _loadTextureWorker(); } 
        }

        /// <summary>
        /// Deletes texture from memory.
        /// </summary>
        public void Free()
        {
            GL.DeleteTextures(1, ref texture);
        }

        public void correctTexDimensions()
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out w);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out h);
        }

        /// <summary>
        /// Draws image.
        /// </summary>
        /// <param name="x">X position of left-upper corner.</param>
        /// <param name="y">Y position of left-upper corner.</param>
        public void Draw(int x, int y)
        {
            Draw(x, y, w, h, 0, 0, w, h);
        }


        /// <summary>
        /// Draws a part of image.
        /// </summary>
        /// <param name="x">X position of left-upper corner.</param>
        /// <param name="y">Y position of left-upper corner.</param>
        /// <param name="imgX">X positon on image.</param>
        /// <param name="imgY">Y positon on image.</param>
        /// <param name="imgW">Width of image part to be drawn.</param>
        /// <param name="imgH">Height of image part to be drawn.</param>
        public void Draw(int x, int y, int imgX, int imgY, int imgW, int imgH)
        {
            Draw(x, y, w, h, imgX, imgY, imgW, imgH);
        }


        /// <summary>
        /// Draws image with specified size.
        /// </summary>
        /// <param name="x">X position of left-upper corner.</param>
        /// <param name="y">Y position of left-upper corner.</param>
        /// <param name="w">Width of image.</param>
        /// <param name="h">Height of image.</param>
        public void Draw(int x, int y, int w, int h)
        {
            Draw(x, y, w, h, 0, 0, this.w, this.h);
        }


        /// <summary>
        /// Draws a part of image with specified size.
        /// </summary>
        /// <param name="x">X position of left-upper corner.</param>
        /// <param name="y">Y position of left-upper corner.</param>
        /// <param name="w">Width of image.</param>
        /// <param name="h">Height of image.</param>
        /// <param name="imgX">X positon on image.</param>
        /// <param name="imgY">Y positon on image.</param>
        /// <param name="imgW">Width of image part to be drawn.</param>
        /// <param name="imgH">Height of image part to be drawn.</param>
        public void Draw(int x, int y, int w, int h, int imgX, int imgY, int imgW, int imgH)
        {//fix: small offset from the edge helps with "white edges" issue. [DK]
            float off = 0.008f;
            // Texture coordinates
            float u1 = 0.0f, u2 = 0.0f, v1 = 0.0f, v2 = 0.0f;

            // Calculate coordinates, prevent dividing by zero
                if (imgX != 0) u1 = 1.0f / ((float)this.w / (float)imgX) + off;
                if (imgW != 0) u2 = 1.0f / ((float)this.w / (float)imgW) - off;
                if (imgY != 0) v1 = 1.0f / ((float)this.h / (float)imgY) + off;
                if (imgH != 0) v2 = 1.0f / ((float)this.h / (float)imgH) - off;

            if (rebuild)
           {
               // Check if texture coordinates have changed
                // DK: If there is texcoords override use it!
               if (!_checkTexcoordsOverride())
               {
                   if (vbo.texcoords[0].u != u1 || vbo.texcoords[1].u != u2 || vbo.texcoords[2].v != v1 || vbo.texcoords[0].v != v2)
                   {
                       // Update texcoords for all vertices
                       BuildTexcoords(u1, u2, v1, v2);
                   }
               }
           
               // Check if position coordinates have changed
               if (vbo.vertices[0].x != x || vbo.vertices[2].y != y || vbo.vertices[0].y != y + h || vbo.vertices[1].x != x + w)
               {
                   BuildVertices(x, y, w, h);
               }
           }

            // Prepare drawing
            Begin(x, y, w, h);
            //Begin(x+w, y+h, w, h);
            // Bind texture
            GL.BindTexture(TextureTarget.Texture2D, texture);
           
            // Draw VBO
            vbo.Draw(vbo.vertices.Length, BeginMode.Quads);

            End();
        }


        /// <summary>
        /// Builds texcoords for quad.
        /// </summary>
        public void BuildTexcoords()
        {   //fix: small offset from the edge helps with "white edges" issue. [DK]
            float off = 0.005f;
            BuildTexcoords(0.0f + off, 1.0f - off, 0.0f + off, 1.0f - off);
        }



        /// <summary>
        /// Builds texcoords for quad.
        /// </summary>
        /// <param name="u1">U1.</param>
        /// <param name="u2">U2.</param>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public void BuildTexcoords(float u1, float u2, float v1, float v2)
        {
            vbo.texcoords[0].u = u1;
            vbo.texcoords[0].v = v2;
            vbo.texcoords[1].u = u2;
            vbo.texcoords[1].v = v2;
            vbo.texcoords[2].u = u2;
            vbo.texcoords[2].v = v1;
            vbo.texcoords[3].u = u1;
            vbo.texcoords[3].v = v1;

            vbo.BuildTex();
        }

        public bool useTexcoordsOverride { get; set; }
        public TexCoord[] overrideTexcoords; 
        /// <summary>
        /// Builds texcoords for quad.
        /// </summary>
        /// <param name="u1">U1.</param>
        /// <param name="u2">U2.</param>
        /// <param name="v1">V1.</param>
        /// <param name="v2">V2.</param>
        public void OverrideTexcoords(float u1, float u2, float v1, float v2)
        {
            useTexcoordsOverride = true;
            overrideTexcoords = new TexCoord[4];
            overrideTexcoords[0].u = u1;
            overrideTexcoords[0].v = v2;
            overrideTexcoords[1].u = u2;
            overrideTexcoords[1].v = v2;
            overrideTexcoords[2].u = u2;
            overrideTexcoords[2].v = v1;
            overrideTexcoords[3].u = u1;
            overrideTexcoords[3].v = v1;
        }

        private bool _checkTexcoordsOverride()
        {
            if (!useTexcoordsOverride) return false;
            TexCoord[] orginalCoords = new TexCoord[4];
            orginalCoords[0].u = vbo.texcoords[0].u;
            orginalCoords[0].v = vbo.texcoords[0].v;
            orginalCoords[1].u = vbo.texcoords[1].u;
            orginalCoords[1].v = vbo.texcoords[1].v;
            orginalCoords[2].u = vbo.texcoords[2].u;
            orginalCoords[2].v = vbo.texcoords[2].v;
            orginalCoords[3].u = vbo.texcoords[3].u;
            orginalCoords[3].v = vbo.texcoords[3].v;

            vbo.texcoords = overrideTexcoords;
            vbo.BuildTex();
            //vbo.texcoords = orginalCoords;

            return true;
        }


        /// <summary>
        /// Builds vertices for quad.
        /// </summary>
        /// <param name="x">X pos.</param>
        /// <param name="y">Y pos.</param>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        public void BuildVertices(int x, int y, int w, int h)
        {
            vbo.vertices[0].x = x;
            vbo.vertices[0].y = y + h;
            vbo.vertices[1].x = x + w;
            vbo.vertices[1].y = y + h;
            vbo.vertices[2].x = x + w;
            vbo.vertices[2].y = y;
            vbo.vertices[3].x = x;
            vbo.vertices[3].y = y;

            vbo.Build();
        }

    }
}
