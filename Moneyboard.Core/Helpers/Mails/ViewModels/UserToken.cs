﻿using System;

namespace Moneyboard.Core.Helpers.Mails.ViewModels
{
    public class UserToken
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public Uri Uri { get; set; }
    }
}
