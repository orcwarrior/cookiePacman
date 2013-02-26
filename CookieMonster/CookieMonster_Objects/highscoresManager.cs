using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Data;

namespace CookieMonster.CookieMonster_Objects
{
    class highscoresManager
    {
        public static void addNewHighscore(GameManager gm)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://cookie.site50.net/addHighScore.php?name=" + Profile.currentProfile.name + "&score=" + gm.statistics.points);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            System.IO.StreamReader input = new System.IO.StreamReader(response.GetResponseStream());

            DataSet dsTest = new DataSet();
        }
    }
}
