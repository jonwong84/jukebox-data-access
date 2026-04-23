namespace JukeboxDataAccess.Interfaces;

/// <summary>
/// Maps an object of type <typeparamref name="TSource"/> to an object of type <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
public interface IMapper<in TSource, out TDestination>
{
    /// <summary>
    /// Maps the <paramref name="source"/> object to a <typeparamref name="TDestination"/> instance.
    /// </summary>
    /// <param name="source">The source object to map.</param>
    /// <returns>A new <typeparamref name="TDestination"/> instance populated from <paramref name="source"/>.</returns>
    TDestination Map(TSource source);
}
