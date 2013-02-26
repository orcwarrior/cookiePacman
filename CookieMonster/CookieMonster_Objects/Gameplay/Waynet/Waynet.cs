using System;
using System.Collections.Generic;

using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class Waynet
    {
        Waypoint[] wpArray;           // waypoints are hashed into array by x,y pos
        public Waypoint[] waypointsArray { get { return wpArray; } }
        const uint wpArraySize = 200; // 200 should be quite enough
        List<byte> wpPositions;
        public bool renderWaynet;
        public GameMap map { get; private set; }
        public Waynet(GameMap m)
        {
            First = null;
            Waypoint.lastid = 0; // reset's id's
            Waypoint.wnet = this;
            shortestWayAlgoritm.wnet = this;
            wpArray = new Waypoint[wpArraySize];
            wpPositions = new List<byte>();
            renderWaynet = false;
            map = m;
        }
        public uint generateHash(Waypoint wp)
        {
            return generateHash((uint)wp.x, (uint)wp.y);
        }
        public uint generateHash(uint x, uint y)
        {
            uint hash = (uint)(x * 5 + y * 9 + ((y - x) * (-2)));
            if (y > x) hash += 2 * y;
            else hash -= 4 * x;
            hash %= wpArraySize;
            return hash;
        }
        public Waypoint getWPAt(uint x, uint y)
        {
            uint i = generateHash(x, y), beg = i;
            if (wpArray[i] == null) return null;
            while ((wpArray[i].x != x) || (wpArray[i].y != y))
            {
                i = (i + 1) % wpArraySize;
                if (i == beg) return null;//wp not found,oh by the way whole array is filled :(
                if (wpArray[i] == null) return null; // there is no such waypoint in this array
            }
            return wpArray[i];
        }
        public void addWaypoint(Waypoint wp)
        {
            uint hash = generateHash(wp), i = hash;

            while (wpArray[i] != null)
            {
                i = (i + 1) % wpArraySize;
                if (i == hash) { new DebugMsg("wpArray overflowed!", DebugLVL.fault); return; }
            }
            wpArray[i] = wp;
            wpPositions.Add((byte)i);

            if (First == null) First = wp;
            Last = wp;
        }
        /// <summary>
        /// for debug propouses renders waypoints on map
        /// </summary>
        public void Render()
        {
            if (renderWaynet == true)
            {
                int gridS = GameManager.gridSize;
                for (int i = 0; i < wpPositions.Count; i++)
                {
                    int ways = 0; Obj o;
                    Waypoint wp = wpArray[wpPositions[i]];
                    if (wp.Down != null) ways++;
                    if (wp.Up != null) ways++;
                    if (wp.Left != null) ways++;
                    if (wp.Right != null) ways++;
                    if (ways == 1)
                        o = new Obj("../data/Textures/HLP_WP.jpg", wp.x * gridS + gridS / 2, wp.y * gridS + gridS / 2, Obj.align.CENTER_BOTH);
                    else if (ways == 2)
                        o = new Obj("../data/Textures/HLP_WP_2.jpg", wp.x * gridS + gridS / 2, wp.y * gridS + gridS / 2, Obj.align.CENTER_BOTH);
                    else if (ways == 3)
                        o = new Obj("../data/Textures/HLP_WP_3.jpg", wp.x * gridS + gridS / 2, wp.y * gridS + gridS / 2, Obj.align.CENTER_BOTH);
                    else
                        o = new Obj("../data/Textures/HLP_WP_4.jpg", wp.x * gridS + gridS / 2, wp.y * gridS + gridS / 2, Obj.align.CENTER_BOTH);

                    o.setRenderOnce();
                }
            }
        }
        /// <summary>
        /// Generate ways for each waypoint in waynet
        /// (if waynet is threaten
        /// </summary>
        public void generateWays()
        {
            for (int i = 0; i < wpPositions.Count; i++)
            {
                wpArray[wpPositions[i]].generateWays();
            }
        }
        /// <summary>
        /// indexer = index value means waypoint id, and it will
        /// return wp with that index
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public Waypoint this[int idx]
        {
            get
            {
                //check if idx is out of bounds:
                if ((idx < 0) || (idx > Last._id))
                    return null;
                else
                    return wpArray[wpPositions[idx]];

            }
        }
        public Waypoint Last { get; private set; }
        public Waypoint First { get; private set; }

        /// <summary>
        /// Gets count of current waynet Waypoints
        /// </summary>
        public int Count { get { return wpPositions.Count; } }
    }
}
