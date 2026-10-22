using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext _context;

    public ReservaRepositorio(HotelContext context)
    {
        _context = context;
    }

    public Task<List<Habitacion>> ListarDisponiblesAsync(DateOnly ingreso, DateOnly salida)
    {
        return _context.Habitacions
            .AsNoTracking()
            .Include(h => h.TipoHabitacion)
            .Where(h => h.Activo)
            .Where(h => !h.Reservas.Any(r =>
                r.Estado != "Cancelada"
                && r.FechaIngreso < salida
                && r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public Task<List<Reserva>> ListarAsync()
    {
        return _context.Reservas
            .AsNoTracking()
            .Include(r => r.Huesped)
            .Include(r => r.Habitacion)
                .ThenInclude(h => h.TipoHabitacion)
            .OrderByDescending(r => r.FechaIngreso)
            .ThenBy(r => r.Habitacion.Numero)
            .ToListAsync();
    }

    public Task<List<Huesped>> ListarHuespedesActivosAsync()
    {
        return _context.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellidos)
            .ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    public Task<List<Habitacion>> ListarHabitacionesActivasAsync()
    {
        return _context.Habitacions
            .AsNoTracking()
            .Include(h => h.TipoHabitacion)
            .Where(h => h.Activo)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public Task<bool> HuespedActivoAsync(int huespedId)
    {
        return _context.Huespeds
            .AsNoTracking()
            .AnyAsync(h => h.HuespedId == huespedId && h.Activo);
    }

    public Task<bool> HabitacionActivaAsync(int habitacionId)
    {
        return _context.Habitacions
            .AsNoTracking()
            .AnyAsync(h => h.HabitacionId == habitacionId && h.Activo);
    }

    public Task<bool> EstaOcupadaAsync(int habitacionId, DateOnly ingreso, DateOnly salida)
    {
        return _context.Reservas
            .AsNoTracking()
            .AnyAsync(r =>
                r.HabitacionId == habitacionId
                && r.Estado != "Cancelada"
                && r.FechaIngreso < salida
                && r.FechaSalida > ingreso);
    }

    public async Task CrearAsync(Reserva reserva)
    {
        reserva.ReservaId = 0;
        reserva.Estado = "Pendiente";
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await _context.Reservas.FirstOrDefaultAsync(r =>
            r.ReservaId == id && (r.Estado == "Pendiente" || r.Estado == "Confirmada"));

        if (reserva is null)
            return false;

        reserva.Estado = "Cancelada";
        await _context.SaveChangesAsync();
        return true;
    }
}
