using AutoMapper;
using Asomameco.Application.DTOs;
using Asomameco.Application.Services.Interfaces;
using Asomameco.Infraestructure.Models;
using Asomameco.Infraestructure.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Asomameco.Application.Services.Implementations
{
    public class ServiceAsamblea : IServiceAsamblea
    {
        private readonly IRepositoryAsamblea _repository;
        private readonly IMapper _mapper;
        public ServiceAsamblea(IRepositoryAsamblea repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }   

        public async Task<int> AddAsync(AsambleaDTO dto)
        {
            var objectMapped = _mapper.Map<Asamblea>(dto);
            return await _repository.AddAsync(objectMapped);
        }
        public async Task UpdateAsync(int id, AsambleaDTO dto)
        {
            var @object = await _repository.FindByIdAsync(id);
            var entity = _mapper.Map(dto, @object!);

            await _repository.UpdateAsync(id, entity);
        }
        public async Task<AsambleaDTO> FindByIdAsync(int id)
        {
            var @object = await _repository.FindByIdAsync(id);
            var objectMapped = _mapper.Map<AsambleaDTO>(@object);
            return objectMapped;
        }
        public async Task<ICollection<AsambleaDTO>> ListAsync()
        {
            //Obtener datos del repositorio
            var list = await _repository.ListAsync();
            var collection = _mapper.Map<ICollection<AsambleaDTO>>(list);
            // Return lista
            return collection;
        }


        public async Task DeleteAsync(int id, AsambleaDTO dto)
        {
            var @object = await _repository.FindByIdAsync(id);
            var entity = _mapper.Map(dto, @object!);

            await _repository.DeleteAsync(id, entity);
        }

    }
}

