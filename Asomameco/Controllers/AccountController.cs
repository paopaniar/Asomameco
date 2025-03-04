using Asomameco.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Asomameco.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Asomameco.Infraestructure.Data;
using System.Net.Mail;
using System.Net;
using Asomameco.Infraestructure.Models;
 
using Org.BouncyCastle.Crypto.Generators;

namespace Asomameco.Controllers
{
    //test
    public class AccountController : Controller
    {
        private readonly IServiceUsuario _usuarioService;
        private readonly AsomamecoContext context;

        public AccountController(IServiceUsuario usuarioService, AsomamecoContext _context)
        {
            _usuarioService = usuarioService;
            context = _context;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(int username, string password)
        {
            var user = await _usuarioService.AuthenticateAsync(username, password);

            if (user != null)
            {

                // Verificar si el usuario est� activo
                if (user.Estado1 == 0) // Aseg�rate de que la propiedad sea `Estado` o cambia seg�n tu modelo
                {
                    return RedirectToAction("CuentaDesactivada");
                }
                // Si el usuario tiene Estado 3, redirigir a la vista de cambio de contrase�a
                if (user.Estado1 == 3)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    return RedirectToAction("ChangePassword");
                }
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserType", user.TipoNavigation.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Nombre);
                HttpContext.Session.SetString("UserSurname", user.Apellidos); // Guardamos el apellido en la sesi�n


                List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Nombre +" "+ user.Apellidos),
                new Claim(ClaimTypes.Role, user.TipoNavigation.Descripcion),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
                ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), properties);



                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Usuario y/o contrase�a inv�lidos";
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string NewPassword, string ConfirmPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            if (NewPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "La contrase�a debe tener al menos 6 caracteres.";
                return View();
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Las contrase�as no coinciden.";
                return View();
            }

            var usuario = await context.Usuario.FindAsync(userId);
            if (usuario == null)
            {


                return RedirectToAction("Login");

            }

            // Actualizar la contrase�a y cambiar el estado a 1
            usuario.Contrase�a = NewPassword;
            usuario.Estado1 = 1;
            await context.SaveChangesAsync();

            // Cerrar sesi�n para que el usuario vuelva a iniciar sesi�n con su nueva contrase�a
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            TempData["SuccessMessage"] = "Contrase�a actualizada con �xito. Inicie sesi�n nuevamente.";



            usuario.TipoNavigation = await context.TipoUsuario.FindAsync(usuario.Tipo);
            HttpContext.Session.SetString("UserId", usuario.Id.ToString());
            HttpContext.Session.SetString("UserType", usuario.TipoNavigation.Id.ToString());
            HttpContext.Session.SetString("UserName", usuario.Nombre);
            HttpContext.Session.SetString("UserSurname", usuario.Apellidos); // Guardamos el apellido en la sesi�n


            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, usuario.Nombre +" "+ usuario.Apellidos),
                new Claim(ClaimTypes.Role, usuario.TipoNavigation.Descripcion),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), properties);




            return RedirectToAction("Index", "Home");
        }


        // M�todo para mostrar la vista cuando el usuario est� desactivado
        [HttpGet]
        public IActionResult CuentaDesactivada()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RecuperarContrase�a()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarContrase�a(string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                TempData["MensajeError"] = "Debe ingresar un correo v�lido.";
                return RedirectToAction("RecuperarContrase�a");
            }

            var usuario = await context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null)
            {
                TempData["MensajeError"] = "El correo no est� registrado.";
                return RedirectToAction("RecuperarContrase�a");
            }

            // Generar contrase�a temporal
            string nuevaContrase�a = GenerarContrase�aTemporal();

            // Guardar nueva contrase�a (preferiblemente hasheada si usas autenticaci�n segura)
            usuario.Contrase�a = nuevaContrase�a;
            usuario.Estado1 = 3;
            await context.SaveChangesAsync();

            // Enviar correo con la nueva contrase�a
            bool enviado = await EnviarCorreoRecuperacion(correo, nuevaContrase�a);


            TempData["MensajeExito"] = "Se ha enviado un correo con instrucciones para restablecer la contrase�a.";
            return RedirectToAction("RecuperarContrase�a");

        }

        private string GenerarContrase�aTemporal()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(caracteres, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }



        private async Task<bool> EnviarCorreoRecuperacion(string correo, string nuevaContrase�a)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("noreplyAsomamecojafethjimenez@gmail.com", "gvbh ueos xzyy pbmm\r\n"),
                    EnableSsl = true
                };

                var mensaje = new MailMessage
                {
                    From = new MailAddress("noreplyAsomamecojafethjimenez@gmail.com"),
                    Subject = "Recuperaci�n de Contrase�a",
                    Body = $"Su nueva contrase�a temporal es: {nuevaContrase�a} Cambie su contrase�a una vez que inicie sesi�n.",
                    IsBodyHtml = false
                };

                mensaje.To.Add(correo);

                await smtpClient.SendMailAsync(mensaje);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Tambi�n limpia la sesi�n

            return RedirectToAction("Login");
        }




    }
}
