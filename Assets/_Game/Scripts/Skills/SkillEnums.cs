namespace Isekai12Realms.Skills
{
    public enum SkillSlotType
    {
        Skill1,
        Skill2,
        Ultimate,
        Passive
    }

    public enum SkillTargetType
    {
        Enemy,
        Player,
        Board,
        RandomTiles,
        SelectedTile,
        Self
    }

    public enum SkillActivationType
    {
        Active,
        Passive
    }

    public enum SkillEffectType
    {
        Damage,
        Heal,
        Shield,
        GainMana,
        DestroyRandomTiles,
        DestroyArea,
        ShuffleBoard,
        ConvertTiles,
        ExtraTurn,
        BuffDamage,
        BuffHeal,
        CleanseDebuff
    }
}
