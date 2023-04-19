using Q3C273.Shared.Enums;
using System;
using System.Security.Principal;

namespace Ton618.User
{
    public class UserAccount
    {
        public string UserName { get; }

        public AccountType Type { get; }

        public UserAccount()
        {
            UserName = Environment.UserName;
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);

                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                    Type = AccountType.Admin;
                else if (principal.IsInRole(WindowsBuiltInRole.User))
                {
                    Type = AccountType.User;
                }
                else if (principal.IsInRole(WindowsBuiltInRole.Guest))
                {
                    Type = AccountType.Guest;
                }
                else
                {
                    Type = AccountType.Unknown;
                }
            }
        }
    }
}
