using MemoScope.Core.Cache;
using MemoScope.Core.Dac;
using MemoScope.Core.Data;
using MemoScope.Core.ProcessInfo;
using Microsoft.Diagnostics.Runtime;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using WinFwk.UIMessages;
using WinFwk.UIModules;
using WinFwk.UITools.Log;
using ClrObject = MemoScope.Core.Data.ClrObject;

namespace MemoScope.Core
{
    public class ClrDump : IClrDump
    {
        private static readonly Logger logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        private static int n = 0;
        public int Id { get; }
        public ClrRuntime Runtime { get; set; }
        public DataTarget Target { get; }
        public string DumpPath { get; }
        public ClrHeap Heap => Runtime.Heap;

        public MessageBus MessageBus { get; }
        public ClrDumpInfo ClrDumpInfo { get; }

        public IList<ClrSegment> Segments => Runtime.Heap.Segments;
        public List<ClrMemoryRegion> Regions => Runtime.EnumerateMemoryRegions().ToList();
        public IList<ClrModule> Modules => Runtime.Modules;

        public List<ClrHandle> Handles => Runtime.EnumerateHandles().ToList();
        public List<ulong> FinalizerQueueObjectAddresses => Runtime.EnumerateFinalizerQueueObjectAddresses().ToList();
        public IEnumerable<IGrouping<ClrType, ulong>> FinalizerQueueObjectAddressesByType => Runtime.EnumerateFinalizerQueueObjectAddresses().GroupBy(address => GetObjectType(address));
        public IList<ClrThread> Threads => Runtime.Threads;
        public ClrThreadPool ThreadPool => Runtime.ThreadPool;
        public List<ClrType> AllTypes => Heap.EnumerateTypes().ToList();

        public Dictionary<int, ThreadProperty> ThreadProperties
        {
            get
            {
                if (threadProperties == null)
                {
                    InitThreadProperties();
                }
                return threadProperties;
            }
        }

        public IEnumerable<ClrRoot> EnumerateClrRoots => Runtime.Heap.EnumerateRoots();

        private Dictionary<int, ThreadProperty> threadProperties;
        private readonly SingleThreadWorker worker;
        private ClrDumpCache cache;

        public ClrDump(DataTarget target, string dumpPath, MessageBus msgBus)
        {
            Id = n++;
            Target = target;
            DumpPath = dumpPath;
            MessageBus = msgBus;
            worker = new SingleThreadWorker(dumpPath);
            worker.Run(InitRuntime, OnError);

            ClrDumpInfo = ClrDumpInfo.Load(dumpPath);
        }

        private void OnError(Exception ex) => MessageBus.Log(this, "Failed to initRuntime: " + DumpPath, ex);

        public void InitCache(CancellationToken token)
        {
            cache = new ClrDumpCache(this);
            cache.Init(token);
        }

        private void InitRuntime()
        {
            MessageBus.Log(this, "InitRuntime: " + DumpPath);
            using (var locator = DacFinderFactory.CreateDactFinder("DacSymbols"))
            {
                var clrVersion = Target.ClrVersions[0];
                Runtime = clrVersion.CreateRuntime(clrVersion.LocalMatchingDac ?? locator.FindDac(clrVersion));
            }
        }

        public List<ClrType> GetTypes() => worker.Eval(() => AllTypes);

        internal void Destroy()
        {
            Dispose();
            cache.Destroy();
        }

        internal void Dispose()
        {
            MessageBus.Log(this, $"{nameof(Dispose)}: " + DumpPath);
            logger.Debug("Cache dispose");
            cache.Dispose();
            logger.Debug("Runtime.DataTarget.Dispose");
            Run(() => Runtime?.DataTarget?.Dispose());
            logger.Debug("Worker.Dispose");
            worker.Dispose();
        }

        public ClrType GetClrType(string typeName) => worker.Eval(() => Heap.EnumerateTypes().FirstOrDefault(clrType => clrType.Name == typeName));

        public List<ClrTypeStats> GetTypeStats() => cache.LoadTypeStat();

        public List<ulong> GetInstances(ClrType type)
        {
            int typeId = cache.GetTypeId(type.Name);
            return GetInstances(typeId);
        }

        public IEnumerable<ulong> EnumerateInstances(ClrType type)
        {
            int typeId = cache.GetTypeId(type.Name);
            return cache.EnumerateInstances(typeId);
        }

        public List<ulong> GetInstances(int typeId) => cache.LoadInstances(typeId);

        public int CountInstances(ClrType type)
        {
            int typeId = cache.GetTypeId(type.Name);
            return cache.CountInstances(typeId);
        }

        public ClrType GetType(ulong methodTable) => Eval(() => Heap.GetTypeByMethodTable(methodTable));

        public ClrType GetType(string typeName) => Eval(() => Heap.GetTypeByName(typeName));

        public bool IsString(ClrType type) => Eval(() => type.IsString);
        public bool IsPrimitive(ClrType type) => Eval(() => type.IsPrimitive);

        public T Eval<T>(Func<T> func) => worker.Active
                ? worker.Eval(func)
                : throw new InvalidOperationException($"{Id}: can't run action because worker is not active !");

        public void Run(Action action)
        {
            if (worker.Active)
            {
                worker.Run(action);
            }
            else
            {
                throw new InvalidOperationException($"{Id}: can't run action because worker is not active !");
            }
        }

        public object GetSimpleValue(ulong address, ClrType type) => Eval(() => GetSimpleValueImpl(address, type));

        private static object GetSimpleValueImpl(ulong address, ClrType type) => SimpleValueHelper.IsSimpleValue(type) ? SimpleValueHelper.GetSimpleValue(address, type, false) : address;

        public object GetFieldValue(ulong address, ClrType type, List<ClrInstanceField> fields) => Eval(() => GetFieldValueImpl(address, type, fields));
        public object GetFieldValue(ulong address, ClrType type, List<string> fieldNames) => Eval(() => GetFieldValueImpl(address, type, fieldNames));
        public object GetFieldValue(ulong address, ClrType type, ClrInstanceField field) => Eval(() => GetFieldValueImpl(address, type, field));

        internal List<FieldInfo> GetFieldInfos(ClrType type) => Eval(() => GetFieldNamesImpl(type));

        private List<FieldInfo> GetFieldNamesImpl(ClrType type)
        {
            var fieldNames = type.Fields.Select(f => new FieldInfo(f.Name, f.Type)).ToList();
            if (type.IsInterface || type.IsAbstract)
            {
                foreach (var someType in Heap.EnumerateTypes())
                {
                    if (type.IsInterface && someType.Interfaces.Any(interf => interf.Name == type.Name))
                    {
                        fieldNames.AddRange(GetFieldNamesImpl(someType));
                    }
                    if (type.IsAbstract && someType.BaseType == type)
                    {
                        fieldNames.AddRange(GetFieldNamesImpl(someType));
                    }
                }
            }
            return fieldNames.Distinct().ToList();
        }

        public static object GetFieldValueImpl(ulong address, ClrType type, List<ClrInstanceField> fields)
        {
            ClrObject obj = new ClrObject(address, type);

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                obj = obj[field];
                if (obj.IsNull)
                {
                    return null;
                }
            }

            return obj.HasSimpleValue ? obj.SimpleValue : obj.Address;
        }
        public static object GetFieldValueImpl(ulong address, ClrType type, List<string> fieldNames)
        {
            ClrObject obj = new ClrObject(address, type);

            for (int i = 0; i < fieldNames.Count; i++)
            {
                var fieldName = fieldNames[i];
                ClrInstanceField field = obj.GetField(fieldName);
                if (field == null)
                {
                    return null;
                }

                obj = obj[field];
                if (obj.IsNull)
                {
                    return null;
                }
            }

            return obj.HasSimpleValue ? obj.SimpleValue : obj.Address;
        }

        public static object GetFieldValueImpl(ulong address, ClrType type, ClrInstanceField field)
        {
            ClrObject obj = new ClrObject(address, type);
            var fieldValue = obj[field];
            return fieldValue.IsNull ? null : fieldValue.HasSimpleValue ? fieldValue.SimpleValue : fieldValue.Address;
        }

        public IEnumerable<ulong> EnumerateReferers(ulong address) => cache.EnumerateReferers(address);

        public List<ulong> GetReferers(ulong address) => cache.LoadReferers(address);

        public bool HasReferers(ulong address) => cache.CountReferers(address) > 0;

        public int CountReferers(ulong address) => cache.CountReferers(address);

        public string GetObjectTypeName(ulong address) => worker.Eval(() =>
                                                                    {
                                                                        var clrType = Heap.GetObjectType(address);
                                                                        return clrType?.Name;
                                                                    });

        public ClrType GetObjectType(ulong address) => worker.Eval(() => GetObjectTypeImpl(address));

        public ClrType GetObjectTypeImpl(ulong address) => Heap.GetObjectType(address);

        private void InitThreadProperties()
        {
            ClrType threadType = GetClrType(typeof(Thread).FullName);
            var threadsInstances = GetInstances(threadType);
            var nameField = threadType.GetFieldByName("m_Name");
            var priorityField = threadType.GetFieldByName("m_Priority");
            var idField = threadType.GetFieldByName("m_ManagedThreadId");

            threadProperties = new Dictionary<int, ThreadProperty>();
            foreach (ulong threadAddress in threadsInstances)
            {
                string name = (string)GetFieldValue(threadAddress, threadType, nameField);
                int priority = (int)GetFieldValue(threadAddress, threadType, priorityField);
                int id = idField != null ? (int)GetFieldValue(threadAddress, threadType, idField) : 0;
                threadProperties[id] = new ThreadProperty
                {
                    Address = threadAddress,
                    ManagedId = id,
                    Priority = priority,
                    Name = name
                };
            }
        }

        public ulong ReadHeapPointer(ulong address)
        {
            Heap.ReadPointer(address, out ulong value);
            return value;
        }

        public ulong ReadRuntimePointer(ulong address)
        {
            Runtime.ReadPointer(address, out ulong value);
            return value;
        }

        public ClrMethod GetMethodByHandle(ulong methodDescriptorPtr) => Runtime.GetMethodByHandle(methodDescriptorPtr);

        // Find the field in instance at address that references refAddress
        public string GetFieldNameReference(ulong refAddress, ulong address, bool prefixWithType = false) => Eval(() => GetFieldNameReferenceImpl(refAddress, address, prefixWithType));

        public string GetFieldNameReferenceImpl(ulong refAddress, ulong address, bool prefixWithType)
        {
            ClrType type = GetObjectTypeImpl(address);
            if (type == null)
            {
                return "Unknown";
            }
            string fieldName = "???";
            ClrObject obj = new ClrObject(address, type);
            if (type.IsArray)
            {
                fieldName = "[ ? ]";
                var length = type.GetArrayLength(address);
                for (int i = 0; i < length; i++)
                {
                    if (obj[i].Address == refAddress)
                    {
                        fieldName = $"[ {i} ]";
                    }
                }
            }
            else
            {
                foreach (var field in type.Fields)
                {
                    switch (field.ElementType)
                    {
                        case ClrElementType.Struct:
                        case ClrElementType.String:
                        case ClrElementType.Array:
                        case ClrElementType.SZArray:
                        case ClrElementType.Object:
                            var fieldValue = obj[field];
                            if (fieldValue.Address == refAddress)
                            {
                                fieldName = field.Name;
                            }
                            break;
                    }
                }
            }
            if (prefixWithType)
            {
                fieldName = $"{fieldName}@{type.Name}";
            }

            return fieldName;
        }

        public List<BlockingObject> GetBlockingObjects()
        {
            List<BlockingObject> blockingObjects = new List<BlockingObject>();
            CancellationTokenSource source = new CancellationTokenSource();
            var token = source.Token;
            MessageBus.BeginTask("Looking for blocking objects...", source);

            int n = 0;
            foreach (var obj in Runtime.Heap.EnumerateBlockingObjects())
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                n++;
                if (n % 512 == 0)
                {
                    MessageBus.Status($"Looking for blocking objects: {blockingObjects.Count:###,###,###,##0}");
                }
                blockingObjects.Add(obj);
            }
            if (token.IsCancellationRequested)
            {
                MessageBus.EndTask($"Blocking objects (cancelled): {blockingObjects.Count:###,###,###,##0} found.");
            }
            else
            {
                MessageBus.EndTask($"Blocking objects: {blockingObjects.Count:###,###,###,##0} found.");
            }
            return blockingObjects;
        }

        public List<ClrRoot> GetClrRoots()
        {
            List<ClrRoot> clrRoots = new List<ClrRoot>();
            CancellationTokenSource source = new CancellationTokenSource();
            var token = source.Token;
            MessageBus.BeginTask("Looking for ClrRoots...", source);

            int n = 0;
            foreach (var obj in Runtime.Heap.EnumerateRoots())
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                n++;
                if (n % 512 == 0)
                {
                    MessageBus.Status($"Looking for ClrRoots: {clrRoots.Count:###,###,###,##0}");
                }
                clrRoots.Add(obj);
            }
            if (token.IsCancellationRequested)
            {
                MessageBus.EndTask($"ClrRoots (cancelled): {clrRoots.Count:###,###,###,##0} found.");
            }
            else
            {
                MessageBus.EndTask($"ClrRoots : {clrRoots.Count:###,###,###,##0} found.");
            }
            return clrRoots;
        }

        public static bool HasField(ClrType clrType) => clrType.IsPrimitive
                ? false
                : clrType.Fields.Any() ? true : clrType.IsInterface && clrType.Methods.Any(meth => meth.Name.StartsWith("get_"));


    }

    public class ThreadProperty
    {
        public ulong Address { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public int ManagedId { get; set; }
    }

    public class FieldInfo : IEquatable<FieldInfo>
    {
        public string Name { get; }
        public ClrType FieldType { get; }
        public FieldInfo(string name, ClrType fieldType)
        {
            Name = name;
            FieldType = fieldType;
        }
        public bool Equals(FieldInfo fieldInfo) => fieldInfo.Name == Name && fieldInfo.FieldType.Name == FieldType.Name;
        public override bool Equals(object o) => ((IEquatable<FieldInfo>)this).Equals((FieldInfo)o);
        public override int GetHashCode() => Name.GetHashCode() * 37 + FieldType.Name.GetHashCode();
    }
}