using System;
using System.ComponentModel.DataAnnotations;

namespace dacn_gplx.ViewModels
{
    public class RegisterViewModel
    {
        // =========================
        // TÀI KHOẢN
        // =========================
        [Required(ErrorMessage = "Username là bắt buộc")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không trùng khớp")]
        public string ConfirmPassword { get; set; }

        // =========================
        // THÔNG TIN HỌC VIÊN
        // =========================
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        public string Hoten { get; set; }

        [Required(ErrorMessage = "CCCD/CMND là bắt buộc")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "CCCD/CMND phải đủ 12 số")]
        public string SoCmndCccd { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Sdt { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Mail { get; set; }

        // =========================
        // NĂM SINH (18 - < 100 TUỔI)
        // =========================
        [Required(ErrorMessage = "Năm sinh là bắt buộc")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(RegisterViewModel), nameof(ValidateAge))]
        public DateOnly? Namsinh { get; set; }

        // =========================
        // VALIDATE TUỔI
        // =========================
        public static ValidationResult ValidateAge(DateOnly? namsinh, ValidationContext context)
        {
            if (!namsinh.HasValue)
                return new ValidationResult("Năm sinh là bắt buộc");

            var today = DateOnly.FromDateTime(DateTime.Today);

            int age = today.Year - namsinh.Value.Year;

            if (namsinh.Value > today.AddYears(-age))
                age--;

            if (age < 18)
                return new ValidationResult("Học viên phải đủ 18 tuổi trở lên");

            if (age >= 100)
                return new ValidationResult("Tuổi phải nhỏ hơn 100");

            return ValidationResult.Success;
        }
    }
}
