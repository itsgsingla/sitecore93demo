using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Links;
using Sitecore.Mvc.Presentation;
using Sitecore.Resources.Media;
using Sitecore.Web.UI.WebControls;
using System.Web;
using Sitecore.Data.Managers;

namespace Demo93.Foundation.Helpers
{
    /// <summary>
    /// The SitecoreHelper class contains two nested classes: ItemMethods and ItemRenderMethods.
    ///  - ItemMethods contains methods for retrieving items from the Sitecore database using Sitecore APIs (e.g. by path, GUID, clone or ancestry)
    ///  - ItemRenderMethods contains methods for retrieving information about items in the Sitecore database using Sitecore APIs (e.g. links, datasources and field values)
    /// </summary>
    public class SitecoreHelper
    {
        #region Nested type: ItemMethods
        public class ItemMethods
        {
            /// <summary>
            /// Use this method when getting the item at a specific path, for a specific database
            /// </summary>
            /// <param name="path">string</param>
            /// <returns>Item </returns>
            public static Item GetItemByPath(string path, Database database)
            {
                return database.GetItem(path);
            }

            /// <summary>
            /// Use this method when getting the item at a specific path, for the current context database
            /// </summary>
            /// <param name="path">string</param>
            /// <returns>Item </returns>
            public static Item GetItemByPath(string path)
            {
                return Sitecore.Context.Database.GetItem(path);
            }


            /// <summary>
            /// Use this method when getting the item for a specific GUID.  Can pass in a specific database.
            /// Only use this method in special circumstances when you need to access master directly (eg, working with finding where clone came from).
            /// </summary>
            /// <param name="guid">The unique identifier.</param>
            /// <param name="database">The database.</param>
            /// <returns>
            /// Item
            /// </returns>
            public static Item GetItemFromGUID(string guid, Database database)
            {
                return database.GetItem(new ID(guid));
            }

            /// <summary>
            /// Use this method when getting the item for a specific GUID.  Will use the current context database (web).
            /// </summary>
            /// <param name="guid">The unique identifier.</param>
            /// <returns>
            /// Item
            /// </returns>
            public static Item GetItemFromGUID(string guid)
            {
                Item item = null;
                if (!string.IsNullOrEmpty(guid))
                {
                    Sitecore.Data.ID ItemID = ID.Parse(guid);
                    item = Sitecore.Context.Database.GetItem(ItemID);
                }
                return item;
            }

            public static Item GetAncestorOrSelfOfTemplate(Item item, ID templateID)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                return IsDerived(item, templateID) ? item : item.Axes.GetAncestors().LastOrDefault(i => IsDerived(i, templateID));
            }

            public static bool IsDerived(Item item, ID templateId)
            {
                if (item == null)
                {
                    return false;
                }

                return !templateId.IsNull && IsDerived(item, item.Database.Templates[templateId]);
            }

            private static bool IsDerived(Item item, Item templateItem)
            {
                if (item == null)
                {
                    return false;
                }

                if (templateItem == null)
                {
                    return false;
                }

                var itemTemplate = TemplateManager.GetTemplate(item);
                return itemTemplate != null && (itemTemplate.ID == templateItem.ID || itemTemplate.DescendsFrom(templateItem.ID));
            }

            public static string GetItemUrl(Item item)
            {
                string path = string.Empty;
                if (item != null)
                {
                    var options = Sitecore.Links.LinkManager.GetDefaultUrlOptions();
                    options.AlwaysIncludeServerUrl = true;

                    path = Sitecore.Links.LinkManager.GetItemUrl(item, options);
                }
                return path;
            }
        }
        #endregion

        #region Nested type: ItemRenderMethods
        public class ItemRenderMethods
        {
            /// <summary>
            /// Gets the URL of an item to be used when linking through to an item
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>String</returns>
            public static string GetItemUrl(Item item, Item currentItem = null)
            {
                if (item == null)
                {
                    return string.Empty;
                }
                if (currentItem != null && currentItem.ID == item.ID)
                {
                    return null;
                }

                string redirectUrl;
                var hasRedirectUrl =
                    GetGeneralLinkURL(item.Fields["Redirect Link"], out redirectUrl) &&
                    !String.IsNullOrEmpty(redirectUrl);

                var options = Sitecore.Links.LinkManager.GetDefaultUrlOptions();
                options.AlwaysIncludeServerUrl = true;

                return hasRedirectUrl ? redirectUrl : LinkManager.GetItemUrl(item, options);
            }

            // <summary>
            /// Tries to get the URL for a LinkField.
            /// </summary>
            /// <param name="linkField">The LinkField to retrieve the URL from.</param>
            /// <param name="url">The URL for the LinkField.</param>
            /// <returns>True if able to return a URL, otherwise false.</returns>
            public static Boolean GetGeneralLinkURL(LinkField linkField, out string url)
            {
                url = string.Empty;

                try
                {
                    if (linkField == null)
                        return false;

                    return linkField.LinkType == "internal"
                               ? TryGetUrlForInternalLinkField(linkField, out url)
                               : TryGetUrlForNonInternalLinkField(linkField, out url);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// Tries to get the URL for a LinkField whose type is "internal", handling item redirection.
            /// </summary>
            /// <param name="linkField">The LinkField to get the URL from.</param>
            /// <param name="url">The URL for the LinkField.</param>
            /// <returns>True if able to return a URL, otherwise false.</returns>
            private static bool TryGetUrlForInternalLinkField(LinkField linkField, out string url)
            {
                url = string.Empty;

                try
                {
                    if (linkField == null || linkField.TargetItem == null)
                    {
                        return false;
                    }

                    // Create a stack of internal link fields that will be used for processing redirects.
                    var internalLinkFieldStack = new Stack<LinkField>();
                    internalLinkFieldStack.Push(linkField);
                    while (internalLinkFieldStack.Count < 3)
                    {
                        var thisInternalLinkField = internalLinkFieldStack.Peek();
                        var thisItem = thisInternalLinkField.TargetItem;
                        var rawRedirectLinkField = thisItem.Fields["Redirect Link"];
                        var hasValidAndFilledRedirectLinkField = rawRedirectLinkField != null &&
                                                                 !String.IsNullOrEmpty(rawRedirectLinkField.Value) &&
                                                                 FieldTypeManager.GetField(rawRedirectLinkField) is
                                                                 LinkField;
                        if (hasValidAndFilledRedirectLinkField)
                        {
                            LinkField redirectLinkField = rawRedirectLinkField;
                            if (redirectLinkField.LinkType == "internal")
                            {
                                var redirectItem = redirectLinkField.TargetItem;
                                if (internalLinkFieldStack.Any(x => x.TargetItem.ID == redirectItem.ID))
                                {
                                    // We've hit an endless loop of redirection!
                                    Log.Error(
                                        "SitecoreHelper.ItemRenderMethods.TryGetUrlForInternalLinkField: The following item(s) are part of an endless loop of redirection: " +
                                        String.Join(", ",
                                                    internalLinkFieldStack.Select(x => x.TargetItem.ID.ToString()).Reverse()
                                                        .ToArray()), typeof(ItemRenderMethods));
                                    return false;
                                }
                                else
                                {
                                    // Add the redirectLinkField to the internalLinkFieldStack and continue processing redirects.
                                    internalLinkFieldStack.Push(redirectLinkField);
                                }
                            }
                            else
                            {
                                // The redirectLinkField is a non-internal type, so return the URL for the redirectLinkField.
                                return TryGetUrlForNonInternalLinkField(redirectLinkField, out url);
                            }
                        }
                        else
                        {
                            // No more redirection, so return this item.
                            url = LinkManager.GetItemUrl(thisItem);
                            if (!string.IsNullOrEmpty(thisInternalLinkField.QueryString))
                                url += "?" + thisInternalLinkField.QueryString;
                            if (!string.IsNullOrEmpty(thisInternalLinkField.Anchor))
                                url += "#" + thisInternalLinkField.Anchor;
                            return true;
                        }
                    }

                    Log.Error(
                        "SitecoreHelper.ItemRenderMethods.TryGetUrlForInternalLinkField: Hit the maximum number of redirects while processing the following chain of items: " +
                        String.Join(", ",
                                    internalLinkFieldStack.Select(x => x.TargetItem.ID.ToString()).Reverse().ToArray()),
                        typeof(ItemRenderMethods));
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// Tries to get the URL for a LinkField whose type is not "internal".
            /// </summary>
            /// <param name="linkField">The LinkField to get the URL from.</param>
            /// <param name="url">The URL for the LinkField.</param>
            /// <returns>True if able to return a URL, otherwise false.</returns>
            private static bool TryGetUrlForNonInternalLinkField(LinkField linkField, out string url)
            {
                url = string.Empty;

                try
                {
                    if (linkField == null)
                    {
                        return false;
                    }

                    switch (linkField.LinkType)
                    {
                        case "external":
                        case "mailto":
                        case "javascript":
                            url = linkField.Url;
                            return true;
                        case "anchor":
                            url = "#" + linkField.Anchor;
                            return true;
                        case "media":
                            if (linkField.TargetItem == null)
                                return false;
                            var mediaItem = new MediaItem(linkField.TargetItem);
                            return GetMediaURL(mediaItem, out url);
                        case "":
                            return true;
                        default:
                            return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// Tries to get the URL for the given MediaItem.
            /// </summary>
            /// <param name="mediaItem">The MediaItem to get the URL for.</param>
            /// <param name="url">The URL for the MediaItem.</param>
            /// <returns>True if a URL could be returned for the MediaItem; otherwise, false.</returns>
            public static Boolean GetMediaURL(MediaItem mediaItem, out String url)
            {
                url = string.Empty;

                if (mediaItem == null)
                {
                    return false;
                }
                else
                {
                    url = Sitecore.StringUtil.EnsurePrefix('/', MediaManager.GetMediaUrl(mediaItem, new MediaUrlOptions { IncludeExtension = true }));
                    return true;
                }
            }

            public static string GetImageURL(Item item, String field)
            {
                if (item == null || field.Length == 0)
                    return null;
                string url = string.Empty;
                ImageField imageField = item.Fields[field];
                if (imageField == null || imageField.MediaItem == null)
                {
                }
                else
                {
                    GetMediaURL(imageField.MediaItem, out url);
                }

                return url;
            }

            /// <summary>
            /// Use this method for getting any needed values from an item.  
            /// Pass in the field name, item, and whether to use FieldRenderer (useRenderer) when getting the value
            /// </summary>
            /// <param name="fieldName">string</param>
            /// <param name="item">Item</param>
            /// <param name="useRenderer">bool</param>
            /// <returns>string</returns>
            public static string GetRawValueByFieldName(string fieldName, Item item, bool useRenderer)
            {
                string fieldValue = string.Empty;
                if (item != null)
                {
                    try
                    {
                        if (useRenderer)
                            fieldValue = FieldRenderer.Render(item, fieldName);
                        else
                            fieldValue = item.Fields[fieldName].Value;
                    }
                    catch (Exception ex)
                    {
                        Sitecore.Diagnostics.Log.Error(ex.Message, new object());
                    }
                }
                return fieldValue;
            }

            public static bool GetCheckBoxValueByFieldName(string fieldName, Item item)
            {
                return GetCheckBoxValueByFieldName(fieldName, item, false);
            }

            /// <summary>
            /// By default the return value is false, can optionally pass in what the default value should be.  
            /// This tries to cast the passed in field as a checkbox field for the item and return whether it is checked or not.
            /// </summary>
            /// <param name="fieldName">string</param>
            /// <param name="item">Item</param>
            /// <param name="defaultValue">bool</param>
            /// <returns>bool</returns>
            public static bool GetCheckBoxValueByFieldName(string fieldName, Item item, bool defaultValue)
            {
                bool isChecked = defaultValue;

                if (item != null)
                {
                    try
                    {
                        var chkField = (CheckboxField)item.Fields[fieldName];
                        isChecked = chkField.Checked;
                    }
                    catch (Exception ex)
                    {
                        Sitecore.Diagnostics.Log.Error(ex.Message, new object());
                    }
                }

                return isChecked;
            }
            /// <summary>
            /// For the passed in item, tries to catch the field for the passed in string to a MultilistField and then uses GetItems to return the list of related items
            /// </summary>
            /// <param name="fieldName">string</param>
            /// <param name="item">Item</param>
            /// <returns>Generics List of Items</returns>
            public static List<Item> GetMultilistValueByFieldName(string fieldName, Item item)
            {
                List<Item> relatedItems = null;

                if (item != null)
                {
                    try
                    {
                        MultilistField relatedField = (MultilistField)item.Fields[fieldName];
                        relatedItems = relatedField.GetItems().ToList();
                    }
                    catch (Exception)
                    {

                    }
                }

                return relatedItems;
            }
            public static bool TryGetDateTimeByFieldName(Item item, string fieldName, out DateTime dateTime)
            {
                dateTime = DateTime.MinValue;

                try
                {
                    var rawDateField = item.Fields[fieldName];
                    var dateFieldIsValidAndHasContent = rawDateField != null &&
                                                        !string.IsNullOrEmpty(rawDateField.Value) &&
                                                        FieldTypeManager.GetField(rawDateField) is DateField;
                    if (!dateFieldIsValidAndHasContent)
                    {
                        return false;
                    }

                    DateField dateField = rawDateField;
                    dateTime = dateField.DateTime;
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            public static bool GetGeneralLinkURL(Item item, string fieldName, out string url)
            {
                url = string.Empty;

                try
                {
                    if (item == null || string.IsNullOrEmpty(fieldName))
                    {
                        return false;
                    }

                    Field rawLinkField = item.Fields[fieldName];
                    bool linkFieldIsValidAndHasContent = rawLinkField != null &&
                                                        !string.IsNullOrEmpty(rawLinkField.Value) &&
                                                        FieldTypeManager.GetField(rawLinkField) is LinkField;
                    if (!linkFieldIsValidAndHasContent)
                    {
                        return false;
                    }

                    LinkField linkField = rawLinkField;
                    return GetGeneralLinkURL(linkField, out url);
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// Gets the link field URL.
            /// </summary>
            /// <param name="sitecoreItem">The sitecore item.</param>
            /// <param name="fieldName">Name of the field.</param>
            /// <param name="url">The URL.</param>
            /// <param name="linkText">The link text.</param>
            /// <param name="linkTarget">The link target.</param>
            public static bool GetLinkFieldTarget(Item sitecoreItem, string fieldName, out string linkTarget)
            {
                linkTarget = "";
                string url;

                try
                {
                    LinkField linkField = sitecoreItem.Fields[fieldName];
                    if (linkField != null)
                    {
                        if (!GetGeneralLinkURL(linkField, out url))
                        {
                            return false;
                        }
                        linkTarget = linkField.Target;
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
                return false;
            }

            public static string GetExternalUrl(Item item, string fieldName)
            {
                if (item == null) return string.Empty;
                LinkField lf = item.Fields[fieldName];
                switch (lf.LinkType.ToLower())
                {
                    case "internal":
                        // Use LinkMananger for internal links, if link is not empty
                        return lf.TargetItem != null ? Sitecore.Links.LinkManager.GetItemUrl(lf.TargetItem) : string.Empty;
                    case "media":
                        // Use MediaManager for media links, if link is not empty
                        return lf.TargetItem != null ? Sitecore.Resources.Media.MediaManager.GetMediaUrl(lf.TargetItem) : string.Empty;
                    case "external":
                        // Just return external links
                        return lf.Url;
                    case "anchor":
                        // Prefix anchor link with # if link if not empty
                        return !string.IsNullOrEmpty(lf.Anchor) ? "#" + lf.Anchor : "javascript:void(0);";
                    case "mailto":
                        // Just return mailto link
                        return lf.Url;
                    case "javascript":
                        // Just return javascript
                        return lf.Url;
                    default:
                        // Just please the compiler, this
                        // condition will never be met
                        return lf.Url;
                }
            }

            public static string GetLinkURL(LinkField linkField)
            {
                string url = "";
                try
                {
                    switch (linkField.LinkType)
                    {
                        case "internal":
                            Item targetItem = linkField.TargetItem;
                            url = ItemMethods.GetItemUrl(targetItem);
                            break;
                        case "external":
                            url = linkField.Url;

                            break;
                        case "media":
                            MediaItem media = new MediaItem(linkField.TargetItem);
                            url = Sitecore.StringUtil.EnsurePrefix('/', Sitecore.Resources.Media.MediaManager.GetMediaUrl(media));
                            break;
                        default:
                            url = "";
                            break;
                    }
                }
                catch (Exception exception)
                {
                    url = "";
                }
                return url;
            }

            /// <summary>
            /// Pass in an item and a field and will use the GUID stored in that field (assuming it is a field that relates to another item) to get the related or referenced item
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="field">String</param>
            /// <returns>Item , Null Value Exception</returns>
            public static Item GetReferenceField(Item item, String field)
            {
                if (item == null || field.Length == 0)
                    return null;

                ReferenceField refField;

                try
                {
                    refField = item.Fields[field];
                    if (refField != null)
                    {
                        if (refField.TargetItem != null)
                        {
                            return refField.TargetItem;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Sitecore.Diagnostics.Log.Error(ex.Message, new object());
                    return null;
                }

                return null;
            }

            /// <summary>
            /// Pass in an item Id and a field and will use the GUID stored in that field (assuming it is a field that relates to another item) to get the related or referenced item
            /// </summary>
            /// <see cref="GetReferenceField(Item, String)"/>
            /// <param name="item">Item</param>
            /// <param name="field">String</param>
            /// <returns>Item , Null Value Exception</returns>

            public static Item GetReferenceField(string itemId, string field)
            {
                var item = ItemMethods.GetItemFromGUID(itemId);
                return item == null ? null : GetReferenceField(item, field);
            }
        }
        #endregion
    }
}