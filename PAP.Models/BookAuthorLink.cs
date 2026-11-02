using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAP.Models
{
    // Representa la relación entre un libro y un autor.
    public class BookAuthorLink
    {
        public int BookId { get; set; }
        public int AuthorId { get; set; }

        // Se utilizan para mostrar nombres en la pantalla.
        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
    }
}
