namespace ProyectoExamenU2.Constants
{
    public static class MessagesConstant
    {
        public const string RECORDS_FOUND = "Registros encontrados correctamente.";
        public const string RECORD_FOUND = "Registro encontrado correctamente.";
        public const string RECORD_NOT_FOUND = "Registro no encontrado.";
        public const string CREATE_SUCCESS = "Registro creado correctamente.";
        public const string CREATE_ERROR = "Se produjo un error al crear el registro.";
        public const string UPDATE_SUCCESS = "Registro editado correctamente.";
        public const string UPDATE_ERROR = "Se produjo un error al editar el registro.";
        public const string DELETE_SUCCESS = "Registro borrado correctamente.";
        public const string DELETE_ERROR = "Se produjo un error al borra el registro.";
        public const string LOGIN_SUCCESS = "Sesión iniciada correctamente.";
        public const string LOGIN_ERROR = "Se produjo un error al iniciar sesión, intentelo mas tarde.";


        // Mensajes para el Seeder de Datos
        public const string SEEDER_INIT_ERROR = "Ocurrio Un error durante la Ejecucion del Seeder";


        //Mensajes para la Autentificaion de Usuario
        // Error: Usuario no autorizado para esta operacion
        public const string UNAUTHORIZED_USER_ERROR = "Error 1001: No autorizado";

        // Error: Token no valido o expirado
        public const string INVALID_TOKEN_ERROR = "Error 1002: No autorizado";

        // Error: Usuario no tiene las claims necesarias
        public const string MISSING_CLAIMS_ERROR = "Error 1003: No autorizado";

        // Error: Usuario ya existe
        public const string USER_ALREADY_EXISTS_ERROR = "Error 1004: No autorizado";

        // Error: Usuario no autenticado
        public const string UNAUTHENTICATED_USER_ERROR = "Error 1005: No autorizado";
    }
}
