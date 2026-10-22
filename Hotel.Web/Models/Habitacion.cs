using System;
using System.Collections.Generic;

namespace Hotel.Web.Models;

public partial class Habitacion
{
    public int HabitacionId { get; set; }

    public int Numero { get; set; }

    public int TipoHabitacionId { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public virtual TipoHabitacion TipoHabitacion { get; set; } = null!;
}
