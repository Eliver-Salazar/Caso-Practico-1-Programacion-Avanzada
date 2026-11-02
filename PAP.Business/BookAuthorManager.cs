using System;
using System.Collections.Generic;

using PAP.Models;
using PAP.Repositories;

namespace PAP.Business
{
    public class BookAuthorManager
    {
        private readonly BookAuthorRepository _repository =
            new BookAuthorRepository();

        // Lista las asociaciones.
        public IEnumerable<BookAuthorLink> GetAll()
        {
            return _repository.GetLinks();
        }

        // Busca una asociación mediante su clave compuesta.
        public BookAuthorLink GetById(int bookId, int authorId)
        {
            return _repository.GetLink(bookId, authorId);
        }

        // Agrega o edita la asociación usando el repository.
        public void Save(
            int bookId, int authorId,
            int? originalBookId, int? originalAuthorId)
        {
            _repository.SaveLink(
                bookId, authorId, originalBookId, originalAuthorId);
        }

        // Elimina únicamente la asociación.
        public void Delete(int bookId, int authorId)
        {
            _repository.DeleteLink(bookId, authorId);
        }
    }
}
