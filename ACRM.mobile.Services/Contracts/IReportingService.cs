using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Services.Contracts
{
    public interface IReportingService : IContentServiceBase
    {
        Task<string> GetFileResourcePath(string fileName, CancellationToken cancellationToken);
        string TransformXMLToHTML(string inputXml, string xsltString);
        Task<string> GetHTMLReport(CancellationToken token);
        Task<string> GetXMLData(CancellationToken token);
        string XMLData { get; set; }
        Task<string> GetReportDoc(CancellationToken token);
        Button ActionButton { get; set; }
        string GetReportConfig(string key);
        string SignatureFile { get; set; }
        int DocFieldId { get; set; }
        Task ExecuteReportAction(CancellationToken cancellationToken);
        string ProcessToken(string inputText, Dictionary<string, string> fieldGroupData = null);
        bool SendByEmail { get; set; }
        bool SendByEmailAttachReport { get; set; }
        List<Menu> SendByEmailActionButtons { get; set; }
        Dictionary<string, string> SourceFieldGroupData { get; set; }
        Task<string> GetReportURL(CancellationToken token);
        Task<EmailContent> GetEmailContentAsync(Menu menu, CancellationToken cancellationToken);
    }
}
