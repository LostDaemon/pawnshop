using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TagRepositoryService : ITagRepositoryService
{
    private readonly Dictionary<TagType, List<BaseTagPrototype>> _tagsByType = new();
    private readonly List<BaseTagPrototype> _allTags = new();

    public void Load()
    {
        _tagsByType.Clear();
        _allTags.Clear();

        // Load all tag prototypes from Resources
        var tagPrototypes = Resources.LoadAll<BaseTagPrototype>("ScriptableObjects/Tags");
        
        foreach (var tagPrototype in tagPrototypes)
        {
            if (tagPrototype == null) continue;

            // Add to all tags list
            _allTags.Add(tagPrototype);

            // Group by tag type
            if (!_tagsByType.ContainsKey(tagPrototype.TagType))
            {
                _tagsByType[tagPrototype.TagType] = new List<BaseTagPrototype>();
            }
            _tagsByType[tagPrototype.TagType].Add(tagPrototype);
        }

        Debug.Log($"[TagRepositoryService] Loaded {_allTags.Count} tag prototypes across {_tagsByType.Count} tag types");
    }

    public BaseTagPrototype GetTagPrototype(TagType tagType)
    {
        if (_tagsByType.TryGetValue(tagType, out var tags) && tags.Count > 0)
        {
            // Return first tag of this type (or could implement random selection logic)
            return tags[0];
        }
        return null;
    }

    public IReadOnlyCollection<BaseTagPrototype> GetAllTagPrototypes()
    {
        return _allTags.AsReadOnly();
    }
}
