using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{
    class Obj_Animation
    {
        #region fields
        private const double DEFAULT_SPEED = 1.0;

        public enum eAniType { PointToPoint, Random };
        public enum eLoopType { ToOrginStep, ToFirst, Rewind, None };
        public enum eAffectionFlags { NONE = 0, affectPos = 1, affectScale = 2, affectOpacity = 4 }
        public eAffectionFlags affectionFlags;
        public bool isIngameAnimation { get; private set; }
        public bool isFinished { get { return _isFinished; } }
        private bool _isFinished;

        private eAniType type;
        private eLoopType loop;
        private int orginPosX, orginPosY;
        private int frames; //frames that passed from the begining
        private List<int> keyframePosX = new List<int>();
        private List<int> keyframePosY = new List<int>();
        private List<double> keyframeScale = new List<double>(); double scaleStep;
        private List<double> keyframeOpacity = new List<double>();
        private List<double> keyframeSpeed = new List<double>(); //pixels/frame
        /// <summary>
        /// if KeyframeDuration is present animation steps will be generated based on duration of next keyframe
        /// </summary>
        private List<Timer> keyframeDuration = new List<Timer>();
        private int currentKeyframe, nextKeyframe;
        private double currentPosX, currentPosY; // it need to be stored double cuz there is able to move
        private Obj owner = null;               // obj by 0.1px per frame etc. this value is always
        private bool relativeMovement;
        private double scaleLastPartDone;
        // rounded for Obj coordinates
        #endregion
        /// <summary>
        /// creating Obj_Animation but without frames,
        /// they need to be added by addKeyframe method
        /// </summary>
        /// <param name="aniObj"></param>
        public Obj_Animation(Obj aniObj)
        {
            orginPosX = aniObj.x; orginPosY = aniObj.y;
            owner = aniObj;
            type = eAniType.PointToPoint;
            loop = eLoopType.ToOrginStep;
            currentKeyframe = 0; nextKeyframe = 1;
            currentPosX = (double)orginPosX;
            currentPosY = (double)orginPosY;
            frames = 0;
            affectionFlags = eAffectionFlags.affectOpacity | eAffectionFlags.affectPos | eAffectionFlags.affectScale;


            if (Game.self.gameManager==null || !((Game.self.gameState & Game.game_state.Game) == Game.game_state.Game) || Game.self.gameManager.gamePaused)
                isIngameAnimation = false;
            else
                isIngameAnimation = true;
        }
        /// <summary>
        /// Creates Obj_Animation, getting orgin positions from current obj positions
        /// </summary>
        /// <param name="aniObj"></param>
        /// <param name="tX"></param>
        /// <param name="tY"></param>
        public Obj_Animation(Obj aniObj, int tX, int tY)
            : this(aniObj)
        {
            keyframePosX.Add(tX); keyframePosY.Add(tY);
        }
        /// <summary>
        /// Creates Obj_Animation, getting orgin positions from current obj positions
        /// </summary>
        /// <param name="aniObj"></param>
        /// <param name="tX"></param>
        /// <param name="tY"></param>
        /// <param name="tS"></param>
        public Obj_Animation(Obj aniObj, int tX, int tY, double tS)
            : this(aniObj, tX, tY)
        {
            keyframeSpeed.Add(tS); keyframeSpeed.Add(tS);
        }
        public Obj_Animation(Obj aniObj, int tX, int tY, double tSpeed, double tScale)
            : this (aniObj,tX,tY,tSpeed)
        {
            keyframeScale.Add(tScale);
        }
        public Obj_Animation(Obj aniObj, int tX, int tY, double tSpeed, double tScale,bool createKeyframeFromOrginObj)
            : this(aniObj, tX, tY, tSpeed,tScale)
        {
            if (createKeyframeFromOrginObj)
            {
                keyframePosX.Insert(0, aniObj.x);
                keyframePosY.Insert(0, aniObj.y);
                keyframeScale.Insert(0, Math.Max(aniObj.scale[0], aniObj.scale[1]));
            }
        }

        public void computeFrame()
        {
            if (nextKeyframe == -1) return;
            double stepx = 0.0, stepy = 0.0;
            if (frames == 0)
            {
                if (keyframeScale.Count > 0)
                {
                    if(keyframeDurationIsBasedOnTimer())
                        if (keyframeDuration[nextKeyframe] != null)
                            keyframeDuration[nextKeyframe].start();

                    if ((affectionFlags & eAffectionFlags.affectScale) == eAffectionFlags.affectScale)
                        owner.ScaleAbs = keyframeScale[0];
                }
            }

            if ((affectionFlags & eAffectionFlags.affectPos) == eAffectionFlags.affectPos)
            {
                computeXYstep(ref stepx, ref stepy);

                if (!keyframeReached(stepx, 0.0))//check for x only:
                    currentPosX += stepx;
                if (!keyframeReached(0.0, stepy))//check for y only:
                    currentPosY += stepy;
            }

            if ((affectionFlags & eAffectionFlags.affectScale) == eAffectionFlags.affectScale)
            {
                computeScaleStep();
            }


            // check if keyframe target pos was reached:
            if (keyframeReached(stepx, stepy))
            {
                correctTransforms();
                if (type == eAniType.PointToPoint)
                {
                    proceedToNextKeyframe();                    
                }
            }

            if ((affectionFlags & eAffectionFlags.affectPos) == eAffectionFlags.affectPos)
            {
                owner.x = (int)Math.Floor(currentPosX); owner.y = (int)Math.Floor(currentPosY);
            }

            //sets new scale (only if scale is not time-based
            if ((nextKeyframe < keyframeScale.Count)&& (keyframeDuration.Count < nextKeyframe)&&((affectionFlags & eAffectionFlags.affectScale) == eAffectionFlags.affectScale))
            { 
                double[] scale = new double[2];
                scale = owner.scale;
                scale[0] += scaleStep;
                scale[1] += scaleStep;
                owner.scale = scale;
            };
            frames++;
        }

        private bool keyframeReached(double stepx, double stepy)
        {
            //keyframe duration is present
            if ( keyframeDurationIsBasedOnTimer() &&
                (keyframeDuration[nextKeyframe].enabled == false))
            {
                return true;
            }
            else if (!keyframeDurationIsBasedOnTimer())
            {
                //movement absolute:
                if (relativeMovement == false &&
                    (((stepx > 0.0) && (currentPosX >= keyframePosX[nextKeyframe]))
                      || ((stepx < 0.0) && (currentPosX <= keyframePosX[nextKeyframe])) || (stepx == 0.0))
                 && (((stepy > 0.0) && (currentPosY >= keyframePosY[nextKeyframe]))
                   || ((stepy < 0.0) && (currentPosY <= keyframePosY[nextKeyframe])) || (stepy == 0.0))
                   )
                {
                    return true;
                }
                else if (relativeMovement &&
                    (((stepx > 0.0) && (currentPosX - orginPosX >= keyframePosX[nextKeyframe]))
                      || ((stepx < 0.0) && (currentPosX - orginPosX <= keyframePosX[nextKeyframe])) || (stepx == 0.0))
                 && (((stepy > 0.0) && (currentPosY - orginPosY >= keyframePosY[nextKeyframe]))
                   || ((stepy < 0.0) && (currentPosY - orginPosY <= keyframePosY[nextKeyframe])) || (stepy == 0.0))
                        )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool keyframeDurationIsBasedOnTimer()
        {
            return (keyframeDuration.Count > nextKeyframe) && (keyframeDuration[nextKeyframe] != null);
        }

        private void computeXYstep(ref double stepx, ref double stepy)
        {
            double speed;
            double oldCurrentPosX = currentPosX;
            double oldCurrentPosY = currentPosY;
            if (keyframeDuration.Count > nextKeyframe) // compute base on keyframe duration
            { //TODO: obivously...

            }
            else //computations will be based on keyframe Speed:
            {
                if (keyframeSpeed.Count > nextKeyframe)
                    speed = keyframeSpeed[nextKeyframe];
                else
                    speed = DEFAULT_SPEED;

                if (keyframePosX[currentKeyframe] < keyframePosX[nextKeyframe])
                    stepx = speed;
                else if (keyframePosX[currentKeyframe] > keyframePosX[nextKeyframe])
                    stepx = (-1.0) * speed;

                if (keyframePosY[currentKeyframe] < keyframePosY[nextKeyframe])
                    stepy = speed;
                else if (keyframePosY[currentKeyframe] > keyframePosY[nextKeyframe])
                    stepy = (-1.0) * speed;
            }
        }
        private void computeScaleStep()
        {
            if (keyframeDuration.Count > nextKeyframe) // compute base on keyframe duration
            {
                double scaleDifference = keyframeScale[nextKeyframe] - keyframeScale[currentKeyframe];
                Timer keyTimer = keyframeDuration[nextKeyframe];
                double partDone = keyTimer.totalTime - keyTimer.currentTime;
                partDone /= keyframeDuration[nextKeyframe].totalTime;
                double thisStepProgress = partDone - scaleLastPartDone;

                //sets new scale:
                double[] scale = new double[2];
                scale = owner.scale;
                scale[0] = keyframeScale[currentKeyframe] + scaleDifference * partDone;
                scale[1] = keyframeScale[currentKeyframe] + scaleDifference * partDone;
                owner.scale = scale;

                scaleLastPartDone = partDone;
            }
            else
            {
                if ((nextKeyframe != -1) && (nextKeyframe < keyframeScale.Count))
                {
                    double predictedFrames = Math.Max(Math.Abs(keyframePosX[currentKeyframe] - keyframePosX[nextKeyframe]), Math.Abs(keyframePosY[currentKeyframe] - keyframePosY[nextKeyframe])) / keyframeSpeed[nextKeyframe];
                    if (predictedFrames != 0.0)
                        scaleStep = Math.Abs(keyframeScale[nextKeyframe] - keyframeScale[currentKeyframe]) / predictedFrames;
                    else
                        scaleStep = keyframeScale[nextKeyframe];
                }
            }
        }
        /// <summary>
        /// Corrects positions and scale to be just the same
        /// as specified in nextKeyframe Lists
        /// </summary>
        private void correctTransforms()
        {
            //correct position:
            if (((affectionFlags & eAffectionFlags.affectPos) == eAffectionFlags.affectPos))
            {
                if (!relativeMovement)
                {
                    currentPosX = keyframePosX[nextKeyframe];
                    currentPosY = keyframePosY[nextKeyframe];
                }
                else
                {
                    currentPosX = orginPosX + keyframePosX[nextKeyframe];
                    currentPosY = orginPosY + keyframePosY[nextKeyframe];

                }
            }
            //correct scale:
            if ((affectionFlags & eAffectionFlags.affectScale) == eAffectionFlags.affectScale)
            {
                double[] scale = new double[2];
                if (keyframeScale.Count >= nextKeyframe)
                {
                    scale[1] = scale[0] = keyframeScale[nextKeyframe];
                    owner.scale = scale;
                }
            }
        }

        private void proceedToNextKeyframe()
        {
            if (loop == eLoopType.ToOrginStep)
            {
                currentPosX = (double)orginPosX;
                currentPosY = (double)orginPosY;
                currentKeyframe = 0;
                nextKeyframe = 1 % keyframePosX.Count;
            }
            if (loop == eLoopType.ToFirst)
            {
                currentKeyframe = nextKeyframe;
                nextKeyframe = (nextKeyframe + 1) % keyframePosX.Count;
            }
            else if (loop == eLoopType.Rewind)
            {
                //toDo: rewind whole list instead
                nextKeyframe = currentKeyframe;
                currentKeyframe = (nextKeyframe - 1) % keyframePosX.Count;
            }
            else if (loop == eLoopType.None)
            {
                if (nextKeyframe < keyframePosX.Count - 1)
                {
                    currentKeyframe = nextKeyframe;
                    nextKeyframe++;
                }
                else _isFinished = true; //signalize that ani is finished already
            }
            //curent frame/ next frame calculated, start keyframeDuration timer if it's present
            if (nextKeyframe < keyframeDuration.Count)
                if (keyframeDuration[nextKeyframe] != null)
                    keyframeDuration[nextKeyframe].start();
        }

        public void addKeyframe(int tX, int tY, double tS)
        {
            keyframePosX.Add(tX); keyframePosY.Add(tY);
            keyframeSpeed.Add(tS);
        }
        public void addKeyframe(int tX, int tY, double tSpeed,double tScale)
        {
            addKeyframe(tX, tY, tSpeed);
            keyframeScale.Add(tScale);
        }
        public void addKeyframe(int tX, int tY, double tSpeed, double tScale, Timer duration)
        {
            addKeyframe(tX, tY, tSpeed,tScale);
            keyframeDuration.Add(duration);

        }
        public void setLoopType(eLoopType l)
        {
            loop = l;
        }
        public void gotoKeyframe(int f)
        {
            if ((f >= 0) && (f + 1 < keyframePosX.Count))
            {
                if ((affectionFlags & eAffectionFlags.affectPos) == eAffectionFlags.affectPos)
                {
                    if (!relativeMovement)
                    {
                        currentPosX = owner.x = keyframePosX[f];
                        currentPosY = owner.y = keyframePosY[f];
                    }
                    else
                    {
                        currentPosX = orginPosX;
                        currentPosY = orginPosY;
                    }
                }
                if ((affectionFlags & eAffectionFlags.affectScale) == eAffectionFlags.affectScale)
                {
                    //double predictedFrames = Math.Max(Math.Abs(keyframePosX[f] - keyframePosX[f + 1]), Math.Abs(keyframePosY[f] - keyframePosY[f + 1])) / keyframeSpeed[f + 1];
                    //scaleStep = Math.Abs(keyframeScale[f + 1] - keyframeScale[f]) / predictedFrames;
                    scaleLastPartDone = 0.0;
                    //rescale:
                    double[] scale = new double[2];
                    scale[1] = scale[0] = keyframeScale[f];
                    owner.scale = scale;
                }
                currentKeyframe = f;
                nextKeyframe = f + 1;
            }
        }
        public void setRelativeMovement()
        {
            relativeMovement = true;
        }
        public void setAbsoluteMovement()
        {
            relativeMovement = false;
        }
    }

}
