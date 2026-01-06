namespace Vidora.Core.Entities;

public class MovieMember
{
    // ID của diễn viên/thành viên đoàn làm phim
    public int MemberId { get; set; }

    // Tên (ví dụ: Leonardo DiCaprio)
    public string Name { get; set; } = string.Empty;

    // Vai trò (ví dụ: Actor, Director, Producer)
    public string Role { get; set; } = string.Empty;
}