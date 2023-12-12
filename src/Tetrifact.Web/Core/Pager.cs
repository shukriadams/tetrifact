using System.Text;
using Tetrifact.Core;

namespace Tetrifact.Web
{
    /// <summary>
    /// Renders pager control as text, this must be emitted into Razor views.
    /// </summary>
    public class Pager
    {
        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public string CssClass { get; set; }

        /// <summary>
        /// Unique id of page. Used to give child elements unique elements.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected int Currentgroup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected int PagesInGroup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected int TotalGroups { get; set; }

        /// <summary>
        /// Used internally only. Set by this.Calculate(), then used to determine if source data set is large enough to warrant paging.
        /// </summary>
        protected bool NotEnoughDataToPage { get; set; }

        /// <summary>
        /// Inline html. Use in place of function calls.
        /// </summary>
        public string Inline { get; set; }

        #endregion

        #region CTORS

        public Pager()
        {
            this.CssClass = "pager";
            this.Inline = string.Empty;
        }

        #endregion

        #region METHODS
        
        /// <summary>
        /// Renders a pager based on the PageableData object, which is itself a standardized collection for carrying pages of objects from a collection.
        /// </summary>
        /// <param name="page">PageableData object which pager will represent</param>
        /// <param name="pagesPerGroup">Number of page links to occur on pager. If more pages than this are possible, a "next" link is rendered.</param>
        /// <param name="baseurl">Nullable. Use if the existing url already has query string arguments in it. Paging args will be appended to url preserving existing.</param>
        /// <param name="pageQueryStringArg">Name of page query string argument</param>
        /// <returns>Rendered Html of pager.</returns>
        public string Render<T>(PageableData<T> page, int pagesPerGroup, string baseurl, string pageQueryStringArg, string hash = "")
        {
            // force cast max items from long to int - we will never realistically page through that many items
            this.Calculate((int)page.VirtualItemCount, page.PageIndex, page.PageSize, pagesPerGroup);
            if (this.NotEnoughDataToPage)
                return string.Empty;

            return this.RenderPageLinks(page.PageIndex, pagesPerGroup, baseurl, pageQueryStringArg, hash);
        }

        /// <summary>
        /// Call this first to calculate all numbers for paging. May also determine that there is not enough data to need paging.
        /// </summary>
        /// <param name="totalItems"></param>
        /// <param name="currentPage">Index of current page in total number of possible pages</param>
        /// <param name="itemsPerPage"></param>
        /// <param name="pagesPerGroup">Number of page links to occur on pager. If more pages than this are possible, a "next" link is rendered.</param>
        private void Calculate(int totalItemsToInt, int currentPage, int itemsPerPage, int pagesPerGroup)
        {
            int totalPages = totalItemsToInt / itemsPerPage;
            if (totalItemsToInt % itemsPerPage > 0)
                totalPages++;

            // no need to page if there is only one page
            if (totalPages < 2)
            {
                this.NotEnoughDataToPage = true;
                return;
            }

            this.Currentgroup = currentPage / pagesPerGroup;

            this.TotalGroups = totalItemsToInt / (itemsPerPage * pagesPerGroup);
            if (totalItemsToInt % (itemsPerPage * pagesPerGroup) > 0)
                this.TotalGroups++;

            // item list
            this.PagesInGroup = pagesPerGroup;
            if (this.Currentgroup == this.TotalGroups - 1 && totalPages % pagesPerGroup > 0)
                this.PagesInGroup = totalPages % pagesPerGroup;
        }

        /// <summary>
        /// Renders a page navigation paging bar
        /// </summary>
        /// <param name="currentPage">Index of current page in total number of possible pages</param>
        /// <param name="pagesPerGroup"></param>
        /// <param name="baseurl"></param>
        /// <param name="pageIndexName"></param>
        /// <returns>Rendered Html of pager.</returns>
        private string RenderPageLinks(int currentPage, int pagesPerGroup, string baseurl, string pageIndexName, string hash)
        {
            StringBuilder s = new StringBuilder();

            s.Append($"<ul class='{this.CssClass}'>");

            if (!baseurl.Contains("?"))
                baseurl = $"{baseurl}?";

            // previous
            if (this.Currentgroup > 0)
            {
                int back = ((this.Currentgroup * pagesPerGroup) - 1);
                s.Append($"<li class='pager-item'><a class='pager-link' href='{baseurl}&{pageIndexName}={back + 1}{hash}'>");
                s.Append("<span class='pager-linkContent'>");
                s.Append("..");
                s.Append("</span>");
                s.Append("</a></li>");
            }


            for (int i = 0; i < this.PagesInGroup; i++)
            {
                int c = (this.Currentgroup * pagesPerGroup) + i;
                bool isActive = currentPage == c;

                s.Append($"<li class='pager-item'>");

                if (isActive)
                    s.Append($"<a class='pager-link pager-link--active'>");
                else
                    s.Append($"<a class='pager-link' href='{baseurl}&{pageIndexName}={c + 1}{hash}'>");

                s.Append("<span class='pager-linkContent'>");
                s.Append(c + 1);
                s.Append("</span>");
                s.Append("</a></li>");
            }


            // next
            if (this.Currentgroup < this.TotalGroups - 1)
            {
                int forward = (this.Currentgroup + 1) * pagesPerGroup;
                s.Append($"<li class='pager-item'><a class='pager-link' href='{baseurl}&{pageIndexName}={forward + 1}{hash}'>");
                s.Append("<span class='pager-linkContent'>");
                s.Append("..");
                s.Append("</span>");
                s.Append("</a></li>");
            }

            s.Append("</ul>");

            return s.ToString();
        }

        #endregion
    }
}
