using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using ACRM.mobile.DataAccess;
using ACRM.mobile.Domain.ActionTemplates;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Application.ActionTemplates;
using ACRM.mobile.Domain.Configuration.DataModel;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Domain.FormatUtils;
using ACRM.mobile.Logging;
using ACRM.mobile.Services.Contracts;
using ACRM.mobile.Services.Extensions;
using ACRM.mobile.Services.SubComponents;
using Newtonsoft.Json.Linq;

namespace ACRM.mobile.Services
{
    public class ReportingService : ContentServiceBase, IReportingService
    {
        private readonly IDocumentService _docServic;
        private readonly IModifyRecordService _modifyRecordService;
        protected readonly IFieldGroupDataService _fieldGroupDataService;
        protected readonly ILinkResolverService _linkResolverService;
        public string SignatureFile { get; set; }
        public int DocFieldId { get; set; } = -1;
        public Button ActionButton { get; set; }
        public bool SendByEmail { get; set; } = false;
        public bool SendByEmailAttachReport { get; set; } = false;
        public List<Menu> SendByEmailActionButtons { get; set; }
        private UserAction ActionButtonUserAction { get; set; }
        public Dictionary<string, string> SourceFieldGroupData { get; set; }
        Dictionary<string, string> defaultConfigDict = new Dictionary<string, string>
        {
            {"sign", "true"},
            {"upload", "true"},
            {"emptySignatureName", "Button:PleaseSign"},
            {"signatureImageTagName", "SignatureImage"},
            {"signatureImageId", "img-signature"},
            {"signedReportFileName", "SampleReport.pdf"},
            {"signedReportFileNameDateFormat", "yyyy-MM-dd"}
        };
        public string XMLData { get; set; }
        public string GetReportConfig(string key)
        {
            if (defaultConfigDict.Keys.Contains(key))
            {
                return defaultConfigDict[key];
            }
            else
            {
                return null;
            }

        }
        public void SetReportConfig(string key, string value)
        {
            if (defaultConfigDict.Keys.Contains(key))
            {
                defaultConfigDict[key] = value;
            }
            else
            {
                defaultConfigDict.Add(key, value);
            }

        }

        public ReportingService(ISessionContext sessionContext,
            IConfigurationService configurationService,
            ICrmDataService crmDataService,
            ILogService logService,
            IUserActionBuilder userActionBuilder,
            HeaderComponent headerComponent,
            FieldGroupComponent fieldGroupComponent,
            ImageResolverComponent imageResolverComponent,
            IFilterProcessor filterProcessor,
            IDocumentService docServic,
            IModifyRecordService modifyRecordService,
            IFieldGroupDataService fieldGroupDataService,
            ILinkResolverService linkResolverService) : base(sessionContext, configurationService,
                crmDataService, logService, userActionBuilder, headerComponent, fieldGroupComponent, imageResolverComponent, filterProcessor)
        {
            _docServic = docServic;
            _modifyRecordService = modifyRecordService;
            _fieldGroupDataService = fieldGroupDataService;
            _linkResolverService = linkResolverService;
        }
        public override async Task PrepareContentAsync(CancellationToken cancellationToken)
        {
            if (_action != null)
            {
                if (_action.ViewReference.Name.Equals("ClientReportWithAction", StringComparison.InvariantCultureIgnoreCase))
                {
                    var buttonName = _action?.ViewReference?.GetArgumentValue("ButtonName");
                    ActionButton = await _configurationService.GetButton(buttonName, cancellationToken);
                    if (ActionButton != null)
                    {
                        var _buttonUserAction = _userActionBuilder.UserActionFromButton(_configurationService, ActionButton, _action?.RecordId, _action?.SourceInfoArea);
                        if (_buttonUserAction != null)
                        {
                            ActionButtonUserAction = _buttonUserAction;
                            _modifyRecordService.SetSourceAction(_buttonUserAction);
                            await _modifyRecordService.PrepareContentAsync(cancellationToken);
                        }
                    }
                    var signingConfig = _action?.ViewReference?.GetArgumentValue("SigningConfig");
                    if (!string.IsNullOrWhiteSpace(signingConfig))
                    {
                        JObject signjson = JObject.Parse(signingConfig);
                        if (signjson != null && signjson.HasValues)
                        {
                            foreach (var config in signjson)
                            {
                                string key = config.Key;
                                JToken value = config.Value;
                                SetReportConfig(key, value.Value<string>());

                            }
                        }

                    }

                }

                var sendByEmailValue = _action?.ViewReference?.GetArgumentValue("SendByEmail");
                if (sendByEmailValue != null && sendByEmailValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    var SendByEmailAttachReportValue = _action?.ViewReference?.GetArgumentValue("SendByEmailAttachReport");
                    var SendByEmailActionValues = _action?.ViewReference?.GetArgumentValue("SendByEmailAction");
                    if (SendByEmailAttachReportValue != null && SendByEmailAttachReportValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SendByEmailAttachReport = true;
                    }
                    if (SendByEmailActionValues != null && !string.IsNullOrWhiteSpace(SendByEmailActionValues))
                    {
                        var sendByEmailActions = SendByEmailActionValues.Split(',');
                        if (sendByEmailActions?.Length > 0)
                        {
                            SendByEmailActionButtons = new List<Menu>();
                            foreach (var item in sendByEmailActions)
                            {
                                var menu = await _configurationService.GetMenu(item, cancellationToken);
                                if (menu != null)
                                {
                                    SendByEmailActionButtons.Add(menu);

                                }
                            }
                            SendByEmail = true;
                        }
                    }
                }
            }


        }

        public async Task<string> GetFileResourcePath(string fileName, CancellationToken cancellationToken)
        {
            string imageViewName = fileName;

            if (!string.IsNullOrWhiteSpace(imageViewName))
            {
                if (imageViewName.Contains("{language}"))
                {
                    imageViewName = imageViewName.Replace("{language}", _sessionContext.LanguageCode);
                }


                ConfigResource configResource = _configurationService.GetConfigResource(imageViewName);
                if (configResource != null)
                {
                    if (configResource != null && !string.IsNullOrWhiteSpace(configResource.FileName))
                    {
                        return _sessionContext.ResourcePath(configResource.FileName);
                    }
                }
                else
                {
                    return await _crmDataService.GetDocumentPath(imageViewName, cancellationToken);
                }
            }

            return null;
        }
        public string TransformXMLToHTML(string inputXml, string xsltString)
        {
            XslCompiledTransform transform = new XslCompiledTransform();
            using (XmlReader reader = XmlReader.Create(new StringReader(xsltString)))
            {
                transform.Load(reader);
            }
            StringWriter results = new StringWriter();
            using (XmlReader reader = XmlReader.Create(new StringReader(inputXml)))
            {
                transform.Transform(reader, null, results);
            }
            return results.ToString();
        }

        public string ProcessToken(string inputText, Dictionary<string, string> fieldGroupData = null)
        {
            string result = inputText;
            if(string.IsNullOrWhiteSpace(inputText))
            {
                return inputText;
            }

            if (fieldGroupData != null)
            {
                foreach (var key in fieldGroupData.Keys)
                {
                    var formatedKey = "{$" + key + "}";
                    if (result.Contains(formatedKey))
                    {
                        result = result.Replace(formatedKey, fieldGroupData[key]);
                    }
                }
            }

            return result;
        }

        public async Task<string> GetReportDoc(CancellationToken token)
        {
            string fileUrl = string.Empty;
            if (_action != null)
            {
                var documentUploadConfig = _action?.ViewReference?.GetArgumentValue("DocumentUploadConfiguration");
                if (!string.IsNullOrWhiteSpace(documentUploadConfig))
                {
                    var ViewRef = await _configurationService.GetViewForMenu(documentUploadConfig, token);
                    var docFieldGroup = ViewRef?.GetArgumentValue("DocumentFieldFieldGroupName");
                    if (!string.IsNullOrWhiteSpace(docFieldGroup))
                    {
                        var (docKey, docFieldId) = await getDocumentKey(docFieldGroup, token);
                        DocFieldId = docFieldId;
                        if (!string.IsNullOrWhiteSpace(docKey))
                        {
                            var reportDocPath = getReportPath(docKey);
                            if (string.IsNullOrEmpty(reportDocPath))
                            {
                                var filePath = await _docServic.GetDocumentPath(docKey, token);
                                if (!string.IsNullOrWhiteSpace(filePath))
                                {
                                    reportDocPath = await copyReportDoc(docKey, filePath, token);

                                }
                            }
                            fileUrl = reportDocPath;
                        }
                    }

                }
            }
            return fileUrl;
        }

        private async Task<string> copyReportDoc(string docKey, string filePath, CancellationToken token)
        {
            if (!Directory.Exists(_sessionContext.ReportFolder()))
            {
                Directory.CreateDirectory(_sessionContext.ReportFolder());
            }
            string reportPath = _sessionContext.ReportPath($"{docKey}.pdf");
            if (File.Exists(filePath))
            {
                File.Copy(filePath, reportPath, true);
                return reportPath;
            }
            return string.Empty;
        }

        private string getReportPath(string docKey)
        {
            string path = _sessionContext.ReportPath($"{docKey}.pdf");
            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task<(string, int)> getDocumentKey(string docFieldGroup, CancellationToken token)
        {
            string docKey = string.Empty;
            int docFieldId = -1;
            try
            {

                string recordId = _action.RecordId;
                string sourceInfoArea = _action.SourceInfoArea;

                var fieldControl = await _configurationService.GetFieldControl(docFieldGroup + ".Edit", token).ConfigureAwait(false);

                string infoAreaId = fieldControl?.InfoAreaId;

                if (!string.IsNullOrEmpty(infoAreaId) && !string.IsNullOrWhiteSpace(recordId) && infoAreaId.Equals(_action.SourceInfoArea, StringComparison.InvariantCultureIgnoreCase))
                {
                    var InfoArea = _configurationService.GetInfoArea(infoAreaId);
                    _infoArea = InfoArea;

                    recordId = recordId.FormatedRecordId(sourceInfoArea);

                    var tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, token).ConfigureAwait(false);

                    if (fieldControl != null)
                    {
                        _fieldGroupComponent.InitializeContext(fieldControl, tableInfo);
                        if (_fieldGroupComponent.HasTabs())
                        {

                            List<FieldControlField> fields = fieldControl.Tabs[0].GetQueryFields();

                            var rawData = await _crmDataService.GetData(token,
                                    new DataRequestDetails
                                    {
                                        TableInfo = tableInfo,
                                        Fields = fields,
                                        SortFields = fieldControl.SortFields,
                                        RecordId = recordId,
                                    },
                                    null, 1000, RequestMode.Fastest);

                            if (rawData.Result != null && rawData.Result.Rows.Count > 0)
                            {
                                var row = rawData.Result.Rows[0];
                                (docKey, docFieldId) = await getDocKey(row, fields, tableInfo, token);

                            }

                        }
                    }
                }

                return (docKey, docFieldId);

            }
            catch (Exception ex)
            {
                _logService.LogDebug($"Failed getDocumentKey with Exception:{ex.Message}");
            }
            return (docKey, docFieldId);
        }

        private async Task<(string, int)> getDocKey(DataRow row, List<FieldControlField> fields, TableInfo tableInfo, CancellationToken token)
        {
            string docKey;
            int? docFieldId = -1;
            var outRow = await _fieldGroupComponent.ExtractDisplayRow(fields, row, false, token);
            docKey = outRow?.Fields[0]?.Data.StringData;
            docFieldId = outRow?.Fields[0]?.Config.FieldConfig.FieldId;
            return (docKey, docFieldId.Value);
        }

        public async Task<string> GetHTMLReport(CancellationToken token)
        {
            string xslFileName = string.Empty;
            string contentHtml = string.Empty;
            if (_action != null)
            {
                xslFileName = _action?.ViewReference?.GetArgumentValue("Xsl");
            }

            if (!string.IsNullOrEmpty(xslFileName))
            {
                var pathXls = await GetFileResourcePath(xslFileName, token);

                XMLData = await GetXMLData(token);
                StringBuilder sb = new StringBuilder();
                sb.Append($"<?xml version=\"1.0\" encoding=\"UTF-8\"?><?xml-stylesheet type=\"text/xsl\" href=\"{pathXls}>\"?>");
                sb.Append(XMLData);
                string contentXls = File.ReadAllText(pathXls, Encoding.UTF8);
                /* - Load Sample XML
                var pathXml = await GetFileResourcePath("clientxmlSample", token);
                string newcontentXml = File.ReadAllText(pathXml, Encoding.UTF8);
                contentHtml = TransformXMLToHTML(newcontentXml, contentXls); */
                contentHtml = TransformXMLToHTML(sb.ToString(), contentXls);

                if (SourceFieldGroupData == null)
                {
                    OrganizerActionTemplate template = new OrganizerActionTemplate(_action.ViewReference);
                    SourceFieldGroupData = await _fieldGroupDataService.GetSourceFieldGroupData(_action.RecordId, template.CopySourceFieldGroupName(), RequestMode.Best, token);
                }
            }

            return contentHtml;
        }

        public async Task<string> GetXMLData(CancellationToken token)
        {
            if (_action != null)
            {
                var RootConfigName = _action?.ViewReference?.GetArgumentValue("ConfigName");
                var RootName = _action?.ViewReference?.GetArgumentValue("RootName");
                var rootXmlName = _action?.ViewReference?.GetArgumentValue("XmlRootElementName");

                var AdditionalConfigNames = _action?.ViewReference?.GetArgumentValue("AdditionalConfigNames");
                var AdditionalRootNames = _action?.ViewReference?.GetArgumentValue("AdditionalRootNames");
                var AdditionalConfigParentLinks = _action?.ViewReference?.GetArgumentValue("AdditionalConfigParentLinks");

                ClientReportNode rootNode = new ClientReportNode { ConfigName = RootConfigName, ParentLink = _action.SourceInfoArea, RootName = RootName };
                rootNode.ItemRow = await PrepareDataAsync(rootNode, token);
                List<ClientReportNode> nodes = new List<ClientReportNode> { rootNode };
                List<ClientReportNode> additionalNodes = await GetAdditionalNodes(AdditionalConfigNames, AdditionalRootNames, AdditionalConfigParentLinks, token);
                if (additionalNodes != null && additionalNodes.Count > 0)
                {
                    nodes.AddRange(additionalNodes);
                }
                XmlDocument xDoc = new XmlDocument();
                XmlElement xRoot = xDoc.CreateElement(rootXmlName);
                foreach (var node in nodes)
                {
                    XmlElement xnode = getXNode(xDoc, node);
                    xRoot.AppendChild(xnode);
                }
                var signingConfig = _action?.ViewReference?.GetArgumentValue("SigningConfig");
                if (GetReportConfig("sign").Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    xRoot.AppendChild(await GetSignatureNode(xDoc, token));
                }
                xRoot.AppendChild(getSettingsNode(xDoc));
                XmlElement dateToday = xDoc.CreateElement("DateToday");
                dateToday.InnerText = $"{DateTime.Now.Year}{DateTime.Now.Month:00}{DateTime.Now.Day:00}";
                xRoot.AppendChild(dateToday);
                xDoc.AppendChild(xRoot);
                return xDoc.OuterXml;

            }

            return string.Empty;

        }

        private async Task<XmlElement> GetSignatureNode(XmlDocument xDoc, CancellationToken token)
        {
            var signatureImageTagName = defaultConfigDict["signatureImageTagName"];
            XmlElement signatureImageNode = xDoc.CreateElement(signatureImageTagName);
            var emptySignatureKey = defaultConfigDict["emptySignatureName"];
            var signaturePath = string.Empty;
            if (!string.IsNullOrEmpty(SignatureFile))
            {
                signaturePath = SignatureFile;
            }
            else if (!string.IsNullOrEmpty(emptySignatureKey))
            {
                signaturePath = await GetFileResourcePath(emptySignatureKey, token);

            }
            signatureImageNode.InnerText = signaturePath;
            return signatureImageNode;
        }

        private XmlElement getSettingsNode(XmlDocument xDoc)
        {
            XmlElement localeSettings = xDoc.CreateElement("LocaleSettings");
            XmlElement dateFormat = xDoc.CreateElement("DateFormat");
            dateFormat.InnerText = "MM-DD-YYYY";
            localeSettings.AppendChild(dateFormat);
            XmlElement dateFormatShort = xDoc.CreateElement("DateFormatShort");
            dateFormatShort.InnerText = "MM-DD-YYYY";
            localeSettings.AppendChild(dateFormatShort);
            XmlElement timeFormat = xDoc.CreateElement("TimeFormat");
            timeFormat.InnerText = "h:mm:ss";
            localeSettings.AppendChild(timeFormat);
            XmlElement timeFormatShort = xDoc.CreateElement("TimeFormatShort");
            timeFormatShort.InnerText = "h:mm";
            localeSettings.AppendChild(timeFormatShort);
            XmlElement timezone = xDoc.CreateElement("Timezone");
            timezone.InnerText = "Asia/Kolkata";
            localeSettings.AppendChild(timezone);
            XmlElement thousandSeparator = xDoc.CreateElement("ThousandSeparator");
            thousandSeparator.InnerText = ",";
            localeSettings.AppendChild(thousandSeparator);
            XmlElement decimalSeparator = xDoc.CreateElement("DecimalSeparator");
            decimalSeparator.InnerText = ".";
            localeSettings.AppendChild(decimalSeparator);
            return localeSettings;
        }

        public async Task<List<ListDisplayRow>> PrepareDataAsync(ClientReportNode clientNode, CancellationToken token)
        {
            List<ListDisplayRow> records = new List<ListDisplayRow>();
            try
            {
                string configurationName;
                int linkId = -1;

                ParentLink parentLink = null;
                string recordId = null;
                configurationName = clientNode.ConfigName;
                if (!string.IsNullOrWhiteSpace(clientNode.ParentLink) && !clientNode.ParentLink.Equals("nolink", StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] configParts = clientNode.ParentLink.Split('#');
                    string parentInfoAreaId = null;
                    if (configParts.Length > 1)
                    {
                        parentInfoAreaId = configParts[0];
                        if (!int.TryParse(configParts[1], out linkId))
                        {
                            linkId = -1;
                        }
                    }
                    else
                    {
                        parentInfoAreaId = configParts[0];
                    }
                    if (parentInfoAreaId.Equals(_action.SourceInfoArea, StringComparison.InvariantCultureIgnoreCase))
                    {
                        parentLink = new ParentLink
                        {
                            LinkId = linkId,
                            ParentInfoAreaId = parentInfoAreaId,
                            RecordId = _action.RecordId,
                        };
                    }
                }


                //var searchAndList = await _configurationService.GetSearchAndList(searchAndListConfigurationName, token).ConfigureAwait(false);
                var fieldControl = await _configurationService.GetFieldControl(configurationName + ".List", token).ConfigureAwait(false);

                string infoAreaId = fieldControl?.InfoAreaId;

                if (!string.IsNullOrEmpty(infoAreaId))
                {
                    var InfoArea = _configurationService.GetInfoArea(infoAreaId);
                    _infoArea = InfoArea;

                    if (parentLink != null && parentLink.ParentInfoAreaId.Equals(infoAreaId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        recordId = parentLink.RecordId.FormatedRecordId(parentLink.ParentInfoAreaId);
                        parentLink = null;
                    }

                    var tableInfo = await _configurationService.GetTableInfoAsync(infoAreaId, token).ConfigureAwait(false);

                    if (fieldControl != null)
                    {
                        _fieldGroupComponent.InitializeContext(fieldControl, tableInfo);
                        if (_fieldGroupComponent.HasTabs())
                        {

                            List<FieldControlField> fields = fieldControl.Tabs[0].GetQueryFields();
                            //_expandComponent.InitializeContext(tab.InfoArea.UnitName, tab.InfoArea.UnitName, tab.TableInfo);

                            var rawData = await _crmDataService.GetData(token,
                                    new DataRequestDetails
                                    {
                                        TableInfo = tableInfo,
                                        Fields = fields,
                                        SortFields = fieldControl.SortFields,
                                        RecordId = recordId,
                                    },
                                    parentLink, 1000, RequestMode.Fastest);

                            if (rawData.Result != null)
                            {
                                var tasks = rawData.Result.Rows.Cast<DataRow>().Select(async row => await GetRow(fields, tableInfo, row, token));
                                records.AddRange(await Task.WhenAll(tasks).ConfigureAwait(false));
                            }

                        }
                    }
                }

                return records;

            }
            catch (Exception ex)
            {
                _logService.LogDebug($"Failed PrepareDataAsync with Exception:{ex.Message}");
            }
            return records;
        }


        private async Task<ListDisplayRow> GetRow(List<FieldControlField> fieldDefinitions, TableInfo tableInfo, DataRow row, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ListDisplayRow outRow = await _fieldGroupComponent.ExtractEditRow(fieldDefinitions, row, null, PanelType.EditPanel, cancellationToken);
            var recordId = row.GetColumnValue("recid", "-1");
            if (!string.IsNullOrWhiteSpace(recordId))
            {
                outRow.RecordId = recordId.FormatedRecordId(tableInfo.InfoAreaId);
                outRow.InfoAreaId = tableInfo.InfoAreaId;
            }
            return outRow;
        }

        private XmlElement getXNode(XmlDocument xDoc, ClientReportNode node)
        {
            XmlElement NodeRoot = xDoc.CreateElement(node.RootName);
            NodeRoot.SetAttribute("online", "false");
            XmlElement rows = xDoc.CreateElement("Rows");
            XmlElement columns = xDoc.CreateElement("Columns");
            if (node.ItemRow != null && node.ItemRow.Count > 0)
            {
                bool needColumnsLoaded = true;
                foreach (var item in node.ItemRow)
                {

                    XmlElement row = xDoc.CreateElement("Row");
                    row.SetAttribute("record", item.RecordId);
                    if (item.LinkedDisplayRows != null && item.LinkedDisplayRows.Count > 0)
                    {
                        foreach (var childInforArea in item.LinkedDisplayRows.Keys)
                        {
                            if (!string.IsNullOrWhiteSpace(item.LinkedDisplayRows[childInforArea]?.RecordId))
                            {
                                row.SetAttribute($"record{childInforArea}", item.LinkedDisplayRows[childInforArea].RecordId);
                            }
                        }
                    }

                    foreach (var field in item.Fields)
                    {
                        ProcessField(field, ref row, ref columns, needColumnsLoaded, xDoc);
                    }

                    if (item.LinkedDisplayRows != null && item.LinkedDisplayRows.Count > 0)
                    {
                        foreach (var childInforArea in item.LinkedDisplayRows.Keys)
                        {
                            var childRow = item.LinkedDisplayRows[childInforArea];
                            if (childRow != null)
                            {

                                foreach (var field in childRow.Fields)
                                {
                                    ProcessField(field, ref row, ref columns, needColumnsLoaded, xDoc);
                                }
                            }
                        }
                    }

                    needColumnsLoaded = false;
                    rows.AppendChild(row);
                }

            }
            NodeRoot.AppendChild(rows);
            NodeRoot.AppendChild(columns);
            return NodeRoot;
        }

        private void ProcessField(ListDisplayField field, ref XmlElement row, ref XmlElement columns, bool needColumnsLoaded, XmlDocument xDoc)
        {
            string InfoAreaTag = field.Config.FieldConfig.InfoAreaId;
            if (InfoAreaTag.Substring(0, 1).All(char.IsDigit))
            {
                InfoAreaTag = $"T{InfoAreaTag}";
            }
            var fieldTagKey = field.Config.FieldConfig.Function;
            if (string.IsNullOrWhiteSpace(fieldTagKey))
            {
                fieldTagKey = $"{InfoAreaTag}_F{field.Config.FieldConfig.FieldId}".ToUpper();
            }
            var rawValue = field.EditData.SelectedRawValue();
            var stringValue = field.Data.StringData;

            if (!string.IsNullOrWhiteSpace(rawValue) && !string.IsNullOrWhiteSpace(stringValue))
            {
                XmlElement fieldTag = xDoc.CreateElement(fieldTagKey);
                if (field.Config.PresentationFieldAttributes.IsSelectableInput())
                {
                    var extKeyStr = field.ExtKeyValue();
                    if (!string.IsNullOrWhiteSpace(extKeyStr))
                    {
                        fieldTag.SetAttribute("extKey", extKeyStr);
                    }
                    fieldTag.SetAttribute("value", rawValue);
                }
                else if (field.Config.PresentationFieldAttributes.IsNumeric)
                {
                    fieldTag.SetAttribute("value", rawValue);
                }
                else if (!rawValue.Equals(stringValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (field.Config.PresentationFieldAttributes.IsBoolean)
                    {
                        rawValue = rawValue.Equals("1") ? "true" : "false";

                    }
                    fieldTag.SetAttribute("value", rawValue);
                }


                fieldTag.InnerText = stringValue;
                row.AppendChild(fieldTag);
            }
            if (needColumnsLoaded)
            {
                XmlElement fieldColimnTag = xDoc.CreateElement(fieldTagKey);
                fieldColimnTag.InnerText = field.Config.PresentationFieldAttributes.FieldInfo.Name;
                columns.AppendChild(fieldColimnTag);
            }
        }

        private async Task<List<ClientReportNode>> GetAdditionalNodes(string additionalConfigNames, string additionalRootNames, string additionalConfigParentLinks, CancellationToken token)
        {
            List<ClientReportNode> additionalNodes = new List<ClientReportNode>();
            if (!string.IsNullOrEmpty(additionalConfigNames))
            {
                var configNames = additionalConfigNames.Split(';');
                if (configNames != null && configNames.Length > 0)
                {
                    var RootNames = additionalRootNames.Split(';');
                    var ParentLinks = additionalConfigParentLinks.Split(';');

                    for (int i = 0; i < configNames.Length; i++)
                    {
                        var config = configNames[i];
                        var RootName = RootNames.Length > i ? RootNames[i] : config;
                        var ParentLink = ParentLinks.Length > i ? ParentLinks[i] : "nolink";
                        var node = new ClientReportNode
                        {
                            ConfigName = config,
                            ParentLink = ParentLink,
                            RootName = RootName
                        };
                        node.ItemRow = await PrepareDataAsync(node, token);
                        additionalNodes.Add(node);
                    }
                }
            }
            return additionalNodes;
        }

        public async Task ExecuteReportAction(CancellationToken cancellationToken)
        {
            if (ActionButtonUserAction != null && !_modifyRecordService.IsBusy())
            {
                await _modifyRecordService.ModifyRecord(ActionButtonUserAction, cancellationToken);
            }
        }

        public async Task<EmailContent> GetEmailContentAsync(Menu menu, CancellationToken cancellationToken)
        {
            if (menu != null)
            {
                if (menu?.ViewReference != null && menu.ViewReference.Name.Equals("ClientEmail", StringComparison.InvariantCultureIgnoreCase))
                {
                    UserAction userAction = _userActionBuilder.UserActionFromMenu(_configurationService,
                        menu, _action?.RecordId, _action?.SourceInfoArea);
                    var EmailFieldgroup = userAction?.ViewReference.GetArgumentValue("EmailFieldgroup");
                    if(!string.IsNullOrWhiteSpace(EmailFieldgroup))
                    {
                        var fieldGroupData = await _fieldGroupDataService.GetSourceFieldGroupData(userAction.RecordId, EmailFieldgroup, RequestMode.Best, cancellationToken);

                        if(fieldGroupData?.Keys?.Count > 0)
                        {
                            var emailContent = new EmailContent();
                            if (fieldGroupData.ContainsKey("TOTEXT"))
                            {
                                emailContent.To = ProcessToken(fieldGroupData["TOTEXT"], fieldGroupData);
                            }
                            if (string.IsNullOrWhiteSpace(emailContent.To) && fieldGroupData.ContainsKey("TOTEXT2"))
                            {
                                emailContent.To = ProcessToken(fieldGroupData["TOTEXT2"], fieldGroupData);
                            }

                            if (fieldGroupData.ContainsKey("SUBJECTTEXT"))
                            {
                                emailContent.Subject = ProcessToken(fieldGroupData["SUBJECTTEXT"], fieldGroupData);
                            }

                            if (fieldGroupData.ContainsKey("CCTEXT"))
                            {
                                emailContent.Cc = ProcessToken(fieldGroupData["CCTEXT"], fieldGroupData);
                            }

                            if (fieldGroupData.ContainsKey("BCCTEXT"))
                            {
                                emailContent.Bcc = ProcessToken(fieldGroupData["BCCTEXT"], fieldGroupData);
                            }

                            if (fieldGroupData.ContainsKey("BODYTEXT"))
                            {
                                emailContent.Body = ProcessToken(fieldGroupData["BODYTEXT"], fieldGroupData);
                            }

                            return emailContent;

                        }

                    }

                }
            }

            return null;
        }

        public async Task<string> GetReportURL(CancellationToken token)
        {
            
            StringBuilder queryBuilder = new StringBuilder("Service=Report&ReportType=CoreReport");

            if (_action != null)
            {
                if (_action.ViewReference.Name.Equals("Report", StringComparison.InvariantCultureIgnoreCase))
                {
                    var parentLink = _action?.ViewReference?.GetArgumentValue("ParentLink");
                    var linkRecord = await _linkResolverService.GetLinkedRecordForAction(_action, parentLink, token);
                    var reportName = _action?.ViewReference?.GetArgumentValue("Report");
                    queryBuilder.Append($"&ReportName={reportName.UTF8Encoding()}");

                    if (!string.IsNullOrEmpty(linkRecord))
                    {
                        queryBuilder.Append($"&RecordIdentification={linkRecord}");
                    }

                    var reportPrivate = _action?.ViewReference?.GetArgumentValue("ReportPrivate");

                    if (!string.IsNullOrEmpty(reportPrivate))
                    {
                        queryBuilder.Append($"&ReportPrivate={reportPrivate}");
                    }

                    var parameters = _action?.ViewReference?.GetArgumentValue("Parameters");

                    if (!string.IsNullOrEmpty(parameters))
                    {
                        queryBuilder.Append($"&Parameters={parameters}");
                    }
                }
            }

            UriBuilder uriBuilder = new UriBuilder(_sessionContext.CrmInstance.UrlPath())
            {
                Query = queryBuilder.ToString()
            };

            return uriBuilder.ToString();
        }

    }
}
