using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace League.Hashes
{
    public static class CharacterHashes
    {
        // Filenames of the character's HUD images.
        public static readonly uint CircleIconDds = 3392217477;
        public static readonly uint SquareIconDds = 3606610482;
        public static readonly uint PassiveIconDds = 3810483779;

        // The character's skin hashes
        public static readonly SkinHashes[] Skins;

        // The character's type, string, can contain something like "Champion" or "Minion | Minion_Lane | Minion_Lane_Siege" 
        public static readonly uint Type = 3758951037;

        // Keys used to find strings. These point towards strings such as "game_character_displayname_Aatrox"
        public static readonly uint DescriptionKey = 3747042364;
        public static readonly uint PassiveNameKey = 3401798261;
        public static readonly uint NameKey = 82690155;

        // A string containing the character's material, such as "Flesh", "Stone", "Metal", "Wood".
        public static readonly uint Material = 3310611270;

        static CharacterHashes()
        {
            Skins = new SkinHashes[9]; // skin.blnd, skin.skn, skin.skl, skin.dds - Hashes which point to these filenames in this order
            Skins[0] = new SkinHashes(4263852911, 769344815, 1895303501, 2640183547);
            Skins[1] = new SkinHashes(3800548434, 306040338, 599757744, 664710616);
            Skins[2] = new SkinHashes(4020459281, 525951185, 3575010799, 4206390233);
            Skins[3] = new SkinHashes(4240370128, 745862032, 2255296558, 3453102554);
            Skins[4] = new SkinHashes(165313679, 965772879, 935582317, 2699814875);
            Skins[5] = new SkinHashes(385224526, 1185683726, 3910835372, 1946527196);
            Skins[6] = new SkinHashes(605135373, 1405594573, 2591121131, 1193239517);
            Skins[7] = new SkinHashes(825046220, 1625505420, 1271406890, 439951838);
            Skins[8] = new SkinHashes(1044957067, 1845416267, 4246659945, 439951838);
        }
    }
}
