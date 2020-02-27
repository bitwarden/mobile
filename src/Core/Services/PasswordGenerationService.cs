﻿using Bit.Core.Abstractions;
using Bit.Core.Models.Domain;
using Bit.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bit.Core.Enums;

namespace Bit.Core.Services
{
    public class PasswordGenerationService : IPasswordGenerationService
    {
        private const string Keys_Options = "passwordGenerationOptions";
        private const string Keys_History = "generatedPasswordHistory";
        private const int MaxPasswordsInHistory = 100;
        private const string LowercaseCharSet = "abcdefghijkmnopqrstuvwxyz";
        private const string UppercaseCharSet = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string NumberCharSet = "23456789";
        private const string SpecialCharSet = "!@#$%^&*";

        private readonly ICryptoService _cryptoService;
        private readonly IStorageService _storageService;
        private readonly ICryptoFunctionService _cryptoFunctionService;
        private readonly IPolicyService _policyService;
        private PasswordGenerationOptions _defaultOptions = new PasswordGenerationOptions(true);
        private PasswordGenerationOptions _optionsCache;
        private List<GeneratedPasswordHistory> _history;

        public PasswordGenerationService(
            ICryptoService cryptoService,
            IStorageService storageService,
            ICryptoFunctionService cryptoFunctionService,
            IPolicyService policyService)
        {
            _cryptoService = cryptoService;
            _storageService = storageService;
            _cryptoFunctionService = cryptoFunctionService;
            _policyService = policyService;
        }

        public async Task<string> GeneratePasswordAsync(PasswordGenerationOptions options)
        {
            // Overload defaults with given options
            options.Merge(_defaultOptions);
            if(options.Type == "passphrase")
            {
                return await GeneratePassphraseAsync(options);
            }

            // Sanitize
            if(options.Uppercase.GetValueOrDefault() && options.MinUppercase.GetValueOrDefault() <= 0)
            {
                options.MinUppercase = 1;
            }
            else if(!options.Uppercase.GetValueOrDefault())
            {
                options.MinUppercase = 0;
            }

            if(options.Lowercase.GetValueOrDefault() && options.MinLowercase.GetValueOrDefault() <= 0)
            {
                options.MinLowercase = 1;
            }
            else if(!options.Lowercase.GetValueOrDefault())
            {
                options.MinLowercase = 0;
            }

            if(options.Number.GetValueOrDefault() && options.MinNumber.GetValueOrDefault() <= 0)
            {
                options.MinNumber = 1;
            }
            else if(!options.Number.GetValueOrDefault())
            {
                options.MinNumber = 0;
            }

            if(options.Special.GetValueOrDefault() && options.MinSpecial.GetValueOrDefault() <= 0)
            {
                options.MinSpecial = 1;
            }
            else if(!options.Special.GetValueOrDefault())
            {
                options.MinSpecial = 0;
            }

            if(options.Length.GetValueOrDefault() < 1)
            {
                options.Length = 10;
            }
            var minLength = options.MinSpecial.GetValueOrDefault() + options.MinLowercase.GetValueOrDefault() +
                options.MinNumber.GetValueOrDefault() + options.MinUppercase.GetValueOrDefault();
            if(options.Length < minLength)
            {
                options.Length = minLength;
            }

            var positionsBuilder = new StringBuilder();
            if(options.Lowercase.GetValueOrDefault() && options.MinLowercase.GetValueOrDefault() > 0)
            {
                for(int i = 0; i < options.MinLowercase.GetValueOrDefault(); i++)
                {
                    positionsBuilder.Append("l");
                }
            }
            if(options.Uppercase.GetValueOrDefault() && options.MinUppercase.GetValueOrDefault() > 0)
            {
                for(int i = 0; i < options.MinUppercase.GetValueOrDefault(); i++)
                {
                    positionsBuilder.Append("u");
                }
            }
            if(options.Number.GetValueOrDefault() && options.MinNumber.GetValueOrDefault() > 0)
            {
                for(int i = 0; i < options.MinNumber.GetValueOrDefault(); i++)
                {
                    positionsBuilder.Append("n");
                }
            }
            if(options.Special.GetValueOrDefault() && options.MinSpecial.GetValueOrDefault() > 0)
            {
                for(int i = 0; i < options.MinSpecial.GetValueOrDefault(); i++)
                {
                    positionsBuilder.Append("s");
                }
            }
            while(positionsBuilder.Length < options.Length.GetValueOrDefault())
            {
                positionsBuilder.Append("a");
            }

            // Shuffle
            var positions = positionsBuilder.ToString().ToCharArray()
                .OrderBy(a => _cryptoFunctionService.RandomNumber()).ToArray();

            // Build out other character sets
            var allCharSet = string.Empty;
            var lowercaseCharSet = LowercaseCharSet;
            if(options.Ambiguous.GetValueOrDefault())
            {
                lowercaseCharSet = string.Concat(lowercaseCharSet, "l");
            }
            if(options.Lowercase.GetValueOrDefault())
            {
                allCharSet = string.Concat(allCharSet, lowercaseCharSet);
            }

            var uppercaseCharSet = UppercaseCharSet;
            if(options.Ambiguous.GetValueOrDefault())
            {
                uppercaseCharSet = string.Concat(uppercaseCharSet, "IO");
            }
            if(options.Uppercase.GetValueOrDefault())
            {
                allCharSet = string.Concat(allCharSet, uppercaseCharSet);
            }

            var numberCharSet = NumberCharSet;
            if(options.Ambiguous.GetValueOrDefault())
            {
                numberCharSet = string.Concat(numberCharSet, "01");
            }
            if(options.Number.GetValueOrDefault())
            {
                allCharSet = string.Concat(allCharSet, numberCharSet);
            }

            var specialCharSet = SpecialCharSet;
            if(options.Special.GetValueOrDefault())
            {
                allCharSet = string.Concat(allCharSet, specialCharSet);
            }

            var password = new StringBuilder();
            for(var i = 0; i < options.Length.GetValueOrDefault(); i++)
            {
                var positionChars = string.Empty;
                switch(positions[i])
                {
                    case 'l':
                        positionChars = lowercaseCharSet;
                        break;
                    case 'u':
                        positionChars = uppercaseCharSet;
                        break;
                    case 'n':
                        positionChars = numberCharSet;
                        break;
                    case 's':
                        positionChars = specialCharSet;
                        break;
                    case 'a':
                        positionChars = allCharSet;
                        break;
                }

                var randomCharIndex = await _cryptoService.RandomNumberAsync(0, positionChars.Length - 1);
                password.Append(positionChars[randomCharIndex]);
            }

            return password.ToString();
        }

        public async Task<string> GeneratePassphraseAsync(PasswordGenerationOptions options)
        {
            options.Merge(_defaultOptions);
            if(options.NumWords.GetValueOrDefault() <= 2)
            {
                options.NumWords = _defaultOptions.NumWords;
            }
            if(options.WordSeparator == null || options.WordSeparator.Length == 0 || options.WordSeparator.Length > 1)
            {
                options.WordSeparator = " ";
            }
            if(options.Capitalize == null)
            {
                options.Capitalize = false;
            }
            if(options.IncludeNumber == null)
            {
                options.IncludeNumber = false;
            }
            var listLength = EEFLongWordList.Instance.List.Count - 1;
            var wordList = new List<string>();
            for(int i = 0; i < options.NumWords.GetValueOrDefault(); i++)
            {
                var wordIndex = await _cryptoService.RandomNumberAsync(0, listLength);
                if(options.Capitalize.GetValueOrDefault())
                {
                    wordList.Add(Capitalize(EEFLongWordList.Instance.List[wordIndex]));
                }
                else
                {
                    wordList.Add(EEFLongWordList.Instance.List[wordIndex]);
                }
            }
            if(options.IncludeNumber.GetValueOrDefault())
            {
                await AppendRandomNumberToRandomWordAsync(wordList);
            }
            return string.Join(options.WordSeparator, wordList);
        }

        public async Task<(PasswordGenerationOptions,PasswordGeneratorPolicyOptions)> GetOptionsAsync()
        {
            if(_optionsCache == null)
            {
                var options = await _storageService.GetAsync<PasswordGenerationOptions>(Keys_Options);
                if(options == null)
                {
                    _optionsCache = _defaultOptions;
                }
                else
                {
                    options.Merge(_defaultOptions);
                    _optionsCache = options;
                }
            }

            var enforcedPolicyOptions = await GetPasswordGeneratorPolicyOptions();
            if(enforcedPolicyOptions != null)
            {
                if (_optionsCache.Length < enforcedPolicyOptions.MinLength) {
                    _optionsCache.Length = enforcedPolicyOptions.MinLength;
                }

                if (enforcedPolicyOptions.UseUppercase) {
                    _optionsCache.Uppercase = true;
                }

                if (enforcedPolicyOptions.UseLowercase) {
                    _optionsCache.Lowercase = true;
                }

                if (enforcedPolicyOptions.UseNumbers) {
                    _optionsCache.Number = true;
                }

                if (_optionsCache.MinNumber < enforcedPolicyOptions.NumberCount) {
                    _optionsCache.MinNumber = enforcedPolicyOptions.NumberCount;
                }

                if (enforcedPolicyOptions.UseSpecial) {
                    _optionsCache.Special = true;
                }

                if (_optionsCache.MinSpecial < enforcedPolicyOptions.SpecialCount) {
                    _optionsCache.MinSpecial = enforcedPolicyOptions.SpecialCount;
                }

                // Must normalize these fields because the receiving call expects all options to pass the current rules
                if (_optionsCache.MinSpecial + _optionsCache.MinNumber > _optionsCache.Length) {
                    _optionsCache.MinSpecial = _optionsCache.Length - _optionsCache.MinNumber;
                }
            } 
            else 
            { 
                // UI layer expects an instantiated object to prevent more explicit null checks
                enforcedPolicyOptions = new PasswordGeneratorPolicyOptions();
            }
            
            return (_optionsCache, enforcedPolicyOptions);
        }
        
        public async Task<PasswordGeneratorPolicyOptions> GetPasswordGeneratorPolicyOptions()
        {
            var policies = await _policyService.GetAll(PolicyType.PasswordGenerator);
            PasswordGeneratorPolicyOptions enforcedOptions = null;
            
            if (policies == null || !policies.Any()) {
                return enforcedOptions;
            }
            
            foreach(var currentPolicy in policies) {
                if(!currentPolicy.Enabled || currentPolicy.Data == null)
                {
                    continue;
                }

                if(enforcedOptions == null)
                {
                    enforcedOptions = new PasswordGeneratorPolicyOptions();
                }
                
                var currentPolicyMinLength = currentPolicy.Data["minLength"];
                if(currentPolicyMinLength != null && Convert.ToInt32(currentPolicyMinLength) > enforcedOptions.MinLength)
                {
                    enforcedOptions.MinLength = Convert.ToInt32(currentPolicyMinLength);
                }

                var currentPolicyUseUpper = currentPolicy.Data["useUpper"];
                if(currentPolicyUseUpper != null && Convert.ToBoolean(currentPolicyUseUpper))
                {
                    enforcedOptions.UseUppercase = true;
                }

                var currentPolicyUseLower = currentPolicy.Data["useLower"];
                if(currentPolicyUseLower != null && Convert.ToBoolean(currentPolicyUseLower))
                {
                    enforcedOptions.UseLowercase = true;
                }

                var currentPolicyUseNumbers = currentPolicy.Data["useNumbers"];
                if(currentPolicyUseNumbers != null && Convert.ToBoolean(currentPolicyUseNumbers))
                {
                    enforcedOptions.UseNumbers = true;
                }

                var currentPolicyMinNumbers = currentPolicy.Data["minNumbers"];
                if(currentPolicyMinNumbers != null && Convert.ToInt32(currentPolicyMinNumbers) > enforcedOptions.NumberCount)
                {
                    enforcedOptions.NumberCount = Convert.ToInt32(currentPolicyMinNumbers);
                }

                var currentPolicyUseSpecial = currentPolicy.Data["useSpecial"];
                if(currentPolicyUseSpecial != null && Convert.ToBoolean(currentPolicyUseSpecial))
                {
                    enforcedOptions.UseSpecial = true;
                }

                var currentPolicyMinSpecial = currentPolicy.Data["minSpecial"];
                if(currentPolicyMinSpecial != null && Convert.ToInt32(currentPolicyMinSpecial) > enforcedOptions.SpecialCount)
                {
                    enforcedOptions.SpecialCount = Convert.ToInt32(currentPolicyMinSpecial);
                }
            }

            return enforcedOptions;
        }

        public async Task SaveOptionsAsync(PasswordGenerationOptions options)
        {
            await _storageService.SaveAsync(Keys_Options, options);
            _optionsCache = options;
        }

        public async Task<List<GeneratedPasswordHistory>> GetHistoryAsync()
        {
            var hasKey = await _cryptoService.HasKeyAsync();
            if(!hasKey)
            {
                return new List<GeneratedPasswordHistory>();
            }
            if(_history == null)
            {
                var encrypted = await _storageService.GetAsync<List<GeneratedPasswordHistory>>(Keys_History);
                _history = await DecryptHistoryAsync(encrypted);
            }
            return _history ?? new List<GeneratedPasswordHistory>();
        }

        public async Task AddHistoryAsync(string password, CancellationToken token = default(CancellationToken))
        {
            var hasKey = await _cryptoService.HasKeyAsync();
            if(!hasKey)
            {
                return;
            }
            var currentHistory = await GetHistoryAsync();
            // Prevent duplicates
            if(MatchesPrevious(password, currentHistory))
            {
                return;
            }
            token.ThrowIfCancellationRequested();
            currentHistory.Insert(0, new GeneratedPasswordHistory { Password = password, Date = DateTime.UtcNow });
            // Remove old items.
            if(currentHistory.Count > MaxPasswordsInHistory)
            {
                currentHistory.RemoveAt(currentHistory.Count - 1);
            }
            var newHistory = await EncryptHistoryAsync(currentHistory);
            token.ThrowIfCancellationRequested();
            await _storageService.SaveAsync(Keys_History, newHistory);
        }

        public async Task ClearAsync()
        {
            _history = new List<GeneratedPasswordHistory>();
            await _storageService.RemoveAsync(Keys_History);
        }

        public Task<object> PasswordStrength(string password, List<string> userInputs = null)
        {
            throw new NotImplementedException();
        }

        public void NormalizeOptions(PasswordGenerationOptions options, 
            PasswordGeneratorPolicyOptions enforcedPolicyOptions)
        {
            options.MinLowercase = 0;
            options.MinUppercase = 0;

            if(!options.Uppercase.GetValueOrDefault() && !options.Lowercase.GetValueOrDefault() &&
                !options.Number.GetValueOrDefault() && !options.Special.GetValueOrDefault())
            {
                options.Lowercase = true;
            }

            var length = options.Length.GetValueOrDefault();
            if(length < 5)
            {
                options.Length = 5;
            }
            else if(length > 128)
            {
                options.Length = 128;
            }
            
            if (options.Length < enforcedPolicyOptions.MinLength) {
                options.Length = enforcedPolicyOptions.MinLength;
            }

            if(options.MinNumber == null)
            {
                options.MinNumber = 0;
            }
            else if(options.MinNumber > options.Length)
            {
                options.MinNumber = options.Length;
            }
            else if(options.MinNumber > 9)
            {
                options.MinNumber = 9;
            }
            
            if (options.MinNumber < enforcedPolicyOptions.NumberCount) {
                options.MinNumber = enforcedPolicyOptions.NumberCount;
            }

            if(options.MinSpecial == null)
            {
                options.MinSpecial = 0;
            }
            else if(options.MinSpecial > options.Length)
            {
                options.MinSpecial = options.Length;
            }
            else if(options.MinSpecial > 9)
            {
                options.MinSpecial = 9;
            }
            
            if (options.MinSpecial < enforcedPolicyOptions.SpecialCount) {
                options.MinSpecial = enforcedPolicyOptions.SpecialCount;
            }

            if(options.MinSpecial + options.MinNumber > options.Length)
            {
                options.MinSpecial = options.Length - options.MinNumber;
            }

            if(options.NumWords == null || options.Length < 3)
            {
                options.NumWords = 3;
            }
            else if(options.NumWords > 20)
            {
                options.NumWords = 20;
            }

            if(options.WordSeparator != null && options.WordSeparator.Length > 1)
            {
                options.WordSeparator = options.WordSeparator[0].ToString();
            }
        }

        // Helpers

        private async Task<List<GeneratedPasswordHistory>> EncryptHistoryAsync(List<GeneratedPasswordHistory> history)
        {
            if(!history?.Any() ?? true)
            {
                return new List<GeneratedPasswordHistory>();
            }
            var tasks = history.Select(async item =>
            {
                if(item == null)
                {
                    return null;
                }
                var encrypted = await _cryptoService.EncryptAsync(item.Password);
                if(encrypted == null)
                {
                    return null;
                }
                return new GeneratedPasswordHistory
                {
                    Password = encrypted.EncryptedString,
                    Date = item.Date
                };
            });
            var h = await Task.WhenAll(tasks);
            return h.Where(x => x != null).ToList();
        }

        private async Task<List<GeneratedPasswordHistory>> DecryptHistoryAsync(List<GeneratedPasswordHistory> history)
        {
            if(!history?.Any() ?? true)
            {
                return new List<GeneratedPasswordHistory>();
            }
            var tasks = history.Select(async item =>
            {
                var decrypted = await _cryptoService.DecryptToUtf8Async(new CipherString(item.Password));
                return new GeneratedPasswordHistory
                {
                    Password = decrypted,
                    Date = item.Date
                };
            });
            var h = await Task.WhenAll(tasks);
            return h.ToList();
        }

        private bool MatchesPrevious(string password, List<GeneratedPasswordHistory> history)
        {
            if(!history?.Any() ?? true)
            {
                return false;
            }
            return history.Last().Password == password;
        }

        private string Capitalize(string str)
        {
            return str.First().ToString().ToUpper() + str.Substring(1);
        }

        private async Task AppendRandomNumberToRandomWordAsync(List<string> wordList)
        {
            if(wordList == null || wordList.Count <= 0)
            {
                return;
            }
            var index = await _cryptoService.RandomNumberAsync(0, wordList.Count - 1);
            var num = await _cryptoService.RandomNumberAsync(0, 9);
            wordList[index] = wordList[index] + num;
        }
    }
}
