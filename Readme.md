# JsonCommands

JsonCommands is a web API plugin package for [Rhetos development platform](https://github.com/Rhetos/Rhetos).

It provides **a JSON web service** for all entities and other readable data structures,
that allows executing multiple read or write commands in one web request.

See [rhetos.org](http://www.rhetos.org/) for more information on Rhetos.

1. [Features](#features)
   1. [General rules](#general-rules)
   2. [Writing data](#writing-data)
2. [Installation](#installation)
   1. [Configure JSON format](#configure-json-format)
3. [How to contribute](#how-to-contribute)
   1. [Building and testing the source code](#building-and-testing-the-source-code)

## Features

### General rules

1. Any POST request should contain a header: `Content-Type: application/json; charset=utf-8`

Examples in this article will assume that your application's base URI is `https://localhost:5000`.

Response:

* The response status code will indicate the success of the request:
  * 200 - OK,
  * 4xx - client error (incorrect data or request format, authentication or authorization error),
  * 500 - internal server error.
* In case of an error, the response body will contain more information on the error. It is a JSON object with properties:
  * UserMessage - a message to be displayed to the user.
  * SystemMessage - additional error metadata for better client UX
    (for example, a property that caused an error).

Following are URI templates for the web methods.

### Writing data

Send a POST request to `https://localhost:5000/jc/write`.
The POST request should have a following format:

```json
[
  {
    "Bookstore.Book": {
      "Delete": [
        { "ID": "00a7302a-df84-43a4-8c1c-6f7aa13c63b4" }
      ],
      "Update": [
        { "ID": "8faa49db-aa6a-4e0c-9459-c1a16826ffc5", "Title": "Some other book" },
        { "ID": "9e76a291-a76f-43e3-85ba-60bb88c3900b", "Title": "Yet another book" }
      ],
      "Insert": [
        { "ID": "ed609ccf-346e-423d-9e21-145571dbaee9", "Title": "The Art of Computer Programming" }
      ]
    }
  },
  {
    "Bookstore.Comment": {
      "Insert": [
        { "Text": "Very interesting", "BookID": "ed609ccf-346e-423d-9e21-145571dbaee9" },
        { "Text": "Educational", "BookID": "ed609ccf-346e-423d-9e21-145571dbaee9" }
      ]
    }
  }
]
```

For each entity write block (for example for Bookstore.Book above), internally Rhetos will always execute delete operation first, then update, then insert. If a custom order is needed within one write command, the client can control it by using multiple blocks for same entity. For example:

```json
[
  {  "Bookstore.Book": { "Insert": [...] } },
  {  "Bookstore.Book": { "Delete": [...] } },
  {  "Bookstore.Book": { "Update": [...] } }
]
```

## Installation

Installing this package to a Rhetos web application:

1. Add "Rhetos.JsonCommands" NuGet package, available at the [NuGet.org](https://www.nuget.org/) on-line gallery.
2. Extend Rhetos services configuration (at `services.AddRhetosHost`) with the JsonCommands API:
   ```cs
   .AddJsonCommands();
   ```

### Configure JSON format

Depending on your intended client applications, you can use standard ASP.NET Core features
to configure the JSON response formatting in Program.cs or Startup.cs.

For compatibility with [Rhetos.FloydExtensions](https://www.nuget.org/packages/Rhetos.FloydExtensions),
you can configure the JSON object serialization for all properties to start with an uppercase letter:

```cs
// If not using Newtonsoft.Json:
builder.Services.AddControllers()
  .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// If using Newtonsoft.Json:
builder.Services.AddControllers()
  .AddNewtonsoftJson(o => o.UseMemberCasing());
```

## How to contribute

Contributions are very welcome. The easiest way is to fork this repo, and then
make a pull request from your fork. The first time you make a pull request, you
may be asked to sign a Contributor Agreement.
For more info see [How to Contribute](https://github.com/Rhetos/Rhetos/wiki/How-to-Contribute) on Rhetos wiki.

### Building and testing the source code

* Note: This package is already available at the [NuGet.org](https://www.nuget.org/) online gallery.
  You don't need to build it from source in order to use it in your application.
* To build the package from source, run `Clean.bat`, `Build.bat` and `Test.bat`.
* For the test script to work, you need to create an empty database and
  a settings file `test\TestApp\ConnectionString.local.json`
  with the database connection string (configuration key "ConnectionStrings:RhetosConnectionString").
* The build output is a NuGet package in the "Install" subfolder.
