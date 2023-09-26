using Mapping.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Tensor.Mapping.Abstractions;

public static class IMappingBuilderExtensions
{
    private static MethodInfo? GetMappingMethod(Type mapperType, Type sourceType, Type destinationType)
        => mapperType.GetMethod("Map", new Type[] { sourceType, destinationType });

    public static IMappingBuilder AddReferencedMappingAssemblies(this IMappingBuilder builder, Assembly calling, string suffix = "Mapping")
    {
        var referencedAssemblyNames = calling.GetReferencedAssemblies();

        foreach (var assemblyName in referencedAssemblyNames)
        {
            if (assemblyName.Name?.EndsWith(suffix) ?? false)
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    builder.AddMappingAssembly(assembly);
                }
            }
        }

        return builder;
    }
    public static IMappingBuilder AddMappingAssembly(this IMappingBuilder builder, Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IBidirectionalMapper<,>)))
            {
                builder.AddBidirectionalMapper(type);
            }
            if (type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IUnidirectionalMapper<,>)))
            {
                builder.AddUnidirectionalMapper(type);
            }

        }
        return builder;
    }
    public static IMappingBuilder AddUnidirectionalMapper<TMapper, TSource, TDestination>(this IMappingBuilder builder)
       where TMapper : IUnidirectionalMapper<TSource, TDestination>
       where TDestination : notnull
    {
        builder.AddUnidirectionalMapper(typeof(TMapper));
        return builder;
    }
    public static IMappingBuilder AddUnidirectionalMapper(this IMappingBuilder builder, Type mapperType)
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

            builder.AddMapping(sourceType, destinationType,
                (source, destination) =>
                {
                    var mapper = ActivatorUtilities.CreateInstance(builder.ServiceProvider, mapperType);

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

                });
        }
        return builder;
    }
    public static IMappingBuilder AddBidirectionalMapper<TMapper, T, K>(this IMappingBuilder builder)
        where TMapper : IBidirectionalMapper<T, K>
        where T : notnull
        where K : notnull
    {
        builder.AddBidirectionalMapper(typeof(TMapper));
        return builder;
    }
    public static IMappingBuilder AddBidirectionalMapper(this IMappingBuilder builder, Type mapperType)
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

            builder.AddMapping(sourceType, destinationType,
                (source, destination) =>
                {
                    var mapper = ActivatorUtilities.CreateInstance(builder.ServiceProvider, mapperType);

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

                });
            builder.AddMapping(destinationType, sourceType,
                (source, destination) =>
                {
                    var mapper = ActivatorUtilities.CreateInstance(builder.ServiceProvider, mapperType);

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
                });
        }
        return builder;
    }
}
