using System;
namespace ACRM.mobile.Localization
{
    public static class LocalizationKeys
    {
        public static readonly string TextGroupLogin = "Login";
        public static readonly string TextGroupTutorial = "Tutorial";
        public static readonly string TextGroupBasic = "basic";
        public static readonly string TextGroupDate = "DateTexts";
        public static readonly string TextGroupSerialEntry = "serialEntry";
        public static readonly string TextGroupProcesses = "processes";
        public static readonly string TextGroupErrors = "errors";
        public static readonly string TextGroupAnalyses = "analyses";
        public static readonly string TextGroupNamesOfWeekdays = "names_of_weekdays_core";
        public static readonly string TextButtonCore = "Buttons_core";

        /// <summary>
        /// Text group for crm client ui
        /// </summary>
        public static readonly string TextGroupClientUIText = "client_ui_text";

        public static readonly int KeyLoginUsernamePlaceholder = 1;
        public static readonly int KeyLoginPasswordPlaceholder = 2;
        public static readonly int KeyLoginNoServerSelected = 3;
        public static readonly int KeyLoginOfflineLoginInProgress = 4;
        public static readonly int KeyLoginOnlineLoginInProgress = 5;
        public static readonly int KeyLoginSynchronisationInProgress = 6;
        public static readonly int KeyLoginErrorMessagePasswordIncorrect = 7;
        public static readonly int KeyLoginErrorMessageNoOfflineDataAvailable = 8;
        public static readonly int KeyLoginErrorMessageFullSyncRequired = 9;
        public static readonly int KeyLoginErrorMessageMissingInternetConnection = 10;
        public static readonly int KeyLoginErrorMessageOfflineModeRejected = 11;
        public static readonly int KeyLoginErrorMessageCheckLoginData = 12;
        public static readonly int KeyLoginErrorMessageServerApplicationCurrentlyStarting = 13;
        public static readonly int KeyLoginErrorMessageGeneral = 14;
        public static readonly int KeyLoginErrorTitleMissingInternetConnection = 15;
        public static readonly int KeyLoginErrorTitleGeneral = 16;
        public static readonly int KeyLoginErrorTitleLoginFailed = 17;
        public static readonly int KeyLoginErrorTitleLoginNotPossible = 18;
        public static readonly int KeyLoginErrorTitleSyncFailed = 19;
        public static readonly int KeyLoginErrorMessageErrorOccuredDuringSync = 20;
        public static readonly int KeyLoginErrorMissingLicense = 21;
        public static readonly int KeyLoginInvalidClientVersionTitle = 22;
        public static readonly int KeyLoginInvalidClientVersionMessage = 23;
        public static readonly int KeyLoginInvalidClientVersionMessageWithUpdateLink = 24;
        public static readonly int KeyLoginInvalidClientVersionActionOpenUpdateLink = 25;
        public static readonly int KeyLoginLogging = 26;
        public static readonly int KeyLoginNewPassword = 27;
        public static readonly int KeyLoginConfirmNewPassword = 28;
        public static readonly int KeyLoginErrorNewPasswordAndConfirmPasswordDontMatch = 29;
        public static readonly int KeyLoginPasswordExpired = 30;
        public static readonly int KeyLoginErrorNoLanguagesReturned = 31;
        public static readonly int KeyLoginTapLogin = 32;
        public static readonly int KeyLoginErrorMessageUserCancelledSync = 33;
        public static readonly int KeyTextErrorUserLoggedIn = 34;
        public static readonly int KeyTextErrorSameServer = 35;
        public static readonly int KeyTextErrorSameServerUrl = 36;

        /// <summary>
        /// Key of message title telling user to select the default server
        /// </summary>
        public static readonly int KeyLoginSelectDefaultServerTitle = 37;

        /// <summary>
        /// Key of message telling user to select the default server
        /// </summary>
        public static readonly int KeyLoginSelectDefaultServerMessage = 38;
        public static readonly int KeyLoginRegisterServerMessage = 39;

        public static readonly int KeyLoginWelcomeMessageWithoutName = 1001;
        public static readonly int KeyLoginWelcomeMessageWithName = 1002;
        public static readonly int KeyLoginPleaseChooseYourLanguage = 1003;
        public static readonly int KeyLoginApplyLanguage = 1004;
        public static readonly int KeyLoginIBoardHeadline = 1100;
        public static readonly int KeyLoginIBoardMoreAboutCRM = 1101;
        public static readonly int KeyLoginIBoardDemoAccount = 1102;
        public static readonly int KeyLoginIBoardTutorial = 1103;
        public static readonly int KeyLoginIBoardBlog = 1104;
        public static readonly int KeyLoginIBoardTwitter = 1105;
        public static readonly int KeyLoginIBoardFacebook = 1106;
        public static readonly int KeyLoginIBoardAbout = 1107;
        public static readonly int KeyLoginIBoardBlog_Url = 1108;
        public static readonly int KeyLoginIBoardAbout_Url = 1109;
        public static readonly int KeyLoginIBoardMoreAboutCRM_Url = 1110;
        public static readonly int KeyLoginIBoardDemoAccount_Url = 1111;
        public static readonly int KeyLoginIBoardTwitter_Url = 1112;
        public static readonly int KeyLoginIBoardFacebook_Url = 1113;
        public static readonly int KeyLoginOptionOfflineLogin = 1201;
        public static readonly int KeyLoginOptionViewLogs = 1202;
        public static readonly int KeyLoginOptionChangePassword = 1203;
        public static readonly int KeyLoginExit = 1204;
        public static readonly int KeyTutorialScreen3001 = 3001;
        public static readonly int KeyTutorialQuit = 3999;


        public static readonly int KeyLoginIncompleteSyncTitle = 2000;
        public static readonly int KeyLoginIncompleteSyncMessage = 2001;
        public static readonly int KeyLoginResume = 2002;
        public static readonly int KeyLoginRestart = 2003;



        /* Date Text Group */
        public static readonly int KeyDateToday = 0;
        public static readonly int KeyDateYesterday = 1;
        public static readonly int KeyDateTomorrow = 2;
        public static readonly int KeyDateNow = 3;
        public static readonly int KeyDateRange = 4;
        public static readonly int KeyDateDate = 6;
        public static readonly int KeyDateYear = 7;
        public static readonly int KeyDateMonth = 8;
        public static readonly int KeyDateDay = 9;
        public static readonly int KeyDateHour = 10;
        public static readonly int KeyDateMinute = 11;


        /* Basic Text Group */
        public static readonly int KeyBasicTabOverview = 0;
        public static readonly int KeyBasicDropdownNoDetails = 1;
        public static readonly int KeyBasicTabAll = 2;
        public static readonly int KeyBasicHeadlineGlobalSearch = 3;
        public static readonly int KeyBasicHeadlineLoading = 4;
        public static readonly int KeyBasicYes = 5;
        public static readonly int KeyBasicNo = 6;
        public static readonly int KeyBasicSyncCore = 7;
        public static readonly int KeyBasicSyncCatalogs = 8;
        public static readonly int KeyBasicSyncPlaceHolder = 9;
        public static readonly int KeyBasicSyncResources = 11;
        public static readonly int KeyBasicAddNewGroup = 10;
        public static readonly int KeyBasicLoadingData = 12;
        public static readonly int KeyBasicMoreData = 13;
        public static readonly int KeyBasicFilter = 14; // Filter
        public static readonly int KeyBasicOnline = 15; // Online
        public static readonly int KeyBasicOffline = 16; // Offline
        public static readonly int KeyBasicEmptyCatalog = 17; // <Empty>
        public static readonly int KeyBasicSelectAll = 18; // Select All
        public static readonly int KeyBasicSelectNone = 19; // Select None
        public static readonly int KeyBasicSelectedCount = 20; // @"%i ausgewaehlt"
        public static readonly int KeyBasicClearAll = 21; // @"Clear All"
        public static readonly int KeyBasicSettings = 22; // @"Settings"
        public static readonly int KeyBasicEdit = 23; // @"Edit"
        public static readonly int KeyBasicSave = 24; // @"Save"
        public static readonly int KeyBasicCancel = 25; // @"Cancel"
        public static readonly int KeyBasicClear = 26; // Clear
        public static readonly int KeyBasicErrorTitleNoMailAccount = 27; // Versand von Email nicht moeglich
        public static readonly int KeyBasicErrorMessageNoMailAccount = 28; // Sie müssen zuerst ein Email-Konto auf Ihrem iPad einrichten
        public static readonly int KeyBasicErrorTitleNoInternet = 29; // Internet connection failure
        public static readonly int KeyBasicErrorMessageNoInternet = 30; // No internet connection available
        public static readonly int KeyBasicErrorTitleReportError = 31; // Report failure
        public static readonly int KeyBasicErrorMessageLoadingReport = 32; // Error while loading report ...
        public static readonly int KeyBasicMessageLoadingReport = 33; // Please wait while loading the report ...
        public static readonly int KeyBasicErrorTitleCouldNotOpenUrl = 34; // Problem loading web page
        public static readonly int KeyBasicErrorMessageCouldNotOpenUrl = 35; // Could not load %@
        public static readonly int KeyBasicOK = 36; // OK
        public static readonly int KeyBasicNewInfoArea = 37; // %@ (new)

        public static readonly int KeyBasicPreviousItem = 38; // Serial Entry Keyboard
        public static readonly int KeyBasicPreviousField = 39; // Serial Entry Keyboard
        public static readonly int KeyBasicNextItem = 40; // Serial Entry Keyboard
        public static readonly int KeyBasicEnter = 41; // Serial Entry Keyboard
        public static readonly int KeyBasicClose = 42; // Schließen
        public static readonly int KeyBasicCreate = 43; // Anlegen

        public static readonly int KeyBasicNoCharacteristics = 44; // No Characteristics in Group

        public static readonly int KeyBasicDistanceFilterKmValue = 45; // Location/Distance-Filter km-Wert
        public static readonly int KeyBasicDistanceFilterMValue = 46; // Location/Distance-Filter m-Wert
        public static readonly int KeyBasicDistanceFilterDetail = 47; // Location/Distance-Filter Text im Detail
        public static readonly int KeyBasicDistanceFilterSummary = 48; // Location/Distance-Filter Text in der Filterauswahl
        public static readonly int KeyBasicDistanceFilterNoLocation = 49; // Location/Distance-Filter Keine Location Verfügbar

        public static readonly int KeyBasicAppFeedbackSubjectName = 50; // "%@: Feedback about the App" - enthält RepName
        public static readonly int KeyBasicCrashReportSubjectName = 51; // "%@: Crash Report" - enthält AppName

        public static readonly int KeyBasicSyncUpSyncCurrentlyInProgressTitle = 52; // "Changes being transferred"
        public static readonly int KeyBasicSyncUpSyncCurrentlyInProgressMessage = 53; // "You have local changes that are currently transferred to the server. You need to wait shortly until this operation is finished."
        public static readonly int KeyBasicSyncLocalChangesNotYetTransferredTitle = 54; // "Untransferred changes"
        public static readonly int KeyBasicSyncLocalChangesNotYetTransferredMessage = 55; // "You have local changes which have not been transferred to the server yet. Do you want to send them to the server now?"
        public static readonly int KeyBasicSyncLocalChangesNotYetTransferredActionLogoutAnyway = 56; // "Logout anyway"
        public static readonly int KeyBasicSyncLocalChangesNotYetTransferredActionSendChangesNow = 57; // "Send changes"
        public static readonly int KeyBasicSyncLocalChangesNotYetTransferredAndNoConnectionTitle = 58; // "Untransferred changes"
        public static readonly int KeyBasicSyncLocalChangesNotYetTransferredAndNoConnectionMessage = 59; // "You have local changes which have not been transferred to the server yet. You have to login at a later time with an available internet connection to send the changes to the server."

        public static readonly int KeyBasicSyncFailedErrorMessage = 60; // "Sync failed!"
        public static readonly int KeyBasic_SyncInProgressStepXFromY = 61; // "%d/%d"

        public static readonly int KeyBasicSyncConflictsActionEdit = 62; // "Editieren"
        public static readonly int KeyBasicSyncConflictsActionDiscardChanges = 63; // "Änderungen verwerfen"
        public static readonly int KeyBasicSyncConflictsActionReportError = 64; // "Fehler melden"
        public static readonly int KeyBasicSyncConflictsNoConflictsFound = 65; // "Keine Konflikte gefunden!"
        public static readonly int KeyBasicSyncConflictsDiscardChangesTitle = 66; // "Änderungen werden verworfen"
        public static readonly int KeyBasicSyncConflictsDiscardChangesMessage = 67; // "Sind Sie sicher, dass Sie alle Änderungen dieses Datensatzes verwerfen möchten? Neu-angelegte Datensätze und davon abhängige werden gelöscht."

        public static readonly int KeyBasicSyncOrganizerFullSyncRunningMessage = 68; // "Daten werden aktualisiert..."
        public static readonly int KeyBasicSyncOrganizerIncrementalSyncRunningMessage = 69; // "Daten werden aktualisiert..."
        public static readonly int KeyBasicSyncOrganizerUpSyncRunningMessage = 70; // "Änderungen werden an den Server geschickt..."
        public static readonly int KeyBasicSyncOrganizerConfigurationSyncRunningMessage = 71; // "Konfiguration wird aktualisiert..."
        public static readonly int KeyBasicSyncOrganizerFullSyncRecommendedWarningMessage = 72; // "You need to reset your data before %@ to keep being able to communicate with the server!" - Enthält Datum
        public static readonly int KeyBasicSyncOrganizerFullSyncMandatoryForOnlineErrorMessage = 73; // "You need to reset your data to communicate with the server! You need to do so before %@ to keep being able to login!" - enthält Datum
        public static readonly int KeyBasicWebReportDefaultOrganizerTitle = 74; // "WebReport"
        public static readonly int KeyBasicHTMLPageDefaultOrganizerTitle = 75; // "Error"
        public static readonly int KeyBasicErrorTitle = 75; // "Error"
        public static readonly int KeyBasicSyncConflictsPopupMessage = 76; // "%d Sync-Konflikte " - enthält Anzahl der Sync-Konflikte
        public static readonly int KeyBasicErrorProtocolMenuTitle = 77; // "Error Protocol"
        public static readonly int KeyBasicSyncOrganizerActionUpdateData = 78; // "Update Data"
        public static readonly int KeyBasicSyncOrganizerActionResetAndReinitialize = 79; // "Reset All Data and Reinitialize"
        public static readonly int KeyBasicSyncOrganizerActionUpdateConfiguration = 80; // "Update Configuration"
        public static readonly int KeyBasicSyncOrganizerLastUpdateTimestampMessage = 81; // "Last Update: %@" - Enthält Datum des letzten Syncs
        public static readonly int KeyBasicSyncOrganizerLastUpdateTimestampNeverSyncedMessage = 82; // "Never"
        public static readonly int KeyBasicOpenInGoogleMapsAction = 83; // "Open In Maps"
        public static readonly int KeyBasicMailErrorLogSubject = 84; // "%@ Error Log" - Enthält App Name
        public static readonly int KeyBasicDelete = 85;
        public static readonly int KeyBasicSearchResultsOffline = 86;
        public static readonly int KeyBasicSearchResultsOnline = 87;
        public static readonly int KeyBasicNoObjectives = 88; // No Objectives
        public static readonly int KeyBasicOrderReportEmailSubject = 89; // Report
        public static readonly int KeyBasicSyncOrganizerActionChangeLanguage = 90; // "Change Language"
        public static readonly int KeyBasicChange = 91; // Change
        public static readonly int KeyBasicLanguage = 92; // Language

        public static readonly int KeyBasicOfflineNotAvailable = 93;
        public static readonly int KeyBasicStart = 94;
        public static readonly int KeyBasicEnd = 95;
        public static readonly int KeyBasicShowRecord = 96;
        public static readonly int KeyBasicCoINumberOfChildren = 97;
        public static readonly int KeyBasicCoIUnknownNumberOfChildren = 98;
        public static readonly int KeyBasicSyncConflictsRetryAll = 99;
        public static readonly int KeyBasicEmployee = 100; // employee
        public static readonly int KeyBasicLastUsed = 101; // Last Used
        public static readonly int KeyBasicRecentlyUsed = 102; // Recently Used
        public static readonly int KeyBasiciPadCalendar = 103; // iPad calendar
        public static readonly int KeyBasicSelected = 104; // Selected
        public static readonly int KeyBasicSendLogPerEmail = 105; // send log per email button
        public static readonly int KeyBasicResetLogfile = 106; // reset the log file
        public static readonly int KeyBasicSyncConflictsReportAllErrors = 107;
        public static readonly int KeyBasicSyncConflictsNoOfflineRecordsFound = 108; // keine nicht synchronisierten Datensätze vorhanden
        public static readonly int KeyBasicChangePassword = 109;
        public static readonly int KeyBasicPasswordNotChanged = 110;
        public static readonly int KeyBasicPasswordChanged = 111;
        public static readonly int KeyBasicFrom = 112;
        public static readonly int KeyBasicTo = 113;
        public static readonly int KeyBasicSelectedXOfY = 114;
        public static readonly int KeyBasicNotSpecified = 115;
        public static readonly int KeyBasicOpeningTimes = 116;
        public static readonly int KeyBasicVisitTimes = 117;
        public static readonly int KeyBasicPhoneTimes = 118;
        public static readonly int KeyBasicDebugOrganizerTitle = 119;
        public static readonly int KeyBasicDebugCrmDbPage = 120;
        public static readonly int KeyBasicDebugConfigDbPage = 121;
        public static readonly int KeyBasicDebugOfflineDbPage = 122;
        public static readonly int KeyBasicDebugLoggingPage = 123;
        public static readonly int KeyBasicDebugDefaultMenuName = 124;
        public static readonly int KeyBasicSearchWithRadius = 125;
        public static readonly int KeyBasicClosed = 126;
        public static readonly int KeyBasicAllDay = 127;
        public static readonly int KeyBasicStartLocation = 128;
        public static readonly int KeyBasicWithinRadius = 129;
        public static readonly int KeyBasicLocation = 130;
        public static readonly int KeyBasicMapStandard = 131;
        public static readonly int KeyBasicMapSatellite = 132;
        public static readonly int KeyBasicMapHybrid = 133;
        public static readonly int KeyBasicMapNavigateTo = 134;
        public static readonly int KeyBasicSwitchToEditOrganizer = 135;
        public static readonly int KeyBasicEditOrganizerWillBeClosed = 136;
        public static readonly int KeyBasicOnlyOneEditIsAllowed = 137;
        public static readonly int KeyBasicDeleteRecordTitle = 138;
        public static readonly int KeyBasicDeleteRecordMessage = 139;
        public static readonly int KeyBasicShowAllRecords = 140;
        public static readonly int KeyBasicGeoResultSectionHeader = 141;
        public static readonly int KeyBasicCurrentLocation = 143;
        public static readonly int KeyBasicCustomProductName = 144;
        public static readonly int KeyBasicHomeBaseFullSync = 145;
        public static readonly int KeyBasicSyncConflictsTitle = 146;
        public static readonly int KeyBasicTimeline = 147;
        public static readonly int KeyBasicTextInBracket = 148;
        public static readonly int KeyBasicDownloadingDocuments = 149;
        public static readonly int KeyBasicLogout = 150;
        public static readonly int KeyBasicGlobal = 151;
        public static readonly int KeyBasicSync = 152;
        public static readonly int KeyBasicSystem = 153;
        public static readonly int KeyBasicBackButton = 154;
        public static readonly int KeyBasicActions = 155;
        public static readonly int KeyBasicBrowseBy = 156;
        public static readonly int KeyBasicSearch = 157;
        public static readonly int KeyBasicRetry = 158;
        public static readonly int KeyBasicFavorites = 159;
        public static readonly int KeyBasicLabelFullSync = 160;
        public static readonly int KeyBasicSublabelFullSync = 161;
        public static readonly int KeyBasicCloseOpenEditOrganizer = 162;
        public static readonly int KeyBasicScanQrCode = 163;
        public static readonly int KeyBasicSendMail = 164;
        public static readonly int KeyBasicName = 165;
        public static readonly int KeyBasicNew = 166;
        public static readonly int KeyBasicUpload = 167;
        public static readonly int KeyBasicInboxUploadTitle = 168;
        public static readonly int KeyBasicInboxTitle = 169;
        public static readonly int KeyBasicInboxAdditionalTitle = 170;
        public static readonly int KeyBasicInboxDescription = 171;
        public static readonly int KeyBasicAddFile = 172;
        public static readonly int KeyBasicAddFileToInbox = 173;
        public static readonly int KeyBasicAddPhoto = 174;
        public static readonly int KeyBasicRetryAll = 175;
        public static readonly int KeyBasicDiscardAll = 176;
        public static readonly int KeyBasicDeleteSelected = 177;
        public static readonly int KeyBasicEditSelected = 178;

        // New (2021) strings
        public static readonly int KeyBasicUserActionNotImplementedError = 288;
        public static readonly int KeyBasicInputFieldMandatory = 289;
        public static readonly int KeyBasicEditModeTitle = 290;
        public static readonly int KeyBasicMorning = 291;
        public static readonly int KeyBasicAfternoon = 292;

        // New (2022) strings
        public static readonly int KeyBasicFirstInterval = 293;
        public static readonly int KeyBasicSecondInterval = 294;
        public static readonly int KeyBasicAddVideo = 295;
        public static readonly int KeyBasicLabelIncrementalDataSync = 296;
        public static readonly int KeyBasicIncrementalSyncPlaceHolder = 297;

        /// <summary>
        /// Text for More actions button
        /// </summary>
        public static readonly int KeyBasicNoInternet = 269;
        public static readonly int KeySyncFailed = 299;
        /* Serial Entry Text Group */

        public static readonly int KeySerialEntryQuantityXCurrency = 0;
        public static readonly int KeySerialEntryComplete = 1;
        public static readonly int KeySerialEntryPackageSize = 2;
        public static readonly int KeySerialEntryQuota = 3;
        public static readonly int KeySerialEntryMinQuantity = 4;
        public static readonly int KeySerialEntryMaxQuantity = 5;
        public static readonly int KeySerialEntryMinMaxQuantity = 6;
        public static readonly int KeySerialEntryDuplicate = 7;
        public static readonly int KeySerialEntryPricing = 8;
        public static readonly int KeySerialEntryDiscount = 9;
        public static readonly int KeySerialEntryUnitPrice = 10;
        public static readonly int KeySerialEntryFreeGoods = 11;
        public static readonly int KeySerialEntryPriceRange = 12;
        public static readonly int KeySerialEntryQuantity = 13;
        public static readonly int KeySerialEntryScanningEAN = 14;
        public static readonly int KeySerialEntryNoListing = 15;
        public static readonly int KeySerialEntrySerialEntry = 16;
        public static readonly int KeySerialEntrySerialEntryAutoCreate = 17;
        public static readonly int KeySerialEntrySummaryTitle = 18;
        public static readonly int KeySerialEntryAllItemsButtonTitle = 19;
        public static readonly int KeySerialEntrySummaryButtonTitle = 20;

        /* Errors Text Group */

        public static readonly int KeyErrorsConfigurationError = 0;
        public static readonly int KeyErrorsExpandMissing = 1;
        public static readonly int KeyErrorsSearchAndListMissing = 2;
        public static readonly int KeyErrorsFilterMissing = 3;
        public static readonly int KeyErrorsFieldControlMissing = 4;
        public static readonly int KeyErrorsActionNotAllowed = 5;
        public static readonly int KeyErrorsRightsFilter = 6;
        public static readonly int KeyErrorsParameterEmpty = 7;
        public static readonly int KeyErrorsActionNotPossible = 8;
        public static readonly int KeyErrorsActionPending = 9;
        public static readonly int KeyErrorsRecordDoesNotExist = 10;
        public static readonly int KeyErrorsErrorLog = 11;
        public static readonly int KeyErrorsSend = 12;
        public static readonly int KeyErrorsCouldNotBeSaved = 13;
        public static readonly int KeyErrorsGeneralServerError = 14;
        public static readonly int KeyErrorsServerTimeout = 15;
        public static readonly int KeyErrorsCrashReportDetailMessage = 16;
        public static readonly int KeyErrorsEditMandatoryFieldNotSet = 17; // "Muss-Feld nicht gesetzt"
        public static readonly int KeyErrorsConfigHeaderMissing = 18; // "Missing"
        public static readonly int KeyErrorsUnexpectedError = 19; // "An unexpected error occured."
        public static readonly int KeyErrorsPleaseTryAgain = 20; // "Please try it again!"
        public static readonly int KeyErrorsCouldNotBeSavedDetailMessage = 21; // "error"
        public static readonly int KeyErrorsCouldNotLoadTableCaption = 22;
        public static readonly int KeyErrorsClientConstraintError = 23;
        public static readonly int KeyErrorsNoExternalParticipantSelected = 24;
        public static readonly int KeyErrorsRecordNotFound = 25;
        public static readonly int KeyErrorsNoDetailInfo = 26;
        public static readonly int KeyErrorsDataNotSaved = 27;
        public static readonly int KeyErrorsErrorInSqlStatement = 28;
        public static readonly int KeyErrorsErrorOnlSelectAllowed = 29;
        public static readonly int KeyErrorsNoResults = 30;
        public static readonly int KeyErrorsWarning = 31;
        public static readonly int KeyErrorsWarningPasswordChangeNeeded = 32;
        public static readonly int KeyErrorsWarningHTMLFormatWillBeLost = 33;
        public static readonly int KeyErrorsOtherErrors = 34;
        public static readonly int KeyErrorsDiscardChangesAndContinue = 35;
        public static readonly int KeyErrorsInboxNotConfigured = 36;
        public static readonly int KeyErrorsInboxFileNotSupported = 37;
        public static readonly int KeyErrorsInboxFileSizeNotSupported = 38;
        public static readonly int KeyErrorsInvalidTimeSelection = 39;
        public static readonly int KeyErrorsInvalidDateSelection = 40;
        public static readonly int KeyErrorsFileAccessDenied = 41;
        public static readonly int KeyErrorsTimeout = 591;
        public static readonly int KeyErrorsUnexpected = 595;
        public static readonly int KeyErrorsLocalFileAccessProblems = 600;

        // New (2022) strings
        public static readonly int KeyErrorsInvalidTimeIntervalTitle = 601;
        public static readonly int KeyErrorsInvalidTimeIntervalDescription = 602;

        /// <summary>
        /// Key for error message about width too small
        /// </summary>
        public static readonly int KeyErrorsWidthTooSmall = 596;

        /// <summary>
        /// Key for error message about height too small
        /// </summary>
        public static readonly int KeyErrorsHeightTooSmall = 597;

        /// <summary>
        /// Key for Resolution not supported
        /// </summary>
        public static readonly int KeyErrorsResolutionNotSupported = 598;

        /* Processes Text Group */
        public static readonly int KeyProcessesEditCharacteristics = 0;
        public static readonly int KeyProcessesSyncing = 1;
        public static readonly int KeyProcessesWaitForChanges = 2;
        public static readonly int KeyProcessesYouMadeChanges = 3;
        public static readonly int KeyProcessesReallyAbortAndLoseChanges = 4;
        public static readonly int KeyProcessesCrashReports = 5;
        public static readonly int KeyProcessesSyncErrors = 6;
        public static readonly int KeyProcessesEditRecordChangedInBackgroundTitle = 7; // "Record has been changed"
        public static readonly int KeyProcessesEditRecordChangedInBackgroundMessage = 8; // "The record you're editing has been changed by somebody else. Your changes will be dismissed"
        public static readonly int KeyProcessesEditSavingChangesProgressMessage = 9; // "Please wait while your changes are saved ..."
        public static readonly int KeyProcessesEditObjectives = 10;
        public static readonly int KeyProcessesFilterAll = 11;
        public static readonly int KeyProcessesFilterListings = 12;
        public static readonly int KeyProcessesDocuments = 13;
        public static readonly int KeyProcessesRecordDataWasSync = 14;
        public static readonly int KeyProcessesAddToFavorites = 15;
        public static readonly int KeyProcessesDeleteFromFavorites = 16;
        public static readonly int KeyProcessesOrderWasApproved = 17;
        public static readonly int KeyProcessesShowDocument = 18;
        public static readonly int KeyProcessesSendDocument = 19;
        public static readonly int KeyProcessesUploadPhotoTakePhoto = 20;
        public static readonly int KeyProcessesUploadPhotoLibrary = 21;
        public static readonly int KeyProcessesUploadPhotoSizes = 22;
        public static readonly int KeyProcessesImagesAvailableOnline = 23;
        public static readonly int KeyProcessesUploadPhotoUploadAlertTitle = 24;
        public static readonly int KeyProcessesUploadPhotoUploadAlertMessage = 25;
        public static readonly int KeyProcessesUploadPhotoUploadAlertBackground = 26;
        public static readonly int KeyProcessesSerialEntrySumItems = 28;
        public static readonly int KeyProcessesSerialEntrySumTotal = 29;
        public static readonly int KeyProcessesUploadPhotoChooseSizeTitle = 27;
        public static readonly int KeyProcessesUploadPhotoPhotoNamePlaceholder = 30;
        public static readonly int KeyProcessesCalendarWeekLabel = 31;
        public static readonly int KeyProcessesCalendarMoreItems = 32;
        public static readonly int KeyProcessesReloadDocument = 33;
        public static readonly int KeyProcessesSignatureTitle = 34;
        public static readonly int KeyProcessesSignatureConfirmButtonTitle = 35;
        public static readonly int KeyProcessesCalendarDayView = 36;
        public static readonly int KeyProcessesCalendarWeekView = 37;
        public static readonly int KeyProcessesCalendarMonthView = 38;
        public static readonly int KeyProcessesCalendarListView = 39;
        public static readonly int KeyProcessesCredentialsWereRejected = 40;
        public static readonly int KeyProcessesCredentialsWorkOffline = 41;
        public static readonly int KeyProcessesCredentialsEnterPassword = 42;
        public static readonly int KeyProcessesCredentialsEnterBoth = 43;
        public static readonly int KeyProcessesCredentialsRetry = 44;
        public static readonly int KeyProcessesSerialEntryFreeGoodsSumTotal = 45;
        public static readonly int KeyProcessesSerialEntrySaveErrors = 46;
        public static readonly int KeyProcessesDiscard = 47;
        public static readonly int KeyProcessesQuestionnaireSurveyAllQuestions = 48;
        public static readonly int KeyProcessesQuestionnaireMandatory = 49;
        public static readonly int KeyProcessesQuestionnaireSummary = 50;
        public static readonly int KeyProcessesQuestionnaireFinalize = 51;
        public static readonly int KeyProcessesQuestionnaireFinalized = 52;
        public static readonly int KeyProcessesSurvey = 53;
        public static readonly int KeyProcessesExecute = 54;
        public static readonly int KeyProcessesAction = 55;
        public static readonly int KeyProcessesQuestionnaireNotAvailable = 56;
        public static readonly int KeyProcessesExecuting = 57;
        public static readonly int KeyProcessesDone = 58;
        public static readonly int KeyProcessesPortfolioResult = 59;
        public static readonly int KeyProcessesObjectivesTitle = 60;
        public static readonly int KeyProcessesObjectivesSaveChanges = 61;
        public static readonly int KeyProcessesObjectivesSaveError = 62;
        public static readonly int KeyProcessesLogfileMailContent = 63;
        public static readonly int KeyProcessesSendReport = 64;
        public static readonly int KeyProcessesCalendarWorkWeekView = 65;
        public static readonly int KeyAnalysesEmptyOptions = 0;
        public static readonly int KeyAnalysesAnalysis = 1;
        public static readonly int KeyAnalysesAnalysisWithName = 2;
        public static readonly int KeyAnalysesBackToAnalysis = 3;
        public static readonly int KeyAnalysesDrilldown = 4;
        public static readonly int KeyAnalysesDetails = 5;
        public static readonly int KeyAnalysesOther = 6;
        public static readonly int KeyAnalysesNoValue = 7;
        public static readonly int KeyAnalysesShowParam = 8;
        public static readonly int KeyAnalysesQueryWithName = 9;
        public static readonly int KeyAnalysesQuery = 10;
        public static readonly int KeyAnalysesReset = 11;
        public static readonly int KeyAnalysesSum = 12;

        public static readonly int KeyNamesOfWeekdaysMonday = 0;
        public static readonly int KeyNamesOfWeekdaysTuesday = 1;
        public static readonly int KeyNamesOfWeekdaysWednesday = 2;
        public static readonly int KeyNamesOfWeekdaysThursday = 3;
        public static readonly int KeyNamesOfWeekdaysFriday = 4;
        public static readonly int KeyNamesOfWeekdaysSaturday = 5;
        public static readonly int KeyNamesOfWeekdaysSunday = 6;

        public static readonly int KeySynchronizing = 512;

        // Texts for client_ui_text group

        /// <summary>
        /// Text for synchronization in progress
        /// </summary>
        public static readonly int KeyClientUISynchronizingText = 0;

        /// <summary>
        /// Text for Application version
        /// </summary>
        public static readonly int KeyClientUIApplicationVersionText = 1;

        /// <summary>
        /// Key for dashboard title
        /// </summary>
        public static readonly int KeyClientUIDashboardTitle = 2;

        /// <summary>
        /// Key for Exit button
        /// </summary>
        public static readonly int KeyClientUIExitText = 3;

        /// <summary>
        /// Key for More Actions button
        /// </summary>
        public static readonly int KeyClientUIMoreActions = 4;

        /// <summary>
        /// Key for More Actions button
        /// </summary>
        public static readonly int KeyClientUINoDataToDisplay = 5;

        /// <summary>
        /// Key for Internet Connection Restored
        /// </summary>
        public static readonly int KeyClientUIConnectionRestored = 6;

        /// <summary>
        /// Key for make phone call action
        /// </summary>
        public static readonly int KeyClientUIMakePhoneCall = 7;

        /// <summary>
        /// Key for make phone call action
        /// </summary>
        public static readonly int KeyClientUIPhoneCallNotSupported = 8;

        /// <summary>
        /// Key check all button in filter page
        /// </summary>
        public static readonly int KeyClientUICheckAll = 9;

        /// <summary>
        /// Key clear all button in filter page
        /// </summary>
        public static readonly int KeyClientUIClearAll = 10;

        /// <summary>
        /// Key enable all button in filter page
        /// </summary>
        public static readonly int KeyClientUIEnableAll = 11;

        /// <summary>
        /// Key for unsupported view message
        /// </summary>
        public static readonly int KeyClientUIUnsupportedViewMessage = 12;

        /// <summary>
        /// Key for unsupported element message
        /// </summary>
        public static readonly int KeyClientUIUnsupportedElementMessage = 13;

        /// <summary>
        /// Key disable all button in filter page
        /// </summary>
        public static readonly int KeyClientUIDisableAll = 14;

        /// <summary>
        /// Key matches found in filter page
        /// </summary>
        public static readonly int KeyClientUIMatchesFound = 15;

        /// <summary>
        /// Key client UI non supported action title
        /// </summary>
        public static readonly int KeyClientUINonSupportedActionTitle = 16;

        /// <summary>
        /// Key client UI non supported action message
        /// </summary>
        public static readonly int KeyClientUINonSupportedActionMessage = 17;

        /// <summary>
        /// Key client UI error log purge
        /// </summary>
        public static readonly int KeyClientUIErrorLogPurge = 18;
        /// <summary>
        /// Key in client UI group for edit selected
        /// </summary>
        public static readonly int KeyClientUIEditSelected = 20;

        /// <summary>
        /// Key in client UI group for delete selected
        /// </summary>
        public static readonly int KeyClientUIDeleteSelected = 21;

        /// <summary>
        /// Key in client UI group for discard all
        /// </summary>
        public static readonly int KeyClientUIDiscardAll = 22;


        /// <summary>
        /// Key client UI error log purge detail
        /// </summary>
        public static readonly int KeyClientUIErrorLogPurgeDetail = 19;
        
        public static readonly int TextButtonCoreApply = 14;

        public static readonly string upTextProcessExecute = "TBD";
        public static readonly string upTextProcessAction = "TBD";
        public static readonly string upTextProcessExecuting = "TBD";
        public static readonly string upTextProcessDone = "TBD";
        public static readonly string upTextRetry = "TBD";
    }
}
