using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UploadTransaction.Domain.BusinessLogicLayer;
using UploadTransaction.Domain.BusinessLogicLayer.Impl;
using UploadTransaction.Domain.DataInfrastructure;
using static UploadTransaction.Helpers.Helper;

namespace UploadTransaction
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TransactionDbContext>(options => options.UseMySQL(Configuration.GetConnectionString("Default")));
            services.AddControllers();
            services.AddCors();
            services.AddSwaggerGen(options =>
            {
                options.OperationFilter<FileUploadFilter>();
            });
            services.AddScoped<ITransactionBusinessLogic, TransactionBusinessLogic>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)
            app.UseSwagger();

            app.UseSwaggerUI();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Upload Transaction Solution");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
