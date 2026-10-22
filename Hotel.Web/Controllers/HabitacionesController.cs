using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio _habitaciones;

    public HabitacionesController(HabitacionRepositorio habitaciones)
    {
        _habitaciones = habitaciones;
    }

    public async Task<IActionResult> Index(int? tipoHabitacionId)
    {
        await CargarTiposFiltroAsync(tipoHabitacionId);
        var habitaciones = await _habitaciones.ListarActivasAsync(tipoHabitacionId);
        return View(habitaciones);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var habitacion = await _habitaciones.ObtenerPorIdAsync(id.Value);
        if (habitacion is null)
            return NotFound();

        return View(habitacion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new Habitacion());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        ModelState.Remove(nameof(habitacion.TipoHabitacion));
        ModelState.Remove(nameof(habitacion.Reservas));
        habitacion.Activo = true;

        if (habitacion.TipoHabitacionId <= 0)
            ModelState.AddModelError(nameof(habitacion.TipoHabitacionId), "Seleccione un tipo de habitación.");

        if (habitacion.Numero <= 0)
            ModelState.AddModelError(nameof(habitacion.Numero), "Indique un número de habitación mayor que cero.");
        else if (await _habitaciones.ExisteNumeroAsync(habitacion.Numero))
            ModelState.AddModelError(nameof(habitacion.Numero), "Ya existe una habitación con ese número.");

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.TipoHabitacionId);
            return View(habitacion);
        }

        await _habitaciones.CrearAsync(habitacion);
        TempData["Mensaje"] = "La habitación se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var habitacion = await _habitaciones.ObtenerPorIdAsync(id.Value);
        if (habitacion is null || !habitacion.Activo)
            return NotFound();

        await CargarTiposAsync(habitacion.TipoHabitacionId);
        return View(habitacion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habitacion habitacion)
    {
        if (id != habitacion.HabitacionId)
            return NotFound();

        ModelState.Remove(nameof(habitacion.TipoHabitacion));
        ModelState.Remove(nameof(habitacion.Reservas));

        if (habitacion.TipoHabitacionId <= 0)
            ModelState.AddModelError(nameof(habitacion.TipoHabitacionId), "Seleccione un tipo de habitación.");

        if (habitacion.Numero <= 0)
            ModelState.AddModelError(nameof(habitacion.Numero), "Indique un número de habitación mayor que cero.");
        else if (await _habitaciones.ExisteNumeroAsync(habitacion.Numero, habitacion.HabitacionId))
            ModelState.AddModelError(nameof(habitacion.Numero), "Ya existe una habitación con ese número.");

        if (!ModelState.IsValid)
        {
            await CargarTiposAsync(habitacion.TipoHabitacionId);
            return View(habitacion);
        }

        var actualizado = await _habitaciones.ActualizarAsync(habitacion);
        if (!actualizado)
            return NotFound();

        TempData["Mensaje"] = "La habitación se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var habitacion = await _habitaciones.ObtenerPorIdAsync(id.Value);
        if (habitacion is null || !habitacion.Activo)
            return NotFound();

        return View(habitacion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!ModelState.IsValid)
        {
            var habitacion = await _habitaciones.ObtenerPorIdAsync(id);
            if (habitacion is null)
                return NotFound();

            return View(habitacion);
        }

        var eliminado = await _habitaciones.EliminarLogicoAsync(id);
        TempData["Mensaje"] = eliminado
            ? "La habitación se dio de baja correctamente."
            : "No se pudo dar de baja la habitación.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarTiposAsync(int? tipoHabitacionId = null)
    {
        var tipos = await _habitaciones.ListarTiposAsync();
        ViewBag.Tipos = new SelectList(tipos, "TipoHabitacionId", "Nombre", tipoHabitacionId);
    }

    private async Task CargarTiposFiltroAsync(int? tipoHabitacionId)
    {
        var tipos = await _habitaciones.ListarTiposAsync();
        ViewBag.TiposFiltro = new SelectList(tipos, "TipoHabitacionId", "Nombre", tipoHabitacionId);
    }
}
