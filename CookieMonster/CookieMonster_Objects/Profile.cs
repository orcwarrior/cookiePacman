﻿using System;
using System.Collections.Generic;
using System.Text;
using EngineApp;
using QuickFont;
using OpenTK.Graphics;
using System.Net;
using System.IO;

namespace CookieMonster.CookieMonster_Objects
{
    [Serializable]
    class Profile : engineReference
    {
        static private string profileExt = ".CMprofile";
        static private Profile hlpProfile;
        static private Profile _current;
        static public string profileNameAndIDSep = "_%_";
        static public Profile currentProfile { 
            get {
                if (_current == null) return hlpProfile;
                else return _current;
                }
            private set { _current = value; value.applyCurrentProfile(); }
        }
        static public List<Profile> profilesList { get; private set; }
        static public int menuSelectedProfile { get; private set; }


        public string name { get; private set; }
        public Configuration config { get; private set; }
        public Savegame save { get; private set; }
        
        // Stuff on server database:

        // true when user connected to database and generated new user in a table
        public bool onlineAccount_AlreadyCreated  { get; private set; }
        public int onlineAccount_ID               { get; private set; }
        public bool onlineAccount_CreationDisabled{ get; private set; }
        
        static Profile()
        {
            profilesList = new List<Profile>();
            menuSelectedProfile = -1;
        }
        /// <summary>
        /// loads profile from hard drive
        /// + any other initialization stuff
        /// </summary>
        static public void Initialize()
        {
            hlpProfile = new Profile("");

            string[] profiles = System.IO.Directory.GetFiles("../data/User Profiles/", "*" + profileExt);
            for(int i=0;i<profiles.Length;i++)
                decryptFromFile(profiles[i]);

            //default selected profile:
            if (profilesList.Count > 0)
                menuSelectedProfile = 1; //first at list
            else
                menuSelectedProfile = 0;//(create profile)
        }
        /// <summary>
        /// Creates new profille, adds it to static profilesList
        /// </summary>
        /// <param name="n">name of profile</param>
        public Profile(String n)
        {
            name = n;
            config = new Configuration();
            if (n != "")//blank - helper profile
                profilesList.Add(this);

        }
        /// <summary>
        /// Auto-save game state
        /// </summary>
        public void autoSave()
        {
            if(save == null)
                save = new Savegame();
            save.Update();
            //update profile file:
            encryptToFile();
        }
        //TODO: apply resolution 
        public void applyCurrentProfile()
        {
            engine.SoundMan.recalculateMusic();
            engine.SoundMan.recalculateSFX();

            //if online profile wasn't created, try to create one:
            if (!onlineAccount_AlreadyCreated)
                this.tryToCreateAccount();
        }


        // ---------
        // Profile-Menu stuff, TODO: Exctract to another class.
        #region Profile-Menu
        public static void Menu_GenerateProfileMenu()
        {
            //remove items (those used for creating/selecting profile)
            Menu profile = engine.menuManager.getMenuByName("MENU_PROFILE");
            //profile.removeMenuItem(profile.getItemByName("ArrL"));
            //profile.removeMenuItem(profile.getItemByName("ArrR"));

            if (profile.getItem(0) == null) //no items created
            {
                generateStaticObjs(profile);

                generateArrowLeft(profile);
                generateArrowRight(profile);
                generateProfileName(profile);
                generateOKButton(profile);
                genereateDeleteButton(profile);
            }
            else
            { // Don't fully generate menu, just refresh some dynamic objects.
                profile.removeMenuItem(profile.getItemByName("ArrL"));
                generateArrowLeft(profile);
                profile.removeMenuItem(profile.getItemByName("ArrR"));
                generateArrowRight(profile);
                
                profile.removeMenuItem(profile.getItemByName(Lang.cur.Usun_Profil));
                genereateDeleteButton(profile);

                profile.removeMenuItem(profile.getItemByName(Lang.cur.OK));
                generateOKButton(profile);
                refreshProfileName(profile);
            }
        }

        private static void generateStaticObjs(Menu profile)
        {
            Obj profileLoginFrame = new Obj("../data/Textures/MENU/MENU_PROFILE.dds", 0.5, 0.43, Obj.align.CENTER_BOTH, false);
            profileLoginFrame.ignoreCameraOffset = true;
            profile.addItem(new Menu_Item("BG", profileLoginFrame, null, null, null, null, null));

            QFont heading1 = TextManager.newQFont("KOMIKAX.ttf", 22, true, new OpenTK.Graphics.Color4(0, 112, 186, 255));

            float x = engine.Width  / 2 - heading1.Measure(Lang.cur.Wybierz_Profil).Width / 2;
            float y = engine.Height * 4 / 10 + 10f;
            profile.addItem(new Menu_Item(Lang.cur.Wybierz_Profil, x, y, heading1));
        }

        private static void genereateDeleteButton(Menu profile)
        {
            QFont DeleteProf = TextManager.newQFont("Rumpelstiltskin.ttf", 32, false, new Color4(0, 112, 186, 255));
            float x = engine.Width / 2 - DeleteProf.Measure(Lang.cur.Usun_Profil).Width / 2 - 120f;
            float y = engine.Height / 2 + 75f; 
                if (menuSelectedProfile > 0)
                {//some profile is selected, can delete it:
                    //DeleteProf = TextManager.newQFont("Rumpelstiltskin.ttf", 32, false, new Color4(0, 112, 186, 255));
                    QFont DeleteProfhover = TextManager.newQFont("Rumpelstiltskin.ttf", 32, true, new Color4(0, 112, 186, 255));
                    profile.addItem(new Menu_Item(Lang.cur.Usun_Profil, x, y, DeleteProf, DeleteProfhover, DeleteProf, Profile.Menu_DeleteProfile));
                }
                else
                {//can't delete profile
                    DeleteProf = TextManager.newQFont("Rumpelstiltskin.ttf", 32, false, new Color4(120, 120, 120, 255));                   
                    profile.addItem(new Menu_Item(Lang.cur.Usun_Profil, x, y, DeleteProf));
                }
        }
        private static void generateOKButton(Menu profile)
        {
            QFont OK = TextManager.newQFont("Rumpelstiltskin.ttf", 32, false, new Color4(40, 100, 133, 255));
            
            string tmp = InputManager.getInputBuffer();

            float x = engine.Width  / 2 - OK.Measure(Lang.cur.OK).Width / 2 + 200f;
            float y = engine.Height / 2 + 75f;
            if (menuSelectedProfile > 0 || (menuSelectedProfile == 0 && tmp.Length > 0 && profile.inputItem.value != profile.inputItem.defaultValue))
            {//OK Active:
                QFont OKhover = TextManager.newQFont("Rumpelstiltskin.ttf", 32, true, new Color4(0, 112, 186, 255));
                profile.addItem(new Menu_Item(Lang.cur.OK, x, y, OK, OKhover, OK, Profile.Menu_CreateProfile));
            }
            else
            {//OK Inactive:
                OK = TextManager.newQFont("Rumpelstiltskin.ttf", 32, false, new Color4(120, 120, 120, 255));
                profile.addItem(new Menu_Item(Lang.cur.OK, x, y, OK));
            }
        }

        private static void generateProfileName(Menu profile)
        {
            //Profile name (or input) text:

            QFont profileName = TextManager.newQFont("Rumpelstiltskin.ttf", 24, false, new Color4(220, 220, 220, 255));
            QFont profileName_hover = TextManager.newQFont("Rumpelstiltskin.ttf", 24, false, new Color4(255, 255, 255, 255));

            float y = engine.Height / 2 - 18f;

            float x;
            if (menuSelectedProfile == 0)//create profile:
            {
                x = engine.Width / 2 - profileName.Measure(Lang.cur.stworz_nowy).Width / 2;
                profile.addInputItem(new Menu_Input_Item(x, y, profileName, profileName_hover, null, Lang.cur.stworz_nowy, false));
            }
            else if (menuSelectedProfile > 0)
            {
                string pName = profilesList[menuSelectedProfile - 1].name;
                x = engine.Width / 2 - profileName.Measure(pName).Width / 2;
                profile.addInputItem(new Menu_Input_Item(x, y,profileName,null,pName,true));
            }
        }
        private static void refreshProfileName(Menu profile)
        {
            //Profile name (or input) text:
            if (menuSelectedProfile == 0)//create profile:
            {
                profile.inputItem.value = Lang.cur.stworz_nowy;
            }
            else if (menuSelectedProfile > 0)
            {
                string pName = profilesList[menuSelectedProfile - 1].name;
                profile.inputItem.value = pName;
            }
        }

        private static void generateArrowRight(Menu profile)
        {
            Obj arrRight;
            if (menuSelectedProfile < profilesList.Count)
            {
                arrRight = new Obj("../data/Textures/MENU/MENU_ARROW_RIGHT.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, false);
                profile.addItem(new Menu_Item("ArrR", arrRight, null, null, null, null, Menu_RightProfile));
            }
            else
            {
                arrRight = new Obj("../data/Textures/MENU/MENU_ARROW_RIGHT_DISABLED.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, false);
                profile.addItem(new Menu_Item("ArrR", arrRight, null, null, null, null, null));
            }

            arrRight.ignoreCameraOffset = true;
            arrRight.x += 125;
        }
        public static void Menu_RightProfile()
        {
            Menu profile = engine.menuManager.getMenuByName("MENU_PROFILE");
            profile.clearMenuItems();
            menuSelectedProfile++;
            Menu_GenerateProfileMenu();
        }

        private static void generateArrowLeft(Menu profile)
        {
            Obj arrLeft;
            Menu_Item itm = profile.getItemByName("ArrL");
            if (menuSelectedProfile > 0)
            {
                arrLeft = new Obj("../data/Textures/MENU/MENU_ARROW_LEFT.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, false);
                profile.addItem(new Menu_Item("ArrL", arrLeft, null, null, null, null, Menu_LeftProfile));
            }
            else
            {
                arrLeft = new Obj("../data/Textures/MENU/MENU_ARROW_LEFT_DISABLED.dds", 0.5, 0.5, Obj.align.CENTER_BOTH, false);
                profile.addItem(new Menu_Item("ArrL", arrLeft, null, null, null, null, null));
            }
            arrLeft.ignoreCameraOffset = true;
            arrLeft.x -= 125;
        }
        public static void Menu_LeftProfile()
        {
            Menu profile = engine.menuManager.getMenuByName("MENU_PROFILE");
            profile.clearMenuItems();
            menuSelectedProfile--;
            Menu_GenerateProfileMenu();
        }
        public static void Profile_KeyStroke(object sender, OpenTK.Input.KeyboardKeyEventArgs p)
        {
            if (engine.menuManager == null) return;
            Menu profile = engine.menuManager.getMenuByName("MENU_PROFILE");
            if (engine.menuManager.current_menu != profile) return; //current menu isnot profile, break func
            if (InputManager.inputLogging == false) return; //no input loging, break

            profile.removeMenuItem(profile.getItemByName("OK"));
            generateOKButton(profile);
        }
        #endregion

        public static void Menu_CreateProfile()
        {
            if (menuSelectedProfile == 0)
            { //new profile was created:
                currentProfile = new Profile(InputManager.getInputBuffer());
                currentProfile.encryptToFile();
            }
            else
            {
                currentProfile = profilesList[menuSelectedProfile - 1];
            }
            //goto main menu:
            engine.menuManager.current_menu = new Menu("MENU_MAIN", Menu_Instances.Main_OnLoad, Menu_Manager.cursor);

            //try to create online account for profile:
            if(!currentProfile.onlineAccount_AlreadyCreated)
             Profile.currentProfile.tryToCreateAccount();
        }
        public static void Menu_DeleteProfile()
        {
            //TODO: Usuniecie profilu z bazy danych
            string name = profilesList[menuSelectedProfile - 1].name;
            System.IO.File.Delete("../data/User Profiles/"+name+profileExt);
            profilesList.RemoveAt(menuSelectedProfile - 1);
            engine.menuManager.showAlert(Lang.cur.Usunieto_Profil_n+name);
            menuSelectedProfile--;
            //redraw choose profile menu:
            Menu_GenerateProfileMenu();
        }

        // ------------------
        // NON-STATIC METHODS

        public void encryptToFile()
        {
            //TODO: encrypt proile id
            string result = "";
            result += config.encrypt();
            if (save != null)
            {
                result += "%";//%- end of block
                result += save.encrypt();
            }
            result += "%"+(onlineAccount_ID * 3 - 17).ToString();
            //replace chars, to add some more confussion:
            //0 -> +
            //2 -> G
            //3 -> h
            //5 -> 2
            //8 -> _
            //C -> 0
            //F -> m
            result = result.Replace("0", "+");
            result = result.Replace("2", "G");
            result = result.Replace("3", "h");
            result = result.Replace("5", "2");
            result = result.Replace("8", "_");
            result = result.Replace("C", "0");
            result = result.Replace("F", "m");
            
            System.IO.File.WriteAllText("../data/User Profiles/" + name + profileExt, result);

        }
        private static void decryptFromFile(string filepath)
        {
            string name = "???";
            try
            {
                string result = System.IO.File.ReadAllText(filepath);
                Profile p;
                int b = filepath.LastIndexOf("/") + 1;
                name = filepath.Substring(b, filepath.LastIndexOf(profileExt) - b);
                p = new Profile(name);

                result = result.Replace("2", "5");
                result = result.Replace("G", "2");
                result = result.Replace("h", "3");
                result = result.Replace("_", "8");
                result = result.Replace("0", "C");
                result = result.Replace("+", "0");
                result = result.Replace("m", "F");
                string conf = result.Substring(0, result.IndexOf("%"));
                // Dekodowanie konfiguracji profilu:
                p.config.decrypt(conf);

                // Dekodowanie zapisu gry:
                if (result.IndexOf("%") != result.LastIndexOf("%"))
                { //if that's true, save is present:
                    string save = result.Substring(result.IndexOf("%") + 1);
                    save = save.Substring(0, save.LastIndexOf("%"));
                    if (save.Length > 0)
                    {
                        p.save = new Savegame();
                        p.save.decrypt(save);
                    }
                }
                // Dekodowanie internetowego id profilu:
                string ProfileID = result.Substring(result.LastIndexOf("%") + 1);
                if (ProfileID.Length > 0)
                {
                    int id = int.Parse(ProfileID);
                    id = (id+17)/3;
                    p.onlineAccount_ID = id;
                    if (p.onlineAccount_ID > 0)
                        p.onlineAccount_AlreadyCreated = true;
                    else
                        p.onlineAccount_AlreadyCreated = false;
                }
            }
            catch (Exception e)
            {
                engine.menuManager.showAlert(Lang.cur.Blad_Odczytu_profilu_o_nazwie + name + "\n"+Lang.cur.Komunikat_Bledu+"\n\""+e.Message+"\"\n"+Lang.cur.Usuwam_Powyzszy_Profil);
                System.IO.File.Delete(filepath);
                //remove last created profile:
                profilesList.RemoveAt(profilesList.Count - 1);
            }
        }


        // ----------------
        // Online Profile/Database stuff
        #region Online Profile/Database
        /// <summary>
        /// Method tries to create online account for player.
        /// It calculate proper hash and send request to php file.
        /// When php validates hash, account is created properly.
        /// </summary>
        /// <returns></returns>
        public bool tryToCreateAccount()
        {
            if (onlineAccount_CreationDisabled) return false;

            try
            {
                // This function is always called by default profile too
                // so we need to avoid reading from null!
                if (Profile.currentProfile == null || Profile.currentProfile.onlineAccount_AlreadyCreated) return false;

                string hash = _calculateProfileHash();
                string hlp = _calculateHelper();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://cookie.site50.net/createAccount.php?name=" + Profile.currentProfile.name + "&h=" + hash + "&hlp=" + hlp);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //read the response stream 
                    String responseText;
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        responseText = sr.ReadLine();
                    }
                    // Now use respones text to get the echos //
                    new DebugMsg(responseText);
                    return this.setupOnlineProfile(responseText);

                }
            }
            catch (Exception e)
            {
                engineReference.getEngine().menuManager.showAlert(Lang.cur.Blad_tworzenia_konta_online_dla_profilu);
                new DebugMsg("Account creation exception: " + e.ToString());
            }
            return false;//profile wasn't created,
        }
        /// <summary>
        /// Przypisuje ID do gracza (zwrocone przez responseText).
        /// </summary>
        /// <param name="responseText">zwrocona odp. przez zapytanie php</param>
        /// <returns>true jeśli id jest poprawne</returns>
        public bool setupOnlineProfile(string responseText)
        {
            int id = int.Parse(responseText.Split(';')[0]);
            if (id > 0)
            {
                onlineAccount_ID = id;
                this.onlineAccount_AlreadyCreated = true;
                return true;
            }
            else
            {
                engineReference.getEngine().menuManager.showAlert(Lang.cur.Blad_tworzenia_konta_online_dla_profilu);
                onlineAccount_ID = -1;
                return false;
            }
        }

        /// <summary>
        /// Generates authentication key of this profile
        /// (based on id and profile name)
        /// </summary>
        /// <returns></returns>
        public string getProfileAuthKey()
        {
            return _calculateProfileHash() + profileNameAndIDSep + _calculateIDKey();
        }

        public void openOnlineAccout()
        {
            // minimalize game:
            engineReference.getEngine().WindowState = OpenTK.WindowState.Minimized;

            // run generated link:
            System.Diagnostics.Process.Start("http://cookie.site50.net/login.php?auth=" + this.getProfileAuthKey());
        }

        /// <summary>
        /// Function calculate (and code) help user infos.
        /// [Stored in main users table]
        /// </summary>
        /// <returns></returns>
        private static string _calculateHelper()
        {
            string hlp = System.Windows.Forms.SystemInformation.ComputerName + ";";
            hlp += System.Windows.Forms.SystemInformation.UserName + ";";
            hlp += Environment.OSVersion + ";";
            hlp += System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            return hashHelper.EncodeTo64(hlp);
        }
        /// <summary>
        /// Creates profile hash-key. With some text manipulations
        /// to avoid breaking authenticatin key.
        /// </summary>
        /// <returns></returns>
        private string _calculateProfileHash()
        {
            string hash = this.name;
            int len = hash.Length;
            hash = hash.Insert(len - 2, hash[len - 1] + ".");
            if (len % 2 == 0)
                hash = hash.Insert(0, "_x");
            else
                hash = hash.Insert(1, "_z");
            hash = hash.Remove(0, 1);
            hash = hashHelper.CalculateMD5(hash);
            return hash;
        }
#endregion







        /// <summary>
        /// Creates ID hash-key. With some text manipulations
        /// </summary>
        /// <returns></returns>
        private string _calculateIDKey()
        {
            return hashHelper.EncodeTo64( ((onlineAccount_ID * onlineAccount_ID) + 196).ToString());
        }


    }
}
