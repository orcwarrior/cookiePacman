using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using EngineApp;
using OpenTK.Input;
using QuickFont;
using OpenTK.Graphics;

namespace CookieMonster.CookieMonster_Objects
{
    
    class Menu : engineReference
    {
        public string name { get; private set; }
        private bool clearViewportOnOpen;
        public delegate void funcOnOpen();
        funcOnOpen onOpen;
        public delegate void funcOnUpdate();
        funcOnUpdate onUpdate;
        public delegate void funcOnRender();
        funcOnRender onRender;
        private bool opened;
        private bool menuFadeIn;
        private bool menuFadeOut;
        private bool fadeWithBlack;
        public string menuName{ get {return name;}}

        private List<Menu_Item> items;
        public Menu_Input_Item inputItem { get; private set; }
        private bool menuDisabled;
        private Timer fadeOutTimer; public bool fadingOut { get { return fadeOutTimer.enabled; } }
        private Timer fadeInTimer;
        public Timer submenuTimer { get; private set; }//enabled when adding submenu
        public Timer submenuCloseTimer { get; private set; }

        #region static fonts
        //Static fonts: (template)
        private const int fontSize = 26;
        public static QFont font = TextManager.newQFont("CheriPL.ttf", fontSize, FontStyle.Regular, false,      new Color4(210, 225, 250, 245));
        public static QFont font_Hover = TextManager.newQFont("CheriPL.ttf", fontSize, FontStyle.Regular, true, new Color4(70, 120, 255, 255));
        public static QFont font_Click = TextManager.newQFont("CheriPL.ttf", fontSize, FontStyle.Regular, true, new Color4(240, 240, 240, 245));

        private const int fontSizeSmall = 19;
        public static QFont fontSmall = TextManager.newQFont("CheriPL.ttf", fontSizeSmall, FontStyle.Regular, true,       new Color4(210, 225, 250, 245));
        public static QFont fontSmall_Hover = TextManager.newQFont("CheriPL.ttf", fontSizeSmall, FontStyle.Regular, true, new Color4(50, 150, 255, 255));
        public static QFont fontSmall_Click = TextManager.newQFont("CheriPL.ttf", fontSizeSmall, FontStyle.Regular, true, new Color4(255, 255, 255, 255));
        public static QFont fontSmall_Disabled = TextManager.newQFont("CheriPL.ttf", fontSizeSmall, FontStyle.Regular, true, new Color4(125, 125, 125, 255));
        
        private const int fontSizeSmallAlt = 25;
        public static QFont fontSmallAlt = TextManager.newQFont("Rumpelstiltskin.ttf", fontSizeSmallAlt, FontStyle.Regular, true,       new Color4(240, 240, 240, 245));
        public static QFont fontSmallAlt_Hover = TextManager.newQFont("Rumpelstiltskin.ttf", fontSizeSmallAlt, FontStyle.Regular, true, new Color4(50, 150, 255, 255));
        public static QFont fontSmallAlt_Click = TextManager.newQFont("Rumpelstiltskin.ttf", fontSizeSmallAlt, FontStyle.Regular, true, new Color4(255, 255, 255, 255));
        #endregion

        static Menu()
        {
            // There was a setting of colours of static QFont objects, it's no longer needed
            // plus it would be invaild (color need to be known at calling of TextMan.newQFont;
        }

        public Menu(string n, funcOnOpen _onOpen)
        {
            name = n;
            onOpen = _onOpen;
            opened = false;
            menuFadeIn = menuFadeOut = fadeWithBlack = false;
            clearViewportOnOpen = true;
            fadeOutTimer = new Timer(Timer.eUnits.MSEC, 200, 0, true, false);
            fadeInTimer = new Timer(Timer.eUnits.MSEC, 400, 0, true, false);
            submenuTimer = new Timer(Timer.eUnits.MSEC, 300, 0, true, false);
            submenuCloseTimer = new Timer(Timer.eUnits.MSEC, 190, 0, true, false);

            items = new List<Menu_Item>();
        }
        public Menu(string n, funcOnOpen _onOpen, Obj cur) : this(n,_onOpen)
        {
            Menu_Manager.cursor = cur;
        }
        public Menu(string n, funcOnOpen _onOpen,funcOnUpdate _onUpdate, Obj cur) : this(n, _onOpen, cur)
        {
            onUpdate = _onUpdate;
        }
        public Menu(string n, funcOnOpen _onOpen,funcOnUpdate _onUpdate, funcOnRender _onRender, Obj cur) : this(n, _onOpen,_onUpdate, cur)
        {
            onRender = _onRender;
        }
        private bool mouseWasPresed;
        private bool fadeInTimerWasActive;
        private bool fadeOutTimerWasActive;
        private bool submenuTimerWasActive;
        private bool submenuCloseTimerWasActive;
        public void Update() 
        {
            if (!opened)
            {
                if(onOpen!=null)
                onOpen();
                opened = true;
            }
            else
            {//update & stuff:
                Obj cursor = Menu_Manager.cursor;
                if(onUpdate!=null) onUpdate();
                
                if (!menuDisabled)
                {
                    bool somewhereCursorIn = false;
                    //Checks for mouse state:
                    bool triggerItemClick = false; //if flags is sat to true, it will trigger Click event on first item that mouse is in
                    if (engine.menuManager.getButtonState(MouseButton.Left))
                    {
                        mouseWasPresed = true;
                    }
                    else if (mouseWasPresed && engine.menuManager.getButtonState(MouseButton.Left) == false)
                    { //mouse was pressed, and now released ->trigger onClick event
                        triggerItemClick = true;
                        mouseWasPresed = false;
                    }
                    else
                        mouseWasPresed = false;
                               

                    //update input Item first:
                    if (inputItem != null)
                    {
                        inputItem.Update();
                        //checks if cursor is in area of input-item:
                        if (inputItem.CursorIn(cursor.x, cursor.y))
                        {
                            somewhereCursorIn = true;
                            if (triggerItemClick) inputItem._onMouseClick();
                        }
                    }

                    //last items == last rendered(On Top) -> so they're more important
                    for (int i = items.Count-1; i >= 0; i--)
                    {
                        if ((!somewhereCursorIn)&&(items[i].CursorIn(cursor.x, cursor.y)))
                        {
                            if (triggerItemClick) items[i]._onMouseClick();
                            if (this == null) return; // fix: sometimes click will trigger deleting of menu
                            if (items.Count > i && items[i] != null)     // or item.
                            {
                                items[i]._onMouseIn();
                            }
                            somewhereCursorIn = true;
                        }
                        else
                            items[i]._onMouseOut();
                    }
                }

                //check for fades
                if (fadeInTimer.enabled)
                {
                    fadeInTimerWasActive = true;
                    for (int i = 0; i < items.Count; i++)
                        items[i].fadeIn(fadeInTimer);
                    if (inputItem!=null) inputItem.fadeIn(fadeInTimer);
                }
                else if (fadeInTimerWasActive)
                { // BUGFIX: when timer was last hit not on ideal 0 value till-the end it will leave some alpha
                  // so we need to call fadeIn once again when .enabled is no longer true
                    fadeInTimerWasActive = false;
                    for (int i = 0; i < items.Count; i++)
                        items[i].fadeIn(Timer.zeroTimer);
                    if (inputItem != null) inputItem.fadeIn(Timer.zeroTimer);
                }
                if (fadeOutTimer.enabled)
                {
                    fadeOutTimerWasActive = true;
                    for (int i = 0; i < items.Count; i++)
                        items[i].fadeOut(fadeOutTimer);//push fadeInTimer so item will be able to 
                    if (inputItem != null) inputItem.fadeOut(fadeOutTimer);
                }
                else if (fadeOutTimerWasActive)
                { // BUGFIX: when timer was last hit not on ideal 0 value till-the end it will leave some alpha
                    // so we need to call fadeIn once again when .enabled is no longer true
                    fadeOutTimerWasActive = false;
                    for (int i = 0; i < items.Count; i++)
                        items[i].fadeOut(Timer.zeroTimer);
                    if (inputItem != null) inputItem.fadeOut(Timer.zeroTimer);
                }
                //check for submenu fades:
                if (submenuTimer.enabled)
                {
                    submenuTimerWasActive = true;
                    for (int i = 0; i < items.Count; i++)
                        items[i].submenu(submenuTimer);//push fadeInTimer so item will be able to 
                    if (inputItem != null) inputItem.submenu(submenuTimer);
                }
                else if (submenuTimerWasActive == true)
                {//BUGFIX:
                    submenuTimerWasActive = false;
                    for (int i = 0; i < items.Count; i++)
                        items[i].submenu(Timer.zeroTimer);//push fadeInTimer so item will be able to 
                    if (inputItem != null) inputItem.submenu(Timer.zeroTimer);
                }
                if (submenuCloseTimer.enabled)
                {
                    submenuCloseTimerWasActive = true;
                    for (int i = 0; i < items.Count; i++)
                        items[i].submenuClose(submenuCloseTimer);//push fadeInTimer so item will be able to 
                    if (inputItem != null) inputItem.submenuClose(submenuCloseTimer);
                }
                else if (submenuCloseTimerWasActive == true)
                {//BUGFIX:
                    submenuCloseTimerWasActive = false;
                    for (int i = 0; i < items.Count; i++)
                        items[i].submenuClose(Timer.zeroTimer);//push fadeInTimer so item will be able to 
                    if (inputItem != null) inputItem.submenuClose(Timer.zeroTimer);
                }                   
            };
        }
        public void Render()
        {
            //first render items without mouse over:
            for (int i = 0; i < items.Count; i++)
            {
                if(!items[i].inHover)
                items[i].Render();
            }   
            //ther render hovered item:
            for (int i = 0; i < items.Count; i++)
            {
                if(items[i].inHover)
                items[i].Render();
            }
            //at least render input items:
            if (inputItem != null) inputItem.Render();

            if(onRender!=null) onRender(); //if there is any onRender function, call it
            //render cursor overlaying all rest: (but render it only once!)
            //if (cursor != null) { cursor.Render(); engine.menuManager.cursorRendered = true; }
        }
        public void addItem(Menu_Item itm)
        {
            itm.setOwner(this);
            items.Add(itm);
        }
        public void addInputItem(Menu_Input_Item inputItm)
        {
            ((Menu_Item)inputItm).setOwner(this);
            inputItem = inputItm;
        }
        public void clearMenuItems()
        {
            for (int i = 0; i < items.Count; i++)
                items[i].Close();
            items.Clear();
        }
        public void removeMenuItem(Menu_Item itm)
        {// IF item founded and removed, close it too.
            if (items.Remove(itm)) itm.Close();
        }
        public Menu_Item getItem(int index)
        {
            if (index < items.Count)
                return items[index];
            else
                return null;
        }
        public Menu_Item getItemByName(string Name)
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i].name == Name)
                    return items[i];
            return null;
        }
        public void closeMenu()
        {
            for (int i = 0; i < items.Count; i++)
                items[i].Close();
            opened = false;
        }

        public void fadeOut()
        {
            fadeOutTimer.start();
        }

        public void fadeIn()
        {
            fadeInTimer.start();
        }

        internal void addedSubmenu()
        {
            submenuTimer.start();
        }

        internal void closeSubmenu()
        {
            submenuCloseTimer.start();
        }
        public override string ToString()
        {
            return menuName;
        }
        /// <summary>
        /// Disabling menu causes menu_items are no longer
        /// hover-able
        /// </summary>
        public void Disable()
        {
            menuDisabled = true;
        }
        /// <summary>
        /// Back to default Menu state (now you can hover menu items)
        /// </summary>
        public void Enable()
        {
            menuDisabled = false;
        }

    }
}
