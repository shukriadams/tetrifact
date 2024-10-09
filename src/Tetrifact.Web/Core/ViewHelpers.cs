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

        /// <summary>
        /// Appends page title and server name, if page title has a value
        /// </summary>
        /// <param name="mainTitle"></param>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public static HtmlString PageTitleConcat(string mainTitle, string serverName) 
        {
            string title = serverName;
            if (!string.IsNullOrEmpty(mainTitle) && !string.IsNullOrEmpty(serverName))
                title = $"{mainTitle}|{title}";

            return new HtmlString(title);
        }
    }

}
