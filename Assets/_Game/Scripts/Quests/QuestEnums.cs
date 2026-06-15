namespace Isekai12Realms.Quests
{
    public enum QuestType { Main, Side, Daily, Achievement, Tutorial }
    public enum QuestObjectiveType { CompleteStage, DefeatEnemy, CollectItem, OwnEquipment, EquipEquipment, UpgradeEquipment, UpgradeSkill, ReachLevel, EarnGold, SpendGold, OpenScreen, MatchTokenCount, UseSkill, WinBattle, TalkToNpc }
    public enum QuestRewardType { Gold, SoulGem, Item, Equipment, EXP }
    public enum QuestStatus { Locked, Available, Active, Completed, Claimed }
}
