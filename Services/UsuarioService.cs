using System;
using System.Collections.Generic;
using System.Linq;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Utils;

namespace TPV_Gastronomico.Services
{
    public class UsuarioService
    {
        public IEnumerable<User> GetAll()
        {
            using var db = new DatabaseContext();
            return db.Users.OrderBy(u => u.Nombre).ToList();
        }

        public User GetById(int id)
        {
            using var db = new DatabaseContext();
            return db.Users.SingleOrDefault(u => u.Id == id);
        }

        public void Add(User user, string plainPassword)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentException("Username obligatorio.");
            if (string.IsNullOrWhiteSpace(user.Nombre)) throw new ArgumentException("Nombre obligatorio.");
            if (string.IsNullOrWhiteSpace(plainPassword)) throw new ArgumentException("Contraseña obligatoria.");

            using var db = new DatabaseContext();

            // Username único
            if (db.Users.Any(u => u.Username == user.Username))
                throw new InvalidOperationException("El nombre de usuario ya existe.");

            user.PasswordHash = PasswordHasher.Hash(plainPassword);
            db.Users.Add(user);
            db.SaveChanges();
        }

        public void Update(User user, string newPlainPassword = null)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user.Id <= 0) throw new ArgumentException("Usuario inválido.");
            if (string.IsNullOrWhiteSpace(user.Username)) throw new ArgumentException("Username obligatorio.");
            if (string.IsNullOrWhiteSpace(user.Nombre)) throw new ArgumentException("Nombre obligatorio.");

            using var db = new DatabaseContext();
            var existing = db.Users.SingleOrDefault(u => u.Id == user.Id);
            if (existing == null) throw new InvalidOperationException("Usuario no encontrado.");

            // Comprobar que username no se duplique con otro distinto
            if (db.Users.Any(u => u.Username == user.Username && u.Id != user.Id))
                throw new InvalidOperationException("El nombre de usuario ya está en uso por otro usuario.");

            existing.Nombre = user.Nombre;
            existing.Username = user.Username;
            existing.Rol = user.Rol;

            if (!string.IsNullOrWhiteSpace(newPlainPassword))
            {
                existing.PasswordHash = PasswordHasher.Hash(newPlainPassword);
            }

            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new DatabaseContext();
            var existing = db.Users.SingleOrDefault(u => u.Id == id);
            if (existing == null) throw new InvalidOperationException("Usuario no encontrado.");

            // Prevenir borrar último admin
            if (existing.Rol == Role.Admin)
            {
                var adminCount = db.Users.Count(u => u.Rol == Role.Admin);
                if (adminCount <= 1)
                    throw new InvalidOperationException("No se puede eliminar: debe existir al menos un administrador.");
            }

            db.Users.Remove(existing);
            db.SaveChanges();
        }
    }
}
