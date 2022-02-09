using Roundpay_Robo.AppCode.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.Models;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Roundpay_Robo.AppCode.StaticModel;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface IRequestInfo
    {
        string GetRemoteIP();
        string GetLocalIP();
        string GetBrowser();
        string GetBrowserVersion();
        string GetUserAgent();
        string GetBrowserFullInfo();
        string GetDomain(IConfiguration Configuration);
        CurrentRequestInfo GetCurrentReqInfo();
       // string GetUserAgentMD5();

    }
    public interface IResourceML
    {        
        void CreateWebsiteDirectory(int WID, string _FolderType);
        StringBuilder GetLogoURL(int WID);


    }
    public interface IBannerML
    {
       
    }
}
