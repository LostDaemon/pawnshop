using System.Collections.Generic;

public class Customer : ICharacter
{
    public SkillSet Skills { get; } = new();
    public ItemModel OwnedItem { get; set; }

    public Customer() { }

    public Customer(Dictionary<SkillType, int> initialSkills, ItemModel ownedItem)
    {
        OwnedItem = ownedItem;
        foreach (var kvp in initialSkills)
            Skills.Set(kvp.Key, kvp.Value);
    }
}