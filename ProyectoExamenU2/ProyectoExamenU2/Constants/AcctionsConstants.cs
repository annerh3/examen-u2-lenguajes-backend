namespace ProyectoExamenU2.Constants
{
    public static class AcctionsConstants
    {
        public const string USER_CREATED = "[USER: CREATED]";               // Usuario registrado en el sistema
        public const string USER_DELETED = "[USER: DELETED]";               // Usuario eliminado del sistema
        public const string USER_CHANGE = "[USER: CHANGE]";                 // Cambio en algo de un usuario



        public const string LOGIN_SUCCESSFUL = "[LOGIN: SUCCESS]";          // Login exitoso
        public const string LOGIN_FAILED = "[LOGIN: FAILED]";               // Intento de login fallido


        public const string DATA_UPDATED = "[DATA: UPDATE]";                // Actualización de datos
        public const string DATA_DELETED = "[DATA: DELETE]";                // Eliminación de datos
        public const string DATA_CREATED = "[DATA: CREATED]";               // Eliminación de datos
        public const string DATA_GET = "[DATA: GET_DATA]";               // Eliminación de datos


        public const string PERMISSION_GRANTED = "[]";                      // Permiso otorgado
        public const string PERMISSION_REVOKED = "[]";         // Permiso retirado
        
        public const string ACCESS_DENIED = "[ACCES: DENIED]";               // Intento de acceso no autorizado]

    }
}
