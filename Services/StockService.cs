using System;
using System.Collections.Generic;
using System.Linq;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Models;

namespace TPV_Gastronomico.Services
{
    public class StockService
    {
        public IEnumerable<Producto> GetAll()
        {
            using var db = new DatabaseContext();
            return db.Productos.OrderBy(p => p.Nombre).ToList();
        }

        public Producto GetById(int id)
        {
            using var db = new DatabaseContext();
            return db.Productos.SingleOrDefault(p => p.Id == id);
        }

        public IEnumerable<Producto> Search(string term)
        {
            using var db = new DatabaseContext();
            if (string.IsNullOrWhiteSpace(term))
                return db.Productos.OrderBy(p => p.Nombre).ToList();

            term = term.Trim().ToLower();
            return db.Productos
                     .Where(p => p.Nombre.ToLower().Contains(term) || (p.Categoria != null && p.Categoria.ToLower().Contains(term)))
                     .OrderBy(p => p.Nombre)
                     .ToList();
        }

        public void Add(Producto producto)
        {
            if (producto == null) throw new ArgumentNullException(nameof(producto));
            if (string.IsNullOrWhiteSpace(producto.Nombre)) throw new ArgumentException("El nombre es obligatorio.");
            if (producto.Precio < 0) throw new ArgumentException("El precio no puede ser negativo.");
            if (producto.Cantidad < 0) throw new ArgumentException("La cantidad no puede ser negativa.");

            using var db = new DatabaseContext();
            db.Productos.Add(producto);
            db.SaveChanges();
        }

        public void Update(Producto producto)
        {
            if (producto == null) throw new ArgumentNullException(nameof(producto));
            if (producto.Id <= 0) throw new ArgumentException("Producto inválido.");
            if (string.IsNullOrWhiteSpace(producto.Nombre)) throw new ArgumentException("El nombre es obligatorio.");
            if (producto.Precio < 0) throw new ArgumentException("El precio no puede ser negativo.");
            if (producto.Cantidad < 0) throw new ArgumentException("La cantidad no puede ser negativa.");

            using var db = new DatabaseContext();
            var existing = db.Productos.SingleOrDefault(p => p.Id == producto.Id);
            if (existing == null) throw new InvalidOperationException("Producto no encontrado.");

            existing.Nombre = producto.Nombre;
            existing.Categoria = producto.Categoria;
            existing.Precio = producto.Precio;
            existing.Cantidad = producto.Cantidad;

            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new DatabaseContext();
            var existing = db.Productos.SingleOrDefault(p => p.Id == id);
            if (existing == null) throw new InvalidOperationException("Producto no encontrado.");

            // Si quieres prevenir borrado cuando hay referencias, EF/DB lo controlará por restricciones.
            db.Productos.Remove(existing);
            db.SaveChanges();
        }

        public IEnumerable<Producto> GetLowStock(int threshold = 5)
        {
            using var db = new DatabaseContext();
            return db.Productos.Where(p => p.Cantidad <= threshold).OrderBy(p => p.Cantidad).ToList();
        }
    }
}
