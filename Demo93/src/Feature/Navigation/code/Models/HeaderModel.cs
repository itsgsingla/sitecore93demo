using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo93.Feature.Navigation.Models
{
    public class HeaderModel
    {
        public string LogoImageUrl { get; set; }

        public string LogoUrl { get; set; }

        public List<NavigationLinks> NavigationLinks { get; set; }
    }
}