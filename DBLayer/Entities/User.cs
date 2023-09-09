using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBLayer.Entities
{
    public class User:IdentityUser<int>
    {
        public string? connectionId { get; set; }
    }
}
