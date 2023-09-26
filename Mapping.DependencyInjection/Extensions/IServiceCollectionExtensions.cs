using Mapping.Abstractions;
using Mapping.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Tensor.Mapping.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMapper(this IServiceCollection serviceCollection)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        AddMapper(serviceCollection, x =>
        {
            x.AddReferencedMappingAssemblies(callingAssembly);
        });

        return serviceCollection;
    }
    public static IServiceCollection AddMapper(this IServiceCollection serviceCollection, Action<IMappingBuilder> configure)
    {
        serviceCollection.TryAddSingleton(serviceProvider =>
        {
            var builder = new DIMappingBuilder(serviceProvider);

            var providers = serviceProvider.GetServices<MappingProvider>();
            foreach (var provider in providers)
            {
                builder.AddMapping(provider.Source, provider.Destination, provider.Map);
            }

            configure(builder);

            return builder;
        });
        serviceCollection.TryAddSingleton<IMapper>(serviceProvider =>
        {
            var mappings = serviceProvider.GetRequiredService<DIMappingBuilder>();
            return new DIMapper(mappings);
        });

        return serviceCollection;

    }
    public static IServiceCollection AddMapping<TSource, TDestination>(this IServiceCollection services, Func<TSource, TDestination?, TDestination> mapping)
        where TDestination : notnull
    {
        services.AddSingleton(new MappingProvider(typeof(TSource), typeof(TDestination), (source, destination) =>
        {
            TSource typedSource = (TSource)source;

            if (destination == null)
            {
                return mapping(typedSource, default);
            }
            else
            {
                return mapping(typedSource, (TDestination)destination);
            }
        }));

        return services;
    }

    public static IServiceCollection AddUnidirectionalMapper<TSource, TDestination>(this IServiceCollection services, IUnidirectionalMapper<TSource, TDestination> mapper)
        where TDestination : notnull
    {
        services.AddSingleton(new MappingProvider(typeof(TSource), typeof(TDestination),
           (source, destination) =>
           {
               TSource typedSource = (TSource)source;
               if (destination == null)
               {
                   return mapper.Map(typedSource, default);
               }
               else
               {
                   return mapper.Map(typedSource, (TDestination)destination);
               }
           }));

        return services;
    }

    public static IServiceCollection AddBidirectionalMapper<T, K>(this IServiceCollection services, IBidirectionalMapper<T, K> mapper)
        where T : notnull
        where K : notnull
    {
        services.AddSingleton(new MappingProvider(typeof(T), typeof(K),
          (source, destination) =>
          {
              T? typedSource = (T)source;
              if (destination == null)
              {
                  return mapper.Map(typedSource, default);
              }
              else
              {
                  return mapper.Map(typedSource, (K)destination);
              }
          }));
        services.AddSingleton(new MappingProvider(typeof(K), typeof(T),
          (source, destination) =>
          {
              K? typedSource = (K)source;
              if (destination == null)
              {
                  return mapper.Map(typedSource, default);
              }
              else
              {
                  return mapper.Map(typedSource, (T)destination);
              }
          }));
        return services;
    }

    private static MethodInfo? GetMappingMethod(Type mapperType, Type sourceType, Type destinationType)
    => mapperType.GetMethod("Map", new Type[] { sourceType, destinationType });

    public static IServiceCollection AddReferencedMappingAssemblies(this IServiceCollection services, Assembly calling, string suffix = "Mapping")
    {
        var referencedAssemblyNames = calling.GetReferencedAssemblies();

        foreach (var assemblyName in referencedAssemblyNames)
        {
            if (assemblyName.Name?.EndsWith(suffix) ?? false)
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    services.AddMappingAssembly(assembly);
                }
            }
        }

        return services;
    }
    public static IServiceCollection AddMappingAssembly(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IBidirectionalMapper<,>)))
            {
                services.AddBidirectionalMapper(type);
            }
            if (type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IUnidirectionalMapper<,>)))
            {
                services.AddUnidirectionalMapper(type);
            }

        }
        return services;
    }
    public static IServiceCollection AddUnidirectionalMapper<TMapper, TSource, TDestination>(this IServiceCollection services)
       where TMapper : IUnidirectionalMapper<TSource, TDestination>
       where TDestination : notnull
    {
        services.AddUnidirectionalMapper(typeof(TMapper));

        return services;
    }
    public static IServiceCollection AddUnidirectionalMapper(this IServiceCollection services, Type mapperType)
    {
        var implementedInterfaces = mapperType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IUnidirectionalMapper<,>));

        foreach (var implementedInterface in implementedInterfaces)
        {
            if (implementedInterface == null)
            {
                throw new InvalidOperationException("Type must implement IUnidirectionMapper");
            }

            var genericArguments = implementedInterface.GetGenericArguments();
            var sourceType = genericArguments[0];
            var destinationType = genericArguments[1];

            services.AddSingleton(serviceProvider => new MappingProvider(sourceType, destinationType,
                (source, destination) =>
                {
                    var mapper = ActivatorUtilities.CreateInstance(serviceProvider, mapperType);

                    var mapMethod = GetMappingMethod(mapperType, sourceType, destinationType);

                    if (mapMethod != null)
                    {
                        var mapped = mapMethod.Invoke(mapper, new object?[] { source, destination });

                        if (mapped == null)
                        {
                            throw new InvalidOperationException("Mapper returned null");
                        }

                        return mapped;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                }));
        }

        return services;
    }
    public static IServiceCollection AddBidirectionalMapper<TMapper, T, K>(this IServiceCollection services)
        where TMapper : IBidirectionalMapper<T, K>
        where T : notnull
        where K : notnull
    {
        services.AddBidirectionalMapper(typeof(TMapper));

        return services;
    }
    public static IServiceCollection AddBidirectionalMapper(this IServiceCollection services, Type mapperType)
    {
        var implementedInterfaces = mapperType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IBidirectionalMapper<,>));

        foreach (var implementedInterface in implementedInterfaces)
        {
            if (implementedInterface == null)
            {
                throw new InvalidOperationException("Type must implement IUnidirectionMapper");
            }

            var genericArguments = implementedInterface.GetGenericArguments();
            var sourceType = genericArguments[0];
            var destinationType = genericArguments[1];

            services.AddSingleton(serviceProvider => new MappingProvider(sourceType, destinationType,
                (source, destination) =>
                {
                    var mapper = ActivatorUtilities.CreateInstance(serviceProvider, mapperType);

                    var mapMethod = GetMappingMethod(mapperType, sourceType, destinationType);

                    if (mapMethod != null)
                    {
                        var mapped = mapMethod.Invoke(mapper, new object?[] { source, destination });

                        if (mapped == null)
                        {
                            throw new InvalidOperationException("Mapper returned null");
                        }

                        return mapped;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                }));
            services.AddSingleton(serviceProvider => new MappingProvider(destinationType, sourceType,
                (source, destination) =>
                {
                    var mapper = ActivatorUtilities.CreateInstance(serviceProvider, mapperType);

                    var mapMethod = GetMappingMethod(mapperType, destinationType, sourceType);

                    if (mapMethod != null)
                    {
                        var mapped = mapMethod.Invoke(mapper, new object?[] { source, destination });

                        if (mapped == null)
                        {
                            throw new InvalidOperationException("Mapper returned null");
                        }

                        return mapped;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }));
        }
        return services;
    }
}
