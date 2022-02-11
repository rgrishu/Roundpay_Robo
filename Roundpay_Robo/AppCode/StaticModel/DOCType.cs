﻿using Microsoft.AspNetCore.Mvc;

namespace Roundpay_Robo.AppCode.StaticModel
{
    public static class DOCType
    {
        public const int PAN = 1;
        public const int AADHAR = 2;
        public const int PHOTO = 3;
        public const int ServiceAggreement = 4;
        public const int GSTRegistration = 5;
        public const int CancelledCheque = 6;
        public const int BusinessAddressProof = 7;
        public const int PASSBOOK = 8;
        public const int VoterID = 9;
        public const int DrivingLicense = 10;
        public const int ShopImage = 12;


        public const int Notification = 11;


        public const int APIDocument = -1000;
        public static IDictionary<int, string> DicDoc = new Dictionary<int, string> {
            { PAN,"PAN Card"},
            { AADHAR,"Aadhaar Card"},
            { PHOTO,"Photo (Passport Size)"},
            { ServiceAggreement,"Service Agreement"},
            { GSTRegistration,"GST Registration"},
            { CancelledCheque,"Cancelled Cheque"},
            { BusinessAddressProof,"Business Address Proof"},
            { APIDocument,"API Technical Document"},
            { PASSBOOK,"PASSBOOK"},
            {VoterID,"VoterID" },
            { DrivingLicense,"DrivingLicense"}
        };
        public static string DocFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/KYC/");
        public static string InvoiceFileSuffix = "Image/Invoice/";
        public static string InvoiceFilePath = Path.Combine(Directory.GetCurrentDirectory(), InvoiceFileSuffix);
        public const string ImgNotificationSuffix = "Image/Notification/";
        public static string ImgNotification = Path.Combine(Directory.GetCurrentDirectory(), ImgNotificationSuffix);
        public static string WebNotificationPath = Path.Combine(Directory.GetCurrentDirectory(), "Image/Webnotification/");
        public static string OperatorIconPath = Path.Combine(Directory.GetCurrentDirectory(), "Image/operator/");
        public static string EmployeeGiftImgPath = Path.Combine(Directory.GetCurrentDirectory(), "Image/Employee/GiftImage/");
        public static string GIBLCertificatePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/GIBL/GIBL_Public_Key.pem");

        public const string TimeDiffPAthName = "timediff.json";
        public static string TimeDiffPAth = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\TimeDiff", TimeDiffPAthName);


        public const string LogoSuffix = "Image/Website/{0}/logo.png";
        public const string PopupSuffix = "Image/Website/{0}/Popup";
        public const string BannerSiteSuffix = "Image/Website/{0}/BannerSite";
        public static string BannerSitePath = Path.Combine(Directory.GetCurrentDirectory(), BannerSiteSuffix);
        public const string BannerUserSuffix = "Image/Website/{0}/BannerUser";
        public static string BannerUserPath = Path.Combine(Directory.GetCurrentDirectory(), BannerUserSuffix);
        public const string BannerOpTypeSuffix = "Image/Website/{0}/BannerOpType/{1}";
        public static string BannerOpTypePath = Path.Combine(Directory.GetCurrentDirectory(), BannerOpTypeSuffix);
        public static string BGServicesSuffix = "Image/Website/{0}/t{1}/services";
        public static string BGServicesPath = Path.Combine(Directory.GetCurrentDirectory(), BGServicesSuffix);


        public static string PopupPath = Path.Combine(Directory.GetCurrentDirectory(), PopupSuffix);
        public const string BankQRSuffix = "Image/BankQR/";
        public static string BankQRPath = Path.Combine(Directory.GetCurrentDirectory(), BankQRSuffix);
        public static string GiftImgPath = Path.Combine(Directory.GetCurrentDirectory(), "Image/GiftImage/");
        public const string PESDocumentSuffix = "Image/PESDocument/{TID}";
        public static string PESDocumentPath = Path.Combine(Directory.GetCurrentDirectory(), PESDocumentSuffix);
        public const string ApkSuffix = "wwwroot/apk/{0}";
        public static string ApkPath = Path.Combine(Directory.GetCurrentDirectory(), ApkSuffix);
        public static string CyberPlatFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/CyberPlateFiles/myprivatekey.pfx");
        public static string PESFiles = Path.Combine(Directory.GetCurrentDirectory(), "Image/PESFiles");
        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string WebsiteFolderPath = "Image/Website/{0}/";
        public const string DefaultFolderPath = "Image/Website/{0}/";
        public const string OperatorPdfFile = "Image/planDoc/";

        public static string PaymentReceipt = Path.Combine(Directory.GetCurrentDirectory(), "Image/PaymentReceipt/");
        public const string PartnerFolderPath = "Image/Partner/";

        public static string SenderKYCPath = Path.Combine(Directory.GetCurrentDirectory(), "Image/Sender/");
        public static string FingpayCertificatePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/cer/fingpay.cer");
        public static string CertificateFooterImageSuffix = "Image/Website/{0}/CertificateFooterImage.png";
        public static string CertificateFooterImage = Path.Combine(Directory.GetCurrentDirectory(), CertificateFooterImageSuffix);
        public static string CertificateImgSide = Path.Combine(Directory.GetCurrentDirectory(), "Image/CertificateImages/side.png");
        public static string CertificateImglogoICICI = Path.Combine(Directory.GetCurrentDirectory(), "Image/CertificateImages/ICICI-logo.png");
        public static string CertificateImgSignSuffix = "Image/Website/{0}/Sign.png";
        public static string CertificateImgSign = Path.Combine(Directory.GetCurrentDirectory(), CertificateImgSignSuffix);
        public const string WebKitPathSuffix = "wwwroot/QtBinariesWindows";
        public static string WebKitPath = Path.Combine(Directory.GetCurrentDirectory(), WebKitPathSuffix);
        public static string Logo = Path.Combine(Directory.GetCurrentDirectory(), LogoSuffix);

        public const string AffiliateItemPathSUffix = "Image/AffiliateItem/{0}/";
        public static string AffiliateItemPath = Path.Combine(Directory.GetCurrentDirectory(), AffiliateItemPathSUffix);

        public static string ChannelImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/Channels/");
        public static string ProfileImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/Profile/");

        public static string ProductImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/Products/{0}/");
        public const string PESDocument = "Image/PESDocument/";
        public static string TinyMCEImagePath = "Image/Editor/{WID}/";
        public static string TinyMCEImage = Path.Combine(Directory.GetCurrentDirectory(), TinyMCEImagePath);
        public static string Employee = "Image/Employee/";

        public const string DefaultFolderTheme = "Image/Website/{0}/t1/";
        public const string ThemeSuffix = "Image/Website/{0}/t1";
        public static string ThemePath = Path.Combine(Directory.GetCurrentDirectory(), ThemeSuffix);

        public const string ShoppingImagePath = "Image/icon/Shopping/{0}";
        public static string PromoCodeImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/PromoCode/");
        public static string IconImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Image/icon/{0}/");
    }
}
