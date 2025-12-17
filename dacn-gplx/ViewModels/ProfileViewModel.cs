using System;
using System.ComponentModel.DataAnnotations;

namespace dacn_gplx.ViewModels
{
    public class ProfileViewModel
    {
        public int HocvienId { get; set; }

        [Display(Name = "Họ tên")]
        public string? Hoten { get; set; }

        [Display(Name = "Số CMND/CCCD")]
        public string? SoCmndCccd { get; set; }

        [Display(Name = "Năm sinh")]
        [DataType(DataType.Date)]
        public DateOnly? Namsinh { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gioitinh { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? Sdt { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string? Mail { get; set; }
        public int? KhamsuckhoeId { get; set; }


        // Avatar hiện tại (link ảnh)
        public string? AvatarUrl { get; set; }
    }
}
