﻿using System;
using System.Collections.Generic;

namespace Asomameco.Infraestructure.Models;

public partial class Asamblea
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public int Estado { get; set; }

    public virtual ICollection<Asistencia> Asistencia { get; set; } = new List<Asistencia>();

    public virtual EstadoAsamblea EstadoNavigation { get; set; } = null!;
}
