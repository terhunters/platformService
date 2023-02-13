using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    public static class PrepDB
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProduction)
        {
            using(var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDBContext>(), isProduction);                
            }
        }

        private static void SeedData(AppDBContext context, bool isProduction)
        {
            if(isProduction)
            {
                context.Database.Migrate();
            }
            
            if (!context.Platforms.Any())
            {
                Console.WriteLine("Seeding data...");
                
                context.Platforms.AddRange(
                    new Platform(){Name = "Record1", Publisher = "Microsoft", License = "Free"},
                    new Platform(){Name = "Record2", Publisher = "Microsoft", License = "Free"},
                    new Platform(){Name = "Record3", Publisher = "Azure", License = "Free"}
                    );

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("We already have data in database");
            }
        }
    }
}