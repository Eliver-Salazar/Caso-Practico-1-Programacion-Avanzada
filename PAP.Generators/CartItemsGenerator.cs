using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PAP.DataAccess;
using PAP.Repositories;

namespace PAP.Generators
{
    public class CartItemsGenerator : IGenerator
    {
        // Comprueba la tabla y las dependencias necesarias.
        public GeneratorStatus GetStatus()
        {
            if (new CartItemRepository().GetAll().Any())
            {
                return new GeneratorStatus
                {
                    EntityName = "CartItems",
                    CanGenerate = false,
                    Message = "La tabla ya contiene datos."
                };
            }

            var missing = new List<string>();

            if (!new ShoppingCartRepository().GetAll().Any())
                missing.Add("ShoppingCarts");

            if (!new BookRepository().GetAll().Any())
                missing.Add("Books");

            return new GeneratorStatus
            {
                EntityName = "CartItems",
                CanGenerate = missing.Count == 0,
                Message = missing.Count == 0
                    ? "Disponible para generar."
                    : "Primero agregá o generá: " +
                      string.Join(", ", missing) + "."
            };
        }

        // Genera un artículo por cada uno de los primeros diez carritos.
        public int Generate()
        {
            var repository = new CartItemRepository();

            if (repository.GetAll().Any())
            {
                throw new InvalidOperationException(
                    "No se pueden generar artículos del carrito: " +
                    "la tabla ya contiene datos.");
            }

            var carts = new ShoppingCartRepository()
                .GetAll()
                .OrderBy(cart => cart.Id)
                .Take(10)
                .ToList();

            var books = new BookRepository()
                .GetAll()
                .OrderBy(book => book.Id)
                .Take(10)
                .ToList();

            if (!carts.Any() || !books.Any())
            {
                throw new InvalidOperationException(
                    "Primero agregá o generá ShoppingCarts y Books.");
            }

            for (int i = 0; i < carts.Count; i++)
            {
                var item = new CartItem
                {
                    // Cada carrito aparece una vez para evitar pares repetidos.
                    CartId = carts[i].Id,
                    BookId = books[i % books.Count].Id,
                    Quantity = i % 3 + 1,
                    AddedAt = DateTime.UtcNow
                };

                repository.Add(item);
            }

            return carts.Count;
        }
    }
}
