using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SasonWebs.DevExpressOverrides;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyModel;
using System.IO;
using Microsoft.CodeAnalysis;
using System.Reflection.PortableExecutable;
//using DevExpress.XtraReports.Web.WebDocumentViewer.Native;
//using DevExpress.XtraReports.Web.QueryBuilder.Native;
//using DevExpress.XtraReports.Web.ReportDesigner.Native;
//using DevExpress.XtraReports.Web.WebDocumentViewer.Native.Services;
//using DevExpress.DataAccess.Web;
//using DevExpress.XtraReports.Web.ReportDesigner.Native.Services;
//using DevExpress.XtraReports.Web.WebDocumentViewer;
//using DevExpress.XtraReports.Web.QueryBuilder;
//using DevExpress.XtraReports.Web.ReportDesigner;
//using DevExpress.XtraReports.Web.Extensions;

namespace SasonWebs
{
    public class Startup
    {
        //AppBuilderServiceRegistrator serviceRegistrator;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddTransient ...

            services.AddMvc()

                    .ConfigureApplicationPartManager(manager =>
                    {
                        var oldMetadataReferenceFeatureProvider = manager.FeatureProviders.First(f => f is MetadataReferenceFeatureProvider);
                        manager.FeatureProviders.Remove(oldMetadataReferenceFeatureProvider);
                        manager.FeatureProviders.Add(new ReferencesMetadataReferenceFeatureProvider());
                    })                
                
                    .AddJsonOptions(json => json.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddSession();
            //services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromMinutes(10);
            //});


            //services.AddCors()


            //this.serviceRegistrator = new AppBuilderServiceRegistrator(services);
            //WebDocumentViewerBootstrapper.RegisterStandardServices(serviceRegistrator);

            //services.AddTransient<ISqlDataSourceConnectionParametersPatcher, BlankSqlDataSourceConnectionParametersPatcher>();
            //services.AddTransient<IWebDocumentViewerUriProvider, ASPNETCoreUriProvider>();
            //services.AddTransient<IReportDesignerUriProvider, ASPNETCoreUriProvider>();
            //services.AddTransient<IWebDocumentViewerReportResolver, ASPNETCoreReportResolver>();


            //QueryBuilderBootstrapper.RegisterStandardServices(serviceRegistrator);
            //ReportDesignerBootstrapper.RegisterStandardServices(serviceRegistrator,
            //    () => serviceRegistrator.GetService<IReportManagementService>(),
            //    () => serviceRegistrator.GetService<IStoragesCleaner>(),
            //    () => serviceRegistrator.GetService<IConnectionProviderFactory>(),
            //    () => serviceRegistrator.GetService<ISqlDataSourceWizardService>(),
            //    () => serviceRegistrator.GetService<ISqlDataSourceConnectionParametersPatcher>()
            //);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
            //    await next();
            //});
            //serviceRegistrator.UseGeneratedServices(app.ApplicationServices);
            //DefaultWebDocumentViewerContainer.Current = DefaultQueryBuilderContainer.Current = DefaultReportDesignerContainer.Current = app.ApplicationServices;
            //ReportStorageWebExtension.RegisterExtensionGlobal(new CustomReportStorageWebExtension(env));
        }
    }
}







namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    public class ReferencesMetadataReferenceFeatureProvider : IApplicationFeatureProvider<MetadataReferenceFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, MetadataReferenceFeature feature)
        {
            var libraryPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var assemblyPart in parts.OfType<AssemblyPart>())
            {
                var dependencyContext = DependencyContext.Load(assemblyPart.Assembly);
                if (dependencyContext != null)
                {
                    foreach (var library in dependencyContext.CompileLibraries)
                    {
                        if (string.Equals("reference", library.Type, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var libraryAssembly in library.Assemblies)
                            {
                                libraryPaths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, libraryAssembly));
                            }
                        }
                        else
                        {
                            foreach (var path in library.ResolveReferencePaths())
                            {
                                libraryPaths.Add(path);
                            }
                        }
                    }
                }
                else
                {
                    libraryPaths.Add(assemblyPart.Assembly.Location);
                }
            }

            foreach (var path in libraryPaths)
            {
                feature.MetadataReferences.Add(CreateMetadataReference(path));
            }
        }

        private static MetadataReference CreateMetadataReference(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var moduleMetadata = ModuleMetadata.CreateFromStream(stream, PEStreamOptions.PrefetchMetadata);
                var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);

                return assemblyMetadata.GetReference(filePath: path);
            }
        }
    }
}