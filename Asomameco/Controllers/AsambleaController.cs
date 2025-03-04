using Asomameco.Application.DTOs;
using Asomameco.Application.Services.Interfaces;
using Asomameco.Application.Services.Implementations;
using Asomameco.Infraestructure.Data;
using Asomameco.Infraestructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using X.PagedList;
using X.PagedList.Extensions;

namespace Asomameco.Web.Controllers
{
    public class AsambleaController : Controller
    {
        private readonly IServiceAsamblea _serviceAsamblea;
        private readonly IServiceEstadoAsamblea _serviceEstadoAsamblea;
        private readonly AsomamecoContext context;

        public AsambleaController(IServiceAsamblea serviceAsamblea, IServiceEstadoAsamblea serviceEstadoAsamblea, AsomamecoContext _context)
        {
            _serviceAsamblea = serviceAsamblea;
            _serviceEstadoAsamblea = serviceEstadoAsamblea;
            context = _context;
        }

      

        public async Task<ActionResult> IndexAdmin(int? page)
        {
            var collection = await _serviceAsamblea.ListAsync();
            //Cantidad de elementos por página
            return View(collection.ToPagedList(page ?? 1, 10));
        }

        [Authorize]
        public async Task<ActionResult> Details(int id)
        {
            try
            {
            
                var @object = await _serviceAsamblea.FindByIdAsync(id);
                if (@object == null)
                {
                    throw new Exception("Asamblea no existente");

                }

                return View(@object);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            var consecutivo = await context.Asamblea.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            ViewBag.id = consecutivo?.Id + 1 ?? 1;

            var estados = await _serviceEstadoAsamblea.ListAsync();
            ViewBag.ListEstado = new SelectList(estados, "Id", "Descripcion"); // Cambio de MultiSelectList a SelectList

            return View();
        }







        [Authorize]
        // POST: AsambleaController/Create
        [HttpPost]

        public async Task<ActionResult> Create(AsambleaDTO dto)
        {

            try
            {


                //Validación del formulario
                if (!ModelState.IsValid)
                {
                    // Lee del ModelState todos los errores que
                    // vienen para el lado del server
                    string errors = string.Join("; ", ModelState.Values
                                       .SelectMany(x => x.Errors)
                                       .Select(x => x.ErrorMessage));
                    ViewBag.ErrorMessage = errors;
                    return View();
                }
           

                //Crear
                await _serviceAsamblea.AddAsync(dto);
                return RedirectToAction("IndexAdmin");
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                ViewBag.ErrorMessage = $"Error al guardar el Asamblea: {innerException}";  
                return View(dto);
            }
        }

 



        [Authorize]
        // GET: AsambleaController/Edit/
        public async Task<ActionResult> Edit(int id)
        {
            var @object = await _serviceAsamblea.FindByIdAsync(id);
          

            // Obtener todos los Estados de Asamblea
            var allEstadosAsamblea = await _serviceEstadoAsamblea.ListAsync();

            // Filtrar los Estados de Asamblea con valores 2 y 3
            var EstadosAsambleaFiltrados = allEstadosAsamblea
                .Where(tu => tu.Id == 1 || tu.Id == 2 || tu.Id == 3) // Suponiendo que 'Id' es el campo que contiene el valor de Estado
                .ToList();

            // Asignar los Estados de Asamblea filtrados al ViewBag
            ViewBag.ListRol = EstadosAsambleaFiltrados;
            ViewBag.Id = @object.Id;

            // Enviar el Estado de Asamblea actual al ViewBag para que sea seleccionado
            ViewBag.SelectedEstadoAsamblea = @object.EstadoNavigation.Id; // Asegúrate de que 'EstadoAsambleaId' es el campo adecuado

            return View(@object);
        }

        //// POST: ProcesoPreparacionController/Edit/5
        [HttpPost]
        [Authorize]

        public async Task<ActionResult> Edit(int id, AsambleaDTO dto)
        {
            try
            {
                // Obtener el Asamblea actual de la base de datos
                var AsambleaActual = await _serviceAsamblea.FindByIdAsync(id);             
          
                await _serviceAsamblea.UpdateAsync(id, dto);

                return RedirectToAction("IndexAdmin");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al actualizar el Asamblea: {ex.InnerException?.Message ?? ex.Message}";
                return RedirectToAction("IndexAdmin");
            }
        }


        [HttpPost]
        [Authorize]

        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var Asamblea = await _serviceAsamblea.FindByIdAsync(id);
                if (Asamblea == null)
                {
                    throw new Exception("Asamblea no existente");
                }


                // Actualizar
                await _serviceAsamblea.DeleteAsync(id, Asamblea);

                // Redirigir a IndexAdmin después de eliminar
                return RedirectToAction("IndexAdmin");
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                ViewBag.ErrorMessage = $"Error al eliminar el Asamblea: {innerException}";

                // Redirigir a IndexAdmin en caso de error
                return RedirectToAction("IndexAdmin");
            }
        }
    }
}
