using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using T3.Core.Compilation;
using T3.Core.Logging;
using T3.Core.Model;
using T3.Core.Operator;
using T3.Core.Operator.Interfaces;
using T3.Editor.Gui.ChildUi;

namespace T3.Editor.UiModel;

// todo - make abstract, create NugetSymbolPackage
internal class EditorSymbolPackage : StaticSymbolPackage
{
    internal EditorSymbolPackage(AssemblyInformation assembly, bool initializeFileWatcher) : base(assembly, false)
    {
        if (initializeFileWatcher)
        {
            InitializeFileWatcher();
        }
    }

    public void LoadUiFiles(IEnumerable<Symbol> newlyReadSymbols, out IReadOnlyCollection<SymbolUi> newlyReadSymbolUis,
                            out IReadOnlyCollection<SymbolUi> preExistingSymbolUis)
    {
        var newSymbols = newlyReadSymbols.ToDictionary(result => result.Id, symbol => symbol);
        var newSymbolsWithoutUis = new ConcurrentDictionary<Guid, Symbol>(newSymbols);
        ConcurrentBag<SymbolUi> preExistingCollection = new();
        Log.Debug($"{AssemblyInformation.Name}: Loading Symbol UIs from \"{Folder}\"");
        var newlyReadSymbolUiList = Directory.EnumerateFiles(Folder, $"*{SymbolUiExtension}", SearchOption.AllDirectories)
                                             .AsParallel()
                                             .Select(JsonFileResult<SymbolUi>.ReadAndCreate)
                                             .Where(result => newSymbols.ContainsKey(result.Guid))
                                             .Select(uiJson =>
                                                     {
                                                         if (!SymbolUiJson.TryReadSymbolUi(uiJson.JToken, uiJson.Guid, out var symbolUi))
                                                         {
                                                             Log.Error($"Error reading symbol Ui for {uiJson.Guid} from file \"{uiJson.FilePath}\"");
                                                             return null;
                                                         }

                                                         symbolUi.UiFilePath = uiJson.FilePath;
                                                         uiJson.Object = symbolUi;
                                                         return uiJson;
                                                     })
                                             .Where(result =>
                                                    {
                                                        if (result?.Object == null)
                                                            return false;

                                                        var symbolUi = result.Object;
                                                        newSymbolsWithoutUis.Remove(symbolUi.Symbol.Id, out _);
                                                        var id = symbolUi.Symbol.Id;

                                                        if (SymbolUis.TryGetValue(id, out var preExistingSymbolUi))
                                                        {
                                                            preExistingCollection.Add(preExistingSymbolUi);
                                                            return false;
                                                        }

                                                        return SymbolUis.TryAdd(id, symbolUi);
                                                    })
                                             .Select(result => result!.Object)
                                             .ToList();

        foreach (var (guid, symbol) in newSymbolsWithoutUis)
        {
            var symbolUi = new SymbolUi(symbol, false);

            if (!SymbolUis.TryAdd(guid, symbolUi))
            {
                Log.Error($"{AssemblyInformation.Name}: Duplicate symbol UI for {symbol.Name}?");
                continue;
            }

            newlyReadSymbolUiList.Add(symbolUi);
        }

        newlyReadSymbolUis = newlyReadSymbolUiList;
        preExistingSymbolUis = preExistingCollection;
    }

    private static void RegisterCustomChildUi(Symbol symbol)
    {
        var valueInstanceType = symbol.InstanceType;
        if (typeof(IDescriptiveFilename).IsAssignableFrom(valueInstanceType))
        {
            CustomChildUiRegistry.Entries.TryAdd(valueInstanceType, DescriptiveUi.DrawChildUi);
        }
    }

    public void RegisterUiSymbols(bool enableLog, IEnumerable<SymbolUi> newSymbolUis, IEnumerable<SymbolUi> preExistingSymbolUis)
    {
        Log.Debug($@"{AssemblyInformation.Name}: Registering UI entries...");

        foreach (var symbolUi in newSymbolUis)
        {
            var symbol = symbolUi.Symbol;

            RegisterCustomChildUi(symbol);

            if (!SymbolUiRegistry.EntriesEditable.TryAdd(symbol.Id, symbolUi))
            {
                SymbolUis.Remove(symbol.Id, out _);
                Log.Error($"Can't load UI for [{symbolUi.Symbol.Name}] Registry already contains id {symbolUi.Symbol.Id}.");
                continue;
            }

            symbolUi.UpdateConsistencyWithSymbol();
            if (enableLog)
                Log.Debug($"Add UI for {symbolUi.Symbol.Name} {symbolUi.Symbol.Id}");
        }

        foreach (var symbolUi in preExistingSymbolUis)
        {
            symbolUi.UpdateConsistencyWithSymbol();
        }
    }

    protected override bool RemoveSymbol(Guid guid)
    {
        return base.RemoveSymbol(guid)
               && SymbolUis.Remove(guid, out _)
               && SymbolUiRegistry.EntriesEditable.Remove(guid, out _);
    }

    protected readonly ConcurrentDictionary<Guid, SymbolUi> SymbolUis = new();
    protected override string ResourcesSubfolder => "Resources";

    public override bool IsModifiable => false;
    protected const string SymbolUiExtension = ".t3ui";
}