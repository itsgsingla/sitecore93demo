using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo93.Feature.Navigation
{
    public struct Template
    {
        public struct SiteSettings
        {
            public static readonly ID ID = new ID("{E64C9586-A4CC-47D2-8CFC-0A2722CC48A9}");

            public struct Fields
            {
                public static readonly ID Logo = new ID("{A8A9BD7D-9A64-440F-88DE-B05D8AFC8A73}");
                public static readonly ID Navigation = new ID("{ED41AB76-943C-4053-8DEE-FB2FA6188D2C}");
            }
        }

        public struct NavigationLinksFolder
        {
            public static readonly ID ID = new ID("{925D59DD-B89C-4817-919E-0159E454C5DA}");

            public struct Fields
            {
                public static readonly ID SelectedLinks = new ID("{56D93F3C-8365-45FA-96D9-604D93FA41CE}");
            }
        }

        public struct NavigationLinks
        {
            public static readonly ID ID = new ID("{0F5361D0-82E7-4B9A-BFD9-E0C0A85F0E47}");

            public struct Fields
            {
                public static readonly ID Name = new ID("{BD7240E5-8F9D-4189-A121-44C9CFEFB850}");
                public static readonly ID Link = new ID("{59BB2D02-804B-4B34-ACEC-7CFD1F61ADD6}");
                public static readonly ID SelectedLinks = new ID("{BC65A6F0-678A-4F40-BCB9-7C6F85BC6AD5}");
            }
        }
    }
}