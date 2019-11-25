using System.Collections.Generic;

namespace Tetrifact.Web
{
    public class ContentSummaryModel
    {
        public IEnumerable<string> Projects { get; set; }

        public ContentSummaryModel(IEnumerable<string> projects) 
        {
            this.Projects = projects;
        }
    }
}
