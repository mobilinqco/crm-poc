﻿using System;
using System.Collections.Generic;

namespace ACRM.mobile.Services.Utils
{
    public class GlyphIconCode
    {
        public static string ResolveCode(string resourceName)
        {
            if (!string.IsNullOrEmpty(resourceName) && resourceName.StartsWith("Icon:"))
            {
                string iconName = resourceName.Replace("Icon:", "Halflings").Replace(" ", "");
                if(Codes.ContainsKey(iconName))
                {
                    return ConvertCode(Codes[iconName]);
                }
                else
                {
                    iconName = resourceName.Replace("Icon:", "").Replace(" ", "");
                    if (Codes.ContainsKey(iconName))
                    {
                        return ConvertCode(Codes[iconName]);
                    }
                }
            }

            return resourceName;
        }

        public static string ConvertCode(string glyphText)
        {
            try
            {
                char result = (char)(Convert.ToInt32(glyphText.Replace("\\", ""), 16));
                return result.ToString();
            }
            catch
            {
                return "\ue107";
            }
        }

        private static readonly Dictionary<string, string> Codes = new Dictionary<string, string> {
            {"Glass", "E001"},
            {"Leaf", "E002"},
            {"Dog", "1F415"},
            {"User", "E004"},
            {"Girl", "1F467"},
            {"Car", "E006"},
            {"UserAdd", "E007"},
            {"UserRemove", "E008"},
            {"Film", "E009"},
            {"Magic", "E010"},
            {"Envelope", "2709"},
            {"Camera", "1F4F7"},
            {"Heart", "E013"},
            {"BeachUmbrella", "E014"},
            {"Train", "1F686"},
            {"Print", "E016"},
            {"Bin", "E017"},
            {"Music", "E018"},
            {"Note", "E019"},
            {"HeartEmpty", "E020"},
            {"Home", "E021"},
            {"Snowflake", "2744"},
            {"Fire", "1F525"},
            {"Magnet", "E024"},
            {"Parents", "E025"},
            {"Binoculars", "E026"},
            {"Road", "E027"},
            {"Search", "E028"},
            {"Cars", "E029"},
            {"Notes2", "E030"},
            {"Pencil", "270F"},
            {"Bus", "1F68C"},
            {"WifiAlt", "E033"},
            {"Luggage", "E034"},
            {"OldMan", "E035"},
            {"Woman", "1F469"},
            {"File", "E037"},
            {"Coins", "E038"},
            {"Airplane", "2708"},
            {"Notes", "E040"},
            {"Stats", "E041"},
            {"Charts", "E042"},
            {"PieChart", "E043"},
            {"Group", "E044"},
            {"Keys", "E045"},
            {"Calendar", "1F4C5"},
            {"Router", "E047"},
            {"CameraSmall", "E048"},
            {"Dislikes", "E049"},
            {"Star", "E050"},
            {"Link", "E051"},
            {"EyeOpen", "E052"},
            {"EyeClose", "E053"},
            {"Alarm", "E054"},
            {"Clock", "E055"},
            {"Stopwatch", "E056"},
            {"Projector", "E057"},
            {"History", "E058"},
            {"Truck", "E059"},
            {"Cargo", "E060"},
            {"Compass", "E061"},
            {"Keynote", "E062"},
            {"Paperclip", "1F4CE"},
            {"Power", "E064"},
            {"Lightbulb", "E065"},
            {"Tag", "E066"},
            {"Tags", "E067"},
            {"Cleaning", "E068"},
            {"Ruller", "E069"},
            {"Gift", "E070"},
            {"Umbrella", "2602"},
            {"Book", "E072"},
            {"Bookmark", "1F516"},
            {"Wifi", "E074"},
            {"Cup", "E075"},
            {"Stroller", "E076"},
            {"Headphones", "E077"},
            {"Headset", "E078"},
            {"WarningSign", "E079"},
            {"Signal", "E080"},
            {"Retweet", "E081"},
            {"Refresh", "E082"},
            {"Roundabout", "E083"},
            {"Random", "E084"},
            {"Heat", "E085"},
            {"Repeat", "E086"},
            {"Display", "E087"},
            {"LogBook", "E088"},
            {"AddressBook", "E089"},
            {"Building", "E090"},
            {"Eyedropper", "E091"},
            {"Adjust", "E092"},
            {"Tint", "E093"},
            {"Crop", "E094"},
            {"VectorPathSquare", "E095"},
            {"VectorPathCircle", "E096"},
            {"VectorPathPolygon", "E097"},
            {"VectorPathLine", "E098"},
            {"VectorPathCurve", "E099"},
            {"VectorPathAll", "E100"},
            {"Font", "E101"},
            {"Italic", "E102"},
            {"Bold", "E103"},
            {"TextUnderline", "E104"},
            {"TextStrike", "E105"},
            {"TextHeight", "E106"},
            {"TextWidth", "E107"},
            {"TextResize", "E108"},
            {"LeftIndent", "E109"},
            {"RightIndent", "E110"},
            {"AlignLeft", "E111"},
            {"AlignCenter", "E112"},
            {"AlignRight", "E113"},
            {"Justify", "E114"},
            {"List", "E115"},
            {"TextSmaller", "E116"},
            {"TextBigger", "E117"},
            {"Embed", "E118"},
            {"EmbedClose", "E119"},
            {"Table", "E120"},
            {"MessageFull", "E121"},
            {"MessageEmpty", "E122"},
            {"MessageIn", "E123"},
            {"MessageOut", "E124"},
            {"MessagePlus", "E125"},
            {"MessageMinus", "E126"},
            {"MessageBan", "E127"},
            {"MessageFlag", "E128"},
            {"MessageLock", "E129"},
            {"MessageNew", "E130"},
            {"Inbox", "E131"},
            {"InboxPlus", "E132"},
            {"InboxMinus", "E133"},
            {"InboxLock", "E134"},
            {"InboxIn", "E135"},
            {"InboxOut", "E136"},
            {"Cogwheel", "E137"},
            {"Cogwheels", "E138"},
            {"Picture", "E139"},
            {"AdjustAlt", "E140"},
            {"DatabaseLock", "E141"},
            {"DatabasePlus", "E142"},
            {"DatabaseMinus", "E143"},
            {"DatabaseBan", "E144"},
            {"FolderOpen", "E145"},
            {"FolderPlus", "E146"},
            {"FolderMinus", "E147"},
            {"FolderLock", "E148"},
            {"FolderFlag", "E149"},
            {"FolderNew", "E150"},
            {"Edit", "E151"},
            {"NewWindow", "E152"},
            {"Check", "E153"},
            {"Unchecked", "E154"},
            {"MoreWindows", "E155"},
            {"ShowBigThumbnails", "E156"},
            {"ShowThumbnails", "E157"},
            {"ShowThumbnailsWithLines", "E158"},
            {"ShowLines", "E159"},
            {"Playlist", "E160"},
            {"Imac", "E161"},
            {"Macbook", "E162"},
            {"Ipad", "E163"},
            {"Iphone", "E164"},
            {"IphoneTransfer", "E165"},
            {"IphoneExchange", "E166"},
            {"Ipod", "E167"},
            {"IpodShuffle", "E168"},
            {"EarPlugs", "E169"},
            {"Record", "E170"},
            {"StepBackward", "E171"},
            {"FastBackward", "E172"},
            {"Rewind", "E173"},
            {"Play", "E174"},
            {"Pause", "E175"},
            {"Stop", "E176"},
            {"Forward", "E177"},
            {"FastForward", "E178"},
            {"StepForward", "E179"},
            {"Eject", "E180"},
            {"FacetimeVideo", "E181"},
            {"DownloadAlt", "E182"},
            {"Mute", "E183"},
            {"VolumeDown", "E184"},
            {"VolumeUp", "E185"},
            {"Screenshot", "E186"},
            {"Move", "E187"},
            {"More", "E188"},
            {"BrightnessReduce", "E189"},
            {"BrightnessIncrease", "E190"},
            {"CirclePlus", "E191"},
            {"CircleMinus", "E192"},
            {"CircleRemove", "E193"},
            {"CircleOk", "E194"},
            {"CircleQuestionMark", "E195"},
            {"CircleInfo", "E196"},
            {"CircleExclamationMark", "E197"},
            {"Remove", "E198"},
            {"Ok", "E199"},
            {"Ban", "E200"},
            {"Download", "E201"},
            {"Upload", "E202"},
            {"ShoppingCart", "E203"},
            {"Lock", "1F512"},
            {"Unlock", "E205"},
            {"Electricity", "E206"},
            {"Ok2", "E207"},
            {"Remove2", "E208"},
            {"CartOut", "E209"},
            {"CartIn", "E210"},
            {"LeftArrow", "E211"},
            {"RightArrow", "E212"},
            {"DownArrow", "E213"},
            {"UpArrow", "E214"},
            {"ResizeSmall", "E215"},
            {"ResizeFull", "E216"},
            {"CircleArrowLeft", "E217"},
            {"CircleArrowRight", "E218"},
            {"CircleArrowTop", "E219"},
            {"CircleArrowDown", "E220"},
            {"PlayButton", "E221"},
            {"Unshare", "E222"},
            {"Share", "E223"},
            {"ChevronRight", "E224"},
            {"ChevronLeft", "E225"},
            {"Bluetooth", "E226"},
            {"Euro", "20AC"},
            {"Usd", "E228"},
            {"Gbp", "E229"},
            {"Retweet2", "E230"},
            {"Moon", "E231"},
            {"Sun", "2609"},
            {"Cloud", "2601"},
            {"Direction", "E234"},
            {"Brush", "E235"},
            {"Pen", "E236"},
            {"ZoomIn", "E237"},
            {"ZoomOut", "E238"},
            {"Pin", "E239"},
            {"Albums", "E240"},
            {"RotationLock", "E241"},
            {"Flash", "E242"},
            {"GoogleMaps", "E243"},
            {"Anchor", "2693"},
            {"Conversation", "E245"},
            {"Chat", "E246"},
            {"Male", "E247"},
            {"Female", "E248"},
            {"Asterisk", "002A"},
            {"Divide", "00F7"},
            {"SnorkelDiving", "E251"},
            {"ScubaDiving", "E252"},
            {"OxygenBottle", "E253"},
            {"Fins", "E254"},
            {"Fishes", "E255"},
            {"Boat", "E256"},
            {"Delete", "E257"},
            {"SheriffsStar", "E258"},
            {"Qrcode", "E259"},
            {"Barcode", "E260"},
            {"Pool", "E261"},
            {"Buoy", "E262"},
            {"Spade", "E263"},
            {"Bank", "1F3E6"},
            {"Vcard", "E265"},
            {"ElectricalPlug", "E266"},
            {"Flag", "E267"},
            {"CreditCard", "E268"},
            {"KeyboardWireless", "E269"},
            {"KeyboardWired", "E270"},
            {"Shield", "E271"},
            {"Ring", "02DA"},
            {"Cake", "E273"},
            {"Drink", "E274"},
            {"Beer", "E275"},
            {"FastFood", "E276"},
            {"Cutlery", "E277"},
            {"Pizza", "E278"},
            {"BirthdayCake", "E279"},
            {"Tablet", "E280"},
            {"Settings", "E281"},
            {"Bullets", "E282"},
            {"Cardio", "E283"},
            {"TShirt", "E284"},
            {"Pants", "E285"},
            {"Sweater", "E286"},
            {"Fabric", "E287"},
            {"Leather", "E288"},
            {"Scissors", "E289"},
            {"Bomb", "1F4A3"},
            {"Skull", "1F480"},
            {"Celebration", "E292"},
            {"TeaKettle", "E293"},
            {"FrenchPress", "E294"},
            {"CoffeCup", "E295"},
            {"Pot", "E296"},
            {"Grater", "E297"},
            {"Kettle", "E298"},
            {"Hospital", "1F3E5"},
            {"HospitalH", "E300"},
            {"Microphone", "1F3A4"},
            {"Webcam", "E302"},
            {"TempleChristianityChurch", "E303"},
            {"TempleIslam", "E304"},
            {"TempleHindu", "E305"},
            {"TempleBuddhist", "E306"},
            {"Bicycle", "1F6B2"},
            {"LifePreserver", "E308"},
            {"ShareAlt", "E309"},
            {"Comments", "E310"},
            {"Flower", "2698"},
            {"Baseball", "26BE"},
            {"Rugby", "E313"},
            {"Ax", "E314"},
            {"TableTennis", "E315"},
            {"Bowling", "1F3B3"},
            {"TreeConifer", "E317"},
            {"TreeDeciduous", "E318"},
            {"MoreItems", "E319"},
            {"Sort", "E320"},
            {"Filter", "E321"},
            {"Gamepad", "E322"},
            {"PlayingDices", "E323"},
            {"Calculator", "E324"},
            {"Tie", "E325"},
            {"Wallet", "E326"},
            {"Piano", "E327"},
            {"Sampler", "E328"},
            {"Podium", "E329"},
            {"SoccerBall", "E330"},
            {"Blog", "E331"},
            {"Dashboard", "E332"},
            {"Certificate", "E333"},
            {"Bell", "1F514"},
            {"Candle", "E335"},
            {"Pushpin", "1F4CC"},
            {"IphoneShake", "E337"},
            {"PinFlag", "E338"},
            {"Turtle", "1F422"},
            {"Rabbit", "1F407"},
            {"Globe", "E341"},
            {"Briefcase", "1F4BC"},
            {"Hdd", "E343"},
            {"ThumbsUp", "E344"},
            {"ThumbsDown", "E345"},
            {"HandRight", "E346"},
            {"HandLeft", "E347"},
            {"HandUp", "E348"},
            {"HandDown", "E349"},
            {"Fullscreen", "E350"},
            {"ShoppingBag", "E351"},
            {"BookOpen", "E352"},
            {"Nameplate", "E353"},
            {"NameplateAlt", "E354"},
            {"Vases", "E355"},
            {"Bullhorn", "E356"},
            {"Dumbbell", "E357"},
            {"Suitcase", "E358"},
            {"FileImport", "E359"},
            {"FileExport", "E360"},
            {"Bug", "1F41B"},
            {"Crown", "1F451"},
            {"Smoking", "E363"},
            {"CloudUpload", "E364"},
            {"CloudDownload", "E365"},
            {"Restart", "E366"},
            {"SecurityCamera", "E367"},
            {"Expand", "E368"},
            {"Collapse", "E369"},
            {"CollapseTop", "E370"},
            {"GlobeAf", "E371"},
            {"Global", "E372"},
            {"Spray", "E373"},
            {"Nails", "E374"},
            {"ClawHammer", "E375"},
            {"ClassicHammer", "E376"},
            {"HandSaw", "E377"},
            {"Riflescope", "E378"},
            {"ElectricalSocketEu", "E379"},
            {"ElectricalSocketUs", "E380"},
            {"MessageForward", "E381"},
            {"CoatHanger", "E382"},
            {"Dress", "1F457"},
            {"Bathrobe", "E384"},
            {"Shirt", "E385"},
            {"Underwear", "E386"},
            {"LogIn", "E387"},
            {"LogOut", "E388"},
            {"Exit", "E389"},
            {"NewWindowAlt", "E390"},
            {"VideoSd", "E391"},
            {"VideoHd", "E392"},
            {"Subtitles", "E393"},
            {"SoundStereo", "E394"},
            {"SoundDolby", "E395"},
            {"Sound51", "E396"},
            {"Sound61", "E397"},
            {"Sound71", "E398"},
            {"CopyrightMark", "E399"},
            {"RegistrationMark", "E400"},
            {"Radar", "E401"},
            {"Skateboard", "E402"},
            {"GolfCourse", "E403"},
            {"Sorting", "E404"},
            {"SortByAlphabet", "E405"},
            {"SortByAlphabetAlt", "E406"},
            {"SortByOrder", "E407"},
            {"SortByOrderAlt", "E408"},
            {"SortByAttributes", "E409"},
            {"SortByAttributesAlt", "E410"},
            {"Compressed", "E411"},
            {"Package", "1F4E6"},
            {"CloudPlus", "E413"},
            {"CloudMinus", "E414"},
            {"DiskSave", "E415"},
            {"DiskOpen", "E416"},
            {"DiskSaved", "E417"},
            {"DiskRemove", "E418"},
            {"DiskImport", "E419"},
            {"DiskExport", "E420"},
            {"Tower", "E421"},
            {"Send", "E422"},
            {"GitBranch", "E423"},
            {"GitCreate", "E424"},
            {"GitPrivate", "E425"},
            {"GitDelete", "E426"},
            {"GitMerge", "E427"},
            {"GitPullRequest", "E428"},
            {"GitCompare", "E429"},
            {"GitCommit", "E430"},
            {"ConstructionCone", "E431"},
            {"ShoeSteps", "E432"},
            {"Plus", "E433"},
            {"Minus", "E434"},
            {"Redo", "E435"},
            {"Undo", "E436"},
            {"Golf", "E437"},
            {"Hockey", "E438"},
            {"Pipe", "E439"},
            {"Wrench", "1F527"},
            {"FolderClosed", "E441"},
            {"PhoneAlt", "E442"},
            {"Earphone", "E443"},
            {"FloppyDisk", "E444"},
            {"FloppySaved", "E445"},
            {"FloppyRemove", "E446"},
            {"FloppySave", "E447"},
            {"FloppyOpen", "E448"},
            {"Translate", "E449"},
            {"Fax", "E450"},
            {"Factory", "1F3ED"},
            {"ShopWindow", "E452"},
            {"Shop", "E453"},
            {"Kiosk", "E454"},
            {"KioskWheels", "E455"},
            {"KioskLight", "E456"},
            {"KioskFood", "E457"},
            {"Transfer", "E458"},
            {"Money", "E459"},
            {"Header", "E460"},
            {"Blacksmith", "E461"},
            {"SawBlade", "E462"},
            {"Basketball", "E463"},
            {"Server", "E464"},
            {"ServerPlus", "E465"},
            {"ServerMinus", "E466"},
            {"ServerBan", "E467"},
            {"ServerFlag", "E468"},
            {"ServerLock", "E469"},
            {"ServerNew", "E470"},
            {"MenuHamburger", "E517"},

            {"HalflingsGlass", "E001"},
            {"HalflingsMusic", "E002"},
            {"HalflingsSearch", "E003"},
            {"HalflingsEnvelope", "2709"},
            {"HalflingsHeart", "E005"},
            {"HalflingsStar", "E006"},
            {"HalflingsStarEmpty", "E007"},
            {"HalflingsUser", "E008"},
            {"HalflingsFilm", "E009"},
            {"HalflingsThLarge", "E010"},
            {"HalflingsTh", "E011"},
            {"HalflingsThList", "E012"},
            {"HalflingsOk", "E013"},
            {"HalflingsRemove", "E014"},
            {"HalflingsZoomIn", "E015"},
            {"HalflingsZoomOut", "E016"},
            {"HalflingsOff", "E017"},
            {"HalflingsSignal", "E018"},
            {"HalflingsCog", "E019"},
            {"HalflingsTrash", "E020"},
            {"HalflingsHome", "E021"},
            {"HalflingsFile", "E022"},
            {"HalflingsTime", "E023"},
            {"HalflingsRoad", "E024"},
            {"HalflingsDownloadAlt", "E025"},
            {"HalflingsDownload", "E026"},
            {"HalflingsUpload", "E027"},
            {"HalflingsInbox", "E028"},
            {"HalflingsPlayCircle", "E029"},
            {"HalflingsRepeat", "E030"},
            {"HalflingsRefresh", "E031"},
            {"HalflingsListAlt", "E032"},
            {"HalflingsLock", "1F512"},
            {"HalflingsFlag", "E034"},
            {"HalflingsHeadphones", "E035"},
            {"HalflingsVolumeOff", "E036"},
            {"HalflingsVolumeDown", "E037"},
            {"HalflingsVolumeUp", "E038"},
            {"HalflingsQrcode", "E039"},
            {"HalflingsBarcode", "E040"},
            {"HalflingsTag", "E041"},
            {"HalflingsTags", "E042"},
            {"HalflingsBook", "E043"},
            {"HalflingsBookmark", "1F516"},
            {"HalflingsPrint", "E045"},
            {"HalflingsCamera", "1F4F7"},
            {"HalflingsFont", "E047"},
            {"HalflingsBold", "E048"},
            {"HalflingsItalic", "E049"},
            {"HalflingsTextHeight", "E050"},
            {"HalflingsTextWidth", "E051"},
            {"HalflingsAlignLeft", "E052"},
            {"HalflingsAlignCenter", "E053"},
            {"HalflingsAlignRight", "E054"},
            {"HalflingsAlignJustify", "E055"},
            {"HalflingsList", "E056"},
            {"HalflingsIndentLeft", "E057"},
            {"HalflingsIndentRight", "E058"},
            {"HalflingsFacetimeVideo", "E059"},
            {"HalflingsPicture", "E060"},
            {"HalflingsPencil", "270F"},
            {"HalflingsMapMarker", "E062"},
            {"HalflingsAdjust", "E063"},
            {"HalflingsTint", "E064"},
            {"HalflingsEdit", "E065"},
            {"HalflingsShare", "E066"},
            {"HalflingsCheck", "E067"},
            {"HalflingsMove", "E068"},
            {"HalflingsStepBackward", "E069"},
            {"HalflingsFastBackward", "E070"},
            {"HalflingsBackward", "E071"},
            {"HalflingsPlay", "E072"},
            {"HalflingsPause", "E073"},
            {"HalflingsStop", "E074"},
            {"HalflingsForward", "E075"},
            {"HalflingsFastForward", "E076"},
            {"HalflingsStepForward", "E077"},
            {"HalflingsEject", "E078"},
            {"HalflingsChevronLeft", "E079"},
            {"HalflingsChevronRight", "E080"},
            {"HalflingsPlusSign", "E081"},
            {"HalflingsMinusSign", "E082"},
            {"HalflingsRemoveSign", "E083"},
            {"HalflingsOkSign", "E084"},
            {"HalflingsQuestionSign", "E085"},
            {"HalflingsInfoSign", "E086"},
            {"HalflingsScreenshot", "E087"},
            {"HalflingsRemoveCircle", "E088"},
            {"HalflingsOkCircle", "E089"},
            {"HalflingsBanCircle", "E090"},
            {"HalflingsArrowLeft", "E091"},
            {"HalflingsArrowRight", "E092"},
            {"HalflingsArrowUp", "E093"},
            {"HalflingsArrowDown", "E094"},
            {"HalflingsShareAlt", "E095"},
            {"HalflingsResizeFull", "E096"},
            {"HalflingsResizeSmall", "E097"},
            {"HalflingsPlus", "002B"},
            {"HalflingsMinus", "2212"},
            {"HalflingsAsterisk", "002A"},
            {"HalflingsExclamationSign", "E101"},
            {"HalflingsGift", "E102"},
            {"HalflingsLeaf", "E103"},
            {"HalflingsFire", "1F525"},
            {"HalflingsEyeOpen", "E105"},
            {"HalflingsEyeClose", "E106"},
            {"HalflingsWarningSign", "E107"},
            {"HalflingsPlane", "E108"},
            {"HalflingsCalendar", "1F4C5"},
            {"HalflingsRandom", "E110"},
            {"HalflingsComments", "E111"},
            {"HalflingsMagnet", "E112"},
            {"HalflingsChevronUp", "E113"},
            {"HalflingsChevronDown", "E114"},
            {"HalflingsRetweet", "E115"},
            {"HalflingsShoppingCart", "E116"},
            {"HalflingsFolderClose", "E117"},
            {"HalflingsFolderOpen", "E118"},
            {"HalflingsResizeVertical", "E119"},
            {"HalflingsResizeHorizontal", "E120"},
            {"HalflingsHdd", "E121"},
            {"HalflingsBullhorn", "E122"},
            {"HalflingsBell", "1F514"},
            {"HalflingsCertificate", "E124"},
            {"HalflingsThumbsUp", "E125"},
            {"HalflingsThumbsDown", "E126"},
            {"HalflingsHandRight", "E127"},
            {"HalflingsHandLeft", "E128"},
            {"HalflingsHandTop", "E129"},
            {"HalflingsHandDown", "E130"},
            {"HalflingsCircleArrowRight", "E131"},
            {"HalflingsCircleArrowLeft", "E132"},
            {"HalflingsCircleArrowTop", "E133"},
            {"HalflingsCircleArrowDown", "E134"},
            {"HalflingsGlobe", "E135"},
            {"HalflingsWrench", "1F527"},
            {"HalflingsTasks", "E137"},
            {"HalflingsFilter", "E138"},
            {"HalflingsBriefcase", "1F4BC"},
            {"HalflingsFullscreen", "E140"},
            {"HalflingsDashboard", "E141"},
            {"HalflingsPaperclip", "1F4CE"},
            {"HalflingsHeartEmpty", "E143"},
            {"HalflingsLink", "E144"},
            {"HalflingsPhone", "E145"},
            {"HalflingsPushpin", "1F4CC"},
            {"HalflingsEuro", "20AC"},
            {"HalflingsUsd", "E148"},
            {"HalflingsGbp", "E149"},
            {"HalflingsSort", "E150"},
            {"HalflingsSortByAlphabet", "E151"},
            {"HalflingsSortByAlphabetAlt", "E152"},
            {"HalflingsSortByOrder", "E153"},
            {"HalflingsSortByOrderAlt", "E154"},
            {"HalflingsSortByAttributes", "E155"},
            {"HalflingsSortByAttributesAlt", "E156"},
            {"HalflingsUnchecked", "E157"},
            {"HalflingsExpand", "E158"},
            {"HalflingsCollapse", "E159"},
            {"HalflingsCollapseTop", "E160"},
            {"HalflingsLogIn", "E161"},
            {"HalflingsFlash", "E162"},
            {"HalflingsLogOut", "E163"},
            {"HalflingsNewWindow", "E164"},
            {"HalflingsRecord", "E165"},
            {"HalflingsSave", "E166"},
            {"HalflingsOpen", "E167"},
            {"HalflingsSaved", "E168"},
            {"HalflingsImport", "E169"},
            {"HalflingsExport", "E170"},
            {"HalflingsSend", "E171"},
            {"HalflingsFloppyDisk", "E172"},
            {"HalflingsFloppySaved", "E173"},
            {"HalflingsFloppyRemove", "E174"},
            {"HalflingsFloppySave", "E175"},
            {"HalflingsFloppyOpen", "E176"},
            {"HalflingsCreditCard", "E177"},
            {"HalflingsTransfer", "E178"},
            {"HalflingsCutlery", "E179"},
            {"HalflingsHeader", "E180"},
            {"HalflingsCompressed", "E181"},
            {"HalflingsEarphone", "E182"},
            {"HalflingsPhoneAlt", "E183"},
            {"HalflingsTower", "E184"},
            {"HalflingsStats", "E185"},
            {"HalflingsSdVideo", "E186"},
            {"HalflingsHdVideo", "E187"},
            {"HalflingsSubtitles", "E188"},
            {"HalflingsSoundStereo", "E189"},
            {"HalflingsSoundDolby", "E190"},
            {"HalflingsSound51", "E191"},
            {"HalflingsSound61", "E192"},
            {"HalflingsSound71", "E193"},
            {"HalflingsCopyrightMark", "E194"},
            {"HalflingsRegistrationMark", "E195"},
            {"HalflingsCloud", "2601"},
            {"HalflingsCloudDownload", "E197"},
            {"HalflingsCloudUpload", "E198"},
            {"HalflingsTreeConifer", "E199"},
            {"HalflingsTreeDeciduous", "E200"},

            {"SocialPinterest", "E001"},
            {"SocialDropbox", "E002"},
            {"SocialGooglePlus", "E003"},
            {"SocialJolicloud", "E004"},
            {"SocialYahoo", "E005"},
            {"SocialBlogger", "E006"},
            {"SocialPicasa", "E007"},
            {"SocialAmazon", "E008"},
            {"SocialTumblr", "E009"},
            {"SocialWordpress", "E010"},
            {"SocialInstapaper", "E011"},
            {"SocialEvernote", "E012"},
            {"SocialXing", "E013"},
            {"SocialZootool", "E014"},
            {"SocialDribbble", "E015"},
            {"SocialDeviantart", "E016"},
            {"SocialReadItLater", "E017"},
            {"SocialLinkedIn", "E018"},
            {"SocialForrst", "E019"},
            {"SocialPinboard", "E020"},
            {"SocialBehance", "E021"},
            {"SocialGithub", "E022"},
            {"SocialYoutube", "E023"},
            {"SocialSkitch", "E024"},
            {"SocialFoursquare", "E025"},
            {"SocialQuora", "E026"},
            {"SocialBadoo", "E027"},
            {"SocialSpotify", "E028"},
            {"SocialStumbleupon", "E029"},
            {"SocialReadability", "E030"},
            {"SocialFacebook", "E031"},
            {"SocialTwitter", "E032"},
            {"SocialInstagram", "E033"},
            {"SocialPosterousSpaces", "E034"},
            {"SocialVimeo", "E035"},
            {"SocialFlickr", "E036"},
            {"SocialLastFm", "E037"},
            {"SocialRss", "E038"},
            {"SocialSkype", "E039"},
            {"SocialEMail", "E040"},
            {"SocialVine", "E041"},
            {"SocialMyspace", "E042"},
            {"SocialGoodreads", "E043"},
            {"SocialApple", "F8FF"},
            {"SocialWindows", "E045"},
            {"SocialYelp", "E046"},
            {"SocialPlaystation", "E047"},
            {"SocialXbox", "E048"},
            {"SocialAndroid", "E049"},
            {"SocialIos", "E050"},

            {"FiletypesTxt", "E001"},
            {"FiletypesDoc", "E002"},
            {"FiletypesRtf", "E003"},
            {"FiletypesLog", "E004"},
            {"FiletypesTex", "E005"},
            {"FiletypesMsg", "E006"},
            {"FiletypesText", "E007"},
            {"FiletypesWpd", "E008"},
            {"FiletypesWps", "E009"},
            {"FiletypesDocx", "E010"},
            {"FiletypesPage", "E011"},
            {"FiletypesCsv", "E012"},
            {"FiletypesDat", "E013"},
            {"FiletypesTar", "E014"},
            {"FiletypesXml", "E015"},
            {"FiletypesVcf", "E016"},
            {"FiletypesPps", "E017"},
            {"FiletypesKey", "1F511"},
            {"FiletypesPpt", "E019"},
            {"FiletypesPptx", "E020"},
            {"FiletypesSdf", "E021"},
            {"FiletypesGbr", "E022"},
            {"FiletypesGed", "E023"},
            {"FiletypesMp3", "E024"},
            {"FiletypesM4a", "E025"},
            {"FiletypesWaw", "E026"},
            {"FiletypesWma", "E027"},
            {"FiletypesMpa", "E028"},
            {"FiletypesIff", "E029"},
            {"FiletypesAif", "E030"},
            {"FiletypesRa", "E031"},
            {"FiletypesMid", "E032"},
            {"FiletypesM3v", "E033"},
            {"FiletypesE3gp", "E034"},
            {"FiletypesShf", "E035"},
            {"FiletypesAvi", "E036"},
            {"FiletypesAsx", "E037"},
            {"FiletypesMp4", "E038"},
            {"FiletypesE3g2", "E039"},
            {"FiletypesMpg", "E040"},
            {"FiletypesAsf", "E041"},
            {"FiletypesVob", "E042"},
            {"FiletypesWmv", "E043"},
            {"FiletypesMov", "E044"},
            {"FiletypesSrt", "E045"},
            {"FiletypesM4v", "E046"},
            {"FiletypesFlv", "E047"},
            {"FiletypesRm", "E048"},
            {"FiletypesPng", "E049"},
            {"FiletypesPsd", "E050"},
            {"FiletypesPsp", "E051"},
            {"FiletypesJpg", "E052"},
            {"FiletypesTif", "E053"},
            {"FiletypesTiff", "E054"},
            {"FiletypesGif", "E055"},
            {"FiletypesBmp", "E056"},
            {"FiletypesTga", "E057"},
            {"FiletypesThm", "E058"},
            {"FiletypesYuv", "E059"},
            {"FiletypesDds", "E060"},
            {"FiletypesAi", "E061"},
            {"FiletypesEps", "E062"},
            {"FiletypesPs", "E063"},
            {"FiletypesSvg", "E064"},
            {"FiletypesPdf", "E065"},
            {"FiletypesPct", "E066"},
            {"FiletypesIndd", "E067"},
            {"FiletypesXlr", "E068"},
            {"FiletypesXls", "E069"},
            {"FiletypesXlsx", "E070"},
            {"FiletypesDb", "E071"},
            {"FiletypesDbf", "E072"},
            {"FiletypesMdb", "E073"},
            {"FiletypesPdb", "E074"},
            {"FiletypesSql", "E075"},
            {"FiletypesAacd", "E076"},
            {"FiletypesApp", "E077"},
            {"FiletypesExe", "E078"},
            {"FiletypesCom", "E079"},
            {"FiletypesBat", "E080"},
            {"FiletypesApk", "E081"},
            {"FiletypesJar", "E082"},
            {"FiletypesHsf", "E083"},
            {"FiletypesPif", "E084"},
            {"FiletypesVb", "E085"},
            {"FiletypesCgi", "E086"},
            {"FiletypesCss", "E087"},
            {"FiletypesJs", "E088"},
            {"FiletypesPhp", "E089"},
            {"FiletypesXhtml", "E090"},
            {"FiletypesHtm", "E091"},
            {"FiletypesHtml", "E092"},
            {"FiletypesAsp", "E093"},
            {"FiletypesCer", "E094"},
            {"FiletypesJsp", "E095"},
            {"FiletypesCfm", "E096"},
            {"FiletypesAspx", "E097"},
            {"FiletypesRss", "E098"},
            {"FiletypesCsr", "E099"},
            {"FiletypesLess", "003C"},
            {"FiletypesOtf", "E101"},
            {"FiletypesTtf", "E102"},
            {"FiletypesFont", "E103"},
            {"FiletypesFnt", "E104"},
            {"FiletypesEot", "E105"},
            {"FiletypesWoff", "E106"},
            {"FiletypesZip", "E107"},
            {"FiletypesZipx", "E108"},
            {"FiletypesRar", "E109"},
            {"FiletypesTarg", "E110"},
            {"FiletypesSitx", "E111"},
            {"FiletypesDeb", "E112"},
            {"FiletypesE7z", "E113"},
            {"FiletypesPkg", "E114"},
            {"FiletypesRpm", "E115"},
            {"FiletypesCbr", "E116"},
            {"FiletypesGz", "E117"},
            {"FiletypesDmg", "E118"},
            {"FiletypesCue", "E119"},
            {"FiletypesBin", "E120"},
            {"FiletypesIso", "E121"},
            {"FiletypesHdf", "E122"},
            {"FiletypesVcd", "E123"},
            {"FiletypesBak", "E124"},
            {"FiletypesTmp", "E125"},
            {"FiletypesIcs", "E126"},
            {"FiletypesMsi", "E127"},
            {"FiletypesCfg", "E128"},
            {"FiletypesIni", "E129"},
            {"FiletypesPrf", "E130"},
        };
    }
}