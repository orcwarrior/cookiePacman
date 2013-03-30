using System;
using System.Collections.Generic;

using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Static table containing all already
    /// calculated shortest ways between paths
    /// </summary>
    class ShortestWaysTable
    {
        public static ShortestWay[,] shortestWays{get; private set;}//[wpSrcID,wpDstID]

        static public void Initialize()
        {
            if (shortestWayAlgoritm.wnet.Count > 0)
                shortestWays = new ShortestWay[shortestWayAlgoritm.wnet.Last._id + 1, shortestWayAlgoritm.wnet.Last._id + 1];
            else
                new DebugMsg("There was no waynet when creating shortestWayTable!", DebugLVL.warn);
        }
        static public void addShortestWay(ShortestWay s)
        {
            if(s.wpList[0]!=null)
            {
                int j = s.wpList.Count-1;
                if(s.wpList[j]!=null)
                {
                    for (int i = 0; i < s.wpList.Count-1; i++)//left side
                    {
                        for (int k = j; k > i; k--)
                        {
                            int iID = s.wpList[i]._id, kID = s.wpList[k]._id;
                            if (shortestWays[iID, kID] == null)//if there is already this way entered don't overwrite it
                            {
                                ShortestWay middleway = s.Copy(i, k+1);
                                addOneShortestWay(middleway);          //add way to ways table
                                addOneShortestWay(middleway.Reverse());//add reversed way too
                            }
                        }
                    }
                }
            }   
        }
        static private void addOneShortestWay(ShortestWay s)
        {
            if (s.wpList[0] != null)
            {
                if (s.wpList[s.wpList.Count - 1] != null)
                {
                    int i1D = s.wpList[0]._id;
                    int i2D = s.wpList[s.wpList.Count - 1]._id;
                    shortestWays[i1D, i2D] = s;
                }
            }

        }
        static public ShortestWay getCopy(int i, int j)
        {
            if ((i > 0) && (shortestWayAlgoritm.wnet.Last._id >= i)
            && (j > 0) && (shortestWayAlgoritm.wnet.Last._id >= j) )
            {
                if (shortestWays[i, j] == null) return null;
                return shortestWays[i,j].Copy();
            }
            return null;
        }
    }
    /// <summary>
    /// Class is used to hold directions (eDir) on crossroads (waynet nodes)
    /// to be followed to reach target waypoint.
    /// </summary>
    class wayDirectionStack
    {
        public List<MOB.eDir> dirList{get; private set;}

        public wayDirectionStack()
        {
            dirList = new List<MOB.eDir>();
        }
        /// <summary>
        /// "pop's" first value from List
        /// </summary>
        /// <returns></returns>
        public MOB.eDir Pop()
        {
            //TODO: index out of range smtimes
            MOB.eDir ret = dirList[0];
            dirList.RemoveAt(0);
            return ret;
        }
        internal wayDirectionStack Copy()
        {
            wayDirectionStack copy = new wayDirectionStack();
            for (int i = 0; i < dirList.Count; i++)
                copy.dirList.Add(dirList[i]);
            return copy;
        }
        internal wayDirectionStack Copy(int beg,int end)
        {
            if (beg < 0) beg=0;
            if (end > dirList.Count) end = dirList.Count;

                wayDirectionStack copy = new wayDirectionStack();
            for (int i = beg; i < end ; i++)
                copy.dirList.Add(dirList[i]);
            return copy;
        }
        /// <summary>
        /// return reversed Direction stack
        /// (usable when creating same way but 
        /// reversing start and dst point)
        /// </summary>
        /// <returns></returns>
        internal wayDirectionStack Reverse()
        {
            //TODO: directions need to be reversed too! U->D; R->L ..etc
            wayDirectionStack reverse = new wayDirectionStack();
            reverse.dirList = new List<MOB.eDir>();
            for (int i = 0; i < dirList.Count; i++)
                reverse.dirList.Add(Reverse(dirList[dirList.Count - 1 - i]));
            return reverse;
        }
        public static MOB.eDir Reverse(MOB.eDir dir)
        {
            if (dir == MOB.eDir.L)
                return MOB.eDir.R;
            else if (dir == MOB.eDir.R)
                return MOB.eDir.L;
            else if (dir == MOB.eDir.U)
                return MOB.eDir.D;
            else //if (dir == MOB.eDir.D)
                return MOB.eDir.U;
            //else
            //    return null;
        }
    }
    class ShortestWay
    {
        /// <summary>
        /// First = start waypoint;
        /// Last = destination Point
        /// </summary>
        public List<Waypoint> wpList{get; private set;}
        public uint weight{get; set;}
        public wayDirectionStack route{get; private set;}

        public ShortestWay()
        {
            wpList = new List<Waypoint>();
            route = new wayDirectionStack();
        }
        public override string ToString()
        {
            string ret = "";
            for (int i = 0; i < route.dirList.Count; i++)
                ret += route.dirList[i]+"+";
            return ret;
        }
        public MOB.eDir Pop()
        {
            //TODO: + prevent from null at indexes
            //Way wp0wp1 = Way.getWay(wpList[0], wpList[1]);
            //if(wp0wp1 == null) return MOB.eDir.UNDEF;
            //else
            //{
                //TODO: getWay dont work properly, but this should be done even later
                //weight -= wp0wp1.weight;
                wpList.RemoveAt(0);
                return route.Pop();
            //}
        }
        /// <summary>
        /// Returns deep clone of object
        /// </summary>
        /// <returns></returns>
        public ShortestWay Copy()
        {
            ShortestWay copy = new ShortestWay();
            copy.wpList = new List<Waypoint>();
            for (int i = 0; i < wpList.Count; i++)
                copy.wpList.Add(wpList[i]);
            copy.weight = weight;
            copy.route = route.Copy();
            return copy;
        }
        /// <summary>
        /// returns deep copy of object
        /// </summary>
        /// <param name="beg">starting index (begin trim)</param>
        /// <param name="end">ending indec (end trim)</param>
        /// <returns></returns>
        public ShortestWay Copy(int beg, int end)
        {
            ShortestWay copy = new ShortestWay();
            copy.wpList = new List<Waypoint>();
            for (int i = beg; i < end; i++)
                copy.wpList.Add(wpList[i]);
            //Weight will be recalculated from scratch:
            copy.weight = 0;
            for (int i = beg; i < end-1; i++)
                copy.weight += Way.getWay(wpList[i], wpList[i + 1]).weight;
            copy.route = new wayDirectionStack();
            copy.route = route.Copy(beg,end);
            return copy;
        }
        public ShortestWay Reverse()
        {
            ShortestWay reverse = new ShortestWay();
            reverse.wpList = new List<Waypoint>();
            for (int i = 0; i < wpList.Count; i++)
                reverse.wpList.Add(wpList[wpList.Count-1-i]);
            reverse.weight = weight;
            reverse.route = new wayDirectionStack();
            reverse.route = route.Reverse();
            return reverse;
        }

        internal void generateDirectionStack()
        {
            for (int i = 0; i < wpList.Count - 1; i++)
            {
                if(wpList[i].Up != null)
                {
                    if (wpList[i].upWP == wpList[i+1])
                    {
                        route.dirList.Add(MOB.eDir.U);
                    }
                }
                if (wpList[i].Left != null)
                {
                    if (wpList[i].Left.end == wpList[i+1])
                    {
                        route.dirList.Add(MOB.eDir.L);
                    }
                }
                if (wpList[i].Down != null)
                {
                    if (wpList[i].downWP == wpList[i+1])
                    {
                        route.dirList.Add(MOB.eDir.D);
                    }
                }
                if (wpList[i].Right != null)
                {
                    if (wpList[i].rightWP == wpList[i+1])
                    {
                        route.dirList.Add(MOB.eDir.R);
                    }
                }
            }
        }
    }
    class wayInInfo
    {
        public Waypoint wp;
        public Waypoint elder; //when weight is modified then elder will be changed too
        public uint smallestWeight = uint.MaxValue;
        public wayInInfo(Waypoint wpoint)
        {
            wp = wpoint;
        }
        /// <summary>
        /// Call it always when new way to this path was found
        /// func will decide if weight need to be updated.
        /// </summary>
        /// <param name="eld"></param>
        /// <param name="smallestWeight"></param>
        public bool Update(Waypoint eld, uint weight)
        {
            if (weight < smallestWeight)
            {
                smallestWeight = weight;
                elder = eld;
                return true;
            }
            return false;
        }
    }
    class wayOutInfo
    {
        public Waypoint wp;
        public Waypoint elder;//from which wp way comes
        public uint addWeight;//weight to add when creating connection from this wp

        public wayOutInfo(Waypoint t, Waypoint eld, uint weight)
        {
            wp = t; elder = eld; addWeight = weight;
        }
    }
    class shortestWayAlgoritm
    { // with use of Dijkstra algorithm
        static public Waynet wnet;
        wayInInfo[] IN;
        wayOutInfo[] OUT;
        Waypoint destinationPoint;

        wayOutInfo lastAddedPoint;
        uint smallestWeight; int smallestWeightID;
        uint nxtSmallestWeight; int nxtSmallestWeightID;
        public ShortestWay result{get; private set;}
        private bool INArrayIsBlank()
        {
            for (int i = 0; i < IN.Length; i++)
            {
                if (IN[i] != null) return false;
            }
            return true;
        }
        public shortestWayAlgoritm(Waypoint startpoint, Waypoint dstpoint)
        {
            bool goThroughAllWays = false;
            if (dstpoint == null) goThroughAllWays = true;
            smallestWeight = nxtSmallestWeight =  uint.MaxValue;
            IN = new wayInInfo[wnet.Count];//declare array with size wnet waypoints count;
            OUT = new wayOutInfo[wnet.Count];
            //1.Create IN List
            for(int i=0;i<wnet.Count;i++)
            {
                    IN[i] = new wayInInfo(wnet[i]);
            }
            //2. Move startpoint to OUT list:
            IN[startpoint._id].smallestWeight = 0;
            moveToOutList(IN[startpoint._id]);
            lastAddedPoint = OUT[startpoint._id];
            smallestWeight -= 1;//for entering into while loop
            //::At this point we have done initiation stuff::

            while (((lastAddedPoint.wp != dstpoint) && (goThroughAllWays == false)) || ((goThroughAllWays == true) && (!INArrayIsBlank())))
            {// I will create graph till I'll add dest.point to it: 
             // (ADD: if dest.point!=null => then i will create full OUT table!)
                
                //1. Check's last added wp connections:
                Waypoint wp; uint weight;
                #region updateWeightByNewWP
                //Firstly, if there is smallestWeight is not present generete it and nxtSmallestWeight btw.
                if ((lastAddedPoint != OUT[startpoint._id]) && (smallestWeight == uint.MaxValue))
                {
                    findCurrentSmallestWeigth();
                    if ((smallestWeight == uint.MaxValue) && (nxtSmallestWeight == smallestWeight))
                    {
                        // paths are generated wrong, Waynet is broken apart :(
                        // TODO: Some error should be raised here
                        return;
                    }
                    
                }
                //A.UP
                if ((lastAddedPoint.wp.Up != null)&&(IN[lastAddedPoint.wp.upWP._id]!=null))
                {
                   wp = lastAddedPoint.wp.upWP;
                   weight = lastAddedPoint.addWeight + lastAddedPoint.wp.Up.weight;
                   // try to update info on this wp, it means
                   // if current weight is smaller than stored 
                   // in wayInInfo then write new elder (lastAddedWp) and new weight 
                   if(IN[wp._id]!=null)
                   IN[wp._id].Update(lastAddedPoint.wp, weight);
                   if (weight < smallestWeight)//this weight is new smallest weight:
                   {
                       if (smallestWeightID != wp._id)
                       { nxtSmallestWeight = smallestWeight; nxtSmallestWeightID = smallestWeightID; }
                       smallestWeight = weight; smallestWeightID = wp._id;
                   }
                   else if (weight < nxtSmallestWeight)
                   {
                       if (smallestWeightID != wp._id)
                       { nxtSmallestWeight = weight; nxtSmallestWeightID = wp._id; }
                   }
                }
                //B. RIGHT
                if ((lastAddedPoint.wp.Right != null) && (IN[lastAddedPoint.wp.rightWP._id] != null))
                {
                    wp = lastAddedPoint.wp.rightWP;
                    weight = lastAddedPoint.addWeight + lastAddedPoint.wp.Right.weight;
                    if (IN[wp._id] != null)
                        IN[wp._id].Update(lastAddedPoint.wp, weight);
                    if (weight < smallestWeight)//this weight is new smallest weight:
                    {
                        if (smallestWeightID != wp._id)
                        { nxtSmallestWeight = smallestWeight; nxtSmallestWeightID = smallestWeightID; }
                        smallestWeight = weight; smallestWeightID = wp._id;
                    }
                    else if (weight < nxtSmallestWeight)
                    {
                        if (smallestWeightID != wp._id)
                        { nxtSmallestWeight = weight; nxtSmallestWeightID = wp._id; }
                    }
                }
                //C. DOWN
                if ((lastAddedPoint.wp.Down != null) && (IN[lastAddedPoint.wp.downWP._id] != null))
                {
                    wp = lastAddedPoint.wp.downWP;
                    weight = lastAddedPoint.addWeight + lastAddedPoint.wp.Down.weight;
                    if (IN[wp._id] != null)
                        IN[wp._id].Update(lastAddedPoint.wp, weight);
                    if (weight < smallestWeight)//this weight is new smallest weight:
                    {
                        if (smallestWeightID != wp._id)
                        { nxtSmallestWeight = smallestWeight; nxtSmallestWeightID = smallestWeightID; }
                        smallestWeight = weight; smallestWeightID = wp._id;
                    }
                    else if (weight < nxtSmallestWeight)
                    {
                        if (smallestWeightID != wp._id)
                        { nxtSmallestWeight = weight; nxtSmallestWeightID = wp._id; }
                    }
                }
                //D. LEFT
                if ((lastAddedPoint.wp.Left != null) && (IN[lastAddedPoint.wp.leftWP._id] != null))
                {
                    wp = lastAddedPoint.wp.leftWP;
                    weight = lastAddedPoint.addWeight + lastAddedPoint.wp.Left.weight;
                    if(IN[wp._id]!=null)
                        IN[wp._id].Update(lastAddedPoint.wp, weight);
                    if (weight < smallestWeight)//this weight is new smallest weight:
                    {
                        if (smallestWeightID != wp._id)
                        { nxtSmallestWeight = smallestWeight; nxtSmallestWeightID = smallestWeightID; }
                        smallestWeight = weight; smallestWeightID = wp._id;
                    }
                    else if (weight < nxtSmallestWeight)
                    {
                        if (smallestWeightID != wp._id)
                        { nxtSmallestWeight = weight; nxtSmallestWeightID = wp._id; }
                    }
                }
                #endregion

                //2. Ways are updates, now it's time to choose way will smallest weight to get into:
                if(smallestWeightID>=0)
                  moveToOutList(IN[smallestWeightID]);
                if (goThroughAllWays == true)//generate shortestWay everytime new way was added
                {
                    generateShortestWay();
                    ShortestWaysTable.addShortestWay(result);
                }
                smallestWeight = nxtSmallestWeight;
                smallestWeightID = nxtSmallestWeightID;
                nxtSmallestWeight = uint.MaxValue; nxtSmallestWeightID = -1;
            }
            // OK, so now we have all needed informations it's time 
            // to create wayDirectionStack and then shortestWay object and add it to shortestWayTable
            // TODO: implement checking if aroute from lastAddedPoint to dstPoint already occurs if true, build new
            // shortestWay and wayDir.Stack with use of it.
            generateShortestWay();
        }
        private ShortestWay generateShortestWay()
        {
            result = new ShortestWay();
            if (lastAddedPoint.wp._id == 2)
            {
                int a = 2; if (a == 2) a++;
            }
            wayOutInfo wO = lastAddedPoint;
            result.weight = lastAddedPoint.addWeight;
            while(true)
            {
                result.wpList.Insert(0, wO.wp);
                if (wO.elder == null) break;
                wO = OUT[wO.elder._id];
            }
            result.generateDirectionStack();
            return result;
        }
        private void findCurrentSmallestWeigth()
        {
            for (int i = 0; i < wnet.Count; i++)
            {
                if (IN[i] != null)
                {
                    if (IN[i].smallestWeight < smallestWeight)
                    {
                        nxtSmallestWeight = smallestWeight; nxtSmallestWeightID = smallestWeightID;
                        smallestWeight = IN[i].smallestWeight;
                        smallestWeightID = i;
                    }
                }
            }
        }
        private void moveToOutList(wayInInfo wi)
        {
            if (wi == null) return;
            wayOutInfo _out = new wayOutInfo(wi.wp,wi.elder,wi.smallestWeight);
            IN[wi.wp._id] = null;
            OUT[_out.wp._id] = _out;
            lastAddedPoint = _out;
        }
    }
}
