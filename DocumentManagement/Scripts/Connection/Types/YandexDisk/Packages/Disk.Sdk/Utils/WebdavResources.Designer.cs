﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#pragma warning disable
namespace Disk.SDK.Utils {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class WebdavResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal WebdavResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Disk.SDK.Utils.WebdavResources", typeof(WebdavResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://webdav.yandex.ru/.
        /// </summary>
        internal static string ApiUrl {
            get {
                return "https://webdav.yandex.ru/";
                return ResourceManager.GetString("ApiUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://oauth.yandex.ru/authorize?response_type=token&amp;client_id={0}&amp;display=popup.
        /// </summary>
        internal static string AuthBrowserUrlFormat {
            get {
                return "https://oauth.yandex.ru/authorize?response_type=token&amp;client_id={0}&amp;display=popup";
                return ResourceManager.GetString("AuthBrowserUrlFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to grant_type=password&amp;username={0}&amp;password={1}&amp;client_id={2}&amp;client_secret={3}.
        /// </summary>
        internal static string AuthParamsFormat {
            get {
                return "grant_type=password&amp;username={0}&amp;password={1}&amp;client_id={2}&amp;client_secret={3}";
                return ResourceManager.GetString("AuthParamsFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://oauth.yandex.ru/token.
        /// </summary>
        internal static string AuthUrl {
            get {
                return "https://oauth.yandex.ru/token";
                return ResourceManager.GetString("AuthUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incorrect parameters - provide correct data and try again.
        /// </summary>
        internal static string BadParameterErrorMesage {
            get {
                return "Incorrect parameters - provide correct data and try again";
                return ResourceManager.GetString("BadParameterErrorMesage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incorrect request - provide correct data and try again.
        /// </summary>
        internal static string BadRequestErrorMessage {
            get {
                return "Incorrect request - provide correct data and try again";
                return ResourceManager.GetString("BadRequestErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;propfind xmlns=&quot;DAV:&quot;&gt;
        ///  &lt;prop&gt;
        ///    &lt;public_url xmlns=&quot;urn:yandex:disk:meta&quot;/&gt;
        ///  &lt;/prop&gt;
        ///&lt;/propfind&gt;.
        /// </summary>
        internal static string CheckPublishingBody {
            get {
                return @"&lt;propfind xmlns=&quot;DAV:&quot;&gt;
                           &lt;prop&gt;
                             &lt;public_url xmlns=&quot;urn:yandex:disk:meta&quot;/&gt;
                           &lt;/prop&gt;
                         &lt;/propfind&gt;";
                return ResourceManager.GetString("CheckPublishingBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to COPY.
        /// </summary>
        internal static string CopyMethod {
            get {
                return "COPY";
                return ResourceManager.GetString("CopyMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DELETE.
        /// </summary>
        internal static string DeleteMethod {
            get {
                return "DELETE";
                return ResourceManager.GetString("DeleteMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GET.
        /// </summary>
        internal static string GetMethod {
            get
            {
                return "GET";
                return ResourceManager.GetString("GetMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;propfind xmlns=&quot;DAV:&quot;&gt;
        ///  &lt;prop&gt;
        ///    &lt;public_url xmlns=&quot;urn:yandex:disk:meta&quot; /&gt;
        ///    &lt;creationdate  /&gt;
        ///    &lt;getlastmodified/&gt;
        ///    &lt;displayname/&gt;
        ///    &lt;getcontentlength/&gt;
        ///    &lt;getcontenttype/&gt;
        ///    &lt;getetag/&gt;
        ///    &lt;resourcetype/&gt;
        ///  &lt;/prop&gt;
        ///&lt;/propfind&gt;.
        /// </summary>
        internal static string ItemDetailsBody {
            get
            {
                return @"&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
                          &lt;propfind xmlns=&quot;DAV:&quot;&gt;
                            &lt;prop&gt;
                              &lt;public_url xmlns=&quot;urn:yandex:disk:meta&quot; /&gt;
                              &lt;creationdate  /&gt;
                              &lt;getlastmodified/&gt;
                              &lt;displayname/&gt;
                              &lt;getcontentlength/&gt;
                              &lt;getcontenttype/&gt;
                              &lt;getetag/&gt;
                              &lt;resourcetype/&gt;
                            &lt;/prop&gt;
                          &lt;/propfind&gt;";
                return ResourceManager.GetString("ItemDetailsBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MKCOL.
        /// </summary>
        internal static string MakedirMethod {
            get
            {
                return "MKCOL";
                return ResourceManager.GetString("MakedirMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MOVE.
        /// </summary>
        internal static string MoveMethod {
            get
            {
                return "MOVE";
                return ResourceManager.GetString("MoveMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must be logged in before starting to work with SDK, try to call AuthorizeAsync(...) at first.
        /// </summary>
        internal static string NotAuthorizedErrorMessage {
            get
            {
                return
                    "You must be logged in before starting to work with SDK, try to call AuthorizeAsync(...) at first";
                return ResourceManager.GetString("NotAuthorizedErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The file is too large for uploading.
        /// </summary>
        internal static string OutOfMemoryErrorMessage {
            get {
                return "The file is too large for uploading";
                return ResourceManager.GetString("OutOfMemoryErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to POST.
        /// </summary>
        internal static string PostMethod {
            get {
                return "POST";
                return ResourceManager.GetString("PostMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PROPFIND.
        /// </summary>
        internal static string PropfindMethod {
            get
            {
                return "PROPFIND";
                return ResourceManager.GetString("PropfindMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PROPPATCH.
        /// </summary>
        internal static string ProppatchMethod {
            get
            {
                return "PROPPATCH";
                return ResourceManager.GetString("ProppatchMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t load SDK provider assembly. Add reference to the platform specific assembly &apos;Disk.SDK.Provider (platform)&apos; and try again..
        /// </summary>
        internal static string ProviderErrorMessage {
            get
            {
                return "Can&apos;t load SDK provider assembly. Add reference to the platform specific assembly &apos;Disk.SDK.Provider (platform)&apos; and try again.";
                return ResourceManager.GetString("ProviderErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;propertyupdate xmlns=&quot;DAV:&quot;&gt;
        ///  &lt;set&gt;
        ///    &lt;prop&gt;
        ///      &lt;public_url xmlns=&quot;urn:yandex:disk:meta&quot;&gt;true&lt;/public_url&gt;
        ///    &lt;/prop&gt;
        ///  &lt;/set&gt;
        ///&lt;/propertyupdate&gt;.
        /// </summary>
        internal static string PublishBody {
            get
            {
                return @"&lt;propertyupdate xmlns=&quot;DAV:&quot;&gt;
                          &lt;set&gt;
                            &lt;prop&gt;
                              &lt;public_url xmlns=&quot;urn:yandex:disk:meta&quot;&gt;true&lt;/public_url&gt;
                            &lt;/prop&gt;
                          &lt;/set&gt;
                        &lt;/propertyupdate&gt;";
                return ResourceManager.GetString("PublishBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PUT.
        /// </summary>
        internal static string PutMethod {
            get
            {
                return "PUT";
                return ResourceManager.GetString("PutMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ([a-zA-Z0-9]){32}.
        /// </summary>
        internal static string TokenRegexPattern {
            get
            {
                return @"([a-zA-Z0-9]){32}";
                return ResourceManager.GetString("TokenRegexPattern", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;propertyupdate xmlns=&quot;DAV:&quot;&gt;
        ///  &lt;remove&gt;
        ///    &lt;prop&gt;
        ///      &lt;public_url xmlns=&quot;urn:yandex:disk:meta&quot; /&gt;
        ///    &lt;/prop&gt;
        ///  &lt;/remove&gt;
        ///&lt;/propertyupdate&gt;.
        /// </summary>
        internal static string UnpublishBody {
            get
            {
                return @"&lt;propertyupdate xmlns=&quot;DAV:&quot;&gt;
                          &lt;remove&gt;
                            &lt;prop&gt;
                              &lt;public_url xmlns=&quot;urn:yandex:disk:meta&quot; /&gt;
                            &lt;/prop&gt;
                          &lt;/remove&gt;
                        &lt;/propertyupdate&gt;";
                return ResourceManager.GetString("UnpublishBody", resourceCulture);
            }
        }
    }
}
