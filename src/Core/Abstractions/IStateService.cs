﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Core.Enums;
using Bit.Core.Models.Data;
using Bit.Core.Models.Domain;
using Bit.Core.Models.View;
using Bit.Core.Utilities;

namespace Bit.Core.Abstractions
{
    public interface IStateService
    {
        bool BiometricLocked { get; set; }
        ExtendedObservableCollection<AccountView> AccountViews { get; set; }
        Task<List<string>> GetUserIdsAsync();
        Task<string> GetActiveUserIdAsync();
        Task SetActiveUserAsync(string userId);
        Task<bool> IsAuthenticatedAsync(string userId = null);
        Task<bool> HasMultipleAccountsAsync();
        Task RefreshAccountViewsAsync(bool allowAddAccountRow);
        Task AddAccountAsync(Account account);
        Task ClearAsync(string userId);
        Task<EnvironmentUrlData> GetPreAuthEnvironmentUrlsAsync();
        Task SetPreAuthEnvironmentUrlsAsync(EnvironmentUrlData value);
        Task<EnvironmentUrlData> GetEnvironmentUrlsAsync(string userId = null);
        Task<bool?> GetBiometricUnlockAsync(string userId = null);
        Task SetBiometricUnlockAsync(bool? value, string userId = null);
        Task<bool> CanAccessPremiumAsync(string userId = null);
        Task<string> GetProtectedPinAsync(string userId = null);
        Task SetProtectedPinAsync(string value, string userId = null);
        Task<string> GetPinProtectedAsync(string userId = null);
        Task SetPinProtectedAsync(string value, string userId = null);
        Task<EncString> GetPinProtectedCachedAsync(string userId = null);
        Task SetPinProtectedCachedAsync(EncString value, string userId = null);
        Task<KdfType?> GetKdfTypeAsync(string userId = null);
        Task SetKdfTypeAsync(KdfType? value, string userId = null);
        Task<int?> GetKdfIterationsAsync(string userId = null);
        Task SetKdfIterationsAsync(int? value, string userId = null);
        Task<string> GetKeyEncryptedAsync(string userId = null);
        Task SetKeyEncryptedAsync(string value, string userId = null);
        Task<SymmetricCryptoKey> GetKeyDecryptedAsync(string userId = null);
        Task SetKeyDecryptedAsync(SymmetricCryptoKey value, string userId = null);
        Task<string> GetKeyHashAsync(string userId = null);
        Task SetKeyHashAsync(string value, string userId = null);
        Task<string> GetEncKeyEncryptedAsync(string userId = null);
        Task SetEncKeyEncryptedAsync(string value, string userId = null);
        Task<Dictionary<string, string>> GetOrgKeysEncryptedAsync(string userId = null);
        Task SetOrgKeysEncryptedAsync(Dictionary<string, string> value, string userId = null);
        Task<string> GetPrivateKeyEncryptedAsync(string userId = null);
        Task SetPrivateKeyEncryptedAsync(string value, string userId = null);
        Task<List<string>> GetAutofillBlacklistedUrisAsync(string userId = null);
        Task SetAutofillBlacklistedUrisAsync(List<string> value, string userId = null);
        Task<bool?> GetAutofillTileAddedAsync(string userId = null);
        Task SetAutofillTileAddedAsync(bool? value, string userId = null);
        Task<string> GetEmailAsync(string userId = null);
        Task<string> GetNameAsync(string userId = null);
        Task<long?> GetLastActiveTimeAsync(string userId = null);
        Task SetLastActiveTimeAsync(long? value, string userId = null);
        Task<int?> GetVaultTimeoutAsync(string userId = null);
        Task SetVaultTimeoutAsync(int? value, string userId = null);
        Task<string> GetVaultTimeoutActionAsync(string userId = null);
        Task SetVaultTimeoutActionAsync(string value, string userId = null);
        Task<DateTime?> GetLastFileCacheClearAsync(string userId = null);
        Task SetLastFileCacheClearAsync(DateTime? value, string userId = null);
        Task<PreviousPageInfo> GetPreviousPageInfoAsync(string userId = null);
        Task SetPreviousPageInfoAsync(PreviousPageInfo value, string userId = null);
        Task<int> GetInvalidUnlockAttemptsAsync(string userId = null);
        Task SetInvalidUnlockAttemptsAsync(int? value, string userId = null);
        Task<string> GetLastBuildAsync(string userId = null);
        Task SetLastBuildAsync(string value, string userId = null);
        Task<bool?> GetDisableFaviconAsync(string userId = null);
        Task SetDisableFaviconAsync(bool? value, string userId = null);
        Task<bool?> GetDisableAutoTotpCopyAsync(string userId = null);
        Task SetDisableAutoTotpCopyAsync(bool? value, string userId = null);
        Task<bool?> GetInlineAutofillEnabledAsync(string userId = null);
        Task SetInlineAutofillEnabledAsync(bool? value, string userId = null);
        Task<bool?> GetAutofillDisableSavePromptAsync(string userId = null);
        Task SetAutofillDisableSavePromptAsync(bool? value, string userId = null);
        Task<Dictionary<string, Dictionary<string, object>>> GetLocalDataAsync(string userId = null);
        Task SetLocalDataAsync(Dictionary<string, Dictionary<string, object>> value, string userId = null);
        Task<Dictionary<string, CipherData>> GetEncryptedCiphersAsync(string userId = null);
        Task SetEncryptedCiphersAsync(Dictionary<string, CipherData> value, string userId = null);
        Task<int?> GetDefaultUriMatchAsync(string userId = null);
        Task SetDefaultUriMatchAsync(int? value, string userId = null);
        Task<HashSet<string>> GetNeverDomainsAsync(string userId = null);
        Task SetNeverDomainsAsync(HashSet<string> value, string userId = null);
        Task<int?> GetClearClipboardAsync(string userId = null);
        Task SetClearClipboardAsync(int? value, string userId = null);
        Task<Dictionary<string, CollectionData>> GetEncryptedCollectionsAsync(string userId = null);
        Task SetEncryptedCollectionsAsync(Dictionary<string, CollectionData> value, string userId = null);
        Task<bool> GetPasswordRepromptAutofillAsync(string userId = null);
        Task SetPasswordRepromptAutofillAsync(bool? value, string userId = null);
        Task<bool> GetPasswordVerifiedAutofillAsync(string userId = null);
        Task SetPasswordVerifiedAutofillAsync(bool? value, string userId = null);
        Task<DateTime?> GetLastSyncAsync(string userId = null);
        Task SetLastSyncAsync(DateTime? value, string userId = null);
        Task<string> GetSecurityStampAsync(string userId = null);
        Task SetSecurityStampAsync(string value, string userId = null);
        Task<bool> GetEmailVerifiedAsync(string userId = null);
        Task SetEmailVerifiedAsync(bool? value, string userId = null);
        Task<bool> GetForcePasswordReset(string userId = null);
        Task SetForcePasswordResetAsync(bool? value, string userId = null);
        Task<bool> GetSyncOnRefreshAsync(string userId = null);
        Task SetSyncOnRefreshAsync(bool? value, string userId = null);
        Task<string> GetRememberedEmailAsync(string userId = null);
        Task SetRememberedEmailAsync(string value, string userId = null);
        Task<bool?> GetRememberEmailAsync(string userId = null);
        Task SetRememberEmailAsync(bool? value, string userId = null);
        Task<string> GetRememberedOrgIdentifierAsync(string userId = null);
        Task SetRememberedOrgIdentifierAsync(string value, string userId = null);
        Task<bool?> GetRememberOrgIdentifierAsync(string userId = null);
        Task SetRememberOrgIdentifierAsync(bool? value, string userId = null);
        Task<string> GetThemeAsync(string userId = null);
        Task SetThemeAsync(string value, string userId = null);
        Task<bool?> GetAddSitePromptShownAsync(string userId = null);
        Task SetAddSitePromptShownAsync(bool? value, string userId = null);
        Task<bool?> GetMigratedFromV1Async(string userId = null);
        Task SetMigratedFromV1Async(bool? value, string userId = null);
        Task<bool?> GetMigratedFromV1AutofillPromptShownAsync(string userId = null);
        Task SetMigratedFromV1AutofillPromptShownAsync(bool? value, string userId = null);
        Task<bool?> GetTriedV1ResyncAsync(string userId = null);
        Task SetTriedV1ResyncAsync(bool? value, string userId = null);
        Task<bool?> GetPushInitialPromptShownAsync(string userId = null);
        Task SetPushInitialPromptShownAsync(bool? value, string userId = null);
        Task<DateTime?> GetPushLastRegistrationDateAsync(string userId = null);
        Task SetPushLastRegistrationDateAsync(DateTime? value, string userId = null);
        Task<string> GetPushCurrentTokenAsync(string userId = null);
        Task SetPushCurrentTokenAsync(string value, string userId = null);
        Task<List<EventData>> GetEventCollectionAsync(string userId = null);
        Task SetEventCollectionAsync(List<EventData> value, string userId = null);
        Task<Dictionary<string, FolderData>> GetEncryptedFoldersAsync(string userId = null);
        Task SetEncryptedFoldersAsync(Dictionary<string, FolderData> value, string userId = null);
        Task<Dictionary<string, PolicyData>> GetEncryptedPoliciesAsync(string userId = null);
        Task SetEncryptedPoliciesAsync(Dictionary<string, PolicyData> value, string userId = null);
        Task<string> GetPushRegisteredTokenAsync();
        Task SetPushRegisteredTokenAsync(string value);
        Task<bool?> GetAppExtensionStartedAsync(string userId = null);
        Task SetAppExtensionStartedAsync(bool? value, string userId = null);
        Task<bool?> GetAppExtensionActivatedAsync(string userId = null);
        Task SetAppExtensionActivatedAsync(bool? value, string userId = null);
        Task<string> GetAppIdAsync(string userId = null);
        Task SetAppIdAsync(string value, string userId = null);
        Task<bool> GetUsesKeyConnectorAsync(string userId = null);
        Task SetUsesKeyConnectorAsync(bool? value, string userId = null);
        Task<Dictionary<string, OrganizationData>> GetOrganizationsAsync(string userId = null);
        Task SetOrganizationsAsync(Dictionary<string, OrganizationData> organizations, string userId = null);
        Task<PasswordGenerationOptions> GetPasswordGenerationOptionsAsync(string userId = null);
        Task SetPasswordGenerationOptionsAsync(PasswordGenerationOptions value, string userId = null);
        Task<List<GeneratedPasswordHistory>> GetEncryptedPasswordGenerationHistory(string userId = null);
        Task SetEncryptedPasswordGenerationHistoryAsync(List<GeneratedPasswordHistory> value, string userId = null);
        Task<Dictionary<string, SendData>> GetEncryptedSendsAsync(string userId = null);
        Task SetEncryptedSendsAsync(Dictionary<string, SendData> value, string userId = null);
        Task<Dictionary<string, object>> GetSettingsAsync(string userId = null);
        Task SetSettingsAsync(Dictionary<string, object> value, string userId = null);
        Task<string> GetAccessTokenAsync(string userId = null);
        Task SetAccessTokenAsync(string value, bool skipTokenStorage, string userId = null);
        Task<string> GetRefreshTokenAsync(string userId = null);
        Task SetRefreshTokenAsync(string value, bool skipTokenStorage, string userId = null);
        Task<string> GetTwoFactorTokenAsync(string email = null);
        Task SetTwoFactorTokenAsync(string value, string email = null);
    }
}
