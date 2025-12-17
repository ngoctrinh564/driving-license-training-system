using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? Rolename { get; set; }

    public string? Mota { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
