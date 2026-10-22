using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio _reservas;

    public ReservasController(ReservaRepositorio reservas)
    {
        _reservas = reservas;
    }

    public async Task<IActionResult> Index()
    {
        var reservas = await _reservas.ListarAsync();
        return View(reservas);
    }

    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        ViewData["Ingreso"] = ingreso;
        ViewData["Salida"] = salida;

        if (ingreso is null && salida is null)
            return View(new List<Habitacion>());

        if (ingreso is null || salida is null)
        {
            ModelState.AddModelError(string.Empty, "Indique la fecha de ingreso y la de salida.");
            return View(new List<Habitacion>());
        }

        if (salida <= ingreso)
        {
            ModelState.AddModelError(string.Empty, "La fecha de salida debe ser posterior a la de ingreso.");
            return View(new List<Habitacion>());
        }

        ViewData["Noches"] = salida.Value.DayNumber - ingreso.Value.DayNumber;
        var habitaciones = await _reservas.ListarDisponiblesAsync(ingreso.Value, salida.Value);
        return View(habitaciones);
    }

    public async Task<IActionResult> Create(int? habitacionId, DateOnly? fechaIngreso, DateOnly? fechaSalida)
    {
        var reserva = new Reserva { Estado = "Pendiente" };

        if (habitacionId is > 0)
            reserva.HabitacionId = habitacionId.Value;
        if (fechaIngreso is not null)
            reserva.FechaIngreso = fechaIngreso.Value;
        if (fechaSalida is not null)
            reserva.FechaSalida = fechaSalida.Value;

        await CargarListasAsync(reserva.HuespedId, reserva.HabitacionId);
        return View(reserva);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
        ModelState.Remove(nameof(reserva.Huesped));
        ModelState.Remove(nameof(reserva.Habitacion));
        ModelState.Remove(nameof(reserva.Estado));
        reserva.Estado = "Pendiente";

        if (reserva.HuespedId <= 0)
            ModelState.AddModelError(nameof(reserva.HuespedId), "Seleccione un huésped.");

        if (reserva.HabitacionId <= 0)
            ModelState.AddModelError(nameof(reserva.HabitacionId), "Seleccione una habitación.");

        if (reserva.FechaIngreso == default)
            ModelState.AddModelError(nameof(reserva.FechaIngreso), "Indique la fecha de ingreso.");

        if (reserva.FechaSalida == default)
            ModelState.AddModelError(nameof(reserva.FechaSalida), "Indique la fecha de salida.");
        else if (reserva.FechaIngreso != default && reserva.FechaSalida <= reserva.FechaIngreso)
            ModelState.AddModelError(nameof(reserva.FechaSalida), "La fecha de salida debe ser posterior a la de ingreso.");

        if (reserva.FechaIngreso != default && reserva.FechaIngreso < DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError(nameof(reserva.FechaIngreso), "La fecha de ingreso no puede ser anterior a hoy.");

        if (!string.IsNullOrEmpty(reserva.Observaciones) && reserva.Observaciones.Length > 200)
            ModelState.AddModelError(nameof(reserva.Observaciones), "Las observaciones no pueden superar los 200 caracteres.");

        if (ModelState.IsValid)
        {
            if (!await _reservas.HuespedActivoAsync(reserva.HuespedId))
                ModelState.AddModelError(nameof(reserva.HuespedId), "Seleccione un huésped activo.");
            else if (!await _reservas.HabitacionActivaAsync(reserva.HabitacionId))
                ModelState.AddModelError(nameof(reserva.HabitacionId), "Seleccione una habitación activa.");
            else if (await _reservas.EstaOcupadaAsync(reserva.HabitacionId, reserva.FechaIngreso, reserva.FechaSalida))
                ModelState.AddModelError(nameof(reserva.HabitacionId), "La habitación no está disponible en el rango de fechas indicado.");
        }

        if (!ModelState.IsValid)
        {
            await CargarListasAsync(reserva.HuespedId, reserva.HabitacionId);
            return View(reserva);
        }

        await _reservas.CrearAsync(reserva);
        TempData["Mensaje"] = "La reserva se registró con estado Pendiente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var cancelada = await _reservas.CancelarAsync(id);
        if (cancelada)
            TempData["Mensaje"] = "La reserva se canceló correctamente.";
        else
            TempData["Error"] = "Solo se pueden cancelar reservas pendientes o confirmadas.";

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync(int? huespedId, int? habitacionId)
    {
        var huespedes = await _reservas.ListarHuespedesActivosAsync();
        var habitaciones = await _reservas.ListarHabitacionesActivasAsync();

        ViewBag.Huespedes = new SelectList(
            huespedes.Select(h => new
            {
                h.HuespedId,
                Texto = $"{h.Apellidos}, {h.Nombres} ({h.Documento})"
            }),
            "HuespedId",
            "Texto",
            huespedId);

        ViewBag.Habitaciones = new SelectList(
            habitaciones.Select(h => new
            {
                h.HabitacionId,
                Texto = $"{h.Numero} — {h.TipoHabitacion.Nombre} (S/ {h.TipoHabitacion.PrecioNoche:0.00})"
            }),
            "HabitacionId",
            "Texto",
            habitacionId);
    }
}
