using ProyectoExamenU2.Constants;
using ProyectoExamenU2.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ProyectoExamenU2.Database
{
    public class ProyectoExamenu2Seeder
    {

        // Lista de Clientes Quemados
        private static List<object> userClients = new List<object>
        {
            new { id = new Guid("ca738dbb-0c10-47b2-a1e7-f61e2e46f515"), name = "Marlon Lopez", email = "m_lopez@gmail.com" },
            new { id = new Guid("ee10a477-0bb1-4f24-851a-d275f187f5fd"), name = "Shalom Henriquez", email = "s_hqz2@gmail.com" },
            new { id = new Guid("dcf71a2c-d287-4b4a-b16a-f4e380f59959"), name = "Ruth Quintanilla", email = "ruthquintanilla3@icloud.com" },
            new { id = new Guid("17a4c7c8-60aa-4d88-9900-666a4ae59ea3"), name = "Naara Chávez", email = "naara.chavez@unah.hn" },
            new { id = new Guid("8f5046cf-d8ee-49ac-a0b0-7b1328bde15f"), name = "Siscomp", email = "siscomp.hn@gmail.com" },
            new { id = new Guid("ca02aadc-05f1-453f-bf7b-c80562d52e55"), name = "Aire Frío", email = "aire.frio@gmail.com" },
            new { id = new Guid("720ce9c4-d31d-46bc-b45b-70ece08ece67"), name = "Municipalidad de Santa Rosa de Copán", email = "src_muni@gmail.com" },
            new { id = new Guid("c6a65998-231e-4355-bf67-536a243ccfae"), name = "Empresa Municipal Aguas de Santa Rosa", email = "gerencia@aguasdesantarosa.org" },
            new { id = new Guid("367ea698-7bb6-466f-9482-5a11427b22c0"), name = "Iglesia Menonita", email = "menonita_src@gmail.com" },
            new { id = new Guid("3027d32a-e2c5-4935-bfa4-b07a5708b980"), name = "Iglesia Católica de Santa Rosa", email = "e.cat_src@gmail.com" },
            new { id = new Guid("2f1d0ac0-e87b-4daf-b153-b4ba121d4d33"), name = "PILARH", email = "pilarh_hn@gmail.com" },
            new { id = new Guid("374756e0-ce74-4b67-ae49-89b1467a0c0e"), name = "Vision Fund", email = "vision_fund@gmail.com" }

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
                await LoadRolesAndUsersAsync(userManager, roleManager, loggerFactory);

                await LoadClientsTypesAsync(loggerFactory, context);
                await LoadClientsAsync(loggerFactory, context);

                await LoadCategoriesProductAsync(loggerFactory, context);


                await LoadProductsAsync(loggerFactory, context);

                await LoadEventsAsync(loggerFactory, context);
                await LoadNotesAsync(loggerFactory, context);
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenu2Seeder>();
                logger.LogError(e, $"{MessagesConstant.SEEDER_INIT_ERROR}");
            }

        }
        //Agregando los Roles 
        public static async Task LoadRolesAndUsersAsync(
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager,
            ILoggerFactory loggerFactory
            )
        {
            try
            {
                if (!await roleManager.Roles.AnyAsync())
                {
                    await roleManager.CreateAsync(new IdentityRole(RolesConstants.ADMIN)); // creando roles
                    await roleManager.CreateAsync(new IdentityRole(RolesConstants.CLIENT));

                }

                if (!await userManager.Users.AnyAsync())
                {   
                    for (int i = 0; i < userClients.Count(); i++)// Creando Usuarios que son clientes
                    {
                        dynamic usercClientInfo = userClients[i]; // para poder acceder a las propiedades del objeto

                        var client = new UserEntity
                        {
                            Id = usercClientInfo.id.ToString(),
                            Email = usercClientInfo.email,
                            UserName = usercClientInfo.email,
                            Name = usercClientInfo.name
                        };

                        await userManager.CreateAsync(client, "Temporal01*"); // crear usuario cliente
                        await userManager.AddToRoleAsync(client, RolesConstants.CLIENT); // asignar rol
                    }


                    // agregando los Administradores
                    var userAdmin1 = new UserEntity
                    {
                        Email = "admin@gmail.com",
                        UserName = "admin@gmail.com",
                        Name = "Héctor Martínez"
                    };

                    var userAdmin2 = new UserEntity
                    {
                        Email = "annerh3@gmail.com",
                        UserName = "annerh3@gmail.com",
                        Name = "Anner Henríquez"
                    };

                    // Agregando las Contrase;as de los Administradores
                    await userManager.CreateAsync(userAdmin1, "Temporal01*");
                    await userManager.CreateAsync(userAdmin2, "Temporal01*");
                    
                    // Agregando Los roles de los Administradores
                    await userManager.AddToRoleAsync(userAdmin1, RolesConstants.ADMIN);
                    await userManager.AddToRoleAsync(userAdmin2, RolesConstants.ADMIN);

                }

            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenu2Seeder>();
                logger.LogError(e.Message);
            }
        }



        // SEED DE DATOS DE Clientes 
        public static async Task LoadClientsAsync(ILoggerFactory loggerFactory, ProyectoExamenU2Context context)
        {
            try
            {
                var jsonFilePath = "SeedData/clients.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var clients = JsonConvert.DeserializeObject<List<ClientEntity>>(jsonContent);

                if (!await context.Clients.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();
                    
                    for (int i = 0; i < clients.Count; i++) // actualiza propiedades de auditoría
                    {
                        clients[i].CreatedBy = user.Id;
                        clients[i].CreatedDate = DateTime.Now;
                        clients[i].UpdatedBy = user.Id;
                        clients[i].UpdatedDate = DateTime.Now;
                    }

                    context.AddRange(clients);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenU2Context>();
                logger.LogError(e, "Error al ejecutar el Seed de Clientes.");
            }
        }



        //seed de los eventos 
        public static async Task LoadEventsAsync(ILoggerFactory loggerFactory, ProyectoExamenU2Context context)
        {
            try
            {
                var jsonFilePath = "SeedData/events.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var events = JsonConvert.DeserializeObject<List<EventEntity>>(jsonContent);

                if (!await context.Events.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();
                    for (int i = 0; i < events.Count; i++) // actualiza propiedades de auditoría
                    {
                        events[i].CreatedBy = user.Id;
                        events[i].CreatedDate = DateTime.Now;
                        events[i].UpdatedBy = user.Id;
                        events[i].UpdatedDate = DateTime.Now;
                    }

                    context.AddRange(events);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenU2Context>();
                logger.LogError(e, "Error al ejecutar el Seed de Eventos.");
            }
        }
        // seed de las Notas
        public static async Task LoadNotesAsync(ILoggerFactory loggerFactory, ProyectoExamenU2Context context)
        {
            try
            {
                var jsonFilePath = "SeedData/notes.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var notes = JsonConvert.DeserializeObject<List<NoteEntity>>(jsonContent);

                if (!await context.Notes.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();
                    for (int i = 0; i < notes.Count; i++) // actualiza propiedades de auditoría
                    {
                        notes[i].CreatedBy = user.Id;
                        notes[i].CreatedDate = DateTime.Now;
                        notes[i].UpdatedBy = user.Id;
                        notes[i].UpdatedDate = DateTime.Now;
                    }

                    context.AddRange(notes);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenU2Context>();
                logger.LogError(e, "Error al ejecutar el Seed de Notes.");
            }
        }
        public static async Task LoadClientsTypesAsync(ILoggerFactory loggerFactory, ProyectoExamenU2Context context)
        {
            try
            {
                var jsonFilePath = "SeedData/clientstypes.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var clientsTypes = JsonConvert.DeserializeObject<List<ClientTypeEntity>>(jsonContent);

                if (!await context.TypesOfClient.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();
                    for (int i = 0; i < clientsTypes.Count; i++) // actualiza propiedades de auditoría
                    {
                        clientsTypes[i].CreatedBy = user.Id;
                        clientsTypes[i].CreatedDate = DateTime.Now;
                        clientsTypes[i].UpdatedBy = user.Id;
                        clientsTypes[i].UpdatedDate = DateTime.Now;
                    }

                    context.AddRange(clientsTypes);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenU2Context>();
                logger.LogError(e, "Error al ejecutar el Seed de clientsTypes.");
            }
        }

        public static async Task LoadCategoriesProductAsync(ILoggerFactory loggerFactory, ProyectoExamenU2Context context)
        {
            try
            {
                var jsonFilePath = "SeedData/categoriesproduct.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var categoriesproduct = JsonConvert.DeserializeObject<List<CategoryProductEntity>>(jsonContent);

                if (!await context.CategoryProducts.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();
                    for (int i = 0; i < categoriesproduct.Count; i++) // actualiza propiedades de auditoría
                    {
                        categoriesproduct[i].CreatedBy = user.Id;
                        categoriesproduct[i].CreatedDate = DateTime.Now;
                        categoriesproduct[i].UpdatedBy = user.Id;
                        categoriesproduct[i].UpdatedDate = DateTime.Now;
                    }

                    context.AddRange(categoriesproduct);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenU2Context>();
                logger.LogError(e, "Error al ejecutar el Seed de categoriesProduct.");
            }
        }

        public static async Task LoadProductsAsync(ILoggerFactory loggerFactory, ProyectoExamenU2Context context)
        {
            try
            {
                var jsonFilePath = "SeedData/products.json";
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath); // Lee el contenido completo del archivo JSON y lo almacena en 'jsonContent'.
                var products = JsonConvert.DeserializeObject<List<ProductEntity>>(jsonContent);

                if (!await context.Products.AnyAsync())
                {
                    var user = await context.Users.FirstOrDefaultAsync();
                    for (int i = 0; i < products.Count; i++) // actualiza propiedades de auditoría
                    {
                        products[i].CreatedBy = user.Id;
                        products[i].CreatedDate = DateTime.Now;
                        products[i].UpdatedBy = user.Id;
                        products[i].UpdatedDate = DateTime.Now;
                    }

                    context.AddRange(products);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                var logger = loggerFactory.CreateLogger<ProyectoExamenU2Context>();
                logger.LogError(e, "Error al ejecutar el Seed de productos.");
            }
        }

    }
}
