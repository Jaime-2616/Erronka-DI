using System.Collections.Generic;
using System.Linq;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Data;

namespace TPV_Gastronomico.Services
{
    public class MesaService
    {
        private readonly DatabaseContext _context;

        public MesaService(DatabaseContext context)
        {
            _context = context;
        }

        public List<Mesa> GetMesas()
        {
            return _context.Mesas.ToList();
        }

        public Mesa GetMesaById(int id)
        {
            return _context.Mesas.Find(id);
        }

        public List<Mesa> GetMesasPorCapacidad(int capacidadMinima)
        {
            return _context.Mesas
                .Where(m => m.Capacidad >= capacidadMinima)
                .OrderBy(m => m.Capacidad)
                .ToList();
        }
    }
}