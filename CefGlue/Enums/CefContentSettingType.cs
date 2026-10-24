﻿//
// This file manually written from cef/include/internal/cef_types_content_settings.h.
// C API name: cef_content_setting_types_t.
//
namespace Xilium.CefGlue
{
    /// <summary>
    /// Supported content setting types. Some types are platform-specific or only
    /// supported with the Chrome runtime. Should be kept in sync with Chromium's
    /// ContentSettingsType type.
    /// </summary>
    public enum CefContentSettingType
    {
        /// This setting governs whether cookies are enabled by the user in the
        /// provided context. However, it may be overridden by other settings. This
        /// enum should NOT be read directly to determine whether cookies are enabled;
        /// the client should instead rely on the CookieSettings API.
        Cookies = 0,
        Images = 1,
        JavaScript = 2,

        /// This setting governs both popups and unwanted redirects like tab-unders
        /// and framebusting.
        Popups = 3,

        Geolocation = 4,
        Notifications = 5,
        AutoSelectCertificate = 6,
        MixedScript = 7,
        MediaStreamMic = 8,
        MediaStreamCamera = 9,
        ProtocolHandlers = 10,
        DeprecatedPpapiBroker = 11,
        AutomaticDownloads = 12,

        /// Advanced device-specific functions on MIDI devices. MIDI-SysEx
        /// communications can be used for changing the MIDI device's persistent state
        /// such as firmware.
        MidiSysEx = 13,

        SslCertDecisions = 14,
        ProtectedMediaIdentifier = 15,
        AppBanner = 16,
        SiteEngagement = 17,
        PersistentStorage = 18,
        UsbChooserData = 19,
        BluetoothGuard = 20,
        BackgroundSync = 21,
        Autoplay = 22,
        ImportantSiteInfo = 23,
        PermissionAutoBlockerData = 24,
        Ads = 25,

        /// Website setting which stores metadata for the subresource filter to aid in
        /// decisions for whether or not to show the UI.
        AdsData = 26,

        /// MIDI stands for Musical Instrument Digital Interface. It is a standard
        /// that allows electronic musical instruments, computers, and other devices
        /// to communicate with each other.
        Midi = 27,

        /// This content setting type is for caching password protection service's
        /// verdicts of each origin.
        PasswordProtection = 28,

        /// Website setting which stores engagement data for media related to a
        /// specific origin.
        MediaEngagement = 29,

        /// Content setting which stores whether or not the site can play audible
        /// sound. This will not block playback but instead the user will not hear it.
        Sound = 30,

        /// Website setting which stores the list of client hints that the origin
        /// requested the browser to remember. The browser is expected to send all
        /// client hints in the HTTP request headers for every resource requested
        /// from that origin.
        ClientHints = 31,

        /// Setting for the Generic Sensor API covering ambient-light-sensor,
        /// accelerometer, gyroscope and magnetometer. These are all mapped to a
        /// single ContentSettingsType.
        Sensors = 32,

        /// Content setting which stores whether or not the user has granted the site
        /// permission to respond to accessibility events, which can be used to
        /// provide a custom accessibility experience. Requires explicit user consent
        /// because some users may not want sites to know they're using assistive
        /// technology.
        AccessibilityEvents = 33,

        /// Used to store whether to allow a website to install a payment handler.
        PaymentHandler = 34,

        /// Content setting which stores whether to allow sites to ask for permission
        /// to access USB devices. If this is allowed specific device permissions are
        /// stored under USB_CHOOSER_DATA.
        UsbGuard = 35,

        /// Nothing is stored in this setting at present. Please refer to
        /// BackgroundFetchPermissionContext for details on how this permission
        /// is ascertained.
        BackgroundFetch = 36,

        /// Website setting which stores the amount of times the user has dismissed
        /// intent picker UI without explicitly choosing an option.
        IntentPickerDisplay = 37,

        /// Used to store whether to allow a website to detect user active/idle state.
        IdleDetection = 38,

        /// Content settings for access to serial ports. The "guard" content setting
        /// stores whether to allow sites to ask for permission to access a port. The
        /// permissions granted to access particular ports are stored in the "chooser
        /// data" website setting.
        SerialGuard = 39,

        SerialChooserData = 40,

        /// Nothing is stored in this setting at present. Please refer to
        /// PeriodicBackgroundSyncPermissionContext for details on how this permission
        /// is ascertained.
        /// This content setting is not registered because it does not require access
        /// to any existing providers.
        PeriodicBackgroundSync = 41,

        /// Content setting which stores whether to allow sites to ask for permission
        /// to do Bluetooth scanning.
        BluetoothScanning = 42,

        /// Content settings for access to HID devices. The "guard" content setting
        /// stores whether to allow sites to ask for permission to access a device.
        /// The permissions granted to access particular devices are stored in the
        /// "chooser data" website setting.
        HidGuard = 43,

        HidChooserData = 44,

        /// Wake Lock API, which has two lock types: screen and system locks.
        /// Currently, screen locks do not need any additional permission, and system
        /// locks are always denied while the right UI is worked out.
        WakeLockScreen = 45,

        WakeLockSystem = 46,

        /// Legacy SameSite cookie behavior. This disables SameSite=Lax-by-default,
        /// SameSite=None requires Secure, and Schemeful Same-Site, forcing the
        /// legacy behavior wherein 1) cookies that don't specify SameSite are treated
        /// as SameSite=None, 2) SameSite=None cookies are not required to be Secure,
        /// and 3) schemeful same-site is not active.
        ///
        /// This will also be used to revert to legacy behavior when future changes
        /// in cookie handling are introduced.
        LegacyCookieAccess = 47,

        /// Content settings which stores whether to allow sites to ask for permission
        /// to save changes to an original file selected by the user through the
        /// File System Access API.
        FileSystemWriteGuard = 48,

        /// Used to store whether to allow a website to exchange data with NFC
        /// devices.
        Nfc = 49,

        /// Website setting to store permissions granted to access particular
        /// Bluetooth devices.
        BluetoothChooserData = 50,

        /// Full access to the system clipboard (sanitized read without user gesture,
        /// and unsanitized read and write with user gesture).
        ClipboardReadWrite = 51,

        /// This is special-cased in the permissions layer to always allow, and as
        /// such doesn't have associated prefs data.
        ClipboardSanitizedWrite = 52,

        /// This content setting type is for caching safe browsing real time url
        /// check's verdicts of each origin.
        SafeBrowsingUrlCheckData = 53,

        /// Used to store whether a site is allowed to request AR or VR sessions with
        /// the WebXr Device API.
        VR = 54,

        AR = 55,

        /// Content setting which stores whether to allow site to open and read files
        /// and directories selected through the File System Access API.
        FileSystemReadGuard = 56,

        /// Access to first party storage in a third-party context. Exceptions are
        /// scoped to the combination of requesting/top-level origin, and are managed
        /// through the Storage Access API. For the time being, this content setting
        /// exists in parallel to third-party cookie rules stored in COOKIES.
        StorageAccess = 57,

        /// Content setting which stores whether to allow a site to control camera
        /// movements. It does not give access to camera.
        CameraPanTiltZoom = 58,

        /// Content setting for Screen Enumeration and Screen Detail functionality.
        /// Permits access to detailed multi-screen information, like size and
        /// position. Permits placing fullscreen and windowed content on specific
        /// screens. See also: https://w3c.github.io/window-placement
        WindowManagement = 59,

        /// <summary>
        /// Stores whether to allow insecure websites to make private network
        /// requests.
        /// See also: https://wicg.github.io/cors-rfc1918
        /// Set through enterprise policies only.
        /// </summary>
        InsecurePrivateNetworkDeprecated = 60,

        /// Content setting which stores whether or not a site can access low-level
        /// locally installed font data using the Local Fonts Access API.
        LocalFonts = 61,

        /// Stores per-origin state for permission auto-revocation (for all permission
        /// types).
        PermissionAutoRevocationData = 62,

        /// Stores per-origin state of the most recently selected directory for the
        /// use by the File System Access API.
        FileSystemLastPickedDirectory = 63,

        /// Controls access to the getDisplayMedia API when {preferCurrentTab: true}
        /// is specified.
        DisplayCapture = 64,

        /// Website setting to store permissions metadata granted to paths on the
        /// local file system via the File System Access API.
        /// |FILE_SYSTEM_WRITE_GUARD| is the corresponding "guard" setting.
        FileSystemAccessChooserData = 65,

        /// Stores a grant that allows a relying party to send a request for identity
        /// information to specified identity providers, potentially through any
        /// anti-tracking measures that would otherwise prevent it. This setting is
        /// associated with the relying party's origin.
        FederatedIdentitySharing = 66,

        /// Whether to use the v8 optimized JIT for running JavaScript on the page.
        JavaScriptJit = 67,

        /// Content setting which stores user decisions to allow loading a site over
        /// HTTP. Entries are added by hostname when a user bypasses the HTTPS-First
        /// Mode interstitial warning when a site does not support HTTPS. Allowed
        /// hosts are exact hostname matches -- subdomains of a host on the allowlist
        /// must be separately allowlisted.
        HttpAllowed = 68,

        /// Stores metadata related to form fill, such as e.g. whether user data was
        /// autofilled on a specific website.
        FormFillMetadata = 69,

        /// Setting to indicate that there is an active federated sign-in session
        /// between a specified relying party and a specified identity provider for
        /// a specified account. When this is present it allows access to session
        /// management capabilities between the sites. This setting is associated
        /// with the relying party's origin.
        FederatedIdentityActiveSession = 70,

        /// Setting to indicate whether Chrome should automatically apply darkening to
        /// web content.
        AutoDarkWebContent = 71,

        /// Setting to indicate whether Chrome should request the desktop view of a
        /// site instead of the mobile one.
        RequestDesktopSite = 72,

        /// Setting to indicate whether browser should allow signing into a website
        /// via the browser FedCM API.
        FederatedIdentityApi = 73,

        /// Stores notification interactions per origin for the past 90 days.
        /// Interactions per origin are pre-aggregated over seven-day windows: A
        /// notification interaction or display is assigned to the last Monday
        /// midnight in local time.
        NotificationInteractions = 74,

        /// Website setting which stores the last reduced accept language negotiated
        /// for a given origin, to be used on future visits to the origin.
        ReducedAcceptLanguage = 75,

        /// Website setting which is used for NotificationPermissionReviewService to
        /// store origin blocklist from review notification permissions feature.
        NotificationPermissionReview = 76,

        PrivateNetworkGuardDeprecated = 77,
        PrivateNetworkChooserDataDeprecated = 78,

        /// Website setting which stores whether the browser has observed the user
        /// signing into an identity-provider based on observing the IdP-SignIn-Status
        /// HTTP header.
        FederatedIdentityIdentityProviderSignInStatus = 79,

        /// Website setting which is used for UnusedSitePermissionsService to
        /// store revoked permissions of unused sites from unused site permissions
        /// feature.
        RevokedUnusedSitePermissions = 80,

        /// Similar to STORAGE_ACCESS, but applicable at the page-level rather than
        /// being specific to a frame.
        TopLevelStorageAccess = 81,

        /// Setting to indicate whether user has opted in to allowing auto re-authn
        /// via the FedCM API.
        FederatedIdentityAutoReAuthNPermission = 82,

        /// Website setting which stores whether the user has explicitly registered
        /// a website as an identity-provider.
        FederatedIdentityIdentityProviderRegistration = 83,

        /// Content setting which is used to indicate whether anti-abuse functionality
        /// should be enabled.
        AntiAbuse = 84,

        ThirdPartyStoragePartitioningDeprecated = 85,

        /// Used to indicate whether HTTPS-First Mode is enabled on the hostname.
        HttpsEnforced = 86,

        /// Setting for enabling the `getAllScreensMedia` API. Spec link:
        /// https://github.com/screen-share/capture-all-screens
        AllScreenCapture = 87,

        /// Stores per origin metadata for cookie controls.
        CookieControlsMetadata = 88,

        TpcdHeuristicsGrantsDeprecated = 89,
        TpcdMetadataGrantsDeprecated = 90,
        TpcdTrialDeprecated = 91,
        TopLevelTpcdTrialDeprecated = 92,
        TopLevelTpcdOriginTrialDeprecated = 93,

        /// Content setting used to indicate whether entering picture-in-picture
        /// automatically should be enabled.
        AutoPictureInPicture = 94,

        /// Whether user has opted into keeping file/directory permissions persistent
        /// between visits for a given origin. When enabled, permission metadata
        /// stored under |FILE_SYSTEM_ACCESS_CHOOSER_DATA| can auto-grant incoming
        /// permission request.
        FileSystemAccessExtendedPermission = 95,

        /// <summary>
        /// Whether the FSA Persistent Permissions restore prompt is eligible to be
        /// shown to the user, for a given origin.
        /// </summary>
        FileSystemAccessRestorePermission = 96,

        /// <summary>
        /// Whether an application capturing another tab, may scroll and zoom
        /// the captured tab.
        /// </summary>
        CapturedSurfaceControl = 97,

        /// <summary>
        /// Content setting for access to smart card readers.
        /// The "guard" content setting stores whether to allow sites to access the
        /// Smart Card API.
        /// </summary>
        SmartCardGuard = 98,

        SmartCardData = 99,

        /// <summary>
        /// Content settings for access to printers for the Web Printing API.
        /// </summary>
        WebPrinting = 100,

        /// <summary>
        /// Content setting used to indicate whether entering HTML Fullscreen
        /// automatically (i.e. without transient activation) should be enabled.
        /// </summary>
        AutomaticFullscreen = 101,

        /// <summary>
        /// Content settings used to indicate that a web app is allowed to prompt the
        /// user for the installation of sub apps.
        /// </summary>
        SubAppInstallationPrompts = 102,

        /// <summary>
        /// Whether an application can enumerate audio output device.
        /// </summary>
        SpeakerSelection = 103,

        /// <summary>
        /// Content settings for access to the Direct Sockets API.
        /// </summary>
        DirectSockets = 104,

        /// <summary>
        /// Keyboard Lock API allows a site to capture keyboard inputs that would
        /// otherwise be handled by the OS or the browser.
        /// </summary>
        KeyboardLock = 105,

        /// <summary>
        /// Pointer Lock API allows a site to hide the cursor and have exclusive
        /// access to mouse inputs.
        /// </summary>
        PointerLock = 106,

        /// <summary>
        /// Website setting which is used for RevokedPermissionsService to store
        /// auto-revoked notification permissions from abusive sites.
        /// </summary>
        RevokedAbusiveNotificationPermissions = 107,

        TrackingProtectionDeprecated = 108,

        /// <summary>
        /// With this permission, when the application calls `getDisplayMedia()`, a
        /// system audio track can be returned without showing the display media
        /// selection picker. The application can explicitly specify
        /// `systemAudio: 'exclude'` or `video: true` to still show the display media
        /// selection picker if needed. Please note that the setting only works for
        /// WebUI.
        /// </summary>
        DisplayMediaSystemAudio = 109,

        /// <summary>
        /// Whether to use the higher-tier v8 optimizers for running JavaScript on the
        /// page.
        /// </summary>
        JavascriptOptimizer = 110,

        StorageAccessHeaderOriginTrialDeprecated = 111,

        /// <summary>
        /// Whether or not sites can request Hand Tracking data within WebXR Sessions.
        /// </summary>
        HandTracking = 112,

        /// <summary>
        /// Website setting to indicate whether user has opted in to allow web apps to
        /// install other web apps.
        /// </summary>
        WebAppInstallation = 113,

        DirectSocketsPrivateNetworkAccessDeprecated = 114,

        /// <summary>
        /// Content settings for legacy cookie scope.
        /// Checks whether cookies scope is handled according to origin-bound cookies
        /// or legacy behavior.
        /// </summary>
        LegacyCookieScope = 115,

        /// <summary>
        /// Website setting to indicate whether the user has allowlisted suspicious
        /// notifications for the origin.
        /// </summary>
        AreSuspiciousNotificationsAllowlistedByUser = 116,

        /// <summary>
        /// Content settings for access to the Controlled Frame API.
        /// </summary>
        ControlledFrame = 117,

        /// <summary>
        /// Website setting which is used for RevokedPermissionsService to
        /// store revoked notification permissions of disruptive sites.
        /// </summary>
        RevokedDisruptiveNotificationPermissions = 118,

        /// <summary>
        /// Content setting for whether the site is allowed to make local network
        /// requests.
        /// </summary>
        LocalNetworkAccess = 119,

        /// <summary>
        /// Stores information on-device language packs for which a site has
        /// installed using the Web Speech API.
        /// </summary>
        OnDeviceSpeechRecognitionLanguagesDownloaded = 120,

        /// <summary>
        /// Stores which Translator API language packs the site has initialized.
        /// </summary>
        InitializedTranslations = 121,

        /// <summary>
        /// Stores a list of notification ids where content detection found the
        /// notification to be suspicious and a warning has already been shown for the
        /// site. Used for recovering notification contents from the database if the
        /// user decides they would like to see all of these notifications.
        /// </summary>
        SuspiciousNotificationIds = 122,

        /// <summary>
        /// To support approximate geolocation, the permission is migrating to use
        /// permissions with options, which won't be stored as ContentSettings. Upon
        /// launch of the feature, GEOLOCATION and GEOLOCATION_WITH_OPTIONS should be
        /// merged.
        /// </summary>
        GeolocationWithOptions = 123,

        /// <summary>
        /// Setting for enabling the Device Attributes API. Spec link:
        /// https://wicg.github.io/WebApiDevice/device_attributes/
        /// </summary>
        DeviceAttributes = 124,

        /// <summary>
        /// Stores per-origin state for permission heuristics. Currently used for
        /// auto-granting geolocation element permission request after repeated
        /// temporary grants.
        /// </summary>
        PermissionActionsHistory = 125,

        /// <summary>
        /// Website setting to indicate whether the user has selected "show original"
        /// when suspicious warning is shown. If the user has selected this, the
        /// notification permission will not be revoked based on suspicious verdict.
        /// </summary>
        SuspiciousNotificationShowOriginal = 126,

        /// <summary>
        /// Content setting for whether the site is allowed to make local network
        /// requests. Split from LOCAL_NETWORK_ACCESS.
        /// </summary>
        LocalNetwork = 127,

        /// <summary>
        /// Content setting for whether the site is allowed to make loopback network
        /// requests. Split from LOCAL_NETWORK_ACCESS.
        /// </summary>
        LoopbackNetwork = 128,

        /// <summary>
        /// Content setting for whether an IWA can add sub apps without prompting
        /// users.
        /// </summary>
        SubAppsWithoutPrompts = 129,

        /// <summary>
        /// Content setting for inline cue menus.
        /// </summary>
        InlineCueMenu = 130,

        /// <summary>
        /// Stores allowlist decisions for Suspicious Site Warnings.
        /// </summary>
        SuspiciousSiteWarningData = 131,

        NumTypes = 132,
    }
}
