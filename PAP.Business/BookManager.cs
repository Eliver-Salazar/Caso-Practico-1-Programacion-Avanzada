using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PAP.Business
{
    public class BookManager
    {
        private readonly BookRepository _repository;

        // Inicializa el repository utilizado por este manager.
        public BookManager()
        {
            _repository = new BookRepository();
        }

        // Conserva el método original utilizado por los otros módulos.
        public IEnumerable<Book> GetAllBooksFromDataModel()
        {
            return _repository.GetAll();
        }

        // Busca un libro por su identificador.
        public Book GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Comprueba ISBN duplicados excluyendo el libro que se está editando.
        // El ISBN es opcional; varios libros pueden tenerlo vacío.
        public bool ISBNExists(string isbn, int id)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return false;

            string normalizedISBN = isbn.Trim();

            return _repository.GetAll().Any(book =>
                book.Id != id &&
                string.Equals(
                    book.ISBN,
                    normalizedISBN,
                    StringComparison.OrdinalIgnoreCase));
        }

        // Agrega un libro mediante el método heredado del repository base.
        public void Add(Book book)
        {
            _repository.Add(book);
        }

        // Actualiza un libro mediante el repository base.
        public void Update(Book book)
        {
            _repository.Update(book);
        }

        // Elimina un libro mediante el repository base.
        // La base de datos protege las relaciones con otras tablas.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}