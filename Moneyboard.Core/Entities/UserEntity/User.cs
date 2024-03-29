﻿using Microsoft.AspNetCore.Identity;
using Moneyboard.Core.Entities.RefreshTokenEntity;
using Moneyboard.Core.Entities.UserProjectEntity;
using Moneyboard.Core.Interfaces;

namespace Moneyboard.Core.Entities.UserEntity
{
    public class User : IdentityUser, IBaseEntity
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string ImageUrl { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime CreateDate { get; set; }
        public string CardNumber { get; set; }

        public ICollection<UserProject> UserProjects { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }

        
    }
}
