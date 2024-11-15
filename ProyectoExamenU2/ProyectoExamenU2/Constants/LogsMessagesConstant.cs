namespace ProyectoExamenU2.Constants
{
    public static class LogsMessagesConstant
    {
        public const string PENDING = "[PENDIENTE] => Registro en proceso.";
        public const string RECORD_ALREADY_EXISTS = "[ERROR] => El registro ya existe.";
        public const string NO_AUTHENTICATION = "[ERROR] => No hay autenticación o permisos insuficientes.";
        public const string INVALID_DATA = "[ERROR] => Datos no válidos.";
        public const string API_ERROR = "[ERROR] => Error en la API.";
        public const string COMPLETED_SUCCESS = "[COMPLETADO] => Operación completada exitosamente.";
        public const string NOT_FOUND = "[ERROR] => El recurso solicitado no fue encontrado.";
        public const string UPDATE_SUCCESS = "[COMPLETADO] => Actualización realizada correctamente.";
        public const string DELETE_SUCCESS = "[COMPLETADO] => Eliminación realizada correctamente.";
        public const string DATABASE_ERROR = "[ERROR] => Error en la base de datos.";

        // validaciones
        public const string FUTURE_DATE_ERROR = " DATE => DATE ERROR>> DATE CANT BE FUTURE DATE";
        public const string BEBIAROR_ERROR = " MOVETYPE => ERROR>> MOVE TYPE IS BOCKED";
        public const string ACTIVE_ACCOUNT_ERROR = " ACCOUNT => ERROR>> ACCOUNT IS INACTIVE";
        public const string AMOUNT_ERROR = " AMOUNT => ERROR>> Fatal Transaccion: Monto es menor al de la cuenta ";

        public const string INSUFFICIENT_FUNDS_ERROR = "Error de Fondos: La cuenta no tiene fondos suficientes para realizar esta transacción.";
    }
}
