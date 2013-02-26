namespace Engine
{
    public class Helper
    {
        /// <summary>
        /// Checks collision between 2 quads.
        /// </summary>
        /// <param name="x1">X of 1st object.</param>
        /// <param name="y1">Y of 1st object.</param>
        /// <param name="w1">W of 1st object.</param>
        /// <param name="h1">H of 1st object.</param>
        /// <param name="x2">X of 2nd object.</param>
        /// <param name="y2">Y of 2nd object.</param>
        /// <param name="w2">W of 2nd object.</param>
        /// <param name="h2">H of 2nd object.</param>
        /// <returns>True if objects collide.</returns>
        public bool IsCollision(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2)
        {
            if (x1 + w1 >= x2 && x1 <= x2 + w2 &&
                y1 + h1 >= y2 && y1 <= y2 + h2)
                return true;

            return false;
        }
    }
}
