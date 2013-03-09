using System;
using System.Collections.Generic;
using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    /// <summary>
    /// Inheritance class, use when class need an refence to Engine object.
    /// </summary>
    class engineReference
    {
        /// <summary>
        /// Referencja do silnika.
        /// </summary>
        static protected EngineApp.Game engine { get; private set; }
        /// <summary>
        /// Ustawia referencje do silnika
        /// </summary>
        /// <param name="e">Obiekt Silnika do ustawienia.</param>
        static public void setEngine(EngineApp.Game e) { engine = e; }
        static public EngineApp.Game getEngine() { return engine; }
    } 
}
