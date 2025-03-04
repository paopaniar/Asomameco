using Asomameco.Infraestructure.Data;
using Asomameco.Infraestructure.Models;
using Asomameco.Infraestructure.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 


namespace Asomameco.Infraestructure.Repository.Implementations
{
    public class RepositoryAsamblea:IRepositoryAsamblea
    {
        private readonly AsomamecoContext _context;
        public RepositoryAsamblea(AsomamecoContext context) {
            _context=context;
        }

        public async Task<Asamblea> FindByIdAsync(int id)
        {

            var @object = await _context.Set<Asamblea>()
             .Include(p => p.EstadoNavigation) // Incluye el tipo de Asamblea   
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync();

            return @object!;
        }

        public async Task<ICollection<Asamblea>> ListAsync()
        {
            var collection = await _context.Set<Asamblea>()
                .Include(p => p.EstadoNavigation) // Incluye el tipo de Asamblea
                .ToListAsync();
            return collection;
        }

    
 

        public async Task<int> AddAsync(Asamblea entity)
        {
 
 
          

            // Insertar el nuevo Asamblea en la tabla Asamblea usando SQL
            var sqlAsamblea = @"
        INSERT INTO Asamblea (Id, Fecha,Estado) 
        VALUES (@Id, @Fecha, @Estado);";

            // Parámetros para la consulta SQL del Asamblea
            var parametersAsamblea = new[]
            {
        new SqlParameter("@Id", entity.Id),
        new SqlParameter("@Fecha", entity.Fecha),
        new SqlParameter("@Estado", entity.Estado)
 
    };

            // Ejecutar la consulta y obtener el Id del Asamblea recién insertado
            var AsambleaID = await _context.Database.ExecuteSqlRawAsync(sqlAsamblea, parametersAsamblea);

            return AsambleaID; // Retornamos el Id del Asamblea creado
        }


        public async Task UpdateAsync(int id, Asamblea entity)
        {
        
            var sqlAsamblea = @"
        UPDATE Asamblea 
        SET 
            Fecha = @Fecha, 
            Estado = @Estado 
     
        WHERE Id = @Id;";

            var parametersAsamblea = new[]
            {
        new SqlParameter("@Id", entity.Id),
        new SqlParameter("@Fecha", entity.Fecha),
        new SqlParameter("@Estado", entity.Estado)
 
    };

            await _context.Database.ExecuteSqlRawAsync(sqlAsamblea, parametersAsamblea);
        }



        public async Task DeleteAsync(int Id, Asamblea entity)
        {
  
            var id = entity.Id; // ID del Asamblea

            var sql = "DELETE FROM Asamblea WHERE Id = @Id";

            // Usando SqlParameter para evitar el error de declaración de variable
            var parameters = new[]
            {
 
        new SqlParameter("@Id", id)
                         };

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }


    }
}
