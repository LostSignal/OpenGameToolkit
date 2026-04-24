//-----------------------------------------------------------------------
// <copyright file="PlayFabMessages.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if false && USING_UNITY_PURCHASING && !UNITY_XBOXONE && !UNITY_LUMIN
#define PURCHASING_ENABLED
#endif

namespace OGT.PlayFab
{
    using global::PlayFab;
    using System.Threading.Tasks;
    using UnityEngine;

#if PURCHASING_ENABLED
    using UnityEngine.Purchasing;
#endif

    public static class PlayFabMessages
    {
        private static readonly OGTLogger Logger = new("PlayFab");

        // Exit App Keys
        private const string ExitAppTitleKey = "EXIT-APP-TITLE";
        private const string ExitAppBodyKey = "EXIT-APP-BODY";

        // Forgot Password Keys
        private const string ForgotPasswordTitleKey = "FORGOT-PASSWORD-TITLE";
        private const string ForgotPasswordBodyKey = "FORGOT-PASSWORD-BODY";

        // Changing Display Name Localization Keys
        private const string ChangeNameTitleKey = "CHANGE-NAME-TITLE";
        private const string ChangeNameBodyKey = "CHANGE-NAME-BODY";
        private const string ChangeNameFailedTitleKey = "CHANGE-NAME-FAILED-TITLE";
        private const string ChangeNameFailedNotAvailableKey = "CHANGE-NAME-FAILED-NOT-AVAILABLE";
        private const string ChangeNameFailedProfaneKey = "CHANGE-NAME-FAILED-PROFANE";

        // Create User Account with Email/Password
        private const string CreateAccountTitleKey = "CREATE-ACCOUNT-TITLE";
        private const string CreateAccountEmailNotAvailableKey = "CREATE-ACCOUNT-EMAIL-NOT-AVAILABLE";
        private const string CreateAccountUsernameNotAvailableKey = "CREATE-ACCOUNT-USERNAME-NOT-AVAILABLE";
        private const string CreateAccountInvalidEmailKey = "CREATE-ACCOUNT-EMAIL-INVALID";
        private const string CreateAccountInvalidPasswordKey = "CREATE-ACCOUNT-PASSWORD-INVALID";
        private const string CreateAccountInvalidUsernameKey = "CREATE-ACCOUNT-USERNAME-INVALID";

        // PlayFab Internal Error
        private const string PlayFabErrorTitleKey = "INTERNAL-PLAYFAB-ERROR-TITLE";
        private const string PlayFabErrorBodyKey = "INTERNAL-PLAYFAB-ERROR-BODY";

        // Login Failed Keys
        private const string LoginFailedTitleKey = "LOGIN-FAILED-TITLE";
        private const string AccountNotFoundKey = "LOGIN-ACCOUNT-NOT-FOUND";
        private const string InvalidEmailOrPasswordKey = "INVALID-EMAIL-OR-PASSWORD";

        // Contact Email Address Not Found
        private const string ContactEmailNotFoundTitleKey = "CONTACT-EMAIL-NOT-FOUND-TITLE";
        private const string ContactEmailNotFoundBodyKey = "CONTACT-EMAIL-NOT-FOUND-BODY";

        // Connecting To Store Localization Keys
        private const string ConnectingToStoreBodyKey = "CONNECTING-TO-STORE-BODY";

        // Insufficient Currency Localization Keys
        private const string InsufficientCurrencyTitleKey = "INSUFFICIENT-CURRENCY-TITLE";
        private const string InsufficientCurrencyBodyKey = "INSUFFICIENT-CURRENCY-BODY";

        // Purchasing Localization Keys
        private const string PurchaseFailedTitleKey = "PURCHASE-FAILED-TITLE";
        private const string PurchaseFailedDuplicateTransactionKey = "PURCHASE-FAILED-DUPLICATE-TRANSACTION";
        private const string PurchaseFailedExistingPurchasePendingKey = "PURCHASE-FAILED-PURCHASE-PENDING";
        private const string PurchaseFailedPaymentDeclinedKey = "PURCHASE-FAILED-PAYMENT-DECLINED";
        private const string PurchaseFailedProductUnavailableKey = "PURCHASE-FAILED-PRODUCT-UNAVAILABLE";
        private const string PurchaseFailedPurchasingUnavailableKey = "PURCHASE-FAILED-PURCHASING-UNVAILABLE";
        private const string PurchaseFailedSignatureInvalidKey = "PURCHASE-FAILED-SIGNATURE-INVALID";
        private const string PurchaseFailedUnknownKey = "PURCHASE-FAILED-UNKNOWN";
        private const string PurchaseFailedUnableToValidateReceiptKey = "PURCHASE-FAILED-UNABLE-TO-VALIDATE-RECEIPT";

        // Store Localization Keys
        private const string StoreFailedTitleKey = "STORE-FAILED-TITLE";
        private const string StoreFailedAppNotKnownKey = "STORE-FAILED-APP-NOT-KNOWN";
        private const string StoreFailedNoProductsAvailableKey = "STORE-FAILED-NO-PRODUCTS-AVAILABLE";
        private const string StoreFailedPurchasingUnavailableKey = "STORE-FAILED-PURCHASING-UNAVAILABLE";
        private const string StoreFailedConnectionTimeOutKey = "STORE-FAILED-CONNECTION-TIME-OUT";

        public static Awaitable<YesNoResult> ShowExitAppPrompt()
        {
            return MessageBox.Instance.ShowYesNo(Get(ExitAppTitleKey), Get(ExitAppBodyKey));
        }

        public static Awaitable<YesNoResult> ShowForgotPasswordPrompt(string email)
        {
            return MessageBox.Instance.ShowYesNo(Get(ForgotPasswordTitleKey), Get(ForgotPasswordBodyKey).Replace("{email}", email));
        }

        public static Awaitable<YesNoResult> ShowInsufficientCurrency()
        {
            return MessageBox.Instance.ShowYesNo(Get(InsufficientCurrencyTitleKey), Get(InsufficientCurrencyBodyKey));
        }

        public static void ShowConnectingToStoreSpinner()
        {
            SpinnerBox.Instance.UpdateBodyText(Get(ConnectingToStoreBodyKey));
        }

        public static Task<StringInputResult> ShowChangeDisplayNameInputBox(string currentDisplayName)
        {
            var title = Get(ChangeNameTitleKey);
            var body = Get(ChangeNameBodyKey);
            return StringInputBox.Instance.Show(title, body, currentDisplayName, 15);
        }

        public static UnityTask<OkResult> HandleError(System.Exception exception)
        {
            UnityTask<OkResult> result = null;

#if PURCHASING_ENABLED
            result ??= HandlePurchasingError(exception as PurchasingException);
            result ??= HandlePurchasingInitializationError(exception as PurchasingInitializationException);
            result ??= HandlePurchasingInitializationTimeOutError(exception as PurchasingInitializationTimeOutException);
#endif

            return result ?? HandlePlayFabError(exception as PlayFab.PlayFabException);
        }

        // TODO [bgish]: Need to move all of this to a localization table
        private static string Get(string localizationKey)
        {
            bool english = Localization.Localization.CurrentLanguage == Localization.Languages.English;

            return localizationKey switch
            {
                // Exit App Keys
                ExitAppTitleKey => english ? "Exit?" : string.Empty,
                ExitAppBodyKey => english ? "Are you sure you want to exit?" : string.Empty,

                // Forgot Password Keys
                ForgotPasswordTitleKey => english ? "Forgot Password" : string.Empty,
                ForgotPasswordBodyKey => english ? "Are you sure you wish to send an account recovery email to \"{email}\"?" : string.Empty,

                // Internal PlayFab Error Keys
                PlayFabErrorTitleKey => english ? "Internal Error" : string.Empty,
                PlayFabErrorBodyKey => english ? "We have encountered an internal error.  Please try again later." : string.Empty,

                // Login Failure Keys
                LoginFailedTitleKey => english ? "Login Failed" : string.Empty,
                AccountNotFoundKey => english ? "The specified account could not be found." : string.Empty,
                InvalidEmailOrPasswordKey => english ? "The given email address or password is incorrect." : string.Empty,

                // Creating User Account
                CreateAccountTitleKey => english ? "Create Account Failed" : string.Empty,
                CreateAccountEmailNotAvailableKey => english ? "The specified email address is not available." : string.Empty,
                CreateAccountUsernameNotAvailableKey => english ? "The specified username is not available." : string.Empty,
                CreateAccountInvalidEmailKey => english ? "The specified email address is invalid." : string.Empty,
                CreateAccountInvalidPasswordKey => english ? "The specified password is invalid.  Must be between 5 and 100 characters long." : string.Empty,
                CreateAccountInvalidUsernameKey => english ? "The specified username is invalid.  Must be between 3 and 20 characters long." : string.Empty,

                // No Contact Email
                ContactEmailNotFoundTitleKey => english ? "Email Not Found" : string.Empty,
                ContactEmailNotFoundBodyKey => english ? "The specified email address could not be found." : string.Empty,

                // Changing Display Name
                ChangeNameTitleKey => english ? "Display Name" : "Tên hiển thị",
                ChangeNameBodyKey => english ? "Enter in your new display name." : "Nhập tên mới ...",
                ChangeNameFailedTitleKey => english ? "Rename Failed" : "Lỗi khi đổi tên",
                ChangeNameFailedNotAvailableKey => english ? "That name is currently not available." : "Tên mới không khả dụng",
                ChangeNameFailedProfaneKey => english ? "That name contains profanity." : "Tên mới tồn tại tự thô tục.",

                // Connecting To Store
                ConnectingToStoreBodyKey => english ? "Connecting to store..." : "Đang kết tối tới Cửa Hàng...",

                // Insufficient Currency
                InsufficientCurrencyTitleKey => english ? "Not Enough Currency" : "Không đủ tiền tệ",
                InsufficientCurrencyBodyKey => english ? "You'll need to buy more currency from the store.<br>Would you like to go there now?" : "Bạn cần thêm tiền từ Cửa Hàng? Đến Cửa Hàng ngay?",

                // Purchasing
                PurchaseFailedTitleKey => english ? "Purchase failed" : "Giao dịch thất bại",
                PurchaseFailedDuplicateTransactionKey => english ? "We've encountered a duplicate transaction." : "Phát hiện giao dịch bị trùng lặp",
                PurchaseFailedExistingPurchasePendingKey => english ? "An existing purchase is already pending." : "Giao dịch đã tồn tại và đang được xử lý.",
                PurchaseFailedPaymentDeclinedKey => english ? "The payment has been declined." : "Giao dịch bị từ chối",
                PurchaseFailedProductUnavailableKey => english ? "The product is unavailable." : "Sản phẩm không khả dụng",
                PurchaseFailedPurchasingUnavailableKey => english ? "Purchasing is currenctly unavailable." : "Tạm thời không thực hiện được giao dịch",
                PurchaseFailedSignatureInvalidKey => english ? "Signature was invalid." : "Chữ kí không có hợp lệ",
                PurchaseFailedUnknownKey => english ? "Sorry we've encountered an unknown error." : "Xảy ra lỗi không xác định.",
                PurchaseFailedUnableToValidateReceiptKey => english ? "Unable to validate receipt." : "Không thể xác nhận đơn hàng.",

                // Store
                StoreFailedTitleKey => english ? "Store Error" : "Lỗi Cửa hàng",
                StoreFailedAppNotKnownKey => english ? "The store doesn't recognize this application." : "Ứng dụng không tồn tại trong Cửa hàng",
                StoreFailedNoProductsAvailableKey => english ? "There are no valid products available for this application." : "Không tồn tại sản phẩm nào khả dụng cho ứng dụng này.",
                StoreFailedPurchasingUnavailableKey => english ? "Unable to purchase.  Purchases have been turned off for this application." : "Không thể thực hiện thanh toán. Ứng dụng này đã bị tắt tính năng thanh toán.",
                StoreFailedConnectionTimeOutKey => english ? "We timed out trying to connect to the store." : "Quá thời gian chờ phản hồi từ Cửa hàng",
                _ => null,
            };
        }

        private static UnityTask<OkResult> HandlePlayFabError(PlayFab.PlayFabException playfabException)
        {
            if (playfabException == null)
            {
                return null;
            }

            switch (playfabException.Error.Error)
            {
                // Major Errors
                case PlayFabErrorCode.InvalidPartnerResponse:
                case PlayFabErrorCode.InvalidTitleId:
                case PlayFabErrorCode.SmtpAddonNotEnabled:
                case PlayFabErrorCode.InvalidParams:
                    {
                        Logger.LogErrorFormat("Internal PlayFab Error: {0}", playfabException.Error.Error);
                        Logger.LogException(playfabException);

                        MessageBox.Instance.ShowOk(Get(PlayFabErrorTitleKey), Get(PlayFabErrorBodyKey));
                        break;
                    }

                // Display Name Changing Errors
                case PlayFabErrorCode.NameNotAvailable:
                    {
                        MessageBox.Instance.ShowOk(Get(ChangeNameFailedTitleKey), Get(ChangeNameFailedNotAvailableKey));
                        break;
                    }

                case PlayFabErrorCode.ProfaneDisplayName:
                    {
                        MessageBox.Instance.ShowOk(Get(ChangeNameFailedTitleKey), Get(ChangeNameFailedProfaneKey));
                        break;
                    }

                // Registering PlayFab User Errors
                case PlayFabErrorCode.EmailAddressNotAvailable:
                    {
                        MessageBox.Instance.ShowOk(Get(CreateAccountTitleKey), Get(CreateAccountEmailNotAvailableKey));
                        break;
                    }

                case PlayFabErrorCode.UsernameNotAvailable:
                    {
                        MessageBox.Instance.ShowOk(Get(CreateAccountTitleKey), Get(CreateAccountUsernameNotAvailableKey));
                        break;
                    }

                case PlayFabErrorCode.InvalidEmailAddress:
                    {
                        MessageBox.Instance.ShowOk(Get(CreateAccountTitleKey), Get(CreateAccountInvalidEmailKey));
                        break;
                    }

                case PlayFabErrorCode.InvalidPassword:
                    {
                        MessageBox.Instance.ShowOk(Get(CreateAccountTitleKey), Get(CreateAccountInvalidPasswordKey));
                        break;
                    }

                case PlayFabErrorCode.InvalidUsername:
                    {
                        MessageBox.Instance.ShowOk(Get(CreateAccountTitleKey), Get(CreateAccountInvalidUsernameKey));
                        break;
                    }

                // Login Errors
                case PlayFabErrorCode.AccountNotFound:
                    {
                        MessageBox.Instance.ShowOk(Get(LoginFailedTitleKey), Get(AccountNotFoundKey));
                        break;
                    }

                case PlayFabErrorCode.InvalidEmailOrPassword:
                    {
                        MessageBox.Instance.ShowOk(Get(LoginFailedTitleKey), Get(InvalidEmailOrPasswordKey));
                        break;
                    }

                // Email Address Not Found
                case PlayFabErrorCode.NoContactEmailAddressFound:
                    {
                        MessageBox.Instance.ShowOk(Get(ContactEmailNotFoundTitleKey), Get(ContactEmailNotFoundBodyKey));
                        break;
                    }

                // common receipt errors between iOS/Android
                case PlayFabErrorCode.ReceiptCancelled:
                case PlayFabErrorCode.InvalidBundleID:
                case PlayFabErrorCode.InvalidReceipt:
                case PlayFabErrorCode.NoMatchingCatalogItemForReceipt:
                case PlayFabErrorCode.ReceiptAlreadyUsed:
                case PlayFabErrorCode.SubscriptionAlreadyTaken:
                case PlayFabErrorCode.InvalidProductForSubscription:

                // ios receipt errors
                case PlayFabErrorCode.DownstreamServiceUnavailable:
                case PlayFabErrorCode.InvalidCurrencyCode:
                case PlayFabErrorCode.InvalidEnvironmentForReceipt:
                case PlayFabErrorCode.InvalidVirtualCurrency:
                case PlayFabErrorCode.ReceiptContainsMultipleInAppItems:
                case PlayFabErrorCode.ReceiptDoesNotContainInAppItems:

                // android receipt errors
                case PlayFabErrorCode.MissingTitleGoogleProperties:
                case PlayFabErrorCode.NoRealMoneyPriceForCatalogItem:
                    {
                        Logger.LogErrorFormat("Hit Error {0} while validating receipt", playfabException.Error.Error);
                        MessageBox.Instance.ShowOk(Get(PurchaseFailedTitleKey), Get(PurchaseFailedUnableToValidateReceiptKey));
                        break;
                    }

                    // TODO [bgish] - handle way more...
            }

            return null;
        }

#if PURCHASING_ENABLED

        private static UnityTask<OkResult> HandlePurchasingInitializationError(PurchasingInitializationException purchasingInitializationException)
        {
            if (purchasingInitializationException == null)
            {
                return null;
            }

            switch (purchasingInitializationException.FailureReason)
            {
                case InitializationFailureReason.AppNotKnown:
                    Lost.Logger.LogErrorFormat("Error initializing purchasing \"{0}\"", purchasingInitializationException.FailureReason.ToString());
                    return MessageBox.Instance.ShowOk(Get(StoreFailedTitleKey), Get(StoreFailedAppNotKnownKey));

                case InitializationFailureReason.NoProductsAvailable:
                    Lost.Logger.LogErrorFormat("Error initializing purchasing \"{0}\"", purchasingInitializationException.FailureReason.ToString());
                    return MessageBox.Instance.ShowOk(Get(StoreFailedTitleKey), Get(StoreFailedNoProductsAvailableKey));

                case InitializationFailureReason.PurchasingUnavailable:
                    return MessageBox.Instance.ShowOk(Get(StoreFailedTitleKey), Get(StoreFailedPurchasingUnavailableKey));

                default:
                    return null;
            }
        }

        private static UnityTask<OkResult> HandlePurchasingInitializationTimeOutError(PurchasingInitializationTimeOutException purchasingInitializationTimeOutException)
        {
            if (purchasingInitializationTimeOutException == null)
            {
                return null;
            }

            return MessageBox.Instance.ShowOk(Get(StoreFailedTitleKey), Get(StoreFailedConnectionTimeOutKey));
        }

        private static UnityTask<OkResult> HandlePurchasingError(PurchasingException purchasingException)
        {
            if (purchasingException == null)
            {
                return null;
            }

            string title = Get(PurchaseFailedTitleKey);

            return purchasingException.FailureReason switch
            {
                PurchaseFailureReason.DuplicateTransaction => MessageBox.Instance.ShowOk(title, Get(PurchaseFailedDuplicateTransactionKey)),
                PurchaseFailureReason.ExistingPurchasePending => MessageBox.Instance.ShowOk(title, Get(PurchaseFailedExistingPurchasePendingKey)),
                PurchaseFailureReason.PaymentDeclined => MessageBox.Instance.ShowOk(title, Get(PurchaseFailedPaymentDeclinedKey)),
                PurchaseFailureReason.ProductUnavailable => MessageBox.Instance.ShowOk(title, Get(PurchaseFailedProductUnavailableKey)),
                PurchaseFailureReason.PurchasingUnavailable => MessageBox.Instance.ShowOk(title, Get(PurchaseFailedPurchasingUnavailableKey)),
                PurchaseFailureReason.SignatureInvalid => MessageBox.Instance.ShowOk(title, Get(PurchaseFailedSignatureInvalidKey)),
                PurchaseFailureReason.Unknown => MessageBox.Instance.ShowOk(title, Get(PurchaseFailedUnknownKey)),
                PurchaseFailureReason.UserCancelled => null, // Do nothing, they know they canceled it
                _ => null,
            };
        }

#endif
    }
}
