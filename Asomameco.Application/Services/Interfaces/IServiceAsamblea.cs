 
using Asomameco.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asomameco.Application.Services.Interfaces
{
    public interface IServiceAsamblea
    {
        Task<ICollection<AsambleaDTO>> ListAsync();
        Task<AsambleaDTO> FindByIdAsync(int id); 
        Task<int> AddAsync(AsambleaDTO dto);
        Task UpdateAsync(int id, AsambleaDTO dto);
        Task DeleteAsync(int id, AsambleaDTO dto);
    }
}
