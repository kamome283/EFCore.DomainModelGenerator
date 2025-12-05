namespace EFCore.DomainModelGenerator.Steps;

internal static class CombineMetadata
{
  public static IEnumerable<MetadataGroup> Combine(
    GeneratorConfig config,
    IEnumerable<ContextMetadata> contexts,
    IEnumerable<ModelMetadata> models,
    IEnumerable<SetMetadata> sets,
    CancellationToken _)
  {
    return CombineImpl.Process(config, contexts, models, sets);
  }
}

internal record MetadataGroup
{
  public GeneratorConfig Config { get; set; } = null!;
  public IEnumerable<ContextMetadata> Contexts { get; set; } = [];
  public ModelMetadata Model { get; set; } = null!;
  public IEnumerable<SetMetadata> Sets { get; set; } = [];
}

file static class CombineImpl
{
  public static IEnumerable<MetadataGroup> Process(
    GeneratorConfig config,
    IEnumerable<ContextMetadata> contexts,
    IEnumerable<ModelMetadata> markedModels,
    IEnumerable<SetMetadata> sets)
  {
    var contextMap = contexts
      .ToDictionary(x => x.ContextType.Name);
    var groupMap = markedModels
      .Select(x => new Group { Model = x })
      .ToDictionary(x => x.DomainName);

    foreach (var set in sets)
    {
      _ = contextMap.TryGetValue(set.ParentType.Name, out var correspondingContext);
      if (correspondingContext is null) throw new CombineMetadataException("correspondingContext");

      _ = groupMap.TryGetValue(set.DomainName, out var correspondingGroup);
      if (correspondingGroup is null)
      {
        correspondingGroup = new Group { Model = new ModelMetadata { DomainName = set.DomainName } };
        groupMap[set.DomainName] = correspondingGroup;
      }

      correspondingGroup.AddSet(set);
      correspondingGroup.AddContextIfNotExist(correspondingContext);
    }

    return groupMap.Values.Select(x => x.ToContextMetadata(config));
  }

  private record Group
  {
    private List<ContextMetadata> Contexts { get; } = [];
    public ModelMetadata Model { private get; set; } = null!;
    private List<SetMetadata> Sets { get; } = [];
    public string DomainName => Model.DomainName;

    public void AddSet(SetMetadata set) => Sets.Add(set);

    public void AddContextIfNotExist(ContextMetadata context)
    {
      if (Contexts.Any(x => x.ContextType.Name == context.ContextType.Name)) return;
      Contexts.Add(context);
    }

    public MetadataGroup ToContextMetadata(GeneratorConfig config) => new()
    {
      Config = config,
      Contexts = Contexts,
      Model = Model,
      Sets = Sets,
    };
  }
}

internal class CombineMetadataException(string segment) : InvalidOperationException(segment);
