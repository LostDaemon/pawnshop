using System.Collections.Generic;

public class ItemInspectionService : IItemInspectionService
{
    public List<BaseTagModel> Inspect(ICharacter character, ItemModel item)
    {
        var revealedTags = new List<BaseTagModel>();

        if (item?.Tags == null)
        {
            return revealedTags;
        }

        bool isPlayer = character is Player;
        bool isCustomer = character is Customer;

        foreach (var tag in item.Tags)
        {
            bool isTagRevealed = false;
            if (isPlayer)
            {
                isTagRevealed = tag.IsRevealedToPlayer;
            }
            else if (isCustomer)
            {
                isTagRevealed = tag.IsRevealedToCustomer;
            }

            if (isTagRevealed)
            {
                //revealedTags.Add(tag);
                continue;
            }

            if (tag.RequiredSkills == null || tag.RequiredSkills.Length == 0)
            {
                continue;
            }

            foreach (var skillType in tag.RequiredSkills)
            {
                if (character.Skills.TryGetValue(skillType, out var skill))
                {
                    int skillLevel = skill.Level;
                    float chance = skillLevel * 20f;

                    var randomValue = UnityEngine.Random.Range(0f, 1f) * 100f;
                    if (randomValue <= chance)
                    {
                        if (isPlayer)
                        {
                            tag.IsRevealedToPlayer = true;
                        }
                        else if (isCustomer)
                        {
                            tag.IsRevealedToCustomer = true;
                        }

                        revealedTags.Add(tag);
                        break;
                    }
                }
            }
        }

        return revealedTags; //Only newly revealed tags
    }
}
