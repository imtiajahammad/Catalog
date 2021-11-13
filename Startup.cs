using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Catalog.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;

namespace Catalog
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
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
            var mongoDbsettings=Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();


            services.AddSingleton<IMongoClient>(serviceProvider=>{

                return new MongoClient(mongoDbsettings.ConnectionString);
            });
            //services.AddSingleton<IItemsRepository,InMemItemsRepository>();
            services.AddSingleton<IItemsRepository,MongoDbItemsRepository>();
            services.AddControllers(options=>
            {
                options.SuppressAsyncSuffixInActionNames=false;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog", Version = "v1" });
            });

            /*services.AddHealthChecks();//to check if the api application is running to serve only*/
            /*
            1. to check if the api application is running to serve
            2. this addition will check if the database is also healthy or not
            */
            /*services.AddHealthChecks()
                    .AddMongoDb(
                        mongoDbsettings.ConnectionString
                        ,name:"mongoDb"
                        ,timeout:TimeSpan.FromSeconds(3));*/

            services.AddHealthChecks()
                    .AddMongoDb(
                        mongoDbsettings.ConnectionString
                        ,name:"mongoDb"
                        ,timeout:TimeSpan.FromSeconds(3)
                        ,tags: new[]{"ready"});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog v1"));
            }

            if(env.IsDevelopment()){
                app.UseHttpsRedirection();
            }
            

            app.UseRouting();

            app.UseAuthorization();

            /*app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });*/

            //more healthChecks -https://github.com/Xabaril/AspnetCore.Diagnostics.HealthChecks
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions{
                    Predicate=(check)=>check.Tags.Contains("ready"),
                    ResponseWriter=async(context,report)=>{
                        var result=JsonSerializer.Serialize(
                            new {
                                status=report.Status.ToString()
                                ,checks=report.Entries.Select(entry=>new
                                {
                                    name=entry.Key,
                                    status=entry.Value.Status.ToString(),
                                    exception=entry.Value.Exception!=null ?entry.Value.Exception.Message:"none",
                                    duration=entry.Value.Duration.ToString()
                                })
                            }
                        );
                    context.Response.ContentType=MediaTypeNames.Application.Json;
                    await context.Response.WriteAsync(result);
                    }
                });
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions{
                    Predicate=(_)=>false
                });
            });
        }
    }
}
/*
TO MAKE MONGO DB IMAGE

docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db mongo

docker stop mongo

docker volume ls

docker volume rm mongodbdata 

docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=mongoadmin mongo

 TO MAKE RESTAPI IMAGE:

 * essentials for deployment in production environment:
    1. Operating System version
    2. .NET 5 Runtime installed
    3. Dependencies such as dlls
    4. Rest API applicaiton
    5. database requirements such as mongoDbEngine etc.

WHY NOT DOCKER?
                * Prepare a box
                                -> Physical machine or virtual machine
                                -> Linus or Windows, pick the correct OS version
                * How to take the files to the production machine?
                                ->through ftp/pendrive etc
                * What if DB requires different version of OS or dependencies?
                * What if we want to move to a new version of .NET?
                * How do we quickly start the REST API on the machine?
                * What if one instance is not enough to handle the load?
DOCKER FILE CAN BE A RESCUE:
                * operating system
                * .NET/ASP.NET Core Runtime
                * Dependencies
                * Where to place the files
                * How to start the REST API 
DOCKER LIFE CYCLE:
                * make a docker image on the Docker Engine using a docker file
                        ->you can push that image to a Container Registry
                                ->pull that image to the REST API container into the production environment
                                        -> not only one single instance of your docker container, you can run multiple instances in produciton environment

DOCKER BENEFITS:
                * Efficien t resource usage
                * Fast start
                * Isolation of each container
                * Runs anywhere where DockerEngine is available
                * Scalability





control shift p->Docker: Add Docker Files to workspace
                ->.NET ASP>NET Core
                ->Linux
                ->port 80
                ->add compose files-NO

 
---------
docker build -t catalog:v1 . 


docker network create net5tutorial

docker network ls

docker stop mongo


docker run -d --rm --name mongo -p 27017:27017 -v mongodbdata:/data/db -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=mongoadmin  --network=net5tutotial mongo

docker images

docker run -it --rm -p 8080:80 -e MongoDbSettings:Host=mongo -e MongoDbSettings:Password=mongoadmin  --network=net5tutorial catalog:v1




--------------------------------------------------------push your image to dub
-- push the api image to dockerHub
docker login

docker tag catalog:v1 imtiaj/catalog:v1


docker push imtiajahammad/cataqlog:v1

-- push the api image to dockerHub

--pulling the api image only
docker logout
docker run -it --rm -p 8080:80 -e MongoDbSettings:Host=mongo -e MongoDbSettings:Password=mongoadmin  --network=net5tutorial imtiajahammad/catalog:v1
--pulling the api image only
*/