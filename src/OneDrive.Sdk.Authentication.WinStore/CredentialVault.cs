﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.OneDrive.Sdk.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Windows.Security.Credentials;

    public class CredentialVault : ICredentialVault
    {
        private const string VaultResourceName = "OneDriveSDK_AuthAdapter";

        private string ClientId { get; set; }

        public CredentialVault(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("You must provide a clientId");
            }

            this.ClientId = clientId;
        }

        public void AddCredentialCacheToVault(CredentialCache credentialCache)
        {
            this.DeleteStoredCredentialCache();

            var vault = new PasswordVault();
            var cred = new PasswordCredential(
                CredentialVault.VaultResourceName,
                this.ClientId,
                Convert.ToBase64String(credentialCache.GetCacheBlob()));
            vault.Add(cred);
        }

        public bool RetrieveCredentialCache(CredentialCache credentialCache)
        {
            var creds = this.RetrieveCredentialFromVault();

            if (creds != null)
            {
                credentialCache.InitializeCacheFromBlob(Convert.FromBase64String(creds.Password));
                return true;
            }

            return false;
        }

        public bool DeleteStoredCredentialCache()
        {
            var creds = this.RetrieveCredentialFromVault();

            if (creds != null)
            {
                var vault = new PasswordVault();
                vault.Remove(creds);
                return true;
            }

            return false;
        }

        private PasswordCredential RetrieveCredentialFromVault()
        {
            var vault = new PasswordVault();
            PasswordCredential creds = null;

            try
            {
                creds = vault.Retrieve(CredentialVault.VaultResourceName, this.ClientId);
            }
            catch (Exception)
            {
                // This happens when the vault is empty. Swallow.
            }

            return creds;
        }
    }
}
