using ERP_SMART_DT_Tuan_Anh.Data;
using ERP_SMART_DT_Tuan_Anh.DTOs;
using ERP_SMART_DT_Tuan_Anh.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERP_SMART_DT_Tuan_Anh.Services;

public class AuthService
{
    public async Task<LoginResultDto> LoginAsync(string username, string password)
    {
        await using var db = DbContextFactory.Create();

        var user = await db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == username && !x.IsDeleted);

        if (user == null)
            return new LoginResultDto { IsSuccess = false, Message = "Tài khoản không tồn tại." };

        if (!user.IsActive)
            return new LoginResultDto { IsSuccess = false, Message = "Tài khoản đã bị khóa." };

        if (!PasswordHasher.Verify(password, user.PasswordHash))
            return new LoginResultDto { IsSuccess = false, Message = "Mật khẩu không đúng." };

        user.LastLogin = DateTime.Now;
        await db.SaveChangesAsync();

        return new LoginResultDto
        {
            IsSuccess = true,
            Message = "Đăng nhập thành công.",
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName ?? user.Username,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName ?? string.Empty
        };
    }
}