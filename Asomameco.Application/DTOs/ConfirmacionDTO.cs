using Asomameco.Infraestructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asomameco.Application.DTOs
{
    public record ConfirmacionDTO
    {
        public int Id { get; set; }

        public int IdMiembro { get; set; }

        public int IdAsamblea { get; set; }

        public DateTime FechaConfirmacion { get; set; }

        public int Metodo { get; set; }

        public virtual Usuario IdMiembroNavigation { get; set; } = null!;
    }
}
