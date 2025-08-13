# This is a fork of NHibernate-Json

---

# nhibernate-json
NHibernate Custom Type mapping to Json

This package allows you to map a model to a SQL column storing its contents in Json via serialization. It uses `System.Text.Json` for serialisation.

## Target Frameworks

This library now multi-targets: `net8.0` and `net9.0`.

If you require legacy `netstandard2.0` support, use an older release prior to this change.

## Usage

Using [FluentNHibernate](https://github.com/nhibernate/fluent-nhibernate): Simply add a CustomType to your existing map class

```c#
 public class ExampleEntityMap : ClassMap<ExampleEntity>
	{
		public ExampleEntityMap()
		{
			Table("ExampleEntities");
			Id(x => x.Id).GeneratedBy.Assigned();
			Map(x => x.Title);
			Map(x => x.Json).CustomType<JsonColumnType<ExampleJsonModel>>().Nullable();
		}
	}
```

## Serialisation Settings
The package uses an internal worker class that has a few pre-set serialisation settings. All of these can be overridden with the standard Json.Net serialisation settings.

```c#
JsonWorker.Configure(x => x.TypeNameHandling = TypeNameHandling.None);
```

## Compression

As an extra, there is a configurable threshold character length that when your serialised model exceeds, it will become compressed. This increases performance and efficiency by reducing SQL storage space and network traffic.

You can configure this threshold via the following property:

```c#
 JsonCompressor.CompressionThreshold = 2000;
```
