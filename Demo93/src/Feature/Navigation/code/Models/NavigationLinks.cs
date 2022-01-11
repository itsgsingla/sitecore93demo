using System;
using System.Collections.Generic;
using Sitecore.Data.Items;
using System.Linq;
using System.Web;

namespace Demo93.Feature.Navigation.Models
{
    public class NavigationLinks
    {
        public Item Item { get; set; }
        public string Link { get; set; }
        public List<NavigationLinks> Children { get; set; }
    }
}