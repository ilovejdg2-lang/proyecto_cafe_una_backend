namespace proyecto_cafe_una_backend.Services;

public static class UsuarioValidacion
{
    public const int MaxNombreLength = 20;
    public const int MaxPasswordLength = 15;
    public const int MinPasswordLength = 6;

    public static void ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new InvalidOperationException("El nombre es obligatorio.");
        }

        if (nombre.Trim().Length > MaxNombreLength)
        {
            throw new InvalidOperationException(
                $"El nombre de usuario no puede tener más de {MaxNombreLength} caracteres.");
        }
    }

    public static void ValidarPassword(string? password, bool requerida = true)
    {
        if (string.IsNullOrEmpty(password))
        {
            if (requerida)
            {
                throw new InvalidOperationException("La contraseña es obligatoria.");
            }

            return;
        }

        if (password.Length > MaxPasswordLength)
        {
            throw new InvalidOperationException(
                $"La contraseña no puede tener más de {MaxPasswordLength} caracteres.");
        }

        if (password.Length < MinPasswordLength)
        {
            throw new InvalidOperationException(
                $"La contraseña debe tener al menos {MinPasswordLength} caracteres.");
        }
    }

    public static void ValidarPasswordActual(string passwordGuardada, string? passwordActual)
    {
        if (string.IsNullOrWhiteSpace(passwordActual))
        {
            throw new InvalidOperationException("Debe ingresar la contraseña de la cuenta.");
        }

        if (!string.Equals(passwordGuardada, passwordActual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("La contraseña no es correcta.");
        }
    }
}
