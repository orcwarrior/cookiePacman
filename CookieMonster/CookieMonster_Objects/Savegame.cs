using System;
using System.Collections.Generic;
using System.Text;

namespace CookieMonster.CookieMonster_Objects
{
    class mapSave
    {
        internal uint data;
        internal uint data2;

        //public uint level : 8; (0-256)
        public uint level
        {
            get { return data & 0xFF; }
            set { data = (data & ~0xFFu) | (value & 0xFF); }//u at end of hex means unsigned
        }

        //public uint lives : 6 (64)
        public uint lives
        {
            get { return (uint)((data >> 8) & 0x3F); }
            set { data = (data & ~(0x3Fu << 8)) | (value & 0x3F) << 8; }
        }

        //public uint score : 12 (4096)
        public uint lvlScore
        {
            get { return (data >> 14) & 0xFFF; }
            set { data = (data & ~(0xFFFu << 14)) | (value & 0xFFF) << 14; }
        }
        //public uint checksum : 6 (64)
        public uint dataChecksum
        {
            get { return (data >> 26) & 0x3F; }
            set { data = (data & ~(0x3Fu << 26)) | (value & 0x3F) << 26; }
        }

        //public uint lvlDuration: 22(4194304)
        public uint lvlDuration
        {
            get { return (uint)(data2 & 0x3FFFFF); }
            set { data2 = (data2 & ~(0x3FFFFFu)) | (value & 0x3FFFFF); }
        }
        public uint data2Checksum1 //:4(16)
        {
            get { return (uint)((data2 >> 22) & 0xF); }
            set { data2 = (data2 & ~(0xFu << 22)) | (value & 0xF) << 22; }
        }
        public uint data2Checksum2 //:6(64)
        {
            get { return (uint)((data2 >> 26) & 0x3F); }
            set { data2 = (data2 & ~(0x3Fu << 26)) | (value & 0x3F) << 26; }
        }

    }
    class playerData
    {
        internal uint data;
        internal uint data2;
        internal UInt16 data3;
        internal uint data4;//gameDuration + checksum

        //public uint level : 8; (0-256)
        public uint level
        {
            get { return (data & 0xFF); }
            set { data = (data & ~0xFFu) | (value & 0xFF); }//u at end of hex means unsigned
        }

        //public uint  exp : 16; (0-65536)
        public uint score { get { return exp; } } //it's same thing
        public uint exp
        {
            get { return ((data >> 8) & 0xFFFF); }
            set { data = (data & ~(0xFFFFu << 8)) | (value & 0xFFFF) << 8; }
        }//25

        //public uint lives : 6 (64)
        public uint lives
        {
            get { return ((data >> 24) & 0x3F); }
            set { data = (data & ~(0x3Fu << 24)) | (value & 0x3F) << 24; }
        }
        public uint zeroCheck
        {
            get { return ((data >> 30) & 0x1); }
        }
        //public uint maxLives : 6 (64)
        public uint maxLives
        {
            get { return (data2 & 0x3F); }
            set { data2 = (data2 & ~(0x3Fu)) | (value & 0x3F); }
        }
        //public uint tp : 6 (64)
        public uint talentPts
        {
            get { return ((data2 >> 6) & 0x3F); }
            set { data2 = (data2 & ~(0x3Fu << 6)) | (value & 0x3F) << 6; }
        }
        //public uint iceBoltLVL : 4 (16)
        public uint iceBoltLVL
        {
            get { return (uint)((data2 >> 12) & 0xF); }
            set { data2 = (data2 & ~(0xFu << 12)) | (value & 0xF) << 12; }
        }
        //public uint speedBoostLVL : 4 (16)
        public uint speedBoostLVL
        {
            get { return (uint)((data2 >> 16) & 0xF); }
            set { data2 = (data2 & ~(0xFu << 16)) | (value & 0xF) << 16; }
        }//52        
        //public uint invHasBomb : 1 (1)
        public uint invHasBomb
        {
            get { return (uint)((data2 >> 20) & 0x1); }
            set { data2 = (data2 & ~(0x1u << 20)) | (value & 0x1) << 20; }
        }//53        
        //public uint invHasTimeSlow : 1 (1)
        public uint invHasTimeSlow
        {
            get { return (uint)((data2 >> 21) & 0x1); }
            set { data2 = (data2 & ~(0x1u << 21)) | (value & 0x1) << 21; }
        }//54
        //public uint speed :  9(512)
        public uint movementSpeed
        {
            get { return (uint)((data2 >> 22) & 0x1FF); }
            set { data2 = (data2 & ~(0x1FFu << 22)) | (value & 0x1FF) << 22; }
        }

        //public uint difficultLevel: 3(8)
        public uint difficultLevel
        {
            get { return (uint)(data3 & 0x7); }
            set { data3 = (UInt16)((data3 & ~(0x7u)) | (value & 0x7)); }
        }
        //public uint maps: 7(128)
        public uint mapsCount
        {
            get { return (uint)((data3 >> 3) & 0x7F); }
            set { data3 = (UInt16)((data3 & ~(0x7Fu << 3)) | (value & 0x7F) << 3); }
        }
        //public uint data3Checksum: 6(64)
        public uint data123Checksum
        {
            get { return (uint)((data3 >> 10) & 0x3F); }
            set { data3 = (UInt16)((data3 & ~(0x3Fu << 10)) | (value & 0x3F) << 10); }
        }
        //public uint gameDuration: 26(67108864)
        public uint gameDuration
        {
            get { return (uint)(data4 & 0x3FFFFFF); }
            set { data4 = (data4 & ~(0x3FFFFFFu)) | (value & 0x3FFFFFF); }
        }
        public uint data4Checksum //:6(64)
        {
            get { return (uint)((data4 >> 26) & 0x3F); }
            set { data4 = (data4 & ~(0x3Fu << 26)) | (value & 0x3F) << 26; }
        }
        
        
    }
    class Savegame
    {
        public List<mapSave> maps { get; private set; }
        public playerData player { get; private set; }

        public Savegame()
        {
            maps = new List<mapSave>();
            player = new playerData();
        }
        public void Update()
        {
            updatePlayerData();
            addMapData();
        }
        /// <summary>
        /// initing GameManager using current Savegame data
        /// (Just loads game from save)
        /// </summary>
        public void Load()
        {
            EngineApp.Game.self.loadGame(this);
        }
        private void addMapData()
        {
          
           mapSave map = new mapSave();
           GameManager gm = EngineApp.Game.self.gameManager;
           map.level = (uint)gm.level;
           map.lives = (uint)gm.PC.lives;
           map.lvlScore = gm.statistics.lvlPoints;
           map.dataChecksum = (map.level * 3 + map.lvlScore) % 64;
           //data2
           map.lvlDuration = (uint)gm.levelDuration.currentTime;
           map.data2Checksum1 = map.lvlDuration%15;
           map.data2Checksum2 = (uint)(gm.Map.mapWidth % 11 + gm.Map.cookiesCount % 22 + gm.Map.wayNetwork.Count % 20 + gm.sortedEnemiesList.Count % 11);
           //BUGFIX: if level == first
           //recreate list of maps
           if (map.level == 1)
               maps = new List<mapSave>();
           maps.Add(map);
        }

        private void updatePlayerData()
        {
            Player pc = EngineApp.Game.self.gameManager.PC;
            player.level = (uint)pc.level;
            player.exp = (uint)pc.exp;
            player.lives = (uint)pc.lives;
            player.maxLives = (uint)pc.maxLives;
            player.talentPts = (uint)pc.talentPoints;

            if (pc.hasSkill(Skill.skillNames.IceBolt) == null)
                player.iceBoltLVL = 0;
            else
                player.iceBoltLVL = (uint)pc.hasSkill(Skill.skillNames.IceBolt).level;

            if (pc.hasSkill(Skill.skillNames.Boost) == null)
                player.speedBoostLVL = 0;
            else
                player.speedBoostLVL = (uint)pc.hasSkill(Skill.skillNames.Boost).level;

            if ((pc.powerUpsInventory & POWER_UP.BOMB) == POWER_UP.BOMB)
                player.invHasBomb = 1;
            else
                player.invHasBomb = 0;

            if ((pc.powerUpsInventory & POWER_UP.ENEMY_SLOWER) == POWER_UP.ENEMY_SLOWER)
                player.invHasTimeSlow = 1;
            else
                player.invHasTimeSlow = 0;
            player.movementSpeed = (uint)pc.baseSpeed;

            //data3:
            player.difficultLevel = (uint)Profile.currentProfile.config.gameplay.level;
            player.mapsCount = (uint)Profile.currentProfile.config.gameplay.maps;

            player.data123Checksum = player.exp % 37 + player.lives % 4 + player.movementSpeed % 12 + player.talentPts % 4 + player.speedBoostLVL % 3 + player.iceBoltLVL % 3 + player.invHasBomb;

            //data4:
            player.gameDuration = (uint)EngineApp.Game.self.gameManager.gameDuration.currentTime;
            //checksum: remainder of gameDuration/59
            player.data4Checksum = player.gameDuration % 59;

        }
        public string encrypt()
        {
            string buffer = "";
            const string sep = "x";
            //player:
            buffer += player.data.ToString("X") + sep;
            buffer += player.data2.ToString("X") + sep;
            buffer += player.data3.ToString("X") + sep;
            buffer += player.data4.ToString("X") + sep;
            //map(s):
            for (int i = 0; i < maps.Count; i++)
            {
                buffer += "#" + maps[i].data.ToString("X") + sep;
                buffer += maps[i].data2.ToString("X");
            }
            return buffer;
        }

        internal void decrypt(string buffer)
        {
            string hlp = buffer.Substring(0, buffer.IndexOf("x"));
            uint x = Convert.ToUInt32(hlp, 16);
            player = new playerData();
            player.data = x;

            buffer = buffer.Substring(buffer.IndexOf("x") + 1);
            hlp = buffer.Substring(0, buffer.IndexOf("x"));
            x = Convert.ToUInt32(hlp, 16);
            player.data2 = x;
            
            buffer = buffer.Substring(buffer.IndexOf("x")+1);
            hlp = buffer.Substring(0, buffer.IndexOf("x"));
            player.data3 = Convert.ToUInt16(hlp, 16);

            buffer = buffer.Substring(buffer.IndexOf("x") + 1);
            hlp = buffer.Substring(0, buffer.IndexOf("x"));
            player.data4 = Convert.ToUInt32(hlp, 16);

            //check for playerData checksums
            uint checksum;
            checksum = player.exp % 37 + player.lives % 4 + player.movementSpeed % 12 + player.talentPts % 4 + player.speedBoostLVL % 3 + player.iceBoltLVL % 3 + player.invHasBomb;
            if (checksum != player.data123Checksum)
                throw new Exception("Save data checksum corrupted(gx123)!");
            if (player.zeroCheck != 0)
                throw new Exception("Save data checksum corrupted(gx0)!");
            checksum = player.gameDuration % 59; 
            if (checksum != player.data4Checksum)
                throw new Exception("Save data checksum corrupted(gx4)!");

            //map(s):
            if (maps == null) maps = new List<mapSave>();
            int last = buffer.IndexOf("#");
            while (last > 0)
            {
                buffer = buffer.Substring(buffer.IndexOf("#") + 1);
                hlp = buffer.Substring(0, buffer.IndexOf("x"));
                mapSave m = new mapSave();
                m.data = Convert.ToUInt32(hlp, 16);
                buffer = buffer.Substring(buffer.IndexOf("x") + 1);
                last = buffer.IndexOf("#");
                if (last > 0)
                    hlp = buffer.Substring(0, last);
                else
                    hlp = buffer;
                if (hlp.Length > 0)
                    m.data2 = Convert.ToUInt32(hlp, 16);
                //check checksums:
                checksum = (m.level * 3 + m.lvlScore) % 64;
                if (checksum != m.dataChecksum)
                    throw new Exception("Save data checksum corrupted(mx"+maps.Count+"x0)!");
                checksum = m.lvlDuration % 15;
                if (checksum != m.data2Checksum1)
                    throw new Exception("Save data checksum corrupted(mx" + maps.Count + "x2x1)!");
                //TODO:Last chcecksum get values from array
                //checksum = (uint)(gm.Map.gridWidth % 11 + gm.Map.cookiesCount % 22 + gm.Map.wayNetwork.Count % 20 + gm.sortedEnemiesList.Count % 11);
                //if (checksum != m.data2Checksum1)
                //    new Exception("Save data checksum corrupted!");

                maps.Add(m);

            }
            //TODO: Maps data
        }
    }
}
