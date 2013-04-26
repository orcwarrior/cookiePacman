using System;
using System.Collections.Generic;
using System.Text;
using QuickFont;
using System.Drawing;
using EngineApp;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Menu items aren't added to viewport, do sth with that!
    /// </summary>
    class Menu_Item : engineReference
    {
        private Menu owner; //owner of this menu item
        public string name { get; private set; }
        //Obj based menu item:
        private Obj visual;
        private Obj visualOnHover;
        private Obj visualOnClick;
        private Obj renderedVisual { get { return _renderedVisual; }
            set 
            {
                if (_renderedVisual != null) { _renderedVisual.addedToViewport = false; _renderedVisual.setVisibleWithChilds(false); }
                _renderedVisual = value;
                if (_renderedVisual != null) { _renderedVisual.setVisibleWithChilds(true); } 
            }
        }
        private Obj _renderedVisual;

        //Text based menu item:
        private string _value;
        public string value { get { return _value; } set { _value = value; itemText.changeText(value); } }
        Text itemText; //rendered Text class item
        public QFont font;
        QFont fontOnHover;
        QFont fontOnClick; 
        public bool inHover { get; private set; }
        public Rectangle bbox { get; private set; }
        /// <summary>
        /// Get/Set's layer of used visual (Text or Obj(all states))
        /// </summary>
        public int layer
        {
            get { if (itemText != null) return itemText.layer; if (visual != null)return visual.layer; return -1; }
            set { 
            if (itemText != null) itemText.layer = value;
            if (visual != null) visual.layer = value;
            if (visualOnClick != null) visualOnClick.layer = value;
            if (visualOnHover != null) visualOnHover.layer = value;
            }
        }
        public delegate void mouseEvt();
        private mouseEvt onMouseIn;
        private mouseEvt onMouseOut;
        private mouseEvt onClick;

        //--------------
        // Item based on TEXTs
        public Menu_Item(string text, float x, float y, QFont _font)
        {
            _value = text;
            name = text;
            TextManager txtMan = engine.textManager;
            font = _font;
            itemText = new Text(font, x, y, text);
            //FIX: Font are rendered just at oppening of submenus for a frame, but they shouldn't
            itemText.updatedThisFrame = false;
        }
        public Menu_Item(string text, float x, float y, QFont _font, QFont _fontOnHover)
            : this(text, x, y, _font)
        {
            fontOnHover = _fontOnHover;
        }
        public Menu_Item(string text, float x, float y, QFont _font, QFont _fontOnHover, QFont _fontOnClick)
            : this(text, x, y, _font, _fontOnHover)
        {
            fontOnClick = _fontOnClick;
        }
        public Menu_Item(string text, float x, float y, QFont _font, QFont _fontOnHover, QFont _fontOnClick, mouseEvt click)
            : this(text, x, y, _font, _fontOnHover, _fontOnClick)
        {
            onClick = click;
        }
        public Menu_Item(string text, float x, float y, QFont _font, QFont _fontOnHover, QFont _fontOnClick, mouseEvt _in, mouseEvt _out, mouseEvt click)
            : this(text, x, y, _font, _fontOnHover, _fontOnClick, click)
        {
            onMouseIn = _in;
            onMouseOut = _out;
        }
        //--------------
        // Item based on OBJects
        public Menu_Item(string n, Obj v, Obj vH, Obj vC, mouseEvt _in, mouseEvt _out, mouseEvt c)
        {
            name = n;
            renderedVisual = visual = v;
            visualOnHover = vH;
            visualOnClick = vC;
            onMouseIn = _in;
            onMouseOut = _out;
            onClick = c;
            bbox = new Rectangle(v.x, v.y, v.width, v.height);
        }
        public void _onMouseClick()
        {
            if (onClick != null)
                onClick();
            inHover = false;//not true, it's onClick rite now!
            if (visualOnClick != null)
            {
                renderedVisual = visualOnClick;
            }
            if (fontOnClick != null && owner.submenuTimer.enabled == false)//BUGFIX: Don't change visuals during submenu ani!
                itemText.changeFont(fontOnClick);
            //sometimes actions like nulling item/whole object can happen onClick, so check for them
            if (this == null || this.owner == null) return;
        }
        public void _onMouseIn()
        {
            // NOTHING TO DO IN HERE!
            if (onMouseIn == null && visualOnHover == null && fontOnHover == null) return;

            if (!inHover)
            {
                inHover = true;
                if (visualOnHover != null)
                {
                    renderedVisual.visible = false;
                    renderedVisual = visualOnHover;
                    renderedVisual.visible = true;

                    if (onMouseIn == null)//then play default beep:
                    {
                        Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP.ogg", false, false);
                        beep.volume = 0.52;
                        beep.Play();
                    }
                    else
                        onMouseIn();//call customized onHover function
                }
                else if (fontOnHover != null)
                {
                    if (owner.submenuTimer.enabled == false)//BUGFIX: Don't change visuals during submenu ani!
                        itemText.changeFont(fontOnHover);
                    if (onMouseIn == null)//then play default beep:
                    {
                        Sound beep = new Sound(Sound.eSndType.SFX, "../data/Sounds/MENU_BEEP_SHORT.ogg", false, false);
                        beep.volume = 0.62;
                        beep.Play();
                    }
                    else
                        onMouseIn();//call customized onHover function
                }
            }
        }
        public void _onMouseOut()
        {
            if (inHover)
            {
                inHover = false;
                if (visualOnHover != null && visualOnHover.objAnimation != null)
                {
                    visualOnHover.objAnimation.gotoKeyframe(0);
                }
                renderedVisual = visual;
                if (font != null && owner.submenuTimer.enabled == false)//BUGFIX: Don't change visuals during submenu ani!
                    itemText.changeFont(font);
            }
        }
        public bool CursorIn(int x, int y)
        {
            int itemX, itemY;
            int itemWidth, itemHeight; 
            // bugfix: cursor obj itself is in lil different
            // position than win.cursor, so we moving it back there:
            x += 10; y += 10;
            if (visual != null)
            {//Cursor intersection check for Obj based menu:
                itemX = visual.x; itemY = visual.y;
                int maxX = itemX + visual.width, maxY = itemY + visual.height;
                //If there is some child, count them too!
                if (visual.childObjs != null)
                {
                    for (int i = 0; i < visual.childObjs.Count; i++)
                    {
                        Obj tmp = visual.childObjs[i];
                        if (visual.x + tmp.x + tmp.width > maxX)
                            maxX = visual.x + tmp.x + tmp.width;
                        if (visual.y + tmp.y + tmp.height > maxX)
                            maxY = visual.y + tmp.y + tmp.height;
                    }
                }
                itemWidth = maxX - itemX; itemHeight = maxY - itemY;
            }
            else if (itemText != null)
            {//Cursor intersection check for Text type Menu item:

                itemX = (int)itemText.x; itemY = (int)itemText.y;
                SizeF size = font.Measure(value);
                itemWidth = (int)size.Width; itemHeight = (int)size.Height;

            }
            else
                return false; //No obj, no Text ->Item is blank so how u could hover that?
            int tolerance_x = itemWidth / 15, tolerance_y = itemHeight / 15;


            if (((x > itemX - tolerance_x) && (x + tolerance_x < itemX + itemWidth))
            && ((y > itemY + tolerance_y) && (y - tolerance_y < itemY + itemHeight)))
            {
                return true;
            }
            else
                return false;
        }
        public void Render()
        {// menu items aren't added to viewport they rendered byself
            if (renderedVisual != null)
            {
                renderedVisual.prepareRender();
                //if (renderedVisual == visualOnClick)
                //   renderedVisual = visualOnHover;
            }
            else if (itemText != null)
            {
                itemText.Update();
            }
        }

        internal void fadeOut(Timer fadeOutTimer)
        {
            double multi = ((double)fadeOutTimer.currentTime / (double)fadeOutTimer.totalTime);
            //if (multi < 0.0) multi = 0.0;
            if (visual != null)
                visual.setAllTexsAlpha((byte)(255 * multi));
            if (visualOnHover != null)
                visualOnHover.setAllTexsAlpha((byte)(255 * multi));
            if (itemText != null)
            {
                QFont ftmp = itemText.fontFace;
                ftmp.Options.Colour.A = (float)multi;
                itemText.changeFont(ftmp);
            }
        }

        internal void fadeIn(Timer fadeInTimer)
        {
            double multi = 1.0 - ((double)fadeInTimer.currentTime / (double)fadeInTimer.totalTime);
            //if (multi < 0.0) multi = 0.0;
            if (visual != null)
                visual.setAllTexsAlpha((byte)(255 * multi));
            if (visualOnHover != null)
                visualOnHover.setAllTexsAlpha((byte)(255 * multi));
            if (itemText != null)
            {
                QFont ftmp = itemText.fontFace;
                if (fontOnHover != null) fontOnHover.Options.Colour.A = 1f;
                if (fontOnClick != null) fontOnClick.Options.Colour.A = 1f;
                font.Options.Colour.A = 1f;
                ftmp.Options.Colour.A = (float)multi;
                itemText.changeFont(ftmp);
            }
        }

        internal void setOwner(Menu menu)
        {
            owner = menu;
        }

        internal void submenu(Timer submenuTimer)
        {
            double moveLenght = 450.0;
            double startAddLenght = 0.0;
            Menu_Manager mgr = engine.menuManager;
            if (this.owner.Equals(mgr.current_menu) && mgr.subSubMenu != null)
            {
                startAddLenght = moveLenght;
                moveLenght = 225.0;
            }
            else if (this.owner.Equals(mgr.subMenu))
            {
                moveLenght = 250.0;
            }
            double multi = 1.0 - ((double)submenuTimer.currentTime / (double)submenuTimer.totalTime);
            //if (multi < 0.0) multi = 0.0;
            if (visual != null)
                visual.x = (int)(visual.orgX - startAddLenght - multi * moveLenght);
            if (visualOnHover != null)
                visualOnHover.x = (int)(visualOnHover.orgX - startAddLenght - multi * moveLenght);
            if (itemText != null)
                itemText.MoveAbs((int)(itemText.orgX - startAddLenght - multi * moveLenght), (int)itemText.orgY);
        }

        internal void submenuClose(Timer submenuCloseTimer)
        {
            double moveLenght = 450.0;
            double startAddLenght = 0.0;
            Menu_Manager mgr = engine.menuManager;
            if (this.owner.Equals(mgr.current_menu) && mgr.closingSubmenusLevel == 2)
            {//this is moving current menu when closing subSubMenu
                startAddLenght = moveLenght;
                moveLenght = 225.0;
            }
            else if (this.owner.Equals(mgr.subMenu))
            {//this is moving subMenu when closing subSubmenu
                //startAddLenght = moveLenght;
                moveLenght = 250.0;
            }
            double multi = ((double)submenuCloseTimer.currentTime / (double)submenuCloseTimer.totalTime);
            //if (multi < 0.0) multi = 0.0;
            if (visual != null)
                visual.x = (int)(visual.orgX - startAddLenght - multi * moveLenght);
            if (visualOnHover != null)
                visualOnHover.x = (int)(visualOnHover.orgX - startAddLenght - multi * moveLenght);
            if (itemText != null)
                itemText.MoveAbs((int)(itemText.orgX - startAddLenght - startAddLenght - multi * moveLenght), (int)itemText.orgY);
        }

        /// <summary>
        /// If item is based on Obj class. IT will clear it from viewport.
        /// </summary>
        public void Close()
        {
            if(visual!=null)            visual.addedToViewport = false;
            if (visualOnClick != null)  visualOnClick.addedToViewport = false;
            if (visualOnHover != null)  visualOnHover.addedToViewport = false;
        }
    }
}
