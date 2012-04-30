#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Lokad.Cqrs.AtomicStorage;

namespace SaaS.Client.Projections.LoginView
{
    [DataContract]
    public sealed class LoginView
    {
        [DataMember(Order = 1)]
        public string Display { get; set; }

        [DataMember(Order = 2)]
        public SecurityId Security { get; set; }

        [DataMember(Order = 3)]
        public string Token { get; set; }

        [DataMember(Order = 4)]
        public string PasswordHash { get; set; }

        [DataMember(Order = 5)]
        public string PasswordSalt { get; set; }

        [DataMember(Order = 6)]
        public string Key { get; set; }

        [DataMember(Order = 7)]
        public DateTime LockedOutTillUtc { get; set; }

        [DataMember(Order = 8)]
        public string LockoutMessage { get; set; }

        [DataMember(Order = 9)]
        public LoginViewType Type { get; set; }

        [DataMember(Order = 10)]
        public string Identity { get; set; }

        [DataMember(Order = 11)]
        public TimeSpan LoginTrackingThreshold { get; set; }

        [DataMember(Order = 12)]
        public DateTime LastLoginUtc { get; set; }

        [DataMember(Order = 13)]
        public IList<string> Permissions { get; set; }

        public LoginView()
        {
            Permissions = new List<string>(0);
        }
    }

    public enum LoginViewType
    {
        Undefined,
        Key,
        Password,
        Identity
    }

    public sealed class LoginViewProjection
    {
        readonly IDocumentWriter<UserId, LoginView> _writer;

        public LoginViewProjection(IDocumentWriter<UserId, LoginView> writer)
        {
            _writer = writer;
        }

        static TimeSpan DefaultThreshold = TimeSpan.FromMinutes(5);

        public void When(SecurityPasswordAdded e)
        {
            _writer.Add(e.UserId, new LoginView
            {
                Security = e.Id,
                Display = e.DisplayName,
                Token = e.Token,
                PasswordHash = e.PasswordHash,
                PasswordSalt = e.PasswordSalt,
                Type = LoginViewType.Password,
                LoginTrackingThreshold = DefaultThreshold
            });
        }

        public void When(SecurityIdentityAdded e)
        {
            _writer.Add(e.UserId, new LoginView
            {
                Security = e.Id,
                Display = e.DisplayName,
                Token = e.Token,
                Identity = e.Identity,
                Type = LoginViewType.Identity,
                LoginTrackingThreshold = DefaultThreshold
            });
        }

        
        public void When(UserLocked e)
        {
            _writer.UpdateOrThrow(e.Id, lv =>
            {
                lv.LockedOutTillUtc = e.LockedTillUtc;
                lv.LockoutMessage = e.LockReason;
            });
        }
        public void When(UserUnlocked e)
        {
            _writer.UpdateOrThrow(e.Id, lv =>
            {
                lv.LockedOutTillUtc = DateTime.MinValue;
                lv.LockoutMessage = null;
            });
        }

        public void When(UserCreated e)
        {
            _writer.UpdateOrThrow(e.Id, lv => { lv.LoginTrackingThreshold = e.ActivityThreshold; });
        }

        public void When(UserLoginSuccessReported e)
        {
            _writer.UpdateOrThrow(e.Id, lv => { lv.LastLoginUtc = e.TimeUtc; });
        }
        public void When(SecurityItemDisplayNameUpdated e)
        {
            _writer.UpdateOrThrow(e.UserId, lv => lv.Display = e.DisplayName);
        }
        public void When(SecurityItemRemoved e)
        {
            _writer.TryDelete(e.UserId);
        }

        public void When(PermissionAddedToSecurityItem e)
        {
            _writer.UpdateOrThrow(e.UserId, lv => lv.Permissions.Add(e.Permission));
        }
    }

}