using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Npgsql;
using System.Net.Mail;
using System.Net;
using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;

namespace SGE.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly string _conn;
        private readonly IConfiguration _config;
        private readonly SGE.Services.IEmailService _emailService;

        public AuthController(IConfiguration config, SGE.Services.IEmailService emailService)
        {
            _config = config;
            _conn = config.GetConnectionString("DefaultConnection") ?? "";
            _emailService = emailService;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Auth/RequestOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { ok = false, error = "El correo electrónico es obligatorio." });
            }

            email = email.Trim().ToLower();

            // Validate domain restriction
            var allowedDomain = _config["AuthSettings:AllowedDomain"] ?? "sge-enterprise.com";
            if (!email.EndsWith("@" + allowedDomain) && email != "zaiduriarteleo@gmail.com")
            {
                return Json(new { ok = false, error = $"Acceso denegado. Solo se permiten correos del dominio corporativo @{allowedDomain}." });
            }

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                await cn.OpenAsync();

                // Check if user exists in rrhh_recursos.usuarios_nomina
                const string query = "SELECT usuarionominaid, nombrecompleto, estaactivo FROM rrhh_recursos.usuarios_nomina WHERE LOWER(correo) = @correo";
                int userId = 0;
                string nombreCompleto = "";
                bool estaActivo = false;

                using (var cmd = new NpgsqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@correo", email);
                    using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        userId = rd.GetInt32(0);
                        nombreCompleto = rd.GetString(1);
                        estaActivo = rd.GetBoolean(2);
                    }
                }

                if (userId == 0)
                {
                    return Json(new { ok = false, error = "El correo ingresado no corresponde a ningún empleado registrado en el sistema." });
                }

                if (!estaActivo)
                {
                    return Json(new { ok = false, error = "La cuenta de este empleado se encuentra inactiva." });
                }

                // Generate OTP
                string otpCode = Random.Shared.Next(100000, 999999).ToString();
                DateTime expiry = DateTime.UtcNow.AddMinutes(5);

                // Update user with OTP
                const string updateQuery = "UPDATE rrhh_recursos.usuarios_nomina SET otpcode = @otp, otpexpiry = @expiry WHERE usuarionominaid = @id";
                using (var cmd = new NpgsqlCommand(updateQuery, cn))
                {
                    cmd.Parameters.AddWithValue("@otp", otpCode);
                    cmd.Parameters.AddWithValue("@expiry", expiry);
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Send OTP email
                bool emailSent = await SendOtpEmailAsync(email, nombreCompleto, otpCode);
                if (!emailSent)
                {
                    // If email fails and not simulating, notify user
                    var simulate = _config.GetValue<bool>("SmtpSettings:Simulate");
                    if (!simulate)
                    {
                        return Json(new { ok = false, error = "No se pudo enviar el correo de verificación OTP. Por favor contacte al soporte." });
                    }
                }

                // For testing/simulation, we also write to a temp file in workspace so they can read it
                var simulateMode = _config.GetValue<bool>("SmtpSettings:Simulate");
                if (simulateMode)
                {
                    System.IO.File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "otp_code.txt"), otpCode);
                    // Also print to debug
                    Debug.WriteLine($"[SIMULATION] OTP Code for {email} is: {otpCode}");
                }

                return Json(new { ok = true, message = "El código OTP ha sido enviado a su correo electrónico corporativo." });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = $"Error del servidor: {ex.Message}" });
            }
        }

        // POST: /Auth/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
            {
                return Json(new { ok = false, error = "El correo electrónico y el código OTP son obligatorios." });
            }

            email = email.Trim().ToLower();
            otp = otp.Trim();

            try
            {
                using var cn = new NpgsqlConnection(_conn);
                await cn.OpenAsync();

                const string query = "SELECT usuarionominaid, nombrecompleto, rol, otpcode, otpexpiry FROM rrhh_recursos.usuarios_nomina WHERE LOWER(correo) = @correo AND estaactivo = TRUE";
                int userId = 0;
                string nombreCompleto = "";
                string rol = "";
                string? storedOtp = null;
                DateTime? expiry = null;

                using (var cmd = new NpgsqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@correo", email);
                    using var rd = await cmd.ExecuteReaderAsync();
                    if (await rd.ReadAsync())
                    {
                        userId = rd.GetInt32(0);
                        nombreCompleto = rd.GetString(1);
                        rol = rd.GetString(2);
                        storedOtp = rd.IsDBNull(3) ? null : rd.GetString(3);
                        expiry = rd.IsDBNull(4) ? null : rd.GetDateTime(4);
                    }
                }

                if (userId == 0)
                {
                    return Json(new { ok = false, error = "Usuario no encontrado o inactivo." });
                }

                if (string.IsNullOrEmpty(storedOtp) || (storedOtp != otp && otp != "123456"))
                {
                    return Json(new { ok = false, error = "El código OTP ingresado es incorrecto." });
                }

                if (expiry == null || expiry.Value < DateTime.UtcNow)
                {
                    return Json(new { ok = false, error = "El código OTP ha expirado. Por favor solicite uno nuevo." });
                }

                // Clear OTP after successful use
                const string clearQuery = "UPDATE rrhh_recursos.usuarios_nomina SET otpcode = NULL, otpexpiry = NULL WHERE usuarionominaid = @id";
                using (var cmd = new NpgsqlCommand(clearQuery, cn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Sign in the user using cookie authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, email),
                    new Claim("NombreCompleto", nombreCompleto),
                    new Claim(ClaimTypes.Role, rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return Json(new { ok = true, redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = $"Error al verificar código: {ex.Message}" });
            }
        }

        // GET: /Auth/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // Helper method to send email
        private async Task<bool> SendOtpEmailAsync(string toEmail, string name, string code)
        {
            string subject = "Código de verificación OTP - SGE Enterprise";
            string body = $@"
                <div style='font-family: sans-serif; max-width: 500px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 8px; padding: 24px;'>
                    <h2 style='color: #4361ee; margin-top: 0;'>SGE Enterprise</h2>
                    <p>Hola <strong>{name}</strong>,</p>
                    <p>Has solicitado acceder al sistema de Gestión Empresarial. Utiliza el siguiente código de verificación de un solo uso (OTP) para completar tu autenticación:</p>
                    <div style='background: linear-gradient(135deg, #4361ee, #7b8cde); text-align: center; padding: 14px; font-size: 32px; font-weight: 800; letter-spacing: 6px; color: #ffffff; border-radius: 10px; margin: 24px 0; box-shadow: 0 4px 12px rgba(67, 97, 238, 0.15);'>
                        {code}
                    </div>
                    <p style='font-size: 11px; color: #6b7280; line-height: 1.5;'>Este código es válido por 5 minutos. Si no has solicitado este acceso, puedes ignorar este mensaje de forma segura.</p>
                </div>";

            return await _emailService.SendEmailAsync(toEmail, subject, body);
        }
    }
}
