using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext _context;

    public HabitacionRepositorio(HotelContext context)
    {
        _context = context;
    }

    public async Task<List<Habitacion>> ListarActivasAsync(int? tipoHabitacionId)
    {
        var consulta = _context.Habitacions
            .AsNoTracking()
            .Include(h => h.TipoHabitacion)
            .Where(h => h.Activo);

        if (tipoHabitacionId is > 0)
            consulta = consulta.Where(h => h.TipoHabitacionId == tipoHabitacionId);

        return await consulta
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public Task<Habitacion?> ObtenerPorIdAsync(int id)
    {
        return _context.Habitacions
            .AsNoTracking()
            .Include(h => h.TipoHabitacion)
            .FirstOrDefaultAsync(h => h.HabitacionId == id);
    }

    public Task<bool> ExisteNumeroAsync(int numero, int? exceptoId = null)
    {
        return _context.Habitacions
            .AsNoTracking()
            .AnyAsync(h => h.Numero == numero && (exceptoId == null || h.HabitacionId != exceptoId));
    }

    public Task<List<TipoHabitacion>> ListarTiposAsync()
    {
        return _context.TipoHabitacions
            .AsNoTracking()
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task CrearAsync(Habitacion habitacion)
    {
        habitacion.HabitacionId = 0;
        habitacion.Activo = true;
        _context.Habitacions.Add(habitacion);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ActualizarAsync(Habitacion habitacion)
    {
        var actual = await _context.Habitacions
            .FirstOrDefaultAsync(h => h.HabitacionId == habitacion.HabitacionId && h.Activo);

        if (actual is null)
            return false;

        actual.Numero = habitacion.Numero;
        actual.TipoHabitacionId = habitacion.TipoHabitacionId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarLogicoAsync(int id)
    {
        var habitacion = await _context.Habitacions
            .FirstOrDefaultAsync(h => h.HabitacionId == id && h.Activo);

        if (habitacion is null)
            return false;

        habitacion.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }
}
