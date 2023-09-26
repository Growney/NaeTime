using Mapping.Abstractions;

namespace Tensor.Mapping.Abstractions;

public static class IMappingBuilderExtensions
{
    public static IMappingBuilder AddMapping<TSource, TDestination>(this IMappingBuilder builder, Func<TSource, TDestination?, TDestination> mapping)
        where TDestination : notnull
    {
        builder.AddMapping(typeof(TSource), typeof(TDestination),
            (source, destination) =>
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
            });
        return builder;
    }
    public static IMappingBuilder AddUnidirectionalMapper<TSource, TDestination>(this IMappingBuilder builder, IUnidirectionalMapper<TSource, TDestination> mapper)
        where TDestination : notnull
    {
        builder.AddMapping(typeof(TSource), typeof(TDestination),
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
           });

        return builder;
    }
    public static IMappingBuilder AddBidirectionalMapper<T, K>(this IMappingBuilder builder, IBidirectionalMapper<T, K> mapper)
        where T : notnull
        where K : notnull
    {
        builder.AddMapping(typeof(T), typeof(K),
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
          });
        builder.AddMapping(typeof(K), typeof(T),
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
          });
        return builder;
    }

}
