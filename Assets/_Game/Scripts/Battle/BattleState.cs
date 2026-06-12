using Isekai12Realms.Data;
using Isekai12Realms.Stages;

namespace Isekai12Realms.Battle
{
    public class BattleState
    {
        public StageDefinition stage;
        public string playerName = "Guest Hero";
        public int playerLevel = 1;
        public int maxHp = 100;
        public int hp = 100;
        public int maxMana = 100;
        public int mana = 0;
        public int shield = 0;
        public int food = 20;
        public int goldReward = 0;
        public int expReward = 0;

        public string enemyName = "Meadow Slime";
        public int enemyLevel = 1;
        public int enemyMaxHp = 80;
        public int enemyHp = 80;
        public int enemyMaxMana = 100;
        public int enemyMana = 0;
        public int enemyShield = 0;

        public BattleTurnOwner currentTurnOwner = BattleTurnOwner.Player;
        public int comboCount = 0;
        public int turnCount = 1;
        public BattleResultType battleResult = BattleResultType.None;
    }
}
