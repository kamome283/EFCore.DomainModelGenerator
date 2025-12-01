namespace EFCore.DomainModelGenerator.Steps;

internal static class CombineMetadata
{
  public static IEnumerable<MetadataGroup> Combine(
    IEnumerable<ContextMetadata> contexts,
    IEnumerable<ModelMetadata> models,
    IEnumerable<SetMetadata> sets,
    CancellationToken _)
  {
    var impl = new Impl(contexts, models, sets);
    return impl.Process();
  }
}

internal record MetadataGroup
{
  public IEnumerable<ContextMetadata> Contexts { get; set; } = [];
  public ModelMetadata Model { get; set; } = null!;
  public IEnumerable<SetMetadata> Sets { get; set; } = [];
}

file class Impl(
  IEnumerable<ContextMetadata> contexts,
  IEnumerable<ModelMetadata> markedModels,
  IEnumerable<SetMetadata> sets)
{
  private readonly Dictionary<string, ContextMetadata> _contexts =
    contexts.ToDictionary(x => x.ContextType.Name);

  private readonly Dictionary<string, Group> _pairs =
    markedModels
      .Select(model => new Group { Model = model })
      .ToDictionary(pair => pair.ModelName);

  public IEnumerable<MetadataGroup> Process()
  {
    foreach (var set in sets)
    {
      var pair = GetCorrespondingPair(set);
      pair.Sets.Add(set);
      var context = GetCorrespondingContext(set);
      pair.AddContextIfNotExist(context);
    }

    return _pairs.Values.Select(x => new MetadataGroup
    {
      Contexts = x.Contexts,
      Model = x.Model,
      Sets = x.Sets,
    });
  }

  private ContextMetadata GetCorrespondingContext(SetMetadata set)
  {
    return _contexts.SingleOrDefault(x => x.Key == set.ParentType.Name).Value ??
           throw new CombineMetadataException("GetCorrespondingContext");
  }

  private Group GetCorrespondingPair(SetMetadata set)
  {
    var modelName = set.ModelName;
    if (_pairs.TryGetValue(modelName, out var foundPair)) return foundPair;
    var model = new ModelMetadata { ModelName = modelName };
    var pair = new Group { Model = model };
    _pairs[modelName] = pair;
    return pair;
  }
}

file record Group
{
  public List<ContextMetadata> Contexts { get; } = [];
  public ModelMetadata Model { get; set; } = null!;
  public List<SetMetadata> Sets { get; } = [];
  public string ModelName => Model.ModelName;

  public void AddContextIfNotExist(ContextMetadata context)
  {
    if (Contexts.Any(x => x.ContextType.Name == context.ContextType.Name)) return;
    Contexts.Add(context);
  }
}

file class CombineMetadataException(string segment) : Exception(segment);
