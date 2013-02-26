using System;
using System.Collections.Generic;

using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class Way
    {
        public uint weight{get; private set;}//distance between waypoints in grid units (right now: 48pixels per grid unit)
        public Waypoint begin{get; private set;}
        public Waypoint end{get; private set;} 
        public enum eWayDir {LR,RL,UD,DU}
        eWayDir direction;
        public Way(Waypoint beg, Waypoint e, uint wgth, eWayDir dir)
        {
            weight = wgth;
            direction = dir;
            begin = beg; end = e;

            if (direction == eWayDir.LR)
            {
                begin.Left = this;
                end.Right = this;
            }
            else if (direction == eWayDir.RL)
            {
                begin.Right = this;
                end.Left = this;
            }
            else if (direction == eWayDir.UD)
            {
                begin.Up = this;
                end.Down = this;
            }
            else if (direction == eWayDir.DU)
            {
                begin.Down = this;
                end.Up = this;
            }
        }

        /// <summary>
        /// Get way object connecting 2 waypoints
        /// </summary>
        /// <param name="wp1">waypoint 1</param>
        /// <param name="wp2">waypoint 2</param>
        /// <returns></returns>
        static public Way getWay(Waypoint wp1, Waypoint wp2)
        {
            if (wp1.upWP == wp2)
                return wp1.Up;
            else if (wp1.downWP == wp2)
                return wp1.Down;
            else if (wp1.leftWP == wp2)
                return wp1.Left;
            else if (wp1.rightWP == wp2)
                return wp1.Right;
            else
                return null;
        }
    }
}
