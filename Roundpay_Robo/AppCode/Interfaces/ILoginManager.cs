using Roundpay_Robo.AppCode.Model;

using Roundpay_Robo.AppCode.Model.App;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface ILoginML
    {
        WebsiteInfo GetWebsiteInfo();

        ResponseStatus DoLogin(LoginDetail loginDetail);
        Task<IResponseStatus> DoLogout(LogoutReq logoutReq);
        bool IsInValidSession();
    }
    public interface ICreateUserML
    {
        ResponseStatus CallCreateUser(UserCreate _req);
    }
    public interface IResponseStatus
    {
        int Statuscode { get; set; }
        string Msg { get; set; }
        int CommonInt { get; set; }
        int CommonInt2 { get; set; }
        string CommonStr { get; set; }
        string CommonStr2 { get; set; }
        string CommonStr3 { get; set; }
        string CommonStr4 { get; set; }
        bool CommonBool { get; set; }
        string ReffID { get; set; }
        int ErrorCode { get; set; }
        string ErrorMsg { get; set; }
        int Status { get; set; }

    }

}
