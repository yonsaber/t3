using System;
using System.IO;
using T3.Core.Logging;
using T3.Core.Model;
using T3.Core.Operator.Slots;
using T3.Core.Resource;

namespace T3.Core.Operator;

public interface IShaderOperator<T> where T : class, IDisposable
{
    public Slot<T> Shader { get; }
    public InputSlot<string> Source { get; }
    public InputSlot<string> EntryPoint { get; }
    public InputSlot<string> DebugName { get; }
    public Instance Instance { get; }
    bool SourceIsSourceCode { get; }
    protected internal ShaderResource<T> ShaderResource { get; set; }

    public bool TryUpdateShader(EvaluationContext context, ref string cachedSource, out string message)
    {
        // cache interface values to avoid additional virtual method calls
        bool isSourceCode = SourceIsSourceCode;
        var sourceSlot = Source;
        var entryPointSlot = EntryPoint;
        var debugNameSlot = DebugName;

        var shouldUpdate = !isSourceCode || sourceSlot.DirtyFlag.IsDirty || entryPointSlot.DirtyFlag.IsDirty || debugNameSlot.DirtyFlag.IsDirty;

        if (!shouldUpdate)
        {
            message = string.Empty;
            return false;
        }

        var source = sourceSlot.GetValue(context);
        var entryPoint = entryPointSlot.GetValue(context);
        var debugName = debugNameSlot.GetValue(context);

        var type = GetType();

        if (!TryGetDebugName(out message, ref debugName))
        {
            Log.Error($"Failed to update shader \"{debugName}\":\n{message}");
            return false;
        }

        //Log.Debug($"Attempting to update shader \"{debugName}\" ({GetType().Name}) with entry point \"{entryPoint}\".");

        // Cache ShaderResource to avoid additional virtual method calls
        var shaderResource = ShaderResource;
        var needsNewResource = shaderResource == null;

        if (!isSourceCode)
        {
            needsNewResource = needsNewResource || cachedSource != source;
        }

        bool updated;

        var instance = Instance;
        if (needsNewResource)
        {
            updated = TryCreateResource(source, entryPoint, debugName, isSourceCode, Shader, instance, out message, out shaderResource);
            if (updated)
                ShaderResource = shaderResource;
        }
        else
        {
            updated = isSourceCode
                          ? shaderResource.TryUpdateFromSource(source, entryPoint, instance.ResourceFolders, out message)
                          : shaderResource.TryUpdateFromFile(source, entryPoint, instance.ResourceFolders, out message);
        }

        if (updated && shaderResource != null)
        {
            shaderResource.UpdateDebugName(debugName);
            Shader.Value = shaderResource.Shader;
            Shader.DirtyFlag.Invalidate();
        }
        else
        {
            Log.Error($"Failed to update shader \"{debugName}\": {message}");
        }

        cachedSource = source;
        return updated;

        bool TryGetDebugName(out string dbgMessage, ref string dbgName)
        {
            dbgMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(dbgName))
                return true;

            if (isSourceCode)
            {
                dbgName = $"{type.Name}({entryPoint}) - {sourceSlot.Id}";
                return true;
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                dbgMessage = "Source path is empty.";
                return false;
            }

            try
            {
                dbgName = Path.GetFileNameWithoutExtension(source) + " - " + entryPoint;
                return true;
            }
            catch (Exception e)
            {
                dbgMessage = $"Invalid source path for shader: {source}: {e.Message}";
                return false;
            }
        }

        static bool TryCreateResource(string source, string entryPoint, string debugName, bool isSourceCode, ISlot shaderSlot, Instance instance,
                                      out string errorMessage, out ShaderResource<T> shaderResource)
        {
            bool updated;
            var resourceManager = ResourceManager.Instance();

            if (isSourceCode)
            {
                updated = resourceManager.TryCreateShaderResourceFromSource(out shaderResource,
                                                                            shaderSource: source,
                                                                            directories: instance.ResourceFolders,
                                                                            entryPoint: entryPoint,
                                                                            name: debugName,
                                                                            errorMessage: out errorMessage);
            }
            else
            {
                updated = resourceManager.TryCreateShaderResource(out shaderResource,
                                                                  watcher: instance.ResourceFileWatcher,
                                                                  resourceFolders: instance.ResourceFolders,
                                                                  relativePath: source,
                                                                  entryPoint: entryPoint,
                                                                  name: debugName,
                                                                  fileChangedAction: () =>
                                                                                     {
                                                                                         //sourceSlot.DirtyFlag.Invalidate();
                                                                                         shaderSlot.DirtyFlag.Invalidate();
                                                                                         //Log.Debug($"Invalidated {sourceSlot}   isDirty: {sourceSlot.DirtyFlag.IsDirty}", sourceSlot.Parent);
                                                                                     },
                                                                  errorMessage: out errorMessage);
            }

            return updated;
        }
    }
}