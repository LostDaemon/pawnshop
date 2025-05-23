using System.Collections.Generic;

public class Customer : ICharacter
{
    public SkillSet Skills { get; } = new();
    public ItemModel OwnedItem { get; set; }
    public float UncertaintyLevel { get; set; }
    public float MoodLevel { get; set; }

    public Customer(Dictionary<SkillType, int> initialSkills, ItemModel ownedItem, float moodLevel = 0f)
    {
        OwnedItem = ownedItem;
        foreach (var kvp in initialSkills)
            Skills.Set(kvp.Key, kvp.Value);
    }
}