using Roundpay_Robo.AppCode.Model;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface IErrorCodeML : IErrorCodeMLParent
    {
        IEnumerable<ErrorTypeMaster> GetTypes();
        IResponseStatus Save(ErrorCodeDetail errorCodeDetail);
        IResponseStatus update(ErrorCodeDetail req);
        List<APIErrorCode> GetAPIErrorCode();
        IResponseStatus UpdateAPIErCode(APIErrorCode aPIErrorCode);
        ErrorCodeDetail GetAPIErrorCodeDescription(string APIGroupCode, string APIErrorCode);
    }
    public interface IErrorCodeMLParent
    {
        IEnumerable<ErrorCodeDetail> Get();
        ErrorCodeDetail Get(int ID);
        ErrorCodeDetail Get(string ErrCode);
        APIErrorCode GetAPIErrorCode(APIErrorCode APIErrCode);
    }
}
