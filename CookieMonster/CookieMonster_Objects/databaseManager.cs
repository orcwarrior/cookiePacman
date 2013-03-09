using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace CookieMonster.CookieMonster_Objects
{
    static class databaseManager
    {
        public static bool tryToCreateAccount()
        {
            try
            {
                // This function is always called by default profile too
                // so we need to avoid reading from null!
                if (Profile.currentProfile == null || Profile.currentProfile.onlineAccountAlreadyCreated) return false;

                string hash = calculateProfileHash();
                string hlp = calculateHelper();

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
                    new DebugMsg(responseText);
                    return Profile.currentProfile.setupOnlineProfile(responseText);
                    // Now use respones text to get the echos //

                }
            }
            catch (Exception e)
            {
                engineReference.getEngine().menuManager.showAlert(Lang.cur.Blad_tworzenia_konta_online_dla_profilu);
                new DebugMsg("Account creation exception: " + e.ToString());
            }
            return false;//profile wasn't created,
        }

        private static string calculateHelper()
        {
            string hlp = System.Windows.Forms.SystemInformation.ComputerName + ";";
            hlp += System.Windows.Forms.SystemInformation.UserName + ";";
            hlp += Environment.OSVersion + ";";
            hlp += Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            return  hashHelper.EncodeTo64(hlp);

        }

        private static string calculateProfileHash()
        {
            string hash = Profile.currentProfile.name;
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
    }
}
