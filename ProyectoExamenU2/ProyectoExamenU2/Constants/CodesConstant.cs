namespace ProyectoExamenU2.Constants
{
    public static class CodesConstant
    {
        public const int OK = 200;                          // Operación exitosa
        public const int CREATED = 201;                     // Registro creado con éxito
        public const int NO_CONTENT = 204;                  // Sin contenido para mostrar
        public const int BAD_REQUEST = 400;                 // Solicitud incorrecta
        public const int UNAUTHORIZED = 401;                // No autorizado
        public const int FORBIDDEN = 403;                   // Acceso prohibido
        public const int NOT_FOUND = 404;                   // Registro no encontrado
        public const int METHOD_NOT_ALLOWED = 405;          // Método no permitido
        public const int CONFLICT = 409;                    // Conflicto en el registro
        public const int UNPROCESSABLE_ENTITY = 422;        // Entidad no procesable
        public const int INTERNAL_SERVER_ERROR = 500;       // Error en el servidor
        public const int NOT_IMPLEMENTED = 501;             // Función no implementada
        public const int BAD_GATEWAY = 502;                 // Error de puerta de enlace
        public const int SERVICE_UNAVAILABLE = 503;         // Servicio no disponible
        public const int GATEWAY_TIMEOUT = 504;             // Tiempo de espera agotado
        public const int PENDING = 102;                     // Procesando o pendriente
    }
}
