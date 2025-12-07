using EFCore.DomainModelGenerator.AnalysisResult;

namespace EFCore.DomainModelGenerator.Steps;

internal static class CombineMetadata
{
  public static AnalysisResult<IEnumerable<MetadataGroup>> Combine(
    AnalysisResult<GeneratorConfig> analyzedConfig,
    AnalysisResult<ContextMetadata>[] analyzedContexts,
    AnalysisResult<ModelMetadata>[] analyzedMarkedModels,
    AnalysisResult<SetMetadata>[] analyzedSets,
    // ReSharper disable once UnusedParameter.Global
    CancellationToken ct)
  {
    var diagnostics = analyzedConfig.Diagnostics.ToList();
    foreach (var x in analyzedContexts)
    {
      diagnostics.AddRange(x.Diagnostics);
    }

    foreach (var x in analyzedMarkedModels)
    {
      diagnostics.AddRange(x.Diagnostics);
    }

    foreach (var x in analyzedSets)
    {
      diagnostics.AddRange(x.Diagnostics);
    }

    var validContexts =
      analyzedContexts
        .SelectMany(static x => x.Result is null ? new ContextMetadata[] { } : [x.Result]);
    var validMarkedModels =
      analyzedMarkedModels
        .SelectMany(static x => x.Result is null ? new ModelMetadata[] { } : [x.Result]);
    var validSets =
      analyzedSets.SelectMany(static x => x.Result is null ? new SetMetadata[] { } : [x.Result]);

    var contextMap = validContexts
      .ToDictionary(x => x.ContextType.Name);
    var groupMap = validMarkedModels
      .Select(x => new Group { Model = x })
      .ToDictionary(x => x.DomainName);

    foreach (var set in validSets)
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

    if (analyzedConfig.Result is null)
    {
      return new AnalysisResult<IEnumerable<MetadataGroup>> { Diagnostics = diagnostics };
    }

    var groups = groupMap.Values.Select(x => x.ToContextMetadata(analyzedConfig.Result));
    return new AnalysisResult<IEnumerable<MetadataGroup>>
    {
      Diagnostics = diagnostics,
      Result = groups,
    };
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

internal record MetadataGroup
{
  public GeneratorConfig Config { get; set; } = null!;
  public IEnumerable<ContextMetadata> Contexts { get; set; } = [];
  public ModelMetadata Model { get; set; } = null!;
  public IEnumerable<SetMetadata> Sets { get; set; } = [];
}

internal class CombineMetadataException(string segment) : InvalidOperationException(segment);
