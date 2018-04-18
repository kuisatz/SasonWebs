//using System;
//using System.Linq;
//using DevExpress.XtraReports.Web.Native.ClientControls.Services;
//using Microsoft.Extensions.DependencyInjection;

//namespace SasonWebs.DevExpressOverrides {
//    public class AppBuilderServiceRegistrator : IServiceContainerRegistrator {
//        readonly IServiceCollection services;
//        IServiceProvider serviceProvider;
//        public AppBuilderServiceRegistrator(IServiceCollection services) {
//            this.services = services;
//        }

//        public void UseGeneratedServices(IServiceProvider serviceProvider) {
//            this.serviceProvider = serviceProvider;
//        }

//        #region IServiceContainerRegistrator
//        public object GetService(Type serviceType) {
//            if(serviceProvider == null)
//                throw new InvalidOperationException("ServiceProvider is not available. Call the IServiceCollection.BuildServiceProvider method first and then the AppBuilderServiceRegistrator.UseGeneratedServices method.");
//            return serviceProvider.GetService(serviceType);
//        }

//        public void RegisterInstance<T>(T instance) {
//            var serviceDescriptor = new ServiceDescriptor(typeof(T), instance);
//            services.Add(serviceDescriptor);
//        }

//        public void RegisterSingleton<T, TImpl>() where TImpl : T {
//            services.AddSingleton(typeof(T), typeof(TImpl));
//        }

//        public void RegisterTransient<T, TImpl>() where TImpl : T {
//            services.AddTransient(typeof(T), typeof(TImpl));
//        }
//        #endregion
//    }
//}
