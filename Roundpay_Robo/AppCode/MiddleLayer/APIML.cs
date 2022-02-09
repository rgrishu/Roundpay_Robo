using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Validators;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace Roundpay_Robo.AppCode.MiddleLayer
{
    public class APIML : IAPIML, ISMSAPIML, IDisposable
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly ISession _session;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _rinfo;
        private readonly LoginResponse _lr;
        private readonly IUserML userML;
        private readonly LoginResponse _lrEmp;
        public APIML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _session = _accessor != null ? _accessor.HttpContext.Session : null;
            _dal = new DAL(_c.GetConnectionString());
            _rinfo = new RequestInfo(_accessor, _env);
            _lr = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponse);
            userML = new UserML(_lr);
            _lrEmp = _session.GetObjectFromJson<LoginResponse>(SessionKeys.LoginResponseEmp);
        }
        public IEnumerable<SMSAPIDetail> GetSMSAPIDetail()
        {
            var resp = new List<SMSAPIDetail>();
            if ((_lr.RoleID == Role.Admin || _lr.IsWebsite) && _lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                var req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    LoginID = _lr.UserID,
                    CommonInt = 0
                };
                IProcedure _proc = new ProcGetSMSAPI(_dal);
                resp = (List<SMSAPIDetail>)_proc.Call(req);
            }
            return resp;
        }
        public SMSAPIDetail GetSMSAPIDetailByID(int APIID)
        {
            var resp = new SMSAPIDetail();
            if (_lr.LoginTypeID == LoginType.ApplicationUser || userML.IsCustomerCareAuthorised(ActionCodes.BulkSMS))
            {
                if (APIID > 0)
                {
                    var req = new CommonReq
                    {
                        LoginTypeID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.LoginTypeID,
                        LoginID = _lr.LoginTypeID == LoginType.CustomerCare ? 1 : _lr.UserID,
                        CommonInt = APIID
                    };
                    IProcedure _proc = new ProcGetSMSAPI(_dal);
                    resp = (SMSAPIDetail)_proc.Call(req);
                }
            }
            return resp;
        }
        public IResponseStatus SaveSMSAPI(APIDetail req)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSMSAPI)))
            {
                if (string.IsNullOrEmpty(req.Name) || Validate.O.IsNumeric(req.Name ?? "") || (req.Name ?? "").Length > 50)
                {
                    resp.Msg = ErrorCodes.InvalidParam + " Name";
                    return resp;
                }
                var _req = new APIDetailReq
                {
                    LoginID = _lr.UserID,
                    LT = _lr.LoginTypeID,
                    ID = req.ID,
                    APIType = req.APIType,
                    Name = req.Name,
                    URL = req.URL,
                    RequestMethod = req.RequestMethod,
                    ResponseTypeID = req.ResponseTypeID,
                    IP = _rinfo.GetRemoteIP(),
                    Browser = _rinfo.GetBrowserFullInfo(),
                    TransactionType = req.TransactionType,
                    IsActive = req.IsActive,
                    Default = req.Default,
                    IsWhatsApp = req.IsWhatsApp,
                    IsHangout = req.IsHangout,
                    IsTelegram = req.IsTelegram
                };

                IProcedure proc = new ProcSMSAPICU(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }
        public IResponseStatus ISSMSAPIActive(int ID, bool IsActive, bool IsDefault)
        {
            var resp = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.AuthError
            };
            if ((_lr.LoginTypeID == LoginType.ApplicationUser && !userML.IsEndUser()) || (userML.IsCustomerCareAuthorised(ActionCodes.AddEditSMSAPI)))
            {
                if (ID == 0)
                {
                    resp.Msg = ErrorCodes.InvalidParam + " ID";
                    return resp;
                }
                var _req = new CommonReq
                {
                    LoginTypeID = _lr.LoginTypeID,
                    CommonInt = ID,
                    CommonBool = IsActive,
                    CommonBool1 = IsDefault
                };
                IProcedure proc = new ProcChangeAPIActiveStatus(_dal);
                resp = (ResponseStatus)proc.Call(_req);
            }
            return resp;
        }

        public APIDetail GetAPIDetailByID(int apiId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<APIDetail> GetAPIDetail(int APIId = -1)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus SaveAPI(APIDetail req)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus UpdateAPISTATUSCHECK(APISTATUSCHECK apistatuscheck)
        {
            throw new NotImplementedException();
        }

        public Task<APISTATUSCHECK> GetAPISTATUSCHECK(APISTATUSCHECK apistatuscheck)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<APISTATUSCHECK> GetAPISTATUSCHECKs(string CheckText, int Status)
        {
            throw new NotImplementedException();
        }

        public IResponseStatus DeleteApiStatusCheck(int Statusid)
        {
            throw new NotImplementedException();
        }

        public APIDetail GetAPIDetailByAPICode(string APICode)
        {
            throw new NotImplementedException();
        }

        public APIGroupDetail GetGroup(int GroupID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<APIGroupDetail> GetGroup()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<APIDetail> GetAPIDetailForBalance()
        {
            throw new NotImplementedException();
        }

        public IResponseStatus UpdateDMRModelForAPI(int OID, int API, int DMRModelID)
        {
            throw new NotImplementedException();
        }

        IEnumerable<APISTATUSCHECK> IAPIML.GetAPISTATUSCHECKs(string CheckText, int Status)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
