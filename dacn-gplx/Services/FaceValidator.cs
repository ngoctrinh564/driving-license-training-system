using OpenCvSharp;

namespace dacn_gplx.Services
{
    public class FaceValidator
    {
        private readonly IWebHostEnvironment _env;

        public FaceValidator(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Kiểm tra ảnh có phải ảnh thẻ hay không
        /// </summary>
        public bool ValidateFace(string imagePath, out string message)
        {
            message = "";

            try
            {
                // Tìm file HaarCascade
                string cascadePath = Path.Combine(
                    _env.WebRootPath,
                    "haarcascade",
                    "haarcascade_frontalface_default.xml"
                );

                if (!System.IO.File.Exists(cascadePath))
                {
                    message = "Không tìm thấy file nhận diện khuôn mặt (haarcascade).";
                    return false;
                }

                // Load ảnh
                Mat img = Cv2.ImRead(imagePath);
                if (img.Empty())
                {
                    message = "Không đọc được ảnh.";
                    return false;
                }

                // Convert sang grayscale
                Mat gray = new Mat();
                Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.EqualizeHist(gray, gray);

                // Tải model HaarCascade
                var faceDetector = new CascadeClassifier(cascadePath);

                Rect[] faces = faceDetector.DetectMultiScale(
                    gray,
                    scaleFactor: 1.1,
                    minNeighbors: 5,
                    minSize: new Size(80, 80)
                );

                if (faces.Length == 0)
                {
                    message = "Không phát hiện khuôn mặt. Hãy dùng ảnh thẻ chính diện.";
                    return false;
                }

                if (faces.Length > 1)
                {
                    message = "Ảnh chứa nhiều người. Vui lòng chọn ảnh chỉ có 1 khuôn mặt.";
                    return false;
                }

                // Kiểm tra độ sáng
                double brightness = Cv2.Mean(gray).Val0;
                if (brightness < 60)
                {
                    message = "Ảnh quá tối, vui lòng chọn ảnh sáng hơn.";
                    return false;
                }

                message = "OK";
                return true;
            }
            catch (Exception ex)
            {
                message = "Lỗi xử lý ảnh: " + ex.Message;
                return false;
            }
        }
    }
}
