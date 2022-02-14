using Dapper;
using Newtonsoft.Json;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.HelperClass;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.Models;
using Roundpay_Robo.Services;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System.Data;
using System.Globalization;
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
        public async Task<CommonResponse> SaveVendor(VendorMaster vendormaster, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("Name", vendormaster.VendorName, DbType.String);
            dbparams.Add("VendorID", vendormaster.ID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<CommonResponse>("proc_SaveVendorMaster", dbparams, commandType: CommandType.StoredProcedure));
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
        public async Task<CommonResponse> LapuLogin(LapuLoginRequest lapulogireq, LoginResponse _lr, int LapuID)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var res = await apiml.LapuApiLogin(lapulogireq, _lr.UserID, LapuID);
            return res;
        }
        public async Task<CommonResponse> LapuBalance(LapuLoginRequest lapulogireq, LoginResponse _lr, int LapuID)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var res = await apiml.LapuApiBalance(lapulogireq, _lr, LapuID);
            return res;
        }
        public async Task<CommonResponse> ValidateOtp(ValidateLapuLoginOTP lapuotp, LoginResponse _lr, int LapuID)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var res = await apiml.LapuLoginOtpValidate(lapuotp, _lr, LapuID);
            return res;
        }
        public async Task<LapuApiTransactionRecord> LapuTransactioDataFromAPi(LapuApiTransacrionReq lapuapitransacrionreq, int UserID, int LapuID)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var startdate = lapuapitransacrionreq.startDate.Trim().Substring(0, 11);
            DateTime dt;
            var dates = DateTime.TryParseExact(startdate, "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
            lapuapitransacrionreq.minRecord = "0";
            lapuapitransacrionreq.maxRecord = "50";
            lapuapitransacrionreq.startDate = dt.ToString("dd-MMM-yy"); ;
            lapuapitransacrionreq.endDate = dt.AddDays(1).ToString("dd-MMM-yy");
            var res = await apiml.LapuTransactioDataFromApi(lapuapitransacrionreq, UserID, LapuID);
            return res;
        }


        //Get Lapu Api Transaction Data
        public async Task<Response> LapuTransactions()
        {
            var res = new Response()
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            try
            {
                ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
                var dbparams = new DynamicParameters();
                var pendtran = await Task.FromResult(_dapper.GetAll<LapuReport>("proc_SelectLapuPendingTransactions", dbparams, commandType: CommandType.StoredProcedure));
                if (pendtran != null && pendtran.Count > 0)
                {
                    foreach (var item in pendtran)
                    {
                        DateTime dt;
                        var startdate = item.EntryDate.Trim().Substring(0, 11);
                        var dates = DateTime.TryParseExact(startdate, "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                        var lapuapitransacrionreq = new LapuApiTransacrionReq()
                        {
                            access_token = item.ProviderTokenID,
                            minRecord = "0",
                            maxRecord = "50",
                            customerId = item.AccountNo,
                            endDate = dt.AddDays(1).ToString("dd-MMM-yy"),
                            startDate = dt.ToString("dd-MMM-yy")
                        };
                        var laputranres = await apiml.LapuTransactioDataFromApi(lapuapitransacrionreq, 0, 0);
                        if (laputranres != null && laputranres.data != null)
                        {
                            if (laputranres.data.errorCode == LapuFailCode.SessionExpired)
                            {
                                res.Msg = laputranres.data.messageText;
                                return res;
                            }
                            if (laputranres.data.txnRecords != null && laputranres.data.txnRecords.txnRecord.Count() == 1)
                            {
                                foreach (var lapurec in laputranres.data.txnRecords.txnRecord)
                                {
                                    string accno = "91" + item.AccountNo, RechargeAmount = item.RechargeAmount.ToString();

                                    if (lapurec.customerId.Trim() == accno && lapurec.txnAmount.Trim() == RechargeAmount)
                                    {
                                        var ltr = new LapuTransaction()
                                        {
                                            UserID = 1,
                                            TID = item.TID,
                                            Type = lapurec.batchState == "SUCCESS" ? RechargeRespType.SUCCESS : RechargeRespType.FAILED,
                                            LiveID = lapurec.voltTxnId,
                                            LapuBalance = 0,
                                            LapuID = item.LapuID,
                                            ErrorCode = "",
                                            Message = ""
                                        };
                                        var updateres = UpdateTransaction(ltr, true,true).Result;
                                    }

                                }
                            }
                        }
                    }
                }
                res.StatusCode = ErrorCodes.One;
                res.Msg = ErrorCodes.SUCCESS;
            }
            catch(Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = "LapuML.cs",
                    FuncName = "LapuTransactions",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 0
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
        
            return res;
        }

        //for Api Use
        public async Task<LapuRechargeResponse> LappuApiRecharge(LapuRechargeRequest lapurecreq)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var Cres = new CommonResponse()
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
                STATUS = RechargeRespType.PENDING,
                MSG = ErrorCodes.Request_Accpeted.ToString(),
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
                DateTimeOffset now = DateTimeOffset.UtcNow;
                long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                var dbparams = new DynamicParameters();
                dbparams.Add("UserID", _req.UserID, DbType.String);
                dbparams.Add("Token", _req.Token, DbType.String);
                dbparams.Add("OID", validateRes.OID, DbType.String);
                dbparams.Add("SPKey", _req.SPKey, DbType.String);
                dbparams.Add("Account", _req.Account, DbType.String);
                dbparams.Add("Amount", _req.Amount, DbType.String);
                dbparams.Add("APIRequestID", _req.APIRequestID, DbType.String);
                dbparams.Add("UnixDatTime", unixTimeMilliseconds, DbType.String);
                // var res = await Task.FromResult(_dapper.GetMultiple<LapuTransaction>("proc_LapuRechargeTransaction", dbparams, commandType: CommandType.StoredProcedure));
                var result = _dapper.GetMultiple<LapuTransaction, APIDetail>("[proc_LapuRechargeTransaction]", dbparams, commandType: CommandType.StoredProcedure).Result;
                var res = (List<LapuTransaction>)result.GetType().GetProperty("Table1").GetValue(result, null);
                var apiresdetails = (List<APIDetail>)result.GetType().GetProperty("Table2").GetValue(result, null);

                if (res != null)
                    if (res.FirstOrDefault().StatusCode == ErrorCodes.One)
                    {
                        // ApiRequestID Exists Then Return The Detail Of Exists APIRequestID  Form Transaction Detail
                        if (res.FirstOrDefault().IsExistAPIRequestID == true)
                        {
                            response.ACCOUNT = res.FirstOrDefault().Account;
                            response.Amount = res.FirstOrDefault().BalanceAmount;
                            response.RPID = res.FirstOrDefault().TransactionID;
                            response.AGENTID = _req.APIRequestID;
                            response.OPID = res.FirstOrDefault().LiveID;
                            response.STATUS = res.FirstOrDefault().Type;
                            response.MSG = res.FirstOrDefault().Msg;
                            response.LapuBAL = res.FirstOrDefault().BalanceAmount;
                            response.ERRORCODE = res.FirstOrDefault().ErrorCode;
                            response.LapuNumber = res.FirstOrDefault().LapuRechargeNumber;
                            return response;
                        }
                        if (apiresdetails == null)
                        {
                            response.ERRORCODE = ErrorCodes.Transaction_Failed_Replace.ToString();
                            response.MSG = ErrorCodes.FAILED;
                            return response;
                        }
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

                            RechargeAPIHit rechargeAPIHit = new RechargeAPIHit();
                            rechargeAPIHit.aPIDetail = apiresdetails.FirstOrDefault();
                            rechargeAPIHit.ServiceID = Convert.ToInt32(validateReq.SPKey);
                            rechargeAPIHit.LoginID = validateReq.LoginID;
                            rechargeAPIHit.IsException = false;

                            var stratdate = DateTime.Now;

                            if (item.LapuTranSleepTime > 0)
                            {
                                System.Threading.Thread.Sleep(item.LapuTranSleepTime);
                            }
                            var EndDate = DateTime.Now;

                            CheckTimeSesion(item.TID, item.LapuTranSleepTime, stratdate, EndDate);

                            var tstatus = doTransaction(req, item.UserID, item.LapuID, item.TID, item.LapuTranSleepTime);

                            TransactionStatus doTransaction(InitiateTransaction req, int UserID, int LapuID, int TID, int SleepTime)
                            {
                                TransactionStatus tstatus = new TransactionStatus();

                                //intiate  api transaction For Recharge

                                string res = apiml.InitiateTransaction(req, UserID, LapuID, TID, SleepTime).Result;
                                var transactionHelper = new TransactionHelper(_dal, _accessor, _env);

                                rechargeAPIHit.Response = res;

                                //Match Api Response Here
                                tstatus = transactionHelper.MatchResponse(validateReq.OID, rechargeAPIHit, _req.Account).Result;

                                // IsResend Rehit Another Lapu When IsResend=true and IsReLoginLapu=false
                                if (tstatus.IsResend)
                                {

                                    // IsReLoginLapu Is Used For Relogin Lapu For Token 
                                    if (tstatus.IsReLoginLapu)
                                    {
                                        ILapuApiML _apiml = new LapuApiML(_accessor, _env, _dapper);

                                        var lapuloginreq = new LapuLoginRequest()
                                        {
                                            mobile = item.LapuNo,
                                            password = item.Password
                                        };
                                        CommonResponse loginRes = _apiml.LapuApiLogin(lapuloginreq, item.UserID, item.LapuID).Result;
                                        if (loginRes != null)
                                        {
                                            if (loginRes.StatusCode == ErrorCodes.One)
                                            {
                                                req.access_token = loginRes.CommonStr;
                                                doTransaction(req, UserID, LapuID, TID, SleepTime);
                                            }
                                        }
                                    }
                                }
                                return tstatus;
                            }

                            //ForTesting Purpose Only Start Here
                            //LapuLoginResponse Recres = new LapuLoginResponse();
                            //Recres.Resp_code = "RCS";
                            //Recres.Resp_desc = "Request Completed Successfully";
                            //Recres.data = new Data();
                            //Recres.data.voltTxnId = "5441153367";
                            //Recres.data.code ="0";

                            //ForTesting Purpose Only Start End Here
                            if (tstatus != null)
                            {
                                response.STATUS = tstatus.Status;
                                response.OPID = tstatus.OperatorID;
                                response.MSG = tstatus.ErrorMsg;
                                response.ERRORCODE = tstatus.ErrorCode;
                                if (tstatus.Status.In(RechargeRespType.SUCCESS, RechargeRespType.FAILED, RechargeRespType.PENDING))
                                {
                                    //update transaction as Failed as success
                                    item.Type = tstatus.Status;
                                    item.LiveID = tstatus.OperatorID;
                                    item.LapuBalance = string.IsNullOrEmpty(tstatus.Balance) ? 0 : Convert.ToDecimal(tstatus.Balance);
                                    item.ErrorCode = tstatus.ErrorCode;
                                    item.Message = tstatus.ErrorMsg;
                                    var updateres = await UpdateTransaction(item);
                                    response.ERRORCODE = tstatus.ErrorCode;
                                    response.MSG = tstatus.ErrorMsg;
                                    return response;
                                }
                            }
                        }
                    }
                    else
                    {
                        response.STATUS = RechargeRespType.FAILED;
                        response.ERRORCODE = res.FirstOrDefault().ErrorCode;
                        response.MSG = res.FirstOrDefault().Msg;
                    }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = "LapuML.cs",
                    FuncName = "LappuApiRecharge",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = lapurecreq.UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }



            return response;
        }


        public class TestTimeDiff
        {
            public int TID { get; set; }
            public int ProcessTime { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public static bool CheckTimeSesion(int TID, int ProcessTime, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                var jsonFile = DOCType.TimeDiffPAth;
                if (File.Exists(jsonFile))
                {
                    var json = System.IO.File.ReadAllText(jsonFile);
                    var jObjectList = JsonConvert.DeserializeObject<List<TestTimeDiff>>(json);
                    jObjectList = jObjectList == null ? new List<TestTimeDiff>() : jObjectList;
                    jObjectList.Add(new TestTimeDiff
                    {
                        TID = TID,
                        ProcessTime = ProcessTime,
                        StartDate = StartDate,
                        EndDate = EndDate
                    });

                    string output = JsonConvert.SerializeObject(jObjectList, Formatting.Indented);
                    File.WriteAllText(jsonFile, output);
                }
                return true;
            }

            catch (Exception ex)
            {
                return false;
            }
        }




        public async Task<LapuProcUpdateTranResposne> UpdateTransaction(LapuTransaction ltr, bool IsCallback = false,bool IsFromApi=false)
        {
            ILapuApiML apiml = new LapuApiML(_accessor, _env, _dapper);
            var dbparams2 = new DynamicParameters();
            dbparams2.Add("UserID", ltr.UserID, DbType.Int32);
            dbparams2.Add("ID", ltr.TID, DbType.Int32);
            dbparams2.Add("Status", ltr.Type, DbType.Int32);
            dbparams2.Add("LiveID", ltr.LiveID, DbType.String);
            dbparams2.Add("LapuCurrentAmt", ltr.LapuBalance, DbType.Decimal);
            dbparams2.Add("LapuID", ltr.LapuID, DbType.Int32);
            dbparams2.Add("ErroCode", ltr.ErrorCode, DbType.String);
            dbparams2.Add("ErrorMessage", ltr.Message, DbType.String);
            dbparams2.Add("IsCallback", IsCallback, DbType.Boolean);
            dbparams2.Add("IsFromApi", IsFromApi, DbType.Boolean);
            var ress = await Task.FromResult(_dapper.Update<LapuProcUpdateTranResposne>("proc_UpdateRechargeTransaction", dbparams2, commandType: CommandType.StoredProcedure));
            if (ress.IsCallbackFound == true)
            {
                if (!string.IsNullOrEmpty(ress.CallbackURL))
                {
                    var apiurlhitting = new APIURLHitting()
                    {
                        UserID = ress.UserID,
                        TransactionID = ress.TransactionID,
                        URL = ress.CallbackURL
                    };
                    apiml.CallBackURLAfterManualRechUpdate(apiurlhitting);
                }
            }
            return ress;
        }

        public async Task<CommonResponse> SaveLapu(Lapu LapuUserDetail, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            CommonResponse res = new CommonResponse();
            try
            {
                dbparams.Add("LoginID", _lr.UserID, DbType.String);
                dbparams.Add("LapuID", LapuUserDetail.LapuID, DbType.String);
                dbparams.Add("VendorID", LapuUserDetail.VendorID, DbType.String);
                dbparams.Add("LapuTypeID", LapuUserDetail.LapuTypeID, DbType.String);
                dbparams.Add("ProviderID", LapuUserDetail.ProviderID, DbType.String);
                dbparams.Add("LapuNickName", LapuUserDetail.LapuNickName, DbType.String);
                dbparams.Add("LapuNo", LapuUserDetail.LapuNo, DbType.String);
                dbparams.Add("lapuUserId", LapuUserDetail.LapuUserID, DbType.String);
                dbparams.Add("Password", LapuUserDetail.Password, DbType.String);
                dbparams.Add("LapuPin", LapuUserDetail.Pin, DbType.String);
                dbparams.Add("OtherVendorName", LapuUserDetail.OtherVendorName, DbType.String);
                res = await Task.FromResult(_dapper.Insert<CommonResponse>("proc_AddLapuDetial", dbparams, commandType: CommandType.StoredProcedure));

            }
            catch (Exception ex)
            {
                var _ = new ProcPageErrorLog(_dal).Call(new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "SaveLapu",
                    Error = ex.Message,
                    LoginTypeID = _lr.LoginTypeID,
                    UserId = _lr.UserID
                });
            }
            return res;
        }
        public async Task<CommonResponse> DeleteLapu(int LapuID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("LapuID", LapuID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<CommonResponse>("proc_DeleteLapuDetial", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<Lapu> GetEditLapulist(int LapuID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LapuID", LapuID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<Lapu>("proc_GetLapuList", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<CommonResponse> UpdateLapuStatus(int LapuID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("LapuID", LapuID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<CommonResponse>("Proc_Update_lapuStatus", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<List<LapuReport>> GetLapuReport(LapuReport Filter, LoginResponse _lr)
        {
            List<LapuReport> res = new List<LapuReport>();
            try
            {
                var dbparams = new DynamicParameters();
                dbparams.Add("Type", Filter.Type, DbType.Int32);
                dbparams.Add("Top", Filter.Top, DbType.Int32);
                dbparams.Add("AccountNo", string.IsNullOrEmpty(Filter.AccountNo) ? "" : Filter.AccountNo, DbType.String);
                dbparams.Add("TransactionID", Filter.TransactionID ?? "", DbType.String);
                dbparams.Add("LapuNo", string.IsNullOrEmpty(Filter.LapuNo) ? "" : Filter.LapuNo, DbType.String);
                dbparams.Add("LiveID", Filter.LiveID ?? "", DbType.String);
                res = await Task.FromResult(_dapper.GetAll<LapuReport>("proc_SelectLapuRechargeReport", dbparams, commandType: CommandType.StoredProcedure));

            }
            catch { }
            return res;
        }
        public async Task<CommonResponse> DeleteLapuVendor(int ID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            dbparams.Add("VendorID", ID, DbType.String);
            var res = await Task.FromResult(_dapper.Insert<CommonResponse>("proc_DeletelapuVendor", dbparams, commandType: CommandType.StoredProcedure));
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
        public async Task<List<Lapu>> GetVendorLapu(LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("LoginID", _lr.UserID, DbType.String);
            var res = await Task.FromResult(_dapper.GetAll<Lapu>("Proc_getVendor", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<List<LapuServices>> GetServices(LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            var res = await Task.FromResult(_dapper.GetAll<LapuServices>("proc_selectLapuService", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
        public async Task<LapuReqRes> GetReqRes(int TID, int LapuID, LoginResponse _lr)
        {
            var dbparams = new DynamicParameters();
            dbparams.Add("TID", TID, DbType.Int32);
            dbparams.Add("LapuID", LapuID, DbType.Int32);
            var res = await Task.FromResult(_dapper.Get<LapuReqRes>("proc_GetLapuReqRes", dbparams, commandType: CommandType.StoredProcedure));
            return res;
        }
    }
}