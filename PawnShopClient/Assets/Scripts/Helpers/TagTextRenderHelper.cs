using PawnShop.Models.Tags;
using UnityEngine;

namespace PawnShop.Helpers
{
    /// <summary>
    /// Helper class for rendering a single tag with proper formatting and color
    /// </summary>
    public static class TagTextRenderHelper
    {
        /// <summary>
        /// Renders a tag with color and clickable link
        /// </summary>
        /// <param name="tag">Tag to render</param>
        /// <returns>Formatted tag string with color and link</returns>
        public static string RenderTag(BaseTagModel tag)
        {
            if (tag == null) return string.Empty;

            string tagColor = GetTagColorHex(tag);
            string tagDisplayName = !string.IsNullOrEmpty(tag.DisplayName) ? tag.DisplayName : tag.TagType.ToString();
            string formattedTag = $"[{tag.TagType}: {tagDisplayName}]";
            
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
