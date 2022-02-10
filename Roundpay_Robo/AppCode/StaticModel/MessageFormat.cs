using Microsoft.AspNetCore.Mvc;

namespace Roundpay_Robo.AppCode.StaticModel
{
    public class MessageFormat
    {
        public const int Registration = 1;
        public const int FundTransfer = 2;
        public const int FundReceive = 3;
        public const int OTP = 4;
        public const int FundDebit = 5;
        public const int FundCredit = 6;
        public const int RechargeAccept = 7;
        public const int RechargeSuccess = 8;
        public const int RechargeFailed = 9;
        public const int OperatorUPMessage = 10;
        public const int OperatorDownMessage = 11;
        public const int RechargeRefund = 12;
        public const int InvoiceFundCredit = 13;
        public const int LowBalanceFormat = 14;
        public const int ForgetPass = 15;
        public const int senderRegistrationOTP = 16;
        public const int BenificieryRegistrationOTP = 17;
        public const int KYCApproved = 18;
        public const int KYCReject = 19;
        public const int FundOrderAlert = 20;
        public const int UserPartialApproval = 21;
        public const int UserSubscription = 22;
        public const int ThankYou = 23;
        public const int CallNotPicked = 24;
        public const int BirthdayWish = 25;
        public const int MarginRevised = 26;
        public const int RechargeRefundReject = 27;
        public const int Payout = 28;
        public const int BBPSSuccess = 29;
        public const int BBPSComplainRegistration = 30;
        public const int PendingRechargeNotification = 31;
        public const int PendingRefundNotification = 32;
    }
}
