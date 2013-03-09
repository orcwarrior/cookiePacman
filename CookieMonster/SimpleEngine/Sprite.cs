namespace Engine
{
    class Sprite : IProperties
    {
        public Image image;                                 // Holds reference to image
        public int framesX, framesY;                        // Frames on oses
        public int frame, frames, firstFrame, lastFrame;    // Current frame, all frames, first frame in animation, last frame in animation
        public double frameTime, currTime;                  // Time to be paused between frames, time passed since last frame
        public int lastX, lastY;        // Holds last draw position


        /// <summary>
        /// Initializes sprite.
        /// </summary>
        /// <param name="image">Reference to image.</param>
        public void Init(ref Image image)
        {
            this.image = image;
            this.framesX = 1;
            this.framesY = 1;
            this.frames = 1;
            this.frameTime = 0;

            firstFrame = 1;
            lastFrame = frames;
            frame = firstFrame;
            w = image.w / framesX;
            h = image.h / framesY;
            lastX = 0;
            lastY = 0;
        }


        /// <summary>
        /// Initializes sprite.
        /// </summary>
        /// <param name="image">Reference to image.</param>
        /// <param name="framesX">Number of frames on ose X.</param>
        /// <param name="framesY">Number of frames on ose Y.</param>
        /// <param name="frames">Total number of frames.</param>
        /// <param name="frameTime">Time paused between frames. Pass 0 to not animate sprite.</param>
        public void Init(ref Image image, int framesX, int framesY, int frames, double frameTime)
        {
            this.image = image;
            this.framesX = framesX;
            this.framesY = framesY;
            this.frames = frames;
            this.frameTime = frameTime;

            firstFrame = 1;
            lastFrame = frames;
            frame = firstFrame;
            w = image.w / framesX;
            h = image.h / framesY;
            lastX = 0;
            lastY = 0;
        }


        /// <summary>
        /// Updates sprite.
        /// </summary>
        /// <param name="time">Time passed since last update.</param>
        public void Update(double time)
        {
            // Do not update if frame time is 0
            if (frameTime != 0)
            {
                if ((currTime += time) >= frameTime)
                {
                    currTime = 0;

                    if ((frame++) >= lastFrame)
                    {
                        frame = firstFrame;
                    }
                }
            }
        }


        /// <summary>
        /// Draws sprite.
        /// </summary>
        public void Draw()
        {
            Draw(lastX, lastY, frame);
        }


        /// <summary>
        /// Draws sprite.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        public void Draw(int x, int y)
        {
            Draw(x, y, frame);
        }


        /// <summary>
        /// Draws single frame of sprite.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="frame">Number of frame.</param>
        public void Draw(int x, int y, int frame)
        {
            lastX = x;
            lastY = y;

            Begin(x, y, w, h);

            // Draw frame as part of image
            int frameW = image.w / framesX;
            int frameH = image.h / framesY;

            image.Draw(x, y, w, h, ((frame - 1) % framesX) * frameW, ((frame - 1) / framesX) * frameH,
                             (((frame - 1) % framesX) * frameW) + frameW, (((frame - 1) / framesX) * frameH) + frameH);

            End();
        }
    }
}
