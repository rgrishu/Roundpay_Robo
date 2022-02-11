using Dapper;
using Newtonsoft.Json;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.AppCode.WebRequest;
using Roundpay_Robo.Models;
using Roundpay_Robo.Services;
using System.Data;
using System.Threading;
namespace Roundpay_Robo.AppCode
{
    public class LapuApiML : ILapuApiML
    {
        #region Global Varibale Declaration
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly IDapper _dapper;
        private readonly LapuAppSetting appSetting;
        private readonly IConfiguration Configuration;
        private readonly IConnectionConfiguration _c;
        private readonly IDAL _dal;
        #endregion



        public LapuApiML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IDapper dapper)
        {
            _accessor = accessor;
            _dapper = dapper;
            _env = env;
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile((_env.IsProduction() ? "appsettings.json" : "appsettings.Development.json"));
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
            appSetting = AppSetting();
        }
        public LapuAppSetting AppSetting()
        {
            var setting = new LapuAppSetting();
            try
            {
                setting = new LapuAppSetting
                {
                    Token = Configuration["Lapu:ATHS:Token"],
                    URL = Configuration["Lapu:ATHS:URL"]
                };
            }
            catch (Exception ex)
            {
                //var errorLog = new ErrorLog
                //{
                //    ClassName = GetType().Name,
                //    FuncName = "GetLapuToken",
                //    Error = ex.Message,
                //    LoginTypeID = 0
                //};
                //var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return setting;
        }
        public async Task<Response> LapuApiLogin(LapuLoginRequest lapuloginreq,int UserID, int LapuID,bool FromBalance=false)
        {
            var response = new Response()
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
               
            };
            try
            {
                LapuLoginResponse res = new LapuLoginResponse();
                lapuloginreq.token = appSetting.Token;
                string URL = appSetting.URL + "Action/login";
                var resp = await AppWebRequest.O.PostJsonDataUsingHWRAsync(URL, lapuloginreq).ConfigureAwait(false);
                var LapuApiReqResForDB = new LapuApiReqResForDB()
                {
                    UserID = UserID,
                    LapuID = LapuID,
                    URL = URL,
                    Request = Newtonsoft.Json.JsonConvert.SerializeObject(lapuloginreq),
                    Response = resp,
                    ClassName = "LapuApiLogin",
                    Method = "Post"
                };
                await SaveLapuReqRes(LapuApiReqResForDB);
                if (!string.IsNullOrEmpty(resp))
                {
                    var lappuerrorcheck = JsonConvert.DeserializeObject<LapuError>(resp);
                    if (lappuerrorcheck.Resp_code == LapuResCode.ERR)
                    {
                        response.StatusCode = ErrorCodes.Minus1;
                        response.Msg = lappuerrorcheck.Resp_desc;
                        return response;
                    }
                    else
                    {
                        res = JsonConvert.DeserializeObject<LapuLoginResponse>(resp);
                        if (res.Resp_code == LapuResCode.VRF)
                        {
                          //  Two Is User For OTP 
                            response.StatusCode = ErrorCodes.Two;
                            response.Msg = "Otp Required";
                            return response;
                            //OTP Required 
                        }
                        else if (res.Resp_code == LapuResCode.RCS)
                        {
                            if (res.data.code == "0")
                            {
                                var dbparams = new DynamicParameters();
                                dbparams.Add("UserID", UserID, DbType.Int32);
                                dbparams.Add("LapuID", LapuID, DbType.Int32);
                                dbparams.Add("Balance", res.data.currentBal, DbType.Decimal);
                                dbparams.Add("ProviderTokenID", res.data.token, DbType.String);
                                response = await Task.FromResult(_dapper.Update<Response>("proc_UpdateLapuListBalance", dbparams, commandType: CommandType.StoredProcedure));
                                if (!FromBalance) {
                                    response.Msg = response.StatusCode == 1 ? "Login Successfull" : response.Msg;
                                }
                                response.CommonStr = res.data.token;
                            }
                        }
                        else if (res.Resp_code == LapuResCode.ERR)
                        {
                            response.StatusCode = ErrorCodes.Minus1;
                            response.Msg = res.Resp_desc;
                            return response;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }
        public async Task<Response> LapuApiBalance(LapuLoginRequest lapuloginreq, LoginResponse _lr, int LapuID)
        {
            var response = new Response()
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };
            if (string.IsNullOrEmpty(lapuloginreq.access_token))
            {
                response = await LapuApiLogin(lapuloginreq, _lr.UserID, LapuID,true);
                return response;
            }

            string ULRProfile = appSetting.URL + "Fetch/get_profile_data";
            var profres = await AppWebRequest.O.PostJsonDataUsingHWRAsync(ULRProfile, lapuloginreq).ConfigureAwait(false);
            var LapuApiReqResForDB2 = new LapuApiReqResForDB()
            {
                UserID = _lr.UserID,
                LapuID = LapuID,
                URL = ULRProfile,
                Request = Newtonsoft.Json.JsonConvert.SerializeObject(lapuloginreq),
                Response = profres,
                ClassName = "LapuApiLogin",
                Method = "Post"
            };
            await SaveLapuReqRes(LapuApiReqResForDB2);
            if (!string.IsNullOrEmpty(profres))
            {
                var lappuerror = JsonConvert.DeserializeObject<LapuError>(profres);
                if (lappuerror.Resp_code == LapuResCode.ERR)
                {
                    response = await LapuApiLogin(lapuloginreq, _lr.UserID, LapuID, true);
                    return response;
                }
                else
                {
                    var profileres = JsonConvert.DeserializeObject<LapuLoginResponse>(profres);
                    if (profileres.Resp_code == LapuResCode.RCS)
                    {
                        if (profileres.data.code == "0")
                        {
                            var dbparams = new DynamicParameters();
                            dbparams.Add("UserID", _lr.UserID, DbType.Int32);
                            dbparams.Add("LapuID", LapuID, DbType.Int32);
                            dbparams.Add("Balance", profileres.data.currentBal, DbType.Decimal);
                            dbparams.Add("ProviderTokenID", lapuloginreq.access_token, DbType.String);
                            response = await Task.FromResult(_dapper.Update<Response>("proc_UpdateLapuListBalance", dbparams, commandType: CommandType.StoredProcedure));
                        }
                        else
                        {
                            //For Session Expire Code Is 1
                            if (profileres.data.code == "1")
                            {
                                response = await LapuApiLogin(lapuloginreq, _lr.UserID, LapuID, true);
                                return response;
                            }
                        }
                    }
                    else
                    {
                        response = await LapuApiLogin(lapuloginreq, _lr.UserID, LapuID, true);
                        return response;
                    }
                }
            }
            return response;
        }

        public async Task<LapuApiTransactionRecord> LapuTransactioDataFromApi(LapuApiTransacrionReq lapuapitransacrionreq,int UserID,int LapuID)
        {
            LapuApiTransactionRecord response = new LapuApiTransactionRecord();
            try
            {
                lapuapitransacrionreq.token = appSetting.Token;
                string ULRProfile = appSetting.URL + "Fetch/get_txn_data";
                var tranres = await AppWebRequest.O.PostJsonDataUsingHWRAsync(ULRProfile, lapuapitransacrionreq).ConfigureAwait(false);
                var LapuApiReqResForDB2 = new LapuApiReqResForDB()
                {
                    UserID = UserID,
                    LapuID = LapuID,
                    URL = ULRProfile,
                    Request = Newtonsoft.Json.JsonConvert.SerializeObject(lapuapitransacrionreq),
                    Response = tranres,
                    ClassName = "LapuApiML",
                    Method = "Post"
                };
                await SaveLapuReqRes(LapuApiReqResForDB2);
                if (!string.IsNullOrEmpty(tranres))
                {
                    response = JsonConvert.DeserializeObject<LapuApiTransactionRecord>(tranres);
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = "LapuAPIML.cs",
                    FuncName = "InitiateTransaction",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return response;
        }

        public async Task<Response> LapuLoginOtpValidate(ValidateLapuLoginOTP lapuloginotpval, LoginResponse _lr, int LapuID)
        {
            var response = new Response()
            {
                StatusCode = ErrorCodes.Minus1,
                Msg = ErrorCodes.FAILED
            };

            string ULRotp = appSetting.URL + "Action/validate_otp";
            var otpres = await AppWebRequest.O.PostJsonDataUsingHWRAsync(ULRotp, lapuloginotpval).ConfigureAwait(false);
            var LapuApiReqResForDB2 = new LapuApiReqResForDB()
            {
                UserID = _lr.UserID,
                LapuID = LapuID,
                URL = ULRotp,
                Request = Newtonsoft.Json.JsonConvert.SerializeObject(lapuloginotpval),
                Response = otpres,
                ClassName = "LapuLoginOtpValidate",
                Method = "Post"
            };
            await SaveLapuReqRes(LapuApiReqResForDB2);
            if (!string.IsNullOrEmpty(otpres))
            {
                var lappuerror = JsonConvert.DeserializeObject<LapuError>(otpres);
                if (lappuerror.Resp_code == LapuResCode.ERR)
                {
                 ///   response = await LapuApiLogin(lapuloginreq, _lr, LapuID, true);
                    return response;
                }
                else
                {
                    var otpobjres = JsonConvert.DeserializeObject<LapuLoginResponse>(otpres);
                    if (otpobjres.Resp_code == LapuResCode.RCS)
                    {
                        if (otpobjres.data.code == "0")
                        {
                            var dbparams = new DynamicParameters();
                            dbparams.Add("UserID", _lr.UserID, DbType.Int32);
                            dbparams.Add("LapuID", LapuID, DbType.Int32);
                            dbparams.Add("Balance", otpobjres.data.currentBal, DbType.Decimal);
                            dbparams.Add("ProviderTokenID", "", DbType.String);
                            response = await Task.FromResult(_dapper.Update<Response>("proc_UpdateLapuListBalance", dbparams, commandType: CommandType.StoredProcedure));
                        }
                        else
                        {
                            //For Session Expire Code Is 1
                            if (otpobjres.data.code == "1")
                            {
                                //response = await LapuApiLogin(lapuloginreq, _lr, LapuID, true);
                                return response;
                            }
                        }
                    }
                    else
                    {
                       // response = await LapuApiLogin(lapuloginreq, _lr, LapuID, true);
                        return response;
                    }
                }
            }
            return response;
        }



        public async Task<string> InitiateTransaction(InitiateTransaction initiatetransaction, int UserID, int LapuID,int TID,int SleepTime)
        {
            var recres = string.Empty;
            try
            {
                initiatetransaction.token = appSetting.Token;
                // string ULRRec = appSetting.URL + "Action/test";
                string ULRRec = appSetting.URL + "Action/init_txn";
                recres = await AppWebRequest.O.PostJsonDataUsingHWRAsync(ULRRec, initiatetransaction).ConfigureAwait(false);
                var dbparams = new DynamicParameters();
                dbparams.Add("UserID", UserID, DbType.Int32);
                dbparams.Add("TID", TID, DbType.Int32);
                dbparams.Add("LapuID", LapuID, DbType.Int32);
                dbparams.Add("URL", ULRRec, DbType.String);
                dbparams.Add("Request", Newtonsoft.Json.JsonConvert.SerializeObject(initiatetransaction), DbType.String);
                dbparams.Add("Response", recres, DbType.String);
                dbparams.Add("ClassName", "InitiateTransaction", DbType.String);
                dbparams.Add("Method", "Post", DbType.String);
               _dapper.Insert<Response>("proc_SaveLapuReqRes", dbparams, commandType: CommandType.StoredProcedure);
            }
            catch(Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = "LapuAPIML.cs",
                    FuncName = "InitiateTransaction",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = UserID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return recres;
        }


        public async Task SaveLapuReqRes(LapuApiReqResForDB larr)
        {
            try
            {
                var dbparams = new DynamicParameters();
                dbparams.Add("UserID", larr.UserID, DbType.Int32);
                dbparams.Add("TID", larr.TID, DbType.Int32);
                dbparams.Add("LapuID", larr.LapuID, DbType.Int32);
                dbparams.Add("URL", larr.URL, DbType.String);
                dbparams.Add("Request", larr.Request, DbType.String);
                dbparams.Add("Response", larr.Response, DbType.String);
                dbparams.Add("ClassName", larr.ClassName, DbType.String);
                dbparams.Add("Method", larr.Method, DbType.String);
                await Task.FromResult(_dapper.Insert<Response>("proc_SaveLapuReqRes", dbparams, commandType: CommandType.StoredProcedure));
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = "LapuAPIML.cs",
                    FuncName = "SaveLapuReqRes",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = larr!=null?larr.UserID:0,
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
        }

    }
}