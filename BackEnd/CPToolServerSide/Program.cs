using System;
using System.Net;
using CPToolServerSide.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CPToolServerSide
{

    public class Program 
    {

        public static void Main(string[] args)
        {

            // Continously Run Server Side for Compare Pay Tool to update Job Status and Results
            
            try
            {
                var host = CreateHostBuilder(args).Build();

                // Add Migrations

                // var services = (IServiceScopeFactory)host.Services.GetService(typeof(IServiceScopeFactory));

                // using var db = services.CreateScope().ServiceProvider.GetService<CPTDBContext>();
                // db.Database.Migrate();    

                // Launch
                host.Run();

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }


        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

