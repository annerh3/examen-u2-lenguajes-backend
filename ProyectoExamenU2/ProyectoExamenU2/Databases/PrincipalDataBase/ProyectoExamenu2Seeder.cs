using ProyectoExamenU2.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProyectoExamenU2.Databases.PrincipalDataBase.Entities;

namespace ProyectoExamenU2.Databases.PrincipalDataBase
{
    public class ProyectoExamenu2Seeder
    {

        // Lista de Empleados Quemados
        private static List<object> employees = new List<object>
        {
            new { name = "Anner H", email = "anner@gmail.com" },
            new { name = "Hector M", email = "hector@gmail.com" },
            new { name = "Marlon Lopez", email = "m_lopez@gmail.com" },
            new { name = "Shalom Henriquez", email = "s_hqz2@gmail.com" },
            new { name = "Ruth Quintanilla", email = "ruthquintanilla3@icloud.com" },
            new { name = "Naara Chávez", email = "naara.chavez@unah.hn" },
          

        };
        // Carga de los Datos de la APP
        public static async Task LoadDataAsync(
            ProyectoExamenU2Context context,
            ILoggerFactory loggerFactory,
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            try
            {
                await LoadUsersAsync(userManager, loggerFactory);

            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenu2Seeder>();
                logger.LogError(e, $"{MessagesConstant.SEEDER_INIT_ERROR}");
            }

        }
        //Agregando los Roles 
        public static async Task LoadUsersAsync(
            UserManager<UserEntity> userManager,
            ILoggerFactory loggerFactory
            )
        {
            try
            {
                for (int i = 0; i < employees.Count(); i++)// Creando Usuarios que son clientes
                {
                    dynamic usercClientInfo = employees[i]; // para poder acceder a las propiedades del objeto

                    var client = new UserEntity
                    {
                        Email = usercClientInfo.email,
                        UserName = usercClientInfo.email,
                        Name = usercClientInfo.name
                    };

                    await userManager.CreateAsync(client, "Temporal01*"); // crear usuario cliente                 
                }
            }

            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenu2Seeder>();
                logger.LogError(e.Message);
            }
        }
    }
}
