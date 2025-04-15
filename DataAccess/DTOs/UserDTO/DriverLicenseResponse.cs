using DataAccess.Models;
using System;

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
        if (license == null) throw new ArgumentNullException(nameof(license));
        if (license.User == null) throw new ArgumentNullException(nameof(license.User));
        try
        {
            var lines = qrResult.Split(new[] { "\r\n" }, StringSplitOptions.None);
            LicenseNumber = license.Code ?? throw new ArgumentNullException(nameof(license.Code));
            Class = license.Class ?? throw new ArgumentNullException(nameof(license.Class));
            DateOfBirth = lines.Length > 2 ? lines[2] : "N/A";
            FullName = license.User.Fullname ?? throw new ArgumentNullException(nameof(license.User.Fullname));
            Address = lines.Length > 4 ? lines[4] : "N/A";
            FrontImageUrl = license.ImgFront ?? throw new ArgumentNullException(nameof(license.ImgFront));
            BackImageUrl = license.ImgBack ?? throw new ArgumentNullException(nameof(license.ImgBack));
            Status = license.Status ?? throw new ArgumentNullException(nameof(license.Status));
            CreatedAt = license.CreateAt;
            UpdatedAt = license.UpdateAt;
        }
        catch (Exception ex)
        {
            throw new Exception("Error processing license data", ex);
        }
    }
}
