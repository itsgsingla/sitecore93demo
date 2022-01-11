using Demo93.Feature.Navigation.Models;
using Demo93.Foundation.Helpers;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Demo93.Feature.Navigation.Repository
{
    public class NavigationRepository: INavigationRepository
    {
        public HeaderModel GetHeader()
        {
            HeaderModel headerModel = null;
            var settingsItem = SitecoreHelper.ItemMethods.GetItemFromGUID(Constant.SettingsItemId);
            if (settingsItem != null)
            {
                var settingsModel = new SettingsModel()
                {
                    LogoImageUrl = SitecoreHelper.ItemRenderMethods.GetImageURL(settingsItem, Template.SiteSettings.Fields.Logo.ToString()),
                    NavigationId = SitecoreHelper.ItemRenderMethods.GetRawValueByFieldName(Template.SiteSettings.Fields.Navigation.ToString(), settingsItem, false)
                };

                headerModel = new HeaderModel();
                headerModel.LogoImageUrl = settingsModel.LogoImageUrl;
                headerModel.LogoUrl = SitecoreHelper.ItemRenderMethods.GetItemUrl(SiteHelper.GetStartItem()); 

                var navigationFolderItem = SitecoreHelper.ItemMethods.GetItemFromGUID(settingsModel.NavigationId);
                var navigationLinksLevel1 = SitecoreHelper.ItemRenderMethods.GetMultilistValueByFieldName(Template.NavigationLinksFolder.Fields.SelectedLinks.ToString(), navigationFolderItem);
                var navLinks = new List<NavigationLinks>();
                foreach (var navItem in navigationLinksLevel1)
                {
                    NavigationLinks navLink = new NavigationLinks();
                    navLink.Item = navItem;
                    navLink.Link = SitecoreHelper.ItemRenderMethods.GetExternalUrl(navItem, Template.NavigationLinks.Fields.Link.ToString());
                    navLink.Children = GetNavigationLinkChildren(navItem);
                    navLinks.Add(navLink);
                }
                headerModel.NavigationLinks = navLinks;
            }

            return headerModel;
        }


        private List<NavigationLinks> GetNavigationLinkChildren(Item navigationLinkItem)
        {
            var selectedNavigationLinksItems = SitecoreHelper.ItemRenderMethods.GetMultilistValueByFieldName(Template.NavigationLinks.Fields.SelectedLinks.ToString(), navigationLinkItem);
            var navLinks = new List<NavigationLinks>();
            foreach (var navItem in selectedNavigationLinksItems)
            {
                NavigationLinks navLink = new NavigationLinks();
                navLink.Item = navItem;
                navLink.Link = SitecoreHelper.ItemRenderMethods.GetExternalUrl(navItem, Template.NavigationLinks.Fields.Link.ToString());
                navLink.Children = GetNavigationLinkChildren(navItem);
                navLinks.Add(navLink);
            }

            return navLinks;
        }
    }
}