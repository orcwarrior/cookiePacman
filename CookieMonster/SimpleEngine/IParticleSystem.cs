using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;


namespace Engine
{
    class IParticleSystem : IProperties
    {
        public    Image image;                // Reference to sprite
        protected Random random;              // Randomizer

        protected VBO vbo = new VBO();        // VBO for particles
        protected int lenght = 0;             // Count of all active particles

        public    int numParticles;           // Number of particles
        protected int x, y;                   // Last center position
        public    float speed, dirX, dirY;    // Speed of particles
        public    double time;                // Maximal life time in seconds of particles
        public    bool active = true;         // Sets if particle system should emit new particles

        public struct Particle
        {
            public float addX, addY;          // Amout added to particle positions every update
            public double time;               // Life time of particle
            public int frame;                 // Frame from sprite
            public bool active;               // Sets if particle should be drawn
        }
        public Particle[] particles;          // Array of all particles


        /// <summary>
        /// PreInitializes a particle system.
        /// </summary>
        /// <param name="image">Reference to image to be used for particles.</param>
        /// <param name="numParticles">Number of particles.</param>
        /// <param name="time">Maximal life time in seconds of particle.</param>
        /// <param name="speed">Speed of particles.</param>
        /// <param name="dirX">Gravitation on ose X.</param>
        /// <param name="dirY">Gravitation on ose Y.</param>
        protected void PreInit(ref Image image, int numParticles, double time, float speed, float dirX, float dirY)
        {
            // Init randomizer
            random = new Random();

            // Store values
            this.image = image;

            this.numParticles = numParticles;
            this.time = time;
            this.speed = speed;
            this.dirX = dirX;
            this.dirY = dirY;

            // Create particles
            particles = new Particle[numParticles];

            // Put some time in so not all particles appear at once
            for (int i = 0; i < numParticles; i++)
            {
                particles[i].time = random.NextDouble() * this.time;
            }
        }


        /// <summary>
        /// Begins drawing.
        /// </summary>
        /// <param name="x">X position of center.</param>
        /// <param name="y">Y position of center.</param>
        protected void Begin(int x, int y)
        {
            // Store center position
            this.x = x;
            this.y = y;

            GL.BindTexture(TextureTarget.Texture2D, image.texture);

            Begin(x, y, w, h);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
        }


        /// <summary>
        /// Ends drawing.
        /// </summary>
        protected void EndA()
        {
            End();

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }
    }
}
