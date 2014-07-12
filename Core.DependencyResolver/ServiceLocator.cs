using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;

namespace Core.DependencyResolver
{
    public static class ServiceLocator
    {
        private static CompositionContainer _container;

        public static void Initialize(AggregateCatalog catalog)
        {
            _container = new CompositionContainer(catalog);
        }

        public static T GetInstance<T>()
        {
            return _container.GetExportedValue<T>();
        }
    }
}