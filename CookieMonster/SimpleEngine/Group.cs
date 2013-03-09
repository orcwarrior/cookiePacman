/* 2D C# Graphics Engine
 * Copyright (c) 2008 Lubos Lenco
 * See license.txt for license info
 */

using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Input;


namespace Engine
{
    class Group
    {
        List<Sprite> sprites = new List<Sprite>();

        
        /// <summary>
        /// Adds sprite to manager.
        /// </summary>
        /// <param name="image">Sprite to be added.</param>
        public void Add(ref Sprite sprite, int x, int y)
        {
            sprite.lastX = x;
            sprite.lastY = y;
            sprites.Add(sprite);
        }


        /// <summary>
        /// Removes sprite form manager.
        /// </summary>
        /// <param name="image">Sprite to be removed.</param>
        public void Remove(ref Sprite sprite)
        {
            sprites.Remove(sprite);
        }


        /// <summary>
        /// Removes all sprites.
        /// </summary>
        public void RemoveAll()
        {
            sprites.Clear();
        }


        /// <summary>
        /// Updates all sprites.
        /// </summary>
        /// <param name="time">Time passed since last update.</param>
        public void Update(double time)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Update(time);
            }
        }


        /// <summary>
        /// Draws all sprites.
        /// </summary>
        public void Draw()
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Draw();
            }
        }
    }
}
