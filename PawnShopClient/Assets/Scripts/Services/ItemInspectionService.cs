using UnityEngine;
using System.Collections.Generic;

public class ItemInspectionService : IItemInspectionService
{
    public List<BaseTagModel> Inspect(ICharacter character, ItemModel item)
    {
        var revealedTags = new List<BaseTagModel>();
        
        if (item?.Tags == null) return revealedTags;

        foreach (var tag in item.Tags)
        {
            if (tag.RequiredSkills == null || tag.RequiredSkills.Length == 0) continue;

            foreach (var skillType in tag.RequiredSkills)
            {
                int skillLevel = character.GetSkillLevel(skillType);
                float chance = skillLevel * 20f;
                
                if (Random.Range(0f, 100f) <= chance)
                {
                    revealedTags.Add(tag);
                    break; // Exit after first successful skill check
                }
            }
        }
        
        return revealedTags;
    }
}
