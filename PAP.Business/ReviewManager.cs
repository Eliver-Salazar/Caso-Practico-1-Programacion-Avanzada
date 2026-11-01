using System;
using System.Collections.Generic;
using System.Linq;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Business
{
    public class ReviewManager
    {
        private readonly ReviewRepository _repository =
            new ReviewRepository();

        // Lista todas las reseñas.
        public IEnumerable<Review> GetAll()
        {
            return _repository.GetAll();
        }

        // Busca una reseña por su ID.
        public Review GetById(int id)
        {
            return _repository.GetById(id);
        }

        // Cada cliente puede registrar una sola reseña por libro.
        public bool ReviewExists(int bookId, int customerId, int id)
        {
            return _repository.GetAll().Any(review =>
                review.BookId == bookId &&
                review.CustomerId == customerId &&
                review.Id != id);
        }

        // Guarda una reseña nueva.
        public void Add(Review review)
        {
            _repository.Add(review);
        }

        // Actualiza una reseña existente.
        public void Update(Review review)
        {
            _repository.Update(review);
        }

        // Elimina una reseña.
        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}