using Microsoft.AspNetCore.Html;
using System.Linq;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    public static class ViewHelpers
    {
        public static HtmlString GetTagColor(Settings settings, string tag)
        {
            string color = string.Empty;
            TagColor tagColor = settings.TagColors.FirstOrDefault(tagColor => tag.ToLower().StartsWith(tagColor.Start.ToLower()));
            if (tagColor != null)
                color = tagColor.Color;

            if (color.Length > 0 && !color.StartsWith("#"))
                color = $"#{color}";

            return new HtmlString($"background-color:{color};");
        }
    }

}
