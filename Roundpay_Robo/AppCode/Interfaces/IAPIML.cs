using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode.Model.ProcModel;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface IAPIML
    {
        APIDetail GetAPIDetailByID(int apiId);
        IEnumerable<APIDetail> GetAPIDetail(int APIId = -1);
        IResponseStatus SaveAPI(APIDetail req);
        IResponseStatus UpdateAPISTATUSCHECK(APISTATUSCHECK apistatuscheck);
        Task<APISTATUSCHECK> GetAPISTATUSCHECK(APISTATUSCHECK apistatuscheck);
        IEnumerable<APISTATUSCHECK> GetAPISTATUSCHECKs(string CheckText, int Status);
        IResponseStatus DeleteApiStatusCheck(int Statusid);
        APIDetail GetAPIDetailByAPICode(string APICode);
        APIGroupDetail GetGroup(int GroupID);
        IEnumerable<APIGroupDetail> GetGroup();
        IEnumerable<APIDetail> GetAPIDetailForBalance();
        IResponseStatus UpdateDMRModelForAPI(int OID, int API, int DMRModelID);
    }
}
