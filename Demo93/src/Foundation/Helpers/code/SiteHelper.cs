using Sitecore.Sites;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;

namespace Demo93.Foundation.Helpers
{
    public class SiteHelper
    {
        public static Item GetContextItem(ID derivedFromTemplateID)
        {
            SiteContext site = Sitecore.Context.Site;
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            var startItem = GetStartItem();
            return SitecoreHelper.ItemMethods.GetAncestorOrSelfOfTemplate(startItem, derivedFromTemplateID);
        }

        public static Item GetRootItem()
        {
            SiteContext site = Sitecore.Context.Site;
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            return site.Database.GetItem(site.RootPath);
        }

        public static Item GetStartItem()
        {
            SiteContext site = Sitecore.Context.Site;
            if (site == null)
                throw new ArgumentNullException(nameof(site));

            return site.Database.GetItem(site.StartPath);
        }
    }
}