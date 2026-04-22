namespace APIEccomerce.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }  // 👈 EF detecta esto como PK automáticamente
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}