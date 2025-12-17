using System;
using System.Collections.Generic;

namespace dacn_gplx.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? RoleId { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool Isactive { get; set; }

    public virtual ICollection<HocVien> HocViens { get; set; } = new List<HocVien>();

    public virtual Role? Role { get; set; }
}
