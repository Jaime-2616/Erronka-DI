using System.Linq;
using System.Windows;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Utils;
using TPV_Gastronomico.Views;

namespace TPV_Gastronomico
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Inicializar la base de datos
            using (var db = new DatabaseContext())
            {
                // Crea la DB si no existe
                db.Database.EnsureCreated();

                // Crear usuario admin si no existe
                if (!db.Users.Any(u => u.Username == "admin"))
                {
                    var admin = new User
                    {
                        Nombre = "Administrador",
                        Username = "admin",
                        PasswordHash = PasswordHasher.Hash("admin123"), // cambiar para entrega
                        Rol = Role.Admin
                    };
                    db.Users.Add(admin);
                    db.SaveChanges();
                }

                // Crear mesas por defecto si no existen
                if (!db.Mesas.Any())
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        db.Mesas.Add(new Mesa
                        {
                            Numero = i,
                            Capacidad = 4
                        });
                    }
                    db.SaveChanges();
                }
            }

            // Abrir ventana de login
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            // IMPORTANTE: No cerrar la aplicación aquí
            // El cierre se manejará en LoginWindow
        }
    }
}