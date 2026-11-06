# COMP3008 Distributed Computing
## Assignment 2

This project contains two ASP.NET Core applications:

- **Bank Data Web Service** - ASP.NET Core Web API with SQLite
- **Bank Data Web Application** - ASP.NET Core MVC

The MVC web application communicates with the Bank Data Web Service to provide customer banking and administrator functionality.

---

### Running the Applications

Both applications must be running at the same time.

### 1. Bank Data Web Service

Run the **DataWebService** project first.

- URL: http://localhost:5234
- Swagger: http://localhost:5234/swagger

The service provides the REST API and access to the SQLite database.

### 2. Bank Data Web Application

Run the **BankWebApp** project after the DataWebService is running.

- URL: http://localhost:5091

The web application communicates with the DataWebService through REST API requests.

---

## Service Base URL Configuration

The DataWebService base URL used by BankWebApp is configured in:

BankWebApp/appsettings.json

    json
    "PartAService": {
        "BaseUrl": "http://localhost:5234"
    }

## .NET Version

Both applications target **.NET 8.0**.

## Seeded Sign-In Details

### Administrator

- **Username:** admin
- **Password:** Admin123!

### Example Customer in Database UserTable

- **Username:** charli.green4
- **Password:** Password1!

The administrator account provides access to administrator management functions, while the customer account provides access to customer banking functions.

## Changes Made to the Part A Service

The Part A Bank Data Web Service was extended to support the requirements of Part B.

Changes include:

- Added user login support using username or email and password.
- Added checks to prevent inactive users from signing in.
- Added user retrieval required by the web application, including lookup by userID.
- Added support for updating user profile/details.
- Added a separate password update endpoint.
- Added user deactivation support.
- Added audit loggin support for administrator actions.
- Added and updated API error handling so useful service errors can be displayed by the web application.
- Updated account and transaction API integration required for customer account history and banking functions.

## Authors **COMP3008 Distributed Computing - Assignment 2**

Group project submission.

- Amanda Nguyen, 22223850
- Jamie Dymmott, 20727017
- Jonathan Huynh, 20596725