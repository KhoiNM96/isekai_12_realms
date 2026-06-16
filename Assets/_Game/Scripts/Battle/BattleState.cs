using Isekai12Realms.Data;
using Isekai12Realms.Realms;
using Isekai12Realms.Stages;

namespace Isekai12Realms.Battle
{
    public class BattleState
    {
        public StageDefinition stage;
        public BattleEncounterData encounter;
        public RealmDefinition realm;
        public string playerName = "Guest Hero";
        public int playerLevel = 1;
        public int maxHp = 100;
        public int hp = 100;
        public int maxMana = 100;
        public int mana = 0;
        public int shield = 0;
        public int food = 20;
        public int atk = 10;
        public int mag = 8;
        public int def = 5;
        public int spd = 5;
        public int luck = 1;
        public int foodBonus;
        public int manaGainBonus;
        public float dropRateBonus;
        public float expBonus;
        public float goldBonus;
        public float healBonus;
        public float critRate;
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
        public float remainingTurnTime = 30f;
        public int comboCount = 0;
        public int turnCount = 0;
        public bool isResolvingTurn;
        public bool inputLocked;
        public bool currentTurnHasExtraTurn;
        public BattleResultType battleResult = BattleResultType.None;
        public string lastEnemyAction = string.Empty;
        public int lastEnemyActionValue = 0;
        public string lastPlayerMove = string.Empty;
        public string lastEnemyMove = string.Empty;
        public bool lastMoveGrantedExtraTurn;
        public int lastMaxMatchSize;
    }
}
