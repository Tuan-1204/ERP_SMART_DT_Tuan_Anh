namespace ERP_SMART_DT_Tuan_Anh.DTOs;

public class LoginResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}