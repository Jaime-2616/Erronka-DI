using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Data;

namespace TPV_Gastronomico.Services
{
    public class ReservaService
    {
        private readonly DatabaseContext _context;

        public ReservaService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Reserva> GetReservas()
        {
            return _context.Reservas
                .Include(r => r.Mesa)
                .Include(r => r.Usuario)
                .ToList();
        }

        public List<Reserva> GetReservasByUsuario(int usuarioId)
        {
            return _context.Reservas
                .Where(r => r.UsuarioId == usuarioId)
                .Include(r => r.Mesa)
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.Fecha)
                .ToList();
        }

        public List<Reserva> GetAllReservas()
        {
            return _context.Reservas
                .Include(r => r.Mesa)
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.Fecha)
                .ToList();
        }

        public bool CrearReserva(Reserva reserva)
        {
            try
            {
                // Verificar si la mesa está disponible para esa fecha y tipo de comida
                // Ahora se compara la fecha completa (fecha + hora + minuto)
                var existeReserva = _context.Reservas
                    .Any(r => r.MesaId == reserva.MesaId &&
                             r.Fecha == reserva.Fecha &&
                             r.Tipo == reserva.Tipo);
                // r.Estado != "Cancelada"); // Comentado temporalmente

                if (existeReserva)
                {
                    MessageBox.Show("La mesa ya está reservada para esa fecha y tipo de comida.");
                    return false;
                }

                _context.Reservas.Add(reserva);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear la reserva: {ex.Message}");
                return false;
            }
        }

        public bool CancelarReserva(int reservaId)
        {
            try
            {
                var reserva = _context.Reservas.Find(reservaId);
                if (reserva != null)
                {
                    // TEMPORAL: Eliminamos en lugar de cambiar estado
                    _context.Reservas.Remove(reserva);
                    // reserva.Estado = "Cancelada"; // Comentado temporalmente
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cancelar la reserva: {ex.Message}");
                return false;
            }
        }

        public List<Mesa> GetMesasDisponibles(DateTime fecha, TipoComida tipoComida, int numPersonas)
        {
            // Ahora consideramos la fecha completa (fecha + hora + minuto) para determinar mesas ocupadas
            var mesasOcupadas = _context.Reservas
                .Where(r => r.Fecha == fecha &&
                           r.Tipo == tipoComida)
                // r.Estado != "Cancelada") // Comentado temporalmente
                .Select(r => r.MesaId)
                .ToList();

            return _context.Mesas
                .Where(m => !mesasOcupadas.Contains(m.Id) && m.Capacidad >= numPersonas)
                .OrderBy(m => m.Capacidad)
                .ToList();
        }

        public bool ModificarReserva(Reserva reservaActualizada)
        {
            try
            {
                var reservaExistente = _context.Reservas.Find(reservaActualizada.Id);
                if (reservaExistente != null)
                {
                    // Verificar disponibilidad si se cambió mesa, fecha u tipo
                    if (reservaExistente.MesaId != reservaActualizada.MesaId ||
                        reservaExistente.Fecha != reservaActualizada.Fecha ||
                        reservaExistente.Tipo != reservaActualizada.Tipo)
                    {
                        var existeOtraReserva = _context.Reservas
                            .Any(r => r.Id != reservaActualizada.Id &&
                                     r.MesaId == reservaActualizada.MesaId &&
                                     r.Fecha == reservaActualizada.Fecha &&
                                     r.Tipo == reservaActualizada.Tipo);
                        // r.Estado != "Cancelada"); // Comentado temporalmente

                        if (existeOtraReserva)
                        {
                            MessageBox.Show("La mesa ya está reservada para esa fecha y tipo de comida.");
                            return false;
                        }
                    }

                    // Actualizar propiedades (sin Estado por ahora)
                    reservaExistente.Fecha = reservaActualizada.Fecha;
                    reservaExistente.Tipo = reservaActualizada.Tipo;
                    reservaExistente.MesaId = reservaActualizada.MesaId;
                    reservaExistente.NumPersonas = reservaActualizada.NumPersonas;
                    // reservaExistente.Estado = reservaActualizada.Estado; // Comentado temporalmente

                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al modificar la reserva: {ex.Message}");
                return false;
            }
        }

        // Método adicional para obtener los tipos de comida disponibles
        public List<string> GetTiposComida()
        {
            return Enum.GetNames(typeof(TipoComida)).ToList();
        }
    }
}