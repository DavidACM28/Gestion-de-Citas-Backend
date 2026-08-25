namespace Gestion.Citas.DataAccess.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = Environment.UserName;

        public BaseEntity()
        {
            Active = true;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
