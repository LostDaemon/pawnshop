using PawnShop.Models.Tags;
using PawnShop.Services;
using UnityEngine;

namespace PawnShop.Helpers
{
    /// <summary>
    /// Helper class for rendering a single tag with proper formatting and color
    /// </summary>
    public static class TagTextRenderHelper
    {
        /// <summary>
        /// Renders a tag with color, icon and clickable link
        /// </summary>
        /// <param name="tag">Tag to render</param>
        /// <returns>Formatted tag string with color, icon and link</returns>
        public static string RenderTag(BaseTagModel tag)
        {
            if (tag == null) return string.Empty;

            string tagColor = GetTagColorHex(tag);
            string tagDisplayName = !string.IsNullOrEmpty(tag.DisplayName) ? tag.DisplayName : tag.TagType.ToString();
            
            // Get icon
            string icon = !string.IsNullOrEmpty(tag.Icon) ? tag.Icon : "\uf005"; // Default FontAwesome star
            
            string formattedTag = $"{icon} {tagDisplayName}";
            
            return $"<color=#{tagColor}><link=\"{tag.ClassId}\">{formattedTag}</link></color>";
        }

        /// <summary>
        /// Renders a tag with color, icon, clickable link and localization
        /// </summary>
        /// <param name="tag">Tag to render</param>
        /// <param name="localizationService">Localization service for translating DisplayName</param>
        /// <returns>Formatted tag string with color, icon, link and localized text</returns>
        public static string RenderTag(BaseTagModel tag, ILocalizationService localizationService)
        {
            if (tag == null) return string.Empty;

            string tagColor = GetTagColorHex(tag);
            string tagDisplayName = !string.IsNullOrEmpty(tag.DisplayName) ? tag.DisplayName : tag.TagType.ToString();
            
            // Apply localization if service is available
            if (localizationService != null)
            {
                tagDisplayName = localizationService.GetLocalization(tagDisplayName);
            }
            
            // Get icon
            string icon = !string.IsNullOrEmpty(tag.Icon) ? tag.Icon : "\uf005"; // Default FontAwesome star
            
            string formattedTag = $"{icon} {tagDisplayName}";
            
            return $"<color=#{tagColor}><link=\"{tag.ClassId}\">{formattedTag}</link></color>";
        }

        /// <summary>
        /// Gets the hex color string for a tag
        /// </summary>
        /// <param name="tag">Tag to get color for</param>
        /// <returns>Hex color string without # prefix</returns>
        private static string GetTagColorHex(BaseTagModel tag)
        {
            // Use the tag's own color if available, otherwise use fallback color
            if (tag != null && tag.Color != Color.clear)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(tag.Color);
                return colorHex;
            }

            // Fallback to a neutral color
            string fallbackHex = ColorUtility.ToHtmlStringRGB(Color.white);
            return fallbackHex;
        }
    }
}
