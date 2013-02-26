using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;
using OpenTK.Input;
using System.Drawing;

namespace CookieMonster.CookieMonster_Objects
{
    class Menu_Manager
    {
        public static Obj cursor{get; set;}
        private List<Menu> Menus_List = new List<Menu>();
        private bool menus_crossfading;
        private Menu fadingInMenu;
        private bool[] buttons_state = new bool[(int)MouseButton.LastButton + 1];
        private Menu _current_menu;
        public Menu current_menu { get { return _current_menu; }
            set
            {   //disable input loging (so if it's used in next menu it will start with blank buffer)
                InputManager.inputLogging = false;
                //if some menu was sat, clear opened flag from it.
                if (current_menu != null)
                    current_menu.closeMenu();
                _current_menu = value;
                if (_current_menu == null) return;
                
                    _current_menu.Enable();
                addMenu(_current_menu);
            }
        }
        public Menu subMenu { get; private set; }
        public Menu subSubMenu { get; private set; }

        private Timer inputIdleCheck = new Timer(Timer.eUnits.MSEC, 1 * 500, 0, true, false);
        private int inputIdleChecksCount; private const int mouseIdleRange = 10;
        private Point mouseIdleLastPos = new Point(0,0);

        private bool _canEnterIdleMode = false;
        /// <summary>
        /// If value is true, menu gui can dispear and enter "theather mode"
        /// if it's set to false, it simply can't + setting (even to the same value
        /// as the last one will reset timer and count of "idle time".
        /// </summary>
        public bool canEnterIdleMode { get { return _canEnterIdleMode;}
            set
            { 
                inputIdleChecksCount = 0;
                inputIdleCheck.restart();
                _canEnterIdleMode = value;
            }
        }
        public bool cursorRendered;
        public Menu_Manager()
        {
            
        }

        public void initCursor()
        {
            cursor = new Obj("../data/Textures/MENU/MENU_CURSOR.dds", 0.5, 0.5, Obj.align.LEFT); //bugfix: probably something wrong in quickFont causes texture load error
            cursor = new Obj("../data/Textures/MENU/MENU_CURSOR.dds", 0.5, 0.5, Obj.align.LEFT);
            cursor.layer = Layer.imgFG;
            cursor.isGUIObjectButUnscaled = true;
        }
        public void setCurrentMenu(Menu m)
        {            
            current_menu = m;
        }
        public void addMenu(Menu m)
        {
            if(getMenuByName(m.name)==null)//if there is no menu with this name on list, add it
            Menus_List.Add(m);
        }

        public void onUpdate()
        {
            if (current_menu == null) return;
            if ((Game.self.gameState & Game.game_state.Menu) == Game.game_state.Menu)
            {

                Menu_Manager.cursor.x = Game.self.Mouse.X - 0;//12
                Menu_Manager.cursor.y = Game.self.Mouse.Y - 12;
                //handle alert window:
                if (confirm != null)
                {//if there is alert wnd, update alert window only
                    confirm.Update();
                    return;
                }

                if (subMenu != null)
                    subMenu.Update();
                //Now it's handled by menuDisabled
                //if(subMenu == null || current_menu.submenuTimer.enabled || current_menu.submenuCloseTimer.enabled)
                    current_menu.Update();//update main only if there is no sub's (or there is sub-ani todo)
                if (subSubMenu != null)
                    subSubMenu.Update();
            }
            if(menus_crossfading && !current_menu.fadingOut)
            {
                menus_crossfading = false;
                setCurrentMenu(fadingInMenu);
                current_menu.fadeIn();
            }
            if (closingSubmenusLevel > 0)
            {
                if (closingSubmenusLevel == 2 && current_menu.submenuCloseTimer.enabled == false)
                { //Start II Phase:
                    subMenu = null;
                    closingSubmenusLevel--;
                    current_menu.closeSubmenu();
                    current_menu.Disable();
                }
                else if (closingSubmenusLevel == 1 && current_menu.submenuCloseTimer.enabled == false)
                {//All is done:
                    closingSubmenusLevel--;
                    current_menu.Enable();//re-enable menu items hovering
                }
            }

            checkInputIdle();
            // prepare all objects in Menu Viewport to render:
            Game.self.menuViewport.prepareMenuViewportObjects();
        }
        // NOTE: 19.09 - added keyboard idle checking
        // keypressing will disturb going into theather mode simply by handling event of keystroke
        // and reseting inputIndleChecksCount to 0
        internal void KeyPress(object sender, OpenTK.KeyPressEventArgs p)
        {
            inputIdleChecksCount = 0;
            toggleTheatherMode(0);
        }
        private void checkInputIdle()
        { 
            if (!inputIdleCheck.enabled)
            {
                inputIdleCheck.start();
                Point curCursor = new Point(Game.self.Mouse.X, Game.self.Mouse.Y);
                if ((Math.Abs(mouseIdleLastPos.X - curCursor.X) < mouseIdleRange)
                && (Math.Abs(mouseIdleLastPos.Y - curCursor.Y) < mouseIdleRange))
                {
                    inputIdleChecksCount++;
                    if (inputIdleChecksCount > 6 * 2)
                        toggleTheatherMode(1);
                }
                else
                {
                    inputIdleChecksCount = 0;
                    toggleTheatherMode(0);
                }
                mouseIdleLastPos = curCursor;
            }
        }
        private int theatherModeState = 2;//0-turning off theather mode; 1-setting To theather mode
                                          //2-Nothing happening;
        private void toggleTheatherMode(int state)
        {
            // Don't enter theather mode if flag is not set
            if((canEnterIdleMode==false)&&(state==1)) return;

            if ((Game.self.gameState & Game.game_state.Game) == Game.game_state.Game
            ||  (Game.self.gameState & Game.game_state.Menu) != Game.game_state.Menu)
                return; //INGAME MENU or not even in menu? well fuck it then.
            if (state == theatherModeState) return; //this state is already on
            if (state == 0 && theatherModeState == 2) return; //toggle off couldn't occur when theather was in just inited state;
            theatherModeState = state;
            if (state == 1)
            {
                if (current_menu != null) current_menu.fadeOut();
                if (subMenu != null) subMenu.fadeOut();
                if (subSubMenu != null) subSubMenu.fadeOut();
                cursor.setAllTexsAlpha(0);
            }
            else if (state == 0)
            {
                if (current_menu != null) current_menu.fadeIn();
                if (subMenu != null) subMenu.fadeIn();
                if (subSubMenu != null) subSubMenu.fadeIn();
                cursor.setAllTexsAlpha(255);
            }
        }

        public void onRender()
        {
            if (current_menu == null) return;
            if ((Game.self.gameState & Game.game_state.Menu) == Game.game_state.Menu)
            {
                current_menu.Render();
                if (subMenu != null && (current_menu.submenuTimer.enabled == false || subMenu.submenuTimer.enabled == true))
                    subMenu.Render();
                if (subSubMenu != null && subMenu.submenuTimer.enabled == false)
                    subSubMenu.Render();

                if (confirm != null) confirm.Render();
                cursor.prepareRender();
            }
        }
        public Menu getMenuByName(string n)
        {
            if (Menus_List.Count == 0) return null;

            int i = 0;
            while ((!Menus_List[i].menuName.Equals(n)) && (i < Menus_List.Count))
            {
                i++;
                if (i == Menus_List.Count) return null;
            }
            return Menus_List[i];
        }

        public bool getButtonState(MouseButton mb)
        {
            if (mb <= MouseButton.LastButton) return buttons_state[(int)mb];
            return false;
        }
        public void setButtonState( MouseButton mb, bool value)
        {
            if (mb <= MouseButton.LastButton) buttons_state[(int)mb] = value;

            //Avoid of getting into theather-mode:
            inputIdleChecksCount = 0;
            toggleTheatherMode(0);
        }
        /// <summary>
        /// Fade out - current menu
        /// (set current menu to new one)
        /// Fade in - new menu
        /// </summary>
        /// <param name="newMenu"></param>
        public void changeMenuWithCrossfade(Menu newMenu)
        {
            current_menu.fadeOut();
            menus_crossfading = true;
            fadingInMenu = newMenu;
        }

        public void openAsSubmenu(Menu sub)
        {
            addMenu(sub);
            if (subMenu == null)
            {
                current_menu.Disable();//disable hovering items
                current_menu.addedSubmenu();//whis function will handle "moving away" animation of menu
            }
            subMenu = sub;
            subMenu.Enable();//make sure opened menu will be enabled
        }

        internal void openAsSubSubmenu(Menu subSub)
        {
            addMenu(subSub);
            if (subSubMenu == null)
            {
                subMenu.addedSubmenu();//whis function will handle "moving away" animation of menu
                current_menu.addedSubmenu();
            }
            subSubMenu = subSub;
            subSubMenu.Enable();//make sure opened menu will be enabled
        }
        public int closingSubmenusLevel { get; private set; }
        internal void closeSubmenu(Menu sub)
        {
            if (subSubMenu != null)//subsub to close
            {
                if (sub == subMenu) { closingSubmenusLevel = 2; }
                else closingSubmenusLevel = 1;//it shouldn't happen, anyhow whatever...
                subSubMenu = null;
                subMenu.closeSubmenu();
                current_menu.closeSubmenu();
                subMenu.Disable();
            }
            else if (subMenu != null)
            {
                closingSubmenusLevel = 1;
                subMenu = null;
                current_menu.closeSubmenu();
                current_menu.Disable();
            }
        }
        /// <summary>
        /// Close all menus in one step
        /// (closing animations etc. not included)
        /// </summary>
        public void close()
        {
            if (subSubMenu != null) subSubMenu = null;
            if (subMenu != null) subMenu = null;
            current_menu = null;
        }
        /// <summary>
        /// this function shows small alert window on top of menu that will 
        /// need to get back control on anything under it
        /// </summary>
        /// <param name="msg"></param>
        private Menu alert;
        private QuickFont.QFont alertOK,alertOKHover; 
        public void showAlert(string msg)
        {
            alert = getMenuByName("ALERT");
            if (alert == null)
            {
                if (alertOK == null)
                {//init alert fonts:
                    alertOK = TextManager.newQFont("CheriPL.ttf", 35, false, new OpenTK.Graphics.Color4(210, 225, 250, 245));
                    alertOKHover = TextManager.newQFont("CheriPL.ttf", 35, false, new OpenTK.Graphics.Color4(70, 120, 255, 255));
                }
        
                Obj BG = new Obj("../data/Textures/MENU/MENU_ALERT_BG.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, false);
                BG.isGUIObjectButUnscaled = true;

                float x = Game.self.activeViewportOrAny.width / 2 - Menu.fontSmallAlt.Measure(msg).Width / 2;
                float y = Game.self.activeViewportOrAny.height * 3 / 10;
                alert = new Menu("ALERT", null);
                alert.addItem(new Menu_Item("BG", BG, null, null, null, null, null));
                alert.addItem(new Menu_Item(msg, x, y, Menu.fontSmallAlt, Menu.fontSmallAlt, Menu.fontSmallAlt, Menu_Instances.Menu_Nothing, null, null));
                alert.addItem(new Menu_Item(Lang.cur.OK, x, y + 300f, alertOK, alertOKHover, Menu.font_Click, closeAlert));
            }
        }
        public void closeAlert()
        {
            alert = null;
        }

        private Menu confirm;
        private QuickFont.QFont confirmYES, confirmYESHover;
        private QuickFont.QFont confirmNO, confirmNOHover;
        public void showConfirm(string msg,Menu_Item.mouseEvt yesClick,Menu_Item.mouseEvt noClick)
        {
            confirm = getMenuByName("CONFIRM");
            if (confirm == null)
            {
                if (confirmYES == null)
                {//init confirm fonts:
                    confirmYESHover = TextManager.newQFont("CheriPL.ttf", 35, false, new OpenTK.Graphics.Color4(163, 250, 164, 245));
                    confirmYES      = TextManager.newQFont("CheriPL.ttf", 35, false, new OpenTK.Graphics.Color4(210, 215, 230, 245));
                    confirmNO       = TextManager.newQFont("CheriPL.ttf", 35, false, new OpenTK.Graphics.Color4(210, 215, 230, 245));
                    confirmNOHover  = TextManager.newQFont("CheriPL.ttf", 35, false, new OpenTK.Graphics.Color4(255, 80, 80, 245));
                }

                Obj BG = new Obj("../data/Textures/MENU/MENU_CONFIRM_BG.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, false);
                BG.isGUIObjectButUnscaled = true;

                float x = Game.self.activeViewportOrAny.width / 2 - Menu.fontSmallAlt.Measure(msg).Width / 2;
                float y = Game.self.activeViewportOrAny.height * 3 / 10;
                confirm = new Menu("CONFIRM", null);
                confirm.addItem(new Menu_Item("BG", BG, null, null, null, null, null));
                confirm.addItem(new Menu_Item(msg, x, y, Menu.fontSmallAlt, Menu.fontSmallAlt, Menu.fontSmallAlt, Menu_Instances.Menu_Nothing, null, null));

                x = Game.self.activeViewportOrAny.width / 2;
                confirm.addItem(new Menu_Item(Lang.cur.Yes, x + 70, y + 300f, confirmYES, confirmYESHover, Menu.font_Click, yesClick));
                confirm.addItem(new Menu_Item(Lang.cur.No, x - 140, y + 300f, confirmNO, confirmNOHover, Menu.font_Click, noClick));
            }
        }
        public void closeConfirm()
        {
            confirm = null;
        }
    }
}