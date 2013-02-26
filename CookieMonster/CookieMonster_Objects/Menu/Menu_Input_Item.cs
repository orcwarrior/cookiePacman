using System;
using System.Collections.Generic;
using System.Text;
using QuickFont;

namespace CookieMonster.CookieMonster_Objects
{
    class Menu_Input_Item : Menu_Item
    {
        Obj BG;//cursor - that "blinking" line
        private Timer cursorON;
        private Timer cursorOFF;
        private bool lastCursorWasON = false;
        private int frameLenght;
        private bool active;
        public string defaultValue { get; private set; }
        public bool sendDefaultValueToBuffer;

        public Menu_Input_Item( float x, float y, QFont _font) : base( "",x,y,_font)
        {
            cursorON = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
            cursorOFF = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
            frameLenght = int.MaxValue;
        }
        public Menu_Input_Item(float x, float y, QFont _font,Obj bg)
            : this( x, y, _font)
        {
            BG = bg;
        }
        public Menu_Input_Item(float x, float y, QFont _font, Obj bg, string defaultVal, bool sendTobuffer)
            : base(defaultVal, x, y, _font)
        {
            cursorON = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
            cursorOFF = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
            frameLenght = int.MaxValue;
            BG = bg;
            defaultValue = defaultVal;
            sendDefaultValueToBuffer = sendTobuffer;
        }
        public Menu_Input_Item(float x, float y, QFont _font,QFont _fontHover, Obj bg, string defaultVal, bool sendTobuffer)
            : base(defaultVal, x, y, _font, _fontHover)
        {
            cursorON = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
            cursorOFF = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
            frameLenght = int.MaxValue;
            BG = bg;
            defaultValue = defaultVal;
            sendDefaultValueToBuffer = sendTobuffer;
        }
        public Menu_Input_Item(float x, float y, QFont _font, QFont _fontHover, Obj bg, string defaultVal, bool sendTobuffer, mouseEvt click)
            : base(defaultVal, x, y, _font, _fontHover,_fontHover,click)
        {
            cursorON = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
            cursorOFF = new Timer(Timer.eUnits.MSEC, 500, 0, true, false);
            frameLenght = int.MaxValue;
            BG = bg;
            defaultValue = defaultVal;
            sendDefaultValueToBuffer = sendTobuffer;
        }
        public void _onMouseClick()
        {
            InputManager.inputLogging = true;
            base._onMouseClick();
            active = true;
            if (sendDefaultValueToBuffer)
                InputManager.sendTobuffer(defaultValue);
        }
        public void Update()
        {
            if (!active) return;

            int start = InputManager.getInputBuffer().Length - frameLenght;
            if (start < 0) start = 0;
            base.value = InputManager.getInputBuffer().Substring(start);

            if (lastCursorWasON && !cursorON.enabled)
            {
                lastCursorWasON = false;
                cursorOFF.start();
            }
            else if (!lastCursorWasON && !cursorOFF.enabled)
            {
                lastCursorWasON = true;
                cursorON.start();
            }
            if (lastCursorWasON)
                base.value += "|";
        }
        public void Render()
        {
            if (BG != null)
                BG.prepareRender();

            base.Render();
        }
    }
}
