﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Brio.Docs.Launcher.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class LocalizationResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LocalizationResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Brio.Docs.Launcher.Resources.LocalizationResources", typeof(LocalizationResources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Документооборот запущен.
        /// </summary>
        public static string Label_DMStarted {
            get {
                return ResourceManager.GetString("Label_DMStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Документооборот остановлен.
        /// </summary>
        public static string Label_DMStopped {
            get {
                return ResourceManager.GetString("Label_DMStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Выход.
        /// </summary>
        public static string Label_Exit {
            get {
                return ResourceManager.GetString("Label_Exit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cкрыть консоль.
        /// </summary>
        public static string Label_HideConsole {
            get {
                return ResourceManager.GetString("Label_HideConsole", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Показать консоль.
        /// </summary>
        public static string Label_ShowConsole {
            get {
                return ResourceManager.GetString("Label_ShowConsole", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Открыть файл настроек.
        /// </summary>
        public static string Label_ShowSettings {
            get {
                return ResourceManager.GetString("Label_ShowSettings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Открыть Swagger.
        /// </summary>
        public static string Label_ShowSwagger {
            get {
                return ResourceManager.GetString("Label_ShowSwagger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Запустить документооборот.
        /// </summary>
        public static string Label_StartDM {
            get {
                return ResourceManager.GetString("Label_StartDM", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Необходимо указать путь к &apos;Brio.Docs.Api.exe&apos;!.
        /// </summary>
        public static string Message_Path_not_found {
            get {
                return ResourceManager.GetString("Message_Path_not_found", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Файл не найден: {0}.
        /// </summary>
        public static string MessageFormat_File_not_found {
            get {
                return ResourceManager.GetString("MessageFormat_File_not_found", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Документооборот BRIO MRS.
        /// </summary>
        public static string Tooltip_Dm_Brio_Mrs {
            get {
                return ResourceManager.GetString("Tooltip_Dm_Brio_Mrs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Остановить документооборот и закрыть приложение.
        /// </summary>
        public static string Tooltip_StopDmAndExit {
            get {
                return ResourceManager.GetString("Tooltip_StopDmAndExit", resourceCulture);
            }
        }
    }
}
