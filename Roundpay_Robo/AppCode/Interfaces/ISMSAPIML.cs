using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface ISMSAPIML
    {
        IEnumerable<SMSAPIDetail> GetSMSAPIDetail();
        SMSAPIDetail GetSMSAPIDetailByID(int APIID);
        IResponseStatus SaveSMSAPI(APIDetail req);
        IResponseStatus ISSMSAPIActive(int ID, bool IsActive, bool IsDefault);

    }
}
