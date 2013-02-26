using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Input;

namespace CookieMonster.CookieMonster_Objects
{
    static class InputManager
    {
        private static bool _inputLogging;

        /// <summary>
        /// if flag is set to true
        /// Key press events will be handled and appended
        /// to buffer (setting bufer to FALSE clears it contents)
        /// </summary>
        public static bool inputLogging
        {
            get { return _inputLogging; }
            set
            {
                if (value == true) //clear only on setting to true
                    buffer.Clear();//clear buffer (new input text will be entered)

                _inputLogging = value;
            }
        }


        public static List<char> buffer = new List<char>();
        internal static void KeyDown(object sender, KeyboardKeyEventArgs k)
        {
          //  if (k.Key == Key.ShiftLeft || k.Key == Key.ShiftRight)
          //      shiftON = true;
          if (k.Key == Key.BackSpace && buffer.Count>0)
                buffer.RemoveAt(buffer.Count - 1);
        }

        internal static void KeyUp(object sender, KeyboardKeyEventArgs k)
        {
          //  if (k.Key == Key.ShiftLeft || k.Key == Key.ShiftRight)
          //      shiftON = false;
            
        }

        internal static void KeyPress(object sender, OpenTK.KeyPressEventArgs p)
        {
            // Don't let Win filename illegal characters in:
            if((p.KeyChar == '\b')
             ||(p.KeyChar == '\r')
             ||(p.KeyChar == '\\')
             ||(p.KeyChar == '/')
             ||(p.KeyChar == '?')
             ||(p.KeyChar == ':')
             ||(p.KeyChar == '"')
             ||(p.KeyChar == '|') )
                return;
            // TODO: ShowMenuTip("illegal character!");
            buffer.Add(p.KeyChar);
        }
        /// <summary>
        /// Pops input buffer, simply clearing it
        /// and putting his contents to returned string
        /// </summary>
        /// <returns></returns>
        public static string getInputBuffer()
        {
            string ret = new string(buffer.ToArray());
            //buffer.Clear();
            return ret;
        }
        public static void sendTobuffer(string param)
        {
            for (int i = 0; i < param.Length; i++)
                buffer.Add(param[i]);
        }
    }
}
