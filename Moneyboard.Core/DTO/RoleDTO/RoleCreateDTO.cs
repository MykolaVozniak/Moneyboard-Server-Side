﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moneyboard.Core.DTO.RoleDTO
{
    public class RoleCreateDTO
    {
        public string RoleName { get; set; }
        public int RolePoints { get; set; }
        public int ProjectId { get; set; }
    }
}
