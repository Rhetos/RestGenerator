# RestGenerator

RestGenerator is a web API plugin package for [Rhetos development platform](https://github.com/Rhetos/Rhetos).
It automatically generates **RESTful JSON web service** for all entities, actions and other data structures that are defined in a Rhetos application.

See [rhetos.org](http://www.rhetos.org/) for more information on Rhetos.

- [RestGenerator](#restgenerator)
  - [Features](#features)
    - [General rules](#general-rules)
    - [Reading data](#reading-data)
    - [Writing data](#writing-data)
    - [Actions](#actions)
    - [Reports](#reports)
  - [Examples](#examples)
  - [HTTPS](#https)
  - [Obsolete and partially supported features](#obsolete-and-partially-supported-features)
  - [Build](#build)
  - [Installation](#installation)
    - [Overriding IIS binding configuration](#overriding-iis-binding-configuration)

## Features

### General rules

1. For each data structure or action, a service is available at base URI `<rhetos server url>/Rest/<module name>/<entity name>/`
2. Any POST request should contain a header: `Content-Type: application/json; charset=utf-8`

For example, a service for entity *Claim* in module *Common*,
on default local server installation (<http://localhost/Rhetos>):

* Base service URI (reading service metadata): `http://localhost/Rhetos/Rest/Common/Claim/`
* To read all entity's records, simply enter the address in the web browser:
  `http://localhost/Rhetos/Rest/Common/Claim/` (don't forget the *slash* at the end)

Response:

* The response status code will indicate the success of the request:
  200 - OK,
  4xx - client error (incorrect data or request format, authentication or authorization error),
  500 - internal server error.
* In case of an error, the response body will contain more information on the error. It is a JSON object with properties:
  * UserMessage - a message to be displayed to the user.
  * SystemMessage - additional error metadata for better client UX
    (for example, a property that caused an error).

Following are URI templates for the web methods.

### Reading data

To read the data from the entity, or any other readable data structure,
execute a GET request on its [base URI](#general-rules):

* Reading records: `/?filters=...&top=...&skip=...&sort=...`
  * The parameters are optional.
  * *Top* and *skip* values are integer number of records.
  * See *Filters* description below.
  * Example of *sorting* by multiple properties: `sort=CreationDate desc,Name,ID`.
* Reading total records count for paging: `/TotalCount?filters=...&sort=...`
* Reading records and total count: `/RecordsAndTotalCount?filters=...&top=...&skip=...&sort=...`
* Reading a single record: `/<id>`

See the [Examples](#examples) chapter below.

**Filters** are provided as a JSON-serialized **array** containing any number of filters of the following types:

1. **Generic** property filter
   * Format: `{"Property":...,"Operation":..., "Value":...}`
   * Example: select items where year is greater than 2005: `[{"Property":"Year","Operation":"Greater", "Value":2005}]`
   * Available operations:
     * `Equals`, `NotEquals`, `Greater`, `GreaterEqual`, `Less`, `LessEqual`
     * `In`, `NotIn` -- Parameter Value is a JSON array.
     * `StartsWith`, `EndsWith`, `Contains`, `NotContains` -- String only.
     * `DateIn`, `DateNotIn` -- Date or DateTime property only, provided value must be string.
       Returns whether the property's value is within a given day, month or year.
       Valid value format is *yyyy-mm-dd*, *yyyy-mm* or *yyyy*.
2. **Specific filter** without a parameter
   * Format: `{"Filter":...}` (provide a full name of the filter)
   * Specific filters refer to concepts such as **ItemFilter**, **ComposableFilterBy** and **FilterBy**,
     and also other [predefined filters](https://github.com/Rhetos/Rhetos/wiki/Filters-and-other-read-methods#predefined-filters) available in the object model.
   * Example: get long books from the Bookstore demo by applying
     [ItemFilter LongBooks](https://github.com/Rhetos/Bookstore/blob/master/src/DslScripts/AdditionalExamples/ExampleFilters.rhe)
     on Book entity: `[{"Filter":"Bookstore.LongBooks"}]`
3. **Specific filter** with a parameter
   * Format: `{"Filter":...,"Value":...}` (value is usually a JSON object)
   * Example: get books with at least 700 pages from the Bookstore demo by applying
     [ComposableFilterBy LongBooks3](https://github.com/Rhetos/Bookstore/blob/master/src/DslScripts/AdditionalExamples/ExampleFilters.rhe)
     on Book entity: `[{"Filter":"Bookstore.LongBooks3","Value":{"MinimumPages":700}}]`

When applying multiple filters in a same request, the intersection of the filtered data is returned (AND).

### Writing data

* Inserting a record: POST at the entity's service [base URI](#general-rules).
  * You may provide the "ID" value of the new record in the request body (just include the ID property in the JSON object).
    If not, it will be automatically generated.
* Updating a record: PUT `/<id>`
* Deleting a record: DELETE `/<id>`

### Actions

* Executing an action: POST at the action's service [base URI](#general-rules).
* The request body should contain a JSON serialized parameters object (properties of the Action in DSL script).
  * If the action has no parameters, the body must be set to an empty JSON object "{}" (until RestGenerator v2.5.0),
    or the body can by empty (since v2.6.0).
* For example, execute an action "Common.AddToLog" to add a [custom log entry](https://github.com/Rhetos/Rhetos/wiki/Logging#logging-data-changes-and-auditing):
  * POST `http://localhost/Rhetos/rest/Common/AddToLog/`
  * Header: `Content-Type: application/json; charset=utf-8`
  * Request body: `{"Action":"just testing","Description":"abc"}`

### Reports

* Downloading a report: `/?parameter=...&convertFormat=...`
  * Query parameters `parameter` and `convertFormat` are optional.
  * Example format `http://localhost/Rhetos/Rest/TestModule/TestReport/?parameter={"Prefix":"a"}&convertFormat=pdf`

## Examples

These examples expect that the Rhetos web application is available at URL <http://localhost/Rhetos/>

Generic property filters:

| Request | URL example |
| --- | --- |
| Using a generic filter to read **multiple items by ID** | <http://localhost/Rhetos/rest/Common/Principal/?filters=[{"Property":"ID","Operation":"in","Value":["c62bc1c1-cc47-40cd-9e91-2dd682d55f95","1b1688c4-4a8a-4131-a151-f04d4d2773a2"]}]> |
| Using a generic filter to search for **empty values** | <http://localhost/Rhetos/rest/Common/Principal/?filters=[{"Property":"Name","Operation":"equal","Value":""}]> |
| Using a generic filter to search for **null values** | <http://localhost/Rhetos/rest/Common/Principal/?filters=[{"Property":"Name","Operation":"equal","Value":null}]> |

## HTTPS

To enable HTTPS, follow the instructions in [Set up HTTPS](https://github.com/Rhetos/Rhetos/wiki/Setting-up-Rhetos-for-HTTPS).

## Obsolete and partially supported features

These features are available for backward compatibility, they will be removed in future versions:

* `/Count` WEB API method. Use `/TotalCount` method instead.
* Reading method query parameters `page` and `psize`. Use `top` and `skip`.
* Reading method query parameters `filter` and `fparam`. Use `filters` instead (see "Specific filter with a parameter").
* Reading method query parameter `genericfilter`. Renamed to `filters`.
* Generic property filter operations `Equal` and `NotEqual`. Use `Equals` and `NotEquals` instead.

Partially supported features:

* `DateNotIn`, `EndsWith` and `NotContains` operations are supported only for *Rhetos v1.0* or later.

## Build

**Note:** This package is already available at the [NuGet.org](https://www.nuget.org/) online gallery.
You don't need to build it from source in order to use it in your application.

To build the package from source, run `Build.bat`.
The script will pause in case of an error.
The build output is a NuGet package in the "Install" subfolder.

## Installation

To install this package to a Rhetos server, add it to the Rhetos server's *RhetosPackages.config* file
and make sure the NuGet package location is listed in the *RhetosPackageSources.config* file.

* The package ID is "**Rhetos.RestGenerator**".
  This package is available at the [NuGet.org](https://www.nuget.org/) online gallery.
  It can be downloaded or installed directly from there.
* For more information, see [Installing plugin packages](https://github.com/Rhetos/Rhetos/wiki/Installing-plugin-packages).

### Overriding IIS binding configuration

Generated web service (WebServiceHost) will automatically create HTTP and HTTPS REST-like endpoint/binding/behavior pairs if service endpoint/binding/behavior configuration is empty.

If you need to override default behavior (i.e. enable only HTTPS), you need to add following in `services` section:

```XML
<service name="Rhetos.Rest.RestService{module}{object}">
  <clear />
  <endpoint binding="webHttpBinding" bindingConfiguration="rhetosWebHttpsBinding" contract="Rhetos.Rest.RestService{module}{object}" />
</service>
```

Also, you need to define new `webHttpBinding` `binding` item:


```XML
<binding name="rhetosWebHttpsBinding" maxReceivedMessageSize="209715200">
  <security mode="Transport" />
  <readerQuotas maxArrayLength="209715200" maxStringContentLength="209715200" />
</binding>
```
