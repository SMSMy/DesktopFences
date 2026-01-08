using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Desktop_Fences.Localization
{
    /// <summary>
    /// Manages application localization and language switching.
    /// Supports English (en) and Arabic (ar) languages.
    /// </summary>
    public static class LocalizationManager
    {
        private static string _currentLanguage = "en";
        private static readonly Dictionary<string, Dictionary<string, string>> _translations = new();

        /// <summary>
        /// Gets the current language code (en or ar)
        /// </summary>
        public static string CurrentLanguage => _currentLanguage;

        /// <summary>
        /// Gets whether the current language is RTL (Right-to-Left)
        /// </summary>
        public static bool IsRTL => _currentLanguage == "ar";

        /// <summary>
        /// Gets the FlowDirection for the current language
        /// </summary>
        public static FlowDirection FlowDirection => IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        /// <summary>
        /// Event raised when language changes
        /// </summary>
        public static event Action? LanguageChanged;

        /// <summary>
        /// Initialize the localization system
        /// </summary>
        static LocalizationManager()
        {
            LoadEnglishStrings();
            LoadArabicStrings();
        }

        /// <summary>
        /// Set the current language
        /// </summary>
        /// <param name="languageCode">Language code: "en" or "ar"</param>
        public static void SetLanguage(string languageCode)
        {
            if (languageCode != "en" && languageCode != "ar")
                languageCode = "en";

            if (_currentLanguage != languageCode)
            {
                _currentLanguage = languageCode;
                LanguageChanged?.Invoke();
            }
        }

        /// <summary>
        /// Get a localized string by key
        /// </summary>
        /// <param name="key">The string key</param>
        /// <returns>Localized string or key if not found</returns>
        public static string GetString(string key)
        {
            if (_translations.TryGetValue(_currentLanguage, out var langDict))
            {
                if (langDict.TryGetValue(key, out var value))
                    return value;
            }

            // Fallback to English
            if (_translations.TryGetValue("en", out var enDict))
            {
                if (enDict.TryGetValue(key, out var value))
                    return value;
            }

            return key; // Return key if not found
        }

        /// <summary>
        /// Shorthand for GetString
        /// </summary>
        public static string S(string key) => GetString(key);

        /// <summary>
        /// Get a formatted localized string
        /// </summary>
        public static string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return format;
            }
        }

        private static void LoadEnglishStrings()
        {
            _translations["en"] = new Dictionary<string, string>
            {
                // ===========================================
                // GENERAL / COMMON
                // ===========================================
                ["OK"] = "OK",
                ["Cancel"] = "Cancel",
                ["Yes"] = "Yes",
                ["No"] = "No",
                ["Error"] = "Error",
                ["Warning"] = "Warning",
                ["Information"] = "Information",
                ["Success"] = "Success",
                ["Close"] = "Close",
                ["Save"] = "Save",
                ["Apply"] = "Apply",
                ["Reset"] = "Reset",
                ["Delete"] = "Delete",
                ["Rename"] = "Rename",
                ["Edit"] = "Edit",
                ["Copy"] = "Copy",
                ["Cut"] = "Cut",
                ["Paste"] = "Paste",
                ["Browse"] = "Browse",
                ["Open"] = "Open",
                ["New"] = "New",
                ["Add"] = "Add",
                ["Remove"] = "Remove",
                ["Select"] = "Select",
                ["None"] = "None",
                ["All"] = "All",
                ["Default"] = "Default",
                ["Custom"] = "Custom",
                ["Enable"] = "Enable",
                ["Disable"] = "Disable",
                ["Enabled"] = "Enabled",
                ["Disabled"] = "Disabled",
                ["Settings"] = "Settings",
                ["Options"] = "Options",
                ["About"] = "About",
                ["Help"] = "Help",
                ["Exit"] = "Exit",
                ["Refresh"] = "Refresh",
                ["Reload"] = "Reload",

                // ===========================================
                // FENCE OPERATIONS
                // ===========================================
                ["NewFence"] = "New Fence",
                ["DeleteFence"] = "Delete Fence",
                ["RenameFence"] = "Rename Fence",
                ["HideFence"] = "Hide Fence",
                ["ShowFence"] = "Show Fence",
                ["LockFence"] = "Lock Fence",
                ["UnlockFence"] = "Unlock Fence",
                ["CustomizeFence"] = "Customize Fence",
                ["ExportFence"] = "Export Fence",
                ["ImportFence"] = "Import Fence",
                ["RestoreFence"] = "Restore Fence",
                ["DuplicateFence"] = "Duplicate Fence",
                ["CloseAllFences"] = "Close All Fences",
                ["ShowAllFences"] = "Show All Fences",
                ["HideAllFences"] = "Hide All Fences",
                ["ReloadFences"] = "Reload All Fences",
                ["ReloadingFences"] = "Reloading all fences, please wait...",
                ["FenceSettings"] = "Fence Settings",
                ["PeekBehind"] = "Peek Behind",
                ["RollUp"] = "Roll Up",
                ["RollDown"] = "Roll Down",
                ["SnapToDimension"] = "Snap to Dimension",
                ["ClearDeadShortcuts"] = "Clear Dead Shortcuts",
                ["CustomizeFence"] = "Customize Fence",
                ["Fence"] = "Fence",
                ["TitleSection"] = "Title",
                ["Default"] = "Default",
                ["Apply"] = "Apply",
                ["ExportAllToDesktop"] = "Export all icons to desktop",
                ["NameAfterPath"] = "Name Fence After Target Path",
                ["StartupTipsTitle"] = "Desktop Fences+ Startup Tips",
                ["StartupTipsContent"] = "WELCOME TO DESKTOP FENCES +\r\n---------------------------\r\n• Roll Up/Down: Double-click the fence title bar.\r\n• Rename: Ctrl + Click the title bar (Enter to save).\r\n• Search (SpotSearch): Press Ctrl + ` (Tilde) to find any icon instantly.\r\n• Options: Click the '♥' menu icon (top-left).\r\n• Reorder Icons on a Fence: Ctrl + Drag icon to new position.\r\n• Context Menu: Right-click icons or Fences for more options.\r\n \r\nTIP: Ctrl + Click or Ctrl + Right-click, gives even more options.\r\n\r\nTry customizing this fence! Right-click the title bar -> Customize...",

                // CustomizeFence Field Labels
                ["CustomColor"] = "Custom Color",
                ["CustomLaunchEffect"] = "Custom Launch Effect",
                ["FenceBorderColor"] = "Fence Border Color",
                ["FenceBorderThickness"] = "Fence Border Thickness",
                ["TitleTextColor"] = "Title Text Color",
                ["TitleTextSize"] = "Title Text Size",
                ["BoldTitleText"] = "Bold Title Text",
                ["IconSize"] = "Icon Size",
                ["IconSpacing"] = "Icon Spacing",
                ["TextColor"] = "Text Color",
                ["DisableTextShadow"] = "Disable Text Shadow",
                ["GrayscaleIcons"] = "Grayscale Icons",
                ["Customize"] = "Customize",

                // Colors
                ["Color"] = "Color",
                ["Color_Gray"] = "Gray",
                ["Color_Black"] = "Black",
                ["Color_White"] = "White",
                ["Color_Beige"] = "Beige",
                ["Color_Green"] = "Green",
                ["Color_Purple"] = "Purple",
                ["Color_Fuchsia"] = "Fuchsia",
                ["Color_Yellow"] = "Yellow",
                ["Color_Orange"] = "Orange",
                ["Color_Red"] = "Red",
                ["Color_Blue"] = "Blue",
                ["Color_Bismark"] = "Bismark",
                ["Color_Teal"] = "Teal",

                // Effects
                ["Effect"] = "Effect",
                ["Effect_Zoom"] = "Zoom",
                ["Effect_Bounce"] = "Bounce",
                ["Effect_FadeOut"] = "FadeOut",
                ["Effect_SlideUp"] = "SlideUp",
                ["Effect_Rotate"] = "Rotate",
                ["Effect_Agitate"] = "Agitate",
                ["Effect_GrowAndFly"] = "GrowAndFly",
                ["Effect_Pulse"] = "Pulse",
                ["Effect_Elastic"] = "Elastic",
                ["Effect_Flip3D"] = "Flip3D",
                ["Effect_Spiral"] = "Spiral",
                ["Effect_Shockwave"] = "Shockwave",
                ["Effect_Matrix"] = "Matrix",
                ["Effect_Supernova"] = "Supernova",
                ["Effect_Teleport"] = "Teleport",

                // Log Levels
                ["MinimumLogLevel"] = "Minimum Log Level",
                ["LogLevel_Debug"] = "Debug",
                ["LogLevel_Info"] = "Info",
                ["LogLevel_Warn"] = "Warn",
                ["LogLevel_Error"] = "Error",

                // Log Categories
                ["LogCat_General"] = "General",
                ["LogCat_FenceCreation"] = "FenceCreation",
                ["LogCat_FenceUpdate"] = "FenceUpdate",
                ["LogCat_IconHandling"] = "IconHandling",
                ["LogCat_ImportExport"] = "ImportExport",
                ["LogCat_Settings"] = "Settings",
                ["LogCat_BackgroundValidation"] = "BackgroundValidation",
                ["LogCat_Performance"] = "Performance",
                ["LogCat_UI"] = "UI",
                ["LogCat_Error"] = "Error",

                // ===========================================================================
                // TAB OPERATIONS
                // ===========================================
                ["NewTab"] = "New Tab",
                ["DeleteTab"] = "Delete Tab",
                ["RenameTab"] = "Rename Tab",
                ["ImportTab"] = "Import Tab",
                ["ExportTab"] = "Export Tab",
                ["MoveToTab"] = "Move to Tab",
                ["EnableTabsOnFence"] = "Enable Tabs On This Fence",

                // ===========================================
                // ICON/FILE OPERATIONS
                // ===========================================
                ["RunAsAdministrator"] = "Run as Administrator",
                ["AlwaysRunAsAdmin"] = "Always Run as Administrator",
                ["CopyPath"] = "Copy Path",
                ["FindTarget"] = "Find Target",
                ["OpenFolder"] = "Open Folder",
                ["SendToDesktop"] = "Send to Desktop",
                ["EditShortcut"] = "Edit Shortcut",
                ["DeleteShortcut"] = "Delete Shortcut",
                ["OpenWith"] = "Open With",
                ["Properties"] = "Properties",
                ["CopyItem"] = "Copy Item",
                ["CutItem"] = "Cut Item",
                ["RenameItem"] = "Rename Item",
                ["DeleteItem"] = "Delete Item",

                // ===========================================
                // PORTAL FENCE
                // ===========================================
                ["NewPortalFence"] = "New Portal Fence",
                ["PortalFenceTarget"] = "Portal Fence Target",
                ["OpenTargetFolder"] = "Open Target Folder",
                ["CopyTargetPath"] = "Copy Target Path",
                ["SetFilter"] = "Set Filter",
                ["ClearFilter"] = "Clear Filter",
                ["ShowHiddenFiles"] = "Show Hidden Files",
                ["HideHiddenFiles"] = "Hide Hidden Files",
                ["NavigateUp"] = "Navigate Up",
                ["NavigateBack"] = "Navigate Back",

                // ===========================================
                // NOTE FENCE
                // ===========================================
                ["NewNoteFence"] = "New Note Fence",
                ["ClearNote"] = "Clear Note",
                ["FormatText"] = "Format Text",
                ["Bold"] = "Bold",
                ["Italic"] = "Italic",
                ["Underline"] = "Underline",
                ["FontSize"] = "Font Size",
                ["FontColor"] = "Font Color",
                ["CopyAllText"] = "Copy All Text",
                ["ClearAllText"] = "Clear All Text",

                // ===========================================
                // OPTIONS / SETTINGS
                // ===========================================
                ["GeneralSettings"] = "General",
                ["AppearanceSettings"] = "Appearance",
                ["BehaviorSettings"] = "Behavior",
                ["BackupSettings"] = "Backup",
                ["AdvancedSettings"] = "Advanced",
                ["Tools"] = "Tools",
                ["Language"] = "Language",
                ["English"] = "English",
                ["Arabic"] = "العربية",
                ["StartWithWindows"] = "Start with Windows",
                ["ShowTrayIcon"] = "Show Tray Icon",
                ["HideTrayIcon"] = "Hide Tray Icon",
                ["EnableSound"] = "Enable Sound",
                ["DisableSound"] = "Disable Sound",
                ["EnableSnap"] = "Enable Snap",
                ["DisableSnap"] = "Disable Snap",
                ["SnapNearFences"] = "Snap Near Fences",
                ["TintLevel"] = "Tint Level",
                ["BaseColor"] = "Base Color",
                ["Transparency"] = "Transparency",
                ["IconSize"] = "Icon Size",
                ["IconSpacing"] = "Icon Spacing",
                ["ShowScrollbars"] = "Show Scrollbars",
                ["HideScrollbars"] = "Hide Scrollbars",
                ["EnableLogging"] = "Enable Logging",
                ["ViewLog"] = "View Log",
                ["ClearLog"] = "Clear Log",
                ["CreateBackup"] = "Create Backup",
                ["RestoreBackup"] = "Restore Backup",
                ["AutoBackup"] = "Auto Backup",
                ["DailyBackup"] = "Daily Backup",
                ["FactoryReset"] = "Factory Reset",
                ["ResetAll"] = "Reset All",
                ["ClearAllData"] = "Clear All Data",

                // Options Tab Content
                ["Startup"] = "Startup",
                ["Behavior"] = "Behavior",
                ["Choices"] = "Choices",
                ["Appearance"] = "Appearance",
                ["Icons"] = "Icons",
                ["Reset"] = "Reset",
                ["Log"] = "Log",
                ["LogConfiguration"] = "Log Configuration",
                ["LogCategories"] = "Log Categories",
                ["SingleClickToLaunch"] = "Single Click to Launch",
                ["EnableSnapNearFences"] = "Enable Snap Near Fences",
                ["EnableDimensionSnap"] = "Enable Dimension Snap",
                ["EnableTrayIcon"] = "Enable Tray Icon",
                ["UseRecycleBin"] = "Use Recycle Bin on Portal Delete",
                ["EnablePortalWatermark"] = "Enable Portal Fences Watermark",
                ["EnableNoteWatermark"] = "Enable Note Fences Watermark",
                ["DisableScrollbars"] = "Disable Fence Scrollbars",
                ["EnableSounds"] = "Enable Sounds",
                ["FenceTint"] = "Fence Tint",
                ["MenuTint"] = "Menu Tint",
                ["MenuIcon"] = "Menu Icon",
                ["LockIcon"] = "Lock Icon",
                ["Backup"] = "Backup",
                ["Restore"] = "Restore",
                ["OpenBackupsFolder"] = "Open Backups Folder",
                ["AutoBackupDaily"] = "Automatic Backup (Daily)",
                ["ResetStyles"] = "Reset Styles",
                ["ResetStylesConfirm"] = "Reset all visual customizations?",
                ["OpenLog"] = "Open Log",

                // ===========================================================================
                // LAUNCH EFFECTS
                // ===========================================
                ["LaunchEffect"] = "Launch Effect",
                ["EffectNone"] = "None",
                ["EffectZoom"] = "Zoom",
                ["EffectBounce"] = "Bounce",
                ["EffectFadeout"] = "Fadeout",
                ["EffectSlideUp"] = "Slide Up",
                ["EffectRotate"] = "Rotate",
                ["EffectAgitate"] = "Agitate",
                ["EffectElastic"] = "Elastic",
                ["SelectEffect"] = "Select Effect",

                // ===========================================
                // COLORS
                // ===========================================
                ["SelectColor"] = "Select Color",
                ["BackgroundColor"] = "Background Color",
                ["TitleColor"] = "Title Color",
                ["BorderColor"] = "Border Color",
                ["TextColor"] = "Text Color",

                // ===========================================
                // TRAY MENU
                // ===========================================
                ["ShowDesktop"] = "Show Desktop",
                ["ShowHiddenFences"] = "Show Hidden Fences",
                ["Backup"] = "Backup",
                ["Restore"] = "Restore",
                ["CheckForUpdates"] = "Check for Updates",
                ["AboutDesktopFences"] = "About Desktop Fences+",
                ["ExitApplication"] = "Exit",

                // ===========================================
                // ABOUT DIALOG
                // ===========================================
                ["Version"] = "Version",
                ["Developer"] = "Developer",
                ["Website"] = "Website",
                ["Donate"] = "Donate",
                ["License"] = "License",
                ["Credits"] = "Credits",
                ["OriginalAuthor"] = "Original Author",
                ["EnhancedBy"] = "Enhanced by",
                ["AIEnhanced"] = "AI-Enhanced Updates",
                ["SupportDevelopment"] = "Support Development",
                ["DonateViaPayPal"] = "♥ Donate via PayPal",
                ["VisitGitHub"] = "⚡ Visit GitHub",

                // ===========================================
                // SEARCH
                // ===========================================
                ["Search"] = "Search",
                ["SearchPlaceholder"] = "Search shortcuts...",
                ["NoResults"] = "No results found",
                ["SearchIn"] = "Search in",
                ["AllFences"] = "All Fences",
                ["AlwaysOnTop"] = "Always On Top",
                ["Theme"] = "Theme",
                ["UseWallpaperColors"] = "Use wallpaper colors for accents",
                ["FenceTransparency"] = "Fence Transparency",
                ["ChooseCustomColor"] = "Choose Custom Fence Color",
                ["Preview"] = "Preview",
                ["Light"] = "Light",
                ["Dark"] = "Dark",
                ["System"] = "System",
                ["Wallpaper"] = "Wallpaper",

                // ===========================================
                // MESSAGES
                // ===========================================
                ["ConfirmDelete"] = "Are you sure you want to delete this?",
                ["ConfirmDeleteFence"] = "Are you sure you want to delete this fence?",
                ["ConfirmDeleteShortcut"] = "Are you sure you want to delete this shortcut?",
                ["ConfirmFactoryReset"] = "WARNING: This will delete ALL fences, shortcuts, and settings!\n\nAre you sure you want to proceed?",
                ["ConfirmClearNote"] = "Are you sure you want to clear all text from this note?",
                ["ConfirmResetCustomizations"] = "Reset all visual customizations?",
                ["FenceCreated"] = "Fence created successfully",
                ["FenceDeleted"] = "Fence deleted successfully",
                ["BackupCreated"] = "Backup created successfully",
                ["BackupRestored"] = "Backup restored successfully",
                ["SettingsSaved"] = "Settings saved successfully",
                ["ErrorOccurred"] = "An error occurred: {0}",
                ["FileNotFound"] = "File not found: {0}",
                ["FolderNotFound"] = "Folder not found: {0}",
                ["TargetNotFound"] = "Target not found",
                ["InvalidPath"] = "Invalid path",
                ["AccessDenied"] = "Access denied",
                ["OperationFailed"] = "Operation failed: {0}",
                ["LaunchError"] = "Launch Error",
                ["SaveError"] = "Save Error",
                ["LoadError"] = "Load Error",
                ["RenameError"] = "Rename Error",
                ["CopyError"] = "Copy Error",
                ["DeleteError"] = "Delete Error",
                ["RestoreError"] = "Restore Error",
                ["FormError"] = "Form Error",
                ["NavigationError"] = "Navigation Error",
                ["FileExists"] = "A file or folder with that name already exists.",
                ["UnableToCopyPath"] = "Unable to copy path.",
                ["UnableToMoveToRecycleBin"] = "Unable to move item to recycle bin.",
                ["UnableToDelete"] = "Unable to delete item.",
                ["LogNotFound"] = "Log file not found.",
                ["ErrorReloadingFences"] = "An error occurred while reloading fences: {0}",
                ["ErrorInitForm"] = "Error initializing form: {0}",
                ["ErrorApplyChanges"] = "Error applying changes: {0}",
                ["ErrorSaveChanges"] = "Error saving changes: {0}",
                ["ErrorLaunch"] = "Error launching: {0}",
                ["RestoreFailed"] = "Restore failed: {0}",
                ["ResetFailed"] = "Reset failed: {0}",
                ["CouldNotNavigate"] = "Could not navigate to folder.\n{0}",

                // ===========================================
                // CUSTOMIZE FENCE FORM
                // ===========================================
                ["FenceCustomization"] = "Fence Customization",
                ["FenceName"] = "Fence Name",
                ["FenceSize"] = "Fence Size",
                ["Width"] = "Width",
                ["Height"] = "Height",
                ["Position"] = "Position",
                ["XPosition"] = "X Position",
                ["YPosition"] = "Y Position",
                ["ShowTitle"] = "Show Title",
                ["HideTitle"] = "Hide Title",
                ["TitleHeight"] = "Title Height",
                ["CornerRadius"] = "Corner Radius",
                ["Preview"] = "Preview",
                ["ApplyToAll"] = "Apply to All",

                // ===========================================
                // THEME
                // ===========================================
                ["Theme"] = "Theme",
                ["LightTheme"] = "Light",
                ["DarkTheme"] = "Dark",
                ["CustomTheme"] = "Custom",
                ["SystemTheme"] = "System Default",
            };
        }

        private static void LoadArabicStrings()
        {
            _translations["ar"] = new Dictionary<string, string>
            {
                // ===========================================
                // عام / مشترك
                // ===========================================
                ["OK"] = "موافق",
                ["Cancel"] = "إلغاء",
                ["Yes"] = "نعم",
                ["No"] = "لا",
                ["Error"] = "خطأ",
                ["Warning"] = "تحذير",
                ["Information"] = "معلومات",
                ["Success"] = "نجاح",
                ["Close"] = "إغلاق",
                ["Save"] = "حفظ",
                ["Apply"] = "تطبيق",
                ["Reset"] = "إعادة تعيين",
                ["Delete"] = "حذف",
                ["Rename"] = "إعادة تسمية",
                ["Edit"] = "تحرير",
                ["Copy"] = "نسخ",
                ["Cut"] = "قص",
                ["Paste"] = "لصق",
                ["Browse"] = "استعراض",
                ["Open"] = "فتح",
                ["New"] = "جديد",
                ["Add"] = "إضافة",
                ["Remove"] = "إزالة",
                ["Select"] = "تحديد",
                ["None"] = "لا شيء",
                ["All"] = "الكل",
                ["Default"] = "افتراضي",
                ["Custom"] = "مخصص",
                ["Enable"] = "تفعيل",
                ["Disable"] = "تعطيل",
                ["Enabled"] = "مُفعَّل",
                ["Disabled"] = "مُعطَّل",
                ["Settings"] = "الإعدادات",
                ["Options"] = "خيارات",
                ["About"] = "حول",
                ["Help"] = "مساعدة",
                ["Exit"] = "خروج",
                ["Refresh"] = "تحديث",
                ["Reload"] = "إعادة تحميل",

                // ===========================================
                // عمليات الإطار
                // ===========================================
                ["NewFence"] = "إطار جديد",
                ["DeleteFence"] = "حذف الإطار",
                ["RenameFence"] = "إعادة تسمية الإطار",
                ["HideFence"] = "إخفاء الإطار",
                ["ShowFence"] = "إظهار الإطار",
                ["LockFence"] = "قفل الإطار",
                ["UnlockFence"] = "فتح قفل الإطار",
                ["CustomizeFence"] = "تخصيص الإطار",
                ["ExportFence"] = "تصدير الإطار",
                ["ImportFence"] = "استيراد إطار",
                ["RestoreFence"] = "استعادة الإطار",
                ["DuplicateFence"] = "تكرار الإطار",
                ["CloseAllFences"] = "إغلاق جميع الإطارات",
                ["ShowAllFences"] = "إظهار جميع الإطارات",
                ["HideAllFences"] = "إخفاء جميع الإطارات",
                ["ReloadFences"] = "إعادة تحميل الإطارات",
                ["ReloadingFences"] = "جاري إعادة تحميل الإطارات، يرجى الانتظار...",
                ["FenceSettings"] = "إعدادات الإطار",
                ["PeekBehind"] = "إلقاء نظرة خلف",
                ["RollUp"] = "طي للأعلى",
                ["RollDown"] = "فتح للأسفل",
                ["SnapToDimension"] = "محاذاة للأبعاد",
                ["ClearDeadShortcuts"] = "حذف الاختصارات التالفة",
                ["Fence"] = "الإطار",
                ["TitleSection"] = "العنوان",
                ["Default"] = "افتراضي",
                ["Apply"] = "تطبيق",
                ["ExportAllToDesktop"] = "تصدير جميع الأيقونات للسطح",
                ["NameAfterPath"] = "تسمية الإطار حسب المسار",
                ["StartupTipsTitle"] = "نصائح بدء Desktop Fences+",
                ["StartupTipsContent"] = "مرحباً بك في Desktop Fences+! انقر نقراً مزدوجاً لطي. Ctrl+انقر للتسمية.",

                // CustomizeFence Field Labels
                ["CustomColor"] = "لون مخصص",
                ["CustomLaunchEffect"] = "تأثير تشغيل مخصص",
                ["FenceBorderColor"] = "لون حدود الإطار",
                ["FenceBorderThickness"] = "سُمك حدود الإطار",
                ["TitleTextColor"] = "لون نص العنوان",
                ["TitleTextSize"] = "حجم نص العنوان",
                ["BoldTitleText"] = "عنوان غامق",
                ["IconSize"] = "حجم الأيقونة",
                ["IconSpacing"] = "تباعد الأيقونات",
                ["TextColor"] = "لون النص",
                ["DisableTextShadow"] = "تعطيل ظل النص",
                ["GrayscaleIcons"] = "أيقونات رمادية",
                ["Customize"] = "تخصيص",

                // الألوان
                ["Color"] = "اللون",
                ["Color_Gray"] = "رمادي",
                ["Color_Black"] = "أسود",
                ["Color_White"] = "أبيض",
                ["Color_Beige"] = "بيج",
                ["Color_Green"] = "أخضر",
                ["Color_Purple"] = "بنفسجي",
                ["Color_Fuchsia"] = "فوشي",
                ["Color_Yellow"] = "أصفر",
                ["Color_Orange"] = "برتقالي",
                ["Color_Red"] = "أحمر",
                ["Color_Blue"] = "أزرق",
                ["Color_Bismark"] = "بيزمارك",
                ["Color_Teal"] = "أزرق مخضر",

                // التأثيرات
                ["Effect"] = "التأثير",
                ["Effect_Zoom"] = "تكبير",
                ["Effect_Bounce"] = "ارتداد",
                ["Effect_FadeOut"] = "تلاشي",
                ["Effect_SlideUp"] = "انزلاق للأعلى",
                ["Effect_Rotate"] = "دوران",
                ["Effect_Agitate"] = "اهتزاز",
                ["Effect_GrowAndFly"] = "نمو وطيران",
                ["Effect_Pulse"] = "نبض",
                ["Effect_Elastic"] = "مرن",
                ["Effect_Flip3D"] = "انقلاب ثلاثي الأبعاد",
                ["Effect_Spiral"] = "حلزوني",
                ["Effect_Shockwave"] = "موجة صدمة",
                ["Effect_Matrix"] = "ماتريكس",
                ["Effect_Supernova"] = "سوبرنوفا",
                ["Effect_Teleport"] = "انتقال آني",

                // مستويات السجل
                ["MinimumLogLevel"] = "أدنى مستوى للسجل",
                ["LogLevel_Debug"] = "تصحيح",
                ["LogLevel_Info"] = "معلومات",
                ["LogLevel_Warn"] = "تحذير",
                ["LogLevel_Error"] = "خطأ",

                // فئات السجل
                ["LogCat_General"] = "عام",
                ["LogCat_FenceCreation"] = "إنشاء الإطارات",
                ["LogCat_FenceUpdate"] = "تحديث الإطارات",
                ["LogCat_IconHandling"] = "معالجة الأيقونات",
                ["LogCat_ImportExport"] = "استيراد/تصدير",
                ["LogCat_Settings"] = "الإعدادات",
                ["LogCat_BackgroundValidation"] = "التحقق الخلفي",
                ["LogCat_Performance"] = "الأداء",
                ["LogCat_UI"] = "واجهة المستخدم",
                ["LogCat_Error"] = "الأخطاء",

                // ===========================================
                // عمليات التبويبات
                // ===========================================
                ["NewTab"] = "تبويب جديد",
                ["DeleteTab"] = "حذف التبويب",
                ["RenameTab"] = "إعادة تسمية التبويب",
                ["ImportTab"] = "استيراد تبويب",
                ["ExportTab"] = "تصدير التبويب",
                ["MoveToTab"] = "نقل إلى تبويب",
                ["EnableTabsOnFence"] = "تفعيل التبويبات على هذا الإطار",

                // ===========================================
                // عمليات الأيقونات/الملفات
                // ===========================================
                ["RunAsAdministrator"] = "تشغيل كمسؤول",
                ["AlwaysRunAsAdmin"] = "تشغيل كمسؤول دائماً",
                ["CopyPath"] = "نسخ المسار",
                ["FindTarget"] = "البحث عن الهدف",
                ["OpenFolder"] = "فتح المجلد",
                ["SendToDesktop"] = "إرسال إلى سطح المكتب",
                ["EditShortcut"] = "تحرير الاختصار",
                ["DeleteShortcut"] = "حذف الاختصار",
                ["OpenWith"] = "فتح بواسطة",
                ["Properties"] = "خصائص",
                ["CopyItem"] = "نسخ العنصر",
                ["CutItem"] = "قص العنصر",
                ["RenameItem"] = "إعادة تسمية العنصر",
                ["DeleteItem"] = "حذف العنصر",

                // ===========================================
                // إطار البوابة
                // ===========================================
                ["NewPortalFence"] = "إطار بوابة جديد",
                ["PortalFenceTarget"] = "هدف إطار البوابة",
                ["OpenTargetFolder"] = "فتح المجلد الهدف",
                ["CopyTargetPath"] = "نسخ مسار الهدف",
                ["SetFilter"] = "تعيين فلتر",
                ["ClearFilter"] = "مسح الفلتر",
                ["ShowHiddenFiles"] = "إظهار الملفات المخفية",
                ["HideHiddenFiles"] = "إخفاء الملفات المخفية",
                ["NavigateUp"] = "الانتقال للأعلى",
                ["NavigateBack"] = "رجوع",

                // ===========================================
                // إطار الملاحظات
                // ===========================================
                ["NewNoteFence"] = "إطار ملاحظات جديد",
                ["ClearNote"] = "مسح الملاحظة",
                ["FormatText"] = "تنسيق النص",
                ["Bold"] = "غامق",
                ["Italic"] = "مائل",
                ["Underline"] = "تسطير",
                ["FontSize"] = "حجم الخط",
                ["FontColor"] = "لون الخط",
                ["CopyAllText"] = "نسخ كل النص",
                ["ClearAllText"] = "مسح كل النص",
                ["AlwaysOnTop"] = "فوق الكل دائماً",
                ["Theme"] = "السمة",
                ["UseWallpaperColors"] = "استخدام ألوان الخلفية للتمييز",
                ["FenceTransparency"] = "شفافية الإطار",
                ["ChooseCustomColor"] = "اختيار لون مخصص للإطار",
                ["Preview"] = "معاينة",
                ["Light"] = "فاتح",
                ["Dark"] = "داكن",
                ["System"] = "النظام",
                ["Wallpaper"] = "الخلفية",

                // ===========================================
                // الخيارات / الإعدادات
                // ===========================================
                ["GeneralSettings"] = "عام",
                ["AppearanceSettings"] = "المظهر",
                ["BehaviorSettings"] = "السلوك",
                ["BackupSettings"] = "النسخ الاحتياطي",
                ["AdvancedSettings"] = "متقدم",
                ["Tools"] = "أدوات",
                ["Language"] = "اللغة",
                ["English"] = "English",
                ["Arabic"] = "العربية",
                ["StartWithWindows"] = "بدء مع ويندوز",
                ["ShowTrayIcon"] = "إظهار أيقونة الدرج",
                ["HideTrayIcon"] = "إخفاء أيقونة الدرج",
                ["EnableSound"] = "تفعيل الصوت",
                ["DisableSound"] = "تعطيل الصوت",
                ["EnableSnap"] = "تفعيل المحاذاة",
                ["DisableSnap"] = "تعطيل المحاذاة",
                ["SnapNearFences"] = "المحاذاة بالقرب من الإطارات",
                ["TintLevel"] = "مستوى التلوين",
                ["BaseColor"] = "اللون الأساسي",
                ["Transparency"] = "الشفافية",
                ["IconSize"] = "حجم الأيقونة",
                ["IconSpacing"] = "تباعد الأيقونات",
                ["ShowScrollbars"] = "إظهار أشرطة التمرير",
                ["HideScrollbars"] = "إخفاء أشرطة التمرير",
                ["EnableLogging"] = "تفعيل السجل",
                ["ViewLog"] = "عرض السجل",
                ["ClearLog"] = "مسح السجل",
                ["CreateBackup"] = "إنشاء نسخة احتياطية",
                ["RestoreBackup"] = "استعادة نسخة احتياطية",
                ["AutoBackup"] = "نسخ احتياطي تلقائي",
                ["DailyBackup"] = "نسخ احتياطي يومي",
                ["FactoryReset"] = "إعادة ضبط المصنع",
                ["ResetAll"] = "إعادة تعيين الكل",
                ["ClearAllData"] = "مسح جميع البيانات",

                // Options Tab Content
                ["Startup"] = "بدء التشغيل",
                ["Behavior"] = "السلوك",
                ["Choices"] = "الخيارات",
                ["Appearance"] = "المظهر",
                ["Icons"] = "الأيقونات",
                ["Reset"] = "إعادة تعيين",
                ["Log"] = "السجل",
                ["LogConfiguration"] = "إعدادات السجل",
                ["LogCategories"] = "فئات السجل",
                ["SingleClickToLaunch"] = "نقرة واحدة للتشغيل",
                ["EnableSnapNearFences"] = "تفعيل المحاذاة بالقرب من الإطارات",
                ["EnableDimensionSnap"] = "تفعيل محاذاة الأبعاد",
                ["EnableTrayIcon"] = "تفعيل أيقونة الدرج",
                ["UseRecycleBin"] = "استخدام سلة المحذوفات للحذف",
                ["EnablePortalWatermark"] = "تفعيل علامة إطارات البوابة",
                ["EnableNoteWatermark"] = "تفعيل علامة إطارات الملاحظات",
                ["DisableScrollbars"] = "تعطيل أشرطة التمرير",
                ["EnableSounds"] = "تفعيل الأصوات",
                ["FenceTint"] = "تلوين الإطار",
                ["MenuTint"] = "تلوين القائمة",
                ["MenuIcon"] = "أيقونة القائمة",
                ["LockIcon"] = "أيقونة القفل",
                ["Backup"] = "نسخ احتياطي",
                ["Restore"] = "استعادة",
                ["OpenBackupsFolder"] = "فتح مجلد النسخ الاحتياطي",
                ["AutoBackupDaily"] = "نسخ احتياطي تلقائي (يومي)",
                ["ResetStyles"] = "إعادة تعيين الأنماط",
                ["ResetStylesConfirm"] = "إعادة تعيين جميع التخصيصات المرئية؟",
                ["OpenLog"] = "فتح السجل",

                // ===========================================
                // تأثيرات الإطلاق
                // ===========================================
                ["LaunchEffect"] = "تأثير الإطلاق",
                ["EffectNone"] = "بدون",
                ["EffectZoom"] = "تكبير",
                ["EffectBounce"] = "ارتداد",
                ["EffectFadeout"] = "تلاشي",
                ["EffectSlideUp"] = "انزلاق للأعلى",
                ["EffectRotate"] = "دوران",
                ["EffectAgitate"] = "اهتزاز",
                ["EffectElastic"] = "مرن",
                ["SelectEffect"] = "اختر التأثير",

                // ===========================================
                // الألوان
                // ===========================================
                ["SelectColor"] = "اختر اللون",
                ["BackgroundColor"] = "لون الخلفية",
                ["TitleColor"] = "لون العنوان",
                ["BorderColor"] = "لون الحدود",
                ["TextColor"] = "لون النص",

                // ===========================================
                // قائمة الدرج
                // ===========================================
                ["ShowDesktop"] = "إظهار سطح المكتب",
                ["ShowHiddenFences"] = "إظهار الإطارات المخفية",
                ["Backup"] = "نسخ احتياطي",
                ["Restore"] = "استعادة",
                ["CheckForUpdates"] = "البحث عن تحديثات",
                ["AboutDesktopFences"] = "حول Desktop Fences+",
                ["ExitApplication"] = "خروج",

                // ===========================================
                // نافذة حول
                // ===========================================
                ["Version"] = "الإصدار",
                ["Developer"] = "المطور",
                ["Website"] = "الموقع",
                ["Donate"] = "تبرع",
                ["License"] = "الترخيص",
                ["Credits"] = "الاعتمادات",
                ["OriginalAuthor"] = "المؤلف الأصلي",
                ["EnhancedBy"] = "تحسين بواسطة",
                ["AIEnhanced"] = "تحديثات مُحسّنة بالذكاء الاصطناعي",
                ["SupportDevelopment"] = "دعم التطوير",
                ["DonateViaPayPal"] = "♥ تبرع عبر PayPal",
                ["VisitGitHub"] = "⚡ زيارة GitHub",

                // ===========================================
                // البحث
                // ===========================================
                ["Search"] = "بحث",
                ["SearchPlaceholder"] = "البحث في الاختصارات...",
                ["NoResults"] = "لم يتم العثور على نتائج",
                ["SearchIn"] = "البحث في",
                ["AllFences"] = "جميع الإطارات",

                // ===========================================
                // الرسائل
                // ===========================================
                ["ConfirmDelete"] = "هل أنت متأكد من رغبتك في الحذف؟",
                ["ConfirmDeleteFence"] = "هل أنت متأكد من رغبتك في حذف هذا الإطار؟",
                ["ConfirmDeleteShortcut"] = "هل أنت متأكد من رغبتك في حذف هذا الاختصار؟",
                ["ConfirmFactoryReset"] = "تحذير: سيؤدي هذا إلى حذف جميع الإطارات والاختصارات والإعدادات!\n\nهل أنت متأكد من رغبتك في المتابعة؟",
                ["ConfirmClearNote"] = "هل أنت متأكد من رغبتك في مسح كل النص من هذه الملاحظة؟",
                ["ConfirmResetCustomizations"] = "إعادة تعيين جميع التخصيصات المرئية؟",
                ["FenceCreated"] = "تم إنشاء الإطار بنجاح",
                ["FenceDeleted"] = "تم حذف الإطار بنجاح",
                ["BackupCreated"] = "تم إنشاء النسخة الاحتياطية بنجاح",
                ["BackupRestored"] = "تم استعادة النسخة الاحتياطية بنجاح",
                ["SettingsSaved"] = "تم حفظ الإعدادات بنجاح",
                ["ErrorOccurred"] = "حدث خطأ: {0}",
                ["FileNotFound"] = "الملف غير موجود: {0}",
                ["FolderNotFound"] = "المجلد غير موجود: {0}",
                ["TargetNotFound"] = "الهدف غير موجود",
                ["InvalidPath"] = "مسار غير صالح",
                ["AccessDenied"] = "تم رفض الوصول",
                ["OperationFailed"] = "فشلت العملية: {0}",
                ["LaunchError"] = "خطأ في التشغيل",
                ["SaveError"] = "خطأ في الحفظ",
                ["LoadError"] = "خطأ في التحميل",
                ["RenameError"] = "خطأ في إعادة التسمية",
                ["CopyError"] = "خطأ في النسخ",
                ["DeleteError"] = "خطأ في الحذف",
                ["RestoreError"] = "خطأ في الاستعادة",
                ["FormError"] = "خطأ في النموذج",
                ["NavigationError"] = "خطأ في التنقل",
                ["FileExists"] = "يوجد ملف أو مجلد بهذا الاسم بالفعل.",
                ["UnableToCopyPath"] = "تعذر نسخ المسار.",
                ["UnableToMoveToRecycleBin"] = "تعذر نقل العنصر إلى سلة المحذوفات.",
                ["UnableToDelete"] = "تعذر حذف العنصر.",
                ["LogNotFound"] = "ملف السجل غير موجود.",
                ["ErrorReloadingFences"] = "حدث خطأ أثناء إعادة تحميل الإطارات: {0}",
                ["ErrorInitForm"] = "خطأ في تهيئة النموذج: {0}",
                ["ErrorApplyChanges"] = "خطأ في تطبيق التغييرات: {0}",
                ["ErrorSaveChanges"] = "خطأ في حفظ التغييرات: {0}",
                ["ErrorLaunch"] = "خطأ في التشغيل: {0}",
                ["RestoreFailed"] = "فشلت الاستعادة: {0}",
                ["ResetFailed"] = "فشلت إعادة التعيين: {0}",
                ["CouldNotNavigate"] = "تعذر الانتقال إلى المجلد.\n{0}",

                // ===========================================
                // نموذج تخصيص الإطار
                // ===========================================
                ["FenceCustomization"] = "تخصيص الإطار",
                ["FenceName"] = "اسم الإطار",
                ["FenceSize"] = "حجم الإطار",
                ["Width"] = "العرض",
                ["Height"] = "الارتفاع",
                ["Position"] = "الموضع",
                ["XPosition"] = "الموضع الأفقي",
                ["YPosition"] = "الموضع العمودي",
                ["ShowTitle"] = "إظهار العنوان",
                ["HideTitle"] = "إخفاء العنوان",
                ["TitleHeight"] = "ارتفاع العنوان",
                ["CornerRadius"] = "نصف قطر الزوايا",
                ["Preview"] = "معاينة",
                ["ApplyToAll"] = "تطبيق على الكل",

                // ===========================================
                // الثيم
                // ===========================================
                ["Theme"] = "الثيم",
                ["LightTheme"] = "فاتح",
                ["DarkTheme"] = "داكن",
                ["CustomTheme"] = "مخصص",
                ["SystemTheme"] = "افتراضي النظام",
            };
        }
    }
}
