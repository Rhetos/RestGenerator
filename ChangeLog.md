# RestGenerator release notes

## 5.0.0 (2022-03-25)

* Migrated from .NET Framework to .NET 5 (ASP.NET Core 5) and Rhetos 5.
* To setup RestGenerator in .NET 5 application, follow the instructions in "Installation" chapter in [Readme.md](Readme.md#installation).
* Swagger/OpenAPI is automatically available for the generated REST API.
  See the configuration options in "Adding Swagger/OpenAPI" chapter in [Readme.md](Readme.md#adding-swaggeropenapi).

## 4.0.0 (2020-09-23)

* Update to Rhetos 4.1.0.
* Using new base concepts (Rhetos 4.1) for generating the available filter parameters.

## 3.0.0 (2020-05-14)

### Breaking changes

* Instead of generating a specific service class for each object, only one service class for each object type is generated.
  * If your custom plugin inserted code at tags from class Rhetos.RestGenerator.Plugins.Plugins.DataStructureCodeGenerator
    (AdditionalPropertyInitialization, AdditionalPropertyConstructorParameter, AdditionalPropertyConstructorSetProperties, AdditionalOperationsTag),
    it should be modified to use Rhetos.RestGenerator.InitialCodeGenerator tags.
* Renamed namespace from `Rhetos.Rest` to `RestService` in generated Rhetos application.
  This will reduce build issues with Rhetos packages that use inconsistent namespaces:
  both Rhetos.PackageName and PackageName (when in Rhetos.* namespace, PackageName could be interpreted as both).

### New features

* Removed hardcoded service binding. This allows running Rhetos application in IIS with multiple host bindings, e.g. HTTPS and HTTP (issue Rhetos/Rhetos#260).
* Tag for adding 'using' statements to generated REST service (issue Rhetos/Rhetos#70).

### Internal improvements

* New concept RestServiceContractInfo allows other Rhetos packages to extend the REST API with new services
  that have shared central configuration of service binding. See concept class summary for more info.
* Added build option to allow specifying the service configuration in Web.config.
  See ServiceContractConfiguration comments for more information.
  Notes for upgrading existing application to new version of RestGenerator:
  REST service has previously used configuration of `rhetosWebHttpBinding` element in Web.config (100MB message limits and standard security mode by default).
  If there is any additional custom configuration specified in that binding element,
  it should be configured again as specified in Readme.md file section "Overriding IIS binding configuration".
* Action without parameters can be executed with an empty body.
  NullReferenceException was thrown previously and the client was required to send an empty JSON object in the HTTP request body.
* Bugfix: Reading with generic filter "System.Guid[]" and provided a list of GUIDs will result with an exception
  "Object of type 'System.String[]' cannot be converted to type 'System.Collections.Generic.IEnumerable`1[System.Guid]'".
* Bugfix: Reading with given filter type name provided without namespace should work with default namespaces
  (System, Rhetos.Dom.DefaultConcepts and a namespace from the source data structure).
  It works on legacy filter parameters (filter and fparam), but not on new one (filters).
