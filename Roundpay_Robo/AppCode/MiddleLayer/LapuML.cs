using Dapper;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.Models;
using Roundpay_Robo.Services;
using System.Data;
using Validators;

namespace Roundpay_Robo.AppCode
{
    public class LapuML : ILapuML
    {
        #region Global Varibale Declaration
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly IDapper _dapper;
        private readonly LapuLoginRequest appSetting;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly IRequestInfo _info;
        private readonly IDAL _dal;
        #endregion
        public LapuML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IDapper dapper)
        {
            _accessor = accessor;
            _dapper = dapper;
            _env = env;
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            _info = new RequestInfo(_accessor, _env);
        }
        public async Task<Response> SaveVendor(VendorMaster vendormaster, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("Name", vendormaster.VendorName, DbType.String);
            dbparams.Add("VendorID", vendormaster.ID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<Response>("proc_SaveVendorMaster", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<List<VendorMaster>> GetVendorList(LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("UserId", _lr.UserID, DbType.Int32);
            dbparams.Add("VendorID", 0, DbType.Int32);
            var res = await Task.FromResult(_dapper.GetAll<VendorMaster>("proc_GetLapuVendor", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<List<Lapu>> GetLapuList(LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("UserID", _lr.UserID, DbType.Int32);
            dbparams.Add("LapuID", 0, DbType.Int32);
            var res = await Task.FromResult(_dapper.GetAll<Lapu>("proc_GetLapuList", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<Response> LapuLogin(LapuLoginRequest lapulogireq, LoginResponse _lr, int LapuID)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var res = await apiml.LapuApiLogin(lapulogireq, _lr, LapuID);
            return res;
        }
        public async Task<Response> LapuBalance(LapuLoginRequest lapulogireq, LoginResponse _lr, int LapuID)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var res = await apiml.LapuApiBalance(lapulogireq, _lr, LapuID);
            return res;
        }
        public async Task<Response> ValidateOtp(ValidateLapuLoginOTP lapuotp, LoginResponse _lr, int LapuID)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var res = await apiml.LapuLoginOtpValidate(lapuotp, _lr, LapuID);
            return res;
        }

        //for Api Use
        public async Task<LapuRechargeResponse> LappuApiRecharge(LapuRechargeRequest lapurecreq)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var Cres = new Response()
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            var _req = new _LapuRechargeRequest
            {
                UserID = lapurecreq.UserID,
                Token = lapurecreq.Token,
                Account = lapurecreq.Account,
                Amount = lapurecreq.Amount,
                APIRequestID = lapurecreq.APIRequestID,
                SPKey = lapurecreq.SPKey,
                OID = 0,
                RequestMode = RequestMode.API,
                IPAddress = _info.GetRemoteIP(),
            };
            var response = new LapuRechargeResponse()
            {
                ACCOUNT = _req.Account,
                Amount = _req.Amount,
                AGENTID = _req.APIRequestID,
                OPID = _req.SPKey,
                STATUS = RechargeRespType.FAILED,
                MSG = ErrorCodes.FAILED,
            };
            #region RequestValidationFromCode  
            /**
             * Request validation from Code started
             * **/
            var validator = Validate.O;
            if (_req.UserID < 2)
            {
                response.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                response.MSG = "Unauthorised access!";
                return response;
            }

            if (!validator.IsAlphaNumeric(_req.Account ?? string.Empty) && !(_req.Account ?? string.Empty).Contains("-") && !(_req.Account ?? string.Empty).Contains("_"))
            {
                response.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                response.MSG = ErrorCodes.InvalidParam + " Account";
                return response;
            }
            if (_req.Amount < 1)
            {
                response.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                response.MSG = ErrorCodes.InvalidParam + " Amount";
                return response;
            }
            if (_req.RequestMode == RequestMode.API && (_req.Token ?? "").Length != 32)
            {
                response.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                response.MSG = ErrorCodes.InvalidParam + " Token";
                return response;
            }
            #endregion
            #region RequestValidationFromDB
            /**
             * Request validation from DB started
             * **/
            var validateReq = new ValidataRechargeApirequest
            {
                LoginID = _req.UserID,
                IPAddress = _req.IPAddress,
                Token = _req.Token,
                SPKey = _req.SPKey,
                OID = _req.OID
            };
            IProcedureAsync _procValidate = new ProcValidataRechargeApirequest(_dal);
            var validateRes = (ValidataRechargeApiResp)await _procValidate.Call(validateReq).ConfigureAwait(false);
            if (validateRes.Statuscode == ErrorCodes.Minus1)
            {
                response.MSG = validateRes.Msg;
                response.ERRORCODE = validateRes.ErrorCode;
                return response;
            }
            if (validateRes.OpParams != null)
            {
                if (validateRes.OpParams.Count > 0)
                {
                    var AccountParam = validateRes.OpParams.Where(w => w.IsAccountNo == true).FirstOrDefault();
                    if (!string.IsNullOrEmpty(AccountParam.Param))
                    {
                        AccountParam.GetFormatedError(nameof(_req.Account), _req.Account ?? string.Empty);
                        if (AccountParam.IsErrorFound)
                        {
                            response.ERRORCODE = ErrorCodes.Invalid_Parameter.ToString();
                            response.MSG = string.IsNullOrEmpty(AccountParam.Remark) ? AccountParam.FormatedError : AccountParam.Remark;
                            return response;
                        }
                        validateRes.OpParams.RemoveAll(x => x.IsAccountNo == true);
                    }
                }
            }
            #endregion
            try
            {
                var dbparams = new DynamicParameters();
                dbparams.Add("UserID", _req.UserID, DbType.String);
                dbparams.Add("Token", _req.Token, DbType.String);
                dbparams.Add("OID", validateRes.OID, DbType.String);
                dbparams.Add("SPKey", _req.SPKey, DbType.String);
                dbparams.Add("Account", _req.Account, DbType.String);
                dbparams.Add("Amount", _req.Amount, DbType.String);
                dbparams.Add("APIRequestID", _req.APIRequestID, DbType.String);
                var res = await Task.FromResult(_dapper.GetAll<LapuTransaction>("proc_LapuRechargeTransaction", dbparams, commandType: CommandType.StoredProcedure));
                if (res != null)
                    if (res.FirstOrDefault().StatusCode == ErrorCodes.One)
                    {
                        foreach (var item in res)
                        {
                            response.RPID = item.TransactionID;
                            response.LapuNumber = item.LapuNo;
                            response.LapuBAL = item.LapuBalance;
                            var req = new InitiateTransaction()
                            {
                                access_token = item.ProviderTokenID,
                                mpin = item.Pin,
                                amount = item.Amount,
                                mobile = item.Mobile,
                                biller = item.ApiOPCode,
                                category = item.Category,
                                lat = "28.5485254",
                                @long = "77.2749852"
                            };

                            var Recres = apiml.InitiateTransaction(req, item.UserID, item.LapuID).Result;
                            //ForTesting Purpose Only Start Here

                            //LapuLoginResponse Recres = new LapuLoginResponse();
                            //Recres.Resp_code = "RCS";
                            //Recres.Resp_desc = "Request Completed Successfully";
                            //Recres.data = new Data();
                            //Recres.data.voltTxnId = "5441153367";
                            //Recres.data.code ="0";

                            //ForTesting Purpose Only Start End Here
                            if (Recres != null)
                            {
                                if (Recres.Resp_code == LapuResCode.ERR)
                                {
                                    //update transaction as Failed
                                    item.Type = 3;
                                    Cres = await UpdateTransaction(item);
                                    response.ERRORCODE = ErrorCodes.Transaction_Failed_Replace.ToString();
                                    response.MSG = Recres.Resp_desc;
                                    return response;
                                }
                                if (Recres.Resp_code == LapuResCode.RCS)
                                {
                                    if (Recres.data.code == "0")
                                    {
                                        item.Type = 2;
                                        item.LiveID = Recres.data.voltTxnId;
                                        item.LapuBalance = Recres.data.balAfterTxn;
                                        Cres = await UpdateTransaction(item);
                                        //Update Transaction
                                        response.ERRORCODE = ErrorCodes.Transaction_Successful.ToString();
                                        response.MSG = Recres.Resp_desc;
                                        return response;
                                    }
                                    else
                                    {
                                        item.Type = 3;
                                        Cres = await UpdateTransaction(item);
                                        response.ERRORCODE = ErrorCodes.Transaction_Failed_Replace.ToString();
                                        response.MSG = Recres.Resp_desc;
                                        //Update Transaction as Failed
                                        return response;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        response.ERRORCODE = res.FirstOrDefault().ErrorCode;
                        response.MSG = res.FirstOrDefault().Msg;
                    }
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public async Task<Response> UpdateTransaction(LapuTransaction ltr)
        {
            var dbparams2 = new DynamicParameters();
            dbparams2.Add("UserID", ltr.UserID, DbType.Int32);
            dbparams2.Add("ID", ltr.TID, DbType.Int32);
            dbparams2.Add("Status", ltr.Type, DbType.Int32);
            dbparams2.Add("LiveID", ltr.LiveID, DbType.String);
            dbparams2.Add("LapuCurrentAmt", ltr.LapuBalance, DbType.Decimal);
            var ress = await Task.FromResult(_dapper.Update<Response>("proc_UpdateRechargeTransaction", dbparams2, commandType: CommandType.StoredProcedure));
            return ress;
        }

        public async Task<Response> SaveLapu(Lapu LapuUserDetail, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("LapuID", LapuUserDetail.LapuID, DbType.String);
            dbparams.Add("VendorID", LapuUserDetail.VendorID, DbType.String);
            dbparams.Add("ProviderID", LapuUserDetail.ProviderID, DbType.String);
            dbparams.Add("LapuNickName", LapuUserDetail.LapuNickName, DbType.String);
            dbparams.Add("LapuNo", LapuUserDetail.LapuNo, DbType.String);
            dbparams.Add("lapuUserId", LapuUserDetail.LapuUserID, DbType.String);
            dbparams.Add("Password", LapuUserDetail.Password, DbType.String);
            dbparams.Add("LapuPin", LapuUserDetail.Pin, DbType.String);
            dbparams.Add("OtherVendorName", LapuUserDetail.OtherVendorName, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<Response>("proc_AddLapuDetial", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<Response> DeleteLapu(int LapuID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("LapuID", LapuID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<Response>("proc_DeleteLapuDetial", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<Lapu> GetEditLapulist(int LapuID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LapuID", LapuID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<Lapu>("proc_GetLapuList", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<Response> UpdateLapuStatus(int LapuID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("LapuID", LapuID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<Response>("Proc_Update_lapuStatus", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<List<LapuReport>> GetLapuReport(LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            var res = await Task.FromResult(_dapper.GetAll<LapuReport>("proc_SelectLapuRechargeReport", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<Response> DeleteLapuVendor(int ID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("VendorID", ID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<Response>("proc_DeletelapuVendor", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<VendorMaster> SelectEditVendor(int ID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("VendorID", ID, DbType.String);
            //var res = await Task.FromResult(_dapper.Insert<Lapu>("proc_GetLapuList", dbparams, commandType: CommandType.StoredProcedure));
            var res = await Task.FromResult(_dapper.Get<VendorMaster>("Proc_SelectEditVendor", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
    }
}