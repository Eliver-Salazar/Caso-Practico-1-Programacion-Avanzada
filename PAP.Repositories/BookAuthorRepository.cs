using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

// Usa exactamente las clases del modelo de Entity Framework.
using Book = PAP.DataAccess.Book;
using Author = PAP.DataAccess.Author;

// Importa solo la clase que representa la asociación en pantalla.
using BookAuthorLink = PAP.Models.BookAuthorLink;

namespace PAP.Repositories
{
    public class BookAuthorRepository : RepositoryBase<Book>
    {
        // Obtiene las asociaciones entre libros y autores.
        public IEnumerable<BookAuthorLink> GetLinks()
        {
            var books = _set.Include(book => book.Authors).ToList();
            var links = new List<BookAuthorLink>();

            // Cada autor asociado al libro representa una fila.
            foreach (var book in books)
            {
                foreach (var author in book.Authors)
                {
                    links.Add(new BookAuthorLink
                    {
                        BookId = book.Id,
                        AuthorId = author.Id,
                        BookTitle = string.IsNullOrWhiteSpace(book.Title)
                            ? book.Name
                            : book.Title,
                        AuthorName = author.FirstName + " " + author.LastName
                    });
                }
            }

            return links;
        }

        // Busca una asociación mediante sus dos identificadores.
        public BookAuthorLink GetLink(int bookId, int authorId)
        {
            return GetLinks().FirstOrDefault(link =>
                link.BookId == bookId && link.AuthorId == authorId);
        }

        // Agrega una asociación o reemplaza una existente.
        public void SaveLink(
            int bookId,
            int authorId,
            int? originalBookId,
            int? originalAuthorId)
        {
            // Recupera ambas entidades desde el mismo contexto.
            var book = _set.Find(bookId);
            var author = _context.Set<Author>().Find(authorId);

            if (book == null || author == null)
                throw new InvalidOperationException(
                    "Libro o autor no encontrado.");

            if (originalBookId.HasValue != originalAuthorId.HasValue)
                throw new InvalidOperationException(
                    "Identificadores originales incompletos.");

            Book originalBook = null;
            Author originalAuthor = null;

            if (originalBookId.HasValue)
            {
                originalBook = _set.Find(originalBookId.Value);

                if (originalBook == null)
                    throw new InvalidOperationException(
                        "Relación original no encontrada.");

                // Carga los autores asociados al libro original.
                _context.Entry(originalBook)
                    .Collection(item => item.Authors).Load();

                originalAuthor = originalBook.Authors.FirstOrDefault(
                    item => item.Id == originalAuthorId.Value);

                if (originalAuthor == null)
                    throw new InvalidOperationException(
                        "Relación original no encontrada.");

                // Si la asociación no cambió, no necesita guardarse.
                if (originalBookId.Value == bookId &&
                    originalAuthorId.Value == authorId)
                {
                    return;
                }
            }

            _context.Entry(book)
                .Collection(item => item.Authors).Load();

            // Impide repetir la misma combinación libro-autor.
            if (book.Authors.Any(item => item.Id == authorId))
                throw new InvalidOperationException(
                    "El autor ya está asociado al libro.");

            // Al editar, retira la asociación anterior.
            if (originalBook != null)
                originalBook.Authors.Remove(originalAuthor);

            // Agrega la nueva asociación.
            book.Authors.Add(author);

            // Usa Save del repository base.
            // Entity Framework modifica la tabla intermedia BookAuthors.
            Save();
        }

        // Elimina solo la relación, conservando libro y autor.
        public void DeleteLink(int bookId, int authorId)
        {
            var book = _set.Find(bookId);

            if (book == null)
                throw new InvalidOperationException("Libro no encontrado.");

            _context.Entry(book)
                .Collection(item => item.Authors).Load();

            var author = book.Authors.FirstOrDefault(
                item => item.Id == authorId);

            if (author == null)
                throw new InvalidOperationException(
                    "Relación no encontrada.");

            book.Authors.Remove(author);
            Save();
        }
    }
}