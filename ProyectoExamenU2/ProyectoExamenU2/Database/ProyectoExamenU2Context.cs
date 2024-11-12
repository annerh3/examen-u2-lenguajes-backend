
using ProyectoExamenU2.Database.Configuration;
using ProyectoExamenU2.Database.Entities;
using ProyectoExamenU2.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProyectoExamenU2.Database
{
    public class ProyectoExamenU2Context: IdentityDbContext<UserEntity>
    {
        //Variables Globales

        private readonly IAuditService _auditService;
        //Constructor de La Clase
        public ProyectoExamenU2Context(
            DbContextOptions options,
            IAuditService auditService
            ) : base(options)
        {
            this._auditService = auditService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");


            // Creando Security Schema
            modelBuilder.HasDefaultSchema("security");

            modelBuilder.Entity<UserEntity>().ToTable("users");
            modelBuilder.Entity<IdentityRole>().ToTable("roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("users_roles");

            //Estos son los permisos
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("users_claims");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("roles_claims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("users_logins");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("users_tokens");

            //Aplicacion de las Configuraciones de Entidades
           // modelBuilder.ApplyConfiguration(new ExampleConfiguration());
            modelBuilder.ApplyConfiguration(new AccountCatalogConfiguration());
            modelBuilder.ApplyConfiguration(new BalanceConfiguraction());
            modelBuilder.ApplyConfiguration(new JournalEntryConfiguraction());
            modelBuilder.ApplyConfiguration(new JournalEntryDetailConfiguraction());


            // Set Foreign Keys OnRestrict
            var eTypes = modelBuilder.Model.GetEntityTypes(); // todo el listado de entidades
            foreach (var type in eTypes)
            {
                var foreignKeys = type.GetForeignKeys();
                foreach (var fk in foreignKeys)
                {
                    fk.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }


            // las configuraciones en decimales ahora se realizan en el archivo de Configuracion
            // fallo realizarlo alli
            modelBuilder.Entity<BalanceEntity>()
                 .Property(e => e.BalanceAmount)
                 .HasPrecision(18, 2);

            modelBuilder.Entity<JournalEntryDetailEntity>()
                .Property(e => e.Account)
                .HasPrecision(18, 2);
            // Ignorar la propiedad calculada TotalPrice
            //modelBuilder.Entity<DetailEntity>()
            //.Property(d => d.TotalPrice)
            //.HasColumnType("decimal(18,2)");

        }

        // Metodo para capturar el usuario que esta guardando los cambios creando o modificando 
        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is AuditEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified
                ));

            foreach (var entry in entries)
            {
                var entity = entry.Entity as AuditEntity;
                if (entity != null)
                {
                    // si esta agregando o creando 
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedBy = _auditService.GetUserId();
                        entity.CreatedDate = DateTime.Now;
                    }
                   // si esta modificando 
                    else
                    {
                        entity.UpdatedBy = _auditService.GetUserId();
                        entity.UpdatedDate = DateTime.Now;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        // Agregando el contexto 
       // public DbSet<CategoryProductEntity> CategoryProducts { get; set; }
        public DbSet<AccountCatalogEntity> AccountCatalogs { get; set; }
        public DbSet<BalanceEntity> Balances { get; set; }
        public DbSet<JournalEntryDetailEntity> JournalEntryDetails { get; set; }
        public DbSet<JournalEntryEntity> JournalEntries { get; set; }


    }
}
