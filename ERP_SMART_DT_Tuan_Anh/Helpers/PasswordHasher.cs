namespace ERP_SMART_DT_Tuan_Anh.Helpers;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return string.Empty;

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
       
            return password == passwordHash;
        }
    }
}