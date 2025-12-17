public class ForgotPasswordViewModel
{
    public string Username { get; set; }

    // Thêm các trường mới
    public string Otp { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}
