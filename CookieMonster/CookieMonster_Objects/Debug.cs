using System;
using System.Collections.Generic;

using System.Text;
using System.Runtime.Remoting;
using System.Drawing;
using QuickFont;

namespace CookieMonster.CookieMonster_Objects
{
    enum DebugLVL
    {
        info,fault,warn,error
    }
    class DebugMsg
    {
        public static Debug debugSys;
        string _msg = ""; public string msg { get { return getMsg(); } }
        DebugLVL _lvl; public DebugLVL lvl { get { return _lvl; } }
        object objRef; public object referencedObj { get { return objRef; } }//only with static debug msg's
        string fieldName;

        /// <summary>
        /// Will be displayed as info
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="l"></param>
        unsafe public DebugMsg(string msg) : this(msg,DebugLVL.info){}
        unsafe public DebugMsg(object watchedObj, string fieldname) : this(watchedObj, fieldname,DebugLVL.info) { }
        /// <summary>
        /// Static message constructor
        /// </summary>
        /// <param name="msg">prefix of watched field value</param>
        /// <param name="watchedField">pointer to field which value has to be displayed</param>
        /// <param name="l">debug msg level of priority</param>
        unsafe public DebugMsg(string msg, DebugLVL l)
        {
            System.DateTime now = System.DateTime.Now;
            _msg = "["+(now.Minute%10).ToString() +":"+ (now.Second % 60).ToString() + "]" + msg;
                _lvl = l;
            if(debugSys!=null)
                debugSys.addDebugMsg(this);


            //write to debugfile:
            Debug.debugLog.WriteLine(_msg);
        }
        unsafe public DebugMsg(object watchedObj,string fieldname, DebugLVL l)
        {
                _lvl = l;
                objRef = watchedObj;
                fieldName = fieldname;
                if (debugSys != null)
                debugSys.addDebugMsg(this);
        }        
                
        /// <summary>
        /// Build a debug message to be displayed
        /// </summary>
        /// <returns>string containing msg to display</returns>
        private string getMsg()
        {
            unsafe
            {
                if (objRef != null)
                {
                _msg = "";
                Type t = objRef.GetType();
                _msg += t.Name+"."+fieldName+"=";
                var pp = t.GetProperty(fieldName);
                var fi = t.GetField(fieldName); 
                if (pp == null)
                {
                    pp = t.BaseType.GetProperty(fieldName);
                    if (pp == null)
                    {
                        
                        if (fi != null) fi.GetValue(objRef);
                    }                        
                }
                    if(pp!=null)
                        _msg += pp.GetValue(objRef, null);
                    else
                        _msg += fi.GetValue(objRef);
                }
            }
            return _msg;
        }
    }
    class Debug
    {
        //logfile:
        static public System.IO.StreamWriter debugLog = new System.IO.StreamWriter("debugLog.txt", false);
        //statics:
        static QFont debug_font = TextManager.newQFont("MonoSpatial.ttf", 12,true);
        static Point dynamicStart = new Point(50, 700);
        static Point staticStart = new Point(1200 - 300, 700); // 300 - total width of dynamic msg
        static int dynamicMsgDuration = 500;

        const int dynamicMsgMax = 10;
        const QFontAlignment dynamicAlign = QFontAlignment.Left;
        const int staticMsgMax = 10;
        const QFontAlignment staticAlign = QFontAlignment.Right;
        const int debugMsgLineHeight = 25;//px
        List<DebugMsg> dynamicMessages;
        List<DebugMsg> dynamicMessagesQuery;
        Timer dynMsgRemainingTime;

        List<DebugMsg> staticMessages;

        List<Text> txt_staticMessages;
        List<Text> txt_dynamicMessages;
        Timer staticRefreshTimer;//too smooth means no ability to read
        const int staticRefreshRate = 100;

        public Debug()
        {
            DebugMsg.debugSys = this;//DebugMsg class need reference to current Debug instance
            
            dynamicMessages = new List<DebugMsg>();
            dynamicMessagesQuery = new List<DebugMsg>();
            staticMessages = new List<DebugMsg>();
            staticRefreshTimer = new Timer(Timer.eUnits.MSEC, staticRefreshRate, 0, true, false);
            // them simply can be generated onrun of Render method
            _createTextObjectForMessages();
        }

        public void addDebugMsg(DebugMsg d)
        {
            if (d.referencedObj == null)//it's dynamic message
            {
                if ((dynamicMessagesQuery.Count > 0) || (dynamicMessages.Count >= dynamicMsgMax))
                    dynamicMessagesQuery.Add(d);//adds msg to query
                else
                {
                    txt_dynamicMessages[dynamicMessages.Count].changeText(d.msg);
                    dynamicMessages.Add(d);
                }
                //start timer:
                if (dynMsgRemainingTime != null) return;
                dynMsgRemainingTime = new Timer(Timer.eUnits.MSEC, dynamicMsgDuration, 0, true, false);
                dynMsgRemainingTime.start();

            }
            else
            {
                txt_staticMessages[staticMessages.Count].changeText(d.msg);
                staticMessages.Add(d);
                if (staticMessages.Count >= staticMsgMax)
                {
                    staticMessages.RemoveAt(0);//remove first message, just proceed by one ;)
                }
            }
        }
        public void Update()
        {
            // dynamic: if timer is disabled it means it's time to go 
            // further, remove oldes msg and add new from query
            if ((dynMsgRemainingTime!=null)&&(dynMsgRemainingTime.enabled == false))
            {
                dynMsgRemainingTime.Dispose();
                dynamicMsgDuration = (dynamicMsgMax - dynamicMessages.Count) * 250 + dynamicMsgDuration - (dynamicMessagesQuery.Count * 250);
                dynamicMsgDuration = (dynamicMsgDuration < 0) ? 50 : dynamicMsgDuration;
                dynMsgRemainingTime = new Timer(Timer.eUnits.MSEC, dynamicMsgDuration);
                dynMsgRemainingTime.start();
                if (dynamicMessages.Count > 0)
                {
                    dynamicMessages.RemoveAt(0);
                    if (dynamicMessagesQuery.Count > 0)
                    {
                        txt_dynamicMessages[dynamicMessages.Count].changeText(dynamicMessagesQuery[0].msg);
                        dynamicMessages.Add(dynamicMessagesQuery[0]);
                        dynamicMessagesQuery.RemoveAt(0);
                    }
                    else if(dynamicMessages.Count>0)
                    {
                        txt_dynamicMessages[dynamicMsgMax-dynamicMessages.Count].changeText("");
                    }
                }   
            }
            //static: do nothing, messages are generated in render process
        }
        public void Render()
        {
            //dynamic:
            for (int i = 0; i < dynamicMessages.Count; i++)
            {
                txt_dynamicMessages[i].Update();
                txt_dynamicMessages[i].changeText(dynamicMessages[i].msg);
            }
            //static:    
            //if (staticRefreshTimer.enabled)//print last generated msg:
            //    for (int i = 0; i < staticMessages.Count; i++)
            //        //TODO: Tutaj muszisz miec Text wpisane w jakas tablice i odswiezac tylko zawartosc wiadomosci
            //        new Text(debug_font, staticStart.X, staticStart.Y - (debugMsgLineHeight * i), staticMessages[i].msg, staticAlign, 500);
            //        //txtMgr.produceText(debug_font, staticMessages[i].msg, staticStart.X, staticStart.Y - (debugMsgLineHeight * i), staticAlign).Print();
            //
            //else
            //{
                staticRefreshTimer.start();//refresh msg value:
                for (int i = 0; i < staticMessages.Count; i++)
                {
                    txt_staticMessages[i].Update();
                    txt_staticMessages[i].changeText(staticMessages[i].msg);
                }
            //txtMgr.produceText(debug_font, staticMessages[i].getMsg(), staticStart.X, staticStart.Y - (debugMsgLineHeight * i), staticAlign).Print();
            //}
        }

        private void _createTextObjectForMessages()
        {
            txt_staticMessages = new List<Text>();
            for (int i = 0; i < staticMsgMax; i++)
                txt_staticMessages.Add(new Text(debug_font, staticStart.X, staticStart.Y - (debugMsgLineHeight * i), "", staticAlign, 300));
            txt_dynamicMessages = new List<Text>();
            for (int i = 0; i < dynamicMsgMax; i++)
                txt_dynamicMessages.Add(new Text(debug_font, dynamicStart.X, dynamicStart.Y - (debugMsgLineHeight * i), "", dynamicAlign, 300));
        }
    }
}
