using Roundpay_Robo.AppCode.Model.Recharge;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface IAPIUserMiddleLayer
    {

        Task SaveAPILog(APIReqResp aPIReqResp);
    }
}
