using System;

namespace PAP.Models
{
    // Ejemplo: Introducción a modelos
    // Un modelo representa los datos del dominio (aquí, un libro) que fluyen entre
    // controlador, negocio y vistas. Las propiedades se enlazan con formularios y vistas tipadas.
    public class Book : Entity
    {
        private string _internalCode;

        public Book()
        {
            _internalCode = Guid.NewGuid().ToString();
        }

        public string InternalCode { get { return _internalCode; } }
        public string Category { get; set; }
        public string Publisher { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public DateTime PublicationDate { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public decimal Price { get; set; }
        public bool IsBestSeller { get; set; }
        public string Genre { get; set; }
        public bool IsInStock { get; set; }
    }
}
