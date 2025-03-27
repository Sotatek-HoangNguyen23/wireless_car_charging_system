using DataAccess.Models;

public class DriverLicenseResponse
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string FrontImageUrl { get; set; } = string.Empty;
    public string BackImageUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DriverLicenseResponse(DriverLicense license, string qrResult)
    {
        try
        {
            var lines = qrResult.Split(new[] { "\r\n" }, StringSplitOptions.None);
            LicenseNumber = lines.Length > 0 && !string.IsNullOrEmpty(lines[0]) ? lines[0] : license.Code;
            FullName = lines.Length > 1 ? lines[1] : "N/A";
            DateOfBirth = lines.Length > 2 ? lines[2] : "N/A";
            Class = lines.Length > 3 && !string.IsNullOrEmpty(lines[3]) ? lines[3] : license.Class;
            Address = lines.Length > 4 ? lines[4] : "N/A";
        }
        catch
        {
            LicenseNumber = license.Code;
            Class = license.Class;
            FullName = "N/A";
            DateOfBirth = "N/A";
            Address = "N/A";
        }

        FrontImageUrl = license.ImgFront;
        BackImageUrl = license.ImgBack;
        Status = license.Status;
        CreatedAt = license.CreateAt;
        UpdatedAt = license.UpdateAt;
    }
}
