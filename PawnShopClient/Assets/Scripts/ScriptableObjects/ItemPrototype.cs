using UnityEngine;
using System.Collections.Generic;
using PawnShop.ScriptableObjects.Tags;
using PawnShop.Models.Tags;

namespace PawnShop.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ItemPrototype", order = 1)]
    public class ItemPrototype : BasePrototype
    {
        [Header("Basic Information")]
        public string Name;
        public Sprite Image;
        public long BasePrice;
        public float Scale;
        [TextArea(3, 5)]
        public string Description;

        [System.Serializable]
        public class TagLimit
        {
            public TagType TagType;
            [Tooltip("Maximum number of tags of this type that can be applied to the item")]
            public int MaxCount = 1;
        }

        [Header("Tags")]
        [Tooltip("Tag types that will be automatically applied to every item of this type")]
        public List<TagType> requiredTags = new List<TagType>();

        [Tooltip("Available tag types and their maximum counts for this item")]
        public List<TagLimit> allowedTags = new List<TagLimit>();

        [Tooltip("Override default tag generation algorithm")]
        public bool OverrideTagsGeneration = false;

        [Tooltip("Tags that will be applied to every item of this type (overrides random generation)")]
        public List<BaseTagPrototype> OverridedTags = new List<BaseTagPrototype>();
    }
}