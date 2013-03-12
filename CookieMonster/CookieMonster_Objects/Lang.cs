using System;
using System.Collections.Generic;
using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class Lang
    {
        public enum language { PL, EN }
        public static Lang cur;

        #region the whole lang
        public string Nie, Tak;
        public string Wybierz_Profil;
        public string Usun_Profil;
        public string OK;
        public string Yes;
        public string No;
        public string areUSure;
        public string endGameSessionAllChangesWillBeLost;
        public string stworz_nowy;
        public string Usunieto_Profil_n;
        public string Blad_Odczytu_profilu_o_nazwie;
        public string Komunikat_Bledu;
        public string Usuwam_Powyzszy_Profil;
        public string Dopalacze;
        public string umiejetnosci;
        public string punkty_0;
        public string punkty;
        public string Latwy;
        public string Normalny;
        public string Trudny;
        public string Hardcore;
        public string Wroc;
        public string Ile_Poziomow_w_GrzeDK;
        public string Tunel_NrDOT;
        public string CzasDDSpace;
        public string PoziomDDSpace;
        public string PktDDSpace;
        public string PktDTalentuDDSpace;
        public string ZyciaDDSpace;
        public string WczytajEXCL;
        public string Grafika;
        public string Dzwiek;
        public string Efekty;
        public string Muzyka;
        public string Rozdzieczlosc;
        public string Renderuj_Sciezki;
        public string POZIOM_FIRST;
        public string POZIOM_REST;
        public string PKT_TALENTU__P;
        public string PKT_TALENTU__T;
        public string KT_ALENTU;
        public string ATRYBUTY__A;
        public string TRYBUTY;
        public string SZYBKOSC;
        public string MAXDOT_ILDOT_ZYC;
        public string UMIEJETNOSCI__U;
        public string MIEJETNOSCI;
        public string TRAMPY_MOCY;
        public string LODOWY_POCISK;
        public string Inicjuje_Game_Managera;
        public string Inicjuje_system_kolizji;
        public string wczytuje_poziom;
        public string tworze_GUI;
        public string alokuje_pamiec_tekstur;
        public string inicjalizuje_obiekty_poziomu;
        public string inicjuje_odtwarzacz_muzyki;
        public string Ustaw_Rozdzielczosc;
        public string gra_opracowane_przez;
        public string creditsGlProgramista;
        public string creditsMainGfxAni;
        public string creditsTesters;
        public string creditsFreesndUses;
        public string creditsMusic;
        public string creditsMusic_dubsective_1;
        public string creditsMusic_dubsective_2;
        public string creditsMusic_dubsective_3;
        public string creditsMusic_rezaloot_1;
        public string creditsMusic_rezaloot_2;
        public string creditsMusic_rezaloot_3;
        public string creditsMusic_kavalsky_1;
        public string creditsMusic_kavalsky_2;
        public string creditsMusic_kavalsky_3;
        public string creditsPodziekowania;
        public string creditsPodziekowania_2;
        public string creditsPodziekowania_3;
        public string creditsPodziekowania_4;
        public string creditsPodziekowania_5;
        public string creditsPodziekowania_6;
        public string creditsPodziekowania_7;
        public string creditsPodziekowania_8;
        public string tip1_title;
        public string tip1_contents;
        public string tip2_title;
        public string tip2_contents;
        public string Blad_tworzenia_konta_online_dla_profilu;
        #endregion

        public Lang(language l)
        {
            if (l == language.PL)
            {
                //profile:
                Wybierz_Profil                  = "Wybierz Profil:";
                Usun_Profil                     = "Usuń Profil";
                OK                              = "OK";
                Yes                             = "Tak";
                No                              = "Nie";
                areUSure                        = "Czy napewno?";
                endGameSessionAllChangesWillBeLost = "Czy napewno chcesz zakończyć grę?\nPostępy na obecnym poziomie\nnie zostaną zapisane!";
                stworz_nowy                     = "(swtorz nowy)";
                Usunieto_Profil_n               = "Usunięto profil:\n";
                Blad_Odczytu_profilu_o_nazwie   = "Błąd oczytu profilu o nazwie: ";
                Komunikat_Bledu                 = "Komunikat błędu:";
                Usuwam_Powyzszy_Profil          = "Usuwam powyższy profil :(";
                Blad_tworzenia_konta_online_dla_profilu = "Błąd podczas tworzenia konta\nonline dla twojego profilu\nsprawdź połączenie internowe oraz\nustawienia firewall'a. Kolejne próby\nutworzenia konta online\nbęda podejmowane później.";
                //GUI:
                Dopalacze       = "dopalacze";
                umiejetnosci    = "umiejętności";
                punkty_0        = "punkty: 0";
                punkty          = "punkty: ";
                //Menu_Instances:
                Latwy                 = "Łatwy";
                Normalny              = "Normalny";
                Trudny                = "Trudny";
                Hardcore              = "Hardcore";
                Wroc                  = "Wróć";
                Ile_Poziomow_w_GrzeDK = "Ile Poziomów w grze?";
                Tunel_NrDOT           = "Tunel nr.";
                CzasDDSpace           = "Czas: ";
                PoziomDDSpace         = "Poziom: ";
                PktDDSpace            = "Pkt: ";
                PktDTalentuDDSpace    = "Pkt. Talentu: ";
                ZyciaDDSpace          = "Życia: ";
                WczytajEXCL           = "Wczytaj!";
                Grafika               = "Grafika";
                Dzwiek                = "Dźwiek";
                Efekty                = "Efekty";
                Muzyka                = "Muzyka";
                Rozdzieczlosc         = "Rozdzielczość";
                Ustaw_Rozdzielczosc   = "(Ustaw rozdzielczość)";
                Renderuj_Sciezki      = "Renderuj Ścieżki";
                //Status screen:
                POZIOM_FIRST    = "P";
                POZIOM_REST     = "OZIOM";
                PKT_TALENTU__P  = "P";
                PKT_TALENTU__T  = "T";
                KT_ALENTU       = "KT.  ALENTU";
                ATRYBUTY__A     = "A";
                TRYBUTY         = "TRYBUTY";
                SZYBKOSC        = "SZYBKOŚĆ";
                MAXDOT_ILDOT_ZYC= "MAX. IL. ŻYĆ";
                UMIEJETNOSCI__U = "U";
                MIEJETNOSCI     = "MIEJĘTNOŚCI";
                TRAMPY_MOCY     = "TRAMPY MOCY";
                LODOWY_POCISK   = "LODOWY POCISK";
                //Loading screen:
                Inicjuje_Game_Managera       = "inicjuje Game Managera";
                Inicjuje_system_kolizji      = "inicjuje System Kolizji";
                wczytuje_poziom              = "wczytuje poziom";
                tworze_GUI                   = "tworze GUI";
                alokuje_pamiec_tekstur       = "alokuje pamięć tekstur";
                inicjalizuje_obiekty_poziomu = "inicjalizuje obiekty poziomu";
                inicjuje_odtwarzacz_muzyki   = "inicjuje odtwarzacz muzyki";

                //Credits:
                gra_opracowane_przez        = "GRA OPRACOWANA PRZEZ";
                creditsGlProgramista        = "GŁÓWNY PROGRAMISTA";
                creditsMainGfxAni           = "GŁÓWNY GRAFIK/ANIMATOR";
                creditsTesters              = "TESTERZY";
                creditsFreesndUses          = "WYKORZYSTANE DŹWIĘKI Z WWW.FREESOUND.ORG";
                creditsMusic                = "MUZYKA";
                creditsMusic_dubsective_1 = "Utwory \"ZEN\" oraz \"BEAMING\"";
                creditsMusic_dubsective_2 = "Skomponowane przez DUBSECTIVE";
                creditsMusic_dubsective_3 = "soundcloud.com/dubsective";

                creditsMusic_rezaloot_1 = "Utwory \"110 BT POLES\" oraz \"LONEY TUNE\"";
                creditsMusic_rezaloot_2 = "Skomponowane przez REZALOOT";
                creditsMusic_rezaloot_3 = "soundcloud.com/rezaloot";

                creditsMusic_kavalsky_1 = "Utwór \"ROBO BOOTY\"";
                creditsMusic_kavalsky_2 = "Skomponowany przez KAVALSKY";
                creditsMusic_kavalsky_3 = "soundcloud.com/kavalsky";

                creditsPodziekowania   = "PODZIĘKOWANIA";
                creditsPodziekowania_2 = "Chciałbym podziękować wszystkim";
                creditsPodziekowania_3 = "którzy powiedzieli choćby jedno pozytywne słowo";
                creditsPodziekowania_4 = "na temat tego projektu w fazie jego rozwijania.";
                creditsPodziekowania_5 = "Przy tworzeniu czegoś co wymaga tak dużego poświęcenia.";
                creditsPodziekowania_6 = "Opinie innych są bardzo istotne oraz motywujące";
                creditsPodziekowania_7 = "do dalszej pracy i skończenia projektu :-)";
                creditsPodziekowania_8 = "DK";

                tip1_title             = "Pierwszy przeciwnik";
                tip1_contents          = "Witaj w rozgrywcę! Twoim pierwszym przeciwnikiem będzie DUCH, musisz go unikać, bo chce cię pożreć! Zbierz niebieskie pigułki, a role się odwrócą.";
              
                tip2_title             = "Pierwszy poziom";
                tip2_contents          = "Gratulacje!\nWłasnie osiągnąłeś pierwszy poziom!\nZa każdy poziom otrzymujesz punkty talentu za które możesz rozwijać umiejętności swojego Ciasteczkowego Potwora! Kliknij klawisz ESCAPE aby otworzyć Menu Postaci.";
            }
            else if(l == language.EN)
            {
                //profile:
                Wybierz_Profil = "Choose Profile:";
                Usun_Profil = "Delete Profile";
                OK = "OK";
                Yes = "Yes";
                No = "No";
                areUSure = "Are you sure?";
                endGameSessionAllChangesWillBeLost = "Are you sure you want to quit the game?\nAll progress at the current level\nwill be lost!";
                stworz_nowy = "(Create new)";
                Usunieto_Profil_n = "Profile removed:\n";
                Blad_Odczytu_profilu_o_nazwie = "Error reading profile: ";
                Komunikat_Bledu = "Error Message:";
                Usuwam_Powyzszy_Profil = "I must delete this profile :(";
                //GUI:
                Dopalacze = "boosters";
                umiejetnosci = "   skills";
                punkty_0 = "points: 0";
                punkty = "points: ";
                //Menu_Instances:
                Latwy = "Easy";
                Normalny = "Normal";
                Trudny = "Hard";
                Hardcore = "Hardcore";
                Wroc = "Back";
                Ile_Poziomow_w_GrzeDK = "Levels to beat:";
                Tunel_NrDOT = "Tunnel nr.";
                CzasDDSpace = "Time: ";
                PoziomDDSpace = "Difficult Level: ";
                PktDDSpace = "Pts: ";
                PktDTalentuDDSpace = "Talent Pts: ";
                ZyciaDDSpace = "Lives: ";
                WczytajEXCL = "Load!";
                Grafika = "Graphics";
                Dzwiek = "Sound";
                Efekty = "Effects";
                Muzyka = "Music";
                Rozdzieczlosc = "Resolution";
                Ustaw_Rozdzielczosc = "(apply resolutiom)";
                Renderuj_Sciezki = "Render Paths";
                //Status screen:
                POZIOM_FIRST = "L";
                POZIOM_REST = "EVEL";
                PKT_TALENTU__P = "T";
                PKT_TALENTU__T = "";
                KT_ALENTU = "ALENT PTS.";
                ATRYBUTY__A = "A";
                TRYBUTY = "TRIBUTES";
                SZYBKOSC = "SPEED";
                MAXDOT_ILDOT_ZYC = "MAX. LIVES";
                UMIEJETNOSCI__U = "S";
                MIEJETNOSCI = "KILLS";
                TRAMPY_MOCY = "CHUCKS OF POWER";
                LODOWY_POCISK = "ICE BOLT";
                //Loading screen:
                Inicjuje_Game_Managera = "initializing Game Managera";
                Inicjuje_system_kolizji = "initializing Collide System";
                wczytuje_poziom = "loading level";
                tworze_GUI = "creating GUI";
                alokuje_pamiec_tekstur = "allocating textures memory";
                inicjalizuje_obiekty_poziomu = "initializing level objects";
                inicjuje_odtwarzacz_muzyki = "initializing music player";
                //Credits
                gra_opracowane_przez        = "GAME DEVELOPED BY";
                creditsGlProgramista        = "MAIN PROGRAMMER";
                creditsMainGfxAni           = "LEAD GRAPHIC/ANIMATION ARTIST";
                creditsTesters              = "TESTERS";
                creditsFreesndUses          = "SOUNDS FROM WWW.FREESOUND.ORG";

            }
            cur = this;
        }



    }
}
