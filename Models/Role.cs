using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BlazorApp.Models
{
    [Table("roles")]
    public class Role : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("role_name")]
        public string? RoleName { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
