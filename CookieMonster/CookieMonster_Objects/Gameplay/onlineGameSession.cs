using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Klasa
    /// </summary>
    class onlineGameSession : engineReference
    {
        /// <summary>Nazwa sesji gry zgodna z nazwą przechowywaną w Bazie danych.</summary>
        public uint gameSessionID { get; private set; }
        
        public onlineGameSession(bool newgame)
        {
            if (!newgame && Profile.currentProfile.save != null)
                gameSessionID = Profile.currentProfile.save.player.gameID;
            else
                gameSessionID = 0;
        }
        /// <summary>
        /// Method tries to connect to server (if account is still not created, it will try to do this
        /// too) and request for a new game id.
        /// </summary>
        public void tryToCreateOnlineGameSessionID()
        {
            if (Profile.currentProfile.onlineAccount_AlreadyCreated || Profile.currentProfile.tryToCreateAccount())
            {
                // Create game session ID if still not created:
                if (gameSessionID == 0)
                {
                    gameSessionID = _createGameSessionID();
                }
            }
        }
        public void postLevelScoresToDatabase()
        {
                // We will have another try to connect to our database(create an account)
                // (If one is still not created)
            if (Profile.currentProfile.onlineAccount_AlreadyCreated || Profile.currentProfile.tryToCreateAccount())
            {

                Savegame sav = Profile.currentProfile.save;
                mapSave lvl = sav.maps[sav.maps.Count - 1];
                // sent key:
                // hash;userID;gameID;
                // totalScore;totalGameTime;playerLevel;playerExp;playerLives;playerMaxLives;playerlTalentPts;playerIceBoltLvl;playerBoostLvl;playerSpeed;mapsCount;difficultLvl
                // levelID;score;levelTime
                string sep = ";";

                // klucz:
                string sentKey = Profile.currentProfile.getProfileAuthKey();//generateGameSessionHash();
                // id gracza:
                sentKey += sep + Profile.currentProfile.onlineAccount_ID;
                // dane dotyczące sesji gry:
                sentKey += sep + gameSessionID + sep + sav.player.score + sep + sav.player.gameDuration + sep + ((sav.player.level == sav.player.mapsCount)?"1":"0") + sep + sav.player.level;
                sentKey += sep + sav.player.exp + sep + sav.player.lives + sep + sav.player.maxLives + sep + sav.player.talentPts;
                sentKey += sep + sav.player.iceBoltLVL + sep + sav.player.speedBoostLVL + sep + sav.player.movementSpeed;
                sentKey += sep + sav.player.mapsCount + sep + sav.player.difficultLevel;
                // dane dotyczące ostatniego poziomu gry:
                sentKey += sep + lvl.level + sep + lvl.lvlScore + sep + lvl.lvlDuration;

                // Utworzenie wysokiego wyniku dla konkretnego poziomu:
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://cookie.site50.net/createMapScore.php?key=" + sentKey);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //read the response stream 
                    String responseText;
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        responseText = sr.ReadToEnd();
                    }
                    // Now use respones text to get the echos //
                    new DebugMsg(responseText);
                }
            }
        }

        private uint _createGameSessionID()
        {
            try
            {
                string hash = Profile.currentProfile.getProfileAuthKey();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://cookie.site50.net/createGameSession.php?key=" + hash);
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
                    return uint.Parse(responseText);
                }
            }
            catch (Exception e)
            {
                engineReference.getEngine().menuManager.showAlert(Lang.cur.Blad_tworzenia_konta_online_dla_profilu);
                new DebugMsg("Game Session creation exception: " + e.ToString());
            }
            return 0;
        }
        private string generateGameSessionHash()
        {
            string hash = Profile.currentProfile.getProfileAuthKey();
            hash += gameSessionID + "imba";
            hash = hashHelper.CalculateSHA1(hash);
            return hash;
        }
    }
}
