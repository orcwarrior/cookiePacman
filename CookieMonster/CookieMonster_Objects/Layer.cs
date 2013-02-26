using System;
using System.Collections.Generic;
using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Some specified layer for nice-ordering and controlling of Objects rendering
    /// NOTE: Remeber that when text and img has the same layer nr. first will be 
    /// rendered image, then all texts (see order in Viewport.Render)
    /// </summary>
    static class Layer
    {
        /// <summary>0</summary>
        public const int _imgUnderTextBG = 0;
        /// <summary>0</summary>
        public const int textBG = 0;
        /// <summary>1</summary>
        public const int imgBG = 1;
        /// <summary>1</summary>
        public const int textFG = 1;
        /// <summary>2</summary>
        public const int imgFG = 2;
        /// <summary>5</summary>
        public const int textGUIBG = 3;
        /// <summary>6</summary>
        public const int imgGUI = 4;
        /// <summary>6</summary>
        public const int textGUIFG = 4;
        /// <summary>7 TOP MOST</summary>
        public const int cursor = 5;
    }
}
