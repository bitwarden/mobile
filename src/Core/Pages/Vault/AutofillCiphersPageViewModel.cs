﻿using Bit.App.Models;
using Bit.App.Utilities;
using Bit.Core;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.View;
using Bit.Core.Resources.Localization;
using Bit.Core.Utilities;

namespace Bit.App.Pages
{
    public class AutofillCiphersPageViewModel : CipherSelectionPageViewModel
    {
        private CipherType? _fillType;

        public string Uri { get; set; }

        public override void Init(AppOptions appOptions)
        {
            Uri = appOptions?.Uri;
            _fillType = appOptions.FillType;

            string name = null;
            if (Uri?.StartsWith(Constants.AndroidAppProtocol) ?? false)
            {
                name = Uri.Substring(Constants.AndroidAppProtocol.Length);
            }
            else
            {
                name = CoreHelpers.GetDomain(Uri);
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "--";
            }
            Name = name;
            PageTitle = string.Format(AppResources.ItemsForUri, Name ?? "--");
            NoDataText = string.Format(AppResources.NoItemsForUri, Name ?? "--");
        }

        protected override async Task<List<GroupingsPageListGroup>> LoadGroupedItemsAsync()
        {
            var groupedItems = new List<GroupingsPageListGroup>();
            var ciphers = await _cipherService.GetAllDecryptedByUrlAsync(Uri, null);

            var matching = ciphers.Item1?.Select(c => new CipherItemViewModel(c, WebsiteIconsEnabled)).ToList();
            var hasMatching = matching?.Any() ?? false;
            if (matching?.Any() ?? false)
            {
                groupedItems.Add(
                    new GroupingsPageListGroup(matching, AppResources.MatchingItems, matching.Count, false, true));
            }

            var fuzzy = ciphers.Item2?.Select(c =>
                new CipherItemViewModel(c, WebsiteIconsEnabled, true)).ToList();
            if (fuzzy?.Any() ?? false)
            {
                groupedItems.Add(
                    new GroupingsPageListGroup(fuzzy, AppResources.PossibleMatchingItems, fuzzy.Count, false,
                    !hasMatching));
            }

            return groupedItems;
        }

        protected override async Task SelectCipherAsync(IGroupingsPageListItem item)
        {
            if (!(item is CipherItemViewModel listItem) || listItem.Cipher is null)
            {
                return;
            }

            var cipher = listItem.Cipher;

            if (_deviceActionService.SystemMajorVersion() < 21)
            {
                await AppHelpers.CipherListOptions(Page, cipher, _passwordRepromptService);
                return;
            }

            if (!await _passwordRepromptService.PromptAndCheckPasswordIfNeededAsync(cipher.Reprompt))
            {
                return;
            }
            var autofillResponse = AppResources.Yes;
            if (listItem.FuzzyAutofill)
            {
                var options = new List<string> { AppResources.Yes };
                if (cipher.Type == CipherType.Login &&
                    Microsoft.Maui.Networking.Connectivity.NetworkAccess != Microsoft.Maui.Networking.NetworkAccess.None)
                {
                    options.Add(AppResources.YesAndSave);
                }
                autofillResponse = await _deviceActionService.DisplayAlertAsync(null,
                    string.Format(AppResources.BitwardenAutofillServiceMatchConfirm, Name), AppResources.No,
                    options.ToArray());
            }
            if (autofillResponse == AppResources.YesAndSave && cipher.Type == CipherType.Login)
            {
                var uris = cipher.Login?.Uris?.ToList();
                if (uris == null)
                {
                    uris = new List<LoginUriView>();
                }
                uris.Add(new LoginUriView
                {
                    Uri = Uri,
                    Match = null
                });
                cipher.Login.Uris = uris;
                try
                {
                    await _deviceActionService.ShowLoadingAsync(AppResources.Saving);
                    await _cipherService.SaveWithServerAsync(await _cipherService.EncryptAsync(cipher));
                    await _deviceActionService.HideLoadingAsync();
                }
                catch (ApiException e)
                {
                    await _deviceActionService.HideLoadingAsync();
                    if (e?.Error != null)
                    {
                        await _platformUtilsService.ShowDialogAsync(e.Error.GetSingleMessage(),
                            AppResources.AnErrorHasOccurred);
                    }
                }
            }
            if (autofillResponse == AppResources.Yes || autofillResponse == AppResources.YesAndSave)
            {
                _autofillHandler.Autofill(cipher);
            }
        }

        protected override async Task AddCipherAsync()
        {
            if (_fillType.HasValue && _fillType != CipherType.Login)
            {
                var pageForOther = new CipherAddEditPage(type: _fillType, fromAutofill: true);
                await Page.Navigation.PushModalAsync(new NavigationPage(pageForOther));
                return;
            }

            var pageForLogin = new CipherAddEditPage(null, CipherType.Login, uri: Uri, name: Name,
                fromAutofill: true);
            await Page.Navigation.PushModalAsync(new NavigationPage(pageForLogin));
        }
    }
}
